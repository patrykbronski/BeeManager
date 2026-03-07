using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeeManager.Models
{
    public enum TypMioduEnum
    {
        Wielokwiatowy = 1,
        Lipowy = 2,
        Akacjowy = 3,
        Gryczany = 4,
        Rzepakowy = 5,
        Spadziowy = 6
    }

    public class Miodobranie
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ul")]
        public int UlId { get; set; }

        public Ul? Ul { get; set; }

        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Data miodobrania")]
        public DateTime DataMiodobrania { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Typ miodu")]
        public TypMioduEnum TypMiodu { get; set; }

        [Required]
        [Column(TypeName = "decimal(7,2)")]
        [Range(0, 99999)]
        [Display(Name = "Ilość (kg)")]
        public decimal IloscKg { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notatki")]
        public string? Notatki { get; set; }
    }
}
