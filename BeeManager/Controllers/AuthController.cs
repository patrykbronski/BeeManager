using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using BeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register(RegisterRequest request)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var exists = await _userManager.Users.AnyAsync(user => user.Email == normalizedEmail);
        if (exists)
        {
            return Conflict(new ApiResponse { Message = "Konto z takim adresem e-mail już istnieje." });
        }

        var user = new ApplicationUser
        {
            UserName = normalizedEmail,
            Email = normalizedEmail,
            FullName = request.FullName.Trim(),
            RequestedRole = request.RequestedRole,
            RegistrationNote = request.RegistrationNote?.Trim(),
            AccountStatus = AccountStatus.Pending
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return BadRequest(new ApiResponse
            {
                Message = string.Join(" ", result.Errors.Select(error => error.Description))
            });
        }

        _logger.LogInformation("Nowa rejestracja użytkownika {Email} z rolą {Role}", user.Email, user.RequestedRole);

        return Ok(new ApiResponse
        {
            Message = "Rejestracja zakończona. Konto oczekuje na zatwierdzenie przez administratora."
        });
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var normalizedLogin = request.Login.Trim().ToLowerInvariant();
        var user = await _userManager.Users.FirstOrDefaultAsync(u =>
            (u.Email != null && u.Email == normalizedLogin) || u.UserName == normalizedLogin);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return Unauthorized(new ApiResponse { Message = "Nieprawidłowy login lub hasło." });
        }

        if (user.AccountStatus == AccountStatus.Pending)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiResponse { Message = "Konto oczekuje na zatwierdzenie przez administratora." });
        }

        if (user.AccountStatus == AccountStatus.Rejected)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiResponse
                {
                    Message = string.IsNullOrWhiteSpace(user.RejectionReason)
                        ? "Konto zostało odrzucone przez administratora."
                        : $"Konto zostało odrzucone: {user.RejectionReason}"
                });
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new ApiResponse { Message = "Konto nie ma przypisanej roli. Skontaktuj się z administratorem." });
        }

        _logger.LogInformation("Udane logowanie użytkownika {Email}", user.Email);
        return Ok(await _tokenService.IssueTokensAsync(user));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var response = await _tokenService.RefreshAsync(request.RefreshToken);
        if (response is null)
        {
            return Unauthorized(new ApiResponse { Message = "Refresh token jest nieprawidłowy lub wygasł." });
        }

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout(RefreshRequest request)
    {
        await _tokenService.RevokeAsync(request.RefreshToken);
        return Ok(new ApiResponse { Message = "Wylogowano pomyślnie." });
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<ActionResult<ApiResponse>> LogoutAll()
    {
        await _tokenService.RevokeAllForUserAsync(CurrentUserId);
        return Ok(new ApiResponse { Message = "Wylogowano ze wszystkich urządzeń." });
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserDto>> Me()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == CurrentUserId);
        if (user is null)
        {
            return NotFound(new ApiResponse { Message = "Nie znaleziono użytkownika." });
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(new CurrentUserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            AccountStatus = user.AccountStatus,
            RequestedRole = user.RequestedRole,
            Roles = roles.ToList()
        });
    }
}
