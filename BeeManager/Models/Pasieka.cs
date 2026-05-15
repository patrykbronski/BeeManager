using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models;

public class Pasieka
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Nazwa { get; set; } = string.Empty;

    [StringLength(255)]
    public string? Lokalizacja { get; set; }

    [StringLength(1000)]
    public string? Opis { get; set; }

    public DateTime UtworzonoAtUtc { get; set; } = DateTime.UtcNow;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser? Owner { get; set; }

    public ICollection<Ul> Ule { get; set; } = new List<Ul>();

    public ICollection<ApiaryMembership> Memberships { get; set; } = new List<ApiaryMembership>();
}
