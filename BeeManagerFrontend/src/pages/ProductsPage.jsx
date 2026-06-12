import { Link } from "react-router-dom";
import Navbar from "../components/Navbar";
import "./Products.css";
import { useAppContext } from "../context/AppContext";
import { trackEvent } from "../analytics";
import { useState } from "react";

const products = [
  { id: 1, name: "Miód wielokwiatowy", price: "35 zł" },
  { id: 2, name: "Miód lipowy", price: "42 zł" },
  { id: 3, name: "Pyłek pszczeli", price: "28 zł" },
];

function ProductsPage() {
  const { addToCart, cartItems, isAuthenticated } = useAppContext();
  const [cartMessage, setCartMessage] = useState("");

  return (
    <>
      <Navbar />

      <div className="products-page">
        <div className="products-container">
          <h2>Lista produktów</h2>
          <p className="products-info">
            
           
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
                    onClick={async () => {
                      if (!isAuthenticated) {
                        setCartMessage("Musisz się zalogować, żeby dodać produkt do koszyka.");
                        return;
                      }

                      setCartMessage("");
                      await addToCart(product);
                      trackEvent("add_to_cart", {
                        item_name: product.name,
                        value: product.price,
                      });
                    }}
                  >
                    Dodaj do koszyka
                  </button>
                </div>
              </div>
            ))}
          </div>

          {cartMessage && <p className="form-error cart-error">{cartMessage}</p>}

          <div className="products-actions">
            <Link to="/" className="product-back">
               Wróć do strony głównej
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}

export default ProductsPage;
