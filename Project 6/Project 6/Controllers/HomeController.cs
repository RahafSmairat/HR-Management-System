using Microsoft.AspNetCore.Mvc;

namespace Project_6.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult login()
        {
            return View();
        }

    }
}
