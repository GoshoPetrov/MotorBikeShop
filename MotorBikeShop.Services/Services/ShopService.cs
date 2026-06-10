using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;
using System.Security.Claims;
using System.Text;

namespace MotorBikeShop.Services
{
    
    public class ShopService : IShopService
    {
        private readonly MotorBikeShopContext _context;

        private readonly ICurrentUserService _currentUser;

        private readonly ILogger<ShopService> _logger;

        public ShopService(MotorBikeShopContext context, ICurrentUserService currentUser, ILogger<ShopService> logger)
        {
            _context = context;
            _currentUser = currentUser;
            _logger = logger;
        }

        private static BasketItemViewModel ToViewModel(BasketItem db)
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
        private static BasketViewModel ToViewModel(Basket db)
        {
            return new BasketViewModel()
            {
                Id = db.Id,
                UserId = db.UserId,
                Items = db.Items.Select(ToViewModel).ToArray()
            };
        }

        private static BikeViewModel ToViewModel(BikeModel db)
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
                    throw new OutOfStockException($"Not enough stock for {item.BikeModel.Name}");
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

            _logger.LogInformation("Purchase confirmed for user {UserId}, vent {VentId}, total {TotalPrice}", userId, vent.Id, vent.TotalPrice);

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
                    _logger.LogInformation("Stock updated for bike {BikeModelId}: reduced by {Quantity}, new stock {NewStock}", item.BikeModelId, item.Quantity, inventory.Quantity);
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
                throw new OutOfStockException("Cannot add more than available stock.");
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
                throw new OutOfStockException();
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
                    throw new OutOfStockException("Cannot add more.");
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
            await _context.SaveChangesAsync();
            return ToViewModel(newBasket);
            
        }

        public async Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel)
        {
            //bikeViewModel.Id
            var bike = await _context.BikeModels
                .Include(b => b.Inventory)
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

            // Update inventory if InventoryQuantity is provided
            if (bikeViewModel.InventoryQuantity.HasValue)
            {
                if (bike.Inventory == null)
                {
                    bike.Inventory = new Inventory
                    {
                        BikeModelId = bike.Id,
                        Quantity = bikeViewModel.InventoryQuantity.Value
                    };
                    _context.Inventories.Add(bike.Inventory);
                }
                else
                {
                    bike.Inventory.Quantity = bikeViewModel.InventoryQuantity.Value;
                }
            }

            _context.Update(bike);
            await _context.SaveChangesAsync();

            return bikeViewModel;
        }

        /// <inheritdoc />
        public async Task<BikeViewModel> UpdateBikeFieldAsync(int bikeId, string field, string value)
        {
            var bike = await _context.BikeModels
                .Include(b => b.Inventory)
                .FirstOrDefaultAsync(b => b.Id == bikeId);

            if (bike == null)
                throw new ShopException($"Bike with ID {bikeId} not found.");

            // Parse and apply the field change
            switch (field)
            {
                case "Name":
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ShopException("Name cannot be empty.");
                    if (value.Length > 100)
                        throw new ShopException("Name must be at most 100 characters.");
                    bike.Name = value;
                    break;

                case "Brand":
                    if (string.IsNullOrWhiteSpace(value))
                        throw new ShopException("Brand cannot be empty.");
                    if (value.Length > 100)
                        throw new ShopException("Brand must be at most 100 characters.");
                    bike.Brand = value;
                    break;

                case "Year":
                    if (!int.TryParse(value, out var year))
                        throw new ShopException("Year must be a valid integer.");
                    if (year < 1900 || year > 2100)
                        throw new ShopException("Year must be between 1900 and 2100.");
                    bike.Year = year;
                    break;

                case "Price":
                    if (!decimal.TryParse(value, out var price))
                        throw new ShopException("Price must be a valid number.");
                    if (price < 0)
                        throw new ShopException("Price cannot be negative.");
                    bike.Price = price;
                    break;

                case "Stock":
                    if (!int.TryParse(value, out var quantity))
                        throw new ShopException("Stock must be a valid integer.");
                    if (quantity < 0)
                        throw new ShopException("Stock cannot be negative.");

                    if (bike.Inventory == null)
                    {
                        bike.Inventory = new Inventory
                        {
                            BikeModelId = bike.Id,
                            Quantity = quantity
                        };
                        _context.Inventories.Add(bike.Inventory);
                    }
                    else
                    {
                        bike.Inventory.Quantity = quantity;
                    }
                    break;

                default:
                    throw new ShopException($"Unknown field: {field}");
            }

            await _context.SaveChangesAsync();

            return ToViewModel(bike);
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

            var viewModel = ToViewModel(bike);
            viewModel.Comments = await GetCommentsForBikeAsync(id);
            return viewModel;
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

            return await bikes.Select(b => ToViewModel(b)).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<ShowcaseViewModel> GetShowcaseAsync(
            string? searchString,
            string? sortBy,
            bool ascending,
            int pageNumber,
            int pageSize)
        {
            // Guard: clamp page number
            if (pageNumber < 1)
                pageNumber = 1;

            // Guard: clamp page size
            if (pageSize < 1)
                pageSize = 6;
            if (pageSize > 100)
                pageSize = 100;

            // 1. Base query
            var query = _context.BikeModels
                .Include(b => b.Inventory)
                .AsQueryable();

            // 2. Apply search filter
            if (!string.IsNullOrWhiteSpace(searchString))
            {
                var term = searchString.ToLower();
                query = query.Where(b =>
                    b.Name.ToLower().Contains(term) ||
                    b.Brand.ToLower().Contains(term));
            }

            // 3. Count total matching items (before sorting/paging)
            var totalItems = await query.CountAsync();

            // 4. Apply sorting
            query = (sortBy?.ToLowerInvariant()) switch
            {
                "brand" => ascending
                    ? query.OrderBy(b => b.Brand)
                    : query.OrderByDescending(b => b.Brand),
                "price" => ascending
                    ? query.OrderBy(b => b.Price)
                    : query.OrderByDescending(b => b.Price),
                "year" => ascending
                    ? query.OrderBy(b => b.Year)
                    : query.OrderByDescending(b => b.Year),
                _ => ascending
                    ? query.OrderBy(b => b.Name)
                    : query.OrderByDescending(b => b.Name),
            };

            // 5. Apply paging
            var bikes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => ToViewModel(b))
                .ToListAsync();

            // 6. Return view model
            return new ShowcaseViewModel
            {
                Bikes = bikes,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                SortBy = sortBy ?? "Name",
                Ascending = ascending,
            };
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
                _logger.LogError(ex, "CSV import failed at line {LineNo}", lineNo);
                throw new ShopException($"Something went wrong at line {lineNo}!");
            }
        }

        // ── Comment Methods ────────────────────────────────────────────────────

        public async Task<List<CommentViewModel>> GetCommentsForBikeAsync(int bikeModelId, CancellationToken ct = default)
        {
            var comments = await _context.Comments
                .AsNoTracking()
                .Where(c => c.BikeModelId == bikeModelId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(ct);

            var currentUserId = _currentUser.UserId;
            var isAdmin = _currentUser.IsAdmin;

            // Load user data for each comment (if available via navigation)
            var userIds = comments.Select(c => c.UserId).Distinct().ToList();
            var users = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u.UserName ?? "Unknown", ct);

            return comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                BikeModelId = c.BikeModelId,
                Content = c.Content,
                AuthorUserName = users.GetValueOrDefault(c.UserId, "Unknown"),
                AuthorId = c.UserId,
                CreatedAt = c.CreatedAt,
                CanDelete = c.UserId == currentUserId || isAdmin
            }).ToList();
        }

        public async Task<CommentViewModel> AddCommentAsync(int bikeModelId, string content, CancellationToken ct = default)
        {
            var bikeExists = await _context.BikeModels.AnyAsync(b => b.Id == bikeModelId, ct);
            if (!bikeExists)
                throw new ShopException("Bike not found.");

            var userId = GetUserId();

            var comment = new Comment
            {
                BikeModelId = bikeModelId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync(ct);

            // Reload with user data for the view model
            await _context.Entry(comment).Reference(c => c.User).LoadAsync(ct);

            return new CommentViewModel
            {
                Id = comment.Id,
                BikeModelId = comment.BikeModelId,
                Content = comment.Content,
                AuthorUserName = comment.User?.UserName ?? "Unknown",
                AuthorId = comment.UserId,
                CreatedAt = comment.CreatedAt,
                CanDelete = true // the author just created it
            };
        }

        public async Task DeleteCommentAsync(int commentId, CancellationToken ct = default)
        {
            var comment = await _context.Comments.FindAsync(new object[] { commentId }, ct);
            if (comment == null)
                throw new ShopException("Comment not found.");

            var userId = GetUserId();
            var isAdmin = _currentUser.IsAdmin;

            if (comment.UserId != userId && !isAdmin)
                throw new UnauthorizedAccessException("You do not have permission to delete this comment.");

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync(ct);
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
