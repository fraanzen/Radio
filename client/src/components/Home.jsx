import { useState, useEffect } from "react";
import { getTodaySchedule } from "../services/api";
import NowPlaying from "./NowPlaying";

function Home() {
  const [currentShow, setCurrentShow] = useState(null);
  const [upcomingShows, setUpcomingShows] = useState([]);
  const [loading, setLoading] = useState(true);
  const [currentTime, setCurrentTime] = useState(new Date());

  const getNextFourHoursSchedule = (events, referenceTime) => {
    const current = events.find((event) => {
      const start = new Date(event.startTime);
      const end = new Date(event.endTime);
      return start <= referenceTime && end >= referenceTime;
    });

    const fourHoursFromNow = new Date(
      referenceTime.getTime() + 4 * 60 * 60 * 1000
    );
    const upcoming = events
      .filter((event) => {
        const start = new Date(event.startTime);
        return start > referenceTime && start <= fourHoursFromNow;
      })
      .slice(0, 10);

    return { current, upcoming };
  };

  useEffect(() => {
    const timeInterval = setInterval(() => {
      setCurrentTime(new Date());
    }, 1000);

    return () => clearInterval(timeInterval);
  }, []);

  useEffect(() => {
    async function fetchSchedule() {
      try {
        const events = await getTodaySchedule();
        const { current, upcoming } = getNextFourHoursSchedule(
          events,
          currentTime
        );

        setCurrentShow(current || null);
        setUpcomingShows(upcoming);
      } catch (error) {
        console.error("Error fetching schedule:", error);
      } finally {
        setLoading(false);
      }
    }

    fetchSchedule();
    const scheduleInterval = setInterval(fetchSchedule, 60000);

    return () => clearInterval(scheduleInterval);
  }, [currentTime]);

  const formatTime = (dateString) => {
    const date = new Date(dateString);
    return date.toLocaleTimeString("en-US", {
      hour: "2-digit",
      minute: "2-digit",
      hour12: false,
    });
  };

  const formatCurrentTime = () => {
    return currentTime.toLocaleTimeString("en-US", {
      hour: "2-digit",
      minute: "2-digit",
      second: "2-digit",
      hour12: false,
    });
  };

  if (loading) {
    return (
      <div className="container-fluid p-0">
        <div className="text-center mt-5">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="container-fluid p-0" style={{ backgroundColor: "#000" }}>
      <div
        className="hero-section text-white position-relative"
        style={{
          background: "linear-gradient(135deg, #4A90E2 0%, #67B5E8 100%)",
          minHeight: "500px",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          position: "relative",
          overflow: "hidden",
        }}
      >
        <div
          className="position-absolute w-100 h-100"
          style={{
            background:
              "radial-gradient(circle at 30% 50%, rgba(255,255,255,0.1) 0%, transparent 50%)",
          }}
        ></div>

        <div
          className="container text-center position-relative"
          style={{ zIndex: 1 }}
        >
          <div className="mb-3">
            <span
              className="badge bg-dark bg-opacity-50 fs-5 px-4 py-2"
              style={{ fontFamily: "monospace", letterSpacing: "0.1em" }}
            >
              {formatCurrentTime()}
            </span>
          </div>
          <h1
            className="display-1 fw-bold mb-4"
            style={{ fontSize: "6rem", letterSpacing: "0.05em" }}
          >
            RADIO STATION FM
          </h1>
          <div className="live-indicator mb-4">
            <span
              className="badge bg-danger fs-4 px-4 py-3"
              style={{
                animation: "pulse 2s infinite",
                boxShadow: "0 0 20px rgba(220,53,69,0.5)",
              }}
            >
              🔴 LIVE NOW
            </span>
          </div>
          {currentShow ? (
            <div className="current-show mt-4">
              <h2 className="display-4 mb-2">{currentShow.title}</h2>
              <p className="fs-5 opacity-75">
                {formatTime(currentShow.startTime)} -{" "}
                {formatTime(currentShow.endTime)}
              </p>
              {currentShow.type === "live" && currentShow.hosts && (
                <p className="fs-6 opacity-75">
                  with {currentShow.hosts.join(", ")}
                </p>
              )}
            </div>
          ) : (
            <div className="current-show mt-4">
              <h2 className="display-4 mb-2">Music Playing</h2>
              <p className="fs-5 opacity-75">Non-stop hits 24/7</p>
            </div>
          )}
        </div>
      </div>

      <div className="container my-5">
        <div className="row">
          <div className="col-lg-8 mx-auto">
            <NowPlaying />

            <div className="card bg-dark text-white border-0 shadow-lg mt-4">
              <div className="card-header bg-transparent border-bottom border-secondary">
                <h3 className="mb-0">
                  <span className="me-2">📅</span>
                  Coming Up
                </h3>
              </div>
              <div className="card-body p-0">
                {upcomingShows.length > 0 ? (
                  <div className="list-group list-group-flush">
                    {upcomingShows.map((show, index) => (
                      <div
                        key={show.id || index}
                        className="list-group-item bg-dark text-white border-secondary d-flex align-items-center py-3"
                        style={{
                          transition: "background-color 0.3s",
                          cursor: "pointer",
                        }}
                        onMouseEnter={(e) =>
                          (e.currentTarget.style.backgroundColor = "#1a1a1a")
                        }
                        onMouseLeave={(e) =>
                          (e.currentTarget.style.backgroundColor = "")
                        }
                      >
                        <div
                          className="time-badge me-4 text-center"
                          style={{ minWidth: "80px" }}
                        >
                          <div
                            className="badge bg-primary fs-6 px-3 py-2"
                            style={{ fontWeight: "600" }}
                          >
                            {formatTime(show.startTime)}
                          </div>
                        </div>
                        <div className="flex-grow-1">
                          <h5 className="mb-1 text-white">{show.title}</h5>
                          <small style={{ color: "#adb5bd" }}>
                            {show.durationMinutes} minutes
                            {show.type === "live" && show.hosts && (
                              <span className="ms-2">
                                • {show.hosts.join(", ")}
                              </span>
                            )}
                            {show.type === "reportage" && show.reporter && (
                              <span className="ms-2">• {show.reporter}</span>
                            )}
                          </small>
                        </div>
                        <div className="ms-3">
                          {show.type === "live" && (
                            <span className="badge bg-danger">🎙️ Live</span>
                          )}
                          {show.type === "reportage" && (
                            <span className="badge bg-info">📰 News</span>
                          )}
                          {show.type === "music" && (
                            <span className="badge bg-success">🎵 Music</span>
                          )}
                        </div>
                      </div>
                    ))}
                  </div>
                ) : (
                  <div
                    className="text-center py-5"
                    style={{ color: "#adb5bd" }}
                  >
                    <p>No upcoming shows in the next 4 hours</p>
                  </div>
                )}
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="container-fluid bg-dark text-white py-5 mt-5">
        <div className="container">
          <div className="row text-center">
            <div className="col-md-4 mb-4">
              <div className="p-4">
                <div
                  className="icon-circle mx-auto mb-3"
                  style={{
                    width: "80px",
                    height: "80px",
                    background:
                      "linear-gradient(135deg, #4A90E2 0%, #67B5E8 100%)",
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "2rem",
                  }}
                >
                  🎵
                </div>
                <h4 className="text-white">Non-Stop Music</h4>
                <p style={{ color: "#adb5bd" }}>The best hits playing 24/7</p>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="p-4">
                <div
                  className="icon-circle mx-auto mb-3"
                  style={{
                    width: "80px",
                    height: "80px",
                    background:
                      "linear-gradient(135deg, #4A90E2 0%, #67B5E8 100%)",
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "2rem",
                  }}
                >
                  🎙️
                </div>
                <h4 className="text-white">Live Shows</h4>
                <p style={{ color: "#adb5bd" }}>
                  Engaging hosts and exciting guests
                </p>
              </div>
            </div>
            <div className="col-md-4 mb-4">
              <div className="p-4">
                <div
                  className="icon-circle mx-auto mb-3"
                  style={{
                    width: "80px",
                    height: "80px",
                    background:
                      "linear-gradient(135deg, #4A90E2 0%, #67B5E8 100%)",
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontSize: "2rem",
                  }}
                >
                  📰
                </div>
                <h4 className="text-white">News & Reports</h4>
                <p style={{ color: "#adb5bd" }}>
                  Stay informed with the latest updates
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.7; }
        }
      `}</style>
    </div>
  );
}

export default Home;
