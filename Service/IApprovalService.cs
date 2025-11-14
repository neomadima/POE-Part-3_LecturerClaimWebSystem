using LecturerClaimsSystem.Models;
using System.Collections.Generic;

namespace LecturerClaimsSystem.Services
{
    public interface IApprovalService
    {
        List<ClaimValidationResult> ValidateClaim(Claim claim);
        bool CanApproveClaim(Claim claim, string approverRole);
        bool RequiresHigherApproval(Claim claim);
        string GetApprovalWorkflow(Claim claim);
    }
}