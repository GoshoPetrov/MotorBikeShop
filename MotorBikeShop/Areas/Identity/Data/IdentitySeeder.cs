using Microsoft.AspNetCore.Identity;

namespace MotorBikeShop.Areas.Identity.Data
{
 

    public static class IdentitySeeder
    {
        public static async Task SeedAsync(
            UserManager<MotorBikeShopUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // 1. Create Roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // 2. Create Admin User
            var adminEmail = "admin@shop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new MotorBikeShopUser
                {
                    UserName = "admin",
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(adminUser, "Admin123!"); // strong password
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // 3. Create Normal User
            var userEmail = "user@shop.com";
            var normalUser = await userManager.FindByEmailAsync(userEmail);

            if (normalUser == null)
            {
                normalUser = new MotorBikeShopUser
                {
                    UserName = "user",
                    Email = userEmail,
                    EmailConfirmed = true
                };

                await userManager.CreateAsync(normalUser, "User123!");
                await userManager.AddToRoleAsync(normalUser, "User");
            }
        }
    }
}
