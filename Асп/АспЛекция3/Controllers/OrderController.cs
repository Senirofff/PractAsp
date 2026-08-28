using Microsoft.AspNetCore.Authorization; 
﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using АспЛекция3.Models;

namespace АспЛекция3.Controllers
{
    [Authorize]  
    public class OrderController : Controller
    {
        private readonly OnlinePharmacyP824Context _context;

        public OrderController(OnlinePharmacyP824Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Мои заказы";

            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Auth");

            var orders = await _context.Orders
                .Where(o => o.IdUser == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            var userRole = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var currentUserId = int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.Sid)?.Value, out var uid) ? uid : 0;

            if (userRole != "Admin" && userRole != "Manager" && order.IdUser != currentUserId)
            {
                return Forbid(); 
            }

            var orderItems = await _context.OrderItems
                .Where(oi => oi.IdOrder == id.Value)
                .ToListAsync();

            var productIds = orderItems.Select(oi => oi.IdProduct).Distinct().ToList();
            var products = await _context.Products
                .Where(p => productIds.Contains(p.IdProduct))
                .ToListAsync();

            ViewBag.OrderItems = orderItems;
            ViewBag.Products = products;

            ViewData["Title"] = $"Заказ №{order.IdOrder}";
            return View(order);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            ViewData["Title"] = $"Удаление заказа №{id}";
            return View(order);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var items = await _context.OrderItems.Where(oi => oi.IdOrder == id).ToListAsync();
            _context.OrderItems.RemoveRange(items);

            var order = await _context.Orders.FindAsync(id);
            if (order != null)
                _context.Orders.Remove(order);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.Sid)?.Value;
            if (!int.TryParse(userIdClaim, out int userId))
                return RedirectToAction("Login", "Auth");

            var cart = await _context.Carts.FirstOrDefaultAsync(c => c.IdUser == userId);
            if (cart == null)
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index", "Product");
            }

            var cartItems = await _context.CartItems.Where(ci => ci.IdCart == cart.IdCart).ToListAsync();
            if (!cartItems.Any())
            {
                TempData["Error"] = "Корзина пуста";
                return RedirectToAction("Index", "Product");
            }

            var productIds = cartItems.Select(ci => ci.IdProduct).ToList();
            var products = await _context.Products.Where(p => productIds.Contains(p.IdProduct)).ToListAsync();
            var user = await _context.Users.FindAsync(userId);
            int discount = user?.DiscountPercent ?? 0;

            decimal totalAmount = 0;
            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.IdProduct == item.IdProduct);
                if (product != null)
                    totalAmount += product.Price * item.Quantity * (1 - discount / 100m);
            }

            var order = new Order
            {
                IdUser = userId,
                OrderDate = DateTime.Now,
                Status = "Новый",
                DeliveryAddress = "Адрес будет уточнён",
                TotalAmount = Math.Round(totalAmount, 2)
            };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); 

            foreach (var item in cartItems)
            {
                var product = products.FirstOrDefault(p => p.IdProduct == item.IdProduct);
                if (product != null)
                {
                    _context.OrderItems.Add(new OrderItem
                    {
                        IdOrder = order.IdOrder,
                        IdProduct = item.IdProduct,
                        Quantity = item.Quantity,
                        Price = Math.Round(product.Price * (1 - discount / 100m), 2)
                    });
                }
            }
            await _context.SaveChangesAsync();

            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Заказ №{order.IdOrder} успешно оформлен!";
            return RedirectToAction("Details", new { id = order.IdOrder });
        }
    }
}