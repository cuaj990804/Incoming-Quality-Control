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
    public class FinishedgoodsController : Controller
    {
        private readonly IncomingContext _context;

        public FinishedgoodsController(IncomingContext context)
        {
            _context = context;
        }
        [HttpPost]
        public async Task<IActionResult> InsertData([FromBody] Finishedgood data)
        {
            if (data == null)
            {
                return BadRequest(new { message = "Datos inválidos" }); // Usar objeto
            }


            try
            {
                // Buscar registro existente incluyendo la fecha de creación
                var existingRecord = await _context.Finishedgoods
                    .Where(r => r.Base == data.Base)
                    .Select(r => new {
                        r.Base,
                        r.RejectionDate,

                    })
                    .FirstOrDefaultAsync();
               

                if (existingRecord != null)
                {
                    return Conflict(new { message = $"El volante ya fue escaneado el {existingRecord.RejectionDate}" }); // Mensaje consistente
                }
                var Record = new Finishedgood
                {
                    Program = data.Program,
                    Partnumber=data.Partnumber,
                    Base = data.Base,
                    Heater = data.Heater,
                    Ntc = data.Ntc,
                    Inside = data.Inside,
                    Outside = data.Outside,
                    Guard = data.Guard,
                    
                };

                await _context.Finishedgoods.AddAsync(Record);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = "Datos insertados correctamente" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Error interno: {ex.Message}" });
            }
        }

        // GET: Finishedgoods
        public async Task<IActionResult> Index()
        {
            return View(await _context.Finishedgoods.ToListAsync());
        }

        // GET: Finishedgoods/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finishedgood = await _context.Finishedgoods
                .FirstOrDefaultAsync(m => m.Id == id);
            if (finishedgood == null)
            {
                return NotFound();
            }

            return View(finishedgood);
        }

        // GET: Finishedgoods/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Finishedgoods/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Program,Partnumber,Base,Heater,Ntc,Inside,Outside,Guard,RejectionDate")] Finishedgood finishedgood)
        {
            if (ModelState.IsValid)
            {
                _context.Add(finishedgood);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(finishedgood);
        }

        // GET: Finishedgoods/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finishedgood = await _context.Finishedgoods.FindAsync(id);
            if (finishedgood == null)
            {
                return NotFound();
            }
            return View(finishedgood);
        }

        // POST: Finishedgoods/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Program,Partnumber,Base,Heater,Ntc,Inside,Outside,Guard,RejectionDate")] Finishedgood finishedgood)
        {
            if (id != finishedgood.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(finishedgood);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FinishedgoodExists(finishedgood.Id))
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
            return View(finishedgood);
        }

        // GET: Finishedgoods/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var finishedgood = await _context.Finishedgoods
                .FirstOrDefaultAsync(m => m.Id == id);
            if (finishedgood == null)
            {
                return NotFound();
            }

            return View(finishedgood);
        }

        // POST: Finishedgoods/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var finishedgood = await _context.Finishedgoods.FindAsync(id);
            if (finishedgood != null)
            {
                _context.Finishedgoods.Remove(finishedgood);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FinishedgoodExists(int id)
        {
            return _context.Finishedgoods.Any(e => e.Id == id);
        }
    }
}
