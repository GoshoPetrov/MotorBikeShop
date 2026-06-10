using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Models;
using MotorBikeShop.Services;
using System.Security.Claims;
using Xunit;

namespace MotorBikeShop.Tests.Services
{
    public class ShopServiceTests : IDisposable
    {
        private readonly MotorBikeShopContext _context;
        private readonly Mock<ICurrentUserService> _currentUserServiceMock;
        private readonly ShopService _shopService;
        private readonly string _testUserId = "test-user-123";

        public ShopServiceTests()
        {
            // Setup in-memory database
            var options = new DbContextOptionsBuilder<MotorBikeShopContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new MotorBikeShopContext(options);

            _currentUserServiceMock = new Mock<ICurrentUserService>();
            _currentUserServiceMock.Setup(x => x.UserId).Returns(_testUserId);

            var loggerMock = new Mock<ILogger<ShopService>>();
            _shopService = new ShopService(_context, _currentUserServiceMock.Object, loggerMock.Object);
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        #region GetBasket Tests

        [Fact]
        public async Task GetBasket_UserHasBasket_ReturnsBasketViewModel()
        {
            // Arrange
            var basket = new Basket
            {
                UserId = _testUserId,
                Items = new List<BasketItem>()
            };
            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.GetBasket();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUserId, result.UserId);
        }

        [Fact]
        public async Task GetBasket_UserHasNoBasket_CreatesAndReturnsNewBasket()
        {
            // Act
            var result = await _shopService.GetBasket();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(_testUserId, result.UserId);
            var basketInDb = await _context.Baskets.FirstOrDefaultAsync(b => b.UserId == _testUserId);
            Assert.NotNull(basketInDb);
        }

        [Fact]
        public async Task GetBasket_UserNotLoggedIn_ThrowsException()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.UserId).Returns((string)null);

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _shopService.GetBasket());
        }

        #endregion

        #region AddItemToBasket Tests

        [Fact]
        public async Task AddItemToBasket_ValidItem_AddsToBasket()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 10 };
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.AddItemToBasket(bikeModel.Id);

            // Assert
            var basket = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == _testUserId);

            Assert.NotNull(basket);
            Assert.Single(basket.Items);
            Assert.Equal(bikeModel.Id, basket.Items.First().BikeModelId);
            Assert.Equal(1, basket.Items.First().Quantity);
        }

        [Fact]
        public async Task AddItemToBasket_ItemAlreadyInBasket_IncreasesQuantity()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 10 };
            await _context.Inventories.AddAsync(inventory);

            var basket = new Basket { UserId = _testUserId, Items = new List<BasketItem>() };
            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();

            await _shopService.AddItemToBasket(bikeModel.Id); // First add

            // Act
            await _shopService.AddItemToBasket(bikeModel.Id); // Second add

            // Assert
            var basketItem = await _context.BasketItems.FirstOrDefaultAsync(i => i.BikeModelId == bikeModel.Id);
            Assert.NotNull(basketItem);
            Assert.Equal(2, basketItem.Quantity);
        }

        [Fact]
        public async Task AddItemToBasket_NoStock_ThrowsOutOfStockException()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 0 };
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OutOfStockException>(() =>
                _shopService.AddItemToBasket(bikeModel.Id));
        }

        [Fact]
        public async Task AddItemToBasket_ExceedsStock_ThrowsOutOfStockException()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 1 };
            await _context.Inventories.AddAsync(inventory);

            var basket = new Basket { UserId = _testUserId, Items = new List<BasketItem>() };
            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();

            await _shopService.AddItemToBasket(bikeModel.Id); // Uses the only stock

            // Act & Assert
            await Assert.ThrowsAsync<OutOfStockException>(() =>
                _shopService.AddItemToBasket(bikeModel.Id));
        }

        #endregion

        #region IncreaseBasketItemQuantity Tests

        [Fact]
        public async Task IncreaseBasketItemQuantity_ValidIncrease_UpdatesQuantity()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 10 };
            await _context.Inventories.AddAsync(inventory);

            var basketItem = new BasketItem { BikeModelId = bikeModel.Id, Quantity = 1 };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.IncreaseBasketItemQuantity(basketItem.Id, 2);

            // Assert
            var updatedItem = await _context.BasketItems.FindAsync(basketItem.Id);
            Assert.Equal(3, updatedItem.Quantity);
        }

        [Fact]
        public async Task IncreaseBasketItemQuantity_QuantityBecomesZero_RemovesItem()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 10 };
            await _context.Inventories.AddAsync(inventory);

            var basketItem = new BasketItem { BikeModelId = bikeModel.Id, Quantity = 1 };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.IncreaseBasketItemQuantity(basketItem.Id, -1);

            // Assert
            var deletedItem = await _context.BasketItems.FindAsync(basketItem.Id);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task IncreaseBasketItemQuantity_ExceedsStock_ThrowsOutOfStockException()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 5 };
            await _context.Inventories.AddAsync(inventory);

            var basketItem = new BasketItem { BikeModelId = bikeModel.Id, Quantity = 5 };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OutOfStockException>(() =>
                _shopService.IncreaseBasketItemQuantity(basketItem.Id, 1));
        }

        [Fact]
        public async Task IncreaseBasketItemQuantity_ItemNotFound_ReturnsGracefully()
        {
            // Act
            await _shopService.IncreaseBasketItemQuantity(99999, 1);

            // Assert - No exception thrown
            Assert.True(true);
        }

        #endregion

        #region RemoveFromBasket Tests

        [Fact]
        public async Task RemoveFromBasket_ValidItem_RemovesAndReturnsItem()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var basketItem = new BasketItem
            {
                BikeModelId = bikeModel.Id,
                Quantity = 1,
                BikeModel = bikeModel
            };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.RemoveFromBasket(basketItem.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(basketItem.Id, result.Id);
            var deletedItem = await _context.BasketItems.FindAsync(basketItem.Id);
            Assert.Null(deletedItem);
        }

        [Fact]
        public async Task RemoveFromBasket_ItemNotFound_ReturnsNullViewModel()
        {
            // Act
            var result = await _shopService.RemoveFromBasket(99999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region ConfirmPurchase Tests

        [Fact]
        public async Task ConfirmPurchase_ValidBasket_CreatesOrderAndClearsBasket()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 10 };
            await _context.Inventories.AddAsync(inventory);

            var basket = new Basket { UserId = _testUserId, Items = new List<BasketItem>() };
            await _context.Baskets.AddAsync(basket);

            var basketItem = new BasketItem
            {
                BikeModelId = bikeModel.Id,
                Quantity = 2,
                Basket = basket,
                BikeModel = bikeModel
            };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.ConfirmPurchase();

            // Assert
            var vent = await _context.Vents.FirstOrDefaultAsync(v => v.UserId == _testUserId);
            Assert.NotNull(vent);
            Assert.Equal(2 * bikeModel.Price, vent.TotalPrice);

            var ventItem = await _context.VentItems.FirstOrDefaultAsync(vi => vi.VentId == vent.Id);
            Assert.NotNull(ventItem);
            Assert.Equal(2, ventItem.Quantity);

            var updatedInventory = await _context.Inventories.FindAsync(inventory.Id);
            Assert.Equal(8, updatedInventory.Quantity);

            var basketAfterPurchase = await _context.Baskets
                .Include(b => b.Items)
                .FirstOrDefaultAsync(b => b.UserId == _testUserId);
            Assert.Empty(basketAfterPurchase.Items);
        }

        [Fact]
        public async Task ConfirmPurchase_EmptyBasket_ReturnsWithoutChanges()
        {
            // Arrange
            var basket = new Basket { UserId = _testUserId, Items = new List<BasketItem>() };
            await _context.Baskets.AddAsync(basket);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.ConfirmPurchase();

            // Assert
            var vents = await _context.Vents.ToListAsync();
            Assert.Empty(vents);
        }

        [Fact]
        public async Task ConfirmPurchase_InsufficientStock_ThrowsOutOfStockException()
        {
            // Arrange
            var bikeModel = await CreateTestBikeModel();
            var inventory = new Inventory { BikeModelId = bikeModel.Id, Quantity = 1 };
            await _context.Inventories.AddAsync(inventory);

            var basket = new Basket { UserId = _testUserId, Items = new List<BasketItem>() };
            await _context.Baskets.AddAsync(basket);

            var basketItem = new BasketItem
            {
                BikeModelId = bikeModel.Id,
                Quantity = 5,
                Basket = basket,
                BikeModel = bikeModel
            };
            await _context.BasketItems.AddAsync(basketItem);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<OutOfStockException>(() => _shopService.ConfirmPurchase());
        }

        #endregion

        #region GetShowcase Tests

        [Fact]
        public async Task GetShowcase_NoSearchString_ReturnsAllBikes()
        {
            // Arrange
            var bike1 = await CreateTestBikeModel("Yamaha R1", "Yamaha");
            var bike2 = await CreateTestBikeModel("Honda CBR", "Honda");
            await AddInventoryForBike(bike1.Id, 5);
            await AddInventoryForBike(bike2.Id, 3);

            // Act
            var result = await _shopService.GetShowcase(null);

            // Assert
            Assert.Equal(2, result.Count);
        }

        [Fact]
        public async Task GetShowcase_WithSearchString_FiltersByName()
        {
            // Arrange
            var bike1 = await CreateTestBikeModel("Yamaha R1", "Yamaha");
            var bike2 = await CreateTestBikeModel("Honda CBR", "Honda");
            await AddInventoryForBike(bike1.Id, 5);
            await AddInventoryForBike(bike2.Id, 3);

            // Act
            var result = await _shopService.GetShowcase("Yamaha");

            // Assert
            Assert.Single(result);
            Assert.Equal("Yamaha R1", result[0].Name);
        }

        [Fact]
        public async Task GetShowcase_WithSearchString_FiltersByBrand()
        {
            // Arrange
            var bike1 = await CreateTestBikeModel("Yamaha R1", "Yamaha");
            var bike2 = await CreateTestBikeModel("Honda CBR", "Honda");
            await AddInventoryForBike(bike1.Id, 5);
            await AddInventoryForBike(bike2.Id, 3);

            // Act
            var result = await _shopService.GetShowcase("Honda");

            // Assert
            Assert.Single(result);
            Assert.Equal("Honda CBR", result[0].Name);
        }

        #endregion

        #region GetShowcaseAsync Tests

        [Fact]
        public async Task GetShowcaseAsync_DefaultPaging_ReturnsFirstPage()
        {
            // Arrange: seed 13 bikes (A-M named so alphabetical order is predictable)
            await Seed13BikesAsync();

            // Act: default params (page 1, pageSize 6, sort by Name asc)
            var result = await _shopService.GetShowcaseAsync(null, null, true, 1, 6);

            // Assert
            Assert.Equal(6, result.Bikes.Count);
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(13, result.TotalItems);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal("Bike A", result.Bikes[0].Name);
            Assert.Equal("Bike F", result.Bikes[5].Name);
        }

        [Fact]
        public async Task GetShowcaseAsync_SecondPage_ReturnsCorrectSlice()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act
            var result = await _shopService.GetShowcaseAsync(null, null, true, 2, 6);

            // Assert
            Assert.Equal(6, result.Bikes.Count);
            Assert.Equal(2, result.PageNumber);
            Assert.Equal("Bike G", result.Bikes[0].Name);
            Assert.Equal("Bike L", result.Bikes[5].Name);
        }

        [Fact]
        public async Task GetShowcaseAsync_LastPage_PartialPage()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: page 3 with 6 per page should have 1 item (bike 13 of 13)
            var result = await _shopService.GetShowcaseAsync(null, null, true, 3, 6);

            // Assert
            Assert.Single(result.Bikes);
            Assert.Equal("Bike M", result.Bikes[0].Name);
            Assert.Equal(3, result.PageNumber);
            Assert.Equal(13, result.TotalItems);
        }

        [Fact]
        public async Task GetShowcaseAsync_SortByNameDesc_ReturnsDescending()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act
            var result = await _shopService.GetShowcaseAsync(null, "Name", false, 1, 13);

            // Assert
            Assert.Equal(13, result.Bikes.Count);
            Assert.Equal("Bike M", result.Bikes[0].Name);
            Assert.Equal("Bike A", result.Bikes[12].Name);
            Assert.False(result.Ascending);
        }

        [Fact]
        public async Task GetShowcaseAsync_SortByPriceAsc_ReturnsCheapestFirst()
        {
            // Arrange: create bikes with varying prices
            var bike1 = await CreateTestBikeModel("Expensive", "BrandX", 2023, 30000);
            var bike2 = await CreateTestBikeModel("Cheap", "BrandY", 2023, 10000);
            var bike3 = await CreateTestBikeModel("Mid", "BrandZ", 2023, 20000);
            await AddInventoryForBike(bike1.Id, 5);
            await AddInventoryForBike(bike2.Id, 5);
            await AddInventoryForBike(bike3.Id, 5);

            // Act
            var result = await _shopService.GetShowcaseAsync(null, "Price", true, 1, 10);

            // Assert
            Assert.Equal(3, result.Bikes.Count);
            Assert.Equal("Cheap", result.Bikes[0].Name);
            Assert.Equal("Mid", result.Bikes[1].Name);
            Assert.Equal("Expensive", result.Bikes[2].Name);
        }

        [Fact]
        public async Task GetShowcaseAsync_SortByYearDesc_ReturnsNewestFirst()
        {
            // Arrange: create bikes with varying years
            var bike1 = await CreateTestBikeModel("Old", "BrandA", 2010, 10000);
            var bike2 = await CreateTestBikeModel("New", "BrandB", 2023, 20000);
            var bike3 = await CreateTestBikeModel("Classic", "BrandC", 2000, 15000);
            await AddInventoryForBike(bike1.Id, 5);
            await AddInventoryForBike(bike2.Id, 5);
            await AddInventoryForBike(bike3.Id, 5);

            // Act
            var result = await _shopService.GetShowcaseAsync(null, "Year", false, 1, 10);

            // Assert
            Assert.Equal(3, result.Bikes.Count);
            Assert.Equal("New", result.Bikes[0].Name);
            Assert.Equal("Old", result.Bikes[1].Name);
            Assert.Equal("Classic", result.Bikes[2].Name);
        }

        [Fact]
        public async Task GetShowcaseAsync_WithSearchString_FiltersAndPages()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: search for "Bike A" — should match Bike A, Bike B? No, just "Bike A"
            // Actually let's filter by brand for predictability
            var yamaha1 = await CreateTestBikeModel("YZF-R1", "Yamaha", 2023, 20000);
            var yamaha2 = await CreateTestBikeModel("MT-07", "Yamaha", 2022, 15000);
            var honda1 = await CreateTestBikeModel("CBR600RR", "Honda", 2023, 18000);
            await AddInventoryForBike(yamaha1.Id, 5);
            await AddInventoryForBike(yamaha2.Id, 5);
            await AddInventoryForBike(honda1.Id, 5);

            // Act
            var result = await _shopService.GetShowcaseAsync("Yamaha", "Name", true, 1, 10);

            // Assert
            Assert.Equal(2, result.Bikes.Count);
            Assert.Equal(2, result.TotalItems);
            Assert.All(result.Bikes, b => Assert.Contains("Yamaha", b.Brand));
        }

        [Fact]
        public async Task GetShowcaseAsync_InvalidPageNumber_ClampsToOne()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: pageNumber=0 should clamp to 1
            var result = await _shopService.GetShowcaseAsync(null, null, true, 0, 6);

            // Assert
            Assert.Equal(1, result.PageNumber);
            Assert.Equal(6, result.Bikes.Count);
        }

        [Fact]
        public async Task GetShowcaseAsync_InvalidPageExceedsTotal_ReturnsEmpty()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: pageNumber=999 exceeds total pages
            var result = await _shopService.GetShowcaseAsync(null, null, true, 999, 6);

            // Assert
            Assert.Empty(result.Bikes);
            Assert.Equal(13, result.TotalItems);
        }

        [Fact]
        public async Task GetShowcaseAsync_PageSizeClampedToMax()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: pageSize=200 should be clamped to 100
            var result = await _shopService.GetShowcaseAsync(null, null, true, 1, 200);

            // Assert
            Assert.Equal(100, result.PageSize);
        }

        [Fact]
        public async Task GetShowcaseAsync_TotalItemsCountedCorrectly()
        {
            // Arrange
            await Seed13BikesAsync();

            // Act: page 1, pageSize 6
            var result = await _shopService.GetShowcaseAsync(null, null, true, 1, 6);

            // Assert: TotalItems reflects all 13, not the page size
            Assert.Equal(13, result.TotalItems);
            Assert.Equal(6, result.Bikes.Count);

            // Act: with search that matches only some
            var yamaha = await CreateTestBikeModel("YZF-R1", "Yamaha", 2023, 20000);
            await AddInventoryForBike(yamaha.Id, 5);

            var searchResult = await _shopService.GetShowcaseAsync("Yamaha", null, true, 1, 6);

            // Assert: TotalItems reflects filtered count
            Assert.Equal(1, searchResult.TotalItems);
        }

        [Fact]
        public async Task GetShowcaseAsync_SortByUnknownField_DefaultsToName()
        {
            // Arrange
            var bikeB = await CreateTestBikeModel("Bike B", "BrandB", 2023, 30000);
            var bikeA = await CreateTestBikeModel("Bike A", "BrandA", 2023, 10000);
            var bikeC = await CreateTestBikeModel("Bike C", "BrandC", 2023, 20000);
            await AddInventoryForBike(bikeA.Id, 5);
            await AddInventoryForBike(bikeB.Id, 5);
            await AddInventoryForBike(bikeC.Id, 5);

            // Act: invalid sort field
            var result = await _shopService.GetShowcaseAsync(null, "InvalidField", true, 1, 10);

            // Assert: sorted by Name asc (default)
            Assert.Equal(3, result.Bikes.Count);
            Assert.Equal("Bike A", result.Bikes[0].Name);
            Assert.Equal("Bike B", result.Bikes[1].Name);
            Assert.Equal("Bike C", result.Bikes[2].Name);
        }

        #endregion

        #region BikeInventory Tests

        [Fact]
        public async Task BikeInventory_ReturnsBikesWithInventory()
        {
            // Arrange
            var bike1 = await CreateTestBikeModel();
            var bike2 = await CreateTestBikeModel();
            await AddInventoryForBike(bike1.Id, 10);
            await AddInventoryForBike(bike2.Id, 5);

            // Act
            var result = await _shopService.BikeInventory();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, bike => Assert.NotNull(bike.InventoryQuantity));
        }

        #endregion

        #region UpdateBike Tests

        [Fact]
        public async Task UpdateBike_ValidBike_UpdatesSuccessfully()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            var updatedViewModel = new BikeViewModel
            {
                Id = bike.Id,
                Name = "Updated Name",
                Brand = "Updated Brand",
                Year = 2024,
                Price = 25000,
                Description = "Updated Description",
                ImageUrl = "updated.jpg"
            };

            // Act
            var result = await _shopService.UpdateBike(updatedViewModel);

            // Assert
            Assert.Equal("Updated Name", result.Name);
            var bikeInDb = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Equal("Updated Name", bikeInDb.Name);
            Assert.Equal(25000, bikeInDb.Price);
        }

        [Fact]
        public async Task UpdateBike_BikeNotFound_ThrowsException()
        {
            // Arrange
            var viewModel = new BikeViewModel { Id = 99999 };

            // Act & Assert
            await Assert.ThrowsAsync<Exception>(() => _shopService.UpdateBike(viewModel));
        }

        #endregion

        #region UpdateBikeFieldAsync Tests

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsName_UpdatesName()
        {
            // Arrange
            var bike = await CreateTestBikeModel("Old Name", "Test Brand");

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Name", "New Name");

            // Assert
            Assert.Equal("New Name", result.Name);
            var bikeInDb = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Equal("New Name", bikeInDb.Name);
        }

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsBrand_UpdatesBrand()
        {
            // Arrange
            var bike = await CreateTestBikeModel("Test Bike", "Old Brand");

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Brand", "New Brand");

            // Assert
            Assert.Equal("New Brand", result.Brand);
            var bikeInDb = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Equal("New Brand", bikeInDb.Brand);
        }

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsYear_UpdatesYear()
        {
            // Arrange
            var bike = await CreateTestBikeModel("Test Bike", "Test Brand", 2020, 15000);

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Year", "2025");

            // Assert
            Assert.Equal(2025, result.Year);
            var bikeInDb = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Equal(2025, bikeInDb.Year);
        }

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsPrice_UpdatesPrice()
        {
            // Arrange
            var bike = await CreateTestBikeModel("Test Bike", "Test Brand");

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Price", "25000.50");

            // Assert
            Assert.Equal(25000.50m, result.Price);
            var bikeInDb = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Equal(25000.50m, bikeInDb.Price);
        }

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsStock_UpdatesInventoryQuantity()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            var inventory = await AddInventoryForBike(bike.Id, 10);

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Stock", "25");

            // Assert
            Assert.Equal(25, result.InventoryQuantity);
            var inventoryInDb = await _context.Inventories.FindAsync(inventory.Id);
            Assert.Equal(25, inventoryInDb.Quantity);
        }

        [Fact]
        public async Task UpdateBikeField_WhenFieldIsStockAndInventoryMissing_CreatesInventory()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            // No inventory created for this bike

            // Act
            var result = await _shopService.UpdateBikeFieldAsync(bike.Id, "Stock", "15");

            // Assert
            Assert.Equal(15, result.InventoryQuantity);
            var inventoryInDb = await _context.Inventories
                .FirstOrDefaultAsync(i => i.BikeModelId == bike.Id);
            Assert.NotNull(inventoryInDb);
            Assert.Equal(15, inventoryInDb.Quantity);
        }

        [Fact]
        public async Task UpdateBikeField_WhenBikeNotFound_ThrowsShopException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(99999, "Name", "Test"));
            Assert.Contains("not found", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenNameIsEmpty_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "Name", ""));
            Assert.Contains("empty", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenYearIsInvalid_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "Year", "not-a-number"));
            Assert.Contains("valid integer", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenYearOutOfRange_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "Year", "1899"));
            Assert.Contains("between", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenPriceIsNegative_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "Price", "-10"));
            Assert.Contains("negative", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenStockIsNegative_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "Stock", "-5"));
            Assert.Contains("negative", ex.Message);
        }

        [Fact]
        public async Task UpdateBikeField_WhenUnknownField_ThrowsShopException()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.UpdateBikeFieldAsync(bike.Id, "InvalidField", "value"));
            Assert.Contains("Unknown", ex.Message);
        }

        #endregion

        #region DeleteBike Tests

        [Fact]
        public async Task DeleteBike_ValidBike_DeletesAndReturnsViewModel()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act
            var result = await _shopService.DeleteBike(bike.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bike.Id, result.Id);
            var deletedBike = await _context.BikeModels.FindAsync(bike.Id);
            Assert.Null(deletedBike);
        }

        [Fact]
        public async Task DeleteBike_BikeNotFound_ReturnsNull()
        {
            // Act
            var result = await _shopService.DeleteBike(99999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetBike Tests

        [Fact]
        public async Task GetBike_ValidId_ReturnsBikeViewModel()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act
            var result = await _shopService.GetBike(bike.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(bike.Id, result.Id);
            Assert.Equal(bike.Name, result.Name);
        }

        [Fact]
        public async Task GetBike_InvalidId_ReturnsNull()
        {
            // Act
            var result = await _shopService.GetBike(99999);

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region Comment Tests

        [Fact]
        public async Task GetCommentsForBikeAsync_WhenBikeHasComments_ReturnsOrderedList()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            var user2Id = "other-user-456";

            await _context.Comments.AddRangeAsync(
                new Comment { Id = 1, BikeModelId = bike.Id, UserId = _testUserId, Content = "Second", CreatedAt = DateTime.UtcNow.AddHours(-1) },
                new Comment { Id = 2, BikeModelId = bike.Id, UserId = user2Id, Content = "First", CreatedAt = DateTime.UtcNow }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.GetCommentsForBikeAsync(bike.Id);

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("First", result[0].Content);  // newest first
            Assert.Equal("Second", result[1].Content);
        }

        [Fact]
        public async Task GetCommentsForBikeAsync_WhenBikeHasNoComments_ReturnsEmptyList()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act
            var result = await _shopService.GetCommentsForBikeAsync(bike.Id);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetCommentsForBikeAsync_WhenUserIsAuthor_SetsCanDeleteTrue()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = _testUserId,
                Content = "My comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.GetCommentsForBikeAsync(bike.Id);

            // Assert
            var ownComment = Assert.Single(result);
            Assert.True(ownComment.CanDelete);
        }

        [Fact]
        public async Task GetCommentsForBikeAsync_WhenUserIsAdmin_SetsCanDeleteTrueForAll()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);

            var bike = await CreateTestBikeModel();
            var otherUserId = "other-user-456";
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = otherUserId,
                Content = "Other user's comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.GetCommentsForBikeAsync(bike.Id);

            // Assert
            var adminView = Assert.Single(result);
            Assert.True(adminView.CanDelete);
        }

        [Fact]
        public async Task GetCommentsForBikeAsync_WhenUserIsNotAuthorOrAdmin_SetsCanDeleteFalse()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

            var bike = await CreateTestBikeModel();
            var otherUserId = "other-user-456";
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = otherUserId,
                Content = "Other user's comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            var result = await _shopService.GetCommentsForBikeAsync(bike.Id);

            // Assert
            var otherComment = Assert.Single(result);
            Assert.False(otherComment.CanDelete);
        }

        [Fact]
        public async Task AddCommentAsync_ValidInput_CreatesComment()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act
            var result = await _shopService.AddCommentAsync(bike.Id, "Great bike!");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Great bike!", result.Content);
            Assert.Equal(bike.Id, result.BikeModelId);
            Assert.Equal(_testUserId, result.AuthorId);

            var commentInDb = await _context.Comments.FirstOrDefaultAsync(c => c.BikeModelId == bike.Id);
            Assert.NotNull(commentInDb);
            Assert.Equal("Great bike!", commentInDb.Content);
        }

        [Fact]
        public async Task AddCommentAsync_ValidInput_ReturnsCorrectViewModel()
        {
            // Arrange
            var bike = await CreateTestBikeModel();

            // Act
            var result = await _shopService.AddCommentAsync(bike.Id, "Nice ride!");

            // Assert
            Assert.Equal(bike.Id, result.BikeModelId);
            Assert.Equal("Nice ride!", result.Content);
            Assert.Equal(_testUserId, result.AuthorId);
            Assert.True(result.CanDelete); // author can delete
        }

        [Fact]
        public async Task AddCommentAsync_BikeNotFound_ThrowsShopException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.AddCommentAsync(99999, "Comment"));
            Assert.Equal("Bike not found.", ex.Message);
        }

        [Fact]
        public async Task DeleteCommentAsync_OwnComment_DeletesSuccessfully()
        {
            // Arrange
            var bike = await CreateTestBikeModel();
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = _testUserId,
                Content = "My comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.DeleteCommentAsync(comment.Id);

            // Assert
            var deleted = await _context.Comments.FindAsync(comment.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteCommentAsync_AdminDeletesOtherComment_DeletesSuccessfully()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(true);

            var bike = await CreateTestBikeModel();
            var otherUserId = "other-user-456";
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = otherUserId,
                Content = "Other's comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act
            await _shopService.DeleteCommentAsync(comment.Id);

            // Assert
            var deleted = await _context.Comments.FindAsync(comment.Id);
            Assert.Null(deleted);
        }

        [Fact]
        public async Task DeleteCommentAsync_NotAuthorNotAdmin_ThrowsUnauthorizedAccessException()
        {
            // Arrange
            _currentUserServiceMock.Setup(x => x.IsAdmin).Returns(false);

            var bike = await CreateTestBikeModel();
            var otherUserId = "other-user-456";
            var comment = new Comment
            {
                BikeModelId = bike.Id,
                UserId = otherUserId,
                Content = "Other's comment",
                CreatedAt = DateTime.UtcNow
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                _shopService.DeleteCommentAsync(comment.Id));
        }

        [Fact]
        public async Task DeleteCommentAsync_CommentNotFound_ThrowsShopException()
        {
            // Act & Assert
            var ex = await Assert.ThrowsAsync<ShopException>(() =>
                _shopService.DeleteCommentAsync(99999));
            Assert.Equal("Comment not found.", ex.Message);
        }

        #endregion

        #region Helper Methods

        private async Task<BikeModel> CreateTestBikeModel(string name = "Test Bike", string brand = "Test Brand")
        {
            var bike = new BikeModel
            {
                Name = name,
                Brand = brand,
                Year = 2023,
                Price = 20000,
                Description = "Test Description",
                ImageUrl = "test.jpg"
            };
            await _context.BikeModels.AddAsync(bike);
            await _context.SaveChangesAsync();
            return bike;
        }

        private async Task<BikeModel> CreateTestBikeModel(string name, string brand, int year, decimal price)
        {
            var bike = new BikeModel
            {
                Name = name,
                Brand = brand,
                Year = year,
                Price = price,
                Description = "Test Description",
                ImageUrl = "test.jpg"
            };
            await _context.BikeModels.AddAsync(bike);
            await _context.SaveChangesAsync();
            return bike;
        }

        private async Task<Inventory> AddInventoryForBike(int bikeModelId, int quantity)
        {
            var inventory = new Inventory
            {
                BikeModelId = bikeModelId,
                Quantity = quantity
            };
            await _context.Inventories.AddAsync(inventory);
            await _context.SaveChangesAsync();
            return inventory;
        }

        /// <summary>
        /// Seeds 13 bikes named "Bike A" through "Bike M" with inventory, sorted alphabetically by name.
        /// Used for paging tests (2 full pages of 6 + 1 extra).
        /// </summary>
        private async Task Seed13BikesAsync()
        {
            for (int i = 0; i < 13; i++)
            {
                var letter = (char)('A' + i);
                var bike = new BikeModel
                {
                    Name = $"Bike {letter}",
                    Brand = $"Brand {letter}",
                    Year = 2020 + (i % 5),
                    Price = 15000 + (i * 1000),
                    Description = "Test Description",
                    ImageUrl = "test.jpg"
                };
                _context.BikeModels.Add(bike);
                await _context.SaveChangesAsync();

                await AddInventoryForBike(bike.Id, 10);
            }
        }

        #endregion
    }
}