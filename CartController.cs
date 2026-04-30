using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Filters;
using BoutiqueEnLigne.Helpers;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BoutiqueEnLigne.Controllers
{
    [RequireLogin]
    public class CartController : Controller
    {
        private const string CartKey = "CART";
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<CartItem> GetCart()
        {
            return HttpContext.Session.GetObject<List<CartItem>>(CartKey) ?? new List<CartItem>();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetObject(CartKey, cart);
        }

        public IActionResult Index()
        {
            
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var cart = GetCart();

            var productIds = cart.Select(c => c.ProductId).ToList();
            var products = _context.Products.Where(p => productIds.Contains(p.Id)).ToList();

            var items = cart.Select(c =>
            {
                var p = products.FirstOrDefault(x => x.Id == c.ProductId);
                return new
                {
                    Product = p,
                    Quantity = c.Quantity,
                    LineTotal = p == null ? 0 : p.Price * c.Quantity
                };
            }).Where(x => x.Product != null).ToList();

            ViewBag.Total = items.Sum(x => x.LineTotal);

            return View(items);
        }

        [HttpPost]
        public IActionResult Add(int id, int qty = 1)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var productExists = _context.Products.Any(p => p.Id == id);
            if (!productExists) return NotFound();

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item == null)
                cart.Add(new CartItem { ProductId = id, Quantity = Math.Max(1, qty) });
            else
                item.Quantity += Math.Max(1, qty);

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int id, int qty)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.ProductId == id);

            if (item != null)
            {
                if (qty <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = qty;
            }

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            var cart = GetCart();
            cart.RemoveAll(x => x.ProductId == id);

            SaveCart(cart);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            SaveCart(new List<CartItem>());
            return RedirectToAction("Index");
        }
    }
}