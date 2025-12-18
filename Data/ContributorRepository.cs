using Microsoft.EntityFrameworkCore;
using Radio.Models;

namespace Radio.Data
{
    public class ContributorRepository
    {
        private readonly RadioDbContext _context;

        public ContributorRepository(RadioDbContext context)
        {
            _context = context;
        }

        // Get all contributors
        public async Task<List<Contributor>> GetAllAsync()
        {
            return await _context.Contributors
                .Include(c => c.User)
                .Include(c => c.PaymentHistory)
                .Include(c => c.Assignments)
                .ToListAsync();
        }

        // Get contributor by ID
        public async Task<Contributor?> GetByIdAsync(int id)
        {
            return await _context.Contributors
                .Include(c => c.User)
                .Include(c => c.PaymentHistory)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.ScheduledContent)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        // Get contributor by UserId
        public async Task<Contributor?> GetByUserIdAsync(string userId)
        {
            return await _context.Contributors
                .Include(c => c.User)
                .Include(c => c.PaymentHistory)
                .Include(c => c.Assignments)
                    .ThenInclude(a => a.ScheduledContent)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }

        // Get contributor by email
        public async Task<Contributor?> GetByEmailAsync(string email)
        {
            return await _context.Contributors
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        // Create new contributor
        public async Task<Contributor> CreateAsync(Contributor contributor)
        {
            _context.Contributors.Add(contributor);
            await _context.SaveChangesAsync();
            return contributor;
        }

        // Update contributor
        public async Task<Contributor> UpdateAsync(Contributor contributor)
        {
            contributor.UpdatedAt = DateTime.UtcNow;
            _context.Contributors.Update(contributor);
            await _context.SaveChangesAsync();
            return contributor;
        }

        // Delete contributor
        public async Task DeleteAsync(int id)
        {
            var contributor = await _context.Contributors.FindAsync(id);
            if (contributor != null)
            {
                _context.Contributors.Remove(contributor);
                await _context.SaveChangesAsync();
            }
        }

        // Assign contributor to scheduled content
        public async Task<ContributorAssignment> AssignToContentAsync(int contributorId, int scheduledContentId, ContributorRole role)
        {
            var assignment = new ContributorAssignment
            {
                ContributorId = contributorId,
                ScheduledContentId = scheduledContentId,
                Role = role
            };

            _context.ContributorAssignments.Add(assignment);
            await _context.SaveChangesAsync();
            return assignment;
        }

        // Remove assignment
        public async Task RemoveAssignmentAsync(int assignmentId)
        {
            var assignment = await _context.ContributorAssignments.FindAsync(assignmentId);
            if (assignment != null)
            {
                _context.ContributorAssignments.Remove(assignment);
                await _context.SaveChangesAsync();
            }
        }

        // Get assignments for a contributor in a date range
        public async Task<List<ContributorAssignment>> GetAssignmentsAsync(int contributorId, DateTime startDate, DateTime endDate)
        {
            return await _context.ContributorAssignments
                .Include(a => a.ScheduledContent)
                .Where(a => a.ContributorId == contributorId &&
                           a.ScheduledContent!.StartTime >= startDate &&
                           a.ScheduledContent.StartTime <= endDate)
                .ToListAsync();
        }

        // Calculate and generate payment record for a month
        public async Task<PaymentRecord> GenerateMonthlyPaymentAsync(int contributorId, int year, int month)
        {
            // Check if payment already exists
            var existingPayment = await _context.PaymentRecords
                .FirstOrDefaultAsync(p => p.ContributorId == contributorId && 
                                         p.Year == year && 
                                         p.Month == month);
            
            if (existingPayment != null)
            {
                return existingPayment;
            }

            // Get all assignments for the month
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);
            
            var assignments = await GetAssignmentsAsync(contributorId, startDate, endDate);

            // Calculate total hours and events
            decimal totalHours = 0;
            int totalEvents = assignments.Count;

            foreach (var assignment in assignments)
            {
                if (assignment.ScheduledContent != null)
                {
                    totalHours += (decimal)assignment.ScheduledContent.Duration.TotalHours;
                }
            }

            // Create payment record
            var paymentRecord = new PaymentRecord
            {
                ContributorId = contributorId,
                Year = year,
                Month = month,
                TotalHours = totalHours,
                TotalEvents = totalEvents
            };

            paymentRecord.CalculatePayment();

            _context.PaymentRecords.Add(paymentRecord);
            await _context.SaveChangesAsync();

            return paymentRecord;
        }

        // Get payment history for a contributor
        public async Task<List<PaymentRecord>> GetPaymentHistoryAsync(int contributorId)
        {
            return await _context.PaymentRecords
                .Where(p => p.ContributorId == contributorId)
                .OrderByDescending(p => p.Year)
                .ThenByDescending(p => p.Month)
                .ToListAsync();
        }

        // Mark payment as paid
        public async Task MarkPaymentAsPaidAsync(int paymentId)
        {
            var payment = await _context.PaymentRecords.FindAsync(paymentId);
            if (payment != null)
            {
                payment.IsPaid = true;
                payment.PaidAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
