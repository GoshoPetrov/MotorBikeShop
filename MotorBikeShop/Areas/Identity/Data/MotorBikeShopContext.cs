using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Areas.Identity.Data.Entities;

namespace MotorBikeShop.Data;

public class MotorBikeShopContext : IdentityDbContext<MotorBikeShopUser>
{
    public MotorBikeShopContext(DbContextOptions<MotorBikeShopContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // MODELS
        modelBuilder.Entity<Model>().HasData(
            new Model
            {
                Id = 1,
                Name = "CBR600RR",
                Brand = "Honda",
                Year = 2022,
                Price = 12000
            },
            new Model
            {
                Id = 2,
                Name = "YZF-R1",
                Brand = "Yamaha",
                Year = 2023,
                Price = 18000
            }
        );

        // INVENTORY
        modelBuilder.Entity<Inventory>().HasData(
            new Inventory
            {
                Id = 1,
                ModelId = 1,
                Quantity = 5
            },
            new Inventory
            {
                Id = 2,
                ModelId = 2,
                Quantity = 3
            }
        );

        // USERS
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@shop.com",
                PasswordHash = "hashed_password"
            }
        );

        // BASKET
        modelBuilder.Entity<Basket>().HasData(
            new Basket
            {
                Id = 1,
                UserId = 1
            }
        );

        // BASKET ITEMS
        modelBuilder.Entity<BasketItem>().HasData(
            new BasketItem
            {
                Id = 1,
                BasketId = 1,
                ModelId = 1,
                Quantity = 1
            }
        );

        // VENTS (Orders)
        modelBuilder.Entity<Vent>().HasData(
            new Vent
            {
                Id = 1,
                UserId = 1,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = 12000
            }
        );

        // VENT ITEMS
        modelBuilder.Entity<VentItem>().HasData(
            new VentItem
            {
                Id = 1,
                VentId = 1,
                ModelId = 1,
                Quantity = 1,
                Price = 12000
            }
        );
    }
}
