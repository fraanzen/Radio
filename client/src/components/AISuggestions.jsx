import { useState } from "react";
import { getAISuggestions } from "../services/api";

function AISuggestions({ isOpen, onClose }) {
  const [prompt, setPrompt] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [conversationId, setConversationId] = useState(null);
  const [messages, setMessages] = useState([]);

  const handleSubmit = async (e) => {
    e.preventDefault();
    if (!prompt.trim()) return;

    setLoading(true);
    setError(null);

    // Add user message to chat
    const userMessage = { type: "user", text: prompt };
    setMessages((prev) => [...prev, userMessage]);

    try {
      const result = await getAISuggestions(prompt, conversationId);
      setConversationId(result.conversationId);

      // Add AI response to chat
      const aiMessage = {
        type: "ai",
        text: result.data.message,
        songs: result.data.songs,
      };
      setMessages((prev) => [...prev, aiMessage]);
      setPrompt("");
    } catch (err) {
      setError("Failed to get suggestions. Make sure the backend is running.");
      console.error("Error getting AI suggestions:", err);
    } finally {
      setLoading(false);
    }
  };

  const handleNewConversation = () => {
    setConversationId(null);
    setMessages([]);
    setPrompt("");
    setError(null);
  };

  if (!isOpen) return null;

  return (
    <div
      className="modal show d-block"
      style={{ backgroundColor: "rgba(0,0,0,0.5)" }}
      onClick={onClose}
    >
      <div
        className="modal-dialog modal-lg modal-dialog-centered"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="modal-content">
          <div className="modal-header bg-primary text-white">
            <h5 className="modal-title">🎵 AI Music Suggestions</h5>
            <div>
              {conversationId && (
                <button
                  className="btn btn-sm btn-outline-light me-2"
                  onClick={handleNewConversation}
                  title="Start new conversation"
                >
                  New Chat
                </button>
              )}
              <button
                type="button"
                className="btn-close btn-close-white"
                onClick={onClose}
              ></button>
            </div>
          </div>

          <div
            className="modal-body"
            style={{ maxHeight: "60vh", overflowY: "auto" }}
          >
            {messages.length === 0 ? (
              <div className="text-center text-muted py-4">
                <p className="mb-2">What kind of music are you looking for?</p>
                <small>
                  Try: "80s rock ballads", "upbeat morning show music", or
                  "chill jazz for late night"
                </small>
              </div>
            ) : (
              <div className="d-flex flex-column gap-3">
                {messages.map((msg, index) => (
                  <div
                    key={index}
                    className={`d-flex ${
                      msg.type === "user"
                        ? "justify-content-end"
                        : "justify-content-start"
                    }`}
                  >
                    <div
                      className={`p-3 rounded-3 ${
                        msg.type === "user"
                          ? "bg-primary text-white"
                          : "bg-light"
                      }`}
                      style={{ maxWidth: "85%" }}
                    >
                      {msg.type === "user" ? (
                        <span>{msg.text}</span>
                      ) : (
                        <>
                          <p className="mb-2 fw-medium">{msg.text}</p>
                          {msg.songs && (
                            <div className="mt-2">
                              <table className="table table-sm table-borderless mb-0">
                                <tbody>
                                  {msg.songs.map((song, i) => (
                                    <tr key={i}>
                                      <td
                                        className="ps-0"
                                        style={{ width: "20px" }}
                                      >
                                        <span className="text-muted">
                                          {i + 1}.
                                        </span>
                                      </td>
                                      <td>
                                        <strong>{song.title}</strong>
                                        <br />
                                        <small className="text-muted">
                                          {song.artist}
                                        </small>
                                      </td>
                                    </tr>
                                  ))}
                                </tbody>
                              </table>
                            </div>
                          )}
                        </>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}

            {error && (
              <div className="alert alert-danger mt-3" role="alert">
                {error}
              </div>
            )}
          </div>

          <div className="modal-footer">
            <form onSubmit={handleSubmit} className="w-100">
              <div className="input-group">
                <input
                  type="text"
                  className="form-control"
                  placeholder={
                    messages.length > 0
                      ? "Ask for more, adjust style, or try something new..."
                      : "Describe the music you're looking for..."
                  }
                  value={prompt}
                  onChange={(e) => setPrompt(e.target.value)}
                  disabled={loading}
                />
                <button
                  type="submit"
                  className="btn btn-primary"
                  disabled={loading || !prompt.trim()}
                >
                  {loading ? (
                    <>
                      <span
                        className="spinner-border spinner-border-sm me-1"
                        role="status"
                      ></span>
                      Thinking...
                    </>
                  ) : (
                    "Ask"
                  )}
                </button>
              </div>
              {conversationId && (
                <small className="text-muted mt-1 d-block">
                  Conversation active - try "more upbeat", "add some 90s hits",
                  or "less ballads"
                </small>
              )}
            </form>
          </div>
        </div>
      </div>
    </div>
  );
}

export default AISuggestions;
