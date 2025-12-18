import axios from "axios";

const API_BASE_URL = "http://localhost:5000";

// Create axios instance with auth token
const createAuthAxios = () => {
  const token = localStorage.getItem("token");
  return axios.create({
    baseURL: API_BASE_URL,
    headers: token ? { Authorization: `Bearer ${token}` } : {},
  });
};

// Authentication
export const login = async (email, password) => {
  const response = await axios.post(`${API_BASE_URL}/auth/login`, {
    email,
    password,
  });
  if (response.data.token) {
    localStorage.setItem("token", response.data.token);
    localStorage.setItem("user", JSON.stringify(response.data));
  }
  return response.data;
};

export const register = async (userData) => {
  const response = await axios.post(`${API_BASE_URL}/auth/register`, userData);
  if (response.data.token) {
    localStorage.setItem("token", response.data.token);
    localStorage.setItem("user", JSON.stringify(response.data));
  }
  return response.data;
};

export const logout = () => {
  localStorage.removeItem("token");
  localStorage.removeItem("user");
};

export const getCurrentUser = () => {
  const userStr = localStorage.getItem("user");
  return userStr ? JSON.parse(userStr) : null;
};

export const isAuthenticated = () => {
  return !!localStorage.getItem("token");
};

export const isAdmin = () => {
  const user = getCurrentUser();
  return user?.roles?.includes("Admin") || false;
};

// Contributors
export const getMyProfile = async () => {
  const api = createAuthAxios();
  const response = await api.get("/contributors/me");
  return response.data;
};

export const getContributor = async (id) => {
  const api = createAuthAxios();
  const response = await api.get(`/contributors/${id}`);
  return response.data;
};

export const getAllContributors = async () => {
  const api = createAuthAxios();
  const response = await api.get("/contributors");
  return response.data;
};

export const createContributor = async (contributorData) => {
  const api = createAuthAxios();
  const response = await api.post("/contributors", contributorData);
  return response.data;
};

export const updateContributor = async (id, updates) => {
  const api = createAuthAxios();
  const response = await api.put(`/contributors/${id}`, updates);
  return response.data;
};

export const deleteContributor = async (id) => {
  const api = createAuthAxios();
  await api.delete(`/contributors/${id}`);
};

export const generatePayment = async (contributorId, year, month) => {
  const api = createAuthAxios();
  const response = await api.post(
    `/contributors/${contributorId}/payments/${year}/${month}`
  );
  return response.data;
};

export const markPaymentPaid = async (paymentId) => {
  const api = createAuthAxios();
  const response = await api.post(`/payments/${paymentId}/mark-paid`);
  return response.data;
};

export const createAssignment = async (assignmentData) => {
  const api = createAuthAxios();
  const response = await api.post("/assignments", assignmentData);
  return response.data;
};

export const deleteAssignment = async (assignmentId) => {
  const api = createAuthAxios();
  await api.delete(`/assignments/${assignmentId}`);
};

// Existing schedule functions
export const getTodaySchedule = async () => {
  const response = await axios.get(`${API_BASE_URL}/schedule/today`);
  return response.data.events;
};

export const getEventById = async (id) => {
  const response = await axios.get(`${API_BASE_URL}/events/${id}`);
  return response.data;
};

export const createEvent = async (eventData) => {
  const response = await axios.post(`${API_BASE_URL}/events`, eventData);
  return response.data;
};

export const deleteEvent = async (id) => {
  const response = await axios.post(`${API_BASE_URL}/events/${id}/delete`);
  return response.data;
};
