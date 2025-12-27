using Microsoft.AspNetCore.Mvc;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class StaffController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "STAFF")
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
