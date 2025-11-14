using LecturerClaimsSystem.Models;
using LecturerClaimsSystem.Services;
using Microsoft.AspNetCore.Mvc;

namespace LecturerClaimsSystem.Controllers
{
    public class LecturerController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IFileService _fileService;

        public LecturerController(IClaimService claimService, IFileService fileService)
        {
            _claimService = claimService;
            _fileService = fileService;
        }

        public async Task<IActionResult> Index()
        {
            var username = HttpContext.Session.GetString("Username");
            var role = HttpContext.Session.GetString("Role");

            if (string.IsNullOrEmpty(username) || role != "Lecturer")
            {
                return RedirectToAction("Login", "Account");
            }

            var claims = await _claimService.GetClaimsForLecturerAsync(username);
            ViewBag.Username = username;
            ViewBag.LatestClaim = claims.FirstOrDefault();

            return View(claims);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitClaim(Claim claim, IFormFile? document)
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username) || HttpContext.Session.GetString("Role") != "Lecturer")
                return RedirectToAction("Login", "Account");

            // Server-side validation
            if (claim.Hours <= 0 || claim.Hours > 100)
            {
                TempData["Error"] = "Hours must be between 0.1 and 100.";
                return RedirectToAction("Index");
            }

            if (claim.Rate <= 0 || claim.Rate > 1000)
            {
                TempData["Error"] = "Rate must be between $0.1 and $1000.";
                return RedirectToAction("Index");
            }

            if (!string.IsNullOrEmpty(claim.Notes) && claim.Notes.Length > 1000)
            {
                TempData["Error"] = "Notes cannot exceed 1000 characters.";
                return RedirectToAction("Index");
            }

            try
            {
                claim.Lecturer = username;
                claim.Date = DateTime.Now;
                claim.Status = "Pending";

                if (document != null && document.Length > 0)
                {
                    try
                    {
                        claim.DocumentPath = await _fileService.SaveFileAsync(document) ?? string.Empty;
                    }
                    catch (Exception ex)
                    {
                        TempData["Error"] = ex.Message;
                        return RedirectToAction("Index");
                    }
                }

                await _claimService.SubmitClaimAsync(claim);
                TempData["Success"] = $"Claim submitted successfully! Total payment: {claim.Total:C}";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error submitting claim: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyClaims()
        {
            var username = HttpContext.Session.GetString("Username");
            if (string.IsNullOrEmpty(username) || HttpContext.Session.GetString("Role") != "Lecturer")
                return RedirectToAction("Login", "Account");

            var claims = await _claimService.GetClaimsForLecturerAsync(username);
            ViewBag.Username = username;
            return View(claims);
        }
    }
}