using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Radio.Models;
using Radio.Models.DTOs;
using Radio.Services;
using Radio.Api;
using Radio.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

builder.Services.AddDbContext<RadioDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=radio.db"));

// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<RadioDbContext>()
.AddDefaultTokenProviders();

// Add JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? "YourSuperSecretKeyForJWTTokenGeneration12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "RadioStationAPI";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "RadioStationClient";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<SchedulerService>();
builder.Services.AddScoped<ContributorRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Radio Station API", 
        Version = "v1",
        Description = "API for managing a radio station's schedule and contributors"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token.",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseCors("AllowReactApp");

app.UseAuthentication();
app.UseAuthorization();

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
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    
    context.Database.EnsureCreated();

    // Create roles if they don't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }
    if (!await roleManager.RoleExistsAsync("Contributor"))
    {
        await roleManager.CreateAsync(new IdentityRole("Contributor"));
    }

    // Create default admin user
    var adminEmail = "admin@radiostation.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(adminUser, "Admin123!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }

    // Create default contributor user
    var contributorRepo = scope.ServiceProvider.GetRequiredService<ContributorRepository>();
    var contributorEmail = "dj@radiostation.com";
    if (await userManager.FindByEmailAsync(contributorEmail) == null)
    {
        var contributorUser = new IdentityUser
        {
            UserName = contributorEmail,
            Email = contributorEmail,
            EmailConfirmed = true
        };
        
        var result = await userManager.CreateAsync(contributorUser, "Dj123456!");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(contributorUser, "Contributor");
            
            // Create contributor profile
            var contributor = new Contributor
            {
                FirstName = "Alex",
                LastName = "Johnson",
                Email = contributorEmail,
                PhoneNumber = "555-0123",
                Address = "123 Radio Street",
                Biography = "Professional DJ with 10 years of experience in radio broadcasting.",
                UserId = contributorUser.Id
            };
            await contributorRepo.CreateAsync(contributor);
        }
    }

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

// Helper method to generate JWT token
string GenerateJwtToken(IdentityUser user, IList<string> roles)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id),
        new Claim(ClaimTypes.Email, user.Email!),
        new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.Now.AddDays(7),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

// ===== AUTHENTICATION ENDPOINTS =====

app.MapPost("/auth/login", async (LoginDto loginDto, UserManager<IdentityUser> userManager, ContributorRepository contributorRepo) =>
{
    var user = await userManager.FindByEmailAsync(loginDto.Email);
    if (user == null || !await userManager.CheckPasswordAsync(user, loginDto.Password))
    {
        return Results.Unauthorized();
    }

    var roles = await userManager.GetRolesAsync(user);
    var token = GenerateJwtToken(user, roles);
    
    var contributor = await contributorRepo.GetByUserIdAsync(user.Id);

    return Results.Ok(new AuthResponseDto
    {
        Token = token,
        Email = user.Email!,
        UserId = user.Id,
        Roles = roles.ToList(),
        ContributorId = contributor?.Id
    });
})
.WithName("Login")
.WithDescription("Authenticate user and get JWT token");

app.MapPost("/auth/register", async (RegisterDto registerDto, UserManager<IdentityUser> userManager, ContributorRepository contributorRepo) =>
{
    var existingUser = await userManager.FindByEmailAsync(registerDto.Email);
    if (existingUser != null)
    {
        return Results.BadRequest(new { error = "Email already registered" });
    }

    var user = new IdentityUser
    {
        UserName = registerDto.Email,
        Email = registerDto.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, registerDto.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors });
    }

    await userManager.AddToRoleAsync(user, "Contributor");

    var contributor = new Contributor
    {
        UserId = user.Id,
        Email = registerDto.Email,
        FirstName = registerDto.FirstName,
        LastName = registerDto.LastName,
        PhoneNumber = registerDto.PhoneNumber,
        Address = registerDto.Address,
        PhotoUrl = registerDto.PhotoUrl,
        Biography = registerDto.Biography
    };

    await contributorRepo.CreateAsync(contributor);

    var roles = await userManager.GetRolesAsync(user);
    var token = GenerateJwtToken(user, roles);

    return Results.Ok(new AuthResponseDto
    {
        Token = token,
        Email = user.Email,
        UserId = user.Id,
        Roles = roles.ToList(),
        ContributorId = contributor.Id
    });
})
.WithName("Register")
.WithDescription("Register new contributor");

// ===== CONTRIBUTOR ENDPOINTS =====

// Get all contributors (Admin only)
app.MapGet("/contributors", async (ContributorRepository repo) =>
{
    var contributors = await repo.GetAllAsync();
    var dtos = contributors.Select(c => new ContributorDto
    {
        Id = c.Id,
        FirstName = c.FirstName,
        LastName = c.LastName,
        FullName = c.FullName,
        Email = c.Email,
        PhoneNumber = c.PhoneNumber,
        Address = c.Address,
        PhotoUrl = c.PhotoUrl,
        Biography = c.Biography,
        UserId = c.UserId,
        CreatedAt = c.CreatedAt,
        UpdatedAt = c.UpdatedAt
    }).ToList();
    
    return Results.Ok(dtos);
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("GetAllContributors");

// Get contributor by ID (Admin or own contributor)
app.MapGet("/contributors/{id:int}", async (int id, HttpContext context, ContributorRepository repo) =>
{
    var contributor = await repo.GetByIdAsync(id);
    if (contributor == null)
    {
        return Results.NotFound();
    }

    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var isAdmin = context.User.IsInRole("Admin");
    
    if (!isAdmin && contributor.UserId != userId)
    {
        return Results.Forbid();
    }

    var dto = new ContributorDetailDto
    {
        Id = contributor.Id,
        FirstName = contributor.FirstName,
        LastName = contributor.LastName,
        FullName = contributor.FullName,
        Email = contributor.Email,
        PhoneNumber = contributor.PhoneNumber,
        Address = contributor.Address,
        PhotoUrl = contributor.PhotoUrl,
        Biography = contributor.Biography,
        UserId = contributor.UserId,
        CreatedAt = contributor.CreatedAt,
        UpdatedAt = contributor.UpdatedAt,
        PaymentHistory = contributor.PaymentHistory.Select(p => new PaymentRecordDto
        {
            Id = p.Id,
            Year = p.Year,
            Month = p.Month,
            TotalHours = p.TotalHours,
            TotalEvents = p.TotalEvents,
            SubtotalAmount = p.SubtotalAmount,
            VatAmount = p.VatAmount,
            TotalAmount = p.TotalAmount,
            GeneratedAt = p.GeneratedAt,
            IsPaid = p.IsPaid,
            PaidAt = p.PaidAt
        }).ToList(),
        RecentAssignments = contributor.Assignments.OrderByDescending(a => a.AssignedAt).Take(10).Select(a => new AssignmentDto
        {
            Id = a.Id,
            ContributorId = a.ContributorId,
            ContributorName = contributor.FullName,
            ScheduledContentId = a.ScheduledContentId,
            ContentTitle = a.ScheduledContent?.Title ?? "",
            ContentStartTime = a.ScheduledContent?.StartTime ?? DateTime.MinValue,
            ContentDuration = a.ScheduledContent?.Duration ?? TimeSpan.Zero,
            Role = a.Role.ToString(),
            AssignedAt = a.AssignedAt
        }).ToList()
    };

    return Results.Ok(dto);
})
.RequireAuthorization()
.WithName("GetContributorById");

// Get current contributor profile
app.MapGet("/contributors/me", async (HttpContext context, ContributorRepository repo) =>
{
    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    if (userId == null)
    {
        return Results.Unauthorized();
    }

    var contributor = await repo.GetByUserIdAsync(userId);
    if (contributor == null)
    {
        return Results.NotFound(new { error = "Contributor profile not found" });
    }

    var dto = new ContributorDetailDto
    {
        Id = contributor.Id,
        FirstName = contributor.FirstName,
        LastName = contributor.LastName,
        FullName = contributor.FullName,
        Email = contributor.Email,
        PhoneNumber = contributor.PhoneNumber,
        Address = contributor.Address,
        PhotoUrl = contributor.PhotoUrl,
        Biography = contributor.Biography,
        UserId = contributor.UserId,
        CreatedAt = contributor.CreatedAt,
        UpdatedAt = contributor.UpdatedAt,
        PaymentHistory = contributor.PaymentHistory.OrderByDescending(p => p.Year).ThenByDescending(p => p.Month).Select(p => new PaymentRecordDto
        {
            Id = p.Id,
            Year = p.Year,
            Month = p.Month,
            TotalHours = p.TotalHours,
            TotalEvents = p.TotalEvents,
            SubtotalAmount = p.SubtotalAmount,
            VatAmount = p.VatAmount,
            TotalAmount = p.TotalAmount,
            GeneratedAt = p.GeneratedAt,
            IsPaid = p.IsPaid,
            PaidAt = p.PaidAt
        }).ToList(),
        RecentAssignments = contributor.Assignments.OrderByDescending(a => a.AssignedAt).Take(20).Select(a => new AssignmentDto
        {
            Id = a.Id,
            ContributorId = a.ContributorId,
            ContributorName = contributor.FullName,
            ScheduledContentId = a.ScheduledContentId,
            ContentTitle = a.ScheduledContent?.Title ?? "",
            ContentStartTime = a.ScheduledContent?.StartTime ?? DateTime.MinValue,
            ContentDuration = a.ScheduledContent?.Duration ?? TimeSpan.Zero,
            Role = a.Role.ToString(),
            AssignedAt = a.AssignedAt
        }).ToList()
    };

    return Results.Ok(dto);
})
.RequireAuthorization(policy => policy.RequireRole("Contributor"))
.WithName("GetMyProfile");

// Create contributor (Admin only)
app.MapPost("/contributors", async (CreateContributorDto dto, UserManager<IdentityUser> userManager, ContributorRepository repo) =>
{
    var existingUser = await userManager.FindByEmailAsync(dto.Email);
    if (existingUser != null)
    {
        return Results.BadRequest(new { error = "Email already registered" });
    }

    var user = new IdentityUser
    {
        UserName = dto.Email,
        Email = dto.Email,
        EmailConfirmed = true
    };

    var result = await userManager.CreateAsync(user, dto.Password);
    if (!result.Succeeded)
    {
        return Results.BadRequest(new { errors = result.Errors });
    }

    await userManager.AddToRoleAsync(user, "Contributor");

    var contributor = new Contributor
    {
        UserId = user.Id,
        Email = dto.Email,
        FirstName = dto.FirstName,
        LastName = dto.LastName,
        PhoneNumber = dto.PhoneNumber,
        Address = dto.Address,
        PhotoUrl = dto.PhotoUrl,
        Biography = dto.Biography
    };

    await repo.CreateAsync(contributor);

    return Results.Created($"/contributors/{contributor.Id}", new ContributorDto
    {
        Id = contributor.Id,
        FirstName = contributor.FirstName,
        LastName = contributor.LastName,
        FullName = contributor.FullName,
        Email = contributor.Email,
        PhoneNumber = contributor.PhoneNumber,
        Address = contributor.Address,
        PhotoUrl = contributor.PhotoUrl,
        Biography = contributor.Biography,
        UserId = contributor.UserId,
        CreatedAt = contributor.CreatedAt
    });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("CreateContributor");

// Update contributor (Admin or own contributor)
app.MapPut("/contributors/{id:int}", async (int id, UpdateContributorDto dto, HttpContext context, ContributorRepository repo) =>
{
    var contributor = await repo.GetByIdAsync(id);
    if (contributor == null)
    {
        return Results.NotFound();
    }

    var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var isAdmin = context.User.IsInRole("Admin");
    
    if (!isAdmin && contributor.UserId != userId)
    {
        return Results.Forbid();
    }

    if (!string.IsNullOrEmpty(dto.FirstName)) contributor.FirstName = dto.FirstName;
    if (!string.IsNullOrEmpty(dto.LastName)) contributor.LastName = dto.LastName;
    if (!string.IsNullOrEmpty(dto.PhoneNumber)) contributor.PhoneNumber = dto.PhoneNumber;
    if (!string.IsNullOrEmpty(dto.Address)) contributor.Address = dto.Address;
    if (dto.PhotoUrl != null) contributor.PhotoUrl = dto.PhotoUrl;
    if (dto.Biography != null) contributor.Biography = dto.Biography;

    await repo.UpdateAsync(contributor);

    return Results.Ok(new ContributorDto
    {
        Id = contributor.Id,
        FirstName = contributor.FirstName,
        LastName = contributor.LastName,
        FullName = contributor.FullName,
        Email = contributor.Email,
        PhoneNumber = contributor.PhoneNumber,
        Address = contributor.Address,
        PhotoUrl = contributor.PhotoUrl,
        Biography = contributor.Biography,
        UserId = contributor.UserId,
        CreatedAt = contributor.CreatedAt,
        UpdatedAt = contributor.UpdatedAt
    });
})
.RequireAuthorization()
.WithName("UpdateContributor");

// Delete contributor (Admin only)
app.MapDelete("/contributors/{id:int}", async (int id, UserManager<IdentityUser> userManager, ContributorRepository repo) =>
{
    var contributor = await repo.GetByIdAsync(id);
    if (contributor == null)
    {
        return Results.NotFound();
    }

    var user = await userManager.FindByIdAsync(contributor.UserId);
    if (user != null)
    {
        await userManager.DeleteAsync(user);
    }

    await repo.DeleteAsync(id);
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("DeleteContributor");

// Generate payment for contributor (Admin only)
app.MapPost("/contributors/{id:int}/payments/{year:int}/{month:int}", async (int id, int year, int month, ContributorRepository repo) =>
{
    var contributor = await repo.GetByIdAsync(id);
    if (contributor == null)
    {
        return Results.NotFound();
    }

    var payment = await repo.GenerateMonthlyPaymentAsync(id, year, month);
    
    return Results.Ok(new PaymentRecordDto
    {
        Id = payment.Id,
        Year = payment.Year,
        Month = payment.Month,
        TotalHours = payment.TotalHours,
        TotalEvents = payment.TotalEvents,
        SubtotalAmount = payment.SubtotalAmount,
        VatAmount = payment.VatAmount,
        TotalAmount = payment.TotalAmount,
        GeneratedAt = payment.GeneratedAt,
        IsPaid = payment.IsPaid,
        PaidAt = payment.PaidAt
    });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("GeneratePayment");

// Mark payment as paid (Admin only)
app.MapPost("/payments/{id:int}/mark-paid", async (int id, ContributorRepository repo) =>
{
    await repo.MarkPaymentAsPaidAsync(id);
    return Results.Ok(new { message = "Payment marked as paid" });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("MarkPaymentPaid");

// Assign contributor to content (Admin only)
app.MapPost("/assignments", async (CreateAssignmentDto dto, ContributorRepository repo) =>
{
    if (!Enum.TryParse<ContributorRole>(dto.Role, true, out var role))
    {
        return Results.BadRequest(new { error = "Invalid role. Must be Host, CoHost, Guest, or Reporter" });
    }

    var assignment = await repo.AssignToContentAsync(dto.ContributorId, dto.ScheduledContentId, role);
    
    return Results.Created($"/assignments/{assignment.Id}", new AssignmentDto
    {
        Id = assignment.Id,
        ContributorId = assignment.ContributorId,
        ScheduledContentId = assignment.ScheduledContentId,
        Role = assignment.Role.ToString(),
        AssignedAt = assignment.AssignedAt
    });
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("CreateAssignment");

// Remove assignment (Admin only)
app.MapDelete("/assignments/{id:int}", async (int id, ContributorRepository repo) =>
{
    await repo.RemoveAssignmentAsync(id);
    return Results.NoContent();
})
.RequireAuthorization(policy => policy.RequireRole("Admin"))
.WithName("DeleteAssignment");

// ===== SCHEDULE ENDPOINTS =====


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

// In-memory conversation storage
var conversations = new Dictionary<string, List<ChatMessage>>();

// AI Music Suggestions Endpoint
app.MapGet("/ai/suggest", async (string prompt, string? conversationId, IConfiguration config) =>
{
    var token = config["AI:GitHubToken"];
    
    if (string.IsNullOrEmpty(token) || token == "YOUR_GITHUB_TOKEN_HERE")
    {
        return Results.BadRequest("GitHub token not configured in appsettings.json");
    }

    var client = new AzureOpenAIClient(
        new Uri("https://models.inference.ai.azure.com"),
        new System.ClientModel.ApiKeyCredential(token));
    
    var chatClient = client.GetChatClient("gpt-4o-mini");

    var systemPrompt = """You are a friendly radio DJ assistant. When given a prompt about music, return a JSON object with two fields: "message" (a brief, enthusiastic 1-2 sentence intro about the music selection) and "songs" (an array of 10 songs, each with "title" and "artist" fields). No markdown, no code blocks - just raw JSON starting with { and ending with }. Remember previous suggestions in this conversation so you can respond to follow-ups like "more like this", "less ballads", "make it more upbeat", etc.""";

    // Get or create conversation history
    var isNewConversation = string.IsNullOrEmpty(conversationId) || !conversations.ContainsKey(conversationId);
    if (isNewConversation)
    {
        conversationId = Guid.NewGuid().ToString("N")[..8];
        conversations[conversationId] = new List<ChatMessage> { new SystemChatMessage(systemPrompt) };
    }
    
    var history = conversations[conversationId];
    history.Add(new UserChatMessage(prompt));

    var completion = await chatClient.CompleteChatAsync(history);

    var response = completion.Value.Content[0].Text;
    
    // Store assistant response in history
    history.Add(new AssistantChatMessage(response));
    
    // Clean up any markdown formatting
    response = response.Trim();
    if (response.StartsWith("```json"))
        response = response.Substring(7);
    else if (response.StartsWith("```"))
        response = response.Substring(3);
    if (response.EndsWith("```"))
        response = response.Substring(0, response.Length - 3);
    response = response.Trim();
    
    var result = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(response);
    return Results.Ok(new { conversationId, data = result });
})
.WithName("SuggestMusic")
.WithDescription("Get AI-generated music suggestions. Use conversationId for follow-ups like 'more like this'")
.WithTags("AI");

app.Run();