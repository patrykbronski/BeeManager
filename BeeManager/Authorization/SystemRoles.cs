using BeeManager.Models;

namespace BeeManager.Authorization;

public static class SystemRoles
{
    public const string Admin = "Admin";
    public const string Owner = "Owner";
    public const string Worker = "Worker";
    public const string Inspector = "Inspector";

    public static readonly string[] All = { Admin, Owner, Worker, Inspector };

    public static string FromRequestedRole(RequestedRole role) => role switch
    {
        RequestedRole.Owner => Owner,
        RequestedRole.Worker => Worker,
        RequestedRole.Inspector => Inspector,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, null)
    };
}
