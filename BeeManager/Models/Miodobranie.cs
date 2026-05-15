using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeManager.Models;

public class Miodobranie
{
    public int Id { get; set; }

    [Required]
    public int UlId { get; set; }

    public Ul? Ul { get; set; }

    [Required]
    public DateTime DataMiodobrania { get; set; } = DateTime.UtcNow.Date;

    [Required]
    public TypMioduEnum TypMiodu { get; set; }

    [Required]
    [Column(TypeName = "decimal(7,2)")]
    [Range(0, 99999)]
    public decimal IloscKg { get; set; }

    [StringLength(1000)]
    public string? Notatki { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public ApplicationUser? CreatedByUser { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
