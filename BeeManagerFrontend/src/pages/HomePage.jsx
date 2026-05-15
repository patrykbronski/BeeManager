import { Link, useLocation } from "react-router-dom";
import { useEffect } from "react";
import Navbar from "../components/Navbar";
import "./HomePage.css";
import { useAppContext } from "../context/AppContext";

function HomePage() {
  const location = useLocation();
  const { cartItems } = useAppContext();

  useEffect(() => {
    const target = sessionStorage.getItem("scrollTarget");

    if (target) {
      setTimeout(() => {
        const element = document.getElementById(target);
        if (element) {
          element.scrollIntoView({ behavior: "smooth" });
        }
        sessionStorage.removeItem("scrollTarget");
      }, 100);
    }
  }, [location]);

  return (
    <div className="page">
      <Navbar />

      <section className="hero">
        <div className="hero-content">
          <h2>Zarządzaj swoją pasieką łatwo</h2>
          <p>
            BeeManager to aplikacja pomagająca pszczelarzom kontrolować ule,
            przeglądy oraz zbiory miodu w jednym miejscu.
          </p>

          <p style={{ fontWeight: 700, marginTop: "16px" }}>
            Produkty w koszyku: {cartItems.length}
          </p>

          <div className="hero-buttons">
            <Link to="/produkty" className="hero-btn hero-btn-primary">
              Zobacz produkty
            </Link>

            <button
              type="button"
              className="hero-btn hero-btn-secondary"
              onClick={() => {
                const el = document.getElementById("about");
                el?.scrollIntoView({ behavior: "smooth" });
              }}
            >
              Dowiedz się więcej
            </button>
          </div>
        </div>
      </section>

      <section id="features" className="features-section">
        <div className="cards">
          <div className="feature-card">
            <h3>🫙 Produkty</h3>
            <p>Przeglądaj produkty i dodawaj je do globalnego koszyka.</p>
          </div>
        </div>
      </section>

      <section id="about" className="about-section">
        <div className="about-box">
          <h3>Dlaczego BeeManager?</h3>
          <p>
            Dzięki aplikacji możesz w jednym miejscu zakupić pyszny miodzik.
          </p>

          <div className="hero-buttons">
            <Link to="/produkty" className="hero-btn hero-btn-primary">
              Idź do produktów
            </Link>

            <Link to="/koszyk" className="hero-btn hero-btn-secondary">
              Otwórz koszyk
            </Link>
          </div>
        </div>
      </section>
    </div>
  );
}

export default HomePage;

