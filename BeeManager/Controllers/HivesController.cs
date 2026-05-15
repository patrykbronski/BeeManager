using BeeManager.Authorization;
using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using BeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Controllers;

[Authorize]
[Route("api/hives")]
public class HivesController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IApiaryAccessService _accessService;

    public HivesController(ApplicationDbContext dbContext, IApiaryAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<HiveDto>>> GetHives([FromQuery] int? apiaryId)
    {
        var query = _dbContext.Ule
            .Include(hive => hive.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .AsQueryable();

        if (apiaryId.HasValue)
        {
            if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, apiaryId.Value))
            {
                return Forbid();
            }

            query = query.Where(hive => hive.PasiekaId == apiaryId.Value);
        }
        else if (!CurrentRoles.Contains(SystemRoles.Admin))
        {
            query = query.Where(hive =>
                hive.Pasieka!.OwnerId == CurrentUserId ||
                hive.Pasieka.Memberships.Any(membership =>
                    membership.UserId == CurrentUserId && membership.Status == MembershipStatus.Approved));
        }

        var hives = await query.OrderBy(hive => hive.NumerUla).ToListAsync();
        return Ok(await MapHivesAsync(hives));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<HiveDto>> GetHive(int id)
    {
        var hive = await _dbContext.Ule
            .Include(item => item.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (hive is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono ula." });
        }

        if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, hive.PasiekaId))
        {
            return Forbid();
        }

        return Ok((await MapHivesAsync(new[] { hive })).Single());
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPost]
    public async Task<ActionResult<HiveDto>> CreateHive(SaveHiveRequest request)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, request.PasiekaId))
        {
            return Forbid();
        }

        var normalizedNumber = request.NumerUla.Trim();
        var exists = await _dbContext.Ule.AnyAsync(hive =>
            hive.PasiekaId == request.PasiekaId && hive.NumerUla == normalizedNumber);
        if (exists)
        {
            return Conflict(new ApiResponse { Message = "Taki numer ula już istnieje w tej pasiece." });
        }

        var hive = new Ul
        {
            PasiekaId = request.PasiekaId,
            NumerUla = normalizedNumber,
            TypUla = request.TypUla,
            Status = request.Status,
            DataZalozenia = request.DataZalozenia,
            Uwagi = request.Uwagi?.Trim()
        };

        _dbContext.Ule.Add(hive);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Ule
            .Include(item => item.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .FirstAsync(item => item.Id == hive.Id);
        return CreatedAtAction(nameof(GetHive), new { id = hive.Id }, (await MapHivesAsync(new[] { created })).Single());
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateHive(int id, SaveHiveRequest request)
    {
        var hive = await _dbContext.Ule.FirstOrDefaultAsync(item => item.Id == id);
        if (hive is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono ula." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, hive.PasiekaId))
        {
            return Forbid();
        }

        var normalizedNumber = request.NumerUla.Trim();
        var exists = await _dbContext.Ule.AnyAsync(item =>
            item.PasiekaId == request.PasiekaId &&
            item.NumerUla == normalizedNumber &&
            item.Id != id);
        if (exists)
        {
            return Conflict(new ApiResponse { Message = "Taki numer ula już istnieje w tej pasiece." });
        }

        hive.PasiekaId = request.PasiekaId;
        hive.NumerUla = normalizedNumber;
        hive.TypUla = request.TypUla;
        hive.Status = request.Status;
        hive.DataZalozenia = request.DataZalozenia;
        hive.Uwagi = request.Uwagi?.Trim();

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Ul został zaktualizowany." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner},{SystemRoles.Worker}")]
    [HttpPatch("{id:int}/notes")]
    public async Task<ActionResult<ApiResponse>> UpdateHiveNotes(int id, UpdateHiveNotesRequest request)
    {
        var hive = await _dbContext.Ule.FirstOrDefaultAsync(item => item.Id == id);
        if (hive is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono ula." });
        }

        if (!await _accessService.CanEditHiveNotesAsync(CurrentUserId, CurrentRoles, hive.PasiekaId))
        {
            return Forbid();
        }

        hive.Uwagi = request.Uwagi.Trim();
        await _dbContext.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Notatki ula zostały zaktualizowane." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteHive(int id)
    {
        var hive = await _dbContext.Ule.FirstOrDefaultAsync(item => item.Id == id);
        if (hive is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono ula." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, hive.PasiekaId))
        {
            return Forbid();
        }

        _dbContext.Ule.Remove(hive);
        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Ul został usunięty." });
    }

    private async Task<IReadOnlyCollection<HiveDto>> MapHivesAsync(IEnumerable<Ul> hives)
    {
        var result = new List<HiveDto>();
        foreach (var hive in hives)
        {
            var canManage = await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, hive.PasiekaId);
            var canEditNotes = await _accessService.CanEditHiveNotesAsync(CurrentUserId, CurrentRoles, hive.PasiekaId);
            result.Add(new HiveDto
            {
                Id = hive.Id,
                PasiekaId = hive.PasiekaId,
                PasiekaNazwa = hive.Pasieka?.Nazwa ?? string.Empty,
                NumerUla = hive.NumerUla,
                TypUla = hive.TypUla,
                Status = hive.Status,
                DataZalozenia = hive.DataZalozenia,
                Uwagi = hive.Uwagi,
                CanManage = canManage,
                CanEditNotes = canEditNotes
            });
        }

        return result;
    }
}
