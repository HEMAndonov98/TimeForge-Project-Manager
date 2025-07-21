using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using TimeForge.Models;

namespace TimeForge.Infrastructure.Seeders;

public static class IdentitySeeder
{
    public static async Task SeedManagerAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        const string managerEmail = "manager@timeforge.com";
        const string managerPassword = "Manager123!";
        const string managerRole = "Manager";

        if (!await roleManager.RoleExistsAsync("Manager"))
        {
            await roleManager.CreateAsync(new IdentityRole(managerRole));
        }

        var manager = await userManager.FindByEmailAsync(managerEmail);
        if (manager == null)
        {
            manager = new User()
            {
                UserName = managerEmail,
                Email = managerEmail,
                EmailConfirmed = true,
                IsManager = true
            };

            var result = await userManager.CreateAsync(manager, managerPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(manager, managerRole);
            }
            else
            {
                throw new Exception("Failed to create Manager user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
    }
}