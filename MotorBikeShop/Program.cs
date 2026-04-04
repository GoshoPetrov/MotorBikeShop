using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data;
using MotorBikeShop.Data;
namespace MotorBikeShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration.GetConnectionString("MotorBikeShopContextConnection") ?? throw new InvalidOperationException("Connection string 'MotorBikeShopContextConnection' not found.");

            builder.Services.AddDbContext<MotorBikeShopContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<MotorBikeShopUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<MotorBikeShopContext>();

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

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
