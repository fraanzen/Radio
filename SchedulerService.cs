using System;
using System.Collections.Generic;
using System.Linq;
using Radio.Models;
using Radio.Data;

namespace Radio.Services
{
    public class SchedulerService
    {
        private readonly RadioDbContext _db;

        public SchedulerService(RadioDbContext db)
        {
            _db = db;
        }

        // Gets a date based on day name like "today", "monday", etc.
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

        // Converts day name string to DayOfWeek enum
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

        // Removes events (especially music) that conflict with a new event
        private void RemoveConflictingEvents(DateTime startTime, TimeSpan duration)
        {
            DateTime endTime = startTime + duration;
            
            var conflictingEvents = _db.ScheduledContents
                .Where(e => e.StartTime.Date == startTime.Date)
                .ToList()
                .Where(e => {
                    DateTime eventEnd = e.StartTime + e.Duration;
                    return e.StartTime < endTime && eventEnd > startTime;
                })
                .ToList();

            foreach (var conflict in conflictingEvents)
            {
                var conflictEnd = conflict.StartTime + conflict.Duration;
                
                // Music block completely wraps around the new event
                if (conflict is MusicContent && conflict.StartTime < startTime && conflictEnd > endTime)
                {
                    var beforeDuration = startTime - conflict.StartTime;
                    var afterStart = endTime;
                    var afterDuration = conflictEnd - endTime;
                    
                    // Shorten the existing music to end when new event starts
                    conflict.Duration = beforeDuration;
                    
                    // Add new music block after the new event
                    if (afterDuration.TotalMinutes > 0)
                    {
                        AddMusicBlock(afterStart, conflictEnd);
                    }
                }
                // Music block starts before and overlaps into new event
                else if (conflict is MusicContent && conflict.StartTime < startTime && conflictEnd > startTime)
                {
                    conflict.Duration = startTime - conflict.StartTime;
                }
                // Music block starts during new event and extends past it
                else if (conflict is MusicContent && conflict.StartTime < endTime && conflictEnd > endTime)
                {
                    conflict.StartTime = endTime;
                    conflict.Duration = conflictEnd - endTime;
                }
                // Music block is completely within the new event
                else if (conflict is MusicContent)
                {
                    _db.ScheduledContents.Remove(conflict);
                }
                // Non-music events (shouldn't happen, but handle it)
                else
                {
                    _db.ScheduledContents.Remove(conflict);
                }
            }

            _db.SaveChanges();
        }

        // Creates a reportage event
        public void ScheduleReportage(DateTime startTime, TimeSpan duration, string title, string topic, string reporter)
        {
            RemoveConflictingEvents(startTime, duration);
            
            var dailySchedule = GetOrCreateDailySchedule(startTime.Date);
            
            var reportage = new Reportage
            {
                Title = title,
                StartTime = startTime,
                Duration = duration,
                Topic = topic,
                Reporter = reporter,
                DailyScheduleId = dailySchedule.Id
            };

            _db.Reportages.Add(reportage);
            _db.SaveChanges();
        }

        // Creates a live session event
        public void ScheduleLiveSession(DateTime startTime, TimeSpan duration, string title, List<string> hosts, List<string>? guests = null)
        {
            RemoveConflictingEvents(startTime, duration);
            
            var dailySchedule = GetOrCreateDailySchedule(startTime.Date);
            
            var session = new LiveSession
            {
                Title = title,
                StartTime = startTime,
                Duration = duration,
                Hosts = hosts ?? new List<string>(),
                Guests = guests ?? new List<string>(),
                DailyScheduleId = dailySchedule.Id
            };
            
            session.DetermineStudio();

            _db.LiveSessions.Add(session);
            _db.SaveChanges();
        }

        // Fills empty time slots with music for 7 days
        public void FillWithMusic()
        {
            DateTime today = DateTime.Today;
            
            for (int i = 0; i < 7; i++)
            {
                DateTime currentDay = today.AddDays(i);
                FillDayWithMusic(currentDay);
            }
        }

        // Fills empty time slots with music for a single day
        private void FillDayWithMusic(DateTime date)
        {
            DateTime dayStart = date.Date;
            DateTime dayEnd = date.Date.AddDays(1);

            var dayContent = _db.ScheduledContents
                .Where(c => c.StartTime.Date == date.Date)
                .OrderBy(c => c.StartTime)
                .ToList();

            if (dayContent.Count > 0 && dayContent[0].StartTime > dayStart)
            {
                AddMusicBlock(dayStart, dayContent[0].StartTime);
            }

            for (int i = 0; i < dayContent.Count - 1; i++)
            {
                if (dayContent[i].EndTime < dayContent[i + 1].StartTime)
                {
                    AddMusicBlock(dayContent[i].EndTime, dayContent[i + 1].StartTime);
                }
            }

            if (dayContent.Count > 0 && dayContent[dayContent.Count - 1].EndTime < dayEnd)
            {
                AddMusicBlock(dayContent[dayContent.Count - 1].EndTime, dayEnd);
            }

            if (dayContent.Count == 0)
            {
                AddMusicBlock(dayStart, dayEnd);
            }
        }

        // Adds a music block between start and end times
        private void AddMusicBlock(DateTime start, DateTime end)
        {
            var dailySchedule = GetOrCreateDailySchedule(start.Date);
            
            var music = new MusicContent
            {
                Title = "Music Playlist",
                StartTime = start,
                Duration = end - start,
                Genre = "Mixed",
                DailyScheduleId = dailySchedule.Id
            };

            _db.MusicContents.Add(music);
            _db.SaveChanges();
        }

        // Gets or creates a DailySchedule for a specific date
        private DailySchedule GetOrCreateDailySchedule(DateTime date)
        {
            var schedule = _db.DailySchedules.FirstOrDefault(ds => ds.Date.Date == date.Date);
            
            if (schedule == null)
            {
                schedule = new DailySchedule { Date = date.Date };
                _db.DailySchedules.Add(schedule);
                _db.SaveChanges();
            }
            
            return schedule;
        }

        // Gets all events for today
        public List<ScheduledContent> GetTodaySchedule()
        {
            DateTime today = DateTime.Today;
            
            return _db.ScheduledContents
                .Where(c => c.StartTime.Date == today)
                .OrderBy(c => c.StartTime)
                .ToList();
        }

        // Gets schedule for the next 7 days
        public List<DailySchedule> GetSevenDaySchedule()
        {
            DateTime today = DateTime.Today;
            var schedules = new List<DailySchedule>();

            for (int i = 0; i < 7; i++)
            {
                DateTime currentDay = today.AddDays(i);
                
                var daySchedule = new DailySchedule
                {
                    Date = currentDay,
                    Content = _db.ScheduledContents
                        .Where(c => c.StartTime.Date == currentDay.Date)
                        .OrderBy(c => c.StartTime)
                        .ToList()
                };

                schedules.Add(daySchedule);
            }

            return schedules;
        }

        // Gets a specific event by ID
        public ScheduledContent? GetEventById(int id)
        {
            return _db.ScheduledContents.Find(id);
        }

        // Changes the start time of an event
        public bool RescheduleEvent(int id, DateTime newStartTime)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content == null) return false;

            content.StartTime = newStartTime;
            _db.SaveChanges();
            
            return true;
        }

        // Adds a host to a live session
        public bool AddHostToEvent(int id, string hostName)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content is LiveSession session)
            {
                if (!session.Hosts.Contains(hostName))
                {
                    session.Hosts.Add(hostName);
                    session.DetermineStudio();
                    _db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        // Removes a host from a live session
        public bool RemoveHostFromEvent(int id, string hostName)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content is LiveSession session)
            {
                bool removed = session.Hosts.Remove(hostName);
                if (removed)
                {
                    session.DetermineStudio();
                    _db.SaveChanges();
                }
                return removed;
            }
            return false;
        }

        // Adds a guest to a live session
        public bool AddGuestToEvent(int id, string guestName)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content is LiveSession session)
            {
                if (!session.Guests.Contains(guestName))
                {
                    session.Guests.Add(guestName);
                    session.DetermineStudio();
                    _db.SaveChanges();
                    return true;
                }
            }
            return false;
        }

        // Removes a guest from a live session
        public bool RemoveGuestFromEvent(int id, string guestName)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content is LiveSession session)
            {
                bool removed = session.Guests.Remove(guestName);
                if (removed)
                {
                    session.DetermineStudio();
                    _db.SaveChanges();
                }
                return removed;
            }
            return false;
        }

        // Deletes an event from the schedule
        public bool DeleteEvent(int id)
        {
            var content = _db.ScheduledContents.Find(id);
            if (content == null) return false;

            _db.ScheduledContents.Remove(content);
            _db.SaveChanges();
            
            return true;
        }
    }
}
