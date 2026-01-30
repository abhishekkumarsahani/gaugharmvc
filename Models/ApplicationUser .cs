using Microsoft.AspNetCore.Identity;

namespace GauGhar.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public ICollection<Cow>? Cows { get; set; }
        public ICollection<MilkRecord>? MilkRecords { get; set; }
        public ICollection<MilkSale>? MilkSales { get; set; }
        public ICollection<Expense>? Expenses { get; set; }
    }
}
