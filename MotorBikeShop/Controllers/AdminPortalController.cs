using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Data;
using MotorBikeShop.Services;

[Authorize(Roles = "Admin")]
public class AdminPortalController : Controller
{
    private readonly IShopService _shopService;

    public AdminPortalController(IShopService shopService)
    {
        _shopService = shopService;
    }

    // LIST ALL BIKES (ADMIN VIEW)
    public async Task<IActionResult> Index()
    {

        var bikes = await _shopService.BikeInventory();

        return View(bikes);
    }
}