using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Utils;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AccountController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;
        
        public AccountController(QLQuyTinhThuongContext context)
        {
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Username, string Password)
        {
            // Validate input
            var validation = ValidationHelper.ValidateLogin(Username, Password);
            if (!validation.IsValid)
            {
                ViewBag.ErrorAlert = AlertHelper.Error(validation.ErrorMessage);
                return View();
            }

            // Sanitize input
            Username = AuthHelper.SanitizeInput(Username);
            
            // Hash password
            string hashed = AuthHelper.HashPassword(Password);
            
            // Tìm user và load role
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == Username && u.Password == hashed);

            if (user == null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tên đăng nhập hoặc mật khẩu không đúng!");
                return View();
            }

            // Kiểm tra trạng thái tài khoản
            if (user.Status != "Hoạt động")
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản đã bị khóa!");
                return View();
            }

            // Lấy vai trò đầu tiên của user
            var userRole = user.UserRoles.FirstOrDefault();
            if (userRole == null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản chưa được phân quyền!");
                return View();
            }

            // Set session
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", userRole.Role.RoleName);

            // Ghi log đăng nhập
            var log = new Log
            {
                UserId = user.UserId,
                Action = "Đăng nhập hệ thống",
                TableName = "Users",
                ActionTime = DateTime.Now,
                NewData = $"User: {user.Username}, Role: {userRole.Role.RoleName}"
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // Chuyển hướng theo vai trò
            return userRole.Role.RoleName switch
            {
                "ADMIN" => RedirectToAction("Index", "Admin"),
                "STAFF" => RedirectToAction("Index", "Staff"),
                "ACCOUNTANT" => RedirectToAction("Index", "Accountant"),
                "MANAGER" => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "Home")
            };
        }

        public async Task<IActionResult> Logout()
        {
            var userId = HttpContext.Session.GetString("UserId");
            if (!string.IsNullOrEmpty(userId))
            {
                // Ghi log đăng xuất
                var log = new Log
                {
                    UserId = int.Parse(userId),
                    Action = "Đăng xuất hệ thống",
                    TableName = "Users",
                    ActionTime = DateTime.Now
                };
                _context.Logs.Add(log);
                await _context.SaveChangesAsync();
            }

            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}
