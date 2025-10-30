import { useState, useEffect } from "react";
import { getTodaySchedule } from "../services/api";

function Schedule() {
  const [events, setEvents] = useState([]);
  const [currentDate, setCurrentDate] = useState(new Date());
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    async function fetchSchedule() {
      try {
        setLoading(true);
        setError(null);
        const scheduleData = await getTodaySchedule();
        setEvents(scheduleData);
      } catch (err) {
        setError("Failed to load schedule. Make sure the backend is running.");
        console.error("Error fetching schedule:", err);
      } finally {
        setLoading(false);
      }
    }

    fetchSchedule();
  }, [currentDate]);

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

  const buildHourlySchedule = () => {
    const hourlySchedule = [];

    for (let hour = 0; hour < 24; hour++) {
      const hourStart = new Date(currentDate);
      hourStart.setHours(hour, 0, 0, 0);
      const hourEnd = new Date(currentDate);
      hourEnd.setHours(hour, 59, 59, 999);

      const eventsInHour = events.filter((event) => {
        const eventStart = new Date(event.startTime);
        const eventEnd = new Date(event.endTime);
        return eventStart <= hourEnd && eventEnd >= hourStart;
      });

      hourlySchedule.push({
        hour,
        hourLabel: `${hour.toString().padStart(2, "0")}:00`,
        events: eventsInHour,
      });
    }

    return hourlySchedule;
  };

  const hourlySchedule = buildHourlySchedule();

  if (loading) {
    return (
      <div className="container mt-4">
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="mt-2">Loading schedule...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-4">
        <div className="alert alert-danger" role="alert">
          <h4 className="alert-heading">Error</h4>
          <p>{error}</p>
        </div>
      </div>
    );
  }

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
        {currentDate.toDateString() !== new Date().toDateString() && (
          <span className="badge bg-warning text-dark ms-2">
            Showing today's schedule (date-specific schedules not yet
            implemented)
          </span>
        )}
      </p>

      <table className="table table-bordered table-hover">
        <thead className="table-dark">
          <tr>
            <th scope="col" style={{ width: "80px" }}>
              Hour
            </th>
            <th scope="col" style={{ width: "100px" }}>
              Type
            </th>
            <th scope="col">Program</th>
            <th scope="col">Details</th>
          </tr>
        </thead>
        <tbody>
          {hourlySchedule.map((hourSlot) => {
            const mainEvent = hourSlot.events[0]; // Get the primary event for this hour

            if (!mainEvent) {
              return (
                <tr key={hourSlot.hour} className="table-secondary">
                  <td className="align-middle">
                    <strong>{hourSlot.hourLabel}</strong>
                  </td>
                  <td className="align-middle">
                    <span className="badge bg-light text-dark">—</span>
                  </td>
                  <td className="align-middle text-muted">
                    <em>No scheduled content</em>
                  </td>
                  <td className="align-middle"></td>
                </tr>
              );
            }

            return (
              <tr key={hourSlot.hour}>
                <td className="align-middle">
                  <strong>{hourSlot.hourLabel}</strong>
                </td>
                <td className="align-middle">
                  {getEventBadge(mainEvent.type)}
                </td>
                <td className="align-middle">
                  <strong>{mainEvent.title}</strong>
                  <br />
                  <small className="text-muted">
                    {formatTime(mainEvent.startTime)} -{" "}
                    {formatTime(mainEvent.endTime)} ({mainEvent.durationMinutes}{" "}
                    min)
                  </small>
                </td>
                <td className="align-middle">
                  <small>{getEventDetails(mainEvent)}</small>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
}

export default Schedule;
