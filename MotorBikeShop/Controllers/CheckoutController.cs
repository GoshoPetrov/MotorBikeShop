using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MotorBikeShop.Areas.Identity.Data.Entities;
using MotorBikeShop.Data;
using MotorBikeShop.Services;

[Authorize]
public class CheckoutController : Controller
{
    private readonly IShopService _shopService;

    public CheckoutController(IShopService shopService)
    {
        _shopService = shopService;
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
        try
        {
            await _shopService.ConfirmPurchase();
            return RedirectToAction("Success");
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

        return RedirectToAction("Index");
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