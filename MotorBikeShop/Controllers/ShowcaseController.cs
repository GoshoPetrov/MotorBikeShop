using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MotorBikeShop.Data;

[Authorize] // only logged-in users can see bikes
public class ShowcaseController : Controller
{
    private readonly MotorBikeShopContext _context;

    public ShowcaseController(MotorBikeShopContext context)
    {
        _context = context;
    }

    // GET: Showcase
    public async Task<IActionResult> Index()
    {
        var bikes = await _context.BikeModels.ToListAsync();
        return View(bikes);
    }

    // GET: Showcase/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var bike = await _context.BikeModels
            .FirstOrDefaultAsync(b => b.Id == id);

        if (bike == null) return NotFound();

        return View(bike);
    }
}