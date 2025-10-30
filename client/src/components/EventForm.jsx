import { useState } from "react";
import { createEvent } from "../services/api";

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
  const [submitting, setSubmitting] = useState(false);
  const [message, setMessage] = useState(null);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setSubmitting(true);
    setMessage(null);

    try {
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

      await createEvent(eventData);

      setMessage({ type: "success", text: "Event created successfully!" });

      setFormData({
        title: "",
        startTime: "",
        durationMinutes: 60,
        hosts: "",
        guests: "",
        topic: "",
        reporter: "",
      });
    } catch (error) {
      setMessage({
        type: "error",
        text: "Failed to create event. Make sure the backend is running on http://localhost:5000",
      });
      console.error("Error creating event:", error);
    } finally {
      setSubmitting(false);
    }
  };

  const handleChange = (e) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value,
    });
  };

  return (
    <div className="container mt-4">
      <h2>Add New Event</h2>

      {message && (
        <div
          className={`alert alert-${
            message.type === "success" ? "success" : "danger"
          } mt-3`}
          role="alert"
        >
          {message.text}
        </div>
      )}

      <form onSubmit={handleSubmit} className="mt-4">
        <div className="mb-3">
          <label className="form-label">Event Type</label>
          <div>
            <div className="form-check form-check-inline">
              <input
                className="form-check-input"
                type="radio"
                id="typeLive"
                value="live"
                checked={eventType === "live"}
                onChange={(e) => setEventType(e.target.value)}
              />
              <label className="form-check-label" htmlFor="typeLive">
                🎙️ Live Session
              </label>
            </div>
            <div className="form-check form-check-inline">
              <input
                className="form-check-input"
                type="radio"
                id="typeReportage"
                value="reportage"
                checked={eventType === "reportage"}
                onChange={(e) => setEventType(e.target.value)}
              />
              <label className="form-check-label" htmlFor="typeReportage">
                📰 Reportage
              </label>
            </div>
          </div>
        </div>

        <div className="mb-3">
          <label htmlFor="title" className="form-label">
            Title *
          </label>
          <input
            type="text"
            className="form-control"
            id="title"
            name="title"
            value={formData.title}
            onChange={handleChange}
            required
            placeholder="Event title"
          />
        </div>

        <div className="row">
          <div className="col-md-6 mb-3">
            <label htmlFor="startTime" className="form-label">
              Start Time *
            </label>
            <input
              type="datetime-local"
              className="form-control"
              id="startTime"
              name="startTime"
              value={formData.startTime}
              onChange={handleChange}
              required
            />
          </div>

          <div className="col-md-6 mb-3">
            <label htmlFor="durationMinutes" className="form-label">
              Duration (minutes) *
            </label>
            <input
              type="number"
              className="form-control"
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
            <div className="mb-3">
              <label htmlFor="hosts" className="form-label">
                Hosts * (comma-separated)
              </label>
              <input
                type="text"
                className="form-control"
                id="hosts"
                name="hosts"
                value={formData.hosts}
                onChange={handleChange}
                required
                placeholder="John Host, Jane Co-host"
              />
            </div>

            <div className="mb-3">
              <label htmlFor="guests" className="form-label">
                Guests (comma-separated, optional)
              </label>
              <input
                type="text"
                className="form-control"
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
            <div className="mb-3">
              <label htmlFor="topic" className="form-label">
                Topic *
              </label>
              <input
                type="text"
                className="form-control"
                id="topic"
                name="topic"
                value={formData.topic}
                onChange={handleChange}
                required
                placeholder="News topic"
              />
            </div>

            <div className="mb-3">
              <label htmlFor="reporter" className="form-label">
                Reporter *
              </label>
              <input
                type="text"
                className="form-control"
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

        <button type="submit" className="btn btn-primary" disabled={submitting}>
          {submitting ? "Creating..." : "Create Event"}
        </button>
      </form>
    </div>
  );
}

export default EventForm;
