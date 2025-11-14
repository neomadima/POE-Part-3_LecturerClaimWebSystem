using System.Collections.Generic;

namespace LecturerClaimsSystem.Models
{
    public class HrAnalytics
    {
        public int TotalLecturers { get; set; }
        public int ActiveLecturers { get; set; }
        public int InactiveLecturers { get; set; }
        public decimal TotalPaymentsThisMonth { get; set; }
        public int TotalClaimsThisMonth { get; set; }
        public decimal AverageClaimAmount { get; set; }
        public List<DepartmentSummary> DepartmentSummaries { get; set; } = new List<DepartmentSummary>();
    }

    public class DepartmentSummary
    {
        public string Department { get; set; } = string.Empty;
        public int LecturerCount { get; set; }
        public decimal TotalPayments { get; set; }
        public int TotalClaims { get; set; }
    }
}