import { Routes, Route } from "react-router-dom";
import HomePage from "./pages/HomePage";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ProductsPage from "./pages/ProductsPage";
import ProductDetailsPage from "./pages/ProductDetailsPage";
import NotFoundPage from "./pages/NotFoundPage";
import CartPage from "./pages/CartPage";
import ApiUsersPage from "./pages/ApiUsersPage";
import ApiariesPage from "./pages/ApiariesPage";
import ApiaryDetailsPage from "./pages/ApiaryDetailsPage";
import AdminPage from "./pages/AdminPage";

function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/login" element={<LoginPage />} />
      <Route path="/register" element={<RegisterPage />} />
      <Route path="/produkty" element={<ProductsPage />} />
      <Route path="/produkt/:id" element={<ProductDetailsPage />} />
      <Route path="/koszyk" element={<CartPage />} />
      <Route path="/api-users" element={<ApiUsersPage />} />
      <Route path="/pasieki" element={<ApiariesPage />} />
      <Route path="/pasieki/:id" element={<ApiaryDetailsPage />} />
      <Route path="/panel-admina" element={<AdminPage />} />
      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}

export default App;
