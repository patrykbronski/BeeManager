using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BeeManager.Data;
using BeeManager.Models;

namespace BeeManager.Controllers
{
    public class MiodobraniaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MiodobraniaController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Miodobrania
        public async Task<IActionResult> Index(int? ulId)
        {
            var query = _context.Miodobrania
                .Include(m => m.Ul)
                .ThenInclude(u => u.Pasieka)
                .AsQueryable();

            if (ulId.HasValue)
            {
                query = query.Where(m => m.UlId == ulId);
                ViewBag.UlId = ulId;
            }

            return View(await query.ToListAsync());
        }


        // GET: Miodobrania/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Miodobrania == null)
            {
                return NotFound();
            }

            var miodobranie = await _context.Miodobrania
                .Include(m => m.Ul)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (miodobranie == null)
            {
                return NotFound();
            }

            return View(miodobranie);
        }

        // GET: Miodobrania/Create
        public IActionResult Create()
        {
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla");
            return View();
        }

        // POST: Miodobrania/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UlId,DataMiodobrania,TypMiodu,IloscKg,Notatki")] Miodobranie miodobranie)
        {
            if (ModelState.IsValid)
            {
                _context.Add(miodobranie);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", miodobranie.UlId);
            return View(miodobranie);
        }

        // GET: Miodobrania/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Miodobrania == null)
            {
                return NotFound();
            }

            var miodobranie = await _context.Miodobrania.FindAsync(id);
            if (miodobranie == null)
            {
                return NotFound();
            }
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", miodobranie.UlId);
            return View(miodobranie);
        }

        // POST: Miodobrania/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UlId,DataMiodobrania,TypMiodu,IloscKg,Notatki")] Miodobranie miodobranie)
        {
            if (id != miodobranie.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(miodobranie);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MiodobranieExists(miodobranie.Id))
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
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", miodobranie.UlId);
            return View(miodobranie);
        }

        // GET: Miodobrania/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Miodobrania == null)
            {
                return NotFound();
            }

            var miodobranie = await _context.Miodobrania
                .Include(m => m.Ul)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (miodobranie == null)
            {
                return NotFound();
            }

            return View(miodobranie);
        }

        // POST: Miodobrania/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Miodobrania == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Miodobrania'  is null.");
            }
            var miodobranie = await _context.Miodobrania.FindAsync(id);
            if (miodobranie != null)
            {
                _context.Miodobrania.Remove(miodobranie);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool MiodobranieExists(int id)
        {
          return (_context.Miodobrania?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
