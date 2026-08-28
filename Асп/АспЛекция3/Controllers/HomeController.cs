using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;  
using АспЛекция3.Models;

namespace АспЛекция3.Controllers
{
    [AllowAnonymous] 
    public class HomeController : Controller
    {
        private readonly OnlinePharmacyP824Context _context;

        public HomeController(OnlinePharmacyP824Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Главная панель";

            var stats = new
            {
                TotalProducts = await _context.Products.CountAsync(),
                TotalCategories = await _context.Categories.CountAsync(),
                TotalUsers = await _context.Users.CountAsync(),
                TotalOrders = await _context.Orders.CountAsync()
            };

            return View(stats);
        }
    }
}