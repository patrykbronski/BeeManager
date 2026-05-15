using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models;

public class ApiaryMembership
{
    public int Id { get; set; }

    [Required]
    public int PasiekaId { get; set; }

    public Pasieka? Pasieka { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public MembershipRole MembershipRole { get; set; }

    public MembershipStatus Status { get; set; } = MembershipStatus.Pending;

    [StringLength(500)]
    public string? RequestMessage { get; set; }

    [StringLength(500)]
    public string? DecisionNote { get; set; }

    public DateTime RequestedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? ReviewedAtUtc { get; set; }

    public string? ReviewedByUserId { get; set; }

    public ApplicationUser? ReviewedByUser { get; set; }
}
