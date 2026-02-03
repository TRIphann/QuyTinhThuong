using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;
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
            return View(new LoginVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sanitize input
            var username = AuthHelper.SanitizeInput(model.Username);

            // Hash password
            string hashed = AuthHelper.HashPassword(model.Password);

            // Tìm user và load role
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Username == username && u.Password == hashed);

            if (user == null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tên đăng nhập hoặc mật khẩu không đúng!");
                return View(model);
            }

            // Kiểm tra trạng thái tài khoản (Active hoặc Hoạt động)
            var status = user.Status?.Trim();
            if (!string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(status, "Hoạt động", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản đã bị khóa!");
                return View(model);
            }

            // Lấy role ưu tiên (ADMIN trước), chuẩn hóa (Trim + Upper)
            var roleName = user.UserRoles
                .Select(ur => (ur.Role.RoleName ?? "").Trim().ToUpperInvariant())
                .OrderByDescending(r => r == "ADMIN")
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản chưa được phân quyền!");
                return View(model);
            }

            // Set session (lưu role đã chuẩn hoá)
            HttpContext.Session.SetInt32("UserId", user.UserId);
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("FullName", user.FullName);
            HttpContext.Session.SetString("Role", roleName);

            // Ghi log đăng nhập
            var log = new Log
            {
                UserId = user.UserId,
                Action = "Đăng nhập hệ thống",
                TableName = "Users",
                ActionTime = DateTime.Now,
                NewData = $"User: {user.Username}, Role: {roleName}"
            };
            _context.Logs.Add(log);
            await _context.SaveChangesAsync();

            // Chuyển hướng theo vai trò (dùng roleName đã chuẩn hoá)
            return roleName switch
            {
                "ADMIN" => RedirectToAction("Index", "Admin"),
                "STAFF" => RedirectToAction("Index", "Staff"),
                "ACCOUNTANT" => RedirectToAction("Index", "Accountant"),
                "MANAGER" => RedirectToAction("Index", "Manager"),
                _ => RedirectToAction("Index", "Home")
            };
        }


        public IActionResult Register()
        {
            return View(new RegisterVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Sanitize input
            var username = AuthHelper.SanitizeInput(model.Username);
            var fullName = AuthHelper.SanitizeInput(model.FullName);
            var email = AuthHelper.SanitizeInput(model.Email);
            var phone = AuthHelper.SanitizeInput(model.Phone ?? "");

            // Kiểm tra username đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại!");
                return View(model);
            }

            // Kiểm tra email đã tồn tại chưa
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (existingEmail != null)
            {
                ModelState.AddModelError("Email", "Email đã được sử dụng!");
                return View(model);
            }

            // Hash password
            string hashed = AuthHelper.HashPassword(model.Password);

            // Tạo user mới
            var newUser = new User
            {
                Username = username,
                Password = hashed,
                FullName = fullName,
                Email = email,
                Phone = phone,
                Status = "Active"
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // Gán role mặc định (STAFF hoặc USER)
            var defaultRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "STAFF");
            if (defaultRole != null)
            {
                var userRole = new UserRole
                {
                    UserId = newUser.UserId,
                    RoleId = defaultRole.RoleId
                };
                _context.UserRoles.Add(userRole);
                await _context.SaveChangesAsync();
            }

            ViewBag.SuccessAlert = AlertHelper.Success("Đăng ký thành công! Vui lòng đăng nhập.");
            return View(new RegisterVm());
        }

        public async Task<IActionResult> Logout()
        {   
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                // Ghi log đăng xuất
                var log = new Log
                {
                    UserId = userId.Value,
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
