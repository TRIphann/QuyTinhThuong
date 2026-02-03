using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLDuLichRBAC_Upgrade.Models;
using QLDuLichRBAC_Upgrade.Models.Entities;
using QLDuLichRBAC_Upgrade.Models.ViewModels;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AccountantController : Controller
    {
        private readonly QLQuyTinhThuongContext _context;

        public AccountantController(QLQuyTinhThuongContext context)
        {
            _context = context;
        }

        private bool IsAccountant()
            => string.Equals((HttpContext.Session.GetString("Role") ?? "").Trim(),
                             "ACCOUNTANT",
                             StringComparison.OrdinalIgnoreCase);

        private int? GetUserId() => HttpContext.Session.GetInt32("UserId");

        public async Task<IActionResult> Index()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var vm = new AccountantDashboardVm
            {
                FullName = HttpContext.Session.GetString("FullName") ?? "Kế toán",

                FundBalance = await _context.Funds
                    .OrderByDescending(f => f.LastUpdated)
                    .Select(f => f.Balance)
                    .FirstOrDefaultAsync(),

                TotalDonations = await _context.Donations.SumAsync(d => d.Amount),
                TotalExpenses = await _context.Expenses.SumAsync(e => e.Amount),

                DonationsThisMonth = await _context.Donations
                    .CountAsync(d => d.DonationDate >= thisMonth),

                ExpensesThisMonth = await _context.Expenses
                    .CountAsync(e => e.ExpenseDate >= thisMonth),

                RecentDonations = await _context.Donations
                    .Include(d => d.Donor)
                    .Include(d => d.ReceivedByUser)
                    .OrderByDescending(d => d.DonationDate)
                    .Take(5)
                    .Select(d => new DonationVm
                    {
                        DonationId = d.DonationId,
                        DonorName = d.Donor.DonorName,
                        DonorType = d.Donor.DonorType,
                        Amount = d.Amount,
                        DonationDate = d.DonationDate,
                        Method = d.Method,
                        ReceivedByName = d.ReceivedByUser != null ? d.ReceivedByUser.FullName : null
                    })
                    .ToListAsync(),

                RecentExpenses = await _context.Expenses
                    .Include(e => e.SupportRequest)
                        .ThenInclude(r => r.Beneficiary)
                    .Include(e => e.PaidByUser)
                    .OrderByDescending(e => e.ExpenseDate)
                    .Take(5)
                    .Select(e => new ExpenseVm
                    {
                        ExpenseId = e.ExpenseId,
                        BeneficiaryName = e.SupportRequest.Beneficiary.FullName,
                        Amount = e.Amount,
                        ExpenseDate = e.ExpenseDate,
                        PaymentMethod = e.PaymentMethod,
                        PaidByName = e.PaidByUser != null ? e.PaidByUser.FullName : null
                    })
                    .ToListAsync()
            };

            return View("Dashboard", vm);
        }

        public async Task<IActionResult> Donations()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var donations = await _context.Donations
                .Include(d => d.Donor)
                .Include(d => d.ReceivedByUser)
                .OrderByDescending(d => d.DonationDate)
                .Select(d => new DonationVm
                {
                    DonationId = d.DonationId,
                    DonorName = d.Donor.DonorName,
                    DonorType = d.Donor.DonorType,
                    Amount = d.Amount,
                    DonationDate = d.DonationDate,
                    Method = d.Method,
                    ReceivedByName = d.ReceivedByUser != null ? d.ReceivedByUser.FullName : null
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Kế toán";
            ViewData["TotalAmount"] = donations.Sum(d => d.Amount);
            return View(donations);
        }

        public async Task<IActionResult> Expenses()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var expenses = await _context.Expenses
                .Include(e => e.SupportRequest)
                    .ThenInclude(r => r.Beneficiary)
                .Include(e => e.PaidByUser)
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new ExpenseVm
                {
                    ExpenseId = e.ExpenseId,
                    BeneficiaryName = e.SupportRequest.Beneficiary.FullName,
                    Amount = e.Amount,
                    ExpenseDate = e.ExpenseDate,
                    PaymentMethod = e.PaymentMethod,
                    PaidByName = e.PaidByUser != null ? e.PaidByUser.FullName : null
                })
                .ToListAsync();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Kế toán";
            ViewData["TotalAmount"] = expenses.Sum(e => e.Amount);
            return View(expenses);
        }

        public async Task<IActionResult> ApprovedRequests()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var approvedRequests = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .Where(r => r.Status == "Đã duyệt")
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

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Kế toán";
            return View(approvedRequests);
        }

        [HttpGet]
        public async Task<IActionResult> CreateExpense(int id)
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var request = await _context.SupportRequests
                .Include(r => r.Beneficiary)
                .FirstOrDefaultAsync(r => r.RequestId == id && r.Status == "Đã duyệt");

            if (request == null)
                return NotFound();

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Kế toán";
            ViewData["Request"] = new SupportRequestVm
            {
                RequestId = request.RequestId,
                BeneficiaryName = request.Beneficiary.FullName,
                BeneficiaryType = request.Beneficiary.BeneficiaryType,
                RequestedAmount = request.RequestedAmount,
                RequestDate = request.RequestDate,
                Reason = request.Reason
            };

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateExpense(ExpenseCreateVm model)
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var userId = GetUserId();
            if (userId == null)
                return RedirectToAction("Login", "Account");

            var request = await _context.SupportRequests.FindAsync(model.RequestId);
            if (request == null || request.Status != "Đã duyệt")
                return NotFound();

            // Create expense
            var expense = new Expense
            {
                RequestId = model.RequestId,
                Amount = model.Amount,
                ExpenseDate = DateTime.Now,
                PaymentMethod = model.PaymentMethod,
                PaidBy = userId.Value
            };

            _context.Expenses.Add(expense);

            // Update request status
            request.Status = "Đã chi";

            // Update fund balance
            var fund = await _context.Funds.OrderByDescending(f => f.LastUpdated).FirstOrDefaultAsync();
            if (fund != null)
            {
                fund.Balance -= model.Amount;
                fund.LastUpdated = DateTime.Now;
            }

            // Log action
            var log = new Log
            {
                UserId = userId.Value,
                Action = $"Chi tiền cho yêu cầu #{model.RequestId}: {model.Amount:N0} VND",
                TableName = "Expenses",
                ActionTime = DateTime.Now
            };
            _context.Logs.Add(log);

            await _context.SaveChangesAsync();

            TempData["Success"] = $"Đã ghi nhận chi tiền {model.Amount:N0} VND cho yêu cầu #{model.RequestId}";
            return RedirectToAction("Expenses");
        }

        public async Task<IActionResult> Reports()
        {
            if (!IsAccountant())
                return RedirectToAction("Login", "Account");

            var thisMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

            var vm = new AccountantReportVm
            {
                CurrentBalance = await _context.Funds
                    .OrderByDescending(f => f.LastUpdated)
                    .Select(f => f.Balance)
                    .FirstOrDefaultAsync(),

                TotalDonations = await _context.Donations.SumAsync(d => d.Amount),
                TotalExpenses = await _context.Expenses.SumAsync(e => e.Amount),

                ThisMonthDonations = await _context.Donations
                    .Where(d => d.DonationDate >= thisMonth)
                    .SumAsync(d => d.Amount),

                ThisMonthExpenses = await _context.Expenses
                    .Where(e => e.ExpenseDate >= thisMonth)
                    .SumAsync(e => e.Amount),

                DonationsByMethod = await _context.Donations
                    .GroupBy(d => d.Method)
                    .Select(g => new DonationStatVm
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(d => d.Amount)
                    })
                    .ToListAsync(),

                ExpensesByMethod = await _context.Expenses
                    .GroupBy(e => e.PaymentMethod)
                    .Select(g => new ExpenseStatVm
                    {
                        Method = g.Key,
                        Count = g.Count(),
                        TotalAmount = g.Sum(e => e.Amount)
                    })
                    .ToListAsync()
            };

            ViewData["FullName"] = HttpContext.Session.GetString("FullName") ?? "Kế toán";
            return View(vm);
        }
    }

    public class AccountantReportVm
    {
        public decimal CurrentBalance { get; set; }
        public decimal TotalDonations { get; set; }
        public decimal TotalExpenses { get; set; }
        public decimal ThisMonthDonations { get; set; }
        public decimal ThisMonthExpenses { get; set; }
        public List<DonationStatVm> DonationsByMethod { get; set; } = new();
        public List<ExpenseStatVm> ExpensesByMethod { get; set; } = new();
    }

    public class DonationStatVm
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class ExpenseStatVm
    {
        public string Method { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
