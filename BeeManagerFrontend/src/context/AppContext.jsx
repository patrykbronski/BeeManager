/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useReducer,
} from "react";
import { beeApi, setAuthToken } from "../api/beeApi";

const AppContext = createContext(undefined);

export function useAppContext() {
  const context = useContext(AppContext);

  if (!context) {
    throw new Error("useAppContext must be used inside AppProvider");
  }

  return context;
}

const initialState = {
  authLoading: true,
  accessToken: null,
  currentUser: null,
  darkMode: false,
  cartItems: [],
};

function appReducer(state, action) {
  switch (action.type) {
    case "SET_AUTH_LOADING":
      return { ...state, authLoading: action.payload };
    case "SET_SESSION":
      return {
        ...state,
        accessToken: action.payload.accessToken,
        currentUser: action.payload.user,
      };
    case "SET_CART_ITEMS":
      return {
        ...state,
        cartItems: action.payload,
      };
    case "CLEAR_AUTH":
      return {
        ...state,
        accessToken: null,
        currentUser: null,
        cartItems: [],
      };
    case "TOGGLE_DARK_MODE":
      return { ...state, darkMode: !state.darkMode };
    case "ADD_TO_CART":
      return { ...state, cartItems: [...state.cartItems, action.payload] };
    case "REMOVE_FROM_CART": {
      const index = state.cartItems.findIndex(
        (item) => item.id === action.payload
      );
      if (index === -1) {
        return state;
      }

      const updated = [...state.cartItems];
      updated.splice(index, 1);
      return { ...state, cartItems: updated };
    }
    case "CLEAR_CART":
      return { ...state, cartItems: [] };
    default:
      return state;
  }
}

export function AppProvider({ children }) {
  const [state, dispatch] = useReducer(appReducer, initialState);

  useEffect(() => {
    document.body.classList.toggle("dark", state.darkMode);
  }, [state.darkMode]);

  useEffect(() => {
    setAuthToken(state.accessToken);
  }, [state.accessToken]);

  useEffect(() => {
    let alive = true;

    async function bootstrapAuth() {
      try {
        const response = await beeApi.post("/auth/refresh", {});
        if (!alive) {
          return;
        }

        dispatch({
          type: "SET_SESSION",
          payload: {
            accessToken: response.data.accessToken,
            user: response.data.user,
          },
        });
        setAuthToken(response.data.accessToken);
      } catch {
        if (alive) {
          dispatch({ type: "CLEAR_AUTH" });
          setAuthToken(null);
        }
      } finally {
        if (alive) {
          dispatch({ type: "SET_AUTH_LOADING", payload: false });
        }
      }
    }

    bootstrapAuth();

    return () => {
      alive = false;
    };
  }, []);

  useEffect(() => {
    let alive = true;

    async function loadCart() {
      if (!state.accessToken) {
        dispatch({ type: "SET_CART_ITEMS", payload: [] });
        return;
      }

      try {
        const response = await beeApi.get("/cart");
        if (alive) {
          dispatch({ type: "SET_CART_ITEMS", payload: response.data });
        }
      } catch {
        if (alive) {
          dispatch({ type: "SET_CART_ITEMS", payload: [] });
        }
      }
    }

    if (!state.authLoading) {
      loadCart();
    }

    return () => {
      alive = false;
    };
  }, [state.accessToken, state.authLoading]);

  const toggleDarkMode = () => {
    dispatch({ type: "TOGGLE_DARK_MODE" });
  };

  const addToCart = async (product) => {
    await beeApi.post("/cart/items", product);
    dispatch({ type: "ADD_TO_CART", payload: product });
  };

  const removeFromCart = async (id) => {
    await beeApi.delete(`/cart/items/${id}`);
    dispatch({ type: "REMOVE_FROM_CART", payload: id });
  };

  const clearCart = async () => {
    await beeApi.delete("/cart/clear");
    dispatch({ type: "CLEAR_CART" });
  };

  const setSession = ({ accessToken, user }) => {
    dispatch({
      type: "SET_SESSION",
      payload: {
        accessToken: accessToken || null,
        user: user || null,
      },
    });
    setAuthToken(accessToken || null);
  };

  const logout = useCallback(async () => {
    try {
      await beeApi.post("/auth/logout", {});
    } catch {
      // Ignore backend logout failures and clear the client state anyway.
    } finally {
      dispatch({ type: "CLEAR_AUTH" });
      setAuthToken(null);
    }
  }, []);

  const isAuthenticated = Boolean(state.accessToken && state.currentUser);
  const isAdmin = state.currentUser?.roles?.includes("Admin") || false;

  const hasRole = useCallback(
    (role) => state.currentUser?.roles?.includes(role) || false,
    [state.currentUser]
  );

  const value = useMemo(
    () => ({
      authLoading: state.authLoading,
      accessToken: state.accessToken,
      currentUser: state.currentUser,
      isAuthenticated,
      isAdmin,
      hasRole,
      setSession,
      logout,
      darkMode: state.darkMode,
      toggleDarkMode,
      cartItems: state.cartItems,
      addToCart,
      removeFromCart,
      clearCart,
    }),
    [
      state.authLoading,
      state.accessToken,
      state.currentUser,
      isAuthenticated,
      isAdmin,
      hasRole,
      logout,
      state.darkMode,
      state.cartItems,
    ]
  );

  return <AppContext.Provider value={value}>{children}</AppContext.Provider>;
}


