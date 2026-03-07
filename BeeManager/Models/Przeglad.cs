using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models
{
    public enum StanRodzinyEnum
    {
        BardzoSlaby = 1,
        Slaby = 2,
        Sredni = 3,
        Dobry = 4,
        BardzoDobry = 5
    }

    public class Przeglad
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ul")]
        public int UlId { get; set; }

        public Ul? Ul { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data przeglądu")]
        public DateTime DataPrzegladu { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Stan rodziny")]
        public StanRodzinyEnum StanRodziny { get; set; }

        [Required]
        [Display(Name = "Obecność matki")]
        public bool ObecnoscMatki { get; set; }

        [Range(0, 50)]
        [Display(Name = "Ilość czerwiu (ramek)")]
        public int IloscCzerwiu { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notatki")]
        public string? Notatki { get; set; }
    }
}
