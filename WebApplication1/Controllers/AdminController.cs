using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.ViewModels;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AdminController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;

        public AdminController(QLQuyTinhThuongContext context)
        {
            _context = context;
        }

        private bool IsAdmin()
            => string.Equals((HttpContext.Session.GetString("Role") ?? "").Trim(),
                             "ADMIN",
                             StringComparison.OrdinalIgnoreCase);

        public async Task<IActionResult> Index()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var vm = new AdminDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Admin",

                TotalUsers = await _context.Users.CountAsync(),
                TotalSupportRequests = await _context.SupportRequests.CountAsync(),

                // Nếu bạn có 1 quỹ duy nhất: lấy quỹ đầu tiên
                FundBalance = await _context.Funds
                    .OrderByDescending(f => f.LastUpdated)
                    .Select(f => f.Balance)
                    .FirstOrDefaultAsync(),

                ApprovalsToday = await _context.Approvals
                    .CountAsync(a => a.ApprovalDate.Date == DateTime.Today),

                RecentLogs = await _context.Logs
                    .OrderByDescending(l => l.ActionTime)
                    .Take(6)
                    .Select(l => new RecentLogVm
                    {
                        Action = l.Action,
                        TableName = l.TableName,
                        ActionTime = l.ActionTime
                    })
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        public async Task<IActionResult> Users()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var users = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Select(u => new AdminUserVm
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Status = u.Status,
                    Role = u.UserRoles
                        .Select(ur => ur.Role.RoleName)
                        .FirstOrDefault() ?? "N/A"
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View(users);
        }

        public async Task<IActionResult> Data()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var vm = new AdminDataVm
            {
                Beneficiaries = await _context.Beneficiaries.CountAsync(),
                SupportRequests = await _context.SupportRequests.CountAsync(),
                Approvals = await _context.Approvals.CountAsync(),
                Expenses = await _context.Expenses.CountAsync(),
                Donors = await _context.Donors.CountAsync(),
                Donations = await _context.Donations.CountAsync(),
                Funds = await _context.Funds.CountAsync(),
                Logs = await _context.Logs.CountAsync()
            };

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View(vm);
        }
    }
}
