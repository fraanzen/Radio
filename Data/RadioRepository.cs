using Microsoft.EntityFrameworkCore;
using Radio.Models;

namespace Radio.Data
{
    public class RadioRepository
    {
        private readonly RadioDbContext _context;

        public RadioRepository(RadioDbContext context)
        {
            _context = context;
        }

        public async Task<List<DailySchedule>> GetWeeklyScheduleAsync(DateTime startDate)
        {
            return await _context.DailySchedules
                .Include(d => d.Content)
                .Where(d => d.Date >= startDate && d.Date < startDate.AddDays(7))
                .OrderBy(d => d.Date)
                .ToListAsync();
        }

        public async Task<DailySchedule?> GetDailyScheduleAsync(DateTime date)
        {
            return await _context.DailySchedules
                .Include(d => d.Content)
                .FirstOrDefaultAsync(d => d.Date.Date == date.Date);
        }

        public async Task<ScheduledContent?> GetEventByIdAsync(int id)
        {
            return await _context.ScheduledContents.FindAsync(id);
        }

        public async Task AddEventAsync(ScheduledContent content)
        {
            var schedule = await GetOrCreateDailyScheduleAsync(content.StartTime.Date);
            schedule.Content.Add(content);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UpdateEventAsync(ScheduledContent content)
        {
            _context.ScheduledContents.Update(content);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var content = await _context.ScheduledContents.FindAsync(id);
            if (content == null) return false;

            _context.ScheduledContents.Remove(content);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        private async Task<DailySchedule> GetOrCreateDailyScheduleAsync(DateTime date)
        {
            var schedule = await _context.DailySchedules
                .Include(d => d.Content)
                .FirstOrDefaultAsync(d => d.Date.Date == date.Date);

            if (schedule == null)
            {
                schedule = new DailySchedule { Date = date };
                _context.DailySchedules.Add(schedule);
            }

            return schedule;
        }
    }
}