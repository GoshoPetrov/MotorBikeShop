using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MotorBikeShop.Services;


[Authorize]
public class BasketController : Controller
{

    private readonly IShopService _shopService;
    public BasketController(IShopService shopService)
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
        try
        {
            await _shopService.AddItemToBasket(bikeModelId); 
            
        }
        catch (ShopException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            //TODO: log the error somewhere
            TempData["Error"] = "Something went wrong. Try again later";
        }

       return RedirectToAction("Index", "Showcase");
    }

    // POST: Remove item
    [HttpPost]
    public async Task<IActionResult> Remove(int itemId)
    {
        var item = await _shopService.RemoveFromBasket(itemId);

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
        try
        {
            await _shopService.IncreaseBasketItemQuantity(itemId, 1);
        }
        catch (ShopException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Something went wrong. Try again later";
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Decrease(int itemId)
    {
        try
        {
            await _shopService.IncreaseBasketItemQuantity(itemId, -1);
        }
        catch (ShopException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (Exception)
        {
            TempData["Error"] = "Something went wrong. Try again later";
        }
        return RedirectToAction(nameof(Index));
    }
}