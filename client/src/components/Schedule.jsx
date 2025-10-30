import { useState } from "react";

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
  const [currentDate, setCurrentDate] = useState(new Date());

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString("en-US", {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    });
  };

  const goToPreviousDay = () => {
    const newDate = new Date(currentDate);
    newDate.setDate(newDate.getDate() - 1);
    setCurrentDate(newDate);
  };

  const goToNextDay = () => {
    const newDate = new Date(currentDate);
    newDate.setDate(newDate.getDate() + 1);
    setCurrentDate(newDate);
  };

  const goToToday = () => {
    setCurrentDate(new Date());
  };

  const getEventBadge = (type) => {
    switch (type) {
      case "live":
        return <span className="badge bg-danger">🎙️ Live</span>;
      case "reportage":
        return <span className="badge bg-info">📰 Reportage</span>;
      case "music":
        return <span className="badge bg-success">🎵 Music</span>;
      default:
        return <span className="badge bg-secondary">📻 Event</span>;
    }
  };

  const getEventDetails = (event) => {
    if (event.type === "live") {
      return (
        <>
          <div>
            <strong>Hosts:</strong> {event.hosts.join(", ")}
          </div>
          {event.guests && event.guests.length > 0 && (
            <div>
              <strong>Guests:</strong> {event.guests.join(", ")}
            </div>
          )}
          <div>
            <strong>Studio:</strong> {event.studio}
          </div>
        </>
      );
    } else if (event.type === "reportage") {
      return (
        <>
          <div>
            <strong>Topic:</strong> {event.topic}
          </div>
          <div>
            <strong>Reporter:</strong> {event.reporter}
          </div>
        </>
      );
    } else if (event.type === "music") {
      return (
        <div>
          <strong>Genre:</strong> {event.genre}
        </div>
      );
    }
    return null;
  };

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2>Schedule</h2>
        <div className="btn-group" role="group">
          <button
            className="btn btn-outline-secondary"
            onClick={goToPreviousDay}
            title="Previous Day"
          >
            ← Previous
          </button>
          <button
            className="btn btn-outline-primary"
            onClick={goToToday}
            title="Go to Today"
          >
            Today
          </button>
          <button
            className="btn btn-outline-secondary"
            onClick={goToNextDay}
            title="Next Day"
          >
            Next →
          </button>
        </div>
      </div>

      <p className="text-muted">
        {currentDate.toLocaleDateString("en-US", {
          weekday: "long",
          year: "numeric",
          month: "long",
          day: "numeric",
        })}
      </p>

      <table className="table table-striped table-hover">
        <thead className="table-dark">
          <tr>
            <th scope="col">Time</th>
            <th scope="col">Type</th>
            <th scope="col">Title</th>
            <th scope="col">Duration</th>
            <th scope="col">Details</th>
          </tr>
        </thead>
        <tbody>
          {events.map((event) => (
            <tr key={event.id}>
              <td className="align-middle">
                {formatTime(event.startTime)} - {formatTime(event.endTime)}
              </td>
              <td className="align-middle">{getEventBadge(event.type)}</td>
              <td className="align-middle">
                <strong>{event.title}</strong>
              </td>
              <td className="align-middle">{event.durationMinutes} min</td>
              <td className="align-middle">
                <small>{getEventDetails(event)}</small>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export default Schedule;
