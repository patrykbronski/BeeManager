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
[Route("api/inspections")]
public class InspectionsController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IApiaryAccessService _accessService;

    public InspectionsController(ApplicationDbContext dbContext, IApiaryAccessService accessService)
    {
        _dbContext = dbContext;
        _accessService = accessService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InspectionDto>>> GetInspections([FromQuery] int? apiaryId, [FromQuery] int? hiveId)
    {
        var query = _dbContext.Przeglady
            .Include(inspection => inspection.Ul)
            .ThenInclude(hive => hive!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .Include(inspection => inspection.CreatedByUser)
            .AsQueryable();

        if (hiveId.HasValue)
        {
            query = query.Where(inspection => inspection.UlId == hiveId.Value);
        }

        if (apiaryId.HasValue)
        {
            if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, apiaryId.Value))
            {
                return Forbid();
            }

            query = query.Where(inspection => inspection.Ul!.PasiekaId == apiaryId.Value);
        }
        else if (!CurrentRoles.Contains(SystemRoles.Admin))
        {
            query = query.Where(inspection =>
                inspection.Ul!.Pasieka!.OwnerId == CurrentUserId ||
                inspection.Ul.Pasieka.Memberships.Any(membership =>
                    membership.UserId == CurrentUserId && membership.Status == MembershipStatus.Approved));
        }

        var items = await query.OrderByDescending(inspection => inspection.DataPrzegladu).ToListAsync();
        return Ok(items.Select(MapInspection));
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner},{SystemRoles.Worker}")]
    [HttpPost]
    public async Task<ActionResult<InspectionDto>> CreateInspection(SaveInspectionRequest request)
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

        var inspection = new Przeglad
        {
            UlId = request.UlId,
            DataPrzegladu = request.DataPrzegladu,
            StanRodziny = request.StanRodziny,
            ObecnoscMatki = request.ObecnoscMatki,
            IloscCzerwiu = request.IloscCzerwiu,
            Notatki = request.Notatki?.Trim(),
            CreatedByUserId = CurrentUserId
        };

        _dbContext.Przeglady.Add(inspection);
        await _dbContext.SaveChangesAsync();

        var created = await _dbContext.Przeglady
            .Include(item => item.Ul)
            .ThenInclude(item => item!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .Include(item => item.CreatedByUser)
            .FirstAsync(item => item.Id == inspection.Id);

        return CreatedAtAction(nameof(GetInspections), new { id = inspection.Id }, MapInspection(created));
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner},{SystemRoles.Worker}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateInspection(int id, SaveInspectionRequest request)
    {
        var inspection = await _dbContext.Przeglady
            .Include(item => item.Ul)
            .ThenInclude(item => item!.Pasieka)
            .ThenInclude(apiary => apiary!.Memberships)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (inspection is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono przeglądu." });
        }

        if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, inspection.Ul!.PasiekaId))
        {
            return Forbid();
        }

        inspection.UlId = request.UlId;
        inspection.DataPrzegladu = request.DataPrzegladu;
        inspection.StanRodziny = request.StanRodziny;
        inspection.ObecnoscMatki = request.ObecnoscMatki;
        inspection.IloscCzerwiu = request.IloscCzerwiu;
        inspection.Notatki = request.Notatki?.Trim();
        inspection.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Przegląd został zaktualizowany." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteInspection(int id)
    {
        var inspection = await _dbContext.Przeglady
            .Include(item => item.Ul)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (inspection is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono przeglądu." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, inspection.Ul!.PasiekaId))
        {
            return Forbid();
        }

        _dbContext.Przeglady.Remove(inspection);
        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Przegląd został usunięty." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Inspector}")]
    [HttpPost("{id:int}/specialist-note")]
    public async Task<ActionResult<ApiResponse>> AddSpecialistNote(int id, SpecialistNoteRequest request)
    {
        var inspection = await _dbContext.Przeglady
            .Include(item => item.Ul)
            .ThenInclude(item => item!.Pasieka)
            .FirstOrDefaultAsync(item => item.Id == id);
        if (inspection is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono przeglądu." });
        }

        if (!await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, inspection.Ul!.PasiekaId))
        {
            return Forbid();
        }

        var entry = $"[Uwagi specjalistyczne {DateTime.UtcNow:yyyy-MM-dd HH:mm}]: {request.Note.Trim()}";
        inspection.Notatki = string.IsNullOrWhiteSpace(inspection.Notatki)
            ? entry
            : $"{inspection.Notatki}{Environment.NewLine}{entry}";
        inspection.UpdatedAtUtc = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Dodano uwagę specjalistyczną." });
    }

    private InspectionDto MapInspection(Przeglad inspection)
    {
        var canManageApiary = CurrentRoles.Contains(SystemRoles.Admin) ||
                              (CurrentRoles.Contains(SystemRoles.Owner) && inspection.Ul?.Pasieka?.OwnerId == CurrentUserId);
        var canEdit = canManageApiary || CurrentRoles.Contains(SystemRoles.Worker);

        return new InspectionDto
        {
            Id = inspection.Id,
            UlId = inspection.UlId,
            PasiekaId = inspection.Ul?.PasiekaId ?? 0,
            NumerUla = inspection.Ul?.NumerUla ?? string.Empty,
            PasiekaNazwa = inspection.Ul?.Pasieka?.Nazwa ?? string.Empty,
            DataPrzegladu = inspection.DataPrzegladu,
            StanRodziny = inspection.StanRodziny,
            ObecnoscMatki = inspection.ObecnoscMatki,
            IloscCzerwiu = inspection.IloscCzerwiu,
            Notatki = inspection.Notatki,
            CreatedBy = inspection.CreatedByUser?.FullName ?? string.Empty,
            CanEdit = canEdit,
            CanDelete = canManageApiary,
            CanAddSpecialistNote = CurrentRoles.Contains(SystemRoles.Admin) || CurrentRoles.Contains(SystemRoles.Inspector)
        };
    }
}
