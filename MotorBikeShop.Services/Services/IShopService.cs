using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MotorBikeShop.Services
{
    public interface IShopService
    {
        Task ConfirmPurchase();
        Task IncreaseBasketItemQuantity(int itemId, int delta = 1);
        Task<BasketItemViewModel> RemoveFromBasket(int itemId);
        Task<BasketViewModel> GetBasket();
        Task AddItemToBasket(int bikeModelId);
        Task<BikeViewModel> UpdateBike(BikeViewModel bikeViewModel);
        Task<BikeViewModel> GetBike(int id);
        Task<BikeViewModel> DeleteBike(int id);
        Task<BikeViewModel> GetBikeModelDetails(int id);
        Task<List<BikeViewModel>> GetBikeModelIndex();
        Task<BikeViewModel> GetShowcaseDetail(int id);
        Task<List<BikeViewModel>> GetShowcase(string csv);
        Task<List<BikeViewModel>> BikeInventory();

        Task<List<BikeModel>> SearchForBikes(string term);

        Task<string> ExportBikesCsv();
        Task<bool> ImportBikesCsv(string csv);
    }
}
