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

            var today = DateTime.Today;

            var vm = new StaffDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Nhân viên",

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
