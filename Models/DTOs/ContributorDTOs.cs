using System.ComponentModel.DataAnnotations;

namespace Radio.Models.DTOs
{
    // Authentication DTOs
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
    }

    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
        public int? ContributorId { get; set; }
    }

    // Contributor DTOs
    public class ContributorDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class ContributorDetailDto : ContributorDto
    {
        public List<PaymentRecordDto> PaymentHistory { get; set; } = new();
        public List<AssignmentDto> RecentAssignments { get; set; } = new();
    }

    public class CreateContributorDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        public string LastName { get; set; } = string.Empty;
        
        [Required]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
    }

    public class UpdateContributorDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? PhotoUrl { get; set; }
        public string? Biography { get; set; }
    }

    // Payment DTOs
    public class PaymentRecordDto
    {
        public int Id { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal TotalHours { get; set; }
        public int TotalEvents { get; set; }
        public decimal SubtotalAmount { get; set; }
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime GeneratedAt { get; set; }
        public bool IsPaid { get; set; }
        public DateTime? PaidAt { get; set; }
    }

    // Assignment DTOs
    public class AssignmentDto
    {
        public int Id { get; set; }
        public int ContributorId { get; set; }
        public string ContributorName { get; set; } = string.Empty;
        public int ScheduledContentId { get; set; }
        public string ContentTitle { get; set; } = string.Empty;
        public DateTime ContentStartTime { get; set; }
        public TimeSpan ContentDuration { get; set; }
        public string Role { get; set; } = string.Empty;
        public DateTime AssignedAt { get; set; }
    }

    public class CreateAssignmentDto
    {
        [Required]
        public int ContributorId { get; set; }
        
        [Required]
        public int ScheduledContentId { get; set; }
        
        [Required]
        public string Role { get; set; } = string.Empty; // Host, CoHost, Guest, Reporter
    }
}
