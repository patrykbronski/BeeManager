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
    public class PasiekiController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PasiekiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pasieki
        public async Task<IActionResult> Index()
        {
              return _context.Pasieki != null ? 
                          View(await _context.Pasieki.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Pasieki'  is null.");
        }

        // GET: Pasieki/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Pasieki == null)
            {
                return NotFound();
            }

            var pasieka = await _context.Pasieki
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pasieka == null)
            {
                return NotFound();
            }

            return View(pasieka);
        }

        // GET: Pasieki/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Pasieki/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Nazwa,Lokalizacja,Opis")] Pasieka pasieka)
        {
            if (ModelState.IsValid)
            {
                pasieka.UtworzonoAt = DateTime.Now;
                _context.Add(pasieka);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(pasieka);
        }


        // GET: Pasieki/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Pasieki == null)
            {
                return NotFound();
            }

            var pasieka = await _context.Pasieki.FindAsync(id);
            if (pasieka == null)
            {
                return NotFound();
            }
            return View(pasieka);
        }

        // POST: Pasieki/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nazwa,Lokalizacja,Opis,UtworzonoAt")] Pasieka pasieka)
        {
            if (id != pasieka.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pasieka);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PasiekaExists(pasieka.Id))
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
            return View(pasieka);
        }

        // GET: Pasieki/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Pasieki == null)
            {
                return NotFound();
            }

            var pasieka = await _context.Pasieki
                .FirstOrDefaultAsync(m => m.Id == id);
            if (pasieka == null)
            {
                return NotFound();
            }

            return View(pasieka);
        }

        // POST: Pasieki/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Pasieki == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Pasieki'  is null.");
            }
            var pasieka = await _context.Pasieki.FindAsync(id);
            if (pasieka != null)
            {
                _context.Pasieki.Remove(pasieka);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PasiekaExists(int id)
        {
          return (_context.Pasieki?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
