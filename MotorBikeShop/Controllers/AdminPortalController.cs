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

    // 📤 EXPORT
    public async Task<IActionResult> Export()
    {
        var json = await _shopService.ExportBikes();

        var bytes = System.Text.Encoding.UTF8.GetBytes(json);

        return File(bytes, "application/json", "bikes.json");
    }

    // 📥 IMPORT PAGE
    public IActionResult Import()
    {
        return View();
    }

    // 📥 IMPORT POST
    [HttpPost]
    public async Task<IActionResult> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please upload a file.";
            return View();
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var json = await reader.ReadToEndAsync();

        var success = await _shopService.ImportBikes(json);

        if (!success)
        {
            TempData["Error"] = "Import failed.";
        }
        else
        {
            TempData["Success"] = "Import successful!";
        }

        return RedirectToAction("Bikes");
    }

    // LIST ALL BIKES (ADMIN VIEW)
    public async Task<IActionResult> Index()
    {

        var bikes = await _shopService.BikeInventory();

        return View(bikes);
    }
}