import React from "react";
import ReactDOM from "react-dom/client";
import { BrowserRouter } from "react-router-dom";
import * as Sentry from "@sentry/react";
import App from "./App";
import "./index.css";
import { AppProvider } from "./context/AppContext";
 
Sentry.init({
  dsn: "https://d48990a1463b626308eb2fce5b6bac60@o4511394917580800.ingest.de.sentry.io/4511553190101072",
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
 