using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Radio.Models;

namespace Radio.Data
{
    // Database context for radio schedule data with Identity support
    public class RadioDbContext : IdentityDbContext<IdentityUser>
    {
        public RadioDbContext(DbContextOptions<RadioDbContext> options)
            : base(options)
        {
        }

        public DbSet<DailySchedule> DailySchedules { get; set; }
        public DbSet<ScheduledContent> ScheduledContents { get; set; }
        public DbSet<LiveSession> LiveSessions { get; set; }
        public DbSet<Reportage> Reportages { get; set; }
        public DbSet<MusicContent> MusicContents { get; set; }
        public DbSet<Contributor> Contributors { get; set; }
        public DbSet<ContributorAssignment> ContributorAssignments { get; set; }
        public DbSet<PaymentRecord> PaymentRecords { get; set; }

        // Configures database schema and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder); // Important for Identity tables
            
            // Configure inheritance
            modelBuilder.Entity<ScheduledContent>()
                .HasDiscriminator<string>("Discriminator")
                .HasValue<LiveSession>("LiveSession")
                .HasValue<Reportage>("Reportage")
                .HasValue<MusicContent>("MusicContent");

            // Configure relationships
            modelBuilder.Entity<DailySchedule>()
                .HasMany(d => d.Content)
                .WithOne(s => s.DailySchedule)
                .HasForeignKey(s => s.DailyScheduleId)
                .OnDelete(DeleteBehavior.Cascade);

            // Store enums as strings
            modelBuilder.Entity<ScheduledContent>()
                .Property(e => e.ContentType)
                .HasConversion<string>();

            modelBuilder.Entity<LiveSession>()
                .Property(l => l.Studio)
                .HasConversion<string>();
            
            // Contributor relationships
            modelBuilder.Entity<Contributor>()
                .HasOne(c => c.User)
                .WithMany()
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            
            modelBuilder.Entity<Contributor>()
                .HasMany(c => c.PaymentHistory)
                .WithOne(p => p.Contributor)
                .HasForeignKey(p => p.ContributorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<Contributor>()
                .HasMany(c => c.Assignments)
                .WithOne(a => a.Contributor)
                .HasForeignKey(a => a.ContributorId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ContributorAssignment>()
                .HasOne(a => a.ScheduledContent)
                .WithMany()
                .HasForeignKey(a => a.ScheduledContentId)
                .OnDelete(DeleteBehavior.Cascade);
            
            modelBuilder.Entity<ContributorAssignment>()
                .Property(a => a.Role)
                .HasConversion<string>();
            
            // Add indexes for performance
            modelBuilder.Entity<Contributor>()
                .HasIndex(c => c.Email)
                .IsUnique();
            
            modelBuilder.Entity<Contributor>()
                .HasIndex(c => c.UserId)
                .IsUnique();
            
            modelBuilder.Entity<PaymentRecord>()
                .HasIndex(p => new { p.ContributorId, p.Year, p.Month })
                .IsUnique();
        }
    }
}