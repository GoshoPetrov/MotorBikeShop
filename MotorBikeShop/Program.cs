using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Data;
using MotorBikeShop.Services;
namespace MotorBikeShop
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("MotorBikeShopContextConnection") ?? throw new InvalidOperationException("Connection string 'MotorBikeShopContextConnection' not found.");

            builder.Services.AddScoped<IShopService, ShopService>();

            builder.Services.AddDbContext<MotorBikeShopContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<MotorBikeShopUser>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddRoles<IdentityRole>()
                .AddEntityFrameworkStores<MotorBikeShopContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;

                var userManager = services.GetRequiredService<UserManager<MotorBikeShopUser>>();
                var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

                await IdentitySeeder.SeedAsync(userManager, roleManager);
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseExceptionHandler("/Error");

            app.UseStatusCodePagesWithReExecute("/Error/{0}");

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapRazorPages();

            app.Run();
        }
    }
}
