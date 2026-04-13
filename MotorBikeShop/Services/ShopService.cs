
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;

namespace MotorBikeShop.Services
{
    public interface IShopService
    {
        Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel);
        Task<BikeViewModel> GetBike(int id);
        Task<BikeViewModel> DeleteBike(int id);
        Task<BikeViewModel> GetBikeModelDetails(int id);
        Task<List<BikeViewModel>> GetBikeModelIndex();
        Task<BikeViewModel> GetShowcaseDetail(int id);
        Task<List<BikeViewModel>> GetShowcase(string searchString);
        Task<List<BikeViewModel>> BikeInventory();
    }

    public class ShopService: IShopService
    {
        private BikeViewModel ToViewModel(BikeModel db)
        {
            return new BikeViewModel()
            {
                Id = db.Id,
                Name = db.Name,
                Brand = db.Brand,
                Year = db.Year,
                Price = db.Price,
                Description = db.Description,
                InventoryQuantity = db.Inventory?.Quantity,
                ImageUrl = db.ImageUrl
            };
        }

        private readonly MotorBikeShopContext _context;

        public ShopService(MotorBikeShopContext context)
        {
            _context = context;
        }

        public async Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel)
        {
            //bikeViewModel.Id
            var bike = await _context.BikeModels
                .FirstOrDefaultAsync(b => b.Id == bikeViewModel.Id);

            if(bike == null)
            {
                throw new Exception($"Bike with id: {bikeViewModel.Id} was not found.");
            }

            bike.Name = bikeViewModel.Name;
            bike.Price = bikeViewModel.Price;
            bike.ImageUrl = bikeViewModel.ImageUrl;
            bike.Description = bikeViewModel.Description;
            bike.Year = bikeViewModel.Year;
            bike.Brand = bikeViewModel.Brand;

            _context.Update(bike);
            await _context.SaveChangesAsync();

            return bikeViewModel;
        }
        public async Task<BikeViewModel?> DeleteBike(int id)
        {
            var bike = await _context.BikeModels.FindAsync(id);
            if (bike != null)
            {
                _context.BikeModels.Remove(bike);
                await _context.SaveChangesAsync();
            }

            return ToViewModel(bike);
        }

        public async Task<BikeViewModel?> GetBike(int id)
        {
            var bike = await _context.BikeModels.FirstOrDefaultAsync(m => m.Id == id);

            return ToViewModel(bike);
        }

        public async Task<BikeViewModel?> GetBikeModelDetails(int id)
        {
            var bike = await _context.BikeModels.FirstOrDefaultAsync(m => m.Id == id);

            return ToViewModel(bike);
        }
        public async Task<List<BikeViewModel>> GetBikeModelIndex()
        {
            var bikes = await _context.BikeModels.ToListAsync();

            return bikes.Select(ToViewModel).ToList();
        }

        public async Task<BikeViewModel?> GetShowcaseDetail(int id)
        {

            var bike = await _context.BikeModels
                .Include(b => b.Inventory) 
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bike == null) return null;

            return ToViewModel(bike);
        }

        public async Task<List<BikeViewModel>> GetShowcase(string searchString)
        {
            var bikes = _context.BikeModels
            .Include(b => b.Inventory)
            .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bikes = bikes.Where(b =>
        b.Name.ToLower().Contains(searchString.ToLower()) ||
        b.Brand.ToLower().Contains(searchString.ToLower()));
            }

            return bikes.Select(ToViewModel).ToList();
        }

        public async Task<List<BikeViewModel>> BikeInventory()
        {
            var bikes = await _context.BikeModels
            .Include(b => b.Inventory) // include stock
            .ToListAsync();

            var result = bikes.Select(ToViewModel).ToList();

            return result;
        }
    }
}
