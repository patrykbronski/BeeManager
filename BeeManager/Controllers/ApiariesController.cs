using BeeManager.Authorization;
using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using BeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Controllers;

[Authorize]
[Route("api/apiaries")]
public class ApiariesController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IApiaryAccessService _accessService;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApiariesController(
        ApplicationDbContext dbContext,
        IApiaryAccessService accessService,
        UserManager<ApplicationUser> userManager)
    {
        _dbContext = dbContext;
        _accessService = accessService;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ApiarySummaryDto>>> GetApiaries()
    {
        var query = _dbContext.Pasieki
            .Include(apiary => apiary.Owner)
            .Include(apiary => apiary.Memberships)
            .AsQueryable();

        if (!CurrentRoles.Contains(SystemRoles.Admin))
        {
            query = query.Where(apiary =>
                apiary.OwnerId == CurrentUserId ||
                apiary.Memberships.Any(membership =>
                    membership.UserId == CurrentUserId && membership.Status == MembershipStatus.Approved));
        }

        var apiaries = await query.OrderBy(apiary => apiary.Nazwa).ToListAsync();

        return Ok(apiaries.Select(apiary =>
        {
            var membership = apiary.Memberships.FirstOrDefault(item =>
                item.UserId == CurrentUserId && item.Status == MembershipStatus.Approved);
            var canManage = CurrentRoles.Contains(SystemRoles.Admin) ||
                            (CurrentRoles.Contains(SystemRoles.Owner) && apiary.OwnerId == CurrentUserId);

            return new ApiarySummaryDto
            {
                Id = apiary.Id,
                Nazwa = apiary.Nazwa,
                Lokalizacja = apiary.Lokalizacja,
                Opis = apiary.Opis,
                OwnerId = apiary.OwnerId,
                OwnerName = apiary.Owner?.FullName ?? string.Empty,
                UtworzonoAtUtc = apiary.UtworzonoAtUtc,
                CanManage = canManage,
                IsOwner = apiary.OwnerId == CurrentUserId,
                MembershipRole = membership?.MembershipRole
            };
        }));
    }

    [Authorize(Roles = $"{SystemRoles.Worker},{SystemRoles.Inspector}")]
    [HttpGet("discover")]
    public async Task<ActionResult<IReadOnlyCollection<ApiarySummaryDto>>> DiscoverApiaries([FromQuery] string? query)
    {
        var normalizedQuery = query?.Trim().ToLowerInvariant();
        var items = await _dbContext.Pasieki
            .Include(apiary => apiary.Owner)
            .Include(apiary => apiary.Memberships)
            .Where(apiary => string.IsNullOrWhiteSpace(normalizedQuery) ||
                             apiary.Nazwa.ToLower().Contains(normalizedQuery) ||
                             (apiary.Lokalizacja != null && apiary.Lokalizacja.ToLower().Contains(normalizedQuery)))
            .OrderBy(apiary => apiary.Nazwa)
            .ToListAsync();

        return Ok(items.Select(apiary =>
        {
            var membershipRole = CurrentRoles.Contains(SystemRoles.Inspector)
                ? MembershipRole.Inspector
                : MembershipRole.Worker;
            var membership = apiary.Memberships.FirstOrDefault(item =>
                item.UserId == CurrentUserId && item.MembershipRole == membershipRole);
            return new ApiarySummaryDto
            {
                Id = apiary.Id,
                Nazwa = apiary.Nazwa,
                Lokalizacja = apiary.Lokalizacja,
                Opis = apiary.Opis,
                OwnerId = apiary.OwnerId,
                OwnerName = apiary.Owner?.FullName ?? string.Empty,
                UtworzonoAtUtc = apiary.UtworzonoAtUtc,
                CanManage = false,
                IsOwner = false,
                MembershipRole = membership?.MembershipRole,
                MembershipStatus = membership?.Status,
                JoinRequestId = membership?.Id
            };
        }));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiarySummaryDto>> GetApiary(int id)
    {
        var canAccess = await _accessService.CanAccessApiaryAsync(CurrentUserId, CurrentRoles, id);
        if (!canAccess)
        {
            return Forbid();
        }

        var apiary = await _dbContext.Pasieki
            .Include(item => item.Owner)
            .Include(item => item.Memberships)
            .FirstOrDefaultAsync(item => item.Id == id);

        if (apiary is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono pasieki." });
        }

        var membership = apiary.Memberships.FirstOrDefault(item =>
            item.UserId == CurrentUserId && item.Status == MembershipStatus.Approved);
        return Ok(new ApiarySummaryDto
        {
            Id = apiary.Id,
            Nazwa = apiary.Nazwa,
            Lokalizacja = apiary.Lokalizacja,
            Opis = apiary.Opis,
            OwnerId = apiary.OwnerId,
            OwnerName = apiary.Owner?.FullName ?? string.Empty,
            UtworzonoAtUtc = apiary.UtworzonoAtUtc,
            CanManage = await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id),
            IsOwner = apiary.OwnerId == CurrentUserId,
            MembershipRole = membership?.MembershipRole
        });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPost]
    public async Task<ActionResult<ApiarySummaryDto>> CreateApiary(CreateApiaryRequest request)
    {
        var apiary = new Pasieka
        {
            Nazwa = request.Nazwa.Trim(),
            Lokalizacja = request.Lokalizacja?.Trim(),
            Opis = request.Opis?.Trim(),
            OwnerId = CurrentUserId
        };

        _dbContext.Pasieki.Add(apiary);
        await _dbContext.SaveChangesAsync();

        var owner = await _dbContext.Users.FirstAsync(user => user.Id == CurrentUserId);
        return CreatedAtAction(nameof(GetApiary), new { id = apiary.Id }, new ApiarySummaryDto
        {
            Id = apiary.Id,
            Nazwa = apiary.Nazwa,
            Lokalizacja = apiary.Lokalizacja,
            Opis = apiary.Opis,
            OwnerId = CurrentUserId,
            OwnerName = owner.FullName,
            UtworzonoAtUtc = apiary.UtworzonoAtUtc,
            CanManage = true,
            IsOwner = true
        });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse>> UpdateApiary(int id, UpdateApiaryRequest request)
    {
        var apiary = await _dbContext.Pasieki.FirstOrDefaultAsync(item => item.Id == id);
        if (apiary is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono pasieki." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        apiary.Nazwa = request.Nazwa.Trim();
        apiary.Lokalizacja = request.Lokalizacja?.Trim();
        apiary.Opis = request.Opis?.Trim();

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Pasieka została zaktualizowana." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse>> DeleteApiary(int id)
    {
        var apiary = await _dbContext.Pasieki.FirstOrDefaultAsync(item => item.Id == id);
        if (apiary is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono pasieki." });
        }

        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        _dbContext.Pasieki.Remove(apiary);
        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Pasieka została usunięta." });
    }

    [Authorize(Roles = $"{SystemRoles.Worker},{SystemRoles.Inspector}")]
    [HttpPost("{id:int}/join-requests")]
    public async Task<ActionResult<ApiResponse>> RequestJoin(int id, CreateJoinRequest request)
    {
        var apiary = await _dbContext.Pasieki.FirstOrDefaultAsync(item => item.Id == id);
        if (apiary is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono pasieki." });
        }

        var membershipRole = CurrentRoles.Contains(SystemRoles.Inspector)
            ? MembershipRole.Inspector
            : MembershipRole.Worker;

        var existing = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.PasiekaId == id && item.UserId == CurrentUserId && item.MembershipRole == membershipRole);

        if (existing is not null)
        {
            existing.Status = MembershipStatus.Pending;
            existing.RequestMessage = request.Message?.Trim();
            existing.RequestedAtUtc = DateTime.UtcNow;
            existing.DecisionNote = null;
            existing.ReviewedAtUtc = null;
            existing.ReviewedByUserId = null;
        }
        else
        {
            _dbContext.ApiaryMemberships.Add(new ApiaryMembership
            {
                PasiekaId = id,
                UserId = CurrentUserId,
                MembershipRole = membershipRole,
                Status = MembershipStatus.Pending,
                RequestMessage = request.Message?.Trim()
            });
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Prośba o dołączenie została wysłana." });
    }

    [Authorize(Roles = $"{SystemRoles.Worker},{SystemRoles.Inspector}")]
    [HttpDelete("{id:int}/join-requests")]
    public async Task<ActionResult<ApiResponse>> CancelJoinRequest(int id)
    {
        var membershipRole = CurrentRoles.Contains(SystemRoles.Inspector)
            ? MembershipRole.Inspector
            : MembershipRole.Worker;

        var membership = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.PasiekaId == id &&
            item.UserId == CurrentUserId &&
            item.MembershipRole == membershipRole);

        if (membership is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono prośby o dołączenie." });
        }

        if (membership.Status != MembershipStatus.Pending)
        {
            return BadRequest(new ApiResponse { Message = "Można anulować tylko oczekującą prośbę." });
        }

        _dbContext.ApiaryMemberships.Remove(membership);
        await _dbContext.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Prośba o dołączenie została anulowana." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpGet("{id:int}/memberships")]
    public async Task<ActionResult<IReadOnlyCollection<MembershipDto>>> GetMemberships(int id, [FromQuery] MembershipStatus? status)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        var query = _dbContext.ApiaryMemberships
            .Include(membership => membership.Pasieka)
            .Include(membership => membership.User)
            .Where(membership => membership.PasiekaId == id);

        if (status.HasValue)
        {
            query = query.Where(membership => membership.Status == status.Value);
        }

        var memberships = await query
            .OrderBy(membership => membership.User!.FullName)
            .ToListAsync();

        return Ok(memberships.Select(membership => new MembershipDto
        {
            Id = membership.Id,
            PasiekaId = membership.PasiekaId,
            PasiekaNazwa = membership.Pasieka?.Nazwa ?? string.Empty,
            UserId = membership.UserId,
            UserName = membership.User?.FullName ?? string.Empty,
            Email = membership.User?.Email ?? string.Empty,
            MembershipRole = membership.MembershipRole,
            Status = membership.Status,
            RequestMessage = membership.RequestMessage,
            DecisionNote = membership.DecisionNote,
            RequestedAtUtc = membership.RequestedAtUtc
        }));
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPost("{id:int}/memberships/direct")]
    public async Task<ActionResult<ApiResponse>> AddMembershipDirectly(int id, DirectMembershipRequest request)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Email == normalizedEmail);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika o podanym adresie e-mail." });
        }

        if (user.AccountStatus != AccountStatus.Approved)
        {
            return BadRequest(new ApiResponse { Message = "Użytkownik musi mieć zatwierdzone konto." });
        }

        var expectedRole = request.MembershipRole == MembershipRole.Inspector ? SystemRoles.Inspector : SystemRoles.Worker;
        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(expectedRole))
        {
            return BadRequest(new ApiResponse { Message = "Wybrany użytkownik nie ma odpowiedniej roli." });
        }

        var existing = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.PasiekaId == id && item.UserId == user.Id && item.MembershipRole == request.MembershipRole);

        if (existing is null)
        {
            _dbContext.ApiaryMemberships.Add(new ApiaryMembership
            {
                PasiekaId = id,
                UserId = user.Id,
                MembershipRole = request.MembershipRole,
                Status = MembershipStatus.Approved,
                ReviewedAtUtc = DateTime.UtcNow,
                ReviewedByUserId = CurrentUserId,
                DecisionNote = "Dodano bezpośrednio przez właściciela lub administratora."
            });
        }
        else
        {
            existing.Status = MembershipStatus.Approved;
            existing.ReviewedAtUtc = DateTime.UtcNow;
            existing.ReviewedByUserId = CurrentUserId;
            existing.DecisionNote = "Dodano bezpośrednio przez właściciela lub administratora.";
        }

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Członek został dodany do pasieki." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPost("{id:int}/memberships/{membershipId:int}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveMembership(int id, int membershipId, ReviewMembershipRequest request)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        var membership = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.Id == membershipId && item.PasiekaId == id);
        if (membership is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono zgłoszenia." });
        }

        membership.Status = MembershipStatus.Approved;
        membership.DecisionNote = request.DecisionNote?.Trim();
        membership.ReviewedByUserId = CurrentUserId;
        membership.ReviewedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Prośba została zatwierdzona." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpPost("{id:int}/memberships/{membershipId:int}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectMembership(int id, int membershipId, ReviewMembershipRequest request)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        var membership = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.Id == membershipId && item.PasiekaId == id);
        if (membership is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono zgłoszenia." });
        }

        membership.Status = MembershipStatus.Rejected;
        membership.DecisionNote = request.DecisionNote?.Trim();
        membership.ReviewedByUserId = CurrentUserId;
        membership.ReviewedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        return Ok(new ApiResponse { Message = "Prośba została odrzucona." });
    }

    [Authorize(Roles = $"{SystemRoles.Admin},{SystemRoles.Owner}")]
    [HttpDelete("{id:int}/memberships/{membershipId:int}")]
    public async Task<ActionResult<ApiResponse>> RemoveMembership(int id, int membershipId)
    {
        if (!await _accessService.CanManageApiaryAsync(CurrentUserId, CurrentRoles, id))
        {
            return Forbid();
        }

        var membership = await _dbContext.ApiaryMemberships.FirstOrDefaultAsync(item =>
            item.Id == membershipId && item.PasiekaId == id);
        if (membership is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono członka pasieki." });
        }

        _dbContext.ApiaryMemberships.Remove(membership);
        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Członek został usunięty z pasieki." });
    }
}
