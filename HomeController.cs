using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BoutiqueEnLigne.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Start()
        {
            var userId = HttpContext.Session.GetInt32("UserId");

            if (userId == null)
                return RedirectToAction("Register", "User");
            var role = HttpContext.Session.GetString("UserRole");
            if (role == "Vendeur")
                return RedirectToAction("Dashboard", "Seller");

            return RedirectToAction("Index", "Product");
        }

        public IActionResult Error404(int code)
        {
            Response.StatusCode = code;
            ViewBag.Code = code;
            return View();
        }
    }
}
