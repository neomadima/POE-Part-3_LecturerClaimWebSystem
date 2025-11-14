using LecturerClaimsSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public class MockHrService : IHrService
    {
        private static List<Lecturer> _lecturers = new List<Lecturer>();
        private static List<PaymentReport> _paymentReports = new List<PaymentReport>();
        private static int _nextLecturerId = 1;
        private static int _nextReportId = 1;

        private readonly IClaimService _claimService;

        public MockHrService(IClaimService claimService)
        {
            _claimService = claimService;
            InitializeMockData();
        }

        private void InitializeMockData()
        {
            // Initialize lecturers if empty
            if (!_lecturers.Any())
            {
                _lecturers.AddRange(new[]
                {
                    new Lecturer
                    {
                        Id = _nextLecturerId++,
                        EmployeeId = "EMP001",
                        FirstName = "John",
                        LastName = "Doe",
                        Email = "john.doe@university.edu",
                        PhoneNumber = "+1234567890",
                        Department = "Computer Science",
                        Position = "Senior Lecturer",
                        HireDate = DateTime.Now.AddYears(-2),
                        DefaultHourlyRate = 55.0m,
                        IsActive = true
                    },
                    new Lecturer
                    {
                        Id = _nextLecturerId++,
                        EmployeeId = "EMP002",
                        FirstName = "Jane",
                        LastName = "Smith",
                        Email = "jane.smith@university.edu",
                        PhoneNumber = "+1234567891",
                        Department = "Mathematics",
                        Position = "Lecturer",
                        HireDate = DateTime.Now.AddYears(-1),
                        DefaultHourlyRate = 45.0m,
                        IsActive = true
                    },
                    new Lecturer
                    {
                        Id = _nextLecturerId++,
                        EmployeeId = "EMP003",
                        FirstName = "Robert",
                        LastName = "Johnson",
                        Email = "robert.johnson@university.edu",
                        PhoneNumber = "+1234567892",
                        Department = "Physics",
                        Position = "Associate Professor",
                        HireDate = DateTime.Now.AddYears(-3),
                        DefaultHourlyRate = 65.0m,
                        IsActive = true
                    }
                });
            }

            // Add some test claims to match the lecturers
            var claims = _claimService.GetApprovedClaimsAsync().Result;
            if (!claims.Any())
            {
                // Add some approved claims for testing
                _claimService.SubmitClaimAsync(new Claim
                {
                    Lecturer = "John Doe",
                    Date = DateTime.Now.AddDays(-5),
                    Hours = 8,
                    Rate = 55,
                    Notes = "Guest lecture for CS101",
                    Status = "Approved",
                    ApprovedBy = "Academic Manager"
                }).Wait();

                _claimService.SubmitClaimAsync(new Claim
                {
                    Lecturer = "Jane Smith",
                    Date = DateTime.Now.AddDays(-3),
                    Hours = 5,
                    Rate = 45,
                    Notes = "Tutorial sessions",
                    Status = "Approved",
                    ApprovedBy = "Programme Coordinator"
                }).Wait();
            }
        }

        // Lecturer Management
        public async Task<List<Lecturer>> GetAllLecturersAsync()
        {
            return await Task.FromResult(_lecturers.OrderBy(l => l.LastName).ThenBy(l => l.FirstName).ToList());
        }

        public async Task<Lecturer?> GetLecturerByIdAsync(int id)
        {
            return await Task.FromResult(_lecturers.FirstOrDefault(l => l.Id == id));
        }

        public async Task<Lecturer?> GetLecturerByEmployeeIdAsync(string employeeId)
        {
            return await Task.FromResult(_lecturers.FirstOrDefault(l => l.EmployeeId == employeeId));
        }

        public async Task<bool> CreateLecturerAsync(Lecturer lecturer)
        {
            try
            {
                lecturer.Id = _nextLecturerId++;
                lecturer.CreatedDate = DateTime.Now;
                _lecturers.Add(lecturer);
                return await Task.FromResult(true);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> UpdateLecturerAsync(Lecturer lecturer)
        {
            try
            {
                var existingLecturer = _lecturers.FirstOrDefault(l => l.Id == lecturer.Id);
                if (existingLecturer != null)
                {
                    existingLecturer.EmployeeId = lecturer.EmployeeId;
                    existingLecturer.FirstName = lecturer.FirstName;
                    existingLecturer.LastName = lecturer.LastName;
                    existingLecturer.Email = lecturer.Email;
                    existingLecturer.PhoneNumber = lecturer.PhoneNumber;
                    existingLecturer.Department = lecturer.Department;
                    existingLecturer.Position = lecturer.Position;
                    existingLecturer.DefaultHourlyRate = lecturer.DefaultHourlyRate;
                    existingLecturer.IsActive = lecturer.IsActive;
                    existingLecturer.UpdatedDate = DateTime.Now;

                    return await Task.FromResult(true);
                }
                return await Task.FromResult(false);
            }
            catch
            {
                return await Task.FromResult(false);
            }
        }

        public async Task<bool> DeactivateLecturerAsync(int id)
        {
            var lecturer = _lecturers.FirstOrDefault(l => l.Id == id);
            if (lecturer != null)
            {
                lecturer.IsActive = false;
                lecturer.UpdatedDate = DateTime.Now;
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        // Payment Reports
        public async Task<PaymentReport> GeneratePaymentReportAsync(DateTime periodStart, DateTime periodEnd, string generatedBy)
        {
            var claims = await GetClaimsForPaymentAsync(periodStart, periodEnd);
            var totalAmount = claims.Sum(c => c.Total);

            var report = new PaymentReport
            {
                Id = _nextReportId++,
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

            _paymentReports.Add(report);
            return await Task.FromResult(report);
        }

        public async Task<List<PaymentReport>> GetPaymentReportsAsync()
        {
            return await Task.FromResult(_paymentReports.OrderByDescending(pr => pr.ReportDate).ToList());
        }

        public async Task<PaymentReport?> GetPaymentReportByIdAsync(int id)
        {
            return await Task.FromResult(_paymentReports.FirstOrDefault(pr => pr.Id == id));
        }

        public async Task<List<Claim>> GetClaimsForPaymentAsync(DateTime periodStart, DateTime periodEnd)
        {
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

            return await Task.FromResult(analytics);
        }
    }
}