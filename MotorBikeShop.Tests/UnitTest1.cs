using Microsoft.EntityFrameworkCore;
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

            _shopService = new ShopService(_context, _currentUserServiceMock.Object);
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
            await Assert.ThrowsAsync<OutOfStockExeption>(() =>
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
            await Assert.ThrowsAsync<OutOfStockExeption>(() =>
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
            await Assert.ThrowsAsync<OutOfStockExeption>(() =>
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
            await Assert.ThrowsAsync<OutOfStockExeption>(() => _shopService.ConfirmPurchase());
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

        #endregion
    }
}