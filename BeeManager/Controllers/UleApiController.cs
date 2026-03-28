using BeeManager.Data;
using BeeManager.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UleApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UleApiController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/UleApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Ul>>> GetUle()
        {
            var ule = await _context.Ule
                .Include(u => u.Pasieka)
                .ToListAsync();

            return Ok(ule);
        }

        // GET: api/UleApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Ul>> GetUl(int id)
        {
            var ul = await _context.Ule
                .Include(u => u.Pasieka)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (ul == null)
            {
                return NotFound();
            }

            return Ok(ul);
        }

        // POST: api/UleApi
        [HttpPost]
        public async Task<ActionResult<Ul>> CreateUl(Ul ul)
        {
            bool exists = await _context.Ule.AnyAsync(u =>
                u.PasiekaId == ul.PasiekaId && u.NumerUla == ul.NumerUla);

            if (exists)
            {
                return BadRequest(new { message = "Taki numer ula ju¿ istnieje w tej pasiece." });
            }

            _context.Ule.Add(ul);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUl), new { id = ul.Id }, ul);
        }

        // PUT: api/UleApi/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUl(int id, Ul ul)
        {
            if (id != ul.Id)
            {
                return BadRequest(new { message = "Id w adresie nie zgadza siê z Id obiektu." });
            }

            bool exists = await _context.Ule.AnyAsync(u =>
                u.PasiekaId == ul.PasiekaId &&
                u.NumerUla == ul.NumerUla &&
                u.Id != ul.Id);

            if (exists)
            {
                return BadRequest(new { message = "Taki numer ula ju¿ istnieje w tej pasiece." });
            }

            _context.Entry(ul).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Ule.AnyAsync(e => e.Id == id))
                {
                    return NotFound();
                }

                throw;
            }

            return NoContent();
        }

        // DELETE: api/UleApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUl(int id)
        {
            var ul = await _context.Ule.FindAsync(id);

            if (ul == null)
            {
                return NotFound();
            }

            _context.Ule.Remove(ul);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}