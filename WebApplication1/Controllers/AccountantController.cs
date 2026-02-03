using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;
using QLDuLichRBAC_Upgrade.Services;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AccountantController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;
        private readonly PaymentService _paymentService;

        public AccountantController(QLQuyTinhThuongContext context, PaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }

        private bool IsAccountant()
            => string.Equals((HttpContext.Session.GetString("Role") ?? "").Trim(),
                             "ACCOUNTANT",
                             StringComparison.OrdinalIgnoreCase);

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        private async Task SetUnreadNotificationsCount()
        {
            var userId = GetUserId();
            if (userId.HasValue)
            {
                var count = await _context.Notifications.CountAsync(n => n.UserId == userId.Value && !n.IsRead);
                ViewData["UnreadNotifications"] = count;
            }
            else
            {
                ViewData["UnreadNotifications"] = 0;
            }
        }

        // =====================================================
        // DASHBOARD - Hiển thị tổng quan cho khách hàng
        // =====================================================
        public async Task<IActionResult> Index()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();

            // Lấy số thông báo chưa đọc
            var unreadNotifications = userId.HasValue
                ? await _context.Notifications.CountAsync(n => n.UserId == userId.Value && !n.IsRead)
                : 0;

            // Lấy danh sách quyên góp của bản thân (qua DonorUserId trong Donations)
            var myDonations = await _context.Donations
                .Where(d => d.DonorUserId == userId)
                .SumAsync(d => d.Amount);

            var myDonationCount = await _context.Donations
                .Where(d => d.DonorUserId == userId)
                .CountAsync();

            // Lấy số hỗ trợ đã hoàn thành (để khách hàng theo dõi)
            var completedSupports = await _context.SupportTasks
                .CountAsync(t => t.Status == "Hoàn thành");

            var vm = new AccountantDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Khách hàng",
                UnreadNotifications = unreadNotifications,
                MyTotalDonations = myDonations,
                MyDonationCount = myDonationCount,
                CompletedSupports = completedSupports
            };

            return View("Dashboard", vm);
        }

        // =====================================================
        // CHUYỂN TIỀN VÀO QUỸ - Trang chính cho khách hàng donate
        // =====================================================
        public async Task<IActionResult> Donate()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            await SetUnreadNotificationsCount();
            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View();
        }

        // API để lấy QR Code cho thanh toán donate
        [HttpGet]
        public IActionResult GetDonateQR(decimal amount)
        {
            if (!IsAccountant())
                return Unauthorized();

            try
            {
                var userId = GetUserId();
                // Tạo memo cho giao dịch
                string memo = $"DONATE{userId ?? 0:D4}{DateTime.Now:MMddHHmm}";

                // Tạo chuỗi VietQR
                string qrData = _paymentService.GenerateVietQRData(
                    "1518893947588", // Số tài khoản
                    "970437",         // Mã ngân hàng MB
                    (int)amount,
                    memo
                );

                // Tạo QR Code base64
                string qrBase64 = _paymentService.GenerateQRCodeBase64(qrData);

                return Json(new
                {
                    success = true,
                    qrCode = $"data:image/png;base64,{qrBase64}",
                    memo = memo,
                    amount = amount,
                    bankName = "MB Bank",
                    accountNumber = "1518893947588",
                    accountName = "PHAN CONG TRI"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Xác nhận đã chuyển tiền donate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmDonation(decimal amount)
        {
            if (!IsAccountant())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            try
            {
                // Tạo Donor nếu chưa có (hoặc dùng donor mặc định)
                var user = await _context.Users.FindAsync(userId.Value);
                
                // Tìm hoặc tạo Donor cho user này
                var donor = await _context.Donors.FirstOrDefaultAsync(d => d.Email == user!.Email);
                if (donor == null)
                {
                    donor = new Donor
                    {
                        DonorName = user!.FullName,
                        DonorType = "Cá nhân",
                        Email = user.Email,
                        Phone = user.Phone
                    };
                    _context.Donors.Add(donor);
                    await _context.SaveChangesAsync();
                }

                // Tạo Donation
                var donation = new Donation
                {
                    DonorId = donor.DonorId,
                    DonorUserId = userId.Value,
                    Amount = amount,
                    DonationDate = DateTime.Now,
                    Method = "Chuyển khoản",
                    IsConfirmed = true // Khách hàng tự xác nhận
                };
                _context.Donations.Add(donation);

                // Ghi log
                var log = new Log
                {
                    UserId = userId.Value,
                    Action = $"Đã quyên góp {amount:N0} VND vào quỹ",
                    TableName = "Donations",
                    ActionTime = DateTime.Now
                };
                _context.Logs.Add(log);

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cảm ơn bạn đã quyên góp!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // =====================================================
        // LỊCH SỬ QUYÊN GÓP CỦA TÔI
        // =====================================================
        public async Task<IActionResult> MyDonations()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            await SetUnreadNotificationsCount();

            var donations = await _context.Donations
                .Where(d => d.DonorUserId == userId.Value)
                .OrderByDescending(d => d.DonationDate)
                .Select(d => new MyDonationVm
                {
                    DonationId = d.DonationId,
                    Amount = d.Amount,
                    DonationDate = d.DonationDate,
                    Method = d.Method,
                    IsConfirmed = d.IsConfirmed ?? false
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            ViewData["TotalAmount"] = donations.Sum(d => d.Amount);
            return View(donations);
        }

        // =====================================================
        // XEM CÁC HỖ TRỢ ĐÃ HOÀN THÀNH - từ thông báo
        // =====================================================
        public async Task<IActionResult> CompletedSupports()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            await SetUnreadNotificationsCount();

            var supports = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .Where(t => t.Status == "Hoàn thành")
                .OrderByDescending(t => t.StaffCompletedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View(supports);
        }

        // Xem chi tiết 1 hỗ trợ
        public async Task<IActionResult> SupportDetail(int id)
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            await SetUnreadNotificationsCount();

            var support = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .Include(t => t.Assigner)
                .Include(t => t.Complaints)
                .FirstOrDefaultAsync(t => t.TaskId == id);

            if (support == null)
                return NotFound();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View(support);
        }

        // =====================================================
        // THÔNG BÁO
        // =====================================================
        public async Task<IActionResult> Notifications()
        {
            if (!IsAccountant())
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View(notifications);
        }

        // =====================================================
        // GỬI PHẢN ÁNH VỀ 1 HỖ TRỢ ĐÃ HOÀN THÀNH
        // =====================================================
        [HttpGet]
        public async Task<IActionResult> SubmitFeedback(int id)
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            await SetUnreadNotificationsCount();

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.AssignedStaff)
                .FirstOrDefaultAsync(t => t.TaskId == id && t.Status == "Hoàn thành");

            if (task == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin hỗ trợ đã hoàn thành";
                return RedirectToAction("CompletedSupports");
            }

            // Kiểm tra đã phản ánh chưa
            var existingComplaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.TaskId == id && c.UserId == userId.Value);
            if (existingComplaint != null)
            {
                TempData["Error"] = "Bạn đã gửi phản ánh cho hỗ trợ này rồi";
                return RedirectToAction("SupportDetail", new { id = id });
            }

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitFeedback(int taskId, string content)
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .FirstOrDefaultAsync(t => t.TaskId == taskId);

            if (task == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin hỗ trợ";
                return RedirectToAction("CompletedSupports");
            }

            if (task.Status != "Hoàn thành")
            {
                TempData["Error"] = "Chỉ có thể phản ánh về hỗ trợ đã hoàn thành";
                return RedirectToAction("SupportDetail", new { id = taskId });
            }

            // Kiểm tra đã quá 3 ngày chưa
            if (task.StaffCompletedAt.HasValue && (DateTime.Now - task.StaffCompletedAt.Value).TotalDays < 3)
            {
                TempData["Error"] = "Chưa đủ 3 ngày để gửi phản ánh";
                return RedirectToAction("SupportDetail", new { id = taskId });
            }

            // Kiểm tra đã phản ánh chưa
            var existingComplaint = await _context.Complaints
                .FirstOrDefaultAsync(c => c.TaskId == taskId && c.UserId == userId.Value);
            if (existingComplaint != null)
            {
                TempData["Error"] = "Bạn đã gửi phản ánh cho hỗ trợ này rồi";
                return RedirectToAction("SupportDetail", new { id = taskId });
            }

            // Tạo phản ánh
            var complaint = new Complaint
            {
                TaskId = taskId,
                UserId = userId.Value,
                Content = content,
                Status = "Chờ xử lý",
                CreatedAt = DateTime.Now
            };
            _context.Complaints.Add(complaint);

            // Gửi thông báo cho Admin và Manager
            var adminAndManagers = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToUpper() == "ADMIN" || ur.Role.RoleName.ToUpper() == "MANAGER")
                .Select(ur => ur.UserId)
                .Distinct()
                .ToListAsync();

            foreach (var recipientId in adminAndManagers)
            {
                var notification = new Notification
                {
                    UserId = recipientId,
                    Title = "Có phản ánh mới từ khách hàng",
                    Message = $"Phản ánh về hỗ trợ cho {task.SupportRequest.Beneficiary.FullName}. Nội dung: {content.Substring(0, Math.Min(100, content.Length))}...",
                    Type = "Phản ánh mới",
                    RelatedTaskId = taskId,
                    IsRead = false,
                    CreatedAt = DateTime.Now
                };
                _context.Notifications.Add(notification);
            }

            // Ghi log
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Gửi phản ánh về hỗ trợ #{taskId}",
                TableName = "Complaints",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã gửi phản ánh thành công!";
            return RedirectToAction("CompletedSupports");
        }

        // =====================================================
        // BÁO CÁO - Thống kê đơn giản cho khách hàng
        // =====================================================
        public async Task<IActionResult> Reports()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();

            // Tổng tiền quỹ đã nhận
            var totalDonations = await _context.Donations
                .Where(d => d.IsConfirmed == true)
                .SumAsync(d => d.Amount);

            // Tổng tiền đã chi
            var totalExpenses = await _context.SupportTasks
                .Where(t => t.Status == "Đang thực hiện" || t.Status == "Hoàn thành" || t.Status == "Yêu cầu hỗ trợ")
                .SumAsync(t => t.Amount + t.AdditionalAmount);

            // Thống kê của bản thân
            var myDonations = userId.HasValue
                ? await _context.Donations.Where(d => d.DonorUserId == userId).SumAsync(d => d.Amount)
                : 0;

            var myDonationCount = userId.HasValue
                ? await _context.Donations.Where(d => d.DonorUserId == userId).CountAsync()
                : 0;

            var vm = new AccountantReportVm
            {
                TotalDonations = totalDonations,
                TotalExpenses = totalExpenses,
                CurrentBalance = totalDonations - totalExpenses,
                MyDonations = myDonations,
                MyDonationCount = myDonationCount,
                TotalCompletedSupports = await _context.SupportTasks.CountAsync(t => t.Status == "Hoàn thành"),
                TotalBeneficiaries = await _context.Beneficiaries.CountAsync()
            };

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Khách hàng";
            return View(vm);
        }
    }

    // ViewModels cho Accountant
    public class MyDonationVm
    {
        public int DonationId { get; set; }
        public decimal Amount { get; set; }
        public DateTime DonationDate { get; set; }
        public string Method { get; set; } = string.Empty;
        public bool IsConfirmed { get; set; }
    }

    public class AccountantReportVm
    {
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal MyDonations { get; set; }
        public int MyDonationCount { get; set; }
        public int TotalCompletedSupports { get; set; }
        public int TotalBeneficiaries { get; set; }
    }
}
