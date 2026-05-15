using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BeeManager.Configuration;
using BeeManager.Contracts;
using BeeManager.Data;
using BeeManager.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BeeManager.Services;

public class TokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public TokenService(
        IOptions<JwtOptions> jwtOptions,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _jwtOptions = jwtOptions.Value;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var roles = await _userManager.GetRolesAsync(user);
        var accessExpiresAt = DateTime.UtcNow.AddMinutes(_jwtOptions.AccessTokenMinutes);
        var refreshExpiresAt = DateTime.UtcNow.AddDays(_jwtOptions.RefreshTokenDays);

        var refreshTokenValue = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        var refreshToken = new RefreshToken
        {
            TokenHash = HashRefreshToken(refreshTokenValue),
            UserId = user.Id,
            ExpiresAtUtc = refreshExpiresAt
        };

        _dbContext.RefreshTokens.Add(refreshToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = BuildAccessToken(user, roles, accessExpiresAt),
            AccessTokenExpiresAtUtc = accessExpiresAt,
            RefreshToken = refreshTokenValue,
            RefreshTokenExpiresAtUtc = refreshExpiresAt,
            User = new CurrentUserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                AccountStatus = user.AccountStatus,
                RequestedRole = user.RequestedRole,
                Roles = roles.ToList()
            }
        };
    }

    public async Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = HashRefreshToken(refreshToken);
        var token = await _dbContext.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash, cancellationToken);

        if (token is null || !token.IsActive || token.User is null || token.User.AccountStatus != AccountStatus.Approved)
        {
            return null;
        }

        token.RevokedAtUtc = DateTime.UtcNow;

        var refreshed = await IssueTokensAsync(token.User, cancellationToken);
        token.ReplacedByToken = refreshed.RefreshToken;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return refreshed;
    }

    public async Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var refreshTokenHash = HashRefreshToken(refreshToken);
        var token = await _dbContext.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.TokenHash == refreshTokenHash, cancellationToken);

        if (token is null || token.RevokedAtUtc is not null)
        {
            return;
        }

        token.RevokedAtUtc = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private string BuildAccessToken(ApplicationUser user, IEnumerable<string> roles, DateTime expiresAt)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("full_name", user.FullName),
            new("requested_role", user.RequestedRole.ToString()),
            new("account_status", user.AccountStatus.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }
}
