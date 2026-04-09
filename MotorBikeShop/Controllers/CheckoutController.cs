using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;

[Authorize]
public class CheckoutController : Controller
{
    private readonly MotorBikeShopContext _context;

    public CheckoutController(MotorBikeShopContext context)
    {
        _context = context;
    }

    // GET: Checkout page
    public async Task<IActionResult> Index()
    {
        return View();
    }

    // POST: Confirm purchase
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Confirm(string fullName, string address)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        // 1. Get basket with items and bikes
        var basket = await _context.Baskets
            .Include(b => b.Items)
            .ThenInclude(i => i.BikeModel)
            .FirstOrDefaultAsync(b => b.UserId == userId);

        if (basket == null || !basket.Items.Any())
        {
            return RedirectToAction("Index", "Basket");
        }

        // 2. VALIDATE STOCK
        foreach (var item in basket.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

            if (inventory == null || inventory.Quantity < item.Quantity)
            {
                TempData["Error"] = $"Not enough stock for {item.BikeModel.Name}";
                return RedirectToAction("Index", "Basket");
            }
        }

        // 3. CREATE ORDER (Vent)
        var vent = new Vent
        {
            UserId = userId, // ✅ string, NOT int
            CreatedAt = DateTime.UtcNow,
            TotalPrice = basket.Items.Sum(i => i.Quantity * i.BikeModel.Price)
        };

        _context.Vents.Add(vent);
        await _context.SaveChangesAsync();

        // 4. CREATE VENT ITEMS
        foreach (var item in basket.Items)
        {
            _context.VentItems.Add(new VentItem
            {
                VentId = vent.Id,
                BikeModelId = item.BikeModelId,
                Quantity = item.Quantity,
                Price = item.BikeModel.Price
            });
        }

        // 5. REDUCE INVENTORY
        foreach (var item in basket.Items)
        {
            var inventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.BikeModelId == item.BikeModelId);

            if (inventory != null)
            {
                inventory.Quantity -= item.Quantity;
            }
        }

        // 6. CLEAR BASKET
        _context.BasketItems.RemoveRange(basket.Items);

        await _context.SaveChangesAsync();

        // 7. SUCCESS PAGE
        return RedirectToAction("Success");
    }

    public IActionResult Success()
    {
        return View();
    }

    public IActionResult Cancel()
    {
        return RedirectToAction("Index", "Basket");
    }
}