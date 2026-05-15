using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace BeeManager.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(150)]
    public string FullName { get; set; } = string.Empty;

    public RequestedRole RequestedRole { get; set; } = RequestedRole.Owner;

    public AccountStatus AccountStatus { get; set; } = AccountStatus.Pending;

    [StringLength(500)]
    public string? RegistrationNote { get; set; }

    [StringLength(500)]
    public string? ReviewNote { get; set; }

    [StringLength(500)]
    public string? RejectionReason { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ApprovedAtUtc { get; set; }

    public DateTime? RejectedAtUtc { get; set; }

    public string? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedByUser { get; set; }

    public ICollection<Pasieka> OwnedApiaries { get; set; } = new List<Pasieka>();

    public ICollection<ApiaryMembership> ApiaryMemberships { get; set; } = new List<ApiaryMembership>();

    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}
