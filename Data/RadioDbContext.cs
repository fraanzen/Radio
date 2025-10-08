using Microsoft.EntityFrameworkCore;
using Radio.Models;

namespace Radio.Data
{
    // Database context for radio schedule data
    public class RadioDbContext : DbContext
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

        // Configures database schema and relationships
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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
        }
    }
}