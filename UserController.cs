using BoutiqueEnLigne.Data;
using BoutiqueEnLigne.Models;
using BoutiqueEnLigne.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BoutiqueEnLigne.Controllers
{
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            if (_context.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Cet email est déjà utilisé.");
                return View(vm);
            }


            var user = new User
            {
                Fullname = vm.Fullname,

                Email = vm.Email,
                Role = vm.Role
            };

            var hasher = new PasswordHasher<User>();
            user.PasswordHash = hasher.HashPassword(user, vm.Password);

            _context.Users.Add(user);
            _context.SaveChanges();

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            HttpContext.Session.SetString("UserName", user.Fullname);

            if (user.Role == "Vendeur")
                return RedirectToAction("Dashboard", "Seller");

            return RedirectToAction("Index", "Product");
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var user = _context.Users.FirstOrDefault(u => u.Email == vm.Email);

            if (user == null)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(vm);
            }

            var hasher = new PasswordHasher<User>();
            var verify = hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password);

            if (verify == PasswordVerificationResult.Failed)
            {
                ModelState.AddModelError("", "Email ou mot de passe incorrect.");
                return View(vm);
            }

            HttpContext.Session.SetInt32("UserId", user.Id);
            HttpContext.Session.SetString("UserRole", user.Role);

            HttpContext.Session.SetString("UserName", user.Fullname);

            if (user.Role == "Vendeur")
                return RedirectToAction("Dashboard", "Seller");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            return View(user);
        }

        [HttpPost]
        public IActionResult Profile(User updated)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Role");
            ModelState.Remove("Products");

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId.Value);
            if (user == null)
                return RedirectToAction("Login");

            if (!ModelState.IsValid)
                return View(user);

            user.Fullname = updated.Fullname;
            user.Email = updated.Email;

            _context.SaveChanges();

            HttpContext.Session.SetString("UserName", user.Fullname);

            ViewData["Success"] = "Profil mis à jour avec succès.";
            return View(user);
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Product");
        }
    }
}