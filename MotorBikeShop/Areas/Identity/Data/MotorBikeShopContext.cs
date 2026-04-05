using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Areas.Identity.Data.Entities;

namespace MotorBikeShop.Data;

public class MotorBikeShopContext : IdentityDbContext<MotorBikeShopUser>
{
    public DbSet<BikeModel> BikeModels => Set<BikeModel>();
    public DbSet<Inventory> Inventories => Set<Inventory>();
    public DbSet<Basket> Baskets => Set<Basket>();
    public DbSet<BasketItem> BasketItems => Set<BasketItem>();
    public DbSet<Vent> Vents => Set<Vent>();
    public DbSet<VentItem> VentItems => Set<VentItem>();

    public MotorBikeShopContext(DbContextOptions<MotorBikeShopContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<BikeModel>()
            .HasOne(b => b.Inventory)
            .WithOne(i => i.BikeModel)
            .HasForeignKey<Inventory>(i => i.BikeModelId);

        modelBuilder.Entity<MotorBikeShopUser>()
            .HasOne(u => u.Basket)
            .WithOne(b => b.User)
            .HasForeignKey<Basket>(b => b.UserId);

        modelBuilder.Entity<Basket>()
            .HasMany(b => b.Items)
            .WithOne(i => i.Basket)
            .HasForeignKey(i => i.BasketId);

        modelBuilder.Entity<BasketItem>()
            .HasOne(bi => bi.BikeModel)
            .WithMany()
            .HasForeignKey(bi => bi.BikeModelId);

        modelBuilder.Entity<MotorBikeShopUser>()
            .HasMany(u => u.Vents)
            .WithOne(v => v.User)
            .HasForeignKey(v => v.UserId);

        modelBuilder.Entity<Vent>()
            .HasMany(v => v.Items)
            .WithOne(vi => vi.Vent)
            .HasForeignKey(vi => vi.VentId);

        modelBuilder.Entity<VentItem>()
            .HasOne(vi => vi.BikeModel)
            .WithMany()
            .HasForeignKey(vi => vi.BikeModelId);

        // BIKE MODELS
        modelBuilder.Entity<BikeModel>().HasData(
            new BikeModel
            {
                Id = 1,
                Name = "CBR600RR",
                Brand = "Honda",
                Year = 2022,
                Price = 12000
            },
            new BikeModel
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
                BikeModelId = 1,
                Quantity = 5
            },
            new Inventory
            {
                Id = 2,
                BikeModelId = 2,
                Quantity = 3
            }
        );


        // BASKET ITEMS
        modelBuilder.Entity<BasketItem>().HasData(
            new BasketItem
            {
                Id = 1,
                BasketId = 1,
                BikeModelId = 1,
                Quantity = 1
            }
        );


        // VENT ITEMS
        modelBuilder.Entity<VentItem>().HasData(
            new VentItem
            {
                Id = 1,
                VentId = 1,
                BikeModelId = 1,
                Quantity = 1,
                Price = 12000
            }
        );
    }
}
