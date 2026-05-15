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
[Route("api/harvests")]
public class HarvestsController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IApiaryAccessService _accessService;

    public HarvestsController(ApplicationDbContext dbContext, IApiaryAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<HarvestDto>>> GetHarvests([FromQuery] int? apiaryId, [FromQuery] int? hiveId)
    {
        var query = _dbContext.Miodobrania
            .Include(harvest => harvest.Ul)
            .ThenInclude(hive => hive!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .Include(harvest => harvest.CreatedByUser)
            .AsQueryable();

        if (hiveId.HasValue)
        {
            query = query.Where(harvest => harvest.UlId == hiveId.Value);
        }

        if (apiaryId.HasValue)
        {
            if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, apiaryId.Value))
            {
                return Forbid();
            }

            query = query.Where(harvest => harvest.Ul!.PasiekaId == apiaryId.Value);
        }
        else if (!CurrentRoles.Contains(SystemRoles.Admin))
        {
            query = query.Where(harvest =>
                harvest.Ul!.Pasieka!.OwnerId == CurrentUserId ||
                harvest.Ul.Pasieka.Memberships.Any(membership =>
                    membership.UserId == CurrentUserId && membership.Status == MembershipStatus.Approved));
        }

        var items = await query.OrderByDescending(harvest => harvest.DataMiodobrania).ToListAsync();
        return Ok(items.Select(MapHarvest));
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner},{SystemRoles.Worker}")]
    [HttpPost]
    public async Task<ActionResult<HarvestDto>> CreateHarvest(SaveHarvestRequest request)
    {
        var hive = await _dbContext.Ule.Include(item => item.Pasieka).FirstOrDefaultAsync(item => item.Id == request.UlId);
        if (hive is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono ula." });
        }

        if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, hive.PasiekaId))
        {
            return Forbid();
        }

        var harvest = new Miodobranie
        {
            UlId = request.UlId,
            DataMiodobrania = request.DataMiodobrania,
            TypMiodu = request.TypMiodu,
            IloscKg = request.IloscKg,
            Notatki = request.Notatki?.Trim(),
            CreatedByUserId = CurrentUserId
        };

        _dbContext.Miodobrania.Add(harvest);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Miodobrania
            .Include(item => item.Ul)
            .ThenInclude(item => item!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .Include(item => item.CreatedByUser)
            .FirstAsync(item => item.Id == harvest.Id);

        return CreatedAtAction(nameof(GetHarvests), new { id = harvest.Id }, MapHarvest(created));
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner},{SystemRoles.Worker}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateHarvest(int id, SaveHarvestRequest request)
    {
        var harvest = await _dbContext.Miodobrania
            .Include(item => item.Ul)
            .ThenInclude(item => item!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (harvest is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono miodobrania." });
        }

        if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, harvest.Ul!.PasiekaId))
        {
            return Forbid();
        }

        harvest.UlId = request.UlId;
        harvest.DataMiodobrania = request.DataMiodobrania;
        harvest.TypMiodu = request.TypMiodu;
        harvest.IloscKg = request.IloscKg;
        harvest.Notatki = request.Notatki?.Trim();
        harvest.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Miodobranie zostało zaktualizowane." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteHarvest(int id)
    {
        var harvest = await _dbContext.Miodobrania
            .Include(item => item.Ul)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (harvest is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono miodobrania." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, harvest.Ul!.PasiekaId))
        {
            return Forbid();
        }

        _dbContext.Miodobrania.Remove(harvest);
        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Miodobranie zostało usunięte." });
    }

    private HarvestDto MapHarvest(Miodobranie harvest)
    {
        var canManageApiary = CurrentRoles.Contains(SystemRoles.Admin) ||
                              (CurrentRoles.Contains(SystemRoles.Owner) && harvest.Ul?.Pasieka?.OwnerId == CurrentUserId);
        var canEdit = canManageApiary || CurrentRoles.Contains(SystemRoles.Worker);

        return new HarvestDto
        {
            Id = harvest.Id,
            UlId = harvest.UlId,
            PasiekaId = harvest.Ul?.PasiekaId ?? 0,
            NumerUla = harvest.Ul?.NumerUla ?? string.Empty,
            PasiekaNazwa = harvest.Ul?.Pasieka?.Nazwa ?? string.Empty,
            DataMiodobrania = harvest.DataMiodobrania,
            TypMiodu = harvest.TypMiodu,
            IloscKg = harvest.IloscKg,
            Notatki = harvest.Notatki,
            CreatedBy = harvest.CreatedByUser?.FullName ?? string.Empty,
            CanEdit = canEdit,
            CanDelete = canManageApiary
        };
    }
}
