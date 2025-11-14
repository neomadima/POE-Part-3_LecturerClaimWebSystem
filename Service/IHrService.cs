using LecturerClaimsSystem.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LecturerClaimsSystem.Services
{
    public interface IHrService
    {
        // Lecturer Management
        Task<List<Lecturer>> GetAllLecturersAsync();
        Task<Lecturer?> GetLecturerByIdAsync(int id);
        Task<Lecturer?> GetLecturerByEmployeeIdAsync(string employeeId);
        Task<bool> CreateLecturerAsync(Lecturer lecturer);
        Task<bool> UpdateLecturerAsync(Lecturer lecturer);
        Task<bool> DeactivateLecturerAsync(int id);

        // Payment Reports
        Task<PaymentReport> GeneratePaymentReportAsync(DateTime periodStart, DateTime periodEnd, string generatedBy);
        Task<List<PaymentReport>> GetPaymentReportsAsync();
        Task<PaymentReport?> GetPaymentReportByIdAsync(int id);

        // Analytics
        Task<HrAnalytics> GetHrAnalyticsAsync();
        Task<List<Claim>> GetClaimsForPaymentAsync(DateTime periodStart, DateTime periodEnd);
    }
}