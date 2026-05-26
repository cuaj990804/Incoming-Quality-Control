using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Incoming.Data;
using Incoming.Models;
using System.Text.RegularExpressions;

namespace Incoming.Controllers
{
    public class InspectionProgramsController : Controller
    {
        private readonly IncomingContext _context;

        public InspectionProgramsController(IncomingContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult InsertInspectionProgram([FromBody] string programName)
        {
            if (string.IsNullOrWhiteSpace(programName))
            {
                return BadRequest("Program name is required.");
            }

            // Verificar si ya existe un registro con ese nombre
            bool exists = _context.InspectionPrograms.Any(p => p.ProgramName == programName);
            if (exists)
            {
                return Conflict($"A program with the name '{programName}' already exists.");
            }

            // Crear nuevo programa
            var newProgram = new InspectionProgram
            {
                ProgramName = programName
            };

            _context.InspectionPrograms.Add(newProgram);
            _context.SaveChanges();

            return Ok($"Program '{programName}' inserted successfully.");
        }
        private void GetProgram()
        {
            ViewBag.ProgramList = _context.InspectionPrograms.Select(p => p.ProgramName).ToList();
            ViewBag.ProgramListIncoming = _context.InspectionPrograms
                .Select(p => p.ProgramName)
                .ToList();

        }
        // GET: InspectionPrograms
        public async Task<IActionResult> Index()
        {
            return View(await _context.InspectionPrograms.ToListAsync());
        }

        // GET: InspectionPrograms/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspectionProgram = await _context.InspectionPrograms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspectionProgram == null)
            {
                return NotFound();
            }

            return View(inspectionProgram);
        }

        // GET: InspectionPrograms/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: InspectionPrograms/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProgramName,Partnumber")] InspectionProgram inspectionProgram)
        {
            if (ModelState.IsValid)
            {
                _context.Add(inspectionProgram);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(inspectionProgram);
        }

        // GET: InspectionPrograms/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspectionProgram = await _context.InspectionPrograms.FindAsync(id);
            if (inspectionProgram == null)
            {
                return NotFound();
            }
            return View(inspectionProgram);
        }

        // POST: InspectionPrograms/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,ProgramName,Partnumber")] InspectionProgram inspectionProgram)
        {
            if (id != inspectionProgram.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inspectionProgram);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InspectionProgramExists(inspectionProgram.Id))
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
            return View(inspectionProgram);
        }

        // GET: InspectionPrograms/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inspectionProgram = await _context.InspectionPrograms
                .FirstOrDefaultAsync(m => m.Id == id);
            if (inspectionProgram == null)
            {
                return NotFound();
            }

            return View(inspectionProgram);
        }

        // POST: InspectionPrograms/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inspectionProgram = await _context.InspectionPrograms.FindAsync(id);
            if (inspectionProgram != null)
            {
                _context.InspectionPrograms.Remove(inspectionProgram);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InspectionProgramExists(int id)
        {
            return _context.InspectionPrograms.Any(e => e.Id == id);
        }
    }
}
