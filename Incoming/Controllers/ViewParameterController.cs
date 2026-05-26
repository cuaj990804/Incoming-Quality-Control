using Incoming.Data;
using Incoming.DTO;
using Incoming.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Incoming.Controllers
{

    public class ViewParameterController : Controller
    {
        private readonly IncomingContext _context;

        public ViewParameterController(IncomingContext context)
        {
            _context = context;
        }
        // GET: ViewParameterController
        public async Task<IActionResult> Index()
        {
            var parameters = await _context.ViewParameters.ToListAsync();
            ViewBag.ProgramList = _context.InspectionPrograms.Select(p => p.ProgramName).ToList();

            return View(parameters);
        }
       

        private object GetTable(int draw, int start, int length)
        {
            var groupedData = _context.ViewParameters
                .GroupBy(e => e.ProgramName)
                .OrderByDescending(g => g.Key)
                .Skip(start)
                .Take(length)
                .Select(g => new
                {
                    Program = g.Key,

                    HeaterMin = g.Where(e => e.Test == "HEATER").Select(e => e.Minimum).FirstOrDefault(),
                    HeaterMax = g.Where(e => e.Test == "HEATER").Select(e => e.Maximum).FirstOrDefault(),

                    NtcMin = g.Where(e => e.Test == "NTC").Select(e => e.Minimum).FirstOrDefault(),
                    NtcMax = g.Where(e => e.Test == "NTC").Select(e => e.Maximum).FirstOrDefault(),

                    InsideMin = g.Where(e => e.Test == "INSIDE").Select(e => e.Minimum).FirstOrDefault(),
                    InsideMax = g.Where(e => e.Test == "INSIDE").Select(e => e.Maximum).FirstOrDefault(),

                    OutsideMin = g.Where(e => e.Test == "OUTSIDE").Select(e => e.Minimum).FirstOrDefault(),
                    OutsideMax = g.Where(e => e.Test == "OUTSIDE").Select(e => e.Maximum).FirstOrDefault(),

                    GuardMin = g.Where(e => e.Test == "GUARD").Select(e => e.Minimum).FirstOrDefault(),
                    GuardMax = g.Where(e => e.Test == "GUARD").Select(e => e.Maximum).FirstOrDefault()
                })
                .ToList();

            return new
            {
                draw = draw,
                recordsTotal = _context.ViewParameters.Select(x => x.ProgramName).Distinct().Count(),
                recordsFiltered = _context.ViewParameters.Select(x => x.ProgramName).Distinct().Count(),
                data = groupedData
            };
        }
        [HttpDelete]
        public async Task<IActionResult> DeleteProgram(int programId)
        {
            // 1. Eliminar los parámetros relacionados
            var parameters = await _context.Parameters
                .Where(p => p.ProgramId == programId)
                .ToListAsync();

            if (parameters.Any())
            {
                _context.Parameters.RemoveRange(parameters);
                await _context.SaveChangesAsync();
            }

            // 2. Eliminar el programa
            var program = await _context.InspectionPrograms
                .FirstOrDefaultAsync(p => p.Id == programId);

            if (program == null)
            {
                return NotFound(new { message = $"Programa con ID {programId} no encontrado." });
            }

            _context.InspectionPrograms.Remove(program);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Programa {programId} y sus parámetros fueron eliminados correctamente." });
        }

        [HttpPut]
        public async Task<IActionResult> RenameProgram(int programId, [FromBody] string newName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(newName))
                {
                    return BadRequest(new { message = "El nuevo nombre no puede estar vacío." });
                }

                // Buscar el programa
                var program = await _context.InspectionPrograms
                    .FirstOrDefaultAsync(p => p.Id == programId);

                if (program == null)
                {
                    return NotFound(new { message = $"Programa con ID {programId} no encontrado." });
                }

                // Verificar si ya existe un programa con el nuevo nombre
                var existingProgram = await _context.InspectionPrograms
                    .FirstOrDefaultAsync(p => p.ProgramName == newName.ToUpper());

                if (existingProgram != null && existingProgram.Id != programId)
                {
                    return Conflict(new { message = $"Ya existe un programa con el nombre '{newName}'." });
                }

                // Actualizar el nombre
                program.ProgramName = newName.ToUpper();
                await _context.SaveChangesAsync();

                return Ok(new { message = $"Programa renombrado exitosamente a '{newName}'." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error al renombrar el programa.", error = ex.Message });
            }
        }

        [HttpPost]
        public JsonResult SetTable()
        {
            try
            {
                int.TryParse(Request.Form["draw"], out int draw);
                int.TryParse(Request.Form["start"], out int start);
                int.TryParse(Request.Form["length"], out int length);

                var resultado = GetTable(draw, start, length);

                return Json(resultado);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public IActionResult InsertParameters([FromBody] Parameter request)
        {
            try
            {
                // Validar entrada
                if (request == null)
                    return BadRequest("Datos de entrada inválidos");

                if (string.IsNullOrWhiteSpace(request.Test))
                    return BadRequest("El nombre de la prueba es requerido");

                if (request.Minimum >= request.Maximum)
                    return BadRequest("El valor mínimo debe ser menor que el máximo");

                // Verificar si ya existe un registro
                var exists = _context.Parameters
                    .Any(p => p.ProgramId == request.ProgramId &&
                              p.Test.ToLower() == request.Test);



                if (exists)
                    return Conflict($"Ya existe un parámetro para el programa {request.Id} y prueba '{request.Test}'");

                // Crear nuevo parámetro
                var newParameter = new Parameter
                {
                    ProgramId = request.ProgramId, // ✅ usa `ProgramId` correctamente
                    Test = request.Test,
                    Minimum = request.Minimum,
                    Maximum = request.Maximum
                };


                _context.Parameters.Add(newParameter);
                _context.SaveChanges();

                return Ok(new
                {
                    message = "Insertado exitosamente",
                    id = newParameter.Id,
                    test = newParameter.Test
                });
            }
            catch (Exception ex)
            {
                // Registrar el error
                return StatusCode(500, "Error interno del servidor");
            }
        }


        [HttpPut]
        public IActionResult UpdateParameters([FromBody] List<ParameterDTO> updates)
        {
            try
            {
                foreach (var u in updates)
                {
                    // Buscar por ProgramId y Test (no por Id del parámetro)
                    var param = _context.Parameters
                        .FirstOrDefault(p => p.ProgramId == u.ProgramId && p.Test == u.Test);

                    if (param != null)
                    {
                        // Actualizar valores existentes
                        param.Minimum = u.Minimum;
                        param.Maximum = u.Maximum;
                    }
                    else
                    {
                        // Crear nuevo parámetro si no existe
                        _context.Parameters.Add(new Parameter
                        {
                            ProgramId = u.ProgramId,
                            Test = u.Test,
                            Minimum = u.Minimum,
                            Maximum = u.Maximum
                        });
                    }
                }

                int affected = _context.SaveChanges();
                return Ok(new
                {
                    success = true,
                    message = $"Actualizados {affected} parámetros"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error interno: " + ex.Message
                });
            }
        }

        [HttpGet]
        public IActionResult GetParameters(string program)
        {
            var result = _context.ViewParameters
                .Where(p => p.ProgramName == program)
                .Select(p => new
                {
                    test = p.Test,
                    Min = p.Minimum,
                    Max = p.Maximum
                })
                .ToList();

            return Ok(result);
        }

        [HttpGet]
        public IActionResult GetProgramId(string programName)
        {
            var program = _context.InspectionPrograms
                .FirstOrDefault(p => p.ProgramName == programName);

            if (program == null)
                return NotFound();

            return Ok(program.Id); // Devuelve directamente el ID (int)
        }


        [HttpGet]
        public async Task<IActionResult> Details(string partNumber)
        {
            if (string.IsNullOrEmpty(partNumber))
            {
                return BadRequest("El número de parte es requerido.");
            }
            //if (partNumber.Contains("KM74")) { partNumber = "EJ / KM"; }


            var result = await _context.ViewParameters
                .Where(i => partNumber.Contains(i.ProgramName))
                .ToListAsync();

            // Encontrar el programa con el nombre más largo que coincida (más específico)
            var bestMatch = result
                .Select(i => i.ProgramName)
                .Distinct()
                .Where(programName => partNumber.Contains(programName))
                .OrderByDescending(programName => programName.Length)
                .FirstOrDefault();

            if (string.IsNullOrEmpty(bestMatch))
            {
                return NotFound("No se encontraron resultados.");
            }

            // Filtrar solo por el mejor match y agrupar
            var filteredResult = result
                .Where(i => i.ProgramName == bestMatch)
                .GroupBy(i => new { i.ProgramName, i.Test })
                .Select(g => new
                {
                    ProgramName = g.Key.ProgramName,
                    Test = g.Key.Test,
                    MinValue = g.Min(i => i.Minimum),
                    MaxValue = g.Max(i => i.Maximum)
                })
                .ToList();

            result = filteredResult.Select(r => new ViewParameter
            {
                ProgramName = r.ProgramName,
                Test = r.Test,
                Minimum = r.MinValue,
                Maximum = r.MaxValue
            }).ToList();

            // Filtrar los resultados por Test específicos
            var orderedResult = result
                .Where(r => new[] { "HEATER", "NTC", "INSIDE", "OUTSIDE", "GUARD" }
                .Contains(r.Test)) // Filtra solo los Test específicos
                .OrderBy(r => Array.IndexOf(new[] { "HEATER", "NTC", "INSIDE", "OUTSIDE", "GUARD" }, r.Test)) // Ordena según el orden deseado
                .Select(r => new
                {
                    ProgramName = r.ProgramName,
                    Test = r.Test,
                    MinValue = r.Minimum,
                    MaxValue = r.Maximum
                })
                .ToList();

            // Devolver los resultados agrupados y la lista de números de parte
            return Json(new { orderedResult });
        }



        // GET: ViewParameterController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ViewParameterController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ViewParameterController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ViewParameterController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ViewParameterController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ViewParameterController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}