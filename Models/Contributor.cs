using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Radio.Models
{
    public class Contributor
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public string FullName => $"{FirstName} {LastName}";
        
        // Contact Information
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        
        [Phone]
        public string PhoneNumber { get; set; } = string.Empty;
        
        [Required]
        public string Address { get; set; } = string.Empty;
        
        // Promotional Material (Optional)
        public string? PhotoUrl { get; set; }
        
        [MaxLength(1000)]
        public string? Biography { get; set; }
        
        // Payment Information
        public const decimal HourlyRate = 750m; // SEK per hour
        public const decimal EventFee = 300m;   // SEK per event
        public const decimal VatRate = 0.25m;   // 25% VAT
        
        // Relationship to Identity
        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual IdentityUser? User { get; set; }
        
        // Navigation properties
        public virtual ICollection<PaymentRecord> PaymentHistory { get; set; } = new List<PaymentRecord>();
        public virtual ICollection<ContributorAssignment> Assignments { get; set; } = new List<ContributorAssignment>();
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
    
    // Track which events a contributor is assigned to
    public class ContributorAssignment
    {
        public int Id { get; set; }
        
        public int ContributorId { get; set; }
        public virtual Contributor? Contributor { get; set; }
        
        public int ScheduledContentId { get; set; }
        public virtual ScheduledContent? ScheduledContent { get; set; }
        
        public ContributorRole Role { get; set; }
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
    
    public enum ContributorRole
    {
        Host,
        CoHost,
        Guest,
        Reporter
    }
    
    // Payment records for contributors
    public class PaymentRecord
    {
        public int Id { get; set; }
        
        public int ContributorId { get; set; }
        public virtual Contributor? Contributor { get; set; }
        
        public int Year { get; set; }
        public int Month { get; set; }
        
        // Calculated amounts
        public decimal TotalHours { get; set; }
        public int TotalEvents { get; set; }
        
        public decimal SubtotalAmount { get; set; } // Before VAT
        public decimal VatAmount { get; set; }
        public decimal TotalAmount { get; set; } // Including VAT
        
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
        public bool IsPaid { get; set; } = false;
        public DateTime? PaidAt { get; set; }
        
        // Calculate payment based on hours and events
        public void CalculatePayment()
        {
            decimal hourlyPayment = TotalHours * Contributor.HourlyRate;
            decimal eventPayment = TotalEvents * Contributor.EventFee;
            SubtotalAmount = hourlyPayment + eventPayment;
            VatAmount = SubtotalAmount * Contributor.VatRate;
            TotalAmount = SubtotalAmount + VatAmount;
        }
    }
}
