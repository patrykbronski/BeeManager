using BeeManager.Data;
using BeeManager.Models;
using Microsoft.EntityFrameworkCore;

namespace BeeManager.Services;

public class ApiaryAccessService : IApiaryAccessService
{
    private readonly ApplicationDbContext _dbContext;

    public ApiaryAccessService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<bool> CanAccessApiaryAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default)
    {
        if (roles.Contains("Admin"))
        {
            return true;
        }

        if (roles.Contains("Owner"))
        {
            return await _dbContext.Pasieki.AnyAsync(p => p.Id == pasiekaId && p.OwnerId == userId, cancellationToken);
        }

        return await _dbContext.ApiaryMemberships.AnyAsync(
            membership => membership.PasiekaId == pasiekaId &&
                          membership.UserId == userId &&
                          membership.Status == MembershipStatus.Approved,
            cancellationToken);
    }

    public async Task<bool> CanManageApiaryAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default)
    {
        if (roles.Contains("Admin"))
        {
            return true;
        }

        return roles.Contains("Owner") &&
               await _dbContext.Pasieki.AnyAsync(p => p.Id == pasiekaId && p.OwnerId == userId, cancellationToken);
    }

    public async Task<bool> CanEditHiveNotesAsync(string userId, string[] roles, int pasiekaId, CancellationToken cancellationToken = default)
    {
        if (await CanManageApiaryAsync(userId, roles, pasiekaId, cancellationToken))
        {
            return true;
        }

        return roles.Contains("Worker") &&
               await _dbContext.ApiaryMemberships.AnyAsync(
                   membership => membership.PasiekaId == pasiekaId &&
                                 membership.UserId == userId &&
                                 membership.Status == MembershipStatus.Approved &&
                                 membership.MembershipRole == MembershipRole.Worker,
                   cancellationToken);
    }
}
