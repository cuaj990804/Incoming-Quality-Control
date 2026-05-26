using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Incoming.Data;
using Incoming.Models;

namespace Incoming.Controllers
{
    public class DefectsController : Controller
    {
        private readonly IncomingContext _context;

        public DefectsController(IncomingContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<JsonResult> CreateAjax([FromBody] Defect defect)
        {
            if (ModelState.IsValid)
            {
                _context.Defects.Add(defect);
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Datos inválidos" });
        }

        [HttpPost]
        public async Task<JsonResult> EditAjax([FromBody] Defect defect)
        {
            if (!_context.Defects.Any(d => d.Id == defect.Id))
                return Json(new { success = false, message = "Defecto no encontrado" });

            _context.Defects.Update(defect);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<JsonResult> DeleteAjax([FromBody] int id)
        {
            var defect = await _context.Defects.FindAsync(id);
            if (defect == null)
                return Json(new { success = false, message = "Defecto no encontrado" });

            _context.Defects.Remove(defect);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }


        // GET: Defects
        public async Task<IActionResult> Index()
        {
            return View(await _context.Defects.ToListAsync());
        }

        [HttpGet]
        public async Task<IActionResult> Details()
        {
            var result = await _context.Defects
         .OrderBy(d => d.DefectName == "DAMAGED CABLE" ? 0 :  // Primera prioridad
                       d.DefectName == "MOLDING" ? 1 :  // Segunda prioridad
                       d.DefectName == "GAP" ? 2 : 3)  // Tercera prioridad
         .ThenBy(d => d.DefectName)  // Orden alfabético para el resto
         .ToListAsync();

            if (!result.Any())
            {
                return NotFound("No se encontraron resultados.");
            }

            return Ok(result);
        }

        // GET: Defects/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Defects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DefectName")] Defect defect)
        {
            if (ModelState.IsValid)
            {
                _context.Add(defect);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(defect);
        }

        // GET: Defects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var defect = await _context.Defects.FindAsync(id);
            if (defect == null)
            {
                return NotFound();
            }
            return View(defect);
        }

        // POST: Defects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DefectName")] Defect defect)
        {
            if (id != defect.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(defect);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DefectExists(defect.Id))
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
            return View(defect);
        }

        // GET: Defects/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var defect = await _context.Defects
                .FirstOrDefaultAsync(m => m.Id == id);
            if (defect == null)
            {
                return NotFound();
            }

            return View(defect);
        }

        // POST: Defects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var defect = await _context.Defects.FindAsync(id);
            if (defect != null)
            {
                _context.Defects.Remove(defect);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DefectExists(int id)
        {
            return _context.Defects.Any(e => e.Id == id);
        }
    }
}
