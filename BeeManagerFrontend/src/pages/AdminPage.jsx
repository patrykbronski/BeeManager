import { useEffect, useState } from "react";
import Navbar from "../components/Navbar";
import { beeApi } from "../api/beeApi";
import { useAppContext } from "../context/AppContext";
import {
  accountStatusLabel,
  membershipRoleLabel,
  membershipStatusLabel,
  requestedRoleOptions,
  roleLabel,
} from "../utils/beeEnums";
import { formatDateTime } from "../utils/formatters";
import "../styles/backend.css";

function AdminPage() {
  const { isAuthenticated, isAdmin, authLoading } = useAppContext();
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [registrations, setRegistrations] = useState([]);
  const [users, setUsers] = useState([]);
  const [joinRequests, setJoinRequests] = useState([]);
  const [registrationForm, setRegistrationForm] = useState({});
  const [userRoles, setUserRoles] = useState({});
  const [saving, setSaving] = useState(false);

  async function loadAll() {
    const [registrationsResponse, usersResponse, joinRequestsResponse] =
      await Promise.all([
        beeApi.get("/admin/registrations", {
          params: { status: "Pending" },
        }),
        beeApi.get("/admin/users"),
        beeApi.get("/admin/join-requests"),
      ]);

    setRegistrations(registrationsResponse.data);
    setUsers(usersResponse.data);
    setJoinRequests(joinRequestsResponse.data);

    const nextForms = {};
    registrationsResponse.data.forEach((item) => {
      nextForms[item.userId] = {
        role: item.requestedRole,
        note: "",
      };
    });
    setRegistrationForm(nextForms);

    const nextUserRoles = {};
    usersResponse.data.forEach((item) => {
      nextUserRoles[item.userId] = item.requestedRole;
    });
    setUserRoles(nextUserRoles);
  }

  useEffect(() => {
    let alive = true;

    async function load() {
      if (authLoading) {
        return;
      }

      if (!isAuthenticated || !isAdmin) {
        setLoading(false);
        return;
      }

      try {
        setLoading(true);
        await loadAll();
        if (alive) {
          setError("");
        }
      } catch (requestError) {
        if (alive) {
          setError(
            requestError?.response?.data?.message ||
              "Nie udało się pobrać danych administratora."
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
  }, [authLoading, isAuthenticated, isAdmin]);

  async function approveRegistration(userId) {
    try {
      setSaving(true);
      const form = registrationForm[userId] || {};
      await beeApi.post(`/admin/registrations/${userId}/approve`, {
        role: form.role,
        note: form.note || "",
      });
      await loadAll();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zatwierdzić rejestracji."
      );
    } finally {
      setSaving(false);
    }
  }

  async function rejectRegistration(userId) {
    const reason = window.prompt("Powód odrzucenia");
    if (!reason) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.post(`/admin/registrations/${userId}/reject`, {
        reason,
      });
      await loadAll();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się odrzucić rejestracji."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteRegistration(userId) {
    const confirmed = window.confirm("Usunąć tę oczekującą rejestrację?");
    if (!confirmed) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/admin/registrations/${userId}`);
      await loadAll();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się usunąć rejestracji."
      );
    } finally {
      setSaving(false);
    }
  }

  async function updateUserRole(userId) {
    try {
      setSaving(true);
      await beeApi.put(`/admin/users/${userId}/role`, {
        role: userRoles[userId],
      });
      await loadAll();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zaktualizować roli."
      );
    } finally {
      setSaving(false);
    }
  }

  async function reviewJoinRequest(action, item) {
    const note = window.prompt(
      action === "approve" ? "Notatka decyzji" : "Powód odrzucenia"
    );

    try {
      setSaving(true);
      await beeApi.post(
        `/apiaries/${item.pasiekaId}/memberships/${item.id}/${action}`,
        {
          decisionNote: note || "",
        }
      );
      await loadAll();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się przetworzyć zgłoszenia."
      );
    } finally {
      setSaving(false);
    }
  }

  if (authLoading) {
    return (
      <>
        <Navbar />
        <div className="backend-shell">
          <section className="backend-section">
            <div className="backend-empty">Sprawdzanie sesji...</div>
          </section>
        </div>
      </>
    );
  }

  if (!isAuthenticated) {
    return (
      <>
        <Navbar />
        <div className="backend-shell">
          <section className="backend-section">
            <div className="backend-empty">Zaloguj się, aby zobaczyć panel admina.</div>
          </section>
        </div>
      </>
    );
  }

  if (!isAdmin) {
    return (
      <>
        <Navbar />
        <div className="backend-shell">
          <section className="backend-section">
            <div className="backend-empty">Brak uprawnień administratora.</div>
          </section>
        </div>
      </>
    );
  }

  return (
    <>
      <Navbar />

      <div className="backend-shell">
        <section className="backend-hero">
          <h2>Panel administratora</h2>
          <p>
            Tutaj są wszystkie procesy administracyjne backendu: przegląd
            rejestracji, użytkowników oraz zgłoszeń do pasiek.
          </p>
        </section>

        {loading ? (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-empty">Ładowanie...</div>
          </section>
        ) : error ? (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-pill danger">{error}</div>
          </section>
        ) : (
          <>
            <section className="backend-grid cols-3" style={{ marginTop: 18 }}>
              <div className="backend-card">
                <span className="backend-muted">Rejestracje</span>
                <strong>{registrations.length}</strong>
              </div>
              <div className="backend-card">
                <span className="backend-muted">Użytkownicy</span>
                <strong>{users.length}</strong>
              </div>
              <div className="backend-card">
                <span className="backend-muted">Zgłoszenia do pasiek</span>
                <strong>{joinRequests.length}</strong>
              </div>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Rejestracje</h3>
                  <p className="backend-muted">
                    Zatwierdzanie uwzględnia rolę oraz opcjonalną notatkę.
                  </p>
                </div>
              </div>

              <div className="backend-stack">
                {registrations.map((item) => (
                  <article key={item.userId} className="backend-card">
                    <div className="backend-section-header" style={{ marginBottom: 8 }}>
                      <div>
                        <h4>{item.fullName}</h4>
                        <p className="backend-muted">{item.email}</p>
                      </div>
                      <span className="backend-pill">{accountStatusLabel(item.accountStatus)}</span>
                    </div>

                    <p className="backend-muted">
                      Wymagana rola: {roleLabel(item.requestedRole)}
                    </p>
                    <p className="backend-muted">
                      Rejestracja: {formatDateTime(item.createdAtUtc)}
                    </p>
                    <p className="backend-muted">
                      Notatka użytkownika: {item.registrationNote || "-"}
                    </p>

                    <div className="backend-form grid-2" style={{ marginTop: 12 }}>
                      <div className="backend-field">
                        <label>Rola po zatwierdzeniu</label>
                        <select
                          value={registrationForm[item.userId]?.role || item.requestedRole}
                          onChange={(event) =>
                            setRegistrationForm((prev) => ({
                              ...prev,
                              [item.userId]: {
                                ...(prev[item.userId] || {}),
                                role: event.target.value,
                              },
                            }))
                          }
                        >
                          {requestedRoleOptions.map((option) => (
                            <option key={option.value} value={option.value}>
                              {option.label}
                            </option>
                          ))}
                        </select>
                      </div>
                      <div className="backend-field">
                        <label>Notatka administratora</label>
                        <input
                          value={registrationForm[item.userId]?.note || ""}
                          onChange={(event) =>
                            setRegistrationForm((prev) => ({
                              ...prev,
                              [item.userId]: {
                                ...(prev[item.userId] || {}),
                                note: event.target.value,
                              },
                            }))
                          }
                          maxLength={500}
                        />
                      </div>
                    </div>

                    <div className="backend-actions" style={{ marginTop: 12 }}>
                      <button
                        type="button"
                        className="backend-btn"
                        onClick={() => approveRegistration(item.userId)}
                        disabled={saving}
                      >
                        Zatwierdź
                      </button>
                      <button
                        type="button"
                        className="backend-ghost"
                        onClick={() => rejectRegistration(item.userId)}
                        disabled={saving}
                      >
                        Odrzuć
                      </button>
                      <button
                        type="button"
                        className="backend-danger"
                        onClick={() => deleteRegistration(item.userId)}
                        disabled={saving}
                      >
                        Usuń
                      </button>
                    </div>
                  </article>
                ))}

                {registrations.length === 0 && (
                  <div className="backend-empty">Brak rejestracji.</div>
                )}
              </div>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Użytkownicy</h3>
                  <p className="backend-muted">
                    Zmiana roli aktualizuje też status konta po stronie backendu.
                  </p>
                </div>
              </div>

              <table className="backend-table">
                <thead>
                  <tr>
                    <th>Imię i nazwisko</th>
                    <th>E-mail</th>
                    <th>Status</th>
                    <th>Rola</th>
                    <th>Notatka</th>
                    <th>Utworzono</th>
                    <th>Akcje</th>
                  </tr>
                </thead>
                <tbody>
                  {users.map((item) => (
                    <tr key={item.userId}>
                      <td>{item.fullName}</td>
                      <td>{item.email}</td>
                      <td>{accountStatusLabel(item.accountStatus)}</td>
                      <td>{item.roles?.join(", ") || roleLabel(item.requestedRole)}</td>
                      <td>{item.reviewNote || "-"}</td>
                      <td>{formatDateTime(item.createdAtUtc)}</td>
                      <td>
                        <div className="backend-actions">
                          <select
                            value={userRoles[item.userId] || item.requestedRole}
                            onChange={(event) =>
                              setUserRoles((prev) => ({
                                ...prev,
                                [item.userId]: event.target.value,
                              }))
                            }
                          >
                            {requestedRoleOptions.map((option) => (
                              <option key={option.value} value={option.value}>
                                {option.label}
                              </option>
                            ))}
                          </select>
                          <button
                            type="button"
                            className="backend-btn"
                            onClick={() => updateUserRole(item.userId)}
                            disabled={saving}
                          >
                            Zapisz
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}

                  {users.length === 0 && (
                    <tr>
                      <td colSpan="7">
                        <div className="backend-empty">Brak użytkowników.</div>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Zgłoszenia do pasiek</h3>
                  <p className="backend-muted">
                    Zgłoszenia z backendu można zatwierdzać albo odrzucać.
                  </p>
                </div>
              </div>

              <table className="backend-table">
                <thead>
                  <tr>
                    <th>Pasieka</th>
                    <th>Użytkownik</th>
                    <th>Rola</th>
                    <th>Status</th>
                    <th>Wiadomość</th>
                    <th>Akcje</th>
                  </tr>
                </thead>
                <tbody>
                  {joinRequests.map((item) => (
                    <tr key={item.id}>
                      <td>{item.pasiekaNazwa}</td>
                      <td>
                        {item.userName}
                        <div className="backend-muted">{item.email}</div>
                      </td>
                      <td>{membershipRoleLabel(item.membershipRole)}</td>
                      <td>{membershipStatusLabel(item.status)}</td>
                      <td>{item.requestMessage || "-"}</td>
                      <td>
                        <div className="backend-actions">
                          <button
                            type="button"
                            className="backend-btn"
                            onClick={() => reviewJoinRequest("approve", item)}
                            disabled={saving}
                          >
                            Zatwierdź
                          </button>
                          <button
                            type="button"
                            className="backend-ghost"
                            onClick={() => reviewJoinRequest("reject", item)}
                            disabled={saving}
                          >
                            Odrzuć
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}

                  {joinRequests.length === 0 && (
                    <tr>
                      <td colSpan="6">
                        <div className="backend-empty">Brak zgłoszeń.</div>
                      </td>
                    </tr>
                  )}
                </tbody>
              </table>
            </section>
          </>
        )}
      </div>
    </>
  );
}

export default AdminPage;
