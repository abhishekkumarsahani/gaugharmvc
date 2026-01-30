using Microsoft.EntityFrameworkCore;
using GauGhar.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace GauGhar.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Cow> Cows { get; set; }
        public DbSet<MilkRecord> MilkRecords { get; set; }
        public DbSet<MilkSale> MilkSales { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<ExpenseCategory> ExpenseCategories { get; set; }
        public DbSet<Vaccination> Vaccinations { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships with explicit delete behavior

            // Cow relationships
            builder.Entity<Cow>()
                .HasMany(c => c.MilkRecords)
                .WithOne(m => m.Cow)
                .HasForeignKey(m => m.CowId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            builder.Entity<Cow>()
                .HasMany(c => c.Vaccinations)
                .WithOne(v => v.Cow)
                .HasForeignKey(v => v.CowId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            // User relationships
            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Cows)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.MilkRecords)
                .WithOne(m => m.User)
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.MilkSales)
                .WithOne(s => s.User)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ApplicationUser>()
                .HasMany(u => u.Expenses)
                .WithOne(e => e.User)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Expense relationships
            builder.Entity<Expense>()
                .HasOne(e => e.ExpenseCategory)
                .WithMany(ec => ec.Expenses)
                .HasForeignKey(e => e.ExpenseCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Unique constraint for cow tag number per user
            builder.Entity<Cow>()
                .HasIndex(c => new { c.TagNumber, c.UserId })
                .IsUnique();

            // Prevent duplicate milk entry for same cow on same day
            builder.Entity<MilkRecord>()
                .HasIndex(m => new { m.CowId, m.Date })
                .IsUnique();

            // Configure computed columns
            builder.Entity<MilkRecord>()
                .Property(m => m.TotalQuantity)
                .HasComputedColumnSql("[MorningQuantity] + [EveningQuantity]");

            builder.Entity<MilkSale>()
                .Property(s => s.TotalAmount)
                .HasComputedColumnSql("[Quantity] * [RatePerLiter]");

            // Seed initial expense categories
            builder.Entity<ExpenseCategory>().HasData(
                new ExpenseCategory { Id = 1, Name = "Fodder", Description = "Animal feed and grass" },
                new ExpenseCategory { Id = 2, Name = "Medicine", Description = "Veterinary medicines" },
                new ExpenseCategory { Id = 3, Name = "Staff Salary", Description = "Employee wages" },
                new ExpenseCategory { Id = 4, Name = "Electricity", Description = "Electricity bills" },
                new ExpenseCategory { Id = 5, Name = "Water", Description = "Water bills" },
                new ExpenseCategory { Id = 6, Name = "Maintenance", Description = "Repairs and maintenance" },
                new ExpenseCategory { Id = 7, Name = "Transport", Description = "Transportation costs" },
                new ExpenseCategory { Id = 8, Name = "Others", Description = "Miscellaneous expenses" }
            );
        }
    }
}