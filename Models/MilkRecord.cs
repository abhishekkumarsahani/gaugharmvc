using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class MilkRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int CowId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; } = DateTime.Today;

        [Required]
        [Display(Name = "Morning Quantity (L)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 50, ErrorMessage = "Quantity must be between 0 and 50")]
        public decimal MorningQuantity { get; set; }

        [Required]
        [Display(Name = "Evening Quantity (L)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 50, ErrorMessage = "Quantity must be between 0 and 50")]
        public decimal EveningQuantity { get; set; }

        // Remove [NotMapped] and make it a regular property
        [Display(Name = "Total Quantity")]
        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalQuantity { get; set; } // Remove the computed getter

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
