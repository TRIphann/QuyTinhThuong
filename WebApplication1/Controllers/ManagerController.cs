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

        public async Task<IActionResult> Index()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var today = DateTime.Today;
            var vm = new ManagerDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Manager",

                PendingRequests = await _context.SupportRequests
                    .CountAsync(r => r.Status == "Chờ xét duyệt"),

                ApprovedToday = await _context.Approvals
                    .CountAsync(a => a.ApprovalDate.Date == today && a.Result == "Đồng ý"),

                RejectedToday = await _context.Approvals
                    .CountAsync(a => a.ApprovalDate.Date == today && a.Result == "Từ chối"),

                TotalBeneficiaries = await _context.Beneficiaries.CountAsync(),

                TotalApprovedAmount = await _context.Approvals
                    .Where(a => a.Result == "Đồng ý")
                    .Join(_context.SupportRequests, a => a.RequestId, r => r.RequestId, (a, r) => r.RequestedAmount)
                    .SumAsync(),

                RecentRequests = await _context.SupportRequests
                    .Include(r => r.Beneficiary)
                    .Where(r => r.Status == "Chờ xét duyệt")
                    .OrderByDescending(r => r.RequestDate)
                    .Take(10)
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
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        public async Task<IActionResult> Requests()
        {
            if (!IsManager())
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Approve(int id)
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id);

            if (request == null)
                return NotFound();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Manager";
            ViewData["Request"] = new SupportRequestVm
            {
                RequestId = request.RequestId,
                BeneficiaryName = request.Beneficiary.FullName,
                BeneficiaryType = request.Beneficiary.BeneficiaryType,
                RequestedAmount = request.RequestedAmount,
                RequestDate = request.RequestDate,
                Reason = request.Reason,
                Status = request.Status
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(ApprovalCreateVm model)
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var request = await _context.SupportRequests.FindAsync(model.RequestId);
            if (request == null)
                return NotFound();

            // Create approval record
            var approval = new Approval
            {
                RequestId = model.RequestId,
                ApprovedBy = userId.Value,
                ApprovalDate = DateTime.Now,
                Result = model.Result,
                Note = model.Note
            };

            _context.Approvals.Add(approval);

            // Update request status
            request.Status = model.Result == "Đồng ý" ? "Đã duyệt" : "Từ chối";

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Phê duyệt yêu cầu #{model.RequestId}: {model.Result}",
                TableName = "Approvals",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã {(model.Result == "Đồng ý" ? "duyệt" : "từ chối")} yêu cầu #{model.RequestId}";
            return RedirectToAction("Requests");
        }

        public async Task<IActionResult> Reports()
        {
            if (!IsManager())
                return RedirectToAction("Login", "Account");

            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var vm = new ManagerReportVm
            {
                TotalRequests = await _context.SupportRequests.CountAsync(),
                ApprovedRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Đã duyệt"),
                RejectedRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Từ chối"),
                PendingRequests = await _context.SupportRequests.CountAsync(r => r.Status == "Chờ xét duyệt"),

                ThisMonthApprovals = await _context.Approvals
                    .CountAsync(a => a.ApprovalDate >= thisMonth),

                TotalApprovedAmount = await _context.Approvals
                    .Where(a => a.Result == "Đồng ý")
                    .Join(_context.SupportRequests, a => a.RequestId, r => r.RequestId, (a, r) => r.RequestedAmount)
                    .SumAsync(),

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
        public int TotalRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ThisMonthApprovals { get; set; }
        public decimal TotalApprovedAmount { get; set; }
        public List<BeneficiaryStatVm> BeneficiaryStats { get; set; } = new();
    }

    public class BeneficiaryStatVm
    {
        public string Type { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
