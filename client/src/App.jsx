import { useState, useEffect } from "react";
import Home from "./components/Home";
import Schedule from "./components/Schedule";
import EventForm from "./components/EventForm";
import Login from "./components/Login";
import ContributorProfile from "./components/ContributorProfile";
import { isAuthenticated, getCurrentUser, logout } from "./services/auth";

function App() {
  const [view, setView] = useState("home");
  const [user, setUser] = useState(null);

  useEffect(() => {
    if (isAuthenticated()) {
      setUser(getCurrentUser());
    }
  }, []);

  const handleLoginSuccess = (userData) => {
    setUser(userData);
    setView("profile");
  };

  const handleLogout = () => {
    logout();
    setUser(null);
    setView("home");
  };

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
            {user && (
              <button
                className={`btn ${
                  view === "add" ? "btn-primary" : "btn-outline-primary"
                } me-2`}
                onClick={() => setView("add")}
              >
                Add Event
              </button>
            )}

            {user ? (
              <>
                <button
                  className={`btn ${
                    view === "profile" ? "btn-success" : "btn-outline-success"
                  } me-2`}
                  onClick={() => setView("profile")}
                >
                  My Profile
                </button>
                <button
                  className="btn btn-outline-light"
                  onClick={handleLogout}
                >
                  Logout ({user.email})
                </button>
              </>
            ) : (
              <button
                className={`btn ${
                  view === "login" ? "btn-success" : "btn-outline-success"
                }`}
                onClick={() => setView("login")}
              >
                Login
              </button>
            )}
          </div>
        </div>
      </nav>

      <main>
        {view === "home" && <Home />}
        {view === "schedule" && <Schedule />}
        {view === "add" && user ? (
          <EventForm />
        ) : view === "add" ? (
          <div className="container mt-5">
            <div className="alert alert-warning" role="alert">
              <h4 className="alert-heading">Authentication Required</h4>
              <p>You must be logged in to add events.</p>
              <hr />
              <button
                className="btn btn-primary"
                onClick={() => setView("login")}
              >
                Go to Login
              </button>
            </div>
          </div>
        ) : null}
        {view === "login" && <Login onLoginSuccess={handleLoginSuccess} />}
        {view === "profile" && user && <ContributorProfile />}
      </main>
    </div>
  );
}

export default App;
