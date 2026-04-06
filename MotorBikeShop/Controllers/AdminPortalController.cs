using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Admin")]
public class AdminPortalController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    // Example: manage bikes
    public IActionResult ManageBikes()
    {
        return View();
    }
}