using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models;

public class Przeglad
{
    public int Id { get; set; }

    [Required]
    public int UlId { get; set; }

    public Ul? Ul { get; set; }

    [Required]
    public DateTime DataPrzegladu { get; set; } = DateTime.UtcNow.Date;

    [Required]
    public StanRodzinyEnum StanRodziny { get; set; }

    [Required]
    public bool ObecnoscMatki { get; set; }

    [Range(0, 50)]
    public int IloscCzerwiu { get; set; }

    [StringLength(2000)]
    public string? Notatki { get; set; }

    [Required]
    public string CreatedByUserId { get; set; } = string.Empty;

    public ApplicationUser? CreatedByUser { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAtUtc { get; set; }
}
