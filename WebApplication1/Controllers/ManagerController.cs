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
        // TẠO CÔNG VIỆC MỚI - Chọn từ yêu cầu ĐÃ DUYỆT
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

            // Lấy danh sách yêu cầu ĐÃ DUYỆT (chưa được giao việc)
            var approvedRequests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .Where(r => r.Status == "Đã phê duyệt")
                .OrderByDescending(r => r.RequestDate)
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
            ViewData["ApprovedRequests"] = approvedRequests;
            ViewBag.Balance = totalDonations - totalExpenses;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTask(int requestId, int[] staffIds, decimal amount, string? note, DateTime? scheduledDate)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            // Validate có chọn nhân viên không
            if (staffIds == null || staffIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một nhân viên";
                return RedirectToAction("CreateTask");
            }

            // Validate ngày bắt đầu phải sau ngày hôm nay
            if (!scheduledDate.HasValue || scheduledDate.Value.Date <= DateTime.Today)
            {
                TempData["Error"] = "Ngày bắt đầu phải sau ngày hôm nay";
                return RedirectToAction("CreateTask");
            }

            // Kiểm tra yêu cầu có tồn tại và đã duyệt chưa
            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (request == null)
            {
                TempData["Error"] = "Không tìm thấy yêu cầu hỗ trợ";
                return RedirectToAction("CreateTask");
            }

            if (request.Status != "Đã phê duyệt")
            {
                TempData["Error"] = "Yêu cầu này chưa được duyệt hoặc đã được giao việc";
                return RedirectToAction("CreateTask");
            }

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
                TempData["Error"] = $"Số tiền vượt quá số dư hiện tại ({currentBalance:N0} VND)";
                return RedirectToAction("CreateTask");
            }

            // TẠO YÊU CẦU PHÊ DUYỆT NGÂN SÁCH thay vì tạo task trực tiếp
            var budgetApproval = new BudgetApproval
            {
                RequestType = "CreateTask",
                RequestedBy = userId.Value,
                RequestedAt = DateTime.Now,
                Amount = amount,
                Description = $"Tạo công việc hỗ trợ {request.Beneficiary?.FullName} - {request.Beneficiary?.BeneficiaryType}. Giao cho {staffIds.Length} nhân viên.",
                RelatedRequestId = requestId,
                Status = "Chờ duyệt",
                StaffIds = System.Text.Json.JsonSerializer.Serialize(staffIds),
                ScheduledDate = scheduledDate,
                ManagerNote = note
            };

            _context.BudgetApprovals.Add(budgetApproval);

            // Gửi thông báo cho Admin
            var admins = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToUpper() == "ADMIN")
                .Select(ur => ur.User)
                .ToListAsync();

            foreach (var admin in admins)
            {
                var notification = new Notification
                {
                    UserId = admin.UserId,
                    Title = "Yêu cầu phê duyệt ngân sách mới",
                    Message = $"Manager yêu cầu phê duyệt {amount:N0} VND để tạo công việc hỗ trợ {request.Beneficiary?.FullName}",
                    Type = "Yêu cầu ngân sách",
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            // Ghi log
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Yêu cầu phê duyệt ngân sách {amount:N0} VND để tạo công việc cho {staffIds.Length} nhân viên",
                TableName = "Budget_Approvals",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã gửi yêu cầu phê duyệt ngân sách {amount:N0} VND đến Admin. Vui lòng chờ phê duyệt!";
            return RedirectToAction("Tasks");
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
                // Nếu yêu cầu NHÂN LỰC -> chuyển sang trang chọn nhân viên
                if (task.SupportRequestType == "Nhân lực")
                {
                    task.SupportResponseStatus = "Đang xử lý";
                    task.SupportResponseNote = responseNote;
                    task.SupportResponseAt = DateTime.Now;
                    task.UpdatedAt = DateTime.Now;
                    await _context.SaveChangesAsync();
                    
                    return RedirectToAction("AssignHelpers", new { taskId = taskId });
                }

                // Nếu yêu cầu tiền, TẠO YÊU CẦU PHÊ DUYỆT NGÂN SÁCH thay vì cộng tiền trực tiếp
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

                    // Tạo yêu cầu phê duyệt ngân sách
                    var budgetApproval = new BudgetApproval
                    {
                        RequestType = "AdditionalSupport",
                        RequestedBy = userId.Value,
                        RequestedAt = DateTime.Now,
                        Amount = additionalAmount.Value,
                        Description = $"Hỗ trợ thêm {additionalAmount.Value:N0} VND cho nhân viên {task.AssignedStaff?.FullName} - Công việc #{taskId}. Lý do: {responseNote}",
                        RelatedTaskId = taskId,
                        Status = "Chờ duyệt"
                    };
                    _context.BudgetApprovals.Add(budgetApproval);

                    // Gửi thông báo cho Admin
                    var admins = await _context.UserRoles
                        .Include(ur => ur.User)
                        .Include(ur => ur.Role)
                        .Where(ur => ur.Role.RoleName.ToUpper() == "ADMIN")
                        .Select(ur => ur.User)
                        .ToListAsync();

                    foreach (var admin in admins)
                    {
                        var adminNotif = new Notification
                        {
                            UserId = admin.UserId,
                            Title = "Yêu cầu phê duyệt ngân sách mới",
                            Message = $"Manager yêu cầu phê duyệt {additionalAmount.Value:N0} VND để hỗ trợ thêm cho công việc #{taskId}",
                            Type = "Yêu cầu ngân sách",
                            IsRead = false,
                            CreatedAt = DateTime.Now
                        };
                        _context.Notifications.Add(adminNotif);
                    }

                    // Cập nhật task status để đánh dấu đang chờ phê duyệt
                    task.SupportResponseStatus = "Đang chờ Admin duyệt";
                    task.SupportResponseNote = $"Đã gửi yêu cầu phê duyệt {additionalAmount.Value:N0} VND đến Admin";
                    task.SupportResponseAt = DateTime.Now;
                    task.UpdatedAt = DateTime.Now;

                    // Ghi log
                    var log = new Log
                    {
                        UserId = userId.Value,
                        Action = $"Yêu cầu phê duyệt ngân sách {additionalAmount.Value:N0} VND cho công việc #{taskId}",
                        TableName = "Budget_Approvals",
                        ActionTime = DateTime.Now
                    };
                    _context.Logs.Add(log);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Đã gửi yêu cầu phê duyệt ngân sách {additionalAmount.Value:N0} VND đến Admin. Vui lòng chờ phê duyệt!";
                    return RedirectToAction("SupportRequests");
                }

                // Nếu không yêu cầu tiền, chỉ chấp nhận yêu cầu
                task.Status = "Đang thực hiện";
                task.SupportResponseStatus = "Đã duyệt";
                task.SupportResponseNote = $"Đã chấp nhận: {responseNote}";
            }
            else
            {
                // Từ chối yêu cầu
                task.Status = "Đang thực hiện";
                task.SupportResponseStatus = "Từ chối";
                task.SupportResponseNote = $"Không chấp nhận: {responseNote}";
            }

            task.SupportResponseAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            // Gửi thông báo cho nhân viên
            var notification = new Notification
            {
                UserId = task.AssignedStaffId!.Value,
                Title = accept ? "Yêu cầu hỗ trợ được chấp nhận" : "Yêu cầu hỗ trợ không được chấp nhận",
                Message = $"Công việc #{task.TaskId}: {(accept ? $"Manager đã chấp nhận yêu cầu. {responseNote}" : $"Manager từ chối. {responseNote}")}",
                Type = "Phản hồi hỗ trợ",
                RelatedTaskId = taskId,
                IsRead = false,
                CreatedAt = DateTime.Now
            };
            _context.Notifications.Add(notification);

            // Ghi log
            var responseLog = new Log
            {
                UserId = userId.Value,
                Action = $"{(accept ? "Chấp nhận" : "Từ chối")} yêu cầu hỗ trợ công việc #{taskId}",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(responseLog);

            await _context.SaveChangesAsync();

            TempData["Success"] = accept ? "Đã chấp nhận yêu cầu hỗ trợ!" : "Đã từ chối yêu cầu hỗ trợ!";
            return RedirectToAction("SupportRequests");
        }

        // Trang chọn nhân viên để hỗ trợ (khi yêu cầu nhân lực)
        public async Task<IActionResult> AssignHelpers(int taskId)
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

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

            // Lấy danh sách nhân viên KHÔNG đang thực hiện công việc khác
            var busyStaffIds = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Yêu cầu hỗ trợ")
                .Where(t => t.AssignedStaffId.HasValue)
                .Select(t => t.AssignedStaffId!.Value)
                .Distinct()
                .ToListAsync();

            // Loại bỏ nhân viên đang yêu cầu hỗ trợ
            busyStaffIds.Add(task.AssignedStaffId ?? 0);

            var availableStaff = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToUpper() == "STAFF")
                .Where(ur => !busyStaffIds.Contains(ur.UserId))
                .Select(ur => ur.User)
                .ToListAsync();

            ViewData["Task"] = task;
            ViewData["AvailableStaff"] = availableStaff;
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View();
        }

        // Gửi lời mời cho các nhân viên được chọn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendHelperInvitations(int taskId, int[] staffIds)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

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

            if (staffIds == null || staffIds.Length == 0)
            {
                TempData["Error"] = "Vui lòng chọn ít nhất một nhân viên";
                return RedirectToAction("AssignHelpers", new { taskId = taskId });
            }

            // Tạo lời mời cho mỗi nhân viên
            foreach (var staffId in staffIds)
            {
                // Kiểm tra đã mời chưa
                var existing = await _context.SupportHelpers
                    .FirstOrDefaultAsync(h => h.TaskId == taskId && h.StaffId == staffId);
                
                if (existing != null) continue;

                var helper = new SupportHelper
                {
                    TaskId = taskId,
                    StaffId = staffId,
                    InvitedBy = userId.Value,
                    InvitedAt = DateTime.Now,
                    Status = "Đang chờ"
                };
                _context.SupportHelpers.Add(helper);

                // Gửi thông báo cho nhân viên
                var notification = new Notification
                {
                    UserId = staffId,
                    Title = "Lời mời hỗ trợ công việc",
                    Message = $"Bạn được mời hỗ trợ {task.AssignedStaff?.FullName} trong công việc hỗ trợ {task.SupportRequest?.Beneficiary?.FullName}. Vui lòng xác nhận tham gia!",
                    Type = "Lời mời hỗ trợ",
                    RelatedTaskId = taskId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            // QUAN TRỌNG: Chuyển status sang "Đã gửi lời mời" để ẩn khỏi danh sách yêu cầu hỗ trợ
            task.Status = "Đã gửi lời mời";
            task.SupportResponseStatus = "Đã gửi lời mời";
            task.UpdatedAt = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã gửi lời mời đến {staffIds.Length} nhân viên!";
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
                    .Where(b => b.Status == "Đã duyệt")
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

        // =====================================================
        // QUẢN LÝ HỖ TRỢ (CHỈ YÊU CẦU HỖ TRỢ)
        // =====================================================
        
        public async Task<IActionResult> Support(string? status = null)
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var requests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
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
            if (!IsManager())
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            ViewData["CurrentStatus"] = status ?? "all";
            ViewData["PendingCount"] = await _context.Beneficiaries.CountAsync(b => b.Status == "Chờ duyệt");
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult CreateBeneficiary()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBeneficiary(string fullName, string beneficiaryType, string? address, string? description)
        {
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();

            var beneficiary = new Beneficiary
            {
                FullName = fullName,
                BeneficiaryType = beneficiaryType,
                Address = address,
                Description = description,
                Status = "Đã duyệt", // Manager thêm thì tự động duyệt
                CreatedBy = userId,
                CreatedAt = DateTime.Now
            };

            _context.Beneficiaries.Add(beneficiary);

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
            if (!IsManager())
                return Unauthorized();

            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null)
                return NotFound();

            beneficiary.Status = "Đã duyệt";
            
            var userId = GetUserId();
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
            TempData["Success"] = $"Đã duyệt đối tượng: {beneficiary.FullName}";
            return RedirectToAction("Beneficiaries");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectBeneficiary(int id)
        {
            if (!IsManager())
                return Unauthorized();

            var beneficiary = await _context.Beneficiaries.FindAsync(id);
            if (beneficiary == null)
                return NotFound();

            beneficiary.Status = "Từ chối";
            
            var userId = GetUserId();
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
            TempData["Success"] = $"Đã từ chối đối tượng: {beneficiary.FullName}";
            return RedirectToAction("Beneficiaries");
        }

        // =====================================================
        // QUẢN LÝ YÊU CẦU HỖ TRỢ (SUPPORT REQUESTS) - Để giao việc
        // =====================================================
        
        public async Task<IActionResult> Requests(string? status)
        {
            if (!IsManager())
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            ViewData["CurrentStatus"] = status ?? "all";
            ViewData["PendingCount"] = await _context.SupportRequests.CountAsync(r => r.Status == "Chờ xét duyệt");
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
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
            if (!IsManager())
                return Unauthorized();

            var userId = GetUserId();
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
            if (!IsManager())
                return Unauthorized();

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id);
            
            if (request == null)
                return NotFound();

            request.Status = "Đã phê duyệt";
            
            var userId = GetUserId();
            
            // Tạo Approval record
            var approval = new Approval
            {
                RequestId = id,
                ApprovedBy = userId ?? 1,
                ApprovalDate = DateTime.Now,
                Result = "Phê duyệt",
                Note = "Duyệt bởi Manager"
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
            TempData["Success"] = $"Đã phê duyệt yêu cầu hỗ trợ cho {request.Beneficiary.FullName}";
            return RedirectToAction("Support");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRequest(int id, string? note)
        {
            if (!IsManager())
                return Unauthorized();

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id);
            
            if (request == null)
                return NotFound();

            request.Status = "Từ chối";
            
            var userId = GetUserId();
            
            // Tạo Approval record
            var approval = new Approval
            {
                RequestId = id,
                ApprovedBy = userId ?? 1,
                ApprovalDate = DateTime.Now,
                Result = "Từ chối",
                Note = note ?? "Từ chối bởi Manager"
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
            TempData["Success"] = $"Đã từ chối yêu cầu hỗ trợ";
            return RedirectToAction("Support");
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
