using LecturerClaimsSystem.Data;
using LecturerClaimsSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public class ClaimService : IClaimService
    {
        private readonly ApplicationDbContext _context;

        public ClaimService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task SubmitClaimAsync(Claim claim)
        {
            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();
        }

        public async Task ApproveClaimAsync(int claimId, string approvedBy)
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null)
            {
                claim.Status = "Approved";
                claim.ApprovedBy = approvedBy;
                claim.ReviewedDate = DateTime.Now;
                await _context.SaveChangesAsync();
            }
        }

        public async Task RejectClaimAsync(int claimId, string reviewNotes = "")
        {
            var claim = await _context.Claims.FindAsync(claimId);
            if (claim != null)
            {
                claim.Status = "Rejected";
                claim.ApprovedBy = "System";
                claim.ReviewedDate = DateTime.Now;
                claim.ReviewNotes = reviewNotes;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<Claim>> GetClaimsForLecturerAsync(string lecturer)
        {
            return await _context.Claims
                .Where(c => c.Lecturer == lecturer)
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetPendingClaimsAsync()
        {
            return await _context.Claims
                .Where(c => c.Status == "Pending")
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        public async Task<List<Claim>> GetApprovedClaimsAsync()
        {
            return await _context.Claims
                .Where(c => c.Status == "Approved")
                .OrderByDescending(c => c.Date)
                .ToListAsync();
        }

        public async Task<Claim?> GetClaimByIdAsync(int id)
        {
            return await _context.Claims.FindAsync(id);
        }
    }
}