
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;

namespace MotorBikeShop.Services
{
    public interface IShopService
    {
        Task<List<BikeViewModel>> BikeInventory();
    }

    public class ShopService: IShopService
    {
        private readonly MotorBikeShopContext _context;

        public ShopService(MotorBikeShopContext context)
        {
            _context = context;
        }

        public async Task<List<BikeViewModel>> BikeInventory()
        {
            var bikes = await _context.BikeModels
            .Include(b => b.Inventory) // include stock
            .ToListAsync();

            var result = bikes.Select((db) => new BikeViewModel()
            {
                Id = db.Id,
                Name = db.Name,
                Brand = db.Brand,
                Year = db.Year,
                Price = db.Price,
                Description = db.Description,
                InventoryQuantity = db.Inventory?.Quantity,
                ImageUrl = db.ImageUrl
            }).ToList();

            return result;
        }
    }
}
