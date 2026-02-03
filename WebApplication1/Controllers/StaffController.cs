using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class StaffController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;

        public StaffController(QLQuyTinhThuongContext context)
        {
            _context = context;
        }

        private bool IsStaff()
            => string.Equals((HttpContext.Session.GetString("Role") ?? "").Trim(),
                             "STAFF",
                             StringComparison.OrdinalIgnoreCase);

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            var today = DateTime.Today;

            // Đếm thông báo chưa đọc
            var unreadNotifications = userId.HasValue 
                ? await _context.Notifications.CountAsync(n => n.UserId == userId.Value && !n.IsRead)
                : 0;

            // Đếm công việc của nhân viên
            var myTasks = userId.HasValue
                ? await _context.SupportTasks
                    .Include(t => t.SupportRequest)
                        .ThenInclude(r => r.Beneficiary)
                    .Where(t => t.AssignedStaffId == userId.Value)
                    .OrderByDescending(t => t.CreatedAt)
                    .ToListAsync()
                : new List<SupportTask>();

            var assignedTasks = myTasks.Count(t => t.Status != "Hoàn thành");
            var pendingTasks = myTasks.Count(t => t.Status == "Chờ thực hiện");
            var inProgressTasks = myTasks.Count(t => t.Status == "Đang thực hiện" || t.Status == "Yêu cầu hỗ trợ");
            var completedTasks = myTasks.Count(t => t.Status == "Hoàn thành");

            var vm = new StaffDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Nhân viên",
                UnreadNotifications = unreadNotifications,
                AssignedTasks = assignedTasks,
                PendingTasks = pendingTasks,
                InProgressTasks = inProgressTasks,
                CompletedTasks = completedTasks,
                MyTasks = myTasks.Where(t => t.Status != "Hoàn thành").Take(10).ToList(),

                TotalDonors = await _context.Donors.CountAsync(),
                TotalBeneficiaries = await _context.Beneficiaries.CountAsync(),

                DonationsToday = await _context.Donations
                    .CountAsync(d => d.DonationDate.Date == today),

                RequestsCreatedToday = await _context.SupportRequests
                    .CountAsync(r => r.RequestDate.Date == today),

                TodayDonationAmount = await _context.Donations
                    .Where(d => d.DonationDate.Date == today)
                    .SumAsync(d => d.Amount),

                RecentDonors = await _context.Donors
                    .OrderByDescending(d => d.DonorId)
                    .Take(5)
                    .Select(d => new DonorVm
                    {
                        DonorId = d.DonorId,
                        DonorName = d.DonorName,
                        DonorType = d.DonorType,
                        Phone = d.Phone,
                        Email = d.Email,
                        TotalDonated = d.Donations.Sum(dn => dn.Amount)
                    })
                    .ToListAsync(),

                RecentBeneficiaries = await _context.Beneficiaries
                    .OrderByDescending(b => b.BeneficiaryId)
                    .Take(5)
                    .Select(b => new BeneficiaryVm
                    {
                        BeneficiaryId = b.BeneficiaryId,
                        FullName = b.FullName,
                        BeneficiaryType = b.BeneficiaryType,
                        Address = b.Address,
                        TotalRequests = b.SupportRequests.Count
                    })
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        // Lấy danh sách thông báo
        public async Task<IActionResult> Notifications()
        {
            if (!IsStaff())
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(notifications);
        }

        // Danh sách công việc được giao
        public async Task<IActionResult> MyTasks()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (!userId.HasValue)
                return RedirectToAction("Login", "Account");

            var tasks = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.DonorUser)
                .Where(t => t.AssignedStaffId == userId.Value)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(tasks);
        }

        // Ghi nhận hoàn thành công việc - gửi thông báo cho TẤT CẢ khách hàng
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CompleteTask(int taskId, string staffNote)
        {
            if (!IsStaff())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.Assigner)
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedStaffId == userId.Value);

            if (task == null)
                return Json(new { success = false, message = "Không tìm thấy công việc" });

            // Cập nhật task
            task.Status = "Hoàn thành";
            task.StaffNote = staffNote;
            task.StaffCompletedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            // Gửi thông báo cho TẤT CẢ khách hàng (role ACCOUNTANT)
            var customers = await _context.UserRoles
                .Include(ur => ur.User)
                .Include(ur => ur.Role)
                .Where(ur => ur.Role.RoleName.ToUpper() == "ACCOUNTANT")
                .Select(ur => ur.User)
                .ToListAsync();

            foreach (var customer in customers)
            {
                var notification = new Notification
                {
                    UserId = customer.UserId,
                    Title = "Quỹ đã hỗ trợ thành công",
                    Message = $"Quỹ Tình Thương đã hỗ trợ {task.TotalAmount:N0} VND cho {task.SupportRequest.Beneficiary.FullName}. Ấn vào để xem chi tiết.",
                    Type = "Hỗ trợ hoàn thành",
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
                Action = $"Hoàn thành công việc #{taskId}",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã hoàn thành công việc!" });
        }

        // Nhân viên ấn "Thực hiện" - BẮT ĐẦU thực hiện, tiền được trừ
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StartTask(int taskId)
        {
            if (!IsStaff())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedStaffId == userId.Value);

            if (task == null)
                return Json(new { success = false, message = "Không tìm thấy công việc" });

            if (task.Status != "Chờ thực hiện")
                return Json(new { success = false, message = "Công việc không ở trạng thái chờ thực hiện" });

            // Cập nhật task - chuyển sang đang thực hiện
            task.Status = "Đang thực hiện";
            task.StartedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            // Ghi log
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Bắt đầu thực hiện công việc #{taskId}, trừ {task.Amount:N0} VND",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã bắt đầu thực hiện công việc!" });
        }

        // Nhân viên yêu cầu hỗ trợ (tiền hoặc nhân lực)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestSupport(int taskId, string supportType, decimal? amount, string reason)
        {
            if (!IsStaff())
                return Unauthorized();

            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized();

            var task = await _context.SupportTasks
                .Include(t => t.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(t => t.Assigner)
                .FirstOrDefaultAsync(t => t.TaskId == taskId && t.AssignedStaffId == userId.Value);

            if (task == null)
                return Json(new { success = false, message = "Không tìm thấy công việc" });

            // Cập nhật task
            task.Status = "Yêu cầu hỗ trợ";
            task.SupportRequestType = supportType;
            task.SupportRequestAmount = amount;
            task.SupportRequestReason = reason;
            task.SupportRequestAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;

            // Gửi thông báo cho Manager đã phân công
            if (task.AssignedBy.HasValue)
            {
                var staffName = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
                var notification = new Notification
                {
                    UserId = task.AssignedBy.Value,
                    Title = "Yêu cầu hỗ trợ từ nhân viên",
                    Message = $"{staffName} yêu cầu hỗ trợ {supportType} cho công việc hỗ trợ {task.SupportRequest.Beneficiary.FullName}. {(supportType == "Tiền" && amount.HasValue ? $"Số tiền: {amount:N0} VND. " : "")}Lý do: {reason}",
                    Type = "Yêu cầu hỗ trợ",
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
                Action = $"Yêu cầu hỗ trợ {supportType} cho công việc #{taskId}",
                TableName = "SupportTasks",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Đã gửi yêu cầu hỗ trợ!" });
        }

        #region Donors Management
        public async Task<IActionResult> Donors()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var donors = await _context.Donors
                .OrderByDescending(d => d.DonorId)
                .Select(d => new DonorVm
                {
                    DonorId = d.DonorId,
                    DonorName = d.DonorName,
                    DonorType = d.DonorType,
                    Phone = d.Phone,
                    Email = d.Email,
                    TotalDonated = d.Donations.Sum(dn => dn.Amount)
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(donors);
        }

        [HttpGet]
        public IActionResult CreateDonor()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDonor(DonorCreateVm model)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var donor = new Donor
            {
                DonorName = model.DonorName,
                DonorType = model.DonorType,
                Address = model.Address,
                Phone = model.Phone,
                Email = model.Email
            };

            _context.Donors.Add(donor);

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Thêm nhà hảo tâm: {model.DonorName}",
                TableName = "Donors",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm nhà hảo tâm: {model.DonorName}";
            return RedirectToAction("Donors");
        }
        #endregion

        #region Beneficiaries Management
        public async Task<IActionResult> Beneficiaries()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var beneficiaries = await _context.Beneficiaries
                .OrderByDescending(b => b.BeneficiaryId)
                .Select(b => new BeneficiaryVm
                {
                    BeneficiaryId = b.BeneficiaryId,
                    FullName = b.FullName,
                    BeneficiaryType = b.BeneficiaryType,
                    Address = b.Address,
                    TotalRequests = b.SupportRequests.Count
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(beneficiaries);
        }

        [HttpGet]
        public IActionResult CreateBeneficiary()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateBeneficiary(BeneficiaryCreateVm model)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiary = new Beneficiary
            {
                FullName = model.FullName,
                BeneficiaryType = model.BeneficiaryType,
                Address = model.Address,
                Description = model.Description
            };

            _context.Beneficiaries.Add(beneficiary);

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Thêm đối tượng hỗ trợ: {model.FullName}",
                TableName = "Beneficiaries",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã thêm đối tượng hỗ trợ: {model.FullName}";
            return RedirectToAction("Beneficiaries");
        }
        #endregion

        #region Donations Management
        public async Task<IActionResult> Donations()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var donations = await _context.Donations
                .Include(d => d.Donor)
                .OrderByDescending(d => d.DonationDate)
                .Take(50)
                .Select(d => new DonationVm
                {
                    DonationId = d.DonationId,
                    DonorName = d.Donor.DonorName,
                    DonorType = d.Donor.DonorType,
                    Amount = d.Amount,
                    DonationDate = d.DonationDate,
                    Method = d.Method
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(donations);
        }

        [HttpGet]
        public async Task<IActionResult> CreateDonation()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            ViewData["Donors"] = await _context.Donors
                .OrderBy(d => d.DonorName)
                .Select(d => new { d.DonorId, d.DonorName })
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDonation(DonationCreateVm model)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var donor = await _context.Donors.FindAsync(model.DonorId);
            if (donor == null)
                return NotFound();

            var donation = new Donation
            {
                DonorId = model.DonorId,
                Amount = model.Amount,
                DonationDate = DateTime.Now,
                Method = model.Method,
                ReceivedBy = userId.Value
            };

            _context.Donations.Add(donation);

            // Update fund balance
            var fund = await _context.Funds.OrderByDescending(f => f.LastUpdated).FirstOrDefaultAsync();
            if (fund != null)
            {
                fund.Balance += model.Amount;
                fund.LastUpdated = DateTime.Now;
            }

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Tiếp nhận quyên góp từ {donor.DonorName}: {model.Amount:N0} VND",
                TableName = "Donations",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã ghi nhận quyên góp {model.Amount:N0} VND từ {donor.DonorName}";
            return RedirectToAction("Donations");
        }
        #endregion

        #region Support Requests Management
        public async Task<IActionResult> Requests()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var requests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .OrderByDescending(r => r.RequestDate)
                .Select(r => new SupportRequestVm
                {
                    RequestId = r.RequestId,
                    BeneficiaryName = r.Beneficiary.FullName,
                    BeneficiaryType = r.Beneficiary.BeneficiaryType,
                    RequestedAmount = r.RequestedAmount,
                    RequestDate = r.RequestDate,
                    Reason = r.Reason,
                    Status = r.Status
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> CreateRequest()
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Nhân viên";
            ViewData["Beneficiaries"] = await _context.Beneficiaries
                .OrderBy(b => b.FullName)
                .Select(b => new { b.BeneficiaryId, b.FullName, b.BeneficiaryType })
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateRequest(SupportRequestCreateVm model)
        {
            if (!IsStaff())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var beneficiary = await _context.Beneficiaries.FindAsync(model.BeneficiaryId);
            if (beneficiary == null)
                return NotFound();

            var request = new SupportRequest
            {
                BeneficiaryId = model.BeneficiaryId,
                RequestDate = DateTime.Now,
                RequestedAmount = model.RequestedAmount,
                Reason = model.Reason,
                Status = "Chờ xét duyệt"
            };

            _context.SupportRequests.Add(request);

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Tạo yêu cầu hỗ trợ cho {beneficiary.FullName}: {model.RequestedAmount:N0} VND",
                TableName = "SupportRequests",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã tạo yêu cầu hỗ trợ cho {beneficiary.FullName}";
            return RedirectToAction("Requests");
        }
        #endregion
    }
}
