using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class Vaccination
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CowId { get; set; }

        [Required]
        [StringLength(100)]
        public string VaccineName { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Date)]
        public DateTime VaccinationDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        [Display(Name = "Next Due Date")]
        public DateTime? NextDueDate { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("CowId")]
        public virtual Cow? Cow { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
