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
    public class UleController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ule
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Ule.Include(u => u.Pasieka);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Ule/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Ule == null)
            {
                return NotFound();
            }

            var ul = await _context.Ule
                .Include(u => u.Pasieka)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ul == null)
            {
                return NotFound();
            }

            return View(ul);
        }

        // GET: Ule/Create
        public IActionResult Create()
        {
            ViewData["PasiekaId"] = new SelectList(_context.Pasieki, "Id", "Nazwa");
            return View();
        }


        // POST: Ule/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,PasiekaId,NumerUla,TypUla,Status,DataZalozenia,Uwagi")] Ul ul)
        {
            if (ModelState.IsValid)
            {
                bool exists = await _context.Ule.AnyAsync(u =>
                    u.PasiekaId == ul.PasiekaId && u.NumerUla == ul.NumerUla);

                if (exists)
                {
                    ModelState.AddModelError("NumerUla", "Taki numer ula już istnieje w tej pasiece.");
                }
            }

            if (ModelState.IsValid)
            {
                _context.Add(ul);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["PasiekaId"] = new SelectList(_context.Pasieki, "Id", "Nazwa", ul.PasiekaId);
            return View(ul);
        }


        // GET: Ule/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Ule == null)
            {
                return NotFound();
            }

            var ul = await _context.Ule.FindAsync(id);
            if (ul == null)
            {
                return NotFound();
            }
            ViewData["PasiekaId"] = new SelectList(_context.Pasieki, "Id", "Nazwa", ul.PasiekaId);
            return View(ul);
        }

        // POST: Ule/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,PasiekaId,NumerUla,TypUla,Status,DataZalozenia,Uwagi")] Ul ul)
        {
            if (id != ul.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ul);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UlExists(ul.Id))
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
            ViewData["PasiekaId"] = new SelectList(_context.Pasieki, "Id", "Nazwa", ul.PasiekaId);
            return View(ul);
        }

        // GET: Ule/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Ule == null)
            {
                return NotFound();
            }

            var ul = await _context.Ule
                .Include(u => u.Pasieka)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (ul == null)
            {
                return NotFound();
            }

            return View(ul);
        }

        // POST: Ule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Ule == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ule'  is null.");
            }
            var ul = await _context.Ule.FindAsync(id);
            if (ul != null)
            {
                _context.Ule.Remove(ul);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UlExists(int id)
        {
          return (_context.Ule?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
