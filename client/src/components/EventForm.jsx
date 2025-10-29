import { useState } from "react";
import "./EventForm.css";

function EventForm() {
  const [eventType, setEventType] = useState("live");
  const [formData, setFormData] = useState({
    title: "",
    startTime: "",
    durationMinutes: 60,
    hosts: "",
    guests: "",
    topic: "",
    reporter: "",
  });

  const handleSubmit = (e) => {
    e.preventDefault();

    // For now, just log the data (will connect to API later)
    const eventData = {
      type: eventType,
      title: formData.title,
      startTime: formData.startTime,
      durationMinutes: parseInt(formData.durationMinutes),
    };

    if (eventType === "live") {
      eventData.hosts = formData.hosts.split(",").map((h) => h.trim());
      eventData.guests = formData.guests
        ? formData.guests.split(",").map((g) => g.trim())
        : [];
    } else if (eventType === "reportage") {
      eventData.topic = formData.topic;
      eventData.reporter = formData.reporter;
    }

    console.log("Event to create:", eventData);
    alert("Event created! (Check console for data)");

    // Reset form
    setFormData({
      title: "",
      startTime: "",
      durationMinutes: 60,
      hosts: "",
      guests: "",
      topic: "",
      reporter: "",
    });
  };

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  return (
    <div className="event-form">
      <h2>Add New Event</h2>

      <form onSubmit={handleSubmit}>
        <div className="form-group">
          <label>Event Type</label>
          <div className="radio-group">
            <label>
              <input
                type="radio"
                value="live"
                checked={eventType === "live"}
                onChange={(e) => setEventType(e.target.value)}
              />
              🎙️ Live Session
            </label>
            <label>
              <input
                type="radio"
                value="reportage"
                checked={eventType === "reportage"}
                onChange={(e) => setEventType(e.target.value)}
              />
              📰 Reportage
            </label>
          </div>
        </div>

        <div className="form-group">
          <label htmlFor="title">Title *</label>
          <input
            type="text"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            required
            placeholder="Event title"
          />
        </div>

        <div className="form-row">
          <div className="form-group">
            <label htmlFor="startTime">Start Time *</label>
            <input
              type="datetime-local"
              id="startTime"
              name="startTime"
              value={formData.startTime}
              onChange={handleChange}
              required
            />
          </div>

          <div className="form-group">
            <label htmlFor="durationMinutes">Duration (minutes) *</label>
            <input
              type="number"
              id="durationMinutes"
              name="durationMinutes"
              value={formData.durationMinutes}
              onChange={handleChange}
              min="1"
              required
            />
          </div>
        </div>

        {eventType === "live" && (
          <>
            <div className="form-group">
              <label htmlFor="hosts">Hosts * (comma-separated)</label>
              <input
                type="text"
                id="hosts"
                name="hosts"
                value={formData.hosts}
                onChange={handleChange}
                required
                placeholder="John Host, Jane Co-host"
              />
            </div>

            <div className="form-group">
              <label htmlFor="guests">Guests (comma-separated, optional)</label>
              <input
                type="text"
                id="guests"
                name="guests"
                value={formData.guests}
                onChange={handleChange}
                placeholder="Guest 1, Guest 2"
              />
            </div>
          </>
        )}

        {eventType === "reportage" && (
          <>
            <div className="form-group">
              <label htmlFor="topic">Topic *</label>
              <input
                type="text"
                id="topic"
                name="topic"
                value={formData.topic}
                onChange={handleChange}
                required
                placeholder="News topic"
              />
            </div>

            <div className="form-group">
              <label htmlFor="reporter">Reporter *</label>
              <input
                type="text"
                id="reporter"
                name="reporter"
                value={formData.reporter}
                onChange={handleChange}
                required
                placeholder="Reporter name"
              />
            </div>
          </>
        )}

        <button type="submit" className="submit-button">
          Create Event
        </button>
      </form>
    </div>
  );
}

export default EventForm;
