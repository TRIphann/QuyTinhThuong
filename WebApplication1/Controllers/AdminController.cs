using Microsoft.AspNetCore.Mvc;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "ADMIN")
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
