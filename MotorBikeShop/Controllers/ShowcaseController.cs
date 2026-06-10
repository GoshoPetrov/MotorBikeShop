using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using MotorBikeShop.Data;
using MotorBikeShop.Services;

namespace MotorBikeShop.Controllers;

[Authorize] // only logged-in users can see bikes
public class ShowcaseController : Controller
{
    private readonly IShopService _shopService;

    public ShowcaseController(IShopService shopService)
    {
        _shopService = shopService;
    }

    // GET: Showcase
    public async Task<IActionResult> Index(
        string? searchString,
        string? sortBy,
        bool ascending = true,
        int pageNumber = 1,
        int pageSize = 6,
        string? message = null)
    {
        var model = await _shopService.GetShowcaseAsync(
            searchString, sortBy, ascending, pageNumber, pageSize);

        ViewBag.Message = message;
        return View(model);
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