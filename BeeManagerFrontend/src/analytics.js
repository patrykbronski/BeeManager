import ReactGA from "react-ga4";


const MEASUREMENT_ID = "G-0240DKKRJ8";

ReactGA.initialize(MEASUREMENT_ID);

export const trackPageView = (path) => {
  ReactGA.send({ hitType: "pageview", page: path });
};

export const trackEvent = (action, params = {}) => {
  ReactGA.event(action, params);
};
