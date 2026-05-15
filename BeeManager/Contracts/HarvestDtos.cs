using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class HarvestDto
{
    public int Id { get; set; }

    public int UlId { get; set; }

    public int PasiekaId { get; set; }

    public string NumerUla { get; set; } = string.Empty;

    public string PasiekaNazwa { get; set; } = string.Empty;

    public DateTime DataMiodobrania { get; set; }

    public TypMioduEnum TypMiodu { get; set; }

    public decimal IloscKg { get; set; }

    public string? Notatki { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }
}

public class SaveHarvestRequest
{
    [Required]
    public int UlId { get; set; }

    [Required]
    public DateTime DataMiodobrania { get; set; }

    [Required]
    public TypMioduEnum TypMiodu { get; set; }

    [Range(0, 99999)]
    public decimal IloscKg { get; set; }

    [StringLength(1000)]
    public string? Notatki { get; set; }
}
