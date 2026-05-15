import Navbar from "../components/Navbar";
import "./Products.css";
import { useAppContext } from "../context/AppContext";
import { Link } from "react-router-dom";

function CartPage() {
  const { cartItems, removeFromCart, clearCart } = useAppContext();

  return (
    <>
      <Navbar />

      <div className="products-page">
        <div className="products-container">
          <h2>Koszyk</h2>

          {cartItems.length === 0 ? (
            <div className="product-details">
              <h3>Koszyk jest pusty</h3>
              <p>Dodaj produkt na stronie produktów.</p>
            </div>
          ) : (
            <>
              <div className="products-grid">
                {cartItems.map((item, index) => (
                  <div key={`${item.id}-${index}`} className="product-card">
                    <h3>{item.name}</h3>
                    <p>Cena: {item.price}</p>

                    <button
                      type="button"
                      className="product-link secondary-btn"
                      onClick={() => removeFromCart(item.id)}
                    >
                      Usuń z koszyka
                    </button>
                  </div>
                ))}
              </div>

              <div className="products-actions">
                <button
                  type="button"
                  className="product-back secondary-btn"
                  onClick={clearCart}
                >
                  Wyczyść koszyk
                </button>
              </div>
            </>
          )}

          <div className="products-actions">
            <Link to="/produkty" className="product-link">
              ← Wróć do produktów
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}

export default CartPage;
