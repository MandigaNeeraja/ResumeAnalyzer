using Microsoft.EntityFrameworkCore;
using ResumeAnalyzer.Constants;
using ResumeAnalyzer.Helpers;
using ResumeAnalyzer.Models;

namespace ResumeAnalyzer.Data
{
    public static class DbSeeder
    {
        private static readonly (string Name, string Email, string Password, string Role)[] SeedUsers =
        [
            ("System Admin", "admin@company.com", "Admin@123", Roles.Admin),
            ("Priya Sharma", "priya.sharma@company.com", "Hr@123", Roles.HR),
            ("Rahul Mehta", "rahul.mehta@company.com", "Hr@123", Roles.HR),
            ("Anita Desai", "anita.desai@company.com", "Hr@123", Roles.HR),
            ("Vikram Singh", "vikram.singh@company.com", "Manager@123", Roles.Manager),
            ("Sneha Patel", "sneha.patel@company.com", "Manager@123", Roles.Manager),
            ("Arjun Nair", "arjun.nair@company.com", "Manager@123", Roles.Manager),
        ];

        public static async Task SeedAsync(AppDbContext context)
        {
            await context.Database.MigrateAsync();

            foreach (var (name, email, password, role) in SeedUsers)
            {
                var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == email);
                if (existing != null)
                    continue;

                context.Users.Add(new User
                {
                    Name = name,
                    Email = email,
                    PasswordHash = PasswordHasher.Hash(password),
                    Role = role,
                    Organization = "Company"
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
