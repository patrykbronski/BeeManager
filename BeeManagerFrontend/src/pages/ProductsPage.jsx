import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";
import "./Products.css";
import { useAppContext } from "../context/AppContext";

const products = [
  { id: 1, name: "Miód wielokwiatowy", price: "35 zł" },
  { id: 2, name: "Miód lipowy", price: "42 zł" },
  { id: 3, name: "Pyłek pszczeli", price: "28 zł" },
];

function ProductsPage() {
  const { addToCart, cartItems } = useAppContext();

  return (
    <>
      <Navbar />

      <div className="products-page">
        <div className="products-container">
          <h2>Lista produktów</h2>
          <p className="products-info">
            To jest osobny widok routingu. URL powinien być:
            <strong> /produkty</strong>
          </p>

          <p className="products-info">
            Liczba produktów w koszyku: <strong>{cartItems.length}</strong>
          </p>

          <div className="products-grid">
            {products.map((product) => (
              <div key={product.id} className="product-card">
                <h3>{product.name}</h3>
                <p>Cena: {product.price}</p>

                <div className="product-buttons">
                  <Link to={`/produkt/${product.id}`} className="product-link">
                    Zobacz szczegóły
                  </Link>

                  <button
                    type="button"
                    className="product-link secondary-btn"
                    onClick={() => addToCart(product)}
                  >
                    Dodaj do koszyka
                  </button>
                </div>
              </div>
            ))}
          </div>

          <div className="products-actions">
            <Link to="/" className="product-back">
              ← Wróć do strony głównej
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}

export default ProductsPage;
