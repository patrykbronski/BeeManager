using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class RegistrationReviewDto
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public RequestedRole RequestedRole { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public string? RegistrationNote { get; set; }

    public string? ReviewNote { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? RejectionReason { get; set; }
}

public class ApproveRegistrationRequest
{
    [Required]
    public RequestedRole Role { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

public class RejectRegistrationRequest
{
    [Required]
    [StringLength(500)]
    public string Reason { get; set; } = string.Empty;
}

public class UpdateUserRoleRequest
{
    [Required]
    public RequestedRole Role { get; set; }
}

public class AdminUserDto
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public RequestedRole RequestedRole { get; set; }

    public AccountStatus AccountStatus { get; set; }

    public List<string> Roles { get; set; } = new();

    public DateTime CreatedAtUtc { get; set; }

    public string? ReviewNote { get; set; }
}
