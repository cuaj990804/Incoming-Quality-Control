using Incoming.Data;
using Incoming.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGeneration.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Incoming.Controllers
{

    public class ContainersController : Controller
    {
        private readonly IncomingContext _context;

        public ContainersController(IncomingContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult EditContainer([FromBody] Container updatedContainer)
        {
            if (updatedContainer == null || updatedContainer.Id == 0)
            {
                return BadRequest("Datos inválidos.");
            }

            var container = _context.Containers.FirstOrDefault(c => c.Id == updatedContainer.Id);
            if (container == null)
            {
                return NotFound();
            }

            // Actualizar los campos
            container.Quantity = updatedContainer.Quantity;
            container.Status = updatedContainer.Status;
            container.DateEnd= updatedContainer.DateEnd; // Si DateEnd es null, usar la fecha actual

            _context.SaveChanges();

            return Ok(new { success = true, message = "Contenedor actualizado correctamente." });
        }

        [HttpGet]
        public IActionResult GetContainerInfo(int id)
        {
            var container = _context.Containers
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    quantity = c.Quantity,
                    status = c.Status,
                    program = c.Program,
                    dateEnd = c.DateEnd.HasValue
                        ? c.DateEnd.Value.ToString("dd/MM/yyyy")
                        : null
                })
                .FirstOrDefault();

            if (container == null)
            {
                return NotFound();
            }

            return Json(container);
        }

        private IQueryable<Container> GetFilter(IFormCollection form)
        {
            var query = _context.Containers.AsQueryable();

            if (!string.IsNullOrEmpty(form["Program"]))
                query = query.Where(e => e.Program.Contains(form["Program"]));

            if (!string.IsNullOrEmpty(form["quantity"]) && int.TryParse(form["quantity"], out int QuantityValue))
            {
                query = query.Where(e => e.Quantity == QuantityValue);
            }
            if (!string.IsNullOrEmpty(form["Status"]))
                query = query.Where(e => e.Status.Contains(form["Status"]));

            // ... Repite para los demás filtros ...

            if (DateTime.TryParse(form["dateStart"], out DateTime startDate))
                query = query.Where(e => e.DateStart >= startDate);

            if (DateTime.TryParse(form["dateEnd"], out DateTime endDate))
                query = query.Where(e => e.DateEnd <= endDate);

            return query;
        }
        private object GetTable(IQueryable<Container> query, int draw, int start, int length)
        {
            int total = _context.Containers.Count();
            int filtrados = query.Count();

            var datos = query.OrderByDescending(e => e.DateStart)
                             .Skip(start)
                             .Take(length)
                             .Select(e => new
                             {
                                 id=e.Id,
                                 e.Program,
                                 e.Quantity,
                                 e.PartialCount,
                                 e.Status,
                                 e.DateStart,
                                 e.DateEnd,
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


        
        [HttpPost]
        public async Task<IActionResult> UpdateContainerQuantity(string program, int quantity)
        {
            try
            {
                // Buscar el primer contenedor que coincida con el programa y esté abierto
                var container = await _context.Containers
                    .FirstOrDefaultAsync(c => c.Program == program && c.Status == "OPEN");

                if (container == null)
                {
                    return NotFound(new { error = "Contenedor no encontrado o no está abierto" });
                }

                // Actualizar la cantidad
                container.Quantity = quantity;

                // Guardar cambios
                await _context.SaveChangesAsync();

                return Ok(new { message = "Cantidad actualizada correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetQuantity(string program)
        {
            try
            {
                // Busca el primer contenedor con el programa dado y estado "OPEN"
                var container = await _context.Containers
                    .FirstOrDefaultAsync(c => c.Program == program && c.Status == "OPEN");

                if (container != null)
                {
                    // Si encuentra, devuelve el valor de Quantity
                    return Json(new { quantity = container.Quantity });
                }
                else
                {
                    // Si no encuentra, devuelve 0 o lo que prefieras
                    return Json(new { quantity = 0 });
                }
            }
            catch (Exception ex)
            {
                // En caso de error, responde 500
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetProgramStatus(string program)
        {
            try
            {
                // Comprueba si existe al menos un contenedor OPEN para este programa
                bool anyOpen = await _context.Containers
                    .AnyAsync(c => c.Program == program && c.Status == "OPEN");

                // Si hay alguno OPEN → devolvemos OPEN, si no → CLOSE
                var status = anyOpen ? "OPEN" : "CLOSE";
                return Json(new { status });
            }
            catch (Exception ex)
            {
                // En caso de error, devolvemos 500 con mensaje genérico
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }



        [HttpGet]
        public async Task<IActionResult> GetProgramCounts(string program)
        {
            try
            {
                var container = await _context.Containers
                    .Where(c => c.Program == program && c.Status == "OPEN")
                    .Select(c => new
                    {
                        c.Quantity, // Mantener PascalCase
                        c.PartialCount
                    })
                    .FirstOrDefaultAsync();

                return Json(container ?? new { Quantity = 0, PartialCount = 0 }); // Valores por defecto en 0
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }


        [HttpPost]
        public async Task<IActionResult> UpdatePartialCount(string program)
        {
            try
            {
                // 1. Obtener el contenedor (no solo el PartialCount)
                var container = await _context.Containers
                    .FirstOrDefaultAsync(c => c.Program == program && c.Status == "OPEN");

                if (container == null)
                {
                    return NotFound(new { error = "Contenedor no encontrado" });
                }

                // 2. Actualizar el PartialCount (ej. incrementar en 1)
                container.PartialCount++; // O la lógica que necesites

                if (container.PartialCount >= container.Quantity)
                {
                    container.Status = "CLOSE";
                    container.DateEnd = DateTime.Now;
                }

                // 3. Guardar cambios
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    PartialCount = container.PartialCount,
                    Quantity = container.Quantity,
                    Status = container.Status
                });
            }
            catch (Exception ex)
            {
                // Registrar el error (recomendado)
                // _logger.LogError(ex, "Error actualizando PartialCount");
                return StatusCode(500, new { error = "Error interno del servidor" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> InsertData([FromBody] Container data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Datos inválidos" }); // Usar objeto
            }


            try
            {
                // Verificar si el valor de 'Base' ya existe en la base de datos
                var existingContainer = await _context.Containers
                  .FirstOrDefaultAsync(r =>
                      r.Status == "OPEN" &&
                      r.Program == data.Program // <-- Comparar el mismo programa
                  );

                if (existingContainer != null)
                {
                    return Conflict(new { message = "Ya existe un contenedor en curso." }); // Mensaje consistente
                }
                var containerRecord = new Container
                {
                    Program = data.Program,
                    Quantity = data.Quantity
                    
                };

                await _context.Containers.AddAsync(containerRecord);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Datos insertados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        // GET: Containers
        public async Task<IActionResult> Index()
        {
            return View(await _context.Containers.ToListAsync());
        }

        // GET: Containers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // GET: Containers/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Containers/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Program,Quantity,PartialCount,Status,DateStart,DateEnd")] Container container)
        {
            if (ModelState.IsValid)
            {
                _context.Add(container);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(container);
        }

        // GET: Containers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers.FindAsync(id);
            if (container == null)
            {
                return NotFound();
            }
            return View(container);
        }

        // POST: Containers/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Program,Quantity,PartialCount,Status,DateStart,DateEnd")] Container container)
        {
            if (id != container.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(container);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ContainerExists(container.Id))
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
            return View(container);
        }

        // GET: Containers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var container = await _context.Containers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (container == null)
            {
                return NotFound();
            }

            return View(container);
        }

        // POST: Containers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var container = await _context.Containers.FindAsync(id);
            if (container != null)
            {
                _context.Containers.Remove(container);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ContainerExists(int id)
        {
            return _context.Containers.Any(e => e.Id == id);
        }
    }
}
