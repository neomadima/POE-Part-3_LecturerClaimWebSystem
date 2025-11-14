using LecturerClaimsSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public class MockClaimService : IClaimService
    {
        private static List<Claim> _claims = new List<Claim>();
        private static int _nextId = 1;

        public MockClaimService()
        {
            // Initialize with test data
            InitializeTestData();
        }

        private void InitializeTestData()
        {
            if (!_claims.Any())
            {
                _claims.AddRange(new[]
                {
                    new Claim {
                        Id = _nextId++,
                        Lecturer = "john.doe",
                        Date = DateTime.Now.AddDays(-2),
                        Hours = 5,
                        Rate = 50,
                        Notes = "Guest lecture",
                        Status = "Pending"
                    },
                    new Claim {
                        Id = _nextId++,
                        Lecturer = "jane.smith",
                        Date = DateTime.Now.AddDays(-1),
                        Hours = 8,
                        Rate = 45,
                        Notes = "Tutorial session",
                        Status = "Pending"
                    },
                    new Claim {
                        Id = _nextId++,
                        Lecturer = "robert.johnson",
                        Date = DateTime.Now.AddDays(-3),
                        Hours = 12,
                        Rate = 60,
                        Notes = "Weekend workshop",
                        Status = "Pending"
                    }
                });
                Console.WriteLine("MockClaimService initialized with test data");
            }
        }

        public async Task SubmitClaimAsync(Claim claim)
        {
            try
            {
                claim.Id = _nextId++;
                _claims.Add(claim);
                Console.WriteLine($"Claim submitted: ID {claim.Id}, Lecturer: {claim.Lecturer}");
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error submitting claim: {ex.Message}");
                throw;
            }
        }

        public async Task ApproveClaimAsync(int claimId, string approvedBy)
        {
            try
            {
                var claim = _claims.FirstOrDefault(c => c.Id == claimId);
                if (claim != null)
                {
                    claim.Status = "Approved";
                    claim.ApprovedBy = approvedBy;
                    claim.ReviewedDate = DateTime.Now;
                    Console.WriteLine($"Claim {claimId} approved by {approvedBy}");
                }
                else
                {
                    Console.WriteLine($"Claim {claimId} not found for approval");
                    throw new ArgumentException($"Claim with ID {claimId} not found");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error approving claim {claimId}: {ex.Message}");
                throw;
            }
        }

        public async Task RejectClaimAsync(int claimId, string reviewNotes = "")
        {
            try
            {
                var claim = _claims.FirstOrDefault(c => c.Id == claimId);
                if (claim != null)
                {
                    claim.Status = "Rejected";
                    claim.ApprovedBy = "System";
                    claim.ReviewedDate = DateTime.Now;
                    claim.ReviewNotes = reviewNotes;
                    Console.WriteLine($"Claim {claimId} rejected. Notes: {reviewNotes}");
                }
                else
                {
                    Console.WriteLine($"Claim {claimId} not found for rejection");
                    throw new ArgumentException($"Claim with ID {claimId} not found");
                }
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error rejecting claim {claimId}: {ex.Message}");
                throw;
            }
        }

        public async Task<List<Claim>> GetClaimsForLecturerAsync(string lecturer)
        {
            var claims = _claims.Where(c => c.Lecturer == lecturer)
                              .OrderByDescending(c => c.Date)
                              .ToList();
            return await Task.FromResult(claims);
        }

        public async Task<List<Claim>> GetPendingClaimsAsync()
        {
            var claims = _claims.Where(c => c.Status == "Pending")
                              .OrderByDescending(c => c.Date)
                              .ToList();
            Console.WriteLine($"GetPendingClaimsAsync returned {claims.Count} claims");
            return await Task.FromResult(claims);
        }

        public async Task<List<Claim>> GetApprovedClaimsAsync()
        {
            var claims = _claims.Where(c => c.Status == "Approved")
                              .OrderByDescending(c => c.Date)
                              .ToList();
            return await Task.FromResult(claims);
        }

        public async Task<Claim?> GetClaimByIdAsync(int id)
        {
            var claim = _claims.FirstOrDefault(c => c.Id == id);
            return await Task.FromResult(claim);
        }
    }
}