using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class ManagerController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;

        public ManagerController(QLQuyTinhThuongContext context)
        {
            _context = context;
        }

        private bool IsManager()
            => string.Equals((HttpContext.Session.GetString("Role") ?? "").Trim(),
                             "MANAGER",
                             StringComparison.OrdinalIgnoreCase);

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        // =====================================================
        // DASHBOARD - Hiển thị tổng quan tài chính
        // =====================================================
        public async Task<IActionResult> Index()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();

            // Tổng tiền quyên góp (đã xác nhận)
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            // Tổng tiền đã chi (các task đã bắt đầu thực hiện hoặc hoàn thành)
            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            // Số dư hiện tại
            var currentBalance = totalDonations - totalExpenses;

            // Đếm thông báo chưa đọc
            var unreadNotifications = userId.HasValue
                ? await _context.Notifications.CountAsync(n => n.UserId == userId.Value && !n.IsRead)
                : 0;

            // Số yêu cầu hỗ trợ từ nhân viên đang chờ
            var pendingSupportRequests = await _context.SupportTasks
                .CountAsync(t => t.Status == "Yêu cầu hỗ trợ");

            // Số công việc đang thực hiện
            var tasksInProgress = await _context.SupportTasks
                .CountAsync(t => t.Status == "Đang thực hiện");

            // Số công việc chờ thực hiện
            var pendingTasks = await _context.SupportTasks
                .CountAsync(t => t.Status == "Chờ thực hiện");

            // Số công việc hoàn thành hôm nay
            var completedToday = await _context.SupportTasks
                .CountAsync(t => t.Status == "Hoàn thành" && t.StaffCompletedAt.HasValue && t.StaffCompletedAt.Value.Date == DateTime.Today);

            var vm = new ManagerDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Manager",
                UnreadNotifications = unreadNotifications,

                // Tài chính
                TotalDonations = totalDonations,
                TotalExpenses = totalExpenses,
                CurrentBalance = currentBalance,

                // Công việc
                PendingTasks = pendingTasks,
                TasksInProgress = tasksInProgress,
                PendingSupportRequests = pendingSupportRequests,
                CompletedToday = completedToday,

                TotalBeneficiaries = await _context.Beneficiaries.CountAsync(),

                // Yêu cầu hỗ trợ từ nhân viên
                SupportRequests = await _context.SupportTasks
                    .Include(t => t.SupportRequest)
                        .ThenInclude(r => r.Beneficiary)
                    .Include(t => t.AssignedStaff)
                    .Where(t => t.Status == "Yêu cầu hỗ trợ")
                    .OrderByDescending(t => t.SupportRequestAt)
                    .Take(5)
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        // =====================================================
        // TẠO CÔNG VIỆC MỚI - Chọn nhân viên, người thụ hưởng, nhập số tiền
        // =====================================================
        public async Task<IActionResult> CreateTask()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            // Lấy danh sách nhân viên
            var staffList = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToUpper() == "STAFF")
                .Select(ur => ur.User)
                .ToListAsync();

            // Lấy danh sách người thụ hưởng
            var beneficiaries = await _context.Beneficiaries
                .OrderBy(b => b.FullName)
                .ToListAsync();

            // Tính số dư hiện tại
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            ViewData["StaffList"] = staffList;
            ViewData["Beneficiaries"] = beneficiaries;
            ViewData["CurrentBalance"] = totalDonations - totalExpenses;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(int beneficiaryId, int staffId, decimal amount, string? note)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            // Kiểm tra số dư
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            var currentBalance = totalDonations - totalExpenses;

            if (amount > currentBalance)
            {
                return Json(new { success = false, message = $"Số tiền vượt quá số dư hiện tại ({currentBalance:N0} VND)" });
            }

            var beneficiary = await _context.Beneficiaries.FindAsync(beneficiaryId);
            if (beneficiary == null)
                return Json(new { success = false, message = "Không tìm thấy người thụ hưởng" });

            // Tạo SupportRequest (không cần tiền vì tiền ở Task)
            var request = new SupportRequest
            {
                BeneficiaryId = beneficiaryId,
                RequestedAmount = 0, // Không dùng nữa
                Reason = note ?? "Hỗ trợ theo chỉ định của quản lý",
                Status = "Đã duyệt",
                RequestDate = DateTime.Now
            };
            _context.SupportRequests.Add(request);
            await _context.SaveChangesAsync();

            // Tạo SupportTask
            var task = new SupportTask
            {
                RequestId = request.RequestId,
                AssignedStaffId = staffId,
                AssignedBy = userId.Value,
                AssignedAt = DateTime.Now,
                Amount = amount,
                Status = "Chờ thực hiện",
                ManagerNote = note,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };
            _context.SupportTasks.Add(task);
            await _context.SaveChangesAsync();

            // Gửi thông báo cho nhân viên
            var notification = new Notification
            {
                UserId = staffId,
                Title = "Bạn có công việc mới",
                Message = $"Bạn được giao hỗ trợ cho {beneficiary.FullName} với số tiền {amount:N0} VND",
                Type = "Công việc mới",
                RelatedTaskId = task.TaskId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            // Ghi log
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Tạo công việc #{task.TaskId} cho nhân viên #{staffId}, số tiền {amount:N0} VND",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã tạo công việc thành công!" });
        }

        // =====================================================
        // DANH SÁCH CÔNG VIỆC
        // =====================================================
        public async Task<IActionResult> Tasks()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var tasks = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .Include(t => t.Assigner)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(tasks);
        }

        // =====================================================
        // THÔNG BÁO
        // =====================================================
        public async Task<IActionResult> Notifications()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var notifications = await _context.Notifications
                .Include(n => n.RelatedTask!)
                    .ThenInclude(t => t.SupportRequest)
                        .ThenInclude(r => r.Beneficiary)
                .Where(n => n.UserId == userId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            // Đánh dấu đã đọc
            foreach (var n in notifications.Where(x => !x.IsRead))
            {
                n.IsRead = true;
            }
            await _context.SaveChangesAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(notifications);
        }

        // =====================================================
        // YÊU CẦU HỖ TRỢ TỪ NHÂN VIÊN
        // =====================================================
        public async Task<IActionResult> SupportRequests()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var tasks = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .Where(t => t.Status == "Yêu cầu hỗ trợ")
                .OrderByDescending(t => t.SupportRequestAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(tasks);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleSupportRequest(int taskId, string action, decimal? additionalAmount, string responseNote)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var accept = action == "approve";

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                TempData["Error"] = "Không tìm thấy công việc";
                return RedirectToAction("SupportRequests");
            }

            if (accept)
            {
                // Nếu yêu cầu tiền, kiểm tra số dư
                if (task.SupportRequestType == "Tiền" && additionalAmount.HasValue && additionalAmount > 0)
                {
                    var totalDonations = await _context.Donations
                        .Where(d => d.IsConfirmed == true)
                        .SumAsync(d => d.Amount);

                    var totalExpenses = await _context.SupportTasks
                        .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                        .SumAsync(t => t.Amount + t.AdditionalAmount);

                    var currentBalance = totalDonations - totalExpenses;

                    if (additionalAmount > currentBalance)
                    {
                        TempData["Error"] = $"Số tiền vượt quá số dư hiện tại ({currentBalance:N0} VND)";
                        return RedirectToAction("SupportRequests");
                    }

                    // Cộng tiền hỗ trợ
                    task.AdditionalAmount += additionalAmount.Value;
                }

                task.Status = "Đang thực hiện";
                task.SupportResponseNote = $"Đã chấp nhận: {responseNote}";
            }
            else
            {
                task.Status = "Đang thực hiện";
                task.SupportResponseNote = $"Không chấp nhận: {responseNote}";
            }

            task.SupportResponseAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            // Gửi thông báo cho nhân viên
            var notification = new Notification
            {
                UserId = task.AssignedStaffId!.Value,
                Title = accept ? "Yêu cầu hỗ trợ được chấp nhận" : "Yêu cầu hỗ trợ không được chấp nhận",
                Message = $"Công việc #{task.TaskId}: {(accept ? $"Đã chấp nhận hỗ trợ. {(additionalAmount > 0 ? $"Thêm {additionalAmount:N0} VND." : "")}" : "")} {responseNote}",
                Type = "Phản hồi hỗ trợ",
                RelatedTaskId = taskId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            // Ghi log
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"{(accept ? "Chấp nhận" : "Từ chối")} yêu cầu hỗ trợ công việc #{taskId}",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = accept ? "Đã chấp nhận yêu cầu hỗ trợ!" : "Đã từ chối yêu cầu hỗ trợ!";
            return RedirectToAction("SupportRequests");
        }

        // =====================================================
        // PHẢN HỒI PHẢN ÁNH TỪ KHÁCH HÀNG
        // =====================================================
        public async Task<IActionResult> Complaints()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var complaints = await _context.Complaints
                .Include(c => c.Task)
                    .ThenInclude(t => t.SupportRequest)
                        .ThenInclude(r => r.Beneficiary)
                .Include(c => c.User)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(complaints);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResponseComplaint(int complaintId, string responseContent)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var complaint = await _context.Complaints
                .Include(c => c.Task)
                .FirstOrDefaultAsync(c => c.ComplaintId == complaintId);

            if (complaint == null)
            {
                TempData["Error"] = "Không tìm thấy phản ánh";
                return RedirectToAction("Complaints");
            }

            // Cập nhật phản ánh
            complaint.ResponseBy = userId.Value;
            complaint.ResponseContent = responseContent;
            complaint.ResponseAt = DateTime.Now;
            complaint.Status = "Đã phản hồi";

            // Gửi thông báo cho người phản ánh
            var notification = new Notification
            {
                UserId = complaint.UserId,
                Title = "Phản ánh đã được phản hồi",
                Message = $"Phản ánh của bạn đã được phản hồi: {responseContent}",
                Type = "Phản hồi phản ánh",
                RelatedTaskId = complaint.TaskId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã phản hồi phản ánh!";
            return RedirectToAction("Complaints");
        }

        // =====================================================
        // BÁO CÁO
        // =====================================================
        public async Task<IActionResult> Reports()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            // Tổng tiền quyên góp
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            // Tổng tiền đã chi
            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            var vm = new ManagerReportVm
            {
                TotalDonations = totalDonations,
                TotalExpenses = totalExpenses,
                CurrentBalance = totalDonations - totalExpenses,

                TotalTasks = await _context.SupportTasks.CountAsync(),
                CompletedTasks = await _context.SupportTasks.CountAsync(t => t.Status == "Hoàn thành"),
                InProgressTasks = await _context.SupportTasks.CountAsync(t => t.Status == "Đang thực hiện"),
                PendingTasks = await _context.SupportTasks.CountAsync(t => t.Status == "Chờ thực hiện"),

                ThisMonthTasks = await _context.SupportTasks
                    .CountAsync(t => t.CreatedAt >= thisMonth),

                BeneficiaryStats = await _context.Beneficiaries
                    .GroupBy(b => b.BeneficiaryType)
                    .Select(g => new BeneficiaryStatVm
                    {
                        Type = g.Key,
                        Count = g.Count()
                    })
                    .ToListAsync()
            };

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(vm);
        }
    }

    public class ManagerReportVm
    {
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int InProgressTasks { get; set; }
        public int PendingTasks { get; set; }
        public int ThisMonthTasks { get; set; }
        public List<BeneficiaryStatVm> BeneficiaryStats { get; set; } = new();
    }

    public class BeneficiaryStatVm
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
