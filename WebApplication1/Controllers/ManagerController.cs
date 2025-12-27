using Microsoft.AspNetCore.Mvc;

namespace QLDuLichRBAC_Upgrade.Controllers
{
    public class ManagerController : Controller
    {
        public IActionResult Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (role != "MANAGER")
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}
