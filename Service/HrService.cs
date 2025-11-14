using LecturerClaimsSystem.Data;
using LecturerClaimsSystem.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public class HrService : IHrService
    {
        private readonly ApplicationDbContext _context;
        private readonly IClaimService _claimService;

        public HrService(ApplicationDbContext context, IClaimService claimService)
        {
            _context = context;
            _claimService = claimService;
        }

        // Lecturer Management
        public async Task<List<Lecturer>> GetAllLecturersAsync()
        {
            return await _context.Lecturers
                .OrderBy(l => l.LastName)
                .ThenBy(l => l.FirstName)
                .ToListAsync();
        }

        public async Task<Lecturer?> GetLecturerByIdAsync(int id)
        {
            return await _context.Lecturers.FindAsync(id);
        }

        public async Task<Lecturer?> GetLecturerByEmployeeIdAsync(string employeeId)
        {
            return await _context.Lecturers
                .FirstOrDefaultAsync(l => l.EmployeeId == employeeId);
        }

        public async Task<bool> CreateLecturerAsync(Lecturer lecturer)
        {
            try
            {
                lecturer.CreatedDate = DateTime.Now;
                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateLecturerAsync(Lecturer lecturer)
        {
            try
            {
                lecturer.UpdatedDate = DateTime.Now;
                _context.Lecturers.Update(lecturer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeactivateLecturerAsync(int id)
        {
            var lecturer = await _context.Lecturers.FindAsync(id);
            if (lecturer != null)
            {
                lecturer.IsActive = false;
                lecturer.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }

        // Payment Reports
        public async Task<PaymentReport> GeneratePaymentReportAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
        {
            var claims = await GetClaimsForPaymentAsync(periodStart, periodEnd);
            var totalAmount = claims.Sum(c => c.Total);

            var report = new PaymentReport
            {
                ReportName = $"Payment Report {periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}",
                ReportDate = DateTime.Now,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                TotalAmount = (decimal)totalAmount,
                TotalClaims = claims.Count,
                GeneratedBy = generatedBy,
                ReportData = System.Text.Json.JsonSerializer.Serialize(new
                {
                    Claims = claims.Select(c => new
                    {
                        c.Lecturer,
                        c.Date,
                        c.Hours,
                        c.Rate,
                        Total = c.Total,
                        c.ApprovedBy
                    }),
                    Summary = new
                    {
                        TotalAmount = totalAmount,
                        TotalClaims = claims.Count,
                        Period = $"{periodStart:yyyy-MM-dd} to {periodEnd:yyyy-MM-dd}"
                    }
                })
            };

            _context.PaymentReports.Add(report);
            await _context.SaveChangesAsync();

            return report;
        }

        public async Task<List<PaymentReport>> GetPaymentReportsAsync()
        {
            return await _context.PaymentReports
                .OrderByDescending(pr => pr.ReportDate)
                .ToListAsync();
        }

        public async Task<PaymentReport?> GetPaymentReportByIdAsync(int id)
        {
            return await _context.PaymentReports.FindAsync(id);
        }

        public async Task<List<Claim>> GetClaimsForPaymentAsync(DateTime periodStart, DateTime periodEnd)
        {
            // Get approved claims within the specified period
            var allClaims = await _claimService.GetApprovedClaimsAsync();
            return allClaims
                .Where(c => c.Date >= periodStart && c.Date <= periodEnd && c.Status == "Approved")
                .ToList();
        }

        // Analytics
        public async Task<HrAnalytics> GetHrAnalyticsAsync()
        {
            var lecturers = await GetAllLecturersAsync();
            var allClaims = await _claimService.GetApprovedClaimsAsync();
            var thisMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var thisMonthEnd = thisMonthStart.AddMonths(1).AddDays(-1);

            var thisMonthClaims = allClaims
                .Where(c => c.Date >= thisMonthStart && c.Date <= thisMonthEnd)
                .ToList();

            var analytics = new HrAnalytics
            {
                TotalLecturers = lecturers.Count,
                ActiveLecturers = lecturers.Count(l => l.IsActive),
                InactiveLecturers = lecturers.Count(l => !l.IsActive),
                TotalPaymentsThisMonth = (decimal)thisMonthClaims.Sum(c => c.Total),
                TotalClaimsThisMonth = thisMonthClaims.Count,
                AverageClaimAmount = thisMonthClaims.Any() ? (decimal)thisMonthClaims.Average(c => c.Total) : 0
            };

            // Department summaries
            var departmentGroups = lecturers
                .GroupBy(l => l.Department)
                .Select(g => new DepartmentSummary
                {
                    Department = g.Key ?? "No Department",
                    LecturerCount = g.Count(),
                    TotalPayments = (decimal)thisMonthClaims
                        .Where(c => g.Any(l => l.FullName == c.Lecturer))
                        .Sum(c => c.Total),
                    TotalClaims = thisMonthClaims
                        .Count(c => g.Any(l => l.FullName == c.Lecturer))
                })
                .ToList();

            analytics.DepartmentSummaries = departmentGroups;

            return analytics;
        }
    }
}