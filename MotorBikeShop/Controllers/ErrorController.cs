using Microsoft.AspNetCore.Mvc;

namespace MotorBikeShop.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("Error/{statusCode}")]
        public IActionResult StatusCode(int statusCode)
        {
            return View("StatusCode", statusCode);
        }
    }
}
