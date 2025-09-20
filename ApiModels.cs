using Radio.Models;

namespace Radio.Api
{
    public class EventRequest
    {
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<string> Hosts { get; set; } = new List<string>();
        public List<string> Guests { get; set; } = new List<string>();
        public string? Topic { get; set; }
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
