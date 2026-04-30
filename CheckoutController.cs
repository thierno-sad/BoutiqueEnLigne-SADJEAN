using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Filters;
using BoutiqueEnLigne.Helpers;
using BoutiqueEnLigne.Models;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Linq;

namespace BoutiqueEnLigne.Controllers
{
    [RequireLogin]
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CheckoutController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            return View();
        }

        [HttpPost]
        public IActionResult CreatePaymentIntent()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return Forbid();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
            if (cart.Count == 0) return BadRequest(new { error = "Panier vide" });

            var ids = cart.Select(c => c.ProductId).ToList();
            var products = _context.Products.Where(p => ids.Contains(p.Id)).ToList();

            decimal total = 0m;
            foreach (var item in cart)
            {
                var p = products.FirstOrDefault(x => x.Id == item.ProductId);
                if (p == null) continue;

                var qty = item.Quantity <= 0 ? 1 : item.Quantity;
                total += p.Price * qty;
            }

            if (total <= 0) return BadRequest(new { error = "Total invalide" });

            long amountInCents = (long)Math.Round(total * 100m, MidpointRounding.AwayFromZero);

            var key = Stripe.StripeConfiguration.ApiKey;
            if (string.IsNullOrWhiteSpace(key))
                return BadRequest(new { error = "Stripe SecretKey non chargée (null/empty)." });

            var service = new PaymentIntentService();
            var intent = service.Create(new PaymentIntentCreateOptions
            {
                Amount = amountInCents,
                Currency = "cad",
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            });

            return Json(new { clientSecret = intent.ClientSecret });
        }

        public IActionResult Success()
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return RedirectToAction("Index", "Product");

            return View();
        }

        [HttpPost]
        public IActionResult Finalize(string paymentIntentId)
        {
            var role = HttpContext.Session.GetString("UserRole");
            if (role != "Client") return Forbid();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return Unauthorized();

            var cart = HttpContext.Session.GetObject<List<CartItem>>("CART") ?? new List<CartItem>();
            if (cart.Count == 0) return BadRequest(new { error = "Panier vide" });

            var ids = cart.Select(c => c.ProductId).ToList();
            var products = _context.Products.Where(p => ids.Contains(p.Id)).ToList();

            var order = new Order
            {
                ClientId = userId.Value,
                CreatedAt = DateTime.UtcNow,
                StripePaymentIntentId = paymentIntentId,
                PaymentStatus = "Paid"
            };

            decimal total = 0m;

            foreach (var ci in cart)
            {
                var p = products.FirstOrDefault(x => x.Id == ci.ProductId);
                if (p == null) continue;

                var qty = ci.Quantity <= 0 ? 1 : ci.Quantity;
                var line = p.Price * qty;

                order.Items.Add(new OrderItem
                {
                    ProductId = p.Id,
                    Quantity = qty,
                    UnitPrice = p.Price,
                    SellerId = p.SellerId,
                    LineTotal = line

                });

                total += line;
            }

            order.TotalAmount = total;

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Session.Remove("CART");

            return Ok(new { ok = true, orderId = order.Id });
        }
    }
}