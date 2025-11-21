import { useState } from "react";
import Home from "./components/Home";
import Schedule from "./components/Schedule";
import EventForm from "./components/EventForm";

function App() {
  const [view, setView] = useState("home");

  return (
    <div>
      <nav className="navbar navbar-expand-lg navbar-dark bg-dark">
        <div className="container-fluid">
          <span
            className="navbar-brand mb-0 h1"
            style={{ cursor: "pointer" }}
            onClick={() => setView("home")}
          >
            Radio Station FM
          </span>
          <div className="d-flex">
            <button
              className={`btn ${
                view === "home" ? "btn-primary" : "btn-outline-primary"
              } me-2`}
              onClick={() => setView("home")}
            >
              Home
            </button>
            <button
              className={`btn ${
                view === "schedule" ? "btn-primary" : "btn-outline-primary"
              } me-2`}
              onClick={() => setView("schedule")}
            >
              Schedule
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
        {view === "home" && <Home />}
        {view === "schedule" && <Schedule />}
        {view === "add" && <EventForm />}
      </main>
    </div>
  );
}

export default App;
