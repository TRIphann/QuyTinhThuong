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
            if (!string.Equals(user.Status?.Trim(), "Hoạt động", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản đã bị khóa!");
                return View();
            }

            // Lấy role ưu tiên (ADMIN trước), chuẩn hóa (Trim + Upper)
            var roleName = user.UserRoles
                .Select(ur => (ur.Role.RoleName ?? "").Trim().ToUpperInvariant())
                .OrderByDescending(r => r == "ADMIN")
                .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(roleName))
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tài khoản chưa được phân quyền!");
                return View();
            }

            // Set session (lưu role đã chuẩn hoá)
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
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
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(string Username, string Password, string FullName, string Email, string Phone)
        {
            // Validate input
            var validation = ValidationHelper.ValidateRegistration(Username, Password, FullName, Email, Phone);
            if (!validation.IsValid)
            {
                ViewBag.ErrorAlert = AlertHelper.Error(validation.ErrorMessage);
                return View();
            }

            // Sanitize input
            Username = AuthHelper.SanitizeInput(Username);
            FullName = AuthHelper.SanitizeInput(FullName);
            Email = AuthHelper.SanitizeInput(Email);
            Phone = AuthHelper.SanitizeInput(Phone);

            // Kiểm tra username đã tồn tại chưa
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == Username);
            if (existingUser != null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Tên đăng nhập đã tồn tại!");
                return View();
            }

            // Kiểm tra email đã tồn tại chưa
            var existingEmail = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);
            if (existingEmail != null)
            {
                ViewBag.ErrorAlert = AlertHelper.Error("Email đã được sử dụng!");
                return View();
            }

            // Hash password
            string hashed = AuthHelper.HashPassword(Password);

            // Tạo user mới
            var newUser = new User
            {
                Username = Username,
                Password = hashed,
                FullName = FullName,
                Email = Email,
                Phone = Phone,
                Status = "Hoạt động"
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
            return View();
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
