using System;
using System.Collections.Generic;

namespace Radio.Models
{
    public abstract class ScheduledContent
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get; set; }
        public DateTime EndTime
        {
            get
            {
                return StartTime.Add(Duration);
            }
        }

        public abstract ContentType ContentType { get; }
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

    public class MusicContent : ScheduledContent
    {
        public override ContentType ContentType
        {
            get
            {
                return ContentType.Music;
            }
        }
        public string Genre { get; set; } = string.Empty;
    }

    public class Reportage : ScheduledContent
    {
        public override ContentType ContentType
        {
            get
            {
                return ContentType.Reportage;
            }
        }
        public string Topic { get; set; } = string.Empty;
        public string Reporter { get; set; } = string.Empty;
    }

    public class LiveSession : ScheduledContent
    {
        public override ContentType ContentType
        {
            get
            {
                return ContentType.LiveSession;
            }
        }
        public List<string> Hosts { get; set; } = new List<string>();
        public List<string> Guests { get; set; } = new List<string>();
        public StudioType Studio { get; set; }

        public bool HasGuests
        {
            get
            {
                return Guests.Count > 0;
            }
        }

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

    public class DailySchedule
    {
        public DateTime Date { get; set; }
        public List<ScheduledContent> Content { get; set; } = new List<ScheduledContent>();

        public void AddContent(ScheduledContent content)
        {
            Content.Add(content);
        }

        public List<ScheduledContent> GetSortedContent()
        {
            List<ScheduledContent> sorted = new List<ScheduledContent>(Content);
            sorted.Sort((a, b) => a.StartTime.CompareTo(b.StartTime));
            return sorted;
        }
    }

    public class WeeklySchedule
    {
        public DateTime WeekStartDate { get; set; }
        public List<DailySchedule> DailySchedules { get; set; } = new List<DailySchedule>();

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

        public DailySchedule GetScheduleForDate(DateTime date)
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

        public void AddContent(ScheduledContent content)
        {
            DailySchedule schedule = GetScheduleForDate(content.StartTime.Date);
            if (schedule != null)
            {
                schedule.AddContent(content);
            }
        }
    }
}