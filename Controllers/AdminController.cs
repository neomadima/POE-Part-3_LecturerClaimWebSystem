using LecturerClaimsSystem.Models;
using LecturerClaimsSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Controllers
{
    public class AdminController : Controller
    {
        private readonly IClaimService _claimService;
        private readonly IApprovalService _approvalService;

        public AdminController(IClaimService claimService, IApprovalService approvalService)
        {
            _claimService = claimService;
            _approvalService = approvalService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role == "Lecturer")
                    return RedirectToAction("Login", "Account");

                var pendingClaims = await _claimService.GetPendingClaimsAsync();

                // Add validation results to each claim
                foreach (var claim in pendingClaims)
                {
                    var validationResults = _approvalService.ValidateClaim(claim);
                    claim.ValidationResults = string.Join("; ", validationResults.Select(r => r.Message));
                }

                ViewBag.Role = role;
                ViewBag.ApprovalService = _approvalService;
                return View(pendingClaims);
            }
            catch (System.Exception ex)
            {
                // Log the detailed error
                System.Console.WriteLine($"ERROR in Admin/Index: {ex.Message}");
                System.Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error loading pending claims: {ex.Message}";
                return View(new System.Collections.Generic.List<Claim>());
            }
        }

        public async Task<IActionResult> ApprovedClaims()
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role == "Lecturer")
                    return RedirectToAction("Login", "Account");

                var approvedClaims = await _claimService.GetApprovedClaimsAsync();
                ViewBag.Role = role;
                return View(approvedClaims ?? new System.Collections.Generic.List<Claim>());
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"ERROR in Admin/ApprovedClaims: {ex.Message}");
                TempData["Error"] = "An error occurred while loading approved claims.";
                return View(new System.Collections.Generic.List<Claim>());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApproveClaim(int id)
        {
            try
            {
                System.Console.WriteLine($"=== APPROVE CLAIM STARTED ===");
                System.Console.WriteLine($"Claim ID: {id}");

                var role = HttpContext.Session.GetString("Role");
                var username = HttpContext.Session.GetString("Username");

                System.Console.WriteLine($"User Role: {role}, Username: {username}");

                if (string.IsNullOrEmpty(role) || role == "Lecturer")
                {
                    System.Console.WriteLine("Unauthorized access attempt");
                    return RedirectToAction("Login", "Account");
                }

                // Get the claim first
                var claim = await _claimService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    System.Console.WriteLine($"Claim with ID {id} not found");
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                System.Console.WriteLine($"Found claim: {claim.Lecturer}, Hours: {claim.Hours}, Rate: {claim.Rate}");

                // Automated validation check
                var validationResults = _approvalService.ValidateClaim(claim);
                var hasErrors = validationResults.Any(r => r.Severity == "Error");

                System.Console.WriteLine($"Validation results: {validationResults.Count}, Errors: {hasErrors}");

                if (hasErrors)
                {
                    var errorMessages = string.Join(", ", validationResults.Where(r => r.Severity == "Error").Select(r => r.Message));
                    System.Console.WriteLine($"Validation errors: {errorMessages}");
                    TempData["Error"] = "Cannot approve claim with validation errors: " + errorMessages;
                    return RedirectToAction("Index");
                }

                // Check if current role can approve this claim
                if (!_approvalService.CanApproveClaim(claim, role))
                {
                    var workflow = _approvalService.GetApprovalWorkflow(claim);
                    System.Console.WriteLine($"Cannot approve: {workflow}");
                    TempData["Error"] = $"Approval denied: {workflow}";
                    return RedirectToAction("Index");
                }

                var approvedBy = username ?? role;
                System.Console.WriteLine($"Approving claim {id} by {approvedBy}");

                // Actually approve the claim
                await _claimService.ApproveClaimAsync(id, approvedBy);

                System.Console.WriteLine($"Claim {id} approved successfully");
                TempData["Success"] = $"Claim approved successfully! Total: {claim.Total:C}";

                System.Console.WriteLine($"=== APPROVE CLAIM COMPLETED ===");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"=== APPROVE CLAIM ERROR ===");
                System.Console.WriteLine($"Error: {ex.Message}");
                System.Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error approving claim: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RejectClaim(int id, string reviewNotes = "")
        {
            try
            {
                System.Console.WriteLine($"=== REJECT CLAIM STARTED ===");
                System.Console.WriteLine($"Claim ID: {id}, Notes: {reviewNotes}");

                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role == "Lecturer")
                {
                    System.Console.WriteLine("Unauthorized access attempt");
                    return RedirectToAction("Login", "Account");
                }

                // Get the claim first to verify it exists
                var claim = await _claimService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    System.Console.WriteLine($"Claim with ID {id} not found");
                    TempData["Error"] = "Claim not found.";
                    return RedirectToAction("Index");
                }

                System.Console.WriteLine($"Rejecting claim: {claim.Lecturer}, Hours: {claim.Hours}, Rate: {claim.Rate}");

                await _claimService.RejectClaimAsync(id, reviewNotes);

                System.Console.WriteLine($"Claim {id} rejected successfully");
                TempData["Warning"] = string.IsNullOrEmpty(reviewNotes)
                    ? "Claim rejected."
                    : $"Claim rejected. Notes: {reviewNotes}";

                System.Console.WriteLine($"=== REJECT CLAIM COMPLETED ===");
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine($"=== REJECT CLAIM ERROR ===");
                System.Console.WriteLine($"Error: {ex.Message}");
                System.Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                TempData["Error"] = $"Error rejecting claim: {ex.Message}";
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> ValidateClaim(int id)
        {
            try
            {
                var role = HttpContext.Session.GetString("Role");
                if (string.IsNullOrEmpty(role) || role == "Lecturer")
                    return Json(new { success = false, message = "Unauthorized" });

                var claim = await _claimService.GetClaimByIdAsync(id);
                if (claim == null)
                {
                    return Json(new { success = false, message = "Claim not found" });
                }

                var validationResults = _approvalService.ValidateClaim(claim);
                var canApprove = _approvalService.CanApproveClaim(claim, role);
                var workflow = _approvalService.GetApprovalWorkflow(claim);

                return Json(new
                {
                    success = true,
                    claimId = id,
                    validationResults = validationResults.Select(vr => new {
                        severity = vr.Severity,
                        message = vr.Message
                    }),
                    canApprove,
                    approvalWorkflow = workflow,
                    requiresHigherApproval = _approvalService.RequiresHigherApproval(claim)
                });
            }
            catch (System.Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Add a debug endpoint to check service status
        public async Task<IActionResult> DebugServices()
        {
            try
            {
                var pendingClaims = await _claimService.GetPendingClaimsAsync();
                var approvedClaims = await _claimService.GetApprovedClaimsAsync();

                return Content($"Services Debug:\n" +
                             $"Pending Claims: {pendingClaims?.Count ?? 0}\n" +
                             $"Approved Claims: {approvedClaims?.Count ?? 0}\n" +
                             $"ClaimService Type: {_claimService.GetType().Name}\n" +
                             $"ApprovalService Type: {_approvalService.GetType().Name}");
            }
            catch (System.Exception ex)
            {
                return Content($"Debug Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}