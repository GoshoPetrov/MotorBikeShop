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
                Price = 12000,
                ImageUrl = "https://cloudfront-us-east-1.images.arcpublishing.com/octane/K25WVPRMCVEDZJ7ZJK4BWE374Y.jpg"
            },
            new BikeModel
            {
                Id = 2,
                Name = "YZ125",
                Brand = "Yamaha",
                Year = 2023,
                Price = 18000,
                ImageUrl = "https://ultimatemotorcycling.com/wp-content/uploads/2022/08/2023-yamaha-yz125x-first-look-gncc-cross-country-racing-two-stroke-motorcycle-dirt-bike-1.jpg"
            },

             // 🔥 NEW BIKES

            new BikeModel
            {
                Id = 3,
                Name = "Ninja ZX-6R",
                Brand = "Kawasaki",
                Year = 2023,
                Price = 13000,
                ImageUrl = "https://www.cycleworld.com/resizer/hDgZ3RY9ecoWZYR-r5gIyTpp8JE=/arc-photo-octane/arc3-prod/public/HS6BF7FJD5EZHNW64BX4VKHCL4.jpg"
            },
            new BikeModel
            {
                Id = 4,
                Name = "Panigale V4",
                Brand = "Ducati",
                Year = 2024,
                Price = 25000,
                ImageUrl = "https://images5.1000ps.net/b-f_W3011628-neue-ducati-panigale-v4-2025-638575009620977741.jpg?format=webp&quality=80&scale=both&width=2816&height=1584&mode=crop"
            },
            new BikeModel
            {
                Id = 5,
                Name = "GSX-R750",
                Brand = "Suzuki",
                Year = 2022,
                Price = 11000,
                ImageUrl = "https://iconicmotorbikeauctions.com/wp-content/uploads/2022/08/Suzuki-GSX-R750-Front-Right-Featured.jpg"
            },
            new BikeModel
            {
                Id = 6,
                Name = "S1000RR",
                Brand = "BMW",
                Year = 2023,
                Price = 22000,
                ImageUrl = "https://bmw.europe-moto.com/img/cms/s1000rrv1.jpg"
            },
            new BikeModel
            {
                Id = 7,
                Name = "RC 390",
                Brand = "KTM",
                Year = 2023,
                Price = 7000,
                ImageUrl = "https://superbikestore.in/cdn/shop/products/16381CJ520_RC-390_2017_R77_3_4_SS_SS_CF_1Gray_2048x2048_a227f7c6-bfe0-40fa-80d1-c083fdfeecaa.jpg?v=1577786021"
            },
            new BikeModel
            {
                Id = 8,
                Name = "CRF450R",
                Brand = "Honda",
                Year = 2023,
                Price = 9500,
                ImageUrl = "https://powersportsbusiness.com/wp-content/uploads/2022/06/23-Honda-CRF450R-50th_Location-2a.jpg"
            },
            new BikeModel
            {
                Id = 9,
                Name = "YZ450F",
                Brand = "Yamaha",
                Year = 2024,
                Price = 9800,
                ImageUrl = "https://motocrossactionmag.com/wp-content/uploads/2023/02/YZ450-front-angle.jpg"
            },
            new BikeModel
            {
                Id = 10,
                Name = "KX250",
                Brand = "Kawasaki",
                Year = 2023,
                Price = 8200,
                ImageUrl = "https://hudsonmotorcycles.com/wp-content/uploads/2024/11/IMG_2362.jpg"
            },
            new BikeModel
            {
                Id = 11,
                Name = "EXC 300",
                Brand = "KTM",
                Year = 2024,
                Price = 10500,
                ImageUrl = "https://images5.1000ps.net/images_bikekat/2025/1-KTM/222-300_EXC/007-638550785727500522-ktm-300-exc.jpg?width=920&height=571&mode=crop&scale=both&format=webp"
            },
            new BikeModel
            {
                Id = 12,
                Name = "TE 300i",
                Brand = "Husqvarna",
                Year = 2023,
                Price = 11000,
                ImageUrl = "https://i.ytimg.com/vi/RbnfQW3TBpo/maxresdefault.jpg"
            }
         );

        // INVENTORY

        modelBuilder.Entity<Inventory>().HasData(
    new Inventory { Id = 1, BikeModelId = 1, Quantity = 5 },
    new Inventory { Id = 2, BikeModelId = 2, Quantity = 3 },
    new Inventory { Id = 3, BikeModelId = 3, Quantity = 4 },
    new Inventory { Id = 4, BikeModelId = 4, Quantity = 2 },
    new Inventory { Id = 5, BikeModelId = 5, Quantity = 6 },
    new Inventory { Id = 6, BikeModelId = 6, Quantity = 3 },
    new Inventory { Id = 7, BikeModelId = 7, Quantity = 5 },
    new Inventory { Id = 8, BikeModelId = 8, Quantity = 7 },
    new Inventory { Id = 9, BikeModelId = 9, Quantity = 4 },
    new Inventory { Id = 10, BikeModelId = 10, Quantity = 6 },
    new Inventory { Id = 11, BikeModelId = 11, Quantity = 2 },
    new Inventory { Id = 12, BikeModelId = 12, Quantity = 3 }
);

    }
}
