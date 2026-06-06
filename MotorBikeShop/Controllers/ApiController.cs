using Microsoft.AspNetCore.Mvc;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Services;

namespace MotorBikeShop.Controllers
{
    public class ApiController : Controller
    {
        private readonly IShopService _shopService;

        public ApiController(IShopService shopService)
        {
            _shopService = shopService;
        }
        public async Task<IActionResult> Search(string term)
        {
            var result = await _shopService.SearchForBikes(term);


            return Json(result.Select((bike) => bike.Name).ToList());
        }
    }
}
