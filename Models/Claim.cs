using System;
using System.ComponentModel.DataAnnotations;

namespace LecturerClaimsSystem.Models
{
    public class Claim
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Lecturer { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Range(0.1, 100, ErrorMessage = "Hours must be between 0.1 and 100")]
        public double Hours { get; set; }

        [Range(0.1, 1000, ErrorMessage = "Rate must be between $0.1 and $1000")]
        public double Rate { get; set; }

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        public string Notes { get; set; } = string.Empty;

        public string DocumentPath { get; set; } = string.Empty;

        [Required]
        public string Status { get; set; } = "Pending";

        public string ApprovedBy { get; set; } = string.Empty;

        public DateTime? ReviewedDate { get; set; }

        public string ReviewNotes { get; set; } = string.Empty;

        public string ValidationResults { get; set; } = string.Empty;

        public double Total => Hours * Rate;

        public string? DocumentFileName => string.IsNullOrEmpty(DocumentPath) ? null : System.IO.Path.GetFileName(DocumentPath);
    }

    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }

    public class ApprovalRules
    {
        public double MaxHoursPerClaim { get; set; } = 40;
        public double MaxRateStandard { get; set; } = 80;
        public double MaxRateSenior { get; set; } = 120;
        public double MaxTotalPerClaim { get; set; } = 5000;
        public bool RequireDocumentForLargeClaims { get; set; } = true;
        public double LargeClaimThreshold { get; set; } = 1000;
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Error
    }

    public class ClaimValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Info"; // Info, Warning, Error
    }

}