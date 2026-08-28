using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization; 
using АспЛекция3.Models;

namespace АспЛекция3.Controllers
{
    [Authorize(Roles = "Администратор")]  
    public class UserController : Controller
    {
        private readonly OnlinePharmacyP824Context _context;

        public UserController(OnlinePharmacyP824Context context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Пользователи системы";
            var users = await _context.Users.ToListAsync();
            return View(users);
        }

        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Добавить пользователя";
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View();
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewData["Title"] = "Изменить пользователя";
            ViewBag.Roles = await _context.Roles.ToListAsync();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(user);
            }

            try
            {
                _context.Users.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при создании пользователя: {ex.Message}");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            if (id != user.IdUser) return BadRequest();

            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(user);
            }

            try
            {
                if (string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    var existing = await _context.Users.AsNoTracking()
                                                      .FirstOrDefaultAsync(u => u.IdUser == id);
                    if (existing != null)
                        user.PasswordHash = existing.PasswordHash;
                }

                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Ошибка при обновлении пользователя: {ex.Message}");
                ViewBag.Roles = await _context.Roles.ToListAsync();
                return View(user);
            }
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound();

            ViewData["Title"] = "Удаление пользователя";
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}