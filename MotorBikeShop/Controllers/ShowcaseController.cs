using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MotorBikeShop.Data;
using MotorBikeShop.Services;

[Authorize] // only logged-in users can see bikes
public class ShowcaseController : Controller
{
    private readonly IShopService _shopService;

    public ShowcaseController(IShopService shopService)
    {
        _shopService = shopService;
    }

    // GET: Showcase
    public async Task<IActionResult> Index(string searchString)
    {
        var bikes = await _shopService.GetShowcase(searchString);

        return View(bikes);
    }

    // GET: Showcase/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var bike = await _shopService.GetShowcaseDetail(id);

        if(bike == null)
        {
            return NotFound();
        }

        return View(bike);
    }
}