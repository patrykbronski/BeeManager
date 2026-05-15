import { useEffect, useState } from "react";
import axios from "axios";
import Navbar from "../components/Navbar";
import "./ApiUsersPage.css";

function ApiUsersPage() {
  const [users, setUsers] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  useEffect(() => {
    axios
      .get("https://jsonplaceholder.typicode.com/users")
      .then((response) => {
        setUsers(response.data);
        setError("");
      })
      .catch(() => {
        setError("Nie udało się pobrać danych z API.");
      })
      .finally(() => {
        setLoading(false);
      });
  }, []);

  return (
    <>
      <Navbar />

      <div className="api-page">
        <div className="api-container">
          <h2>Użytkownicy z API</h2>

          <p className="api-info">
            Dane są pobierane dynamicznie z zewnętrznego REST API przy użyciu
            biblioteki <strong>Axios</strong>.
          </p>

          {loading && (
            <div className="api-message">
              <h3>Ładowanie danych...</h3>
              <p>Trwa pobieranie danych z API.</p>
            </div>
          )}

          {error && (
            <div className="api-message error">
              <h3>Błąd</h3>
              <p>{error}</p>
            </div>
          )}

          {!loading && !error && (
            <div className="api-grid">
              {users.map((user) => (
                <div key={user.id} className="api-card">
                  <h3>{user.name}</h3>
                  <p>
                    <strong>Email:</strong> {user.email}
                  </p>
                  <p>
                    <strong>Telefon:</strong> {user.phone}
                  </p>
                  <p>
                    <strong>Strona:</strong> {user.website}
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </>
  );
}

export default ApiUsersPage;
