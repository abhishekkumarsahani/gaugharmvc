using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

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
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 50)]
        public decimal MorningQuantity { get; set; }

        [Required]
        [Column(TypeName = "decimal(5,2)")]
        [Range(0, 50)]
        public decimal EveningQuantity { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TotalQuantity { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }

        [Required]
        [ValidateNever]
        public string UserId { get; set; } = string.Empty;

        public Cow? Cow { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
