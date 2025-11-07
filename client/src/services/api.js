const API_BASE_URL = "http://localhost:5000";

// Get today's schedule
export async function getTodaySchedule() {
  const response = await fetch(`${API_BASE_URL}/schedule/today`);
  if (!response.ok) {
    throw new Error("Failed to fetch schedule");
  }
  const data = await response.json();
  return data.events;
}

// Get schedule for a specific date
export async function getScheduleByDate(date) {
  const response = await fetch(`${API_BASE_URL}/schedule/today`);
  if (!response.ok) {
    throw new Error("Failed to fetch schedule");
  }
  const data = await response.json();
  return data.events;
}

// Create a new event (live session or reportage)
export async function createEvent(eventData) {
  const response = await fetch(`${API_BASE_URL}/events`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
    },
    body: JSON.stringify(eventData),
  });

  if (!response.ok) {
    throw new Error("Failed to create event");
  }

  return response;
}

// Get event by ID
export async function getEventById(id) {
  const response = await fetch(`${API_BASE_URL}/events/${id}`);
  if (!response.ok) {
    throw new Error("Failed to fetch event");
  }
  return response.json();
}

// Delete event
export async function deleteEvent(id) {
  const response = await fetch(`${API_BASE_URL}/events/${id}/delete`, {
    method: "POST",
  });

  if (!response.ok) {
    throw new Error("Failed to delete event");
  }

  return response;
}
