using Radio.Models;

namespace Radio.Api
{
    /// <summary>
    /// Request model for creating a new event in the radio schedule
    /// </summary>
    public class EventRequest
    {
        /// <summary>
        /// Type of event: "live" for live sessions or "reportage" for news/reports
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Start time of the event
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Duration of the event in minutes
        /// </summary>
        public int DurationMinutes { get; set; }

        /// <summary>
        /// Title of the event
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// List of hosts for live sessions
        /// </summary>
        public List<string> Hosts { get; set; } = new List<string>();

        /// <summary>
        /// List of guests for live sessions (optional)
        /// </summary>
        public List<string> Guests { get; set; } = new List<string>();

        /// <summary>
        /// Topic for reportage events
        /// </summary>
        public string? Topic { get; set; }

        /// <summary>
        /// Reporter name for reportage events
        /// </summary>
        public string? Reporter { get; set; }
    }

    public class EventResponse
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Hosts { get; set; } = new List<string>();
        public List<string> Guests { get; set; } = new List<string>();
        public string? Topic { get; set; }
        public string? Reporter { get; set; }
        public string? Genre { get; set; }
        public string? Studio { get; set; }
    }

    public class RescheduleRequest
    {
        public DateTime NewStartTime { get; set; }
    }

    public class HostRequest
    {
        public string HostName { get; set; } = string.Empty;
    }

    public class GuestRequest
    {
        public string GuestName { get; set; } = string.Empty;
    }

    public class ScheduleResponse
    {
        public DateTime Date { get; set; }
        public List<EventResponse> Events { get; set; } = new List<EventResponse>();
    }
}
