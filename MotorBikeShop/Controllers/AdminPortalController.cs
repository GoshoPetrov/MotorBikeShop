using System.Text;
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
        var csv = await _shopService.ExportBikesCsv();

        var bytes = Encoding.UTF8.GetBytes(csv);

        return File(bytes, "text/csv", "bikes.csv");
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
            TempData["Error"] = "Upload a CSV file.";
            return View();
        }

        using var reader = new StreamReader(file.OpenReadStream());
        var csv = await reader.ReadToEndAsync();

        var success = await _shopService.ImportBikesCsv(csv);

        TempData[success ? "Success" : "Error"] =
            success ? "Import successful!" : "Import failed.";

        return RedirectToAction("Index", "AdminPortal");
    }

    // LIST ALL BIKES (ADMIN VIEW)
    public async Task<IActionResult> Index()
    {

        var bikes = await _shopService.BikeInventory();

        return View(bikes);
    }

    /// <summary>
    /// AJAX endpoint: updates a single bike field inline.
    /// Returns JSON with { success: true/false, errors: [...] }.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateBikeField(int bikeId, string field, string value)
    {
        try
        {
            var updated = await _shopService.UpdateBikeFieldAsync(bikeId, field, value);
            return Json(new { success = true, bike = updated });
        }
        catch (ShopException ex)
        {
            return Json(new { success = false, errors = new[] { ex.Message } });
        }
    }
}