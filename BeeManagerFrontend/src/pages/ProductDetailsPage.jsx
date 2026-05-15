import { Link, useParams } from "react-router-dom";
import Navbar from "../components/Navbar";
import "./Products.css";
import { useAppContext } from "../context/AppContext";

const products = [
  {
    id: 1,
    name: "Miód wielokwiatowy",
    price: "35 zł",
    description: "Naturalny miód z pasieki.",
  },
  {
    id: 2,
    name: "Miód lipowy",
    price: "42 zł",
    description: "Aromatyczny miód lipowy.",
  },
  {
    id: 3,
    name: "Pyłek pszczeli",
    price: "28 zł",
    description: "Bogaty w witaminy pyłek pszczeli.",
  },
];

function ProductDetailsPage() {
  const { id } = useParams();
  const { addToCart } = useAppContext();

  const product = products.find((item) => item.id === Number(id));

  return (
    <>
      <Navbar />

      <div className="products-page">
        <div className="products-container">
          <h2>Szczegóły produktu</h2>
          <p className="products-info">
            Parametr pobrany przez <strong>useParams()</strong>. Aktualne ID:
            <strong> {id}</strong>
          </p>

          {product ? (
            <div className="product-details">
              <h3>{product.name}</h3>
              <p>
                <strong>ID:</strong> {product.id}
              </p>
              <p>
                <strong>Cena:</strong> {product.price}
              </p>
              <p>
                <strong>Opis:</strong> {product.description}
              </p>

              <button
                type="button"
                className="product-link secondary-btn"
                onClick={() =>
                  addToCart({
                    id: product.id,
                    name: product.name,
                    price: product.price,
                  })
                }
              >
                Dodaj do koszyka
              </button>
            </div>
          ) : (
            <div className="product-details">
              <h3>Nie znaleziono produktu</h3>
              <p>Brak produktu o ID: {id}</p>
            </div>
          )}

          <div className="products-actions">
            <Link to="/produkty" className="product-link">
              ← Wróć do listy produktów
            </Link>
          </div>
        </div>
      </div>
    </>
  );
}

export default ProductDetailsPage;
