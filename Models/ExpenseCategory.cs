using System.ComponentModel.DataAnnotations;

namespace GauGhar.Models
{
    public class ExpenseCategory
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Expense>? Expenses { get; set; }
    }
}
