using LecturerClaimsSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace LecturerClaimsSystem.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            // If user is already logged in, redirect to appropriate dashboard
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(role))
            {
                if (role == "Lecturer")
                {
                    return RedirectToAction("Index", "Lecturer");
                }
                else
                {
                    return RedirectToAction("Index", "Admin");
                }
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Simple authentication - in real app, use Identity framework
            if (string.IsNullOrEmpty(model.Username) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Role))
            {
                ModelState.AddModelError("", "Please fill in all fields");
                return View(model);
            }

            // Store user info in session
            HttpContext.Session.SetString("Username", model.Username);
            HttpContext.Session.SetString("Role", model.Role);

            if (model.Role == "Lecturer")
            {
                return RedirectToAction("Index", "Lecturer");
            }
            else
            {
                return RedirectToAction("Index", "Admin");
            }
        }

        // Change this to GET method instead of POST for simplicity
        public IActionResult Logout()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            Console.WriteLine($"Logging out user: {username}, Role: {role}");

            HttpContext.Session.Clear();

            Console.WriteLine("Session cleared, redirecting to login");

            return RedirectToAction("Login", "Account");
        }

        // Keep the POST version as well for compatibility
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LogoutPost()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }
    }
}