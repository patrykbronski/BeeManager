using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class CreateApiaryRequest
{
    [Required]
    [StringLength(150)]
    public string Nazwa { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Lokalizacja { get; set; }

    [StringLength(1000)]
    public string? Opis { get; set; }
}

public class UpdateApiaryRequest : CreateApiaryRequest
{
}

public class CreateJoinRequest
{
    [StringLength(500)]
    public string? Message { get; set; }
}

public class DirectMembershipRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public MembershipRole MembershipRole { get; set; }
}

public class ReviewMembershipRequest
{
    [StringLength(500)]
    public string? DecisionNote { get; set; }
}
