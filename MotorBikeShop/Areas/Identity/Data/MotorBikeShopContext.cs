using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data;

namespace MotorBikeShop.Data;

public class MotorBikeShopContext : IdentityDbContext<MotorBikeShopUser>
{
    public MotorBikeShopContext(DbContextOptions<MotorBikeShopContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);
    }
}
