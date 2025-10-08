# 📻 Radio Station API

A RESTful API for managing a radio station's schedule, built with .NET 9.0 and Entity Framework Core.

## 📋 Features

- **Schedule Management**: Create and manage daily and weekly radio schedules
- **Event Types**: Support for live sessions, reportages, and music content
- **Database Persistence**: SQLite database with Entity Framework Core
- **API Documentation**: Interactive Swagger UI for easy testing
- **Studio Assignment**: Automatic studio allocation based on hosts and guests
- **Music Fill**: Automatically fills empty time slots with music

## 🚀 Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- Any IDE (Visual Studio, VS Code, or Rider)

### Installation

1. Clone the repository:

```bash
git clone https://github.com/fraanzen/Radio.git
cd Radio
```

2. Restore dependencies:

```bash
dotnet restore
```

3. Run the application:

```bash
dotnet run
```

The API will be available at `http://localhost:5000`

## 📖 API Documentation

Once the application is running, visit the Swagger UI:

```
http://localhost:5000/swagger
```

### Main Endpoints

| Method | Endpoint                  | Description               |
| ------ | ------------------------- | ------------------------- |
| GET    | `/schedule/today`         | Get today's schedule      |
| GET    | `/schedule/week`          | Get 7-day schedule        |
| GET    | `/events/{id}`            | Get specific event by ID  |
| POST   | `/events`                 | Create a new event        |
| POST   | `/events/{id}/reschedule` | Change event start time   |
| POST   | `/events/{id}/hosts`      | Add host to live session  |
| POST   | `/events/{id}/guests`     | Add guest to live session |
| POST   | `/events/{id}/delete`     | Delete an event           |

## 🎯 Usage Examples

### Create a Live Session

```json
POST /events
{
  "type": "live",
  "startTime": "2025-10-09T14:00:00",
  "durationMinutes": 60,
  "title": "Afternoon Drive Show",
  "hosts": ["John Host", "Jane Co-host"],
  "guests": []
}
```

### Create a Reportage

```json
POST /events
{
  "type": "reportage",
  "startTime": "2025-10-09T18:00:00",
  "durationMinutes": 15,
  "title": "Evening News",
  "topic": "Local Events",
  "reporter": "Mike Reporter"
}
```

## 🗄️ Database

The application uses SQLite with Entity Framework Core for data persistence.

### Tables

- **DailySchedules**: Stores daily schedule information
- **ScheduledContents**: Stores all events (live sessions, reportages, music) using Table-Per-Hierarchy inheritance

### Migrations

The database is automatically created on first run. To recreate the database:

```bash
# Delete existing database
Remove-Item radio.db

# Run the application (database will be recreated)
dotnet run
```

## 🏗️ Project Structure

```
Radio/
├── Data/
│   └── RadioDbContext.cs          # Database context
├── Mapping/
│   ├── EventMapper.cs             # Event mapping logic
│   └── EventRequestMapper.cs      # Request mapping logic
├── Migrations/                     # EF Core migrations
├── ApiModels.cs                    # API request/response models
├── Program.cs                      # Application entry point
├── ScheduleModels.cs               # Domain models
└── SchedulerService.cs             # Business logic
```

## 🛠️ Technologies Used

- **.NET 9.0**: Modern web framework
- **Entity Framework Core 9.0**: ORM for database operations
- **SQLite**: Lightweight file-based database
- **Swashbuckle**: API documentation with Swagger
- **Minimal APIs**: Clean and simple endpoint definitions

## 📝 Event Types

### Live Session

- Radio shows with hosts and optional guests
- Automatic studio assignment (Studio 1 or Studio 2)
- Support for multiple hosts and guests

### Reportage

- News segments or reports
- Includes topic and reporter information

### Music Content

- Automatically fills empty time slots
- Mixed genre playlist

## 👥 Authors

- **Mattias**

## 📄 License

This project is created for educational purposes.
