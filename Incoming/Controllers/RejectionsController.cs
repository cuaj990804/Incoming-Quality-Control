using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Incoming.Data;
using Incoming.Models;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2019.Drawing.Diagram11;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using System.Text.RegularExpressions;

namespace Incoming.Controllers
{
    public class RejectionsController : Controller
    {
        private readonly IncomingContext _context;

        public RejectionsController(IncomingContext context)
        {
            _context = context;
        }

        // GET: Rejections
        public async Task<IActionResult> Index()
        {
            return View(await _context.Rejections.ToListAsync());
        }

        [HttpPost]
        public JsonResult GetTable()
        {
            try
            {
                int.TryParse(Request.Form["draw"].FirstOrDefault(), out int NroPeticion);
                int.TryParse(Request.Form["length"].FirstOrDefault(), out int CantidadRegistros);
                int.TryParse(Request.Form["start"].FirstOrDefault(), out int OmitirRegistros);

                string ProgramFilter = Request.Form["Program"].FirstOrDefault() ?? "";
                string BaseFilter = Request.Form["Base"].FirstOrDefault() ?? "";
                string MoldingFilter = Request.Form["Molding"].FirstOrDefault() ?? "";
                string GAPFilter = Request.Form["GAP"].FirstOrDefault() ?? "";
                string ConnectorFilter = Request.Form["Connector"].FirstOrDefault() ?? "";
                string HeaterFilter = Request.Form["Heater"].FirstOrDefault() ?? "";
                string NTCFilter = Request.Form["NTC"].FirstOrDefault() ?? "";
                string InsideFilter = Request.Form["INSIDE"].FirstOrDefault() ?? "";
                string OUTSIDEFilter = Request.Form["OUTSIDE"].FirstOrDefault() ?? "";
                string GUARDFilter = Request.Form["GUARD"].FirstOrDefault() ?? "";

                // Obtener rango de fechas desde los parámetros
                string startDateIntoStr = Request.Form["startDate"].FirstOrDefault();
                string endDateIntoStr = Request.Form["endDate"].FirstOrDefault();

                DateTime? startDateInto = null, endDateInto = null, startDateOut = null, endDateOut = null;
                if (DateTime.TryParse(startDateIntoStr, out DateTime parsedStartDateInto))
                {
                    startDateInto = parsedStartDateInto;
                }
                if (DateTime.TryParse(endDateIntoStr, out DateTime parsedEndDateIntoStr))
                {
                    endDateInto = parsedEndDateIntoStr;
                }
                



                // Base query
                var queryEmpleado = _context.Rejections.AsQueryable();



                // Filtros adicionales
                if (!string.IsNullOrEmpty(ProgramFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Program.Contains(ProgramFilter));
                }

                if (!string.IsNullOrEmpty(BaseFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Base.Contains(BaseFilter)); // Ajusta según la lógica real
                }

                if (!string.IsNullOrEmpty(MoldingFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Molding.Contains(MoldingFilter));
                }

                if (!string.IsNullOrEmpty(GAPFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Gap.Contains(GAPFilter)); // Ajusta el campo según lo necesario
                }

                if (!string.IsNullOrEmpty(ConnectorFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Connector.Contains(ConnectorFilter)); // Ajusta el campo según lo necesario
                }

                if (!string.IsNullOrEmpty(HeaterFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Heater.Contains(HeaterFilter)); // Ajusta el campo según lo necesario
                }

                if (!string.IsNullOrEmpty(NTCFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Ntc.Contains(NTCFilter)); // Ajusta el campo según lo necesario
                }
                if (!string.IsNullOrEmpty(InsideFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Inside.Contains(InsideFilter)); // Ajusta el campo según lo necesario
                }
                if (!string.IsNullOrEmpty(OUTSIDEFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Outside.Contains(OUTSIDEFilter)); // Ajusta el campo según lo necesario
                }
                if (!string.IsNullOrEmpty(GUARDFilter))
                {
                    queryEmpleado = queryEmpleado.Where(e => e.Guard.Contains(GUARDFilter)); // Ajusta el campo según lo necesario
                }


                if (startDateInto.HasValue || endDateInto.HasValue)
                {
                    if (startDateInto.HasValue) {
                        queryEmpleado = queryEmpleado.Where(e => e.RejectionDate >= startDateInto);

                    }
                        

                    if (endDateInto.HasValue)
                    {
                        queryEmpleado = queryEmpleado.Where(e => e.RejectionDate < endDateInto.Value);
                    }

                }
                        

                if (startDateOut.HasValue || endDateOut.HasValue)
                {
                    if (startDateOut.HasValue)
                        queryEmpleado = queryEmpleado.Where(e => e.RejectionDate.HasValue &&
                                                               e.RejectionDate >= startDateOut);

                    if (endDateOut.HasValue)
                        queryEmpleado = queryEmpleado.Where(e => e.RejectionDate.HasValue &&
                                                     e.RejectionDate < endDateOut.Value.AddDays(1));
                }




                // Total registros antes y después del filtro
                int TotalRegistros = _context.Rejections.Count();
                int TotalRegistrosFiltrados = queryEmpleado.Count();
                // Ordenar por fecha y hora de manera descendente
                queryEmpleado = queryEmpleado.OrderByDescending(e => e.RejectionDate);



                var lista = queryEmpleado
                    .Skip(OmitirRegistros)
                    .Take(CantidadRegistros)
                    .Select(e => new
                    {
                        e.Program,
                        e.Base,
                        e.Molding,
                        e.Gap,
                        e.Connector,
                        e.Heater,
                        e.Ntc,
                        e.Inside,
                        e.Outside,
                        e.Guard,
                        e.RejectionDate,
                        
                    })
                    .ToList();

                // Respuesta al cliente
                return Json(new
                {
                    draw = NroPeticion,
                    recordsTotal = TotalRegistros,
                    recordsFiltered = TotalRegistrosFiltrados,
                    data = lista
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public IActionResult ExportToExcel([FromQuery] DateTime? startDate,
                                            [FromQuery] DateTime? endDate,
                                            [FromQuery] string Program,
                                            [FromQuery] string Base,
                                            [FromQuery] string Molding,
                                            [FromQuery] string Gap,
                                            [FromQuery] string Connector,
                                            [FromQuery] string Heater,
                                            [FromQuery] string Ntc,
                                            [FromQuery] string Inside,
                                            [FromQuery] string Outside,
                                            [FromQuery] string Guard
                                            )
        {
            try
            {
                IQueryable<Rejection> query = _context.Rejections;

                if (!string.IsNullOrWhiteSpace(Program))
                    query = query.Where(r => r.Program == Program);

                if (!string.IsNullOrWhiteSpace(Base))
                    query = query.Where(r => r.Base == Base);

                if (!string.IsNullOrWhiteSpace(Molding))
                    query = query.Where(r => r.Molding == Molding);

                if (!string.IsNullOrWhiteSpace(Gap))
                    query = query.Where(r => r.Gap == Gap);

                if (!string.IsNullOrWhiteSpace(Connector))
                    query = query.Where(r => r.Connector == Connector);

                if (!string.IsNullOrWhiteSpace(Heater))
                    query = query.Where(r => r.Heater == Heater);

                if (!string.IsNullOrWhiteSpace(Ntc))
                    query = query.Where(r => r.Ntc == Ntc);

                if (!string.IsNullOrWhiteSpace(Inside))
                    query = query.Where(r => r.Inside == Inside);

                if (!string.IsNullOrWhiteSpace(Outside))
                    query = query.Where(r => r.Outside == Outside);

                if (!string.IsNullOrWhiteSpace(Guard))
                    query = query.Where(r => r.Guard == Guard);




                // Create the workbook
                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("Julians Data");

                    // Agregar encabezados a las columnas
                    worksheet.Cell(1, 1).Value = "Fecha";
                    worksheet.Cell(1, 2).Value = "Programa";
                    worksheet.Cell(1, 3).Value = "Base";
                    worksheet.Cell(1, 4).Value = "Moldeo";
                    worksheet.Cell(1, 5).Value = "GAP";
                    worksheet.Cell(1, 6).Value = "Conector";
                    worksheet.Cell(1, 7).Value = "Heater";
                    worksheet.Cell(1, 8).Value = "NTC";
                    worksheet.Cell(1, 9).Value = "Inside";
                    worksheet.Cell(1, 10).Value = "Outside";
                    worksheet.Cell(1, 11).Value = "Guard";
                    // Ajustar el ancho de las columnas
                    worksheet.Column(1).Width = 15; 
                    worksheet.Column(2).Width = 20; 
                    worksheet.Column(3).Width = 25; 
                    worksheet.Column(4).Width = 30; 
                    worksheet.Column(5).Width = 30; 
                    worksheet.Column(6).Width = 20;

                    worksheet.Column(7).Width = 15;
                    worksheet.Column(8).Width = 20;
                    worksheet.Column(9).Width = 25;
                    worksheet.Column(10).Width = 30;
                    worksheet.Column(11).Width = 30;

                    var data = query.Take(1000).ToList();

                    // Rellenar filas con los datos
                    for (int i = 0; i < data.Count; i++)
                    {
                        worksheet.Cell(i + 2, 1).Value = data[i].RejectionDate?.ToString("MM/dd/yyyy") ?? "N/A";
                        worksheet.Cell(i + 2, 2).Value = data[i].Program;
                        worksheet.Cell(i + 2, 3).Value = data[i].Base;
                        worksheet.Cell(i + 2, 4).Value = data[i].Molding;
                        worksheet.Cell(i + 2, 5).Value = data[i].Gap;
                        worksheet.Cell(i + 2, 6).Value = data[i].Connector;

                        worksheet.Cell(i + 2, 7).Value = ProcessField(data[i].Heater);
                        worksheet.Cell(i + 2, 8).Value = ProcessField(data[i].Ntc);
                        worksheet.Cell(i + 2, 9).Value = ProcessField(data[i].Inside);
                        worksheet.Cell(i + 2, 10).Value = ProcessField(data[i].Outside);
                        worksheet.Cell(i + 2, 11).Value = ProcessField(data[i].Guard);
                    }

                    // Send the file
                    MemoryStream excelStream = new MemoryStream();
                    workbook.SaveAs(excelStream);
                    excelStream.Position = 0;
                    return File(excelStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MyFileName.xlsx");
                }


            }
            catch (Exception ex)
            {
                // Loguea la excepción para obtener más detalles
                Console.WriteLine($"Error al guardar el archivo: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // Agrega esta línea para obtener más detalles

                // Retorna un mensaje detallado para el cliente
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
        private string ProcessField(string value)
        {
            if (string.IsNullOrEmpty(value)) return "OVERFLOW";

            string strValue = value.Trim().ToUpper();

            if (strValue == "OK" || strValue == "N/A") return value;

            if (Regex.IsMatch(strValue, @"^-?\d+\.?\d*$")) return value;

            return "OVERFLOW";
        }

        // GET: Rejections/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rejection = await _context.Rejections
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rejection == null)
            {
                return NotFound();
            }

            return View(rejection);
        }

        // GET: Rejections/Create

        public IActionResult Create()
        {
            return View();
        }

        // POST: Rejections/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Program,Base,Heater,Ntc,Inside,Outside,Guard,RejectionDate")] Rejection rejection)
        {
            if (ModelState.IsValid)
            {
                _context.Add(rejection);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(rejection);
        }



        // GET: Rejections/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rejection = await _context.Rejections.FindAsync(id);
            if (rejection == null)
            {
                return NotFound();
            }
            return View(rejection);
        }

        // POST: Rejections/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Program,Base,Heater,Ntc,Inside,Outside,Guard,RejectionDate")] Rejection rejection)
        {
            if (id != rejection.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(rejection);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RejectionExists(rejection.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(rejection);
        }

        // GET: Rejections/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var rejection = await _context.Rejections
                .FirstOrDefaultAsync(m => m.Id == id);
            if (rejection == null)
            {
                return NotFound();
            }

            return View(rejection);
        }

        // POST: Rejections/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var rejection = await _context.Rejections.FindAsync(id);
            if (rejection != null)
            {
                _context.Rejections.Remove(rejection);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool RejectionExists(int id)
        {
            return _context.Rejections.Any(e => e.Id == id);
        }
        [HttpPost]
        public async Task<IActionResult> InsertData([FromBody] Rejection data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Datos inválidos" }); // Usar objeto
            }


            try
            {
                // Verificar si el volante ya existe en la tabla de rechazos
                var existingRejection = await _context.Rejections
                                                   .FirstOrDefaultAsync(r => r.Base == data.Base);

                if (existingRejection != null)
                {
                    // Retornar información detallada de los defectos encontrados
                    return Conflict(new {
                        message = "El volante ya fue escaneado con defectos anteriormente",
                        hasDefects = true,
                        rejectionId = existingRejection.Id,
                        defects = new {
                            molding = existingRejection.Molding,
                            gap = existingRejection.Gap,
                            connector = existingRejection.Connector,
                            heater = existingRejection.Heater,
                            ntc = existingRejection.Ntc,
                            inside = existingRejection.Inside,
                            outside = existingRejection.Outside,
                            guard = existingRejection.Guard
                        },
                        rejectionDate = existingRejection.RejectionDate,
                        program = existingRejection.Program,
                        baseNumber = existingRejection.Base
                    });
                }

                // Verificar si el volante existe en la tabla de aceptados (sin defectos)
                var existingAccepted = await _context.AcceptedPieces
                                                   .FirstOrDefaultAsync(a => a.Base == data.Base);

                // Si el volante estaba en AcceptedPieces, eliminarlo porque ahora tiene defectos
                if (existingAccepted != null)
                {
                    _context.AcceptedPieces.Remove(existingAccepted);
                }

                var rejectionRecord = new Rejection
                {
                    Program = data.Program,
                    Base = data.Base,
                    Heater = data.Heater,
                    Ntc = data.Ntc,
                    Inside = data.Inside,
                    Outside = data.Outside,
                    Guard = data.Guard,
                    Molding=data.Molding,
                    Gap=data.Gap,
                    Connector=data.Connector,
                };

                await _context.Rejections.AddAsync(rejectionRecord);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Datos insertados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> InsertAcceptedData([FromBody] AcceptedPiece data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            try
            {
                // Verificar si el volante ya existe en la tabla de rechazos
                var existingRejection = await _context.Rejections
                                                   .FirstOrDefaultAsync(r => r.Base == data.Base);

                if (existingRejection != null)
                {
                    // Retornar información detallada de los defectos encontrados
                    return Conflict(new {
                        message = "El volante ya fue escaneado con defectos anteriormente",
                        hasDefects = true,
                        rejectionId = existingRejection.Id,
                        defects = new {
                            molding = existingRejection.Molding,
                            gap = existingRejection.Gap,
                            connector = existingRejection.Connector,
                            heater = existingRejection.Heater,
                            ntc = existingRejection.Ntc,
                            inside = existingRejection.Inside,
                            outside = existingRejection.Outside,
                            guard = existingRejection.Guard
                        },
                        rejectionDate = existingRejection.RejectionDate,
                        program = existingRejection.Program,
                        baseNumber = existingRejection.Base
                    });
                }

                // Verificar si el volante ya existe en la tabla de aceptados
                var existingAccepted = await _context.AcceptedPieces
                                                   .FirstOrDefaultAsync(a => a.Base == data.Base);

                if (existingAccepted != null)
                {
                    return Conflict(new {
                        message = "El volante ya fue escaneado sin defectos",
                        hasDefects = false,
                        acceptedDate = existingAccepted.AcceptedDate
                    });
                }

                var acceptedRecord = new AcceptedPiece
                {
                    Program = data.Program,
                    Base = data.Base,
                    Heater = data.Heater,
                    Ntc = data.Ntc,
                    Inside = data.Inside,
                    Outside = data.Outside,
                    Guard = data.Guard,
                    Molding = data.Molding,
                    Gap = data.Gap,
                    Connector = data.Connector,
                };

                await _context.AcceptedPieces.AddAsync(acceptedRecord);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Volante sin defectos registrado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConvertRejectionToAccepted([FromBody] AcceptedPiece data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            try
            {
                // Buscar el registro en Rejections
                var existingRejection = await _context.Rejections
                                                   .FirstOrDefaultAsync(r => r.Base == data.Base);

                if (existingRejection == null)
                {
                    return NotFound(new { message = "No se encontró el volante en rechazos" });
                }

                // Eliminar de Rejections
                _context.Rejections.Remove(existingRejection);

                // Crear nuevo registro en AcceptedPieces
                var acceptedRecord = new AcceptedPiece
                {
                    Program = data.Program,
                    Base = data.Base,
                    Heater = data.Heater,
                    Ntc = data.Ntc,
                    Inside = data.Inside,
                    Outside = data.Outside,
                    Guard = data.Guard,
                    Molding = data.Molding,
                    Gap = data.Gap,
                    Connector = data.Connector,
                };

                await _context.AcceptedPieces.AddAsync(acceptedRecord);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Volante movido de rechazos a aceptados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet]
        public JsonResult GetRejectionCounts()
        {
            var today = DateTime.Today; // Obtiene la fecha actual del servidor

            var counts = _context.Rejections
                .Where(r => EF.Functions.DateDiffDay(r.RejectionDate, today) == 0) // Filtra por registros del día actual
                .GroupBy(r => r.Program)
                .Select(g => new
                {
                    ProgramName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(g => g.Count)
                .ToList();

            return Json(counts);
        }

        [HttpGet]
        public async Task<IActionResult> GetDefectData(int id)
        {
            try
            {
                var rejection = await _context.Rejections.FindAsync(id);
                if (rejection == null)
                {
                    return NotFound(new { message = "Defecto no encontrado" });
                }

                return Ok(new
                {
                    id = rejection.Id,
                    program = rejection.Program,
                    baseNumber = rejection.Base,
                    molding = rejection.Molding,
                    gap = rejection.Gap,
                    connector = rejection.Connector,
                    heater = rejection.Heater,
                    ntc = rejection.Ntc,
                    inside = rejection.Inside,
                    outside = rejection.Outside,
                    guard = rejection.Guard,
                    rejectionDate = rejection.RejectionDate
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateDefect([FromBody] Rejection data)
        {
            if (data == null || data.Id == 0)
            {
                return BadRequest(new { message = "Datos inválidos" });
            }

            try
            {
                var existingRejection = await _context.Rejections.FindAsync(data.Id);
                if (existingRejection == null)
                {
                    return NotFound(new { message = "Defecto no encontrado" });
                }

                // Actualizar los campos
                existingRejection.Program = data.Program;
                existingRejection.Base = data.Base;
                existingRejection.Molding = data.Molding;
                existingRejection.Gap = data.Gap;
                existingRejection.Connector = data.Connector;
                existingRejection.Heater = data.Heater;
                existingRejection.Ntc = data.Ntc;
                existingRejection.Inside = data.Inside;
                existingRejection.Outside = data.Outside;
                existingRejection.Guard = data.Guard;

                _context.Rejections.Update(existingRejection);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "Defecto actualizado correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

    }
}
