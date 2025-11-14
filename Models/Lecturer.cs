using System.ComponentModel.DataAnnotations;

namespace LecturerClaimsSystem.Models
{
    public class Lecturer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string EmployeeId { get; set; } = string.Empty;

        [Required]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;

        public string Department { get; set; } = string.Empty;

        public string Position { get; set; } = string.Empty;

        public DateTime HireDate { get; set; } = DateTime.Now;

        public decimal DefaultHourlyRate { get; set; } = 50.0m;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }

        public string FullName => $"{FirstName} {LastName}";
    }

    public class PaymentReport
    {
        public int Id { get; set; }
        public string ReportName { get; set; } = string.Empty;
        public DateTime ReportDate { get; set; } = DateTime.Now;
        public DateTime PeriodStart { get; set; }
        public DateTime PeriodEnd { get; set; }
        public decimal TotalAmount { get; set; }
        public int TotalClaims { get; set; }
        public string GeneratedBy { get; set; } = string.Empty;
        public string ReportData { get; set; } = string.Empty; // JSON data
    }
}