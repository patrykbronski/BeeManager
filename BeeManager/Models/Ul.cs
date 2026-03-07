using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models
{
    public enum TypUlaEnum
    {
        Wielkopolski = 1,
        Dadant = 2,
        Warszawski = 3
    }

    public enum StatusUlaEnum
    {
        Aktywny = 1,
        Pusty = 2,
        Zniszczony = 3
    }

    public class Ul
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Pasieka")]
        public int PasiekaId { get; set; }

        public Pasieka? Pasieka { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Numer ula")]
        public string NumerUla { get; set; } = "";


        [Display(Name = "Typ ula")]
        public TypUlaEnum? TypUla { get; set; }

        [Display(Name = "Status")]
        public StatusUlaEnum? Status { get; set; }






        [Display(Name = "Data założenia")]
        public DateTime? DataZalozenia { get; set; }

        public string? Uwagi { get; set; }
    }
}
