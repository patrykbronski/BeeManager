import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import * as Sentry from "@sentry/react";
import App from "./App";
import "./index.css";
import { AppProvider } from "./context/AppContext";

Sentry.init({
  dsn: "https://0fb8a8fd3ceceea9592d3b93a09018c2@o4511394919612416.ingest.de.sentry.io/4511394924134480",
  tracesSampleRate: 1.0,
});

ReactDOM.createRoot(document.getElementById("root")).render(
  <React.StrictMode>
    <BrowserRouter>
      <AppProvider>
        <Sentry.ErrorBoundary fallback={<p>Wystąpił nieoczekiwany błąd aplikacji.</p>}>
          <App />
        </Sentry.ErrorBoundary>
      </AppProvider>
    </BrowserRouter>
  </React.StrictMode>
);
