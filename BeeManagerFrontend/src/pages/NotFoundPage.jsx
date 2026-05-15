import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";
import "./Products.css";

function NotFoundPage() {
  return (
    <>
      <Navbar />

      <div className="products-page">
        <div className="products-container">
          <h2>Błąd 404</h2>
          <p className="products-info">Taka ścieżka nie istnieje.</p>

          <div className="product-details">
            <p>
              Przykład działania trasy fallback: <strong>path="*"</strong>
            </p>
          </div>

          <div className="products-actions">
            <Link to="/" className="product-link">
              Wróć na stronę główną
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}

export default NotFoundPage;
