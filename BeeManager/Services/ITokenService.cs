using BeeManager.Contracts;
using BeeManager.Models;

namespace BeeManager.Services;

public interface ITokenService
{
    Task<AuthResponse> IssueTokensAsync(ApplicationUser user, CancellationToken cancellationToken = default);

    Task<AuthResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAsync(string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeAllForUserAsync(string userId, CancellationToken cancellationToken = default);
}
