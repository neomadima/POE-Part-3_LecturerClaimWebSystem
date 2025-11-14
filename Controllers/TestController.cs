using Microsoft.AspNetCore.Mvc;

namespace LecturerClaimsSystem.Controllers
{
    public class TestController : Controller
    {
        public IActionResult SessionTest()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            return Content($"Username: {username}, Role: {role}");
        }

        public IActionResult SetSession()
        {
            HttpContext.Session.SetString("Username", "testuser");
            HttpContext.Session.SetString("Role", "Lecturer");
            return Content("Session set");
        }
    }
}