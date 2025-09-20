using Radio.Models;
using Radio.Services;
using Radio.Api;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var scheduler = new SchedulerService();

scheduler.ScheduleReportage(
    scheduler.GetDay("today").AddHours(8), 
    TimeSpan.FromMinutes(30), 
    "Morning News", 
    "Local News", 
    "John Reporter"
);

scheduler.ScheduleLiveSession(
    scheduler.GetDay("today").AddHours(7), 
    TimeSpan.FromHours(1), 
    "Morning Show", 
    new List<string> { "Mike Host" }
);

scheduler.ScheduleLiveSession(
    scheduler.GetDay("today").AddHours(16), 
    TimeSpan.FromHours(2), 
    "Afternoon Talk", 
    new List<string> { "Sarah Host", "Bob Host" }
);

scheduler.ScheduleLiveSession(
    scheduler.GetDay("sunday").AddHours(10), 
    TimeSpan.FromHours(2), 
    "Sunday Brunch Show", 
    new List<string> { "Alex Host" },
    new List<string> { "Chef Maria" }
);

scheduler.FillWithMusic();

EventResponse ConvertToEventResponse(ScheduledContent content)
{
    EventResponse response = new EventResponse();
    response.Id = content.Id;
    response.StartTime = content.StartTime;
    response.EndTime = content.EndTime;
    response.DurationMinutes = (int)content.Duration.TotalMinutes;
    response.Title = content.Title;

    if (content is LiveSession live)
    {
        response.Type = "live";
        response.Hosts = live.Hosts;
        response.Guests = live.Guests;
        if (live.Studio == StudioType.Studio1)
        {
            response.Studio = "Studio 1";
        }
        else
        {
            response.Studio = "Studio 2";
        }
    }
    else if (content is Reportage reportage)
    {
        response.Type = "reportage";
        response.Topic = reportage.Topic;
        response.Reporter = reportage.Reporter;
    }
    else if (content is MusicContent music)
    {
        response.Type = "music";
        response.Genre = music.Genre;
    }

    return response;
}

app.MapGet("/schedule/today", () =>
{
    var todayEvents = scheduler.GetTodaySchedule();
    var eventList = new List<EventResponse>();
    
    for (int i = 0; i < todayEvents.Count; i++)
    {
        eventList.Add(ConvertToEventResponse(todayEvents[i]));
    }
    
    var response = new ScheduleResponse();
    response.Date = DateTime.Today;
    response.Events = eventList;
    return response;
});

app.MapGet("/schedule/week", () =>
{
    var weekSchedules = scheduler.GetSevenDaySchedule();
    var weekResponse = new List<ScheduleResponse>();
    
    for (int i = 0; i < weekSchedules.Count; i++)
    {
        var daySchedule = weekSchedules[i];
        var dayEvents = daySchedule.GetSortedContent();
        var dayEventsList = new List<EventResponse>();
        
        for (int j = 0; j < dayEvents.Count; j++)
        {
            dayEventsList.Add(ConvertToEventResponse(dayEvents[j]));
        }
        
        var dayResponse = new ScheduleResponse();
        dayResponse.Date = daySchedule.Date;
        dayResponse.Events = dayEventsList;
        weekResponse.Add(dayResponse);
    }
    
    return weekResponse;
});

app.MapGet("/events/{id:int}", (int id) =>
{
    var eventItem = scheduler.GetEventById(id);
    if (eventItem == null)
    {
        return Results.NotFound("Event not found");
    }
    
    return Results.Ok(ConvertToEventResponse(eventItem));
});

app.MapPost("/events", (EventRequest request) =>
{
    if (request.Type == "live")
    {
        List<string>? guests = null;
        if (request.Guests.Count > 0)
        {
            guests = request.Guests;
        }
        
        scheduler.ScheduleLiveSession(
            request.StartTime,
            TimeSpan.FromMinutes(request.DurationMinutes),
            request.Title,
            request.Hosts,
            guests
        );
        return Results.Ok("Live session created");
    }
    else if (request.Type == "reportage")
    {
        if (request.Topic == null || request.Reporter == null)
        {
            return Results.BadRequest("Topic and Reporter required for reportage");
        }
        
        scheduler.ScheduleReportage(
            request.StartTime,
            TimeSpan.FromMinutes(request.DurationMinutes),
            request.Title,
            request.Topic,
            request.Reporter
        );
        return Results.Ok("Reportage created");
    }
    else
    {
        return Results.BadRequest("Type must be live or reportage");
    }
});

app.MapPost("/events/{id:int}/reschedule", (int id, RescheduleRequest request) =>
{
    bool success = scheduler.RescheduleEvent(id, request.NewStartTime);
    if (success)
    {
        return Results.Ok("Event rescheduled");
    }
    return Results.NotFound("Event not found");
});

app.MapPost("/events/{id:int}/hosts", (int id, HostRequest request) =>
{
    bool success = scheduler.AddHostToEvent(id, request.HostName);
    if (success)
    {
        return Results.Ok("Host added");
    }
    return Results.BadRequest("Could not add host");
});

app.MapPost("/events/{id:int}/hosts/remove", (int id, HostRequest request) =>
{
    bool success = scheduler.RemoveHostFromEvent(id, request.HostName);
    if (success)
    {
        return Results.Ok("Host removed");
    }
    return Results.BadRequest("Could not remove host");
});

app.MapPost("/events/{id:int}/guests", (int id, GuestRequest request) =>
{
    bool success = scheduler.AddGuestToEvent(id, request.GuestName);
    if (success)
    {
        return Results.Ok("Guest added");
    }
    return Results.BadRequest("Could not add guest");
});

app.MapPost("/events/{id:int}/guests/remove", (int id, GuestRequest request) =>
{
    bool success = scheduler.RemoveGuestFromEvent(id, request.GuestName);
    if (success)
    {
        return Results.Ok("Guest removed");
    }
    return Results.BadRequest("Could not remove guest");
});

app.MapPost("/events/{id:int}/delete", (int id) =>
{
    bool success = scheduler.DeleteEvent(id);
    if (success)
    {
        return Results.Ok("Event deleted");
    }
    return Results.NotFound("Event not found");
});

app.MapGet("/", () => "Radio API - Available endpoints: /schedule/today, /schedule/week, /events/{id}, POST /events, POST /events/{id}/reschedule, POST /events/{id}/hosts, etc.");

app.Run();