import { useState } from "react";
import Schedule from "./components/Schedule";
import EventForm from "./components/EventForm";

function App() {
  const [view, setView] = useState("schedule");

  return (
    <div>
      <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
        <div className="container-fluid">
          <span className="navbar-brand mb-0 h1">
            📻 Radio Station Scheduler
          </span>
          <div className="d-flex">
            <button
              className={`btn ${
                view === "schedule" ? "btn-primary" : "btn-outline-primary"
              } me-2`}
              onClick={() => setView("schedule")}
            >
              View Schedule
            </button>
            <button
              className={`btn ${
                view === "add" ? "btn-primary" : "btn-outline-primary"
              }`}
              onClick={() => setView("add")}
            >
              Add Event
            </button>
          </div>
        </div>
      </nav>

      <main>
        {view === "schedule" && <Schedule />}
        {view === "add" && <EventForm />}
      </main>
    </div>
  );
}

export default App;
