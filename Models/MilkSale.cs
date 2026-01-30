using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class MilkSale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime SaleDate { get; set; } = DateTime.Today;

        [Required]
        [StringLength(200)]
        [Display(Name = "Buyer Name")]
        public string BuyerName { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Buyer Type")]
        public string BuyerType { get; set; } = "Local"; // Dairy, Hotel, Local

        [Required]
        [Display(Name = "Quantity (L)")]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0.1, 1000, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; }

        [Required]
        [Display(Name = "Rate per Liter")]
        [Column(TypeName = "decimal(10,2)")]
        [Range(1, 200, ErrorMessage = "Rate must be between 1 and 200")]
        public decimal RatePerLiter { get; set; }

        // Remove [NotMapped] and make it a regular property
        [Display(Name = "Total Amount")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; } // Remove the computed getter

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
