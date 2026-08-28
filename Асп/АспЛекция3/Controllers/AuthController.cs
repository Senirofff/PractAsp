using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using АспЛекция3.Models;

namespace АспЛекция3.Controllers
{
    public class AuthController : Controller
    {
        private readonly OnlinePharmacyP824Context _context;

        public AuthController(OnlinePharmacyP824Context context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            ViewData["Title"] = "Вход в систему";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string login, string password)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
            {
                ModelState.AddModelError("", "Логин и пароль обязательны");
                return View();
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Неверный логин или пароль");
                return View();
            }
            if (user.IsActive == false) 
            { 
                ModelState.AddModelError("", "Аккаунт заблокирован"); 
                return View(); 
            }

            var roleTitle = await _context.Roles
                .Where(r => r.IdRole == user.IdRole)
                .Select(r => r.Title)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(roleTitle))
                roleTitle = "User";

            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.Name, user.Login),
        new Claim(ClaimTypes.Sid, user.IdUser.ToString()),
        new Claim(ClaimTypes.Role, roleTitle) 
    };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Product");
        }

        [HttpGet]
        public async Task<IActionResult> Register()
        {
            ViewData["Title"] = "Регистрация";
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = _context.Roles.ToList();
                return View(user);
            }

            if (await _context.Users.AnyAsync(u => u.Login == user.Login))
            {
                ModelState.AddModelError("Login", "Пользователь с таким логином уже существует");
                ViewBag.Roles = _context.Roles.ToList();
                return View(user);
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            user.IdRole = 4;
            user.IsActive = true;

            var role = await _context.Roles.FindAsync(4);
            if (role != null && role.Title == "ВИП-клиент")
            {
                user.DiscountPercent = 15; 
            }

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Регистрация успешна! Теперь вы можете войти.";
            return RedirectToAction("Login");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Product");
        }
        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            ViewData["Title"] = "Доступ запрещён";
            return View();
        }
    }


}