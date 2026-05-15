using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class InspectionDto
{
    public int Id { get; set; }

    public int UlId { get; set; }

    public int PasiekaId { get; set; }

    public string NumerUla { get; set; } = string.Empty;

    public string PasiekaNazwa { get; set; } = string.Empty;

    public DateTime DataPrzegladu { get; set; }

    public StanRodzinyEnum StanRodziny { get; set; }

    public bool ObecnoscMatki { get; set; }

    public int IloscCzerwiu { get; set; }

    public string? Notatki { get; set; }

    public string CreatedBy { get; set; } = string.Empty;

    public bool CanEdit { get; set; }

    public bool CanDelete { get; set; }

    public bool CanAddSpecialistNote { get; set; }
}

public class SaveInspectionRequest
{
    [Required]
    public int UlId { get; set; }

    [Required]
    public DateTime DataPrzegladu { get; set; }

    [Required]
    public StanRodzinyEnum StanRodziny { get; set; }

    [Required]
    public bool ObecnoscMatki { get; set; }

    [Range(0, 50)]
    public int IloscCzerwiu { get; set; }

    [StringLength(2000)]
    public string? Notatki { get; set; }
}

public class SpecialistNoteRequest
{
    [Required]
    [StringLength(1000)]
    public string Note { get; set; } = string.Empty;
}
