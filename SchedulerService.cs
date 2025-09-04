using System;
using System.Collections.Generic;
using Radio.Models;

namespace Radio.Services
{
    public class SchedulerService
    {
        private WeeklySchedule currentSchedule;
        private int nextContentId = 1;

        public SchedulerService()
        {
            currentSchedule = new WeeklySchedule(DateTime.Today);
        }

        public DateTime GetDay(string dayName)
        {
            DayOfWeek targetDay = ParseDayName(dayName);
            DateTime today = DateTime.Today;
            DayOfWeek currentDay = today.DayOfWeek;
            
            int daysUntilTarget = ((int)targetDay - (int)currentDay + 7) % 7;
            if (daysUntilTarget == 0 && targetDay != currentDay)
            {
                daysUntilTarget = 7;
            }
            
            return today.AddDays(daysUntilTarget);
        }

        private DayOfWeek ParseDayName(string dayName)
        {
            string day = dayName.ToLower();
            if (day == "sunday") return DayOfWeek.Sunday;
            if (day == "monday") return DayOfWeek.Monday;
            if (day == "tuesday") return DayOfWeek.Tuesday;
            if (day == "wednesday") return DayOfWeek.Wednesday;
            if (day == "thursday") return DayOfWeek.Thursday;
            if (day == "friday") return DayOfWeek.Friday;
            if (day == "saturday") return DayOfWeek.Saturday;
            if (day == "today") return DateTime.Today.DayOfWeek;
            if (day == "tomorrow") return DateTime.Today.AddDays(1).DayOfWeek;
            
            throw new ArgumentException($"Invalid day name: {dayName}");
        }

        public void ScheduleReportage(DateTime startTime, TimeSpan duration, string title, string topic, string reporter)
        {
            Reportage reportage = new Reportage();
            reportage.Id = nextContentId++;
            reportage.Title = title;
            reportage.StartTime = startTime;
            reportage.Duration = duration;
            reportage.Topic = topic;
            reportage.Reporter = reporter;

            currentSchedule.AddContent(reportage);
        }

        public void ScheduleLiveSession(DateTime startTime, TimeSpan duration, string title, List<string> hosts, List<string> guests = null)
        {
            LiveSession session = new LiveSession();
            session.Id = nextContentId++;
            session.Title = title;
            session.StartTime = startTime;
            session.Duration = duration;
            session.Hosts = hosts ?? new List<string>();
            session.Guests = guests ?? new List<string>();
            
            session.DetermineStudio();

            currentSchedule.AddContent(session);
        }

        public void FillWithMusic()
        {
            for (int i = 0; i < currentSchedule.DailySchedules.Count; i++)
            {
                DailySchedule daily = currentSchedule.DailySchedules[i];
                FillDayWithMusic(daily);
            }
        }

        private void FillDayWithMusic(DailySchedule daily)
        {
            DateTime dayStart = daily.Date;
            DateTime dayEnd = daily.Date.AddDays(1);
            
            List<ScheduledContent> sorted = daily.GetSortedContent();
            
            if (sorted.Count > 0 && sorted[0].StartTime > dayStart)
            {
                AddMusicBlock(daily, dayStart, sorted[0].StartTime);
            }
            
            for (int i = 0; i < sorted.Count - 1; i++)
            {
                if (sorted[i].EndTime < sorted[i + 1].StartTime)
                {
                    AddMusicBlock(daily, sorted[i].EndTime, sorted[i + 1].StartTime);
                }
            }
            
            if (sorted.Count > 0 && sorted[sorted.Count - 1].EndTime < dayEnd)
            {
                AddMusicBlock(daily, sorted[sorted.Count - 1].EndTime, dayEnd);
            }
            
            if (sorted.Count == 0)
            {
                AddMusicBlock(daily, dayStart, dayEnd);
            }
        }

        private void AddMusicBlock(DailySchedule daily, DateTime start, DateTime end)
        {
            MusicContent music = new MusicContent();
            music.Id = nextContentId++;
            music.Title = "Music Playlist";
            music.StartTime = start;
            music.Duration = end - start;
            music.Genre = "Mixed";

            daily.AddContent(music);
        }

        public string GetScheduleOverview()
        {
            string overview = "Radio Schedule - Week of " + currentSchedule.WeekStartDate.ToString("yyyy-MM-dd") + "\n\n";

            for (int i = 0; i < currentSchedule.DailySchedules.Count; i++)
            {
                DailySchedule daily = currentSchedule.DailySchedules[i];
                overview += daily.Date.ToString("dddd, MMM dd") + ":\n";

                List<ScheduledContent> content = daily.GetSortedContent();
                for (int j = 0; j < content.Count; j++)
                {
                    ScheduledContent item = content[j];
                    string type = GetContentTypeString(item.ContentType);
                    overview += "  " + item.StartTime.ToString("HH:mm") + "-" + item.EndTime.ToString("HH:mm") + " " + type + ": " + item.Title + "\n";
                }
                overview += "\n";
            }

            return overview;
        }

        private string GetContentTypeString(ContentType type)
        {
            if (type == ContentType.Music) return "[Music]";
            if (type == ContentType.Reportage) return "[Reportage]";
            if (type == ContentType.LiveSession) return "[Live]";
            return "[Unknown]";
        }

        public string GetStudioSummary()
        {
            string summary = "Studio Assignment Summary:\n";
            for (int i = 0; i < currentSchedule.DailySchedules.Count; i++)
            {
                DailySchedule daily = currentSchedule.DailySchedules[i];
                for (int j = 0; j < daily.Content.Count; j++)
                {
                    if (daily.Content[j] is LiveSession session)
                    {
                        string studioInfo = session.Studio == StudioType.Studio1 ? "Studio 1 (cheaper)" : "Studio 2";
                        string guestInfo = session.HasGuests ? $" + {session.Guests.Count} guest(s)" : "";
                        summary += $"  {session.Title}: {studioInfo}{guestInfo}\n";
                    }
                }
            }
            return summary;
        }
    }
}
