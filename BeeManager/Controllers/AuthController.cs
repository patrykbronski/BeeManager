using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using BeeManager.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;

namespace BeeManager.Controllers;

[Route("api/auth")]
public class AuthController : ApiControllerBase
{
    private const string RefreshTokenCookieName = "bee_refresh_token";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext,
        ITokenService tokenService,
        IHostEnvironment environment,
        ILogger<AuthController> logger)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _environment = environment;
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

        _logger.LogInformation("Nowa rejestracja uzytkownika {Email} z rola {Role}", user.Email, user.RequestedRole);

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

        _logger.LogInformation("Udane logowanie uzytkownika {Email}", user.Email);
        var response = await _tokenService.IssueTokensAsync(user);
        SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);
        return Ok(ToClientAuthResponse(response));
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshRequest request)
    {
        var refreshToken = GetRefreshToken(request);
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized(new ApiResponse { Message = "Brak refresh tokena." });
        }

        var response = await _tokenService.RefreshAsync(refreshToken);
        if (response is null)
        {
            ClearRefreshTokenCookie();
            return Unauthorized(new ApiResponse { Message = "Refresh token jest nieprawidłowy lub wygasł." });
        }

        SetRefreshTokenCookie(response.RefreshToken, response.RefreshTokenExpiresAtUtc);
        return Ok(ToClientAuthResponse(response));
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout(RefreshRequest request)
    {
        var refreshToken = GetRefreshToken(request);
        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            await _tokenService.RevokeAsync(refreshToken);
        }

        ClearRefreshTokenCookie();
        return Ok(new ApiResponse { Message = "Wylogowano pomyślnie." });
    }

    [Authorize]
    [HttpPost("logout-all")]
    public async Task<ActionResult<ApiResponse>> LogoutAll()
    {
        await _tokenService.RevokeAllForUserAsync(CurrentUserId);
        ClearRefreshTokenCookie();
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

    private static AuthResponse ToClientAuthResponse(AuthResponse response) =>
        new()
        {
            AccessToken = response.AccessToken,
            AccessTokenExpiresAtUtc = response.AccessTokenExpiresAtUtc,
            User = response.User
        };

    private string? GetRefreshToken(RefreshRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return request.RefreshToken.Trim();
        }

        return Request.Cookies.TryGetValue(RefreshTokenCookieName, out var cookieValue)
            ? cookieValue
            : null;
    }

    private void SetRefreshTokenCookie(string refreshToken, DateTime expiresAtUtc)
    {
        Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Expires = expiresAtUtc,
            Path = "/"
        });
    }

    private void ClearRefreshTokenCookie()
    {
        Response.Cookies.Delete(RefreshTokenCookieName, new CookieOptions
        {
            Secure = !_environment.IsDevelopment(),
            SameSite = _environment.IsDevelopment() ? SameSiteMode.Lax : SameSiteMode.None,
            Path = "/"
        });
    }
}
