/* eslint-disable no-unused-vars */
import { useCallback, useEffect, useMemo, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import Navbar from "../components/Navbar";
import { beeApi } from "../api/beeApi";
import { useAppContext } from "../context/AppContext";
import "./ApiariesPage.css";
import "../styles/backend.css";

const emptyApiary = {
  nazwa: "",
  lokalizacja: "",
  opis: "",
};

function ApiariesPage() {
  const navigate = useNavigate();
  const { currentUser, hasRole, isAuthenticated, authLoading } = useAppContext();
  const [apiaries, setApiaries] = useState([]);
  const [discoveries, setDiscoveries] = useState([]);
  const [query, setQuery] = useState("");
  const [form, setForm] = useState(emptyApiary);
  const [joinMessageByApiary, setJoinMessageByApiary] = useState({});
  const [expandedDiscoveries, setExpandedDiscoveries] = useState({});
  const [joinStatusByApiary, setJoinStatusByApiary] = useState({});
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);

  const canCreateApiary = hasRole("Admin") || hasRole("Owner");
  const canDiscover = hasRole("Worker") || hasRole("Inspector");

  async function loadApiaries() {
    const response = await beeApi.get("/apiaries");
    setApiaries(response.data);
  }

  const loadDiscoveries = useCallback(
    async (search = "") => {
      if (!canDiscover) {
        setDiscoveries([]);
        return;
      }

      const response = await beeApi.get("/apiaries/discover", {
        params: search ? { query: search } : {},
      });
      setDiscoveries(
        response.data.map((item) => {
          const localStatus = joinStatusByApiary[item.id];
          return localStatus === undefined
            ? item
            : {
                ...item,
                membershipStatus: localStatus,
              };
        })
      );
    },
    [canDiscover, joinStatusByApiary]
  );

  useEffect(() => {
    let alive = true;

    async function load() {
      if (authLoading) {
        return;
      }

      if (!isAuthenticated) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        await loadApiaries();
        await loadDiscoveries();
        if (alive) {
          setError("");
        }
      } catch (requestError) {
        if (alive) {
          setError(
            requestError?.response?.data?.message ||
              "Nie udało się pobrać pasiek."
          );
        }
      } finally {
        if (alive) {
          setLoading(false);
        }
      }
    }

    load();
    return () => {
      alive = false;
    };
  }, [authLoading, isAuthenticated, loadDiscoveries]);

  const stats = useMemo(
    () => [
      { label: "Pasieki", value: apiaries.length },
      {
        label: "Zatwierdzone członkostwa",
        value: apiaries.filter((item) => item.membershipRole).length,
      },
      {
        label: "Dołączone odkrycia",
        value: discoveries.filter((item) => item.membershipStatus === "Approved")
          .length,
      },
    ],
    [apiaries, discoveries]
  );

  async function handleCreateApiary(event) {
    event.preventDefault();
    try {
      setSaving(true);
      await beeApi.post("/apiaries", form);
      setForm(emptyApiary);
      await loadApiaries();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się utworzyć pasieki."
      );
    } finally {
      setSaving(false);
    }
  }

  function expandDiscovery(apiaryId) {
    setExpandedDiscoveries((prev) => ({
      ...prev,
      [apiaryId]: true,
    }));
  }

  function collapseDiscovery(apiaryId) {
    setExpandedDiscoveries((prev) => ({
      ...prev,
      [apiaryId]: false,
    }));
  }

  async function handleJoin(apiaryId) {
    try {
      setSaving(true);
      await beeApi.post(`/apiaries/${apiaryId}/join-requests`, {
        message: joinMessageByApiary[apiaryId] || "",
      });
      setJoinMessageByApiary((prev) => ({
        ...prev,
        [apiaryId]: "",
      }));
      collapseDiscovery(apiaryId);
      setJoinStatusByApiary((prev) => ({
        ...prev,
        [apiaryId]: "Pending",
      }));
      setDiscoveries((prev) =>
        prev.map((item) =>
          item.id === apiaryId
            ? {
                ...item,
                membershipStatus: "Pending",
            }
            : item
        )
      );
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się wysłać prośby o dołączenie."
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleCancelJoin(apiaryId) {
    const confirmed = window.confirm("Anulować prośbę o dołączenie?");
    if (!confirmed) {
      return;
    }

      try {
      setSaving(true);
      await beeApi.delete(`/apiaries/${apiaryId}/join-requests`);
      setJoinStatusByApiary((prev) => ({
        ...prev,
        [apiaryId]: null,
      }));
      setDiscoveries((prev) =>
        prev.map((item) =>
          item.id === apiaryId
            ? {
                ...item,
                membershipStatus: null,
                joinRequestId: null,
            }
            : item
        )
      );
      collapseDiscovery(apiaryId);
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się anulować prośby o dołączenie."
      );
    } finally {
      setSaving(false);
    }
  }

  async function handleSearch(event) {
    event.preventDefault();
    try {
      setLoading(true);
      await loadDiscoveries(query);
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się wyszukać pasiek."
      );
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <Navbar />

      <div className="backend-shell">
        <section className="backend-hero">
          <h2>Pasieki i dostęp do backendu</h2>
          <p>
            To jest właściwy moduł aplikacji BeeManager. Tutaj widać pasieki,
            można zakładać nowe, wysyłać prośby o dołączenie i przechodzić do
            szczegółów pasieki, gdzie są ule, miodobrania, przeglądy i
            członkostwa.
          </p>
          {currentUser?.fullName && (
            <p style={{ marginTop: 12, fontWeight: 700 }}>
              Zalogowano jako {currentUser.fullName}
            </p>
          )}

          <div className="backend-toolbar">
            <Link to="/" className="backend-ghost">
              Dashboard
            </Link>
            <Link to="/panel-admina" className="backend-ghost">
              Panel admina
            </Link>
          </div>
        </section>

        {authLoading ? (
          <div className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-empty">Sprawdzanie sesji...</div>
          </div>
        ) : !isAuthenticated ? (
          <div className="backend-section" style={{ marginTop: 18 }}>
            <h3>Wymagane logowanie</h3>
            <p className="backend-muted">
              Zaloguj się, aby zobaczyć pasieki i korzystać z funkcji backendu.
            </p>
          </div>
        ) : (
          <>
            <section className="backend-grid cols-3" style={{ marginTop: 18 }}>
              {stats.map((item) => (
                <div key={item.label} className="backend-card">
                  <div className="backend-kpi">
                    <span className="backend-muted">{item.label}</span>
                    <strong>{item.value}</strong>
                  </div>
                </div>
              ))}
            </section>

            {canCreateApiary && (
              <section className="backend-section" style={{ marginTop: 18 }}>
                <div className="backend-section-header">
                  <div>
                    <h3>Nowa pasieka</h3>
                    <p className="backend-muted">
                      Właściciel lub administrator może dodać nową pasiekę.
                    </p>
                  </div>
                </div>

                <form className="backend-form grid-2" onSubmit={handleCreateApiary}>
                  <div className="backend-field">
                    <label>Nazwa</label>
                    <input
                      value={form.nazwa}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, nazwa: event.target.value }))
                      }
                      required
                      maxLength={150}
                    />
                  </div>
                  <div className="backend-field">
                    <label>Lokalizacja</label>
                    <input
                      value={form.lokalizacja}
                      onChange={(event) =>
                        setForm((prev) => ({
                          ...prev,
                          lokalizacja: event.target.value,
                        }))
                      }
                      maxLength={255}
                    />
                  </div>
                  <div className="backend-field" style={{ gridColumn: "1 / -1" }}>
                    <label>Opis</label>
                    <textarea
                      value={form.opis}
                      onChange={(event) =>
                        setForm((prev) => ({ ...prev, opis: event.target.value }))
                      }
                      maxLength={1000}
                    />
                  </div>
                  <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                    <button type="submit" className="backend-btn" disabled={saving}>
                      {saving ? "Zapisywanie..." : "Utwórz pasiekę"}
                    </button>
                  </div>
                </form>
              </section>
            )}

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Twoje pasieki</h3>
                  <p className="backend-muted">
                    Kliknij pasiekę, aby przejść do pełnego zarządzania.
                  </p>
                </div>
              </div>

              {loading ? (
                <div className="backend-empty">Ładowanie...</div>
              ) : apiaries.length === 0 ? (
                <div className="backend-empty">Brak dostępnych pasiek.</div>
              ) : (
                <div className="backend-grid cols-2">
                  {apiaries.map((apiary) => (
                    <article key={apiary.id} className="backend-card">
                      <div
                        className="backend-section-header"
                        style={{ marginBottom: 10 }}
                      >
                        <div>
                          <h4>{apiary.nazwa}</h4>
                          <p className="backend-muted">
                            {apiary.lokalizacja || "Brak lokalizacji"}
                          </p>
                        </div>
                        <span className="backend-pill">
                          {apiary.isOwner ? "Właściciel" : apiary.membershipRole || "Dostęp"}
                        </span>
                      </div>

                      <p className="backend-muted">
                        {apiary.opis || "Brak opisu pasieki."}
                      </p>

                      <div className="backend-actions" style={{ marginTop: 14 }}>
                        <button
                          type="button"
                          className="backend-btn"
                          onClick={() => navigate(`/pasieki/${apiary.id}`)}
                        >
                          Otwórz
                        </button>
                      </div>
                    </article>
                  ))}
                </div>
              )}
            </section>

            {canDiscover && (
              <section className="backend-section" style={{ marginTop: 18 }}>
                <div className="backend-section-header">
                  <div>
                    <h3>Odkryj pasieki</h3>
                    <p className="backend-muted">
                      Funkcja dla pracowników i inspektorów. Możesz wysłać
                      prośbę o dołączenie.
                    </p>
                  </div>

                  <form className="backend-actions" onSubmit={handleSearch}>
                    <input
                      value={query}
                      onChange={(event) => setQuery(event.target.value)}
                      placeholder="Szukaj po nazwie lub lokalizacji"
                      style={{ minWidth: 280 }}
                    />
                    <button type="submit" className="backend-btn">
                      Szukaj
                    </button>
                  </form>
                </div>

                <div className="discover-grid">
                  {discoveries.map((apiary) => (
                    <article key={apiary.id} className="backend-card discover-card">
                      <div className="discover-card-main">
                        <div className="discover-card-topline">
                          <h4>{apiary.nazwa}</h4>
                          {apiary.membershipStatus === "Pending" ? (
                            <span className="backend-pill warn">Prośba wysłana</span>
                          ) : apiary.membershipStatus === "Approved" ? (
                            <span className="backend-pill success">Dołączono</span>
                          ) : null}
                        </div>

                        <div className="discover-card-meta">
                          <span>
                            <strong>Właściciel:</strong> {apiary.ownerName || "-"}
                          </span>
                          <span>
                            <strong>Lokalizacja:</strong>{" "}
                            {apiary.lokalizacja || "Brak lokalizacji"}
                          </span>
                        </div>
                      </div>

                      <div className="discover-card-actions">
                        {apiary.membershipStatus === "Pending" ? (
                          <>
                            <p className="backend-muted discover-card-note">
                              Wysłano prośbę o dołączenie do tej pasieki.
                            </p>
                            <button
                              type="button"
                              className="backend-ghost"
                              onClick={() => handleCancelJoin(apiary.id)}
                              disabled={saving}
                            >
                              Anuluj prośbę
                            </button>
                          </>
                        ) : apiary.membershipStatus === "Approved" ? (
                          <p className="backend-muted discover-card-note">
                            Jesteś już członkiem tej pasieki.
                          </p>
                        ) : expandedDiscoveries[apiary.id] ? (
                          <>
                            <div className="backend-field discover-message-field">
                              <label>Wiadomość do właściciela</label>
                              <textarea
                                value={joinMessageByApiary[apiary.id] || ""}
                                onChange={(event) =>
                                  setJoinMessageByApiary((prev) => ({
                                    ...prev,
                                    [apiary.id]: event.target.value,
                                  }))
                                }
                                maxLength={500}
                              />
                            </div>

                            <div className="backend-actions discover-card-buttons">
                              <button
                                type="button"
                                className="backend-btn"
                                onClick={() => handleJoin(apiary.id)}
                                disabled={saving}
                              >
                                Wyślij prośbę
                              </button>
                              <button
                                type="button"
                                className="backend-ghost"
                                onClick={() => collapseDiscovery(apiary.id)}
                                disabled={saving}
                              >
                                Zwiń
                              </button>
                            </div>
                          </>
                        ) : (
                          <button
                            type="button"
                            className="backend-btn"
                            onClick={() => expandDiscovery(apiary.id)}
                          >
                            Dołącz
                          </button>
                        )}
                      </div>
                    </article>
                  ))}
                </div>
              </section>
            )}
          </>
        )}

        {error && (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-pill danger">{error}</div>
          </section>
        )}
      </div>
    </>
  );
}

export default ApiariesPage;
