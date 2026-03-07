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
    public class PrzegladyController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrzegladyController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Przeglady
        public async Task<IActionResult> Index(int? ulId)
        {
            var query = _context.Przeglady
                .Include(p => p.Ul)
                .ThenInclude(u => u.Pasieka)
                .AsQueryable();


            if (ulId.HasValue)
            {
                query = query.Where(p => p.UlId == ulId);
                ViewBag.UlId = ulId;
            }

            return View(await query.ToListAsync());
        }


        // GET: Przeglady/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Przeglady == null)
            {
                return NotFound();
            }

            var przeglad = await _context.Przeglady
                .Include(p => p.Ul)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (przeglad == null)
            {
                return NotFound();
            }

            return View(przeglad);
        }

        // GET: Przeglady/Create
        public IActionResult Create()
        {
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla");
            return View();
        }


        // POST: Przeglady/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UlId,DataPrzegladu,StanRodziny,ObecnoscMatki,IloscCzerwiu,Notatki")] Przeglad przeglad)
        {
            if (ModelState.IsValid)
            {
                _context.Add(przeglad);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", przeglad.UlId);
            return View(przeglad);
        }

        // GET: Przeglady/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Przeglady == null)
            {
                return NotFound();
            }

            var przeglad = await _context.Przeglady.FindAsync(id);
            if (przeglad == null)
            {
                return NotFound();
            }
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", przeglad.UlId);

            return View(przeglad);
        }

        // POST: Przeglady/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UlId,DataPrzegladu,StanRodziny,ObecnoscMatki,IloscCzerwiu,Notatki")] Przeglad przeglad)
        {
            if (id != przeglad.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(przeglad);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PrzegladExists(przeglad.Id))
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
            ViewData["UlId"] = new SelectList(_context.Ule, "Id", "NumerUla", przeglad.UlId);
            return View(przeglad);
        }

        // GET: Przeglady/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Przeglady == null)
            {
                return NotFound();
            }

            var przeglad = await _context.Przeglady
                .Include(p => p.Ul)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (przeglad == null)
            {
                return NotFound();
            }

            return View(przeglad);
        }

        // POST: Przeglady/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Przeglady == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Przeglady'  is null.");
            }
            var przeglad = await _context.Przeglady.FindAsync(id);
            if (przeglad != null)
            {
                _context.Przeglady.Remove(przeglad);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PrzegladExists(int id)
        {
          return (_context.Przeglady?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
