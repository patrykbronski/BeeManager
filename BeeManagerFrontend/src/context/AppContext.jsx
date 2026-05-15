/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useContext,
  useEffect,
  useMemo,
  useState,
} from "react";
import { beeApi, setAuthToken } from "../api/beeApi";

const AppContext = createContext(undefined);

export function AppProvider({ children }) {
  const [authLoading, setAuthLoading] = useState(true);
  const [accessToken, setAccessToken] = useState(() =>
    localStorage.getItem("bee_access_token")
  );
  const [refreshToken, setRefreshToken] = useState(() =>
    localStorage.getItem("bee_refresh_token")
  );
  const [currentUser, setCurrentUser] = useState(() => {
    const raw = localStorage.getItem("bee_current_user");
    return raw ? JSON.parse(raw) : null;
  });
  const [darkMode, setDarkMode] = useState(() => {
    return localStorage.getItem("theme") === "dark";
  });

  const [cartItems, setCartItems] = useState(() => {
    const saved = localStorage.getItem("cartItems");
    return saved ? JSON.parse(saved) : [];
  });

  useEffect(() => {
    if (darkMode) {
      document.body.classList.add("dark");
      localStorage.setItem("theme", "dark");
    } else {
      document.body.classList.remove("dark");
      localStorage.setItem("theme", "light");
    }
  }, [darkMode]);

  useEffect(() => {
    setAuthToken(accessToken);
    if (accessToken) {
      localStorage.setItem("bee_access_token", accessToken);
    } else {
      localStorage.removeItem("bee_access_token");
    }
  }, [accessToken]);

  useEffect(() => {
    if (refreshToken) {
      localStorage.setItem("bee_refresh_token", refreshToken);
    } else {
      localStorage.removeItem("bee_refresh_token");
    }
  }, [refreshToken]);

  useEffect(() => {
    if (currentUser) {
      localStorage.setItem("bee_current_user", JSON.stringify(currentUser));
    } else {
      localStorage.removeItem("bee_current_user");
    }
  }, [currentUser]);

  useEffect(() => {
    let alive = true;

    async function loadCurrentUser() {
      if (!accessToken) {
        if (alive) {
          setAuthLoading(false);
        }
        return;
      }

      try {
        const response = await beeApi.get("/auth/me");
        if (alive) {
          setCurrentUser(response.data);
        }
      } catch {
        if (alive) {
          setAccessToken(null);
          setRefreshToken(null);
          setCurrentUser(null);
        }
      } finally {
        if (alive) {
          setAuthLoading(false);
        }
      }
    }

    loadCurrentUser();

    return () => {
      alive = false;
    };
  }, [accessToken]);

  useEffect(() => {
    localStorage.setItem("cartItems", JSON.stringify(cartItems));
  }, [cartItems]);

  const toggleDarkMode = () => {
    setDarkMode((prev) => !prev);
  };

  const addToCart = (product) => {
    setCartItems((prev) => [...prev, product]);
  };

  const removeFromCart = (id) => {
    setCartItems((prev) => {
      const index = prev.findIndex((item) => item.id === id);
      if (index === -1) return prev;

      const updated = [...prev];
      updated.splice(index, 1);
      return updated;
    });
  };

  const clearCart = () => {
    setCartItems([]);
  };

  const setSession = ({ accessToken: nextAccessToken, refreshToken: nextRefreshToken, user }) => {
    setAccessToken(nextAccessToken || null);
    setRefreshToken(nextRefreshToken || null);
    setCurrentUser(user || null);
    if (nextAccessToken) {
      setAuthToken(nextAccessToken);
    }
  };

  const logout = () => {
    setAccessToken(null);
    setRefreshToken(null);
    setCurrentUser(null);
    setAuthToken(null);
  };

  const isAuthenticated = Boolean(accessToken && currentUser);
  const isAdmin = currentUser?.roles?.includes("Admin") || false;

  const hasRole = (role) => currentUser?.roles?.includes(role) || false;

  const value = useMemo(
    () => ({
      authLoading,
      accessToken,
      refreshToken,
      currentUser,
      isAuthenticated,
      isAdmin,
      hasRole,
      setSession,
      logout,
      darkMode,
      toggleDarkMode,
      cartItems,
      addToCart,
      removeFromCart,
      clearCart,
    }),
    [
      authLoading,
      accessToken,
      refreshToken,
      currentUser,
      isAuthenticated,
      isAdmin,
      hasRole,
      darkMode,
      cartItems,
    ]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}

export function useAppContext() {
  const context = useContext(AppContext);

  if (!context) {
    throw new Error("useAppContext must be used inside AppProvider");
  }

  return context;
}
