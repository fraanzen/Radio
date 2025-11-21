import { useState, useEffect } from "react";

function NowPlaying() {
  const [currentSong, setCurrentSong] = useState({
    title: "Summer Breeze",
    artist: "The Vibes",
    album: "Sunset Sessions",
    duration: "3:45",
    elapsed: "1:23",
  });

  const [progress, setProgress] = useState(37);

  useEffect(() => {
    const interval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 100) {
          setCurrentSong({
            title: [
              "Electric Dreams",
              "Midnight City",
              "Ocean Eyes",
              "Golden Hour",
            ][Math.floor(Math.random() * 4)],
            artist: ["Luna Sky", "The Groove", "Neon Waves", "Echo Park"][
              Math.floor(Math.random() * 4)
            ],
            album: "Top Hits 2025",
            duration: "3:45",
            elapsed: "0:00",
          });
          return 0;
        }
        return prev + 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, []);

  const formatProgress = () => {
    const totalSeconds = 225;
    const elapsedSeconds = Math.floor((progress / 100) * totalSeconds);
    const minutes = Math.floor(elapsedSeconds / 60);
    const seconds = elapsedSeconds % 60;
    return `${minutes}:${seconds.toString().padStart(2, "0")}`;
  };

  return (
    <div className="card bg-dark text-white border-0 shadow-lg">
      <div className="card-body p-4">
        <div className="d-flex align-items-center mb-3">
          <div
            className="album-art me-4"
            style={{
              width: "80px",
              height: "80px",
              background: "linear-gradient(135deg, #4A90E2 0%, #67B5E8 100%)",
              borderRadius: "8px",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              fontSize: "2.5rem",
              flexShrink: 0,
            }}
          >
            🎵
          </div>
          <div className="flex-grow-1">
            <div className="mb-1">
              <span className="badge bg-danger mb-2">
                <span
                  style={{
                    display: "inline-block",
                    width: "8px",
                    height: "8px",
                    backgroundColor: "#fff",
                    borderRadius: "50%",
                    marginRight: "6px",
                    animation: "pulse 2s infinite",
                  }}
                ></span>
                NOW PLAYING
              </span>
            </div>
            <h5 className="mb-1 fw-bold text-white">{currentSong.title}</h5>
            <p className="mb-0" style={{ color: "#adb5bd" }}>
              {currentSong.artist} • {currentSong.album}
            </p>
          </div>
        </div>

        <div className="progress mb-2" style={{ height: "6px" }}>
          <div
            className="progress-bar"
            role="progressbar"
            style={{
              width: `${progress}%`,
              background: "linear-gradient(90deg, #4A90E2 0%, #67B5E8 100%)",
            }}
            aria-valuenow={progress}
            aria-valuemin="0"
            aria-valuemax="100"
          ></div>
        </div>

        <div
          className="d-flex justify-content-between"
          style={{ color: "#adb5bd" }}
        >
          <small>{formatProgress()}</small>
          <small>{currentSong.duration}</small>
        </div>
      </div>

      <style>{`
        @keyframes pulse {
          0%, 100% { opacity: 1; }
          50% { opacity: 0.5; }
        }
      `}</style>
    </div>
  );
}

export default NowPlaying;
