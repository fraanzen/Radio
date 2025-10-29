import { useState } from "react";
import "./App.css";
import Schedule from "./components/Schedule";
import EventForm from "./components/EventForm";

function App() {
  const [view, setView] = useState("schedule");

  return (
    <div className="app">
      <header className="app-header">
        <h1>📻 Radio Station Scheduler</h1>
        <nav>
          <button
            className={view === "schedule" ? "active" : ""}
            onClick={() => setView("schedule")}
          >
            View Schedule
          </button>
          <button
            className={view === "add" ? "active" : ""}
            onClick={() => setView("add")}
          >
            Add Event
          </button>
        </nav>
      </header>

      <main className="app-main">
        {view === "schedule" && <Schedule />}
        {view === "add" && <EventForm />}
      </main>
    </div>
  );
}

export default App;
