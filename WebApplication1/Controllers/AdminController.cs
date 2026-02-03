using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.ViewModels;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC.Utils;

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
                    .Include(l => l.User)
                        .ThenInclude(u => u!.UserRoles)
                            .ThenInclude(ur => ur.Role)
                    .OrderByDescending(l => l.ActionTime)
                    .Take(6)
                    .Select(l => new RecentLogVm
                    {
                        Action = l.Action,
                        TableName = l.TableName,
                        ActionTime = l.ActionTime,
                        UserName = l.User != null ? l.User.FullName : "System",
                        RoleName = l.User != null && l.User.UserRoles.Any() 
                            ? l.User.UserRoles.First().Role.RoleName 
                            : "N/A"
                    })
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        public async Task<IActionResult> Users(string? role)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // Lấy tất cả users để thống kê
            var allUsers = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .ToListAsync();

            var query = allUsers.AsQueryable();

            // Lọc theo role nếu có
            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.UserRoles.Any(ur => ur.Role.RoleName.ToUpper() == role.ToUpper()));
            }

            var users = query
                .Select(u => new AdminUserVm
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Username = u.Username,
                    Email = u.Email,
                    Status = u.Status,
                    Role = u.UserRoles
                        .Select(ur => ur.Role.RoleName)
                        .FirstOrDefault() ?? "N/A",
                    LastLoginTime = _context.Logs
                        .Where(l => l.UserId == u.UserId && l.Action.Contains("Đăng nhập"))
                        .OrderByDescending(l => l.ActionTime)
                        .Select(l => (DateTime?)l.ActionTime)
                        .FirstOrDefault()
                })
                .ToList();

            // Thống kê cho biểu đồ
            var totalUsers = allUsers.Count;
            var activeUsers = allUsers.Count(u => u.Status == "Active");
            var lockedUsers = allUsers.Count(u => u.Status == "Locked");
            var pendingUsers = allUsers.Count(u => u.Status == "Pending");

            var staffCount = allUsers.Count(u => u.UserRoles.Any(ur => ur.Role.RoleName.ToUpper() == "STAFF"));
            var accountantCount = allUsers.Count(u => u.UserRoles.Any(ur => ur.Role.RoleName.ToUpper() == "ACCOUNTANT"));
            var managerCount = allUsers.Count(u => u.UserRoles.Any(ur => ur.Role.RoleName.ToUpper() == "MANAGER"));
            var adminCount = allUsers.Count(u => u.UserRoles.Any(ur => ur.Role.RoleName.ToUpper() == "ADMIN"));

            // Hoạt động đăng nhập theo ngày (7 ngày gần đây)
            var loginStats = await _context.Logs
                .Where(l => l.Action.Contains("Đăng nhập") && l.ActionTime >= DateTime.Today.AddDays(-6))
                .GroupBy(l => l.ActionTime.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // Tạo dữ liệu 7 ngày
            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .Select(d => new { 
                    Date = d, 
                    Count = loginStats.FirstOrDefault(x => x.Date == d)?.Count ?? 0 
                })
                .ToList();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["CurrentRole"] = role ?? "all";
            
            // Stats
            ViewData["TotalUsers"] = totalUsers;
            ViewData["ActiveUsers"] = activeUsers;
            ViewData["LockedUsers"] = lockedUsers;
            ViewData["PendingUsers"] = pendingUsers;
            ViewData["StaffCount"] = staffCount;
            ViewData["AccountantCount"] = accountantCount;
            ViewData["ManagerCount"] = managerCount;
            ViewData["AdminCount"] = adminCount;
            
            // Login chart data
            ViewData["LoginDates"] = string.Join(",", last7Days.Select(x => $"'{x.Date:dd/MM}'"));
            ViewData["LoginCounts"] = string.Join(",", last7Days.Select(x => x.Count));
            
            return View(users);
        }

        public async Task<IActionResult> Data()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // Base counts
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

            // Total amounts
            vm.TotalDonations = await _context.Donations.SumAsync(d => d.Amount);
            vm.TotalExpenses = await _context.Expenses.SumAsync(e => e.Amount);
            vm.FundBalance = await _context.Funds.OrderByDescending(f => f.LastUpdated).Select(f => f.Balance).FirstOrDefaultAsync();

            // Request status counts
            vm.PendingRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Chờ duyệt");
            vm.ApprovedRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Đã duyệt");
            vm.RejectedRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Từ chối");

            // Recent Beneficiaries (top 5)
            vm.RecentBeneficiaries = await _context.Beneficiaries
                .OrderByDescending(b => b.BeneficiaryId)
                .Take(5)
                .Select(b => new AdminBeneficiaryVm
                {
                    BeneficiaryId = b.BeneficiaryId,
                    FullName = b.FullName,
                    BeneficiaryType = b.BeneficiaryType,
                    Address = b.Address
                })
                .ToListAsync();

            // Recent Donations (top 5)
            vm.RecentDonations = await _context.Donations
                .Include(d => d.Donor)
                .OrderByDescending(d => d.DonationDate)
                .Take(5)
                .Select(d => new AdminDonationVm
                {
                    DonationId = d.DonationId,
                    DonorName = d.Donor != null ? d.Donor.DonorName : "Ẩn danh",
                    Amount = d.Amount,
                    DonationDate = d.DonationDate,
                    PaymentMethod = d.Method
                })
                .ToListAsync();

            // Recent Expenses (top 5)
            vm.RecentExpenses = await _context.Expenses
                .Include(e => e.SupportRequest)
                    .ThenInclude(sr => sr!.Beneficiary)
                .OrderByDescending(e => e.ExpenseDate)
                .Take(5)
                .Select(e => new AdminExpenseVm
                {
                    ExpenseId = e.ExpenseId,
                    BeneficiaryName = e.SupportRequest != null && e.SupportRequest.Beneficiary != null 
                        ? e.SupportRequest.Beneficiary.FullName : "N/A",
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    PaymentMethod = e.PaymentMethod
                })
                .ToListAsync();

            // Top Donors
            vm.TopDonors = await _context.Donors
                .Include(d => d.Donations)
                .OrderByDescending(d => d.Donations.Sum(dn => dn.Amount))
                .Take(5)
                .Select(d => new AdminDonorVm
                {
                    DonorId = d.DonorId,
                    FullName = d.DonorName,
                    DonorType = d.DonorType,
                    TotalDonated = d.Donations.Sum(dn => dn.Amount),
                    DonationCount = d.Donations.Count
                })
                .ToListAsync();

            // Recent Logs (top 8)
            vm.RecentLogs = await _context.Logs
                .Include(l => l.User)
                .OrderByDescending(l => l.ActionTime)
                .Take(8)
                .Select(l => new AdminLogVm
                {
                    LogId = l.LogId,
                    Action = l.Action,
                    TableName = l.TableName,
                    UserName = l.User != null ? l.User.FullName : "System",
                    ActionTime = l.ActionTime
                })
                .ToListAsync();

            // Recent Requests (top 5)
            vm.RecentRequests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .OrderByDescending(r => r.RequestDate)
                .Take(5)
                .Select(r => new SupportRequestVm
                {
                    RequestId = r.RequestId,
                    BeneficiaryName = r.Beneficiary != null ? r.Beneficiary.FullName : "N/A",
                    BeneficiaryType = r.Beneficiary != null ? r.Beneficiary.BeneficiaryType : "",
                    RequestedAmount = r.RequestedAmount,
                    Status = r.Status,
                    RequestDate = r.RequestDate,
                    Reason = r.Reason
                })
                .ToListAsync();

            // Donation chart - last 7 days
            var donationStats = await _context.Donations
                .Where(d => d.DonationDate >= DateTime.Today.AddDays(-6))
                .GroupBy(d => d.DonationDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var last7Days = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .Select(d => new { Date = d, Total = donationStats.FirstOrDefault(x => x.Date == d)?.Total ?? 0 })
                .ToList();

            vm.DonationChartLabels = string.Join(",", last7Days.Select(x => $"'{x.Date:dd/MM}'"));
            vm.DonationChartData = string.Join(",", last7Days.Select(x => x.Total));

            // Expense chart - last 7 days
            var expenseStats = await _context.Expenses
                .Where(e => e.ExpenseDate >= DateTime.Today.AddDays(-6))
                .GroupBy(e => e.ExpenseDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.Amount) })
                .ToListAsync();

            var last7DaysExpenses = Enumerable.Range(0, 7)
                .Select(i => DateTime.Today.AddDays(-6 + i))
                .Select(d => new { Date = d, Total = expenseStats.FirstOrDefault(x => x.Date == d)?.Total ?? 0 })
                .ToList();

            vm.ExpenseChartLabels = string.Join(",", last7DaysExpenses.Select(x => $"'{x.Date:dd/MM}'"));
            vm.ExpenseChartData = string.Join(",", last7DaysExpenses.Select(x => x.Total));

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View(vm);
        }

        // ===== CREATE USER =====
        [HttpGet]
        public async Task<IActionResult> CreateUser()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewData["Roles"] = await _context.Roles.ToListAsync();
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View(new CreateUserVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(CreateUserVm model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _context.Roles.ToListAsync();
                ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
                return View(model);
            }

            // Check username exists
            if (await _context.Users.AnyAsync(u => u.Username == model.Username))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                ViewData["Roles"] = await _context.Roles.ToListAsync();
                ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
                return View(model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Username = model.Username,
                Password = Security.HashPassword(model.Password),
                Email = model.Email,
                Phone = model.Phone,
                Status = "Active"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Add user role
            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = model.RoleId
            };
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();

            // Log action
            _context.Logs.Add(new Log
            {
                UserId = int.TryParse(HttpContext.Session.GetString("UserId"), out var uid) ? uid : null,
                Action = $"Thêm mới tài khoản #{user.UserId}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm tài khoản '{model.Username}' thành công!";
            return RedirectToAction("Users");
        }

        // ===== EDIT USER =====
        [HttpGet]
        public async Task<IActionResult> EditUser(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            var lastLogin = await _context.Logs
                .Where(l => l.UserId == id && l.Action.Contains("Đăng nhập"))
                .OrderByDescending(l => l.ActionTime)
                .Select(l => (DateTime?)l.ActionTime)
                .FirstOrDefaultAsync();

            var model = new EditUserVm
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                Phone = user.Phone,
                Status = user.Status,
                RoleId = user.UserRoles.FirstOrDefault()?.RoleId ?? 0,
                CurrentRole = user.UserRoles.FirstOrDefault()?.Role?.RoleName,
                LastLoginTime = lastLogin
            };

            ViewData["Roles"] = await _context.Roles.ToListAsync();
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(EditUserVm model)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
            {
                ViewData["Roles"] = await _context.Roles.ToListAsync();
                ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
                return View(model);
            }

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (user == null)
                return NotFound();

            // Check username conflict
            if (await _context.Users.AnyAsync(u => u.Username == model.Username && u.UserId != model.UserId))
            {
                ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại");
                ViewData["Roles"] = await _context.Roles.ToListAsync();
                ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
                return View(model);
            }

            // Update user info
            user.FullName = model.FullName;
            user.Username = model.Username;
            user.Email = model.Email;
            user.Phone = model.Phone;
            user.Status = model.Status;

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(model.NewPassword))
            {
                user.Password = Security.HashPassword(model.NewPassword);
            }

            // Update role
            var existingRole = user.UserRoles.FirstOrDefault();
            if (existingRole != null)
            {
                existingRole.RoleId = model.RoleId;
            }
            else
            {
                _context.UserRoles.Add(new UserRole
                {
                    UserId = user.UserId,
                    RoleId = model.RoleId
                });
            }

            await _context.SaveChangesAsync();

            // Log action
            _context.Logs.Add(new Log
            {
                UserId = int.TryParse(HttpContext.Session.GetString("UserId"), out var uid2) ? uid2 : null,
                Action = $"Cập nhật tài khoản #{user.UserId}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã cập nhật tài khoản '{model.Username}' thành công!";
            return RedirectToAction("Users");
        }

        // ===== DELETE USER =====
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var user = await _context.Users
                .Include(u => u.UserRoles)
                .FirstOrDefaultAsync(u => u.UserId == id);

            if (user == null)
                return NotFound();

            // Không cho phép xóa chính mình
            var currentUserId = HttpContext.Session.GetString("UserId");
            if (currentUserId == id.ToString())
            {
                TempData["Error"] = "Không thể xóa tài khoản đang đăng nhập!";
                return RedirectToAction("Users");
            }

            var username = user.Username;

            // Remove user roles first
            _context.UserRoles.RemoveRange(user.UserRoles);
            
            // Remove user
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Log action
            _context.Logs.Add(new Log
            {
                UserId = int.TryParse(currentUserId, out var uid) ? uid : null,
                Action = $"Xóa tài khoản #{id}",
                TableName = "Users",
                ActionTime = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã xóa tài khoản '{username}' thành công!";
            return RedirectToAction("Users");
        }
    }
}
