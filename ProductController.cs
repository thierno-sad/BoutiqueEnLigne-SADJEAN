using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Filters;
using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.Services;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Controllers
{
    [RequireLogin]
    public class ProductController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly DummyJsonService _dummy;

        public ProductController(ApplicationDbContext context, DummyJsonService dummy)
        {
            _context = context;
            _dummy = dummy;
        }

        public IActionResult Index(string? q, string? category, string? sort)
        {
            var productsQuery = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(q))
            {
                productsQuery = productsQuery.Where(p =>
                    p.Title.Contains(q) || p.Description.Contains(q));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                productsQuery = productsQuery.Where(p => p.Category == category);
            }

            
            productsQuery = sort switch
            {
                "price_asc" => productsQuery.OrderBy(p => p.Price),
                "price_desc" => productsQuery.OrderByDescending(p => p.Price),
                _ => productsQuery.OrderBy(p => p.Title)
            };

            var categories = _context.Products
                .Select(p => p.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.Q = q;
            ViewBag.Category = category;
            ViewBag.Sort = sort;

            ViewBag.HasProducts = _context.Products.Any();

            var products = productsQuery.ToList();
            return View(products);
        }

        public async Task<IActionResult> Import()
        {
            var data = await _dummy.GetProductsAsync(limit: 30);
            if (data == null) return RedirectToAction("Index");

            var role = HttpContext.Session.GetString("UserRole");
            var sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
                return RedirectToAction("Login", "User");

            int sellerIdToUse;

            if (role == "Vendeur")
            {
                sellerIdToUse = sessionUserId.Value;
            }
            else
            {
                sellerIdToUse = _context.Users
                    .Where(u => u.Role == "Vendeur")
                    .Select(u => u.Id)
                    .FirstOrDefault();

                if (sellerIdToUse == 0)
                {
                    TempData["Error"] = "Aucun vendeur n'existe. Crée un compte vendeur avant d'importer.";
                    return RedirectToAction("Index");
                }
            }

            var existingTitles = _context.Products
                .Select(x => x.Title)
                .ToHashSet();

            foreach (var p in data.Products)
            {
                if (existingTitles.Contains(p.Title))
                    continue;

                var product = new Product
                {
                    Title = p.Title,
                    Description = p.Description,
                    Price = p.Price,
                    Category = p.Category,
                    ImageUrl = p.Thumbnail,
                    SellerId = sellerIdToUse 
                };

                _context.Products.Add(product);
            }

            _context.SaveChanges();
            TempData["Success"] = "Produits importés avec succès.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }
    }
}