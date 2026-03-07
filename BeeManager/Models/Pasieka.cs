using System.ComponentModel.DataAnnotations;

namespace BeeManager.Models
{
    public class Pasieka
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Nazwa { get; set; } = "";

        [StringLength(255)]
        public string? Lokalizacja { get; set; }

        public string? Opis { get; set; }

        public DateTime UtworzonoAt { get; set; } = DateTime.Now;
    }
}
