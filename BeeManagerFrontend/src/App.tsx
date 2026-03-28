import "./App.css";
import { useEffect, useState } from "react";
import api from "./api.ts";

function App() {
    const [uleCount, setUleCount] = useState<number | null>(null);
    const [apiError, setApiError] = useState("");

    useEffect(() => {
        api.get("/api/UleApi")
            .then((response) => {
                console.log(response.data) // pokazuje dane w konsoli
                setUleCount(response.data.length);
            })
            .catch((error) => {
                console.error("Błąd połączenia z backendem:", error);
                setApiError("Nie udało się połączyć z backendem.");
            });
    }, []);

    return (
        <div className="app">
            <header className="header">
                <div className="header-inner">
                        
                        <div className="logo">
                        <span>🐝</span>
                        <h1>BeeManager</h1>
                        </div>

                        <nav className="nav-right">
                        <a href="#">Pasieki</a>
                        <a href="#">Ule</a>
                        <a href="#">Przeglądy</a>
                        <a href="#">Miodobrania</a>
                        </nav>

                    </div>
            </header>

            <section className="hero">
                <div className="container">
                    <h1>Zarządzaj swoją pasieką łatwo</h1>
                    <p>
                        BeeManager to aplikacja pomagająca pszczelarzom kontrolować ule,
                        przeglądy oraz zbiory miodu w jednym miejscu.
                    </p>

                    <div className="buttons">
                        <button className="primary">Rozpocznij</button>
                        <button className="secondary">Dowiedz się więcej</button>
                    </div>
                </div>
            </section>

            <section className="features">
                <div className="container grid">
                            <div className="card" style={{ gridColumn: "1 / -1" }}>
                                <h3>🔌 Status połączenia</h3>
                                {apiError ? (
                                    <p>{apiError}</p>
                                ) : uleCount !== null ? (
                                    <p>Połączono z backendem. Liczba uli w bazie: {uleCount}</p>
                                ) : (
                                    <p>Trwa łączenie z backendem...</p>
                                )}
                            </div>
                    <div className="card">
                        <h3>🐝 Ule</h3>
                        <p>Kontroluj stan uli i rodzin pszczelich.</p>
                    </div>

                    <div className="card">
                        <h3>📋 Przeglądy</h3>
                        <p>Zapisuj przeglądy i obserwacje.</p>
                    </div>

                    <div className="card">
                        <h3>🍯 Miodobrania</h3>
                        <p>Śledź zbiory miodu i historię sezonów.</p>
                    </div>
                </div>
            </section>

            <footer>
                <p>© 2026 BeeManager</p>
            </footer>
        </div>
    );
}

export default App;