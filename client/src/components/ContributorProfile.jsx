import { useState, useEffect } from "react";
import { getMyProfile } from "../services/auth";

function ContributorProfile() {
  const [profile, setProfile] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadProfile();
  }, []);

  const loadProfile = async () => {
    try {
      setLoading(true);
      const data = await getMyProfile();
      setProfile(data);
    } catch (err) {
      console.error("Error loading profile:", err);
      if (err.response?.status === 404) {
        setError(
          "No contributor profile found. Admin users don't have contributor profiles. Please register as a contributor or login with a contributor account."
        );
      } else if (err.response?.status === 401) {
        setError("Not authenticated. Please log in.");
      } else {
        setError(
          "Failed to load profile. " +
            (err.response?.data?.error || err.message)
        );
      }
    } finally {
      setLoading(false);
    }
  };

  const formatCurrency = (amount) => {
    return new Intl.NumberFormat("sv-SE", {
      style: "currency",
      currency: "SEK",
    }).format(amount);
  };

  const formatDate = (dateString) => {
    return new Date(dateString).toLocaleDateString("sv-SE", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  };

  const formatDateTime = (dateString) => {
    return new Date(dateString).toLocaleString("sv-SE", {
      year: "numeric",
      month: "long",
      day: "numeric",
      hour: "2-digit",
      minute: "2-digit",
    });
  };

  const getMonthName = (month) => {
    const months = [
      "Januari",
      "Februari",
      "Mars",
      "April",
      "Maj",
      "Juni",
      "Juli",
      "Augusti",
      "September",
      "Oktober",
      "November",
      "December",
    ];
    return months[month - 1];
  };

  if (loading) {
    return (
      <div className="container mt-4">
        <div className="text-center">
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p className="mt-2">Loading profile...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="container mt-4">
        <div className="alert alert-danger" role="alert">
          <h4 className="alert-heading">Error</h4>
          <p>{error}</p>
        </div>
      </div>
    );
  }

  if (!profile) {
    return (
      <div className="container mt-4">
        <div className="alert alert-warning" role="alert">
          No profile found. Please contact the administrator.
        </div>
      </div>
    );
  }

  return (
    <div className="container mt-4">
      <h2 className="mb-4">Contributor Profile</h2>

      {/* Personal Information Card */}
      <div className="card mb-4">
        <div className="card-header bg-primary text-white">
          <h5 className="mb-0">Personal Information</h5>
        </div>
        <div className="card-body">
          <div className="row">
            {profile.photoUrl && (
              <div className="col-md-3 text-center mb-3">
                <img
                  src={profile.photoUrl}
                  alt={profile.fullName}
                  className="img-fluid rounded-circle"
                  style={{ maxWidth: "200px" }}
                />
              </div>
            )}
            <div className={profile.photoUrl ? "col-md-9" : "col-12"}>
              <div className="row mb-3">
                <div className="col-md-6">
                  <strong>Name:</strong>
                  <p>{profile.fullName}</p>
                </div>
                <div className="col-md-6">
                  <strong>Email:</strong>
                  <p>{profile.email}</p>
                </div>
              </div>
              <div className="row mb-3">
                <div className="col-md-6">
                  <strong>Phone:</strong>
                  <p>{profile.phoneNumber}</p>
                </div>
                <div className="col-md-6">
                  <strong>Address:</strong>
                  <p>{profile.address}</p>
                </div>
              </div>
              {profile.biography && (
                <div className="row">
                  <div className="col-12">
                    <strong>Biography:</strong>
                    <p>{profile.biography}</p>
                  </div>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Payment History Card */}
      <div className="card mb-4">
        <div className="card-header bg-success text-white">
          <h5 className="mb-0">Payment History</h5>
        </div>
        <div className="card-body">
          {profile.paymentHistory && profile.paymentHistory.length > 0 ? (
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th>Period</th>
                    <th>Hours</th>
                    <th>Events</th>
                    <th>Subtotal</th>
                    <th>VAT (25%)</th>
                    <th>Total</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {profile.paymentHistory.map((payment) => (
                    <tr key={payment.id}>
                      <td>
                        <strong>
                          {getMonthName(payment.month)} {payment.year}
                        </strong>
                      </td>
                      <td>{payment.totalHours.toFixed(2)} h</td>
                      <td>{payment.totalEvents}</td>
                      <td>{formatCurrency(payment.subtotalAmount)}</td>
                      <td>{formatCurrency(payment.vatAmount)}</td>
                      <td>
                        <strong>{formatCurrency(payment.totalAmount)}</strong>
                      </td>
                      <td>
                        {payment.isPaid ? (
                          <span className="badge bg-success">
                            Paid{" "}
                            {payment.paidAt ? formatDate(payment.paidAt) : ""}
                          </span>
                        ) : (
                          <span className="badge bg-warning text-dark">
                            Pending
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-muted">No payment history available.</p>
          )}
        </div>
      </div>

      {/* Recent Assignments Card */}
      <div className="card mb-4">
        <div className="card-header bg-info text-white">
          <h5 className="mb-0">Recent Assignments</h5>
        </div>
        <div className="card-body">
          {profile.recentAssignments && profile.recentAssignments.length > 0 ? (
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th>Program</th>
                    <th>Date & Time</th>
                    <th>Duration</th>
                    <th>Role</th>
                  </tr>
                </thead>
                <tbody>
                  {profile.recentAssignments.map((assignment) => (
                    <tr key={assignment.id}>
                      <td>
                        <strong>{assignment.contentTitle}</strong>
                      </td>
                      <td>{formatDateTime(assignment.contentStartTime)}</td>
                      <td>
                        {Math.floor(
                          assignment.contentDuration.totalMinutes || 0
                        )}{" "}
                        min
                      </td>
                      <td>
                        <span
                          className={`badge ${
                            assignment.role === "Host"
                              ? "bg-primary"
                              : assignment.role === "CoHost"
                              ? "bg-info"
                              : assignment.role === "Guest"
                              ? "bg-secondary"
                              : "bg-warning"
                          }`}
                        >
                          {assignment.role}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-muted">No assignments yet.</p>
          )}
        </div>
      </div>
    </div>
  );
}

export default ContributorProfile;
