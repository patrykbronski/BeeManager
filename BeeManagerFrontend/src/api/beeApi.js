import axios from "axios";

const baseURL = import.meta.env.VITE_API_BASE_URL || "/api";

export const beeApi = axios.create({
  baseURL,
  headers: {
    "Content-Type": "application/json",
  },
});

export function setAuthToken(token) {
  if (token) {
    beeApi.defaults.headers.common.Authorization = `Bearer ${token}`;
  } else {
    delete beeApi.defaults.headers.common.Authorization;
  }
}

