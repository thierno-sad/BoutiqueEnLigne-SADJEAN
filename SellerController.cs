using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Filters;
using BoutiqueEnLigne.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BoutiqueEnLigne.Controllers
{
    [RequireLogin]
    public class SellerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SellerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var myProducts = _context.Products
                .Where(p => p.SellerId == userId)
                .OrderByDescending(p => p.Id)
                .ToList();

            ViewBag.Count = myProducts.Count;
            ViewBag.TotalValue = myProducts.Sum(p => p.Price);

            return View(myProducts);

        }

        public IActionResult AddProduct()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            if (!ModelState.IsValid)
                return View(product);

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            product.SellerId = userId;

            _context.Products.Add(product);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public IActionResult EditProduct(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var product = _context.Products.FirstOrDefault(p => p.Id == id && p.SellerId == userId);
            if (product == null)
                return RedirectToAction("Dashboard");

            return View(product);
        }

        [HttpPost]
        public IActionResult EditProduct(Product updated)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var product = _context.Products.FirstOrDefault(p => p.Id == updated.Id && p.SellerId == userId);
            if (product == null)
                return RedirectToAction("Dashboard");

            ModelState.Remove("Seller");

            if (!ModelState.IsValid)
                return View(updated);

            product.Title = updated.Title;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Category = updated.Category;
            product.ImageUrl = updated.ImageUrl;

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public IActionResult DeleteProduct(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var product = _context.Products.FirstOrDefault(p => p.Id == id && p.SellerId == userId);
            if (product == null)
                return RedirectToAction("Dashboard");

            return View(product);
        }

        [HttpPost]
        public IActionResult ConfirmDelete(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Product");

            var userId = HttpContext.Session.GetInt32("UserId")!.Value;

            var product = _context.Products.FirstOrDefault(p => p.Id == id && p.SellerId == userId);
            if (product == null)
                return RedirectToAction("Dashboard");

            _context.Products.Remove(product);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        public IActionResult MySales()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Vendeur")
                return RedirectToAction("Index", "Home");

            var sellerId = HttpContext.Session.GetInt32("UserId");
            if (sellerId == null)
                return RedirectToAction("Login", "User");

            var sales = _context.OrderItems
                .Where(i => i.SellerId == sellerId.Value)
                .Include(i => i.Product)
                .Include(i => i.Order)
                .OrderByDescending(i => i.OrderId)
                .ThenByDescending(i => i.Id)
                .ToList();

            ViewBag.TotalEarned = sales.Sum(i => i.LineTotal);

            ViewBag.ByInvoice = sales
                .GroupBy(i => i.OrderId)
                .Select(g => new
                {
                    OrderId = g.Key,
                    Date = g.First().Order!.CreatedAt,
                    Total = g.Sum(x => x.LineTotal),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.OrderId)
                .ToList();

            return View(sales);
        }
    }
}
