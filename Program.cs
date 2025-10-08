using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Radio.Models;
using Radio.Services;
using Radio.Api;
using Radio.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<RadioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=radio.db"));

builder.Services.AddScoped<SchedulerService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Radio Station API", 
        Version = "v1",
        Description = "API for managing a radio station's schedule"
    });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Radio Station API V1");
    c.RoutePrefix = "swagger";
});

// Initialize database and seed data
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<RadioDbContext>();
    context.Database.EnsureCreated();

    if (!context.ScheduledContents.Any())
    {
        var scheduler = scope.ServiceProvider.GetRequiredService<SchedulerService>();
        scheduler.FillWithMusic();
    }
}

// Converts domain model to API response model
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

app.MapGet("/schedule/today", (SchedulerService scheduler) =>
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
    return Results.Ok(response);
})
.WithName("GetTodaySchedule")
.WithDescription("Gets the schedule for today, including all live sessions, reportages, and music content");

app.MapGet("/schedule/week", (SchedulerService scheduler) =>
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

app.MapGet("/events/{id:int}", (int id, SchedulerService scheduler) =>
{
    var eventItem = scheduler.GetEventById(id);
    if (eventItem == null)
    {
        return Results.NotFound("Event not found");
    }
    
    return Results.Ok(ConvertToEventResponse(eventItem));
});

app.MapPost("/events", (EventRequest request, SchedulerService scheduler) =>
{
    if (string.IsNullOrEmpty(request.Type))
    {
        return Results.BadRequest("Event type is required");
    }

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
            request.Guests != null && request.Guests.Count > 0 ? request.Guests : new List<string>()
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

app.MapPost("/events/{id:int}/reschedule", (int id, RescheduleRequest request, SchedulerService scheduler) =>
{
    bool success = scheduler.RescheduleEvent(id, request.NewStartTime);
    if (success)
    {
        return Results.Ok("Event rescheduled");
    }
    return Results.NotFound("Event not found");
});

app.MapPost("/events/{id:int}/hosts", (int id, HostRequest request, SchedulerService scheduler) =>
{
    bool success = scheduler.AddHostToEvent(id, request.HostName);
    if (success)
    {
        return Results.Ok("Host added");
    }
    return Results.BadRequest("Could not add host");
});

app.MapPost("/events/{id:int}/hosts/remove", (int id, HostRequest request, SchedulerService scheduler) =>
{
    bool success = scheduler.RemoveHostFromEvent(id, request.HostName);
    if (success)
    {
        return Results.Ok("Host removed");
    }
    return Results.BadRequest("Could not remove host");
});

app.MapPost("/events/{id:int}/guests", (int id, GuestRequest request, SchedulerService scheduler) =>
{
    bool success = scheduler.AddGuestToEvent(id, request.GuestName);
    if (success)
    {
        return Results.Ok("Guest added");
    }
    return Results.BadRequest("Could not add guest");
});

app.MapPost("/events/{id:int}/guests/remove", (int id, GuestRequest request, SchedulerService scheduler) =>
{
    bool success = scheduler.RemoveGuestFromEvent(id, request.GuestName);
    if (success)
    {
        return Results.Ok("Guest removed");
    }
    return Results.BadRequest("Could not remove guest");
});

app.MapPost("/events/{id:int}/delete", (int id, SchedulerService scheduler) =>
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