using LecturerClaimsSystem.Models;
using System.Collections.Generic;
using System.Linq;

namespace LecturerClaimsSystem.Services
{
    public class ApprovalService : IApprovalService
    {
        private readonly ApprovalRules _rules;

        public ApprovalService()
        {
            _rules = new ApprovalRules
            {
                MaxHoursPerClaim = 40,
                MaxRateStandard = 80,
                MaxRateSenior = 120,
                MaxTotalPerClaim = 5000,
                RequireDocumentForLargeClaims = true,
                LargeClaimThreshold = 1000
            };
        }

        public List<ClaimValidationResult> ValidateClaim(Claim claim)
        {
            var results = new List<ClaimValidationResult>();

            // Hours validation
            if (claim.Hours > _rules.MaxHoursPerClaim)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = false,
                    Message = $"Hours worked ({claim.Hours}) exceeds maximum allowed ({_rules.MaxHoursPerClaim})",
                    Severity = "Error"
                });
            }
            else if (claim.Hours > _rules.MaxHoursPerClaim * 0.8)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = true,
                    Message = $"Hours worked ({claim.Hours}) is approaching maximum limit",
                    Severity = "Warning"
                });
            }

            // Rate validation
            if (claim.Rate > _rules.MaxRateSenior)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = false,
                    Message = $"Hourly rate (${claim.Rate}) exceeds maximum allowed (${_rules.MaxRateSenior})",
                    Severity = "Error"
                });
            }
            else if (claim.Rate > _rules.MaxRateStandard)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = true,
                    Message = $"Hourly rate (${claim.Rate}) requires senior approval",
                    Severity = "Warning"
                });
            }

            // Total amount validation
            if (claim.Total > _rules.MaxTotalPerClaim)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = false,
                    Message = $"Total amount (${claim.Total}) exceeds maximum allowed (${_rules.MaxTotalPerClaim})",
                    Severity = "Error"
                });
            }
            else if (claim.Total > _rules.MaxTotalPerClaim * 0.8)
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = true,
                    Message = $"Total amount (${claim.Total}) is approaching maximum limit",
                    Severity = "Warning"
                });
            }

            // Document requirement validation
            if (_rules.RequireDocumentForLargeClaims &&
                claim.Total >= _rules.LargeClaimThreshold &&
                string.IsNullOrEmpty(claim.DocumentPath))
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = false,
                    Message = $"Documentation required for claims over ${_rules.LargeClaimThreshold}",
                    Severity = "Warning"
                });
            }

            // If no issues found, add success message
            if (!results.Any(r => r.Severity == "Error" || r.Severity == "Warning"))
            {
                results.Add(new ClaimValidationResult
                {
                    IsValid = true,
                    Message = "Claim meets all validation criteria",
                    Severity = "Info"
                });
            }

            return results;
        }

        public bool CanApproveClaim(Claim claim, string approverRole)
        {
            var validationResults = ValidateClaim(claim);

            // Check if there are any errors that prevent approval
            if (validationResults.Any(r => r.Severity == "Error"))
                return false;

            // Check role-based approval limits
            switch (approverRole)
            {
                case "Programme Coordinator":
                    return claim.Total <= 1000 && claim.Rate <= _rules.MaxRateStandard;

                case "Academic Manager":
                    return claim.Total <= _rules.MaxTotalPerClaim;

                default:
                    return false;
            }
        }

        public bool RequiresHigherApproval(Claim claim)
        {
            return claim.Total > 1000 || claim.Rate > _rules.MaxRateStandard;
        }

        public string GetApprovalWorkflow(Claim claim)
        {
            if (claim.Total > 1000)
                return "Academic Manager approval required (Total > $1000)";

            if (claim.Rate > _rules.MaxRateStandard)
                return "Academic Manager approval required (Rate > standard limit)";

            return "Programme Coordinator can approve";
        }
    }
}