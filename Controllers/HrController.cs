using LecturerClaimsSystem.Models;
using LecturerClaimsSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Controllers
{
    public class HrController : Controller
    {
        private readonly IHrService _hrService;
        private readonly IClaimService _claimService;

        public HrController(IHrService hrService, IClaimService claimService)
        {
            _hrService = hrService;
            _claimService = claimService;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            var analytics = await _hrService.GetHrAnalyticsAsync();
            ViewBag.Role = role;
            return View(analytics);
        }

        public async Task<IActionResult> Lecturers()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            var lecturers = await _hrService.GetAllLecturersAsync();
            ViewBag.Role = role;
            return View(lecturers);
        }

        public async Task<IActionResult> PaymentReports()
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            var reports = await _hrService.GetPaymentReportsAsync();
            ViewBag.Role = role;
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> GeneratePaymentReport(DateTime periodStart, DateTime periodEnd)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            try
            {
                var generatedBy = HttpContext.Session.GetString("Username") ?? "HR System";
                var report = await _hrService.GeneratePaymentReportAsync(periodStart, periodEnd, generatedBy);

                TempData["Success"] = $"Payment report generated successfully! Total: {report.TotalAmount:C} for {report.TotalClaims} claims.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Error generating report: {ex.Message}";
            }

            return RedirectToAction("PaymentReports");
        }

        [HttpPost]
        public async Task<IActionResult> CreateLecturer(Lecturer lecturer)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            if (await _hrService.CreateLecturerAsync(lecturer))
            {
                TempData["Success"] = "Lecturer created successfully!";
            }
            else
            {
                TempData["Error"] = "Error creating lecturer. Please check the details.";
            }

            return RedirectToAction("Lecturers");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateLecturer(Lecturer lecturer)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            if (await _hrService.UpdateLecturerAsync(lecturer))
            {
                TempData["Success"] = "Lecturer updated successfully!";
            }
            else
            {
                TempData["Error"] = "Error updating lecturer.";
            }

            return RedirectToAction("Lecturers");
        }

        [HttpPost]
        public async Task<IActionResult> DeactivateLecturer(int id)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || (role != "HR Manager" && role != "Academic Manager"))
                return RedirectToAction("Login", "Account");

            if (await _hrService.DeactivateLecturerAsync(id))
            {
                TempData["Success"] = "Lecturer deactivated successfully!";
            }
            else
            {
                TempData["Error"] = "Error deactivating lecturer.";
            }

            return RedirectToAction("Lecturers");
        }

        public async Task<IActionResult> DownloadReport(int id)
        {
            var report = await _hrService.GetPaymentReportByIdAsync(id);
            if (report == null)
            {
                TempData["Error"] = "Report not found.";
                return RedirectToAction("PaymentReports");
            }

            // Generate CSV file
            var csvContent = GenerateCsvReport(report);
            var fileName = $"PaymentReport_{report.PeriodStart:yyyyMMdd}_{report.PeriodEnd:yyyyMMdd}.csv";

            return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
        }

        private string GenerateCsvReport(PaymentReport report)
        {
            var claimsData = System.Text.Json.JsonSerializer.Deserialize<dynamic>(report.ReportData);

            var csv = new System.Text.StringBuilder();
            csv.AppendLine($"Payment Report: {report.ReportName}");
            csv.AppendLine($"Period: {report.PeriodStart:yyyy-MM-dd} to {report.PeriodEnd:yyyy-MM-dd}");
            csv.AppendLine($"Generated On: {report.ReportDate:yyyy-MM-dd HH:mm}");
            csv.AppendLine($"Generated By: {report.GeneratedBy}");
            csv.AppendLine($"Total Amount: {report.TotalAmount:C}");
            csv.AppendLine($"Total Claims: {report.TotalClaims}");
            csv.AppendLine();
            csv.AppendLine("Lecturer,Date,Hours,Rate,Total,Approved By");

            // This would be populated with actual claim data from the JSON
            // For now, we'll just add a placeholder
            csv.AppendLine("Sample Data,2024-01-01,5,50,250,HR System");

            return csv.ToString();
        }
    }
}