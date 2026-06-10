using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;
using System.Security.Claims;
using System.Text;

namespace MotorBikeShop.Services
{
    
    public class ShopService : IShopService
    {

        private BasketItemViewModel ToViewModel(BasketItem db)
        {
            return new BasketItemViewModel()
            {
                Id = db.Id,
                BikeModelId = db.BikeModelId,
                Quantity = db.Quantity,
                Name = db.BikeModel.Name,
                Price = db.BikeModel.Price


            };
        }
        private BasketViewModel ToViewModel(Basket db)
        {
            return new BasketViewModel()
            {
                Id = db.Id,
                UserId = db.UserId,
                Items = db.Items.Select(ToViewModel).ToArray()
            };
        }

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

        private readonly ICurrentUserService _currentUser;

        public ShopService(MotorBikeShopContext context, ICurrentUserService currentUser)
        {
            _context = context;
            _currentUser = currentUser;
        }

        public async Task<List<BikeModel>> SearchForBikes(string term)
        {
            try
            {
                var result = await _context.Inventories
                    .Where(b => b.BikeModel.Name.StartsWith(term) || b.BikeModel.Brand.StartsWith(term))
                    .Select((b) => b.BikeModel)
                    .ToListAsync();

                return result;
            }
            catch
            {
                //this is not an inportant functionality
                return new List<BikeModel>();
            }

        }

        public async Task ConfirmPurchase()
        {
            var userId = GetUserId();

            // 1. Get basket with items and bikes
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .ThenInclude(i => i.BikeModel)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null || !basket.Items.Any())
            {
                return;
            }

            // 2. VALIDATE STOCK
            foreach (var item in basket.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

                if (inventory == null || inventory.Quantity < item.Quantity)
                {
                    throw new OutOfStockExeption($"Not enough stock for {item.BikeModel.Name}");
                }
            }

            // 3. CREATE ORDER (Vent)
            var vent = new Vent
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                TotalPrice = basket.Items.Sum(i => i.Quantity * i.BikeModel.Price)
            };

            _context.Vents.Add(vent);
            await _context.SaveChangesAsync();

            // 4. CREATE VENT ITEMS
            foreach (var item in basket.Items)
            {
                _context.VentItems.Add(new VentItem
                {
                    VentId = vent.Id,
                    BikeModelId = item.BikeModelId,
                    Quantity = item.Quantity,
                    Price = item.BikeModel.Price
                });
            }

            // 5. REDUCE INVENTORY
            foreach (var item in basket.Items)
            {
                var inventory = await _context.Inventories
                    .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

                if (inventory != null)
                {
                    inventory.Quantity -= item.Quantity;
                }
            }

            // 6. CLEAR BASKET
            _context.BasketItems.RemoveRange(basket.Items);

            await _context.SaveChangesAsync();
        }

        public async Task IncreaseBasketItemQuantity(int itemId, int delta = 1)
        {
            // 1. Get basket item
            var item = await _context.BasketItems
                .FirstOrDefaultAsync(i => i.Id == itemId);

            if (item == null)
            {
                return;
            }

            // 2. Get inventory for this bike
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

            if (inventory == null)
            {
                throw new InventoryNotFoundException();
                
            }

            // 3. Check stock limit
            if (item.Quantity + delta >= 0 
                && item.Quantity + delta <= inventory.Quantity)
            {
                item.Quantity += delta;

                if(item.Quantity == 0)
                {
                    _context.BasketItems.Remove(item);
                }
                await _context.SaveChangesAsync();
            }
            else
            {
                throw new OutOfStockExeption("Cannot add more than available stock.");
            }

        }

        public async Task AddItemToBasket(int bikeModelId)
        {
            var userId = GetUserId();

            // 1. Get inventory for the bike
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.BikeModelId == bikeModelId);

            // ❌ If no stock or bike doesn't exist
            if (inventory == null || inventory.Quantity <= 0)
            {
                throw new OutOfStockExeption();
            }

            // 2. Get or create basket
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket == null)
            {
                basket = new Basket
                {
                    UserId = userId,
                    Items = new List<BasketItem>()
                };

                _context.Baskets.Add(basket);
                await _context.SaveChangesAsync();
            }

            // 3. Check if item already exists
            var existingItem = basket.Items
                .FirstOrDefault(i => i.BikeModelId == bikeModelId);

            if (existingItem != null)
            {
                // ✅ Only increase if stock allows
                if (existingItem.Quantity < inventory.Quantity)
                {
                    existingItem.Quantity++;
                }
                else
                {
                    throw new OutOfStockExeption("Cannot add more.");
                }
            }
            else
            {
                // ✅ Add new item
                basket.Items.Add(new BasketItem
                {
                    BikeModelId = bikeModelId,
                    Quantity = 1
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<BasketItemViewModel> RemoveFromBasket(int itemId)
        {
            var item = await _context.BasketItems.FindAsync(itemId);

            if (item != null)
            {
                _context.BasketItems.Remove(item);
                await _context.SaveChangesAsync();
                return ToViewModel(item);
            }

            return null;
        }

        private string GetUserId()
        {
            var userId = _currentUser.UserId;

            if(userId == null)
            {
                throw new Exception("Login to view your basket.");
            }
            return userId;
        }

        public async Task<BasketViewModel> GetBasket()
        {
            var userId = GetUserId();

            var basket = await _context.Baskets
                .Include(b => b.Items)
                .ThenInclude(i => i.BikeModel)
                .FirstOrDefaultAsync(b => b.UserId == userId);

            if (basket != null) return ToViewModel(basket);

            var newBasket = new Basket()
            {
                UserId = userId
            };

            _context.Baskets.Add(newBasket);
            _context.SaveChanges();
            return ToViewModel(newBasket);
            
        }

        public async Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel)
        {
            //bikeViewModel.Id
            var bike = await _context.BikeModels
                .FirstOrDefaultAsync(b => b.Id == bikeViewModel.Id);

            if (bike == null)
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
                return ToViewModel(bike);
            }

            return null;
            
        }

        public async Task<BikeViewModel?> GetBike(int id)
        {
            var bike = await _context.BikeModels.FirstOrDefaultAsync(m => m.Id == id);

            if(bike == null)
            {
                return null;
            }
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

        public async Task<string> ExportBikesCsv()
        {
            var bikes = await BikeInventory();

            var sb = new StringBuilder();

            // Header
            sb.AppendLine("Id,Name,Brand,Year,Price,Description,InventoryQuantity,ImageUrl");

            foreach (var b in bikes)
            {
                sb.AppendLine($"{b.Id}," +
                              $"{Escape(b.Name)}," +
                              $"{Escape(b.Brand)}," +
                              $"{b.Year}," +
                              $"{b.Price}," +
                              $"{Escape(b.Description)}," +
                              $"{b.InventoryQuantity}," +
                              $"{Escape(b.ImageUrl)}");
            }

            return sb.ToString();
        }

        public async Task<bool> ImportBikesCsv(string csv)
        {
            int lineNo = 1; // header is at 1
            try
            {
                var lines = csv.Split('\n')
                               .Skip(1); // skip header

                foreach (var line in lines)
                {
                    lineNo++;
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    var values = line.Split(',');

                    if (values.Length < 8)
                        continue; // skip invalid rows

                    var id = int.Parse(values[0]);

                    var bikeVm = new BikeViewModel
                    {
                        Id = id,
                        Name = values[1],
                        Brand = values[2],
                        Year = int.Parse(values[3]),
                        Price = decimal.Parse(values[4]),
                        Description = values[5],
                        InventoryQuantity = int.TryParse(values[6], out var q) ? q : 0,
                        ImageUrl = values[7]
                    };

                    // 🔍 CHECK IF EXISTS
                    var existing = await _context.BikeModels
                        .Include(b => b.Inventory)
                        .FirstOrDefaultAsync(b => b.Id == bikeVm.Id);

                    if (existing != null)
                    {
                        // 🔄 UPDATE
                        existing.Name = bikeVm.Name;
                        existing.Brand = bikeVm.Brand;
                        existing.Year = bikeVm.Year;
                        existing.Price = bikeVm.Price;
                        existing.Description = bikeVm.Description;
                        if (!string.IsNullOrWhiteSpace(bikeVm.ImageUrl))
                            existing.ImageUrl = bikeVm.ImageUrl;

                        // inventory safe update
                        existing.Inventory ??= new Inventory();
                        existing.Inventory.Quantity = bikeVm.InventoryQuantity ?? 0;
                    }
                    else
                    {
                        // ➕ INSERT
                        var newBike = new BikeModel
                        {
                            Id = bikeVm.Id,
                            Name = bikeVm.Name,
                            Brand = bikeVm.Brand,
                            Year = bikeVm.Year,
                            Price = bikeVm.Price,
                            Description = bikeVm.Description,
                            ImageUrl = bikeVm.ImageUrl,
                            Inventory = new Inventory
                            {
                                Quantity = bikeVm.InventoryQuantity ?? 0
                            }
                        };

                        _context.BikeModels.Add(newBike);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); // TODO: Log and analize
                throw new ShopException($"Sopmething went wrong at line {lineNo}!");
            }
        }

        private string Escape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";

            if (value.Contains(",") || value.Contains("\""))
            {
                return $"\"{value.Replace("\"", "\"\"")}\"";
            }

            return value;
        }
    }
}
