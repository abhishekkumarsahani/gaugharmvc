using GauGhar.Data;
using GauGhar.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public static class SeedData
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService<DbContextOptions<ApplicationDbContext>>()))
        {
            // Create roles
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            string[] roleNames = { "Admin", "Staff", "Viewer" };

            foreach (var roleName in roleNames)
            {
                var roleExist = await roleManager.RoleExistsAsync(roleName);
                if (!roleExist)
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var adminUser = await userManager.FindByEmailAsync("admin@gaugar.com");

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "admin@gaugar.com",
                    Email = "admin@gaugar.com",
                    FullName = "Administrator",
                    EmailConfirmed = true
                };

                var createPowerUser = await userManager.CreateAsync(adminUser, "Admin@123");
                if (createPowerUser.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Ensure expense categories exist
            if (!context.ExpenseCategories.Any())
            {
                context.ExpenseCategories.AddRange(
                    new ExpenseCategory { Name = "Fodder", Description = "Animal feed and grass" },
                    new ExpenseCategory { Name = "Medicine", Description = "Veterinary medicines" },
                    new ExpenseCategory { Name = "Staff Salary", Description = "Employee wages" },
                    new ExpenseCategory { Name = "Electricity", Description = "Electricity bills" },
                    new ExpenseCategory { Name = "Water", Description = "Water bills" },
                    new ExpenseCategory { Name = "Maintenance", Description = "Repairs and maintenance" },
                    new ExpenseCategory { Name = "Transport", Description = "Transportation costs" },
                    new ExpenseCategory { Name = "Others", Description = "Miscellaneous expenses" }
                );
                await context.SaveChangesAsync();
            }
        }
    }
}