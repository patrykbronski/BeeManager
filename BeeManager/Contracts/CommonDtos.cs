using BeeManager.Models;

namespace BeeManager.Contracts;

public class ApiResponse
{
    public string Message { get; set; } = string.Empty;
}

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = Array.Empty<T>();
}

public class ApiarySummaryDto
{
    public int Id { get; set; }

    public string Nazwa { get; set; } = string.Empty;

    public string? Lokalizacja { get; set; }

    public string? Opis { get; set; }

    public string OwnerId { get; set; } = string.Empty;

    public string OwnerName { get; set; } = string.Empty;

    public DateTime UtworzonoAtUtc { get; set; }

    public bool CanManage { get; set; }

    public bool IsOwner { get; set; }

    public MembershipRole? MembershipRole { get; set; }

    public MembershipStatus? MembershipStatus { get; set; }

    public int? JoinRequestId { get; set; }
}

public class MembershipDto
{
    public int Id { get; set; }

    public int PasiekaId { get; set; }

    public string PasiekaNazwa { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public MembershipRole MembershipRole { get; set; }

    public MembershipStatus Status { get; set; }

    public string? RequestMessage { get; set; }

    public string? DecisionNote { get; set; }

    public DateTime RequestedAtUtc { get; set; }
}
