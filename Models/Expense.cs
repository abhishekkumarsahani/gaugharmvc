using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class Expense
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "Expense Date")]
        [DataType(DataType.Date)]
        public DateTime ExpenseDate { get; set; } = DateTime.Today;

        [Required]
        public int ExpenseCategoryId { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        [Range(0.01, 1000000, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
        public string UserId { get; set; } = string.Empty;

        // Navigation properties
        [ForeignKey("ExpenseCategoryId")]
        public virtual ExpenseCategory? ExpenseCategory { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}
