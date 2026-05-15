using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models;

public class Ul
{
    public int Id { get; set; }

    [Required]
    public int PasiekaId { get; set; }

    public Pasieka? Pasieka { get; set; }

    [Required]
    [StringLength(50)]
    public string NumerUla { get; set; } = string.Empty;

    public TypUlaEnum? TypUla { get; set; }

    public StatusUlaEnum? Status { get; set; }

    public DateTime? DataZalozenia { get; set; }

    [StringLength(1000)]
    public string? Uwagi { get; set; }

    public ICollection<Przeglad> Przeglady { get; set; } = new List<Przeglad>();

    public ICollection<Miodobranie> Miodobrania { get; set; } = new List<Miodobranie>();
}
