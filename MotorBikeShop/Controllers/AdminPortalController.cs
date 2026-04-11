using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MotorBikeShop.Data;

[Authorize(Roles = "Admin")]
public class AdminPortalController : Controller
{
    private readonly MotorBikeShopContext _context;

    public AdminPortalController(MotorBikeShopContext context)
    {
        _context = context;
    }

    // LIST ALL BIKES (ADMIN VIEW)
    public async Task<IActionResult> Index()
    {
        var bikes = await _context.BikeModels
            .Include(b => b.Inventory) // include stock
            .ToListAsync();

        return View(bikes);
    }
}