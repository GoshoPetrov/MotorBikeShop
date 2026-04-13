using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MotorBikeShop.Data;
using MotorBikeShop.Services;
using AspNetCoreGeneratedDocument;

[Authorize]
public class BasketController : Controller
{

    private readonly ShopService _shopService;
    public BasketController(ShopService shopService)
    {
        _shopService = shopService;
    }

    // GET: Basket
    public async Task<IActionResult> Index()
    {
        var userId = GetUserId();

        var basket = await _shopService.GetBasket();

        return View(basket);
    }

    // POST: Add to basket
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(int bikeModelId)
    {
        var userId = GetUserId();

        // 1. Get inventory for the bike
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.BikeModelId == bikeModelId);

        // ❌ If no stock or bike doesn't exist
        if (inventory == null || inventory.Quantity <= 0)
        {
            TempData["Error"] = "This bike is out of stock.";
            return RedirectToAction("Index", "Showcase");
        }

        // 2. Get or create basket
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (basket == null)
        {
            basket = new Basket
            {
                UserId = userId,
                Items = new List<BasketItem>()
            };

            _context.Baskets.Add(basket);
            await _context.SaveChangesAsync();
        }

        // 3. Check if item already exists
        var existingItem = basket.Items
            .FirstOrDefault(i => i.BikeModelId == bikeModelId);

        if (existingItem != null)
        {
            // ✅ Only increase if stock allows
            if (existingItem.Quantity < inventory.Quantity)
            {
                existingItem.Quantity++;
            }
            else
            {
                TempData["Error"] = "Cannot add more than available stock.";
            }
        }
        else
        {
            // ✅ Add new item
            basket.Items.Add(new BasketItem
            {
                BikeModelId = bikeModelId,
                Quantity = 1
            });
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Showcase");
    }

    // POST: Remove item
    [HttpPost]
    public async Task<IActionResult> Remove(int itemId)
    {
        var item = await _context.BasketItems.FindAsync(itemId);

        if (item != null)
        {
            _context.BasketItems.Remove(item);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private string GetUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)!;
    }

    [HttpPost]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Increase(int itemId)
    {
        // 1. Get basket item
        var item = await _context.BasketItems
            .FirstOrDefaultAsync(i => i.Id == itemId);

        if (item == null)
        {
            return RedirectToAction(nameof(Index));
        }

        // 2. Get inventory for this bike
        var inventory = await _context.Inventories
            .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

        if (inventory == null)
        {
            TempData["Error"] = "Inventory not found.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Check stock limit
        if (item.Quantity < inventory.Quantity)
        {
            item.Quantity++;
            await _context.SaveChangesAsync();
        }
        else
        {
            TempData["Error"] = "Cannot add more than available stock.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Decrease(int itemId)
    {
        var item = await _context.BasketItems.FindAsync(itemId);

        if (item != null)
        {
            item.Quantity--;

            if (item.Quantity <= 0)
            {
                _context.BasketItems.Remove(item);
            }

            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}