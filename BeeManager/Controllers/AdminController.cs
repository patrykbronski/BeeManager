using BeeManager.Authorization;
using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Controllers;

[Authorize(Roles = SystemRoles.Admin)]
[Route("api/admin")]
public class AdminController : ApiControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminController> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("registrations")]
    public async Task<ActionResult<IReadOnlyCollection<RegistrationReviewDto>>> GetRegistrations([FromQuery] AccountStatus? status)
    {
        var query = _dbContext.Users.AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(user => user.AccountStatus == status.Value);
        }

        var registrations = await query
            .OrderByDescending(user => user.CreatedAtUtc)
            .Select(user => new RegistrationReviewDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                RequestedRole = user.RequestedRole,
                AccountStatus = user.AccountStatus,
                RegistrationNote = user.RegistrationNote,
                ReviewNote = user.ReviewNote,
                CreatedAtUtc = user.CreatedAtUtc,
                RejectionReason = user.RejectionReason
            })
            .ToListAsync();

        return Ok(registrations);
    }

    [HttpPost("registrations/{userId}/approve")]
    public async Task<ActionResult<ApiResponse>> ApproveRegistration(string userId, ApproveRegistrationRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika." });
        }

        user.RequestedRole = request.Role;
        user.AccountStatus = AccountStatus.Approved;
        user.ApprovedAtUtc = DateTime.UtcNow;
        user.RejectedAtUtc = null;
        user.RejectionReason = null;
        user.ReviewNote = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
        user.ReviewedByUserId = CurrentUserId;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var targetRole = SystemRoles.FromRequestedRole(request.Role);
        await _userManager.AddToRoleAsync(user, targetRole);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Administrator zatwierdził użytkownika {UserId} jako {Role}", userId, targetRole);

        return Ok(new ApiResponse { Message = "Użytkownik został zatwierdzony." });
    }

    [HttpPost("registrations/{userId}/reject")]
    public async Task<ActionResult<ApiResponse>> RejectRegistration(string userId, RejectRegistrationRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika." });
        }

        user.AccountStatus = AccountStatus.Rejected;
        user.RejectedAtUtc = DateTime.UtcNow;
        user.ApprovedAtUtc = null;
        user.ReviewNote = null;
        user.ReviewedByUserId = CurrentUserId;
        user.RejectionReason = request.Reason;

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Administrator odrzucił użytkownika {UserId}", userId);

        return Ok(new ApiResponse { Message = "Rejestracja została odrzucona." });
    }

    [HttpDelete("registrations/{userId}")]
    public async Task<ActionResult<ApiResponse>> DeleteRegistration(string userId)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika." });
        }

        if (user.AccountStatus != AccountStatus.Pending)
        {
            return BadRequest(new ApiResponse { Message = "Można usuwać tylko oczekujące rejestracje." });
        }

        var deleteResult = await _userManager.DeleteAsync(user);
        if (!deleteResult.Succeeded)
        {
            return BadRequest(new ApiResponse
            {
                Message = string.Join(" ", deleteResult.Errors.Select(error => error.Description))
            });
        }

        _logger.LogInformation("Administrator usunął oczekującą rejestrację użytkownika {UserId}", userId);

        return Ok(new ApiResponse { Message = "Rejestracja została usunięta." });
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyCollection<AdminUserDto>>> GetUsers()
    {
        var users = await _dbContext.Users
            .OrderBy(user => user.FullName)
            .ToListAsync();

        var result = new List<AdminUserDto>();
        foreach (var user in users)
        {
            result.Add(new AdminUserDto
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                RequestedRole = user.RequestedRole,
                AccountStatus = user.AccountStatus,
                Roles = (await _userManager.GetRolesAsync(user)).ToList(),
                CreatedAtUtc = user.CreatedAtUtc,
                ReviewNote = user.ReviewNote
            });
        }

        return Ok(result);
    }

    [HttpPut("users/{userId}/role")]
    public async Task<ActionResult<ApiResponse>> UpdateUserRole(string userId, UpdateUserRoleRequest request)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Id == userId);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika." });
        }

        var currentRoles = await _userManager.GetRolesAsync(user);
        if (currentRoles.Count > 0)
        {
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
        }

        var targetRole = SystemRoles.FromRequestedRole(request.Role);
        await _userManager.AddToRoleAsync(user, targetRole);
        user.RequestedRole = request.Role;
        user.AccountStatus = AccountStatus.Approved;
        user.ApprovedAtUtc ??= DateTime.UtcNow;
        user.ReviewedByUserId = CurrentUserId;

        await _dbContext.SaveChangesAsync();
        return Ok(new ApiResponse { Message = "Rola użytkownika została zaktualizowana." });
    }

    [HttpGet("join-requests")]
    public async Task<ActionResult<IReadOnlyCollection<MembershipDto>>> GetJoinRequests([FromQuery] MembershipStatus? status)
    {
        var query = _dbContext.ApiaryMemberships
            .Include(membership => membership.Pasieka)
            .Include(membership => membership.User)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(membership => membership.Status == status.Value);
        }

        var items = await query
            .OrderByDescending(membership => membership.RequestedAtUtc)
            .ToListAsync();

        return Ok(items.Select(membership => new MembershipDto
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
}
