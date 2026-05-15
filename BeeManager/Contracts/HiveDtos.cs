using System.ComponentModel.DataAnnotations;
using BeeManager.Models;

namespace BeeManager.Contracts;

public class HiveDto
{
    public int Id { get; set; }

    public int PasiekaId { get; set; }

    public string PasiekaNazwa { get; set; } = string.Empty;

    public string NumerUla { get; set; } = string.Empty;

    public TypUlaEnum? TypUla { get; set; }

    public StatusUlaEnum? Status { get; set; }

    public DateTime? DataZalozenia { get; set; }

    public string? Uwagi { get; set; }

    public bool CanManage { get; set; }

    public bool CanEditNotes { get; set; }
}

public class SaveHiveRequest
{
    [Required]
    public int PasiekaId { get; set; }

    [Required]
    [StringLength(50)]
    public string NumerUla { get; set; } = string.Empty;

    public TypUlaEnum? TypUla { get; set; }

    public StatusUlaEnum? Status { get; set; }

    public DateTime? DataZalozenia { get; set; }

    [StringLength(1000)]
    public string? Uwagi { get; set; }
}

public class UpdateHiveNotesRequest
{
    [Required]
    [StringLength(1000)]
    public string Uwagi { get; set; } = string.Empty;
}
