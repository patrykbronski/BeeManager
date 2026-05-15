using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models;

public class RefreshToken
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string TokenHash { get; set; } = string.Empty;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public ApplicationUser? User { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    [StringLength(200)]
    public string? ReplacedByToken { get; set; }

    public bool IsActive => RevokedAtUtc is null && ExpiresAtUtc > DateTime.UtcNow;
}
