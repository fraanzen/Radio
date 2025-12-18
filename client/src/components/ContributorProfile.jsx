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
    <div className="container-fluid" style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', minHeight: '100vh', paddingTop: '2rem', paddingBottom: '3rem' }}>
      <div className="container">
        {/* Header Section */}
        <div className="text-center mb-5">
          <h1 className="display-4 fw-bold text-white mb-2">Contributor Profile</h1>
          <p className="text-white-50">Manage your information and track your work</p>
        </div>

        {/* Profile Header Card */}
        <div className="card shadow-lg border-0 mb-4" style={{ borderRadius: '20px', overflow: 'hidden' }}>
          <div style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)', padding: '3rem 2rem' }}>
            <div className="row align-items-center">
              {profile.photoUrl && (
                <div className="col-md-3 text-center mb-3 mb-md-0">
                  <img
                    src={profile.photoUrl}
                    alt={profile.fullName}
                    className="rounded-circle border border-5 border-white shadow"
                    style={{ width: '180px', height: '180px', objectFit: 'cover' }}
                  />
                </div>
              )}
              <div className={profile.photoUrl ? "col-md-9" : "col-12"}>
                <h2 className="text-white fw-bold mb-3">{profile.fullName}</h2>
                <div className="row g-3">
                  <div className="col-md-6">
                    <div className="d-flex align-items-center text-white">
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" className="me-2" viewBox="0 0 16 16">
                        <path d="M.05 3.555A2 2 0 0 1 2 2h12a2 2 0 0 1 1.95 1.555L8 8.414.05 3.555ZM0 4.697v7.104l5.803-3.558L0 4.697ZM6.761 8.83l-6.57 4.027A2 2 0 0 0 2 14h12a2 2 0 0 0 1.808-1.144l-6.57-4.027L8 9.586l-1.239-.757Zm3.436-.586L16 11.801V4.697l-5.803 3.546Z"/>
                      </svg>
                      <span>{profile.email}</span>
                    </div>
                  </div>
                  <div className="col-md-6">
                    <div className="d-flex align-items-center text-white">
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" className="me-2" viewBox="0 0 16 16">
                        <path d="M3.654 1.328a.678.678 0 0 0-1.015-.063L1.605 2.3c-.483.484-.661 1.169-.45 1.77a17.568 17.568 0 0 0 4.168 6.608 17.569 17.569 0 0 0 6.608 4.168c.601.211 1.286.033 1.77-.45l1.034-1.034a.678.678 0 0 0-.063-1.015l-2.307-1.794a.678.678 0 0 0-.58-.122l-2.19.547a1.745 1.745 0 0 1-1.657-.459L5.482 8.062a1.745 1.745 0 0 1-.46-1.657l.548-2.19a.678.678 0 0 0-.122-.58L3.654 1.328zM1.884.511a1.745 1.745 0 0 1 2.612.163L6.29 2.98c.329.423.445.974.315 1.494l-.547 2.19a.678.678 0 0 0 .178.643l2.457 2.457a.678.678 0 0 0 .644.178l2.189-.547a1.745 1.745 0 0 1 1.494.315l2.306 1.794c.829.645.905 1.87.163 2.611l-1.034 1.034c-.74.74-1.846 1.065-2.877.702a18.634 18.634 0 0 1-7.01-4.42 18.634 18.634 0 0 1-4.42-7.009c-.362-1.03-.037-2.137.703-2.877L1.885.511z"/>
                      </svg>
                      <span>{profile.phoneNumber}</span>
                    </div>
                  </div>
                  <div className="col-12">
                    <div className="d-flex align-items-start text-white">
                      <svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" fill="currentColor" className="me-2 mt-1" viewBox="0 0 16 16">
                        <path d="M8.707 1.5a1 1 0 0 0-1.414 0L.646 8.146a.5.5 0 0 0 .708.708L2 8.207V13.5A1.5 1.5 0 0 0 3.5 15h9a1.5 1.5 0 0 0 1.5-1.5V8.207l.646.647a.5.5 0 0 0 .708-.708L13 5.793V2.5a.5.5 0 0 0-.5-.5h-1a.5.5 0 0 0-.5.5v1.293L8.707 1.5ZM13 7.207V13.5a.5.5 0 0 1-.5.5h-9a.5.5 0 0 1-.5-.5V7.207l5-5 5 5Z"/>
                      </svg>
                      <span>{profile.address}</span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
            {profile.biography && (
              <div className="mt-4 pt-4 border-top border-white border-opacity-25">
                <h6 className="text-white-50 mb-2">BIOGRAPHY</h6>
                <p className="text-white mb-0" style={{ fontSize: '1.1rem' }}>{profile.biography}</p>
              </div>
            )}
          </div>
        </div>

        {/* Stats Overview */}
        <div className="row g-4 mb-4">
          <div className="col-md-4">
            <div className="card shadow-sm border-0 h-100" style={{ borderRadius: '15px' }}>
              <div className="card-body text-center p-4">
                <div className="mb-3" style={{ color: '#667eea' }}>
                  <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"/>
                  </svg>
                </div>
                <h3 className="fw-bold mb-1" style={{ color: '#667eea' }}>
                  {profile.paymentHistory?.reduce((sum, p) => sum + p.totalHours, 0)?.toFixed(1) || '0'} h
                </h3>
                <p className="text-muted mb-0">Total Hours</p>
              </div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card shadow-sm border-0 h-100" style={{ borderRadius: '15px' }}>
              <div className="card-body text-center p-4">
                <div className="mb-3" style={{ color: '#10b981' }}>
                  <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M4 10.781c.148 1.667 1.513 2.85 3.591 3.003V15h1.043v-1.216c2.27-.179 3.678-1.438 3.678-3.3 0-1.59-.947-2.51-2.956-3.028l-.722-.187V3.467c1.122.11 1.879.714 2.07 1.616h1.47c-.166-1.6-1.54-2.748-3.54-2.875V1H7.591v1.233c-1.939.23-3.27 1.472-3.27 3.156 0 1.454.966 2.483 2.661 2.917l.61.162v4.031c-1.149-.17-1.94-.8-2.131-1.718H4zm3.391-3.836c-1.043-.263-1.6-.825-1.6-1.616 0-.944.704-1.641 1.8-1.828v3.495l-.2-.05zm1.591 1.872c1.287.323 1.852.859 1.852 1.769 0 1.097-.826 1.828-2.2 1.939V8.73l.348.086z"/>
                  </svg>
                </div>
                <h3 className="fw-bold mb-1" style={{ color: '#10b981' }}>
                  {formatCurrency(profile.paymentHistory?.reduce((sum, p) => sum + p.totalAmount, 0) || 0)}
                </h3>
                <p className="text-muted mb-0">Total Earned</p>
              </div>
            </div>
          </div>
          <div className="col-md-4">
            <div className="card shadow-sm border-0 h-100" style={{ borderRadius: '15px' }}>
              <div className="card-body text-center p-4">
                <div className="mb-3" style={{ color: '#f59e0b' }}>
                  <svg xmlns="http://www.w3.org/2000/svg" width="48" height="48" fill="currentColor" viewBox="0 0 16 16">
                    <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2zM8 1.918l-.797.161A4.002 4.002 0 0 0 4 6c0 .628-.134 2.197-.459 3.742-.16.767-.376 1.566-.663 2.258h10.244c-.287-.692-.502-1.49-.663-2.258C12.134 8.197 12 6.628 12 6a4.002 4.002 0 0 0-3.203-3.92L8 1.917zM14.22 12c.223.447.481.801.78 1H1c.299-.199.557-.553.78-1C2.68 10.2 3 6.88 3 6c0-2.42 1.72-4.44 4.005-4.901a1 1 0 1 1 1.99 0A5.002 5.002 0 0 1 13 6c0 .88.32 4.2 1.22 6z"/>
                  </svg>
                </div>
                <h3 className="fw-bold mb-1" style={{ color: '#f59e0b' }}>
                  {profile.recentAssignments?.length || 0}
                </h3>
                <p className="text-muted mb-0">Recent Events</p>
              </div>
            </div>
          </div>
        </div>

      {/* Payment History Card */}
      <div className="card shadow-sm border-0 mb-4" style={{ borderRadius: '20px' }}>
        <div className="card-body p-4">
          <div className="d-flex align-items-center mb-4">
            <div className="me-3" style={{ color: '#10b981' }}>
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">
                <path d="M1 3a1 1 0 0 1 1-1h12a1 1 0 0 1 1 1H1zm7 8a2 2 0 1 0 0-4 2 2 0 0 0 0 4z"/>
                <path d="M0 5a1 1 0 0 1 1-1h14a1 1 0 0 1 1 1v8a1 1 0 0 1-1 1H1a1 1 0 0 1-1-1V5zm3 0a2 2 0 0 1-2 2v4a2 2 0 0 1 2 2h10a2 2 0 0 1 2-2V7a2 2 0 0 1-2-2H3z"/>
              </svg>
            </div>
            <div>
              <h4 className="mb-0 fw-bold">Payment History</h4>
              <p className="text-muted mb-0 small">Track your monthly earnings</p>
            </div>
          </div>
          {profile.paymentHistory && profile.paymentHistory.length > 0 ? (
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead style={{ background: '#f8f9fa' }}>
                  <tr>
                    <th className="border-0 text-muted fw-semibold">Period</th>
                    <th className="border-0 text-muted fw-semibold">Hours</th>
                    <th className="border-0 text-muted fw-semibold">Events</th>
                    <th className="border-0 text-muted fw-semibold">Subtotal</th>
                    <th className="border-0 text-muted fw-semibold">VAT</th>
                    <th className="border-0 text-muted fw-semibold">Total</th>
                    <th className="border-0 text-muted fw-semibold">Status</th>
                  </tr>
                </thead>
                <tbody>
                  {profile.paymentHistory.map((payment) => (
                    <tr key={payment.id}>
                      <td>
                        <strong className="text-dark">
                          {getMonthName(payment.month)} {payment.year}
                        </strong>
                      </td>
                      <td><span className="badge bg-light text-dark">{payment.totalHours.toFixed(2)} h</span></td>
                      <td><span className="badge bg-light text-dark">{payment.totalEvents}</span></td>
                      <td className="text-muted">{formatCurrency(payment.subtotalAmount)}</td>
                      <td className="text-muted">{formatCurrency(payment.vatAmount)}</td>
                      <td>
                        <strong className="text-success">{formatCurrency(payment.totalAmount)}</strong>
                      </td>
                      <td>
                        {payment.isPaid ? (
                          <span className="badge rounded-pill" style={{ background: '#10b981', color: 'white' }}>
                            ✓ Paid {payment.paidAt ? formatDate(payment.paidAt) : ""}
                          </span>
                        ) : (
                          <span className="badge rounded-pill" style={{ background: '#f59e0b', color: 'white' }}>
                            ⏱ Pending
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <div className="text-center py-5">
              <div className="mb-3 text-muted">
                <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" fill="currentColor" viewBox="0 0 16 16">
                  <path d="M14 4.5V14a2 2 0 0 1-2 2H4a2 2 0 0 1-2-2V2a2 2 0 0 1 2-2h5.5L14 4.5zm-3 0A1.5 1.5 0 0 1 9.5 3V1H4a1 1 0 0 0-1 1v12a1 1 0 0 0 1 1h8a1 1 0 0 0 1-1V4.5h-2z"/>
                </svg>
              </div>
              <p className="text-muted mb-0">No payment history available yet</p>
            </div>
          )}
        </div>
      </div>

      {/* Recent Assignments Card */}
      <div className="card shadow-sm border-0 mb-4" style={{ borderRadius: '20px' }}>
        <div className="card-body p-4">
          <div className="d-flex align-items-center mb-4">
            <div className="me-3" style={{ color: '#667eea' }}>
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">
                <path d="M11 6.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-5 3a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1z"/>
                <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z"/>
              </svg>
            </div>
            <div>
              <h4 className="mb-0 fw-bold">Recent Assignments</h4>
              <p className="text-muted mb-0 small">Your upcoming and past events</p>
            </div>
          </div>
          {profile.recentAssignments && profile.recentAssignments.length > 0 ? (
            <div className="table-responsive">
              <table className="table table-hover align-middle">
                <thead style={{ background: '#f8f9fa' }}>
                  <tr>
                    <th className="border-0 text-muted fw-semibold">Program</th>
                    <th className="border-0 text-muted fw-semibold">Date & Time</th>
                    <th className="border-0 text-muted fw-semibold">Duration</th>
                    <th className="border-0 text-muted fw-semibold">Role</th>
                  </tr>
                </thead>
                <tbody>
                  {profile.recentAssignments.map((assignment) => (
                    <tr key={assignment.id}>
                      <td>
                        <strong className="text-dark">{assignment.contentTitle}</strong>
                      </td>
                      <td className="text-muted">{formatDateTime(assignment.contentStartTime)}</td>
                      <td>
                        <span className="badge bg-light text-dark">
                          {Math.floor(assignment.contentDuration.totalMinutes || 0)} min
                        </span>
                      </td>
                      <td>
                        <span
                          className={`badge rounded-pill ${
                            assignment.role === "Host"
                              ? "text-white"
                              : assignment.role === "CoHost"
                              ? "text-white"
                              : assignment.role === "Guest"
                              ? "text-white"
                              : "text-dark"
                          }`}
                          style={{
                            background: assignment.role === "Host" ? "#667eea" 
                              : assignment.role === "CoHost" ? "#764ba2"
                              : assignment.role === "Guest" ? "#6c757d"
                              : "#f59e0b"
                          }}
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
            <div className="text-center py-5">
              <div className="mb-3 text-muted">
                <svg xmlns="http://www.w3.org/2000/svg" width="64" height="64" fill="currentColor" viewBox="0 0 16 16">
                  <path d="M11 6.5a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm-5 3a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1zm3 0a.5.5 0 0 1 .5-.5h1a.5.5 0 0 1 .5.5v1a.5.5 0 0 1-.5.5h-1a.5.5 0 0 1-.5-.5v-1z"/>
                  <path d="M3.5 0a.5.5 0 0 1 .5.5V1h8V.5a.5.5 0 0 1 1 0V1h1a2 2 0 0 1 2 2v11a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V3a2 2 0 0 1 2-2h1V.5a.5.5 0 0 1 .5-.5zM1 4v10a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V4H1z"/>
                </svg>
              </div>
              <p className="text-muted mb-0">No assignments yet</p>
            </div>
          )}
        </div>
      </div>

      {/* Payment Rates */}
      <div className="card shadow-sm border-0 mb-4" style={{ borderRadius: '20px' }}>
        <div className="card-body p-4">
          <div className="d-flex align-items-center mb-4">
            <div className="me-3" style={{ color: '#10b981' }}>
              <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="currentColor" viewBox="0 0 16 16">
                <path d="M0 4a2 2 0 0 1 2-2h12a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2V4zm2-1a1 1 0 0 0-1 1v1h14V4a1 1 0 0 0-1-1H2zm13 4H1v5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1V7z"/>
                <path d="M2 10a1 1 0 0 1 1-1h1a1 1 0 0 1 1 1v1a1 1 0 0 1-1 1H3a1 1 0 0 1-1-1v-1z"/>
              </svg>
            </div>
            <div>
              <h4 className="mb-0 fw-bold">Payment Rates</h4>
              <p className="text-muted mb-0 small">Your earnings per hour and event</p>
            </div>
          </div>
          
          <div className="row g-4 mb-4">
            <div className="col-md-4">
              <div className="text-center p-4 rounded-3" style={{ background: 'linear-gradient(135deg, #667eea 0%, #764ba2 100%)' }}>
                <div className="mb-2">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="white" viewBox="0 0 16 16">
                    <path d="M16 8A8 8 0 1 1 0 8a8 8 0 0 1 16 0zM8 3.5a.5.5 0 0 0-1 0V9a.5.5 0 0 0 .252.434l3.5 2a.5.5 0 0 0 .496-.868L8 8.71V3.5z"/>
                  </svg>
                </div>
                <h6 className="text-white text-opacity-75 mb-2 small">HOURLY RATE</h6>
                <h2 className="fw-bold text-white mb-0">750 SEK</h2>
                <small className="text-white text-opacity-75">per hour</small>
              </div>
            </div>
            <div className="col-md-4">
              <div className="text-center p-4 rounded-3" style={{ background: 'linear-gradient(135deg, #10b981 0%, #059669 100%)' }}>
                <div className="mb-2">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="white" viewBox="0 0 16 16">
                    <path d="M8 16a2 2 0 0 0 2-2H6a2 2 0 0 0 2 2zM8 1.918l-.797.161A4.002 4.002 0 0 0 4 6c0 .628-.134 2.197-.459 3.742-.16.767-.376 1.566-.663 2.258h10.244c-.287-.692-.502-1.49-.663-2.258C12.134 8.197 12 6.628 12 6a4.002 4.002 0 0 0-3.203-3.92L8 1.917zM14.22 12c.223.447.481.801.78 1H1c.299-.199.557-.553.78-1C2.68 10.2 3 6.88 3 6c0-2.42 1.72-4.44 4.005-4.901a1 1 0 1 1 1.99 0A5.002 5.002 0 0 1 13 6c0 .88.32 4.2 1.22 6z"/>
                  </svg>
                </div>
                <h6 className="text-white text-opacity-75 mb-2 small">EVENT FEE</h6>
                <h2 className="fw-bold text-white mb-0">300 SEK</h2>
                <small className="text-white text-opacity-75">per event</small>
              </div>
            </div>
            <div className="col-md-4">
              <div className="text-center p-4 rounded-3" style={{ background: 'linear-gradient(135deg, #f59e0b 0%, #d97706 100%)' }}>
                <div className="mb-2">
                  <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" fill="white" viewBox="0 0 16 16">
                    <path d="M5.5 9.511c.076.954.83 1.697 2.182 1.785V12h.6v-.709c1.4-.098 2.218-.846 2.218-1.932 0-.987-.626-1.496-1.745-1.76l-.473-.112V5.57c.6.068.982.396 1.074.85h1.052c-.076-.919-.864-1.638-2.126-1.716V4h-.6v.719c-1.195.117-2.01.836-2.01 1.853 0 .9.606 1.472 1.613 1.707l.397.098v2.034c-.615-.093-1.022-.43-1.114-.9H5.5zm2.177-2.166c-.59-.137-.91-.416-.91-.836 0-.47.345-.822.915-.925v1.76h-.005zm.692 1.193c.717.166 1.048.435 1.048.91 0 .542-.412.914-1.135.982V8.518l.087.02z"/>
                    <path d="M8 15A7 7 0 1 1 8 1a7 7 0 0 1 0 14zm0 1A8 8 0 1 0 8 0a8 8 0 0 0 0 16z"/>
                    <path d="M8 13.5a5.5 5.5 0 1 1 0-11 5.5 5.5 0 0 1 0 11zm0 .5A6 6 0 1 0 8 2a6 6 0 0 0 0 12z"/>
                  </svg>
                </div>
                <h6 className="text-white text-opacity-75 mb-2 small">VAT</h6>
                <h2 className="fw-bold text-white mb-0">25%</h2>
                <small className="text-white text-opacity-75">added to total</small>
              </div>
            </div>
          </div>
          
          <div className="p-4 rounded-3" style={{ background: '#f8f9fa' }}>
            <h6 className="fw-bold mb-3">💡 Payment Calculation Examples</h6>
            <div className="row g-3">
              <div className="col-md-6">
                <div className="d-flex align-items-start">
                  <span className="badge bg-primary me-2 mt-1">1h</span>
                  <div>
                    <div className="small text-muted">1-hour event</div>
                    <div className="fw-semibold">(750 × 1h + 300) × 1.25 = <span style={{ color: '#10b981' }}>1,312.50 SEK</span></div>
                  </div>
                </div>
              </div>
              <div className="col-md-6">
                <div className="d-flex align-items-start">
                  <span className="badge bg-primary me-2 mt-1">2h</span>
                  <div>
                    <div className="small text-muted">2-hour event</div>
                    <div className="fw-semibold">(750 × 2h + 300) × 1.25 = <span style={{ color: '#10b981' }}>2,250 SEK</span></div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
      </div>
    </div>
  );
}

export default ContributorProfile;
