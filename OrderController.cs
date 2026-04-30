using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Controllers
{
    [RequireLogin]
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;
        public OrderController(ApplicationDbContext context) => _context = context;

       
        public IActionResult Invoice(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var order = _context.Orders
                .Include(o => o.Client)
                .Include(o => o.Items)
                .ThenInclude(i => i.Product)
                .Include(o => o.Items)
                .ThenInclude(i => i.Seller)
                .FirstOrDefault(o => o.Id == id && o.ClientId == userId);

            if (order == null) return NotFound();

            return View(order);
        }

        public IActionResult MyOrders()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var orders = _context.Orders
                .Where(o => o.ClientId == userId)
                .OrderByDescending(o => o.Id)
                .ToList();
            ViewBag.TotalSpent = orders.Sum(o => o.TotalAmount);

            return View(orders);
        }
    }
}