import { useState } from "react";
import "./Schedule.css";

// Hardcoded data matching backend structure
const mockEvents = [
  {
    id: 1,
    type: "live",
    title: "Morning Show",
    startTime: "2025-10-28T07:00:00",
    endTime: "2025-10-28T09:00:00",
    durationMinutes: 120,
    hosts: ["Mike Host", "Sarah Co-host"],
    guests: [],
    studio: "Studio 2",
  },
  {
    id: 2,
    type: "reportage",
    title: "Morning News",
    startTime: "2025-10-28T09:00:00",
    endTime: "2025-10-28T09:30:00",
    durationMinutes: 30,
    topic: "Local News",
    reporter: "John Reporter",
  },
  {
    id: 3,
    type: "music",
    title: "Music Playlist",
    startTime: "2025-10-28T09:30:00",
    endTime: "2025-10-28T12:00:00",
    durationMinutes: 150,
    genre: "Mixed",
  },
  {
    id: 4,
    type: "live",
    title: "Lunch Talk",
    startTime: "2025-10-28T12:00:00",
    endTime: "2025-10-28T13:00:00",
    durationMinutes: 60,
    hosts: ["Alex Host"],
    guests: ["Chef Maria"],
    studio: "Studio 2",
  },
  {
    id: 5,
    type: "music",
    title: "Music Playlist",
    startTime: "2025-10-28T13:00:00",
    endTime: "2025-10-28T16:00:00",
    durationMinutes: 180,
    genre: "Mixed",
  },
  {
    id: 6,
    type: "live",
    title: "Afternoon Drive",
    startTime: "2025-10-28T16:00:00",
    endTime: "2025-10-28T18:00:00",
    durationMinutes: 120,
    hosts: ["DJ Rocky"],
    guests: [],
    studio: "Studio 1",
  },
];

function Schedule() {
  const [events] = useState(mockEvents);

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString("en-US", {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    });
  };

  const getEventIcon = (type) => {
    switch (type) {
      case "live":
        return "🎙️";
      case "reportage":
        return "📰";
      case "music":
        return "🎵";
      default:
        return "📻";
    }
  };

  return (
    <div className="schedule">
      <h2>Today's Schedule</h2>
      <p className="schedule-date">
        {new Date().toLocaleDateString("en-US", {
          weekday: "long",
          year: "numeric",
          month: "long",
          day: "numeric",
        })}
      </p>

      <div className="timeline">
        {events.map((event) => (
          <div key={event.id} className={`event-card ${event.type}`}>
            <div className="event-time">
              <span className="event-icon">{getEventIcon(event.type)}</span>
              <span className="time-range">
                {formatTime(event.startTime)} - {formatTime(event.endTime)}
              </span>
              <span className="duration">{event.durationMinutes} min</span>
            </div>

            <div className="event-content">
              <h3>{event.title}</h3>

              {event.type === "live" && (
                <div className="event-details">
                  <p>
                    <strong>Hosts:</strong> {event.hosts.join(", ")}
                  </p>
                  {event.guests && event.guests.length > 0 && (
                    <p>
                      <strong>Guests:</strong> {event.guests.join(", ")}
                    </p>
                  )}
                  <p>
                    <strong>Studio:</strong> {event.studio}
                  </p>
                </div>
              )}

              {event.type === "reportage" && (
                <div className="event-details">
                  <p>
                    <strong>Topic:</strong> {event.topic}
                  </p>
                  <p>
                    <strong>Reporter:</strong> {event.reporter}
                  </p>
                </div>
              )}

              {event.type === "music" && (
                <div className="event-details">
                  <p>
                    <strong>Genre:</strong> {event.genre}
                  </p>
                </div>
              )}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}

export default Schedule;
