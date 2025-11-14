using LecturerClaimsSystem.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public interface IClaimService
    {
        Task SubmitClaimAsync(Claim claim);
        Task ApproveClaimAsync(int claimId, string approvedBy);
        Task RejectClaimAsync(int claimId, string reviewNotes = "");
        Task<List<Claim>> GetClaimsForLecturerAsync(string lecturer);
        Task<List<Claim>> GetPendingClaimsAsync();
        Task<List<Claim>> GetApprovedClaimsAsync();
        Task<Claim?> GetClaimByIdAsync(int id);
    }
}