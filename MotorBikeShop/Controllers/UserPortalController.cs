using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "User,Admin")] // users AND admins can access user portal
public class UserPortalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Example: view basket
    public IActionResult MyBasket()
    {
        return View();
    }
}