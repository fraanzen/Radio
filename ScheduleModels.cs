using System;
using System.Collections.Generic;
using System.Linq;

namespace Radio.Models
{
    // Base class for all scheduled content types
    public abstract class ScheduledContent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public virtual DateTime EndTime => StartTime.Add(Duration);
        
        public int? DailyScheduleId { get; set; }
        public virtual DailySchedule? DailySchedule { get; set; }

        protected ContentType _contentType;
        public virtual ContentType ContentType 
        {
            get => _contentType; 
            protected set => _contentType = value;
        }
    }

    public enum ContentType
    {
        Music,
        Reportage,
        LiveSession
    }

    public enum StudioType
    {
        Studio1,
        Studio2
    }

    // Music content for filling time slots
    public class MusicContent : ScheduledContent
    {
        public MusicContent()
        {
            _contentType = ContentType.Music;
        }
        public string Genre { get; set; } = string.Empty;
    }

    // News or report content
    public class Reportage : ScheduledContent
    {
        public Reportage()
        {
            _contentType = ContentType.Reportage;
        }
        public string Topic { get; set; } = string.Empty;
        public string Reporter { get; set; } = string.Empty;
    }

    // Live radio show with hosts and guests
    public class LiveSession : ScheduledContent
    {
        public LiveSession()
        {
            _contentType = ContentType.LiveSession;
        }
        
        // Database fields for storing the lists as comma-separated strings
        public string HostsData { get; set; } = string.Empty;
        public string GuestsData { get; set; } = string.Empty;
        public StudioType Studio { get; set; }

        // Navigation properties for easy access
        public List<string> Hosts
        {
            get => string.IsNullOrEmpty(HostsData) ? new List<string>() : HostsData.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            set => HostsData = string.Join(',', value);
        }

        public List<string> Guests
        {
            get => string.IsNullOrEmpty(GuestsData) ? new List<string>() : GuestsData.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
            set => GuestsData = string.Join(',', value);
        }

        public bool HasGuests
        {
            get
            {
                return Guests.Count > 0;
            }
        }

        // Determines which studio based on number of hosts and guests
        public void DetermineStudio()
        {
            if (Hosts.Count == 1 && !HasGuests)
            {
                Studio = StudioType.Studio1;
            }
            else
            {
                Studio = StudioType.Studio2;
            }
        }
    }

    // Schedule container for a single day
    public class DailySchedule
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public virtual List<ScheduledContent> Content { get; set; } = new List<ScheduledContent>();

        // Adds content to this schedule
        public void AddContent(ScheduledContent content)
    {
        content.DailyScheduleId = Id;
        content.DailySchedule = this;
        Content.Add(content);
    }

        // Returns content sorted by start time
        public List<ScheduledContent> GetSortedContent()
        {
            List<ScheduledContent> sorted = new List<ScheduledContent>(Content);
            sorted.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return sorted;
        }
    }

    // Schedule container for a week
    public class WeeklySchedule
    {
        public DateTime WeekStartDate { get; set; }
        public List<DailySchedule> DailySchedules { get; set; } = new List<DailySchedule>();

        // Creates a weekly schedule starting from a date
        public WeeklySchedule(DateTime startDate)
        {
            WeekStartDate = startDate.Date;
            for (int i = 0; i < 7; i++)
            {
                DailySchedule daily = new DailySchedule();
                daily.Date = WeekStartDate.AddDays(i);
                DailySchedules.Add(daily);
            }
        }

        // Gets the schedule for a specific date
        public DailySchedule? GetScheduleForDate(DateTime date)
        {
            for (int i = 0; i < DailySchedules.Count; i++)
            {
                if (DailySchedules[i].Date.Date == date.Date)
                {
                    return DailySchedules[i];
                }
            }
            return null;
        }

        // Adds content to the appropriate day
        public void AddContent(ScheduledContent content)
        {
            var schedule = GetScheduleForDate(content.StartTime.Date);
            if (schedule != null)
            {
                schedule.AddContent(content);
            }
        }
    }
}