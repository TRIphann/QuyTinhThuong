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

            // Tính số dư quỹ theo cách thống nhất: Tổng quyên góp - Tổng chi
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            var currentBalance = totalDonations - totalExpenses;

            var vm = new AdminDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Admin",

                TotalUsers = await _context.Users.CountAsync(),
                TotalSupportRequests = await _context.SupportRequests.CountAsync(),

                // Số dư quỹ tính theo công thức thống nhất
                FundBalance = currentBalance,

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

            // Total amounts - tính thống nhất
            var totalDonationsConfirmed = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalTaskExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            vm.TotalDonations = await _context.Donations.SumAsync(d => d.Amount);
            vm.TotalExpenses = totalTaskExpenses;
            vm.FundBalance = totalDonationsConfirmed - totalTaskExpenses;

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

        // =====================================================
        // QUẢN LÝ HỖ TRỢ (CHỈ YÊU CẦU HỖ TRỢ)
        // =====================================================
        
        public async Task<IActionResult> Support(string? status = null)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var requests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["Status"] = status ?? "all";
            ViewData["PendingRequests"] = requests.Count(r => r.Status == "Chờ xét duyệt");
            ViewData["Requests"] = requests;
            
            return View();
        }

        // =====================================================
        // QUẢN LÝ ĐỐI TƯỢNG HỖ TRỢ (BENEFICIARIES)
        // =====================================================
        
        public async Task<IActionResult> Beneficiaries(string? status)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var query = _context.Beneficiaries
                .Include(b => b.Creator)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(b => b.Status == status);
            }

            var beneficiaries = await query
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["CurrentStatus"] = status ?? "all";
            ViewData["PendingCount"] = await _context.Beneficiaries.CountAsync(b => b.Status == "Chờ duyệt");
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult CreateBeneficiary()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBeneficiary(string fullName, string beneficiaryType, string? address, string? description)
        {
            if (!IsAdmin())
                return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");

            var beneficiary = new Beneficiary
            {
                FullName = fullName,
                BeneficiaryType = beneficiaryType,
                Address = address,
                Description = description,
                Status = "Đã duyệt", // Admin thêm thì tự động duyệt
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            _context.Beneficiaries.Add(beneficiary);

            // Log
            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Thêm đối tượng hỗ trợ: {fullName}",
                    TableName = "Beneficiaries",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm đối tượng hỗ trợ: {fullName}";
            return RedirectToAction("Beneficiaries");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBeneficiary(int id)
        {
            if (!IsAdmin())
                return Unauthorized();

            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null)
                return NotFound();

            beneficiary.Status = "Đã duyệt";
            
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Duyệt đối tượng hỗ trợ: {beneficiary.FullName}",
                    TableName = "Beneficiaries",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã duyệt đối tượng: {beneficiary.FullName}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBeneficiary(int id)
        {
            if (!IsAdmin())
                return Unauthorized();

            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null)
                return NotFound();

            beneficiary.Status = "Từ chối";
            
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Từ chối đối tượng hỗ trợ: {beneficiary.FullName}",
                    TableName = "Beneficiaries",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã từ chối đối tượng: {beneficiary.FullName}" });
        }

        // =====================================================
        // QUẢN LÝ YÊU CẦU HỖ TRỢ (SUPPORT REQUESTS)
        // =====================================================
        
        public async Task<IActionResult> Requests(string? status)
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var query = _context.SupportRequests
                .Include(r => r.Beneficiary)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(r => r.Status == status);
            }

            var requests = await query
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["CurrentStatus"] = status ?? "all";
            ViewData["PendingCount"] = await _context.SupportRequests.CountAsync(r => r.Status == "Chờ xét duyệt");
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["Beneficiaries"] = await _context.Beneficiaries
                .Where(b => b.Status == "Đã duyệt")
                .OrderBy(b => b.FullName)
                .ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(int beneficiaryId, decimal requestedAmount, string reason)
        {
            if (!IsAdmin())
                return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            var beneficiary = await _context.Beneficiaries.FindAsync(beneficiaryId);
            if (beneficiary == null)
                return NotFound();

            var request = new SupportRequest
            {
                BeneficiaryId = beneficiaryId,
                RequestDate = DateTime.Now,
                RequestedAmount = requestedAmount,
                Reason = reason,
                Status = "Chờ xét duyệt"
            };

            _context.SupportRequests.Add(request);

            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Tạo yêu cầu hỗ trợ cho {beneficiary.FullName}: {requestedAmount:N0} VND",
                    TableName = "SupportRequests",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo yêu cầu hỗ trợ cho {beneficiary.FullName}";
            return RedirectToAction("Requests");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRequest(int id)
        {
            if (!IsAdmin())
                return Unauthorized();

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id);
            
            if (request == null)
                return NotFound();

            request.Status = "Đã phê duyệt";
            
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Tạo Approval record
            var approval = new Approval
            {
                RequestId = id,
                ApprovedBy = userId ?? 1,
                ApprovalDate = DateTime.Now,
                Result = "Phê duyệt",
                Note = "Duyệt bởi Admin"
            };
            _context.Approvals.Add(approval);

            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Phê duyệt yêu cầu hỗ trợ #{id} cho {request.Beneficiary.FullName}",
                    TableName = "SupportRequests",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã phê duyệt yêu cầu hỗ trợ cho {request.Beneficiary.FullName}" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id, string? note)
        {
            if (!IsAdmin())
                return Unauthorized();

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id);
            
            if (request == null)
                return NotFound();

            request.Status = "Từ chối";
            
            var userId = HttpContext.Session.GetInt32("UserId");
            
            // Tạo Approval record
            var approval = new Approval
            {
                RequestId = id,
                ApprovedBy = userId ?? 1,
                ApprovalDate = DateTime.Now,
                Result = "Từ chối",
                Note = note ?? "Từ chối bởi Admin"
            };
            _context.Approvals.Add(approval);

            if (userId.HasValue)
            {
                _context.Logs.Add(new Log
                {
                    UserId = userId.Value,
                    Action = $"Từ chối yêu cầu hỗ trợ #{id} cho {request.Beneficiary.FullName}",
                    TableName = "SupportRequests",
                    ActionTime = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true, message = $"Đã từ chối yêu cầu hỗ trợ" });
        }

        // =====================================================
        // QUẢN LÝ TÀI CHÍNH - TAB TỔNG QUAN
        // =====================================================
        public async Task<IActionResult> FinanceOverview()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            // Tính số dư quỹ
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            var currentBalance = totalDonations - totalExpenses;

            // Lịch sử giao dịch (Donations + Expenses) - 30 ngày gần đây
            var last30Days = DateTime.Today.AddDays(-29);

            var donations = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ReceivedByUser)
                .Where(d => d.DonationDate >= last30Days)
                .OrderByDescending(d => d.DonationDate)
                .Select(d => new
                {
                    Type = "Tiền vào",
                    Date = d.DonationDate,
                    Amount = d.Amount,
                    Description = $"Quyên góp từ {d.Donor.DonorName}",
                    Method = d.Method,
                    Handler = d.ReceivedByUser != null ? d.ReceivedByUser.FullName : "Hệ thống"
                })
                .ToListAsync();

            var expenses = await _context.Expenses
                .Include(e => e.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(e => e.PaidByUser)
                .Where(e => e.ExpenseDate >= last30Days)
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new
                {
                    Type = "Tiền ra",
                    Date = e.ExpenseDate,
                    Amount = e.Amount,
                    Description = $"Chi trả cho {(e.SupportRequest != null && e.SupportRequest.Beneficiary != null ? e.SupportRequest.Beneficiary.FullName : "N/A")}",
                    Method = e.PaymentMethod,
                    Handler = e.PaidByUser != null ? e.PaidByUser.FullName : "N/A"
                })
                .ToListAsync();

            // Gộp và sắp xếp
            var transactions = donations.Cast<dynamic>()
                .Concat(expenses.Cast<dynamic>())
                .OrderByDescending(t => t.Date)
                .ToList();

            // Thống kê theo tháng (6 tháng gần đây)
            var monthlyStats = new List<dynamic>();
            for (int i = 5; i >= 0; i--)
            {
                var month = DateTime.Today.AddMonths(-i);
                var startOfMonth = new DateTime(month.Year, month.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1);

                var monthDonations = await _context.Donations
                    .Where(d => d.IsConfirmed == true && d.DonationDate >= startOfMonth && d.DonationDate < endOfMonth)
                    .SumAsync(d => d.Amount);

                var monthExpenses = await _context.Expenses
                    .Where(e => e.ExpenseDate >= startOfMonth && e.ExpenseDate < endOfMonth)
                    .SumAsync(e => e.Amount);

                monthlyStats.Add(new
                {
                    Month = month.ToString("MM/yyyy"),
                    Donations = monthDonations,
                    Expenses = monthExpenses,
                    Balance = monthDonations - monthExpenses
                });
            }

            // Số yêu cầu phê duyệt đang chờ
            var pendingApprovals = await _context.BudgetApprovals
                .CountAsync(b => b.Status == "Chờ duyệt");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["TotalDonations"] = totalDonations;
            ViewData["TotalExpenses"] = totalExpenses;
            ViewData["CurrentBalance"] = currentBalance;
            ViewData["Transactions"] = transactions;
            ViewData["MonthlyStats"] = monthlyStats;
            ViewData["PendingApprovals"] = pendingApprovals;

            return View();
        }

        // =====================================================
        // QUẢN LÝ TÀI CHÍNH - TAB XỬ LÝ YÊU CẦU
        // =====================================================
        public async Task<IActionResult> FinanceApprovals()
        {
            if (!IsAdmin())
                return RedirectToAction("Login", "Account");

            var approvals = await _context.BudgetApprovals
                .Include(b => b.Requester)
                .Include(b => b.RelatedRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(b => b.RelatedTask)
                    .ThenInclude(t => t.AssignedStaff)
                .OrderByDescending(b => b.RequestedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Admin";
            ViewData["PendingCount"] = approvals.Count(a => a.Status == "Chờ duyệt");

            return View(approvals);
        }

        // Phê duyệt yêu cầu ngân sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveBudgetRequest(int approvalId)
        {
            if (!IsAdmin())
                return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized();

            var approval = await _context.BudgetApprovals
                .Include(b => b.Requester)
                .Include(b => b.RelatedRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(b => b.RelatedTask)
                .FirstOrDefaultAsync(b => b.ApprovalId == approvalId);

            if (approval == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu";
                return RedirectToAction("FinanceApprovals");
            }

            if (approval.Status != "Chờ duyệt")
            {
                TempData["Error"] = "Yêu cầu đã được xử lý";
                return RedirectToAction("FinanceApprovals");
            }

            // Kiểm tra số dư
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            var currentBalance = totalDonations - totalExpenses;

            if (approval.Amount > currentBalance)
            {
                TempData["Error"] = $"Số dư không đủ! Hiện tại: {currentBalance:N0} VND, Yêu cầu: {approval.Amount:N0} VND";
                return RedirectToAction("FinanceApprovals");
            }

            // Cập nhật approval
            approval.Status = "Đã duyệt";
            approval.ApprovedBy = userId.Value;
            approval.ApprovedAt = DateTime.Now;

            // Xử lý theo loại yêu cầu
            if (approval.RequestType == "CreateTask")
            {
                // Tạo task cho các staff
                var staffIds = System.Text.Json.JsonSerializer.Deserialize<int[]>(approval.StaffIds ?? "[]");
                
                foreach (var staffId in staffIds ?? Array.Empty<int>())
                {
                    var task = new SupportTask
                    {
                        RequestId = approval.RelatedRequestId!.Value,
                        AssignedStaffId = staffId,
                        AssignedBy = approval.RequestedBy,
                        AssignedAt = DateTime.Now,
                        ScheduledDate = approval.ScheduledDate,
                        Amount = approval.Amount,
                        Status = "Chờ thực hiện",
                        ManagerNote = approval.ManagerNote,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.SupportTasks.Add(task);

                    // Gửi thông báo cho staff
                    var notification = new Notification
                    {
                        UserId = staffId,
                        Title = "Công việc mới được giao",
                        Message = $"Bạn được phân công hỗ trợ {approval.RelatedRequest?.Beneficiary?.FullName} với số tiền {approval.Amount:N0} VND",
                        Type = "Công việc mới",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(notification);
                }

                // Cập nhật status của request
                if (approval.RelatedRequest != null)
                {
                    approval.RelatedRequest.Status = "Đang hỗ trợ";
                }

                // Gửi thông báo cho Manager
                var managerNotif = new Notification
                {
                    UserId = approval.RequestedBy,
                    Title = "Yêu cầu ngân sách được chấp thuận",
                    Message = $"Admin đã chấp thuận yêu cầu tạo công việc với số tiền {approval.Amount:N0} VND",
                    Type = "Phê duyệt ngân sách",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(managerNotif);

                // Tạo record Expense để ghi lại giao dịch tiền ra
                var expense = new Expense
                {
                    RequestId = approval.RelatedRequestId!.Value,
                    Amount = approval.Amount,
                    ExpenseDate = DateTime.Now,
                    PaymentMethod = "Chuyển khoản",
                    PaidBy = userId.Value
                };
                _context.Expenses.Add(expense);
            }
            else if (approval.RequestType == "AdditionalSupport")
            {
                // Cộng tiền hỗ trợ cho task
                if (approval.RelatedTask != null)
                {
                    approval.RelatedTask.AdditionalAmount += approval.Amount;
                    approval.RelatedTask.Status = "Đang thực hiện";
                    approval.RelatedTask.SupportResponseStatus = "Đã duyệt";
                    approval.RelatedTask.SupportResponseNote = $"Admin đã duyệt thêm {approval.Amount:N0} VND";
                    approval.RelatedTask.SupportResponseAt = DateTime.Now;
                    approval.RelatedTask.UpdatedAt = DateTime.Now;

                    // Gửi thông báo cho Manager
                    var managerNotif = new Notification
                    {
                        UserId = approval.RequestedBy,
                        Title = "Yêu cầu ngân sách được chấp thuận",
                        Message = $"Admin đã chấp thuận yêu cầu hỗ trợ thêm {approval.Amount:N0} VND cho công việc #{approval.RelatedTaskId}",
                        Type = "Phê duyệt ngân sách",
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(managerNotif);

                    // Gửi thông báo cho Staff
                    if (approval.RelatedTask.AssignedStaffId.HasValue)
                    {
                        var staffNotif = new Notification
                        {
                            UserId = approval.RelatedTask.AssignedStaffId.Value,
                            Title = "Yêu cầu hỗ trợ được chấp nhận",
                            Message = $"Admin đã chấp thuận hỗ trợ thêm {approval.Amount:N0} VND cho công việc của bạn",
                            Type = "Phản hồi hỗ trợ",
                            RelatedTaskId = approval.RelatedTaskId,
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        };
                        _context.Notifications.Add(staffNotif);
                    }

                    // Tạo record Expense để ghi lại giao dịch tiền ra (hỗ trợ thêm)
                    var expense = new Expense
                    {
                        RequestId = approval.RelatedTask.RequestId,
                        Amount = approval.Amount,
                        ExpenseDate = DateTime.Now,
                        PaymentMethod = "Chuyển khoản",
                        PaidBy = userId.Value
                    };
                    _context.Expenses.Add(expense);
                }
            }

            // Ghi log
            _context.Logs.Add(new Log
            {
                UserId = userId.Value,
                Action = $"Phê duyệt yêu cầu ngân sách #{approvalId} - {approval.Amount:N0} VND",
                TableName = "Budget_Approvals",
                ActionTime = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã phê duyệt yêu cầu ngân sách {approval.Amount:N0} VND!";
            return RedirectToAction("FinanceApprovals");
        }

        // Từ chối yêu cầu ngân sách
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBudgetRequest(int approvalId, string rejectionReason)
        {
            if (!IsAdmin())
                return Unauthorized();

            var userId = HttpContext.Session.GetInt32("UserId");
            if (!userId.HasValue)
                return Unauthorized();

            var approval = await _context.BudgetApprovals
                .Include(b => b.Requester)
                .Include(b => b.RelatedTask)
                    .ThenInclude(t => t.AssignedStaff)
                .FirstOrDefaultAsync(b => b.ApprovalId == approvalId);

            if (approval == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu";
                return RedirectToAction("FinanceApprovals");
            }

            if (approval.Status != "Chờ duyệt")
            {
                TempData["Error"] = "Yêu cầu đã được xử lý";
                return RedirectToAction("FinanceApprovals");
            }

            // Cập nhật approval
            approval.Status = "Từ chối";
            approval.ApprovedBy = userId.Value;
            approval.ApprovedAt = DateTime.Now;
            approval.RejectionReason = rejectionReason;

            // Gửi thông báo cho Manager
            var managerNotif = new Notification
            {
                UserId = approval.RequestedBy,
                Title = "Yêu cầu ngân sách bị từ chối",
                Message = $"Admin đã từ chối yêu cầu {approval.RequestType} với số tiền {approval.Amount:N0} VND. Lý do: {rejectionReason}",
                Type = "Từ chối ngân sách",
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(managerNotif);

            // Nếu là AdditionalSupport, gửi thông báo cho Staff
            if (approval.RequestType == "AdditionalSupport" && approval.RelatedTask != null)
            {
                // Reset task về trạng thái Đang thực hiện
                approval.RelatedTask.Status = "Đang thực hiện";
                approval.RelatedTask.SupportResponseStatus = "Từ chối";
                approval.RelatedTask.SupportResponseNote = $"Admin từ chối: {rejectionReason}";
                approval.RelatedTask.SupportResponseAt = DateTime.Now;
                approval.RelatedTask.UpdatedAt = DateTime.Now;

                if (approval.RelatedTask.AssignedStaffId.HasValue)
                {
                    var staffNotif = new Notification
                    {
                        UserId = approval.RelatedTask.AssignedStaffId.Value,
                        Title = "Yêu cầu hỗ trợ bị từ chối",
                        Message = $"Admin đã từ chối yêu cầu hỗ trợ thêm {approval.Amount:N0} VND. Lý do: {rejectionReason}",
                        Type = "Từ chối hỗ trợ",
                        RelatedTaskId = approval.RelatedTaskId,
                        IsRead = false,
                        CreatedAt = DateTime.Now
                    };
                    _context.Notifications.Add(staffNotif);
                }
            }

            // Ghi log
            _context.Logs.Add(new Log
            {
                UserId = userId.Value,
                Action = $"Từ chối yêu cầu ngân sách #{approvalId} - {approval.Amount:N0} VND",
                TableName = "Budget_Approvals",
                ActionTime = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã từ chối yêu cầu ngân sách!";
            return RedirectToAction("FinanceApprovals");
        }
    }
}
