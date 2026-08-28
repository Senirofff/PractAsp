using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using АспЛекция3.Models;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory; 

namespace АспЛекция3.Controllers
{
    [RequestSizeLimit(50_000_000)]
    [RequestFormLimits(MultipartBodyLengthLimit = 50_000_000)]
    public class ProductController : Controller
    {
        private readonly OnlinePharmacyP824Context _context;
        private readonly IMemoryCache _cache; 

        public ProductController(OnlinePharmacyP824Context context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(string? search, int? categoryId, string? sortBy, int page = 1)
        {
            ViewData["Title"] = "Каталог товаров";
            ViewData["CurrentSearch"] = search ?? "";
            ViewData["CurrentCategory"] = categoryId;
            ViewData["CurrentSort"] = sortBy ?? "";

            const int pageSize = 10;
            string cacheKey = $"products_page_{page}_search_{search}_cat_{categoryId}_sort_{sortBy}";

            if (!_cache.TryGetValue(cacheKey, out List<Product> cachedProducts))
            {
                var query = _context.Products.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                {
                    query = query.Where(p => p.Name.Contains(search));
                }

                if (categoryId.HasValue && categoryId.Value > 0)
                {
                    query = query.Where(p => p.IdCategory == categoryId.Value);
                }


                cachedProducts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromSeconds(60));

                _cache.Set(cacheKey, cachedProducts, cacheOptions);

                ViewData["DataSourceInfo"] = "Данные загружены из Базы Данных SQL Server и сохранены в кэш.";
                ViewData["DataSourceClass"] = "alert-warning"; 
            }
            else
            {
                ViewData["DataSourceInfo"] = "Данные мгновенно извлечены из оперативной памяти (MemoryCache)";
                ViewData["DataSourceClass"] = "alert-success";
            }

            ViewBag.Categories = await _context.Categories.ToListAsync();

            return View(cachedProducts);
        }

        [Authorize]
        public async Task<IActionResult> Cart()
        {
            ViewData["Title"] = "Корзина";
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return RedirectToAction("Login", "Auth");
            }

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.IdUser == userId);
            if (cart == null)
            {
                return View(new List<CartItem>());
            }

            var cartItems = await _context.CartItems
                .Where(ci => ci.IdCart == cart.IdCart)
                .ToListAsync();

            var products = await _context.Products.ToListAsync();
            ViewBag.Products = products;

            var user = await _context.Users.FindAsync(userId);
            ViewBag.UserDiscount = user?.DiscountPercent ?? 0;

            return View(cartItems);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int id, int quantity = 1)
        {
            int cartId = await GetOrCreateCartIdAsync();
            if (cartId == -1) return RedirectToAction("Login", "Auth");

            var item = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.IdCart == cartId && ci.IdProduct == id);

            if (item != null)
            {
                item.Quantity += quantity;
                _context.CartItems.Update(item);
            }
            else
            {
                _context.CartItems.Add(new CartItem
                {
                    IdCart = cartId,
                    IdProduct = id,
                    Quantity = quantity
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int id, int quantity)
        {
            if (quantity < 1) return RedirectToAction(nameof(Cart));

            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                item.Quantity = quantity;
                _context.CartItems.Update(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cart));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int id)
        {
            var item = await _context.CartItems.FindAsync(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Cart));
        }

        private async Task<int> GetOrCreateCartIdAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return -1;
            }

            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.IdUser == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    IdUser = userId,
                    CreatedDate = DateOnly.FromDateTime(DateTime.Now)
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart.IdCart;
        }
    }
}