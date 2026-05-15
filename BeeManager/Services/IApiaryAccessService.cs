using BeeManager.Models;

namespace BeeManager.Services;

public interface IApiaryAccessService
{
    Task<bool> CanAccessApiaryAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default);

    Task<bool> CanManageApiaryAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default);

    Task<bool> CanEditHiveNotesAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default);
}
