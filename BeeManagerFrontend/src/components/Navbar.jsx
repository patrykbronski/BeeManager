import { Link, useNavigate } from "react-router-dom";
import "./Navbar.css";
import { useAppContext } from "../context/AppContext";

function Navbar() {
  const navigate = useNavigate();
  const { darkMode, toggleDarkMode, cartItems, currentUser, isAuthenticated, isAdmin, logout } =
    useAppContext();

  const goToSection = (sectionId) => {
    sessionStorage.setItem("scrollTarget", sectionId);
    navigate("/");
  };

  const handleLogout = () => {
    logout();
    navigate("/");
  };

  return (
    <header className="header">
      <div className="header-inner">
        <div className="logo">
          <span className="logo-icon">🐝</span>
          <h1>BeeManager</h1>
        </div>

        <nav className="nav-right">
          <Link to="/">Dashboard</Link>
          <Link to="/pasieki">Pasieki</Link>
          {isAdmin && <Link to="/panel-admina">Admin</Link>}
          <Link to="/produkty">Produkty</Link>
          <Link to="/api-users">API</Link>

          <button
            type="button"
            className="nav-link-btn"
            onClick={() => goToSection("features")}
          >
            Funkcje
          </button>

          <button
            type="button"
            className="nav-link-btn"
            onClick={() => goToSection("about")}
          >
            O aplikacji
          </button>

          <Link to="/koszyk" className="cart-link">
            Koszyk ({cartItems.length})
          </Link>
        </nav>

        <div className="auth-buttons">
          {isAuthenticated && currentUser && (
            <span className="user-chip">👤 {currentUser.fullName}</span>
          )}

          <button
            type="button"
            className="theme-toggle"
            onClick={toggleDarkMode}
          >
            {darkMode ? "☀ Jasny" : "🌙 Ciemny"}
          </button>

          {isAuthenticated ? (
            <button type="button" className="btn btn-outline" onClick={handleLogout}>
              Wyloguj
            </button>
          ) : (
            <>
              <Link to="/login" className="btn btn-outline">
                Zaloguj
              </Link>

              <Link to="/register" className="btn btn-primary">
                Zarejestruj
              </Link>
            </>
          )}
        </div>
      </div>
    </header>
  );
}

export default Navbar;
