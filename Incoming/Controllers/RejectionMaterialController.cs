using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Incoming.Data;
using Incoming.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Incoming.Controllers
{
    [Authorize(Roles = "ADMINISTRATOR,SUPERVISOR")]
    public class RejectionMaterialController : Controller
    {
        private readonly IncomingContext _context;
        public RejectionMaterialController(IncomingContext context)
        {
            _context = context;
        }
        // GET: RejectionMaterialController
        public async Task<IActionResult> Index()
        {
            var rejection = await _context.ViewRejectionmaterials.ToListAsync();
            ViewBag.ProgramList = _context.InspectionPrograms.Select(p => p.ProgramName).ToList();

            return View(rejection);
           
        }
        [HttpGet]
        public IActionResult ExportToExcel(
    string? category,
    string? program,
    string? partnumber,
    string? basenumer,
    string? molding,
    string? gap,
    string? connector,
    string? Heater,
    string? NTC,
    string? Inside,
    string? Outside,
    string? Guard,
    string? ElectricTest,
    string? datestart,
    string? dateend
)
        {
            var plantillaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "base_report.xlsx");

            try
            {
                IQueryable<ViewRejectionmaterial> query = _context.ViewRejectionmaterials;

                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(r => r.Category == category);

                if (!string.IsNullOrWhiteSpace(program))
                    query = query.Where(r => r.Program == program);

                if (!string.IsNullOrWhiteSpace(partnumber))
                    query = query.Where(r => r.Partnumber == partnumber);

                if (!string.IsNullOrWhiteSpace(basenumer))
                    query = query.Where(r => r.Base == basenumer);

                if (!string.IsNullOrWhiteSpace(molding))
                    query = query.Where(r => r.Molding == molding);

                if (!string.IsNullOrWhiteSpace(gap))
                    query = query.Where(r => r.Gap == gap);

                if (!string.IsNullOrWhiteSpace(connector))
                    query = query.Where(r => r.Connector == connector);

                if (!string.IsNullOrWhiteSpace(ElectricTest))
                {
                    if (ElectricTest == "DEFECT")
                        query = query.Where(e =>
                            (e.Heater != "OK" && e.Heater != "N/A") ||
                            (e.Ntc != "OK" && e.Ntc != "N/A") ||
                            (e.Inside != "OK" && e.Inside != "N/A") ||
                            (e.Outside != "OK" && e.Outside != "N/A") ||
                            (e.Guard != "OK" && e.Guard != "N/A")
                        );
                    else if (ElectricTest == "OK")
                        query = query.Where(e =>
                            e.Heater == "OK" ||
                            e.Ntc == "OK" ||
                            e.Inside == "OK" ||
                            e.Outside == "OK" ||
                            e.Guard == "OK"
                        );
                }

                if (!string.IsNullOrWhiteSpace(Heater))
                    query = query.Where(r => r.Heater == Heater);

                if (!string.IsNullOrWhiteSpace(NTC))
                    query = query.Where(r => r.Ntc == NTC);

                if (!string.IsNullOrWhiteSpace(Inside))
                    query = query.Where(r => r.Inside == Inside);

                if (!string.IsNullOrWhiteSpace(Outside))
                    query = query.Where(r => r.Outside == Outside);

                if (!string.IsNullOrWhiteSpace(Guard))
                    query = query.Where(r => r.Guard == Guard);

                if (DateTime.TryParse(datestart, out DateTime startDaterange))
                    query = query.Where(r => r.RejectionDate >= startDaterange);

                if (DateTime.TryParse(dateend, out DateTime endDaterange))
                {
                    endDaterange = endDaterange.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.RejectionDate <= endDaterange);
                }

                var registros = query.Take(1000).ToList();

                // 🔹 Agrupamos registros, combinando EJ y KM en "EJ / KM"
                var defectosPorPrograma = registros
                    .Select(r => new
                    {
                        Program = (r.Program == "EJ" || r.Program == "KM") ? "EJ / KM" : r.Program,
                        r.Connector,
                        r.Molding,
                        r.Gap,
                        r.Heater,
                        r.Ntc,
                        r.Inside,
                        r.Outside,
                        r.Guard
                    })
                    .GroupBy(r => r.Program)
                    .ToDictionary(g => g.Key, g => new
                    {
                        Connector = g.Count(x => x.Connector == "DEFECT"),
                        Molding = g.Count(x => x.Molding == "DEFECT"),
                        Gap = g.Count(x => x.Gap == "DEFECT"),
                        Heater = g.Count(x => !string.IsNullOrWhiteSpace(x.Heater) && x.Heater != "OK" && x.Heater != "N/A"),
                        NTC = g.Count(x => !string.IsNullOrWhiteSpace(x.Ntc) && x.Ntc != "OK" && x.Ntc != "N/A"),
                        Inside = g.Count(x => !string.IsNullOrWhiteSpace(x.Inside) && x.Inside != "OK" && x.Inside != "N/A"),
                        Outside = g.Count(x => !string.IsNullOrWhiteSpace(x.Outside) && x.Outside != "OK" && x.Outside != "N/A"),
                        Guard = g.Count(x => !string.IsNullOrWhiteSpace(x.Guard) && x.Guard != "OK" && x.Guard != "N/A")
                    });

                var programlist = registros
                    .Where(r => !string.IsNullOrWhiteSpace(r.Program))
                    .Select(r => (r.Program == "EJ" || r.Program == "KM") ? "EJ / KM" : r.Program)
                    .Distinct()
                    .ToList();

                var DateRange = registros
                    .Where(r => r.RejectionDate.HasValue)
                    .Select(r => r.RejectionDate.Value)
                    .ToList();

                DateTime? DateMax = null;
                DateTime? DateMin = null;
                List<Container> containers = new();

                if (DateRange.Any())
                {
                    DateMin = DateRange.Min();
                    DateMax = DateRange.Max();
                    DateTime startDate = DateMin.Value.Date;
                    DateTime endDate = DateMax.Value.Date.AddDays(1).AddTicks(-1);

                    containers = _context.Containers
                        .Where(c => (c.Program == "EJ" || c.Program == "KM" || programlist.Contains(c.Program))
                            && c.Status == "CLOSE"
                            && c.DateStart >= startDate
                            && c.DateEnd <= endDate)
                        .ToList();
                }

                // 🔹 Agrupamos contenedores, combinando EJ y KM en "EJ / KM"
                var conteoPorPrograma = containers
                    .Select(c => new
                    {
                        Program = (c.Program == "EJ" || c.Program == "KM") ? "EJ / KM" : c.Program
                    })
                    .GroupBy(c => c.Program)
                    .ToDictionary(g => g.Key, g => g.Count());

                if (!registros.Any())
                    return NoContent();

                using (var workbook = new XLWorkbook(plantillaPath))
                {
                    IXLWorksheet worksheet = workbook.Worksheets.FirstOrDefault() ?? workbook.AddWorksheet("Sheet1");

                    var filaPorPrograma = new Dictionary<string, int>
            {
                { "KX", 5 },
                { "LB", 6 },
                { "EJ / KM", 7 }
            };

                    foreach (var kvp in filaPorPrograma)
                    {
                        string programa = kvp.Key;
                        int fila = kvp.Value;

                        int cantidad = conteoPorPrograma.ContainsKey(programa) ? conteoPorPrograma[programa] : 0;
                        worksheet.Cell(fila, 2).Value = cantidad;

                        if (defectosPorPrograma.TryGetValue(programa, out var defectos))
                        {
                            worksheet.Cell(fila, 3).Value = defectos.Connector;
                            worksheet.Cell(fila, 4).Value = defectos.Molding;
                            worksheet.Cell(fila, 5).Value = defectos.Gap;
                            worksheet.Cell(fila, 6).Value = defectos.Heater;
                            worksheet.Cell(fila, 7).Value = defectos.NTC;
                            worksheet.Cell(fila, 8).Value = defectos.Inside;
                            worksheet.Cell(fila, 9).Value = defectos.Outside;
                            worksheet.Cell(fila, 10).Value = defectos.Guard;
                        }
                    }

                    // Fechas en la cabecera
                    if (DateMax.HasValue)
                    {
                        worksheet.Cell(2, 3).Value = DateMin.Value.ToString("MM/dd/yyyy");
                        worksheet.Cell(2, 7).Value = DateMax.Value.ToString("MM/dd/yyyy");
                    }

                    int startRow = 10;
                    int startCol = 1;

                    foreach (var item in registros)
                    {
                        worksheet.Cell(startRow, startCol).Value = item.Program;
                        worksheet.Cell(startRow, startCol + 1).Value = item.Base;
                        worksheet.Cell(startRow, startCol + 2).Value = item.Connector;
                        worksheet.Cell(startRow, startCol + 3).Value = item.Molding;
                        worksheet.Cell(startRow, startCol + 4).Value = item.Gap;
                        worksheet.Cell(startRow, startCol + 5).Value = item.Heater;
                        worksheet.Cell(startRow, startCol + 6).Value = item.Ntc;
                        worksheet.Cell(startRow, startCol + 7).Value = item.Inside;
                        worksheet.Cell(startRow, startCol + 8).Value = item.Outside;
                        worksheet.Cell(startRow, startCol + 9).Value = item.Guard;
                        worksheet.Cell(startRow, startCol + 10).Value = item.RejectionDate?.ToString("MM/dd/yyyy");
                        startRow++;
                    }

                    if (registros.Count > 0)
                    {
                        int lastRow = startRow - 1;
                        int lastColumn = startCol + 10;

                        var dataRange = worksheet.Range(10, 1, lastRow, lastColumn);
                        dataRange.Style.Font.Bold = true;
                        dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                        dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                    }

                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();

                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "ResiduosNoPeligrosos.xlsx");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar el archivo Excel: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return StatusCode(500, "Error al generar el archivo Excel.");
            }
        }


        [HttpGet]
        public IActionResult ExportToExceFinishgoodl(
        string? category,
        string? program,
        string? partnumber,
        string? basenumer,
        string? molding,
        string? gap,
        string? connector,
        string? Heater,
        string? NTC,
        string? Inside,
        string? Outside,
        string? Guard,
        string? ElectricTest,
        string? datestart,
        string? dateend
        )
        {

            var plantillaPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "finishedgood_report.xlsx");
            if (System.IO.File.Exists(plantillaPath))
            {
                Console.WriteLine($"Archivo encontrado en: {plantillaPath}");
                // Procede con la carga o la operación con el archivo
            }
            else
            {
                Console.WriteLine("El archivo no existe en la ruta especificada.");
            }

            Console.WriteLine($"Ruta de la plantilla: {plantillaPath}");



            try
            {
                // Consulta con filtros dinámicos
                IQueryable<ViewRejectionmaterial> query = _context.ViewRejectionmaterials;

                if (!string.IsNullOrWhiteSpace(category))
                    query = query.Where(r => r.Category == category);

                if (!string.IsNullOrWhiteSpace(program))
                    query = query.Where(r => r.Program == program);

                if (!string.IsNullOrWhiteSpace(partnumber))
                    query = query.Where(r => r.Partnumber == partnumber);

                if (!string.IsNullOrWhiteSpace(basenumer))
                    query = query.Where(r => r.Base == basenumer);

                if (!string.IsNullOrWhiteSpace(molding))
                    query = query.Where(r => r.Molding == molding);

                if (!string.IsNullOrWhiteSpace(gap))
                    query = query.Where(r => r.Gap == gap);

                if (!string.IsNullOrWhiteSpace(connector))
                    query = query.Where(r => r.Connector == connector);

                if (!string.IsNullOrWhiteSpace(ElectricTest))
                {
                    if (ElectricTest == "DEFECT")
                        query = query.Where(e =>
                            (e.Heater != "OK" && e.Heater != "N/A") ||
                            (e.Ntc != "OK" && e.Ntc != "N/A") ||
                            (e.Inside != "OK" && e.Inside != "N/A") ||
                            (e.Outside != "OK" && e.Outside != "N/A") ||
                            (e.Guard != "OK" && e.Guard != "N/A")
                        );

                    else if (ElectricTest == "OK")
                        query = query.Where(e =>
                            e.Heater == "OK" ||
                            e.Ntc == "OK" ||
                            e.Inside == "OK" ||
                            e.Outside == "OK" ||
                            e.Guard == "OK"
                        );

                }

                if (!string.IsNullOrWhiteSpace(Heater))
                    query = query.Where(r => r.Heater == Heater);

                if (!string.IsNullOrWhiteSpace(NTC))
                    query = query.Where(r => r.Ntc == NTC);

                if (!string.IsNullOrWhiteSpace(Inside))
                    query = query.Where(r => r.Inside == Inside);

                if (!string.IsNullOrWhiteSpace(Outside))
                    query = query.Where(r => r.Outside == Outside);

                if (!string.IsNullOrWhiteSpace(Guard))
                    query = query.Where(r => r.Guard == Guard);
                if (DateTime.TryParse(datestart, out DateTime startDate))
                {
                    query = query.Where(r => r.RejectionDate >= startDate);
                }

                if (DateTime.TryParse(dateend, out DateTime endDate))
                {
                    // Para incluir todo el día hasta las 23:59:59
                    endDate = endDate.Date.AddDays(1).AddTicks(-1);
                    query = query.Where(r => r.RejectionDate <= endDate);
                }


                var registros = query.Take(1000).ToList();

                


               


                var DateRange = registros.Where(r => r.RejectionDate.HasValue).Select(r => r.RejectionDate.Value).ToList();
                DateTime? DateMax = null;
                DateTime? DateMin = null;
                List<Container> containers = new();

                if (DateRange.Any())
                {
                    DateMin = DateRange.Min();
                    DateMax = DateRange.Max();
                    



                }


                if (!registros.Any())
                {
                    return NoContent(); // Devuelve un 204 si no hay registros
                }

                // Crear el archivo Excel
                using (var workbook = new XLWorkbook(plantillaPath))
                {

                    IXLWorksheet worksheet;

                    if (workbook.Worksheets.Count == 0)
                    {
                        worksheet = workbook.AddWorksheet("Sheet1"); // Si no hay hojas, se crea una nueva
                    }
                    else
                    {
                        worksheet = workbook.Worksheet(1); // Selecciona la primera hoja
                    }
                    

                   
                    //Fecha de inicio y finalizacion
                    if (DateMax.HasValue)
                    {
                        worksheet.Cell(2, 3).Value = DateMin.Value.ToString("MM/dd/yyyy");
                        worksheet.Cell(2, 7).Value = DateMax.Value.ToString("MM/dd/yyyy");
                    }
                    int startRow = 5; // Empieza a insertar datos desde la fila 5
                    int startCol = 1; // Empieza desde la columna 


                    // Llenar con datos
                    foreach (var item in registros)
                    {
                        worksheet.Cell(startRow, startCol).Value = item.Program;
                        worksheet.Cell(startRow, startCol + 1).Value = item.Partnumber;
                        worksheet.Cell(startRow, startCol + 2).Value = item.Base;
                        worksheet.Cell(startRow, startCol + 3).Value = item.Heater;
                        worksheet.Cell(startRow, startCol + 4).Value = item.Ntc;
                        worksheet.Cell(startRow, startCol + 5).Value = item.Inside;
                        worksheet.Cell(startRow, startCol + 6).Value = item.Outside;
                        worksheet.Cell(startRow, startCol + 7).Value = item.Guard;
                        worksheet.Cell(startRow, startCol + 8).Value = item.RejectionDate?.ToString("MM/dd/yyyy");

                        startRow++; // Avanza a la siguiente fila
                    }
                    // 2. Aplicar estilos DESPUÉS de insertar los datos
                    if (registros.Count > 0)
                    {
                        int firstRow = 5;
                        int lastRow = startRow - 1;
                        int lastColumn = startCol + 8; // Solo insertas hasta +8 columnas (no +10 como estaba)

                        if (lastRow >= firstRow)
                        {
                            var dataRange = worksheet.Range(
                                firstRow,     // startRow
                                startCol,     // startColumn
                                lastRow,      // endRow
                                lastColumn    // endColumn
                            );

                            dataRange.Style.Font.Bold = true;
                            dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                            dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                        }
                    }




                    using var stream = new MemoryStream();
                    workbook.SaveAs(stream);
                    var content = stream.ToArray(); // aquí ya no hay stream abierto

                    return File(content,
                                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                                "ResiduosNoPeligrosos.xlsx");

                }
            }
            catch (Exception ex)
            {
                // Log de error más detallado
                Console.WriteLine($"Error al generar el archivo Excel: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                return StatusCode(500, "Error al generar el archivo Excel.");
            }
        }

        private IQueryable<ViewRejectionmaterial> GetFilter(IFormCollection form)
        {
            var query = _context.ViewRejectionmaterials.AsNoTracking().AsQueryable();

            


            if (!string.IsNullOrEmpty(form["Category"]))
                query = query.Where(e => e.Category.Contains(form["Category"]));

            if (!string.IsNullOrEmpty(form["Program"]))
                query = query.Where(e => e.Program != null && e.Program.Contains(form["Program"]));

            if (!string.IsNullOrEmpty(form["Base"]))
                query = query.Where(e => e.Base != null && e.Base.Contains(form["Base"]));
            if (!string.IsNullOrEmpty(form["ElectricTest"]))
            {
                if (form["ElectricTest"] == "DEFECT")
                    query = query.Where(e =>
                        (e.Heater != "OK" && e.Heater != "N/A") ||
                        (e.Ntc != "OK" && e.Ntc != "N/A") ||
                        (e.Inside != "OK" && e.Inside != "N/A") ||
                        (e.Outside != "OK" && e.Outside != "N/A") ||
                        (e.Guard != "OK" && e.Guard != "N/A")
                    );

                else if (form["ElectricTest"] == "OK")
                    query = query.Where(e =>
                        e.Heater == "OK" ||
                        e.Ntc == "OK" ||
                        e.Inside == "OK" ||
                        e.Outside == "OK" ||
                        e.Guard == "OK"
                    );

            }

            if (!string.IsNullOrEmpty(form["Heater"]))
            {
                if (form["Heater"] == "DEFECT")
                    query = query.Where(e => e.Heater != "OK" && e.Heater != "N/A");
                else if (form["Heater"] == "OK")
                    query = query.Where(e => e.Heater == "OK");
            }


            if (!string.IsNullOrEmpty(form["Ntc"]))
            {
                if (form["Ntc"] == "DEFECT")
                    query = query.Where(e => e.Ntc != "OK" && e.Ntc != "N/A");
                else if (form["Ntc"] == "OK")
                    query = query.Where(e => e.Ntc == "OK");
            }
         

            if (!string.IsNullOrEmpty(form["Inside"]))
            {
                if (form["Inside"] == "DEFECT")
                    query = query.Where(e => e.Inside != "OK" && e.Inside != "N/A");
                else if (form["Inside"] == "OK")
                    query = query.Where(e => e.Inside == "OK");
            }
                

            if (!string.IsNullOrEmpty(form["Outside"]))
            {
                if (form["Outside"] == "DEFECT")
                    query = query.Where(e => e.Outside != "OK" && e.Outside != "N/A");
                else if (form["Outside"] == "OK")
                    query = query.Where(e => e.Outside == "OK");
            }
  

            if (!string.IsNullOrEmpty(form["Guard"]))
            {
                if (form["Guard"] == "DEFECT")
                    query = query.Where(e => e.Guard != "OK" && e.Guard != "N/A");
                else if (form["Guard"] == "OK")
                    query = query.Where(e => e.Guard == "OK");
            }
           

            if (!string.IsNullOrEmpty(form["Molding"]))
                query = query.Where(e => e.Molding != null && e.Molding.Contains(form["Molding"]));

            if (!string.IsNullOrEmpty(form["Gap"]))
                query = query.Where(e => e.Gap != null && e.Gap.Contains(form["Gap"]));

            if (!string.IsNullOrEmpty(form["Connector"]))
                query = query.Where(e => e.Connector != null && e.Connector.Contains(form["Connector"]));

            if (!string.IsNullOrEmpty(form["Partnumber"]))
                query = query.Where(e => e.Partnumber != null && e.Partnumber.Contains(form["Partnumber"]));

            if (DateTime.TryParse(form["startDate"], out DateTime startDate))
                query = query.Where(e => e.RejectionDate != null && e.RejectionDate >= startDate);

            if (DateTime.TryParse(form["endDate"], out DateTime endDate))
            {
                endDate = endDate.Date.AddDays(1); 
                query = query.Where(e => e.RejectionDate != null && e.RejectionDate < endDate);
            }
                

            return query;
        }

        private object GetTable(IQueryable<ViewRejectionmaterial> query, int draw, int start, int length)
        {
            int total = _context.ViewRejectionmaterials.Count();
            int filtrados = query.Count();

            var datos = query.OrderByDescending(e => e.RejectionDate)
                             .Skip(start)
                             .Take(length)
                             .Select(e => new
                             {
                                 e.Category,
                                 e.Program,
                                 e.Base,
                                 e.Heater,
                                 e.Ntc,
                                 e.Inside,
                                 e.Outside,
                                 e.Guard,
                                 e.RejectionDate,
                                 e.Molding,
                                 e.Gap,
                                 e.Connector,
                                 e.Partnumber
                             })
                             .ToList();

            return new
            {
                draw,
                recordsTotal = total,
                recordsFiltered = filtrados,
                data = datos
            };
        }

        [HttpPost]
        public JsonResult SetTable()
        {
            try
            {
                int.TryParse(Request.Form["draw"], out int draw);
                int.TryParse(Request.Form["start"], out int start);
                int.TryParse(Request.Form["length"], out int length);

                var queryFiltrada = GetFilter(Request.Form);
                var resultado = GetTable(queryFiltrada, draw, start, length);

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        public IActionResult GetByBase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return BadRequest("El parámetro 'value' es null o vacío.");
            }

            var data = _context.Rejections
                .Where(r => r.Base != null && r.Base.Contains(value))
                .Select(r => new {
                    r.Id,
                    r.Program,
                    r.Base,
                    r.Heater,
                    r.Ntc,
                    r.Inside,
                    r.Outside,
                    r.Guard,
                    r.Molding,
                    r.Gap,
                    r.Connector,
                    RejectionDate = r.RejectionDate.HasValue ? r.RejectionDate.Value.ToString("MM/dd/yyyy") : ""
                })
                .ToList();

            return Json(data);
        }



        [HttpDelete]
        public IActionResult Delete(string value)
        {
            // Suponiendo que 'Folio' es una propiedad en la entidad Rejections
            var record = _context.Rejections.FirstOrDefault(r => r.Base == value);

            if (record == null)
                return NotFound();

            _context.Rejections.Remove(record);
            _context.SaveChanges();

            return Ok();
        }


    }
}
