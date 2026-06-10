using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;

namespace MotorBikeShop.Infrastructure;

public static class DatabaseInitializer
{
    /// <summary>
    /// Applies any pending EF Core migrations and runs optional seed logic.
    /// Safe to call on every startup — EF will skip already-applied migrations.
    /// </summary>
    public static async Task InitializeAsync(IServiceProvider services, ILogger logger)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<MotorBikeShopContext>();

        try
        {
            logger.LogInformation("Applying EF Core migrations...");
            await db.Database.MigrateAsync();   // creates the DB if absent, then migrates
            logger.LogInformation("Migrations applied successfully.");

            await MotorBikeShopContext.SeedDataAsync(db);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;  // let the app crash loudly so Docker restarts it and retries
        }
    }

    /// <summary>
    /// Seeds sample comments after Identity users and bikes have been seeded.
    /// Called from Program.cs after IdentitySeeder.SeedAsync.
    /// </summary>
    public static async Task SeedCommentsAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MotorBikeShopContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<MotorBikeShopUser>>();

        try
        {
            if (await context.Comments.AnyAsync())
                return;

            var admin = await userManager.FindByEmailAsync("admin@shop.com");
            var user = await userManager.FindByEmailAsync("user@shop.com");

            if (admin == null || user == null || !await context.BikeModels.AnyAsync())
                return;

            var bikes = await context.BikeModels.Take(3).ToListAsync();

            var comments = new List<Comment>
            {
                new()
                {
                    BikeModelId = bikes[0].Id,
                    UserId = user.Id,
                    Content = "This bike is amazing! Very smooth ride.",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                },
                new()
                {
                    BikeModelId = bikes[0].Id,
                    UserId = admin.Id,
                    Content = "We just got a new shipment. Check out the latest models!",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                },
                new()
                {
                    BikeModelId = bikes[1].Id,
                    UserId = user.Id,
                    Content = "Perfect for off-road adventures.",
                    CreatedAt = DateTime.UtcNow.AddHours(-5)
                },
                new()
                {
                    BikeModelId = bikes[2].Id,
                    UserId = admin.Id,
                    Content = "Limited stock remaining! Get yours now.",
                    CreatedAt = DateTime.UtcNow.AddHours(-2)
                }
            };

            context.Comments.AddRange(comments);
            await context.SaveChangesAsync();
            logger.LogInformation("Seeded {Count} comments.", comments.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding comments.");
        }
    }
}