using Microsoft.EntityFrameworkCore;
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

            // ── Seed data ─────────────────────────────────────────────────────
            // Uncomment / extend with your own seeding logic:
            // await SeedAsync(db, logger);

            await MotorBikeShopContext.SeedDataAsync(db);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing the database.");
            throw;  // let the app crash loudly so Docker restarts it and retries
        }
    }

    // private static async Task SeedAsync(MotorBikeShopContext db, ILogger logger)
    // {
    //     if (!await db.YourEntities.AnyAsync())
    //     {
    //         db.YourEntities.AddRange(/* your seed records */);
    //         await db.SaveChangesAsync();
    //         logger.LogInformation("Database seeded.");
    //     }
    // }
}