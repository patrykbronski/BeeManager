import { useEffect, useMemo, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import Navbar from "../components/Navbar";
import { beeApi } from "../api/beeApi";
import { useAppContext } from "../context/AppContext";
import {
  familyStateLabel,
  familyStateOptions,
  honeyTypeLabel,
  honeyTypeOptions,
  hiveStatusLabel,
  hiveStatusOptions,
  hiveTypeLabel,
  hiveTypeOptions,
  membershipRoleLabel,
  membershipRoleOptions,
  membershipStatusLabel,
} from "../utils/beeEnums";
import { formatDate, formatDateTime } from "../utils/formatters";
import "../styles/backend.css";

const emptyApiaryForm = {
  nazwa: "",
  lokalizacja: "",
  opis: "",
};

const emptyHiveForm = {
  pasiekaId: "",
  numerUla: "",
  typUla: "",
  status: "",
  dataZalozenia: "",
  uwagi: "",
};

const emptyHarvestForm = {
  ulId: "",
  dataMiodobrania: "",
  typMiodu: "Wielokwiatowy",
  iloscKg: "",
  notatki: "",
};

const emptyInspectionForm = {
  ulId: "",
  dataPrzegladu: "",
  stanRodziny: "Dobry",
  obecnoscMatki: true,
  iloscCzerwiu: 0,
  notatki: "",
};

function ApiaryDetailsPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const apiaryId = Number(id);
  const { isAuthenticated, hasRole, authLoading } = useAppContext();

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [apiary, setApiary] = useState(null);
  const [apiaryForm, setApiaryForm] = useState(emptyApiaryForm);
  const [hives, setHives] = useState([]);
  const [harvests, setHarvests] = useState([]);
  const [inspections, setInspections] = useState([]);
  const [memberships, setMemberships] = useState([]);
  const [hiveForm, setHiveForm] = useState(emptyHiveForm);
  const [harvestForm, setHarvestForm] = useState(emptyHarvestForm);
  const [inspectionForm, setInspectionForm] = useState(emptyInspectionForm);
  const [membershipForm, setMembershipForm] = useState({
    email: "",
    membershipRole: "Worker",
  });
  const [activeTab, setActiveTab] = useState("overview");
  const [editingHiveId, setEditingHiveId] = useState(null);
  const [editingHarvestId, setEditingHarvestId] = useState(null);
  const [editingInspectionId, setEditingInspectionId] = useState(null);
  const [saving, setSaving] = useState(false);

  const canManageApiary = apiary?.canManage || false;
  const canEditNotes = apiary?.canManage || hasRole("Worker") || false;
  const canReviewMemberships = canManageApiary;

  async function loadAll() {
    const apiaryResponse = await beeApi.get(`/apiaries/${apiaryId}`);
    const [hivesResponse, harvestsResponse, inspectionsResponse] = await Promise.all([
      beeApi.get("/hives", { params: { apiaryId } }),
      beeApi.get("/harvests", { params: { apiaryId } }),
      beeApi.get("/inspections", { params: { apiaryId } }),
    ]);
    const membershipsResponse = apiaryResponse.data.canManage
      ? await beeApi.get(`/apiaries/${apiaryId}/memberships`)
      : { data: [] };

    setApiary(apiaryResponse.data);
    setApiaryForm({
      nazwa: apiaryResponse.data.nazwa || "",
      lokalizacja: apiaryResponse.data.lokalizacja || "",
      opis: apiaryResponse.data.opis || "",
    });
    setHives(hivesResponse.data);
    setHarvests(harvestsResponse.data);
    setInspections(inspectionsResponse.data);
    setMemberships(membershipsResponse.data);
    setHiveForm((prev) => ({ ...prev, pasiekaId: String(apiaryId) }));
  }

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
        await loadAll();
        if (alive) {
          setError("");
        }
      } catch (requestError) {
        if (alive) {
          setError(
            requestError?.response?.data?.message ||
              "Nie udało się pobrać szczegółów pasieki."
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
  }, [apiaryId, authLoading, isAuthenticated]);

  useEffect(() => {
    setHiveForm((prev) => ({ ...prev, pasiekaId: String(apiaryId) }));
  }, [apiaryId]);

  const tabs = useMemo(
    () => [
      { id: "overview", label: "Pasieka" },
      { id: "memberships", label: "Członkowie" },
      { id: "hives", label: "Ule" },
      { id: "harvests", label: "Miodobrania" },
      { id: "inspections", label: "Przeglądy" },
    ],
    []
  );

  async function refresh() {
    await loadAll();
  }

  async function saveApiary(event) {
    event.preventDefault();
    try {
      setSaving(true);
      await beeApi.put(`/apiaries/${apiaryId}`, apiaryForm);
      await refresh();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zapisać pasieki."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteApiary() {
    if (!window.confirm("Usunąć pasiekę?")) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/apiaries/${apiaryId}`);
      navigate("/pasieki");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się usunąć pasieki."
      );
    } finally {
      setSaving(false);
    }
  }

  async function addMembership(event) {
    event.preventDefault();
    try {
      setSaving(true);
      await beeApi.post(`/apiaries/${apiaryId}/memberships/direct`, {
        email: membershipForm.email,
        membershipRole: membershipForm.membershipRole,
      });
      setMembershipForm({ email: "", membershipRole: "Worker" });
      await refresh();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się dodać członka."
      );
    } finally {
      setSaving(false);
    }
  }

  async function reviewMembership(action, membership) {
    const note = window.prompt(
      action === "approve" ? "Notatka decyzji (opcjonalna)" : "Powód odrzucenia"
    );

    try {
      setSaving(true);
      if (action === "approve") {
        await beeApi.post(`/apiaries/${apiaryId}/memberships/${membership.id}/approve`, {
          decisionNote: note || "",
        });
      } else {
        await beeApi.post(`/apiaries/${apiaryId}/memberships/${membership.id}/reject`, {
          decisionNote: note || "",
        });
      }
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się przetworzyć zgłoszenia."
      );
    } finally {
      setSaving(false);
    }
  }

  async function removeMembership(membershipId) {
    if (!window.confirm("Usunąć członka pasieki?")) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/apiaries/${apiaryId}/memberships/${membershipId}`);
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się usunąć członka."
      );
    } finally {
      setSaving(false);
    }
  }

  function openHiveEditor(item) {
    setEditingHiveId(item.id);
    setHiveForm({
      pasiekaId: String(item.pasiekaId || apiaryId),
      numerUla: item.numerUla || "",
      typUla: item.typUla || "",
      status: item.status || "",
      dataZalozenia: item.dataZalozenia ? String(item.dataZalozenia).slice(0, 10) : "",
      uwagi: item.uwagi || "",
    });
    setActiveTab("hives");
  }

  async function saveHive(event) {
    event.preventDefault();
    const payload = {
      pasiekaId: Number(hiveForm.pasiekaId || apiaryId),
      numerUla: hiveForm.numerUla,
      typUla: hiveForm.typUla || null,
      status: hiveForm.status || null,
      dataZalozenia: hiveForm.dataZalozenia || null,
      uwagi: hiveForm.uwagi || null,
    };

    try {
      setSaving(true);
      if (editingHiveId) {
        await beeApi.put(`/hives/${editingHiveId}`, payload);
      } else {
        await beeApi.post("/hives", payload);
      }
      setEditingHiveId(null);
      setHiveForm(emptyHiveForm);
      await refresh();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message || "Nie udało się zapisać ula."
      );
    } finally {
      setSaving(false);
    }
  }

  async function updateHiveNotes(item) {
    const note = window.prompt("Nowe notatki ula", item.uwagi || "");
    if (note === null) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.patch(`/hives/${item.id}/notes`, { uwagi: note });
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zaktualizować notatek ula."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteHive(item) {
    if (!window.confirm(`Usunąć ul ${item.numerUla}?`)) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/hives/${item.id}`);
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message || "Nie udało się usunąć ula."
      );
    } finally {
      setSaving(false);
    }
  }

  function openHarvestEditor(item) {
    setEditingHarvestId(item.id);
    setHarvestForm({
      ulId: String(item.ulId),
      dataMiodobrania: item.dataMiodobrania ? String(item.dataMiodobrania).slice(0, 10) : "",
      typMiodu: item.typMiodu || "Wielokwiatowy",
      iloscKg: item.iloscKg ?? "",
      notatki: item.notatki || "",
    });
    setActiveTab("harvests");
  }

  async function saveHarvest(event) {
    event.preventDefault();
    const payload = {
      ulId: Number(harvestForm.ulId),
      dataMiodobrania: harvestForm.dataMiodobrania,
      typMiodu: harvestForm.typMiodu,
      iloscKg: Number(harvestForm.iloscKg),
      notatki: harvestForm.notatki || null,
    };

    try {
      setSaving(true);
      if (editingHarvestId) {
        await beeApi.put(`/harvests/${editingHarvestId}`, payload);
      } else {
        await beeApi.post("/harvests", payload);
      }
      setEditingHarvestId(null);
      setHarvestForm(emptyHarvestForm);
      await refresh();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zapisać miodobrania."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteHarvest(item) {
    if (!window.confirm("Usunąć miodobranie?")) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/harvests/${item.id}`);
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się usunąć miodobrania."
      );
    } finally {
      setSaving(false);
    }
  }

  function openInspectionEditor(item) {
    setEditingInspectionId(item.id);
    setInspectionForm({
      ulId: String(item.ulId),
      dataPrzegladu: item.dataPrzegladu ? String(item.dataPrzegladu).slice(0, 10) : "",
      stanRodziny: item.stanRodziny || "Dobry",
      obecnoscMatki: Boolean(item.obecnoscMatki),
      iloscCzerwiu: item.iloscCzerwiu ?? 0,
      notatki: item.notatki || "",
    });
    setActiveTab("inspections");
  }

  async function saveInspection(event) {
    event.preventDefault();
    const payload = {
      ulId: Number(inspectionForm.ulId),
      dataPrzegladu: inspectionForm.dataPrzegladu,
      stanRodziny: inspectionForm.stanRodziny,
      obecnoscMatki: inspectionForm.obecnoscMatki,
      iloscCzerwiu: Number(inspectionForm.iloscCzerwiu),
      notatki: inspectionForm.notatki || null,
    };

    try {
      setSaving(true);
      if (editingInspectionId) {
        await beeApi.put(`/inspections/${editingInspectionId}`, payload);
      } else {
        await beeApi.post("/inspections", payload);
      }
      setEditingInspectionId(null);
      setInspectionForm(emptyInspectionForm);
      await refresh();
      setError("");
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się zapisać przeglądu."
      );
    } finally {
      setSaving(false);
    }
  }

  async function addSpecialistNote(item) {
    const note = window.prompt("Uwagi specjalistyczne");
    if (!note) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.post(`/inspections/${item.id}/specialist-note`, {
        note,
      });
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message ||
          "Nie udało się dodać uwagi specjalistycznej."
      );
    } finally {
      setSaving(false);
    }
  }

  async function deleteInspection(item) {
    if (!window.confirm("Usunąć przegląd?")) {
      return;
    }

    try {
      setSaving(true);
      await beeApi.delete(`/inspections/${item.id}`);
      await refresh();
    } catch (requestError) {
      setError(
        requestError?.response?.data?.message || "Nie udało się usunąć przeglądu."
      );
    } finally {
      setSaving(false);
    }
  }

  const apiaryMembers = memberships.filter((item) => item.status === "Approved");
  const pendingMemberships = memberships.filter((item) => item.status === "Pending");

  return (
    <>
      <Navbar />

      <div className="backend-shell">
        <section className="backend-hero">
          <h2>{apiary?.nazwa || "Szczegóły pasieki"}</h2>
          <p>
            Tutaj zarządzasz jednym konkretnym obiektem: pasieką, ulami,
            miodobraniami, przeglądami oraz członkostwem. Poniżej są wszystkie
            pola dostępne w backendzie.
          </p>

          <div className="backend-toolbar">
            <Link to="/pasieki" className="backend-ghost">
              Wróć do listy
            </Link>
            <button
              type="button"
              className="backend-ghost"
              onClick={refresh}
              disabled={loading}
            >
              Odśwież
            </button>
          </div>
        </section>

        {authLoading || loading ? (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-empty">Ładowanie...</div>
          </section>
        ) : !isAuthenticated ? (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-empty">Zaloguj się, aby zobaczyć szczegóły pasieki.</div>
          </section>
        ) : error ? (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-pill danger">{error}</div>
          </section>
        ) : apiary ? (
          <>
            <section className="backend-grid cols-3" style={{ marginTop: 18 }}>
              <div className="backend-card">
                <span className="backend-muted">Właściciel</span>
                <strong>{apiary.ownerName || "-"}</strong>
              </div>
              <div className="backend-card">
                <span className="backend-muted">Utworzono</span>
                <strong>{formatDateTime(apiary.utworzonoAtUtc)}</strong>
              </div>
              <div className="backend-card">
                <span className="backend-muted">Dostęp</span>
                <strong>{apiary.membershipRole || (apiary.isOwner ? "Właściciel" : "Brak")}</strong>
              </div>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Pasieka</h3>
                  <p className="backend-muted">
                    Pola zgodne z backendem: nazwa, lokalizacja i opis.
                  </p>
                </div>
                <div className="backend-actions">
                  {canManageApiary && (
                    <button
                      type="button"
                      className="backend-pill danger"
                      onClick={deleteApiary}
                      disabled={saving}
                    >
                      Usuń pasiekę
                    </button>
                  )}
                </div>
              </div>

              <form className="backend-form grid-2" onSubmit={saveApiary}>
                <div className="backend-field">
                  <label>Nazwa</label>
                  <input
                    value={apiaryForm.nazwa}
                    onChange={(event) =>
                      setApiaryForm((prev) => ({ ...prev, nazwa: event.target.value }))
                    }
                    required
                    maxLength={150}
                    disabled={!canManageApiary}
                  />
                </div>
                <div className="backend-field">
                  <label>Lokalizacja</label>
                  <input
                    value={apiaryForm.lokalizacja}
                    onChange={(event) =>
                      setApiaryForm((prev) => ({
                        ...prev,
                        lokalizacja: event.target.value,
                      }))
                    }
                    maxLength={255}
                    disabled={!canManageApiary}
                  />
                </div>
                <div className="backend-field" style={{ gridColumn: "1 / -1" }}>
                  <label>Opis</label>
                  <textarea
                    value={apiaryForm.opis}
                    onChange={(event) =>
                      setApiaryForm((prev) => ({ ...prev, opis: event.target.value }))
                    }
                    maxLength={1000}
                    disabled={!canManageApiary}
                  />
                </div>
                {canManageApiary && (
                  <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                    <button type="submit" className="backend-btn" disabled={saving}>
                      Zapisz pasiekę
                    </button>
                  </div>
                )}
              </form>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-header">
                <div>
                  <h3>Członkowie</h3>
                  <p className="backend-muted">
                    Backend obsługuje zgłoszenia, bezpośrednie dodawanie oraz
                    akceptację/odrzucenie.
                  </p>
                </div>
              </div>

              {canReviewMemberships && (
                <form className="backend-form grid-2" onSubmit={addMembership}>
                  <div className="backend-field">
                    <label>E-mail</label>
                    <input
                      type="email"
                      value={membershipForm.email}
                      onChange={(event) =>
                        setMembershipForm((prev) => ({ ...prev, email: event.target.value }))
                      }
                      required
                    />
                  </div>
                  <div className="backend-field">
                    <label>Rola członkostwa</label>
                    <select
                      value={membershipForm.membershipRole}
                      onChange={(event) =>
                        setMembershipForm((prev) => ({
                          ...prev,
                          membershipRole: event.target.value,
                        }))
                      }
                    >
                      {membershipRoleOptions.map((option) => (
                        <option key={option.value} value={option.value}>
                          {option.label}
                        </option>
                      ))}
                    </select>
                  </div>
                  <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                    <button type="submit" className="backend-btn" disabled={saving}>
                      Dodaj bezpośrednio
                    </button>
                  </div>
                </form>
              )}

              <div className="backend-divider" />

              <div className="backend-grid cols-2">
                <div className="backend-card">
                  <h4>Zatwierdzone</h4>
                  {apiaryMembers.length === 0 ? (
                    <div className="backend-empty">Brak członków.</div>
                  ) : (
                    <div className="backend-stack">
                      {apiaryMembers.map((membership) => (
                        <article key={membership.id} className="backend-card">
                          <strong>{membership.userName}</strong>
                          <div className="backend-muted">{membership.email}</div>
                          <div className="backend-muted">
                            {membershipRoleLabel(membership.membershipRole)} ·{" "}
                            {membershipStatusLabel(membership.status)}
                          </div>
                          <div className="backend-actions" style={{ marginTop: 10 }}>
                            {canReviewMemberships && (
                              <button
                                type="button"
                                className="backend-ghost"
                                onClick={() => removeMembership(membership.id)}
                                disabled={saving}
                              >
                                Usuń
                              </button>
                            )}
                          </div>
                        </article>
                      ))}
                    </div>
                  )}
                </div>

                <div className="backend-card">
                  <h4>Oczekujące</h4>
                  {pendingMemberships.length === 0 ? (
                    <div className="backend-empty">Brak oczekujących zgłoszeń.</div>
                  ) : (
                    <div className="backend-stack">
                      {pendingMemberships.map((membership) => (
                        <article key={membership.id} className="backend-card">
                          <strong>{membership.userName}</strong>
                          <div className="backend-muted">{membership.email}</div>
                          <div className="backend-muted">
                            {membershipRoleLabel(membership.membershipRole)} ·{" "}
                            {membershipStatusLabel(membership.status)}
                          </div>
                          <p className="backend-muted">{membership.requestMessage || ""}</p>
                          <div className="backend-actions" style={{ marginTop: 10 }}>
                            <button
                              type="button"
                              className="backend-btn"
                              onClick={() => reviewMembership("approve", membership)}
                              disabled={saving}
                            >
                              Zatwierdź
                            </button>
                            <button
                              type="button"
                              className="backend-ghost"
                              onClick={() => reviewMembership("reject", membership)}
                              disabled={saving}
                            >
                              Odrzuć
                            </button>
                          </div>
                        </article>
                      ))}
                    </div>
                  )}
                </div>
              </div>
            </section>

            <section className="backend-section" style={{ marginTop: 18 }}>
              <div className="backend-section-tabs">
                {tabs.map((tab) => (
                  <button
                    key={tab.id}
                    type="button"
                    className={`backend-btn backend-tab ${activeTab === tab.id ? "active" : ""}`}
                    onClick={() => setActiveTab(tab.id)}
                  >
                    {tab.label}
                  </button>
                ))}
              </div>

              {activeTab === "overview" && (
                <div className="backend-grid cols-3">
                  <div className="backend-card">
                    <span className="backend-muted">Ule</span>
                    <strong>{hives.length}</strong>
                  </div>
                  <div className="backend-card">
                    <span className="backend-muted">Miodobrania</span>
                    <strong>{harvests.length}</strong>
                  </div>
                  <div className="backend-card">
                    <span className="backend-muted">Przeglądy</span>
                    <strong>{inspections.length}</strong>
                  </div>
                </div>
              )}

              {activeTab === "hives" && (
                <div className="backend-stack">
                  <form className="backend-form grid-2" onSubmit={saveHive}>
                    <div className="backend-field">
                      <label>Numer ula</label>
                      <input
                        value={hiveForm.numerUla}
                        onChange={(event) =>
                          setHiveForm((prev) => ({ ...prev, numerUla: event.target.value }))
                        }
                        required
                        maxLength={50}
                      />
                    </div>
                    <div className="backend-field">
                      <label>Pasieka ID</label>
                      <input value={hiveForm.pasiekaId} disabled />
                    </div>
                    <div className="backend-field">
                      <label>Typ ula</label>
                      <select
                        value={hiveForm.typUla}
                        onChange={(event) =>
                          setHiveForm((prev) => ({ ...prev, typUla: event.target.value }))
                        }
                      >
                        <option value="">Brak</option>
                        {hiveTypeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="backend-field">
                      <label>Status</label>
                      <select
                        value={hiveForm.status}
                        onChange={(event) =>
                          setHiveForm((prev) => ({ ...prev, status: event.target.value }))
                        }
                      >
                        <option value="">Brak</option>
                        {hiveStatusOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="backend-field">
                      <label>Data założenia</label>
                      <input
                        type="date"
                        value={hiveForm.dataZalozenia}
                        onChange={(event) =>
                          setHiveForm((prev) => ({
                            ...prev,
                            dataZalozenia: event.target.value,
                          }))
                        }
                      />
                    </div>
                    <div className="backend-field" style={{ gridColumn: "1 / -1" }}>
                      <label>Uwagi</label>
                      <textarea
                        value={hiveForm.uwagi}
                        onChange={(event) =>
                          setHiveForm((prev) => ({ ...prev, uwagi: event.target.value }))
                        }
                        maxLength={1000}
                      />
                    </div>
                    <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                      <button type="submit" className="backend-btn" disabled={saving}>
                        {editingHiveId ? "Zapisz ul" : "Dodaj ul"}
                      </button>
                      {editingHiveId && (
                        <button
                          type="button"
                          className="backend-ghost"
                          onClick={() => {
                            setEditingHiveId(null);
                            setHiveForm(emptyHiveForm);
                          }}
                        >
                          Anuluj
                        </button>
                      )}
                    </div>
                  </form>

                  <table className="backend-table">
                    <thead>
                      <tr>
                        <th>Numer</th>
                        <th>Typ</th>
                        <th>Status</th>
                        <th>Data</th>
                        <th>Uwagi</th>
                        <th>Akcje</th>
                      </tr>
                    </thead>
                    <tbody>
                      {hives.length === 0 ? (
                        <tr>
                          <td colSpan="6">
                            <div className="backend-empty">Brak uli.</div>
                          </td>
                        </tr>
                      ) : (
                        hives.map((item) => (
                          <tr key={item.id}>
                            <td>{item.numerUla}</td>
                            <td>{hiveTypeLabel(item.typUla)}</td>
                            <td>{hiveStatusLabel(item.status)}</td>
                            <td>{formatDate(item.dataZalozenia)}</td>
                            <td>{item.uwagi || "-"}</td>
                            <td>
                              <div className="backend-actions">
                                {item.canManage && (
                                  <button
                                    type="button"
                                    className="backend-link"
                                    onClick={() => openHiveEditor(item)}
                                  >
                                    Edytuj
                                  </button>
                                )}
                                {item.canEditNotes && canEditNotes && (
                                  <button
                                    type="button"
                                    className="backend-ghost"
                                    onClick={() => updateHiveNotes(item)}
                                  >
                                    Notatki
                                  </button>
                                )}
                                {item.canManage && (
                                  <button
                                    type="button"
                                    className="backend-pill danger"
                                    onClick={() => deleteHive(item)}
                                  >
                                    Usuń
                                  </button>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              )}

              {activeTab === "harvests" && (
                <div className="backend-stack">
                  <form className="backend-form grid-2" onSubmit={saveHarvest}>
                    <div className="backend-field">
                      <label>Ul ID</label>
                      <input
                        value={harvestForm.ulId}
                        onChange={(event) =>
                          setHarvestForm((prev) => ({ ...prev, ulId: event.target.value }))
                        }
                        required
                        type="number"
                      />
                    </div>
                    <div className="backend-field">
                      <label>Data miodobrania</label>
                      <input
                        type="date"
                        value={harvestForm.dataMiodobrania}
                        onChange={(event) =>
                          setHarvestForm((prev) => ({
                            ...prev,
                            dataMiodobrania: event.target.value,
                          }))
                        }
                        required
                      />
                    </div>
                    <div className="backend-field">
                      <label>Typ miodu</label>
                      <select
                        value={harvestForm.typMiodu}
                        onChange={(event) =>
                          setHarvestForm((prev) => ({ ...prev, typMiodu: event.target.value }))
                        }
                      >
                        {honeyTypeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="backend-field">
                      <label>Ilość kg</label>
                      <input
                        type="number"
                        step="0.01"
                        value={harvestForm.iloscKg}
                        onChange={(event) =>
                          setHarvestForm((prev) => ({ ...prev, iloscKg: event.target.value }))
                        }
                        required
                      />
                    </div>
                    <div className="backend-field" style={{ gridColumn: "1 / -1" }}>
                      <label>Notatki</label>
                      <textarea
                        value={harvestForm.notatki}
                        onChange={(event) =>
                          setHarvestForm((prev) => ({ ...prev, notatki: event.target.value }))
                        }
                        maxLength={1000}
                      />
                    </div>
                    <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                      <button type="submit" className="backend-btn" disabled={saving}>
                        {editingHarvestId ? "Zapisz miodobranie" : "Dodaj miodobranie"}
                      </button>
                      {editingHarvestId && (
                        <button
                          type="button"
                          className="backend-ghost"
                          onClick={() => {
                            setEditingHarvestId(null);
                            setHarvestForm(emptyHarvestForm);
                          }}
                        >
                          Anuluj
                        </button>
                      )}
                    </div>
                  </form>

                  <table className="backend-table">
                    <thead>
                      <tr>
                        <th>Ula</th>
                        <th>Data</th>
                        <th>Typ</th>
                        <th>Ilość</th>
                        <th>Notatki</th>
                        <th>Akcje</th>
                      </tr>
                    </thead>
                    <tbody>
                      {harvests.length === 0 ? (
                        <tr>
                          <td colSpan="6">
                            <div className="backend-empty">Brak miodobrań.</div>
                          </td>
                        </tr>
                      ) : (
                        harvests.map((item) => (
                          <tr key={item.id}>
                            <td>{item.numerUla}</td>
                            <td>{formatDate(item.dataMiodobrania)}</td>
                            <td>{honeyTypeLabel(item.typMiodu)}</td>
                            <td>{item.iloscKg}</td>
                            <td>{item.notatki || "-"}</td>
                            <td>
                              <div className="backend-actions">
                                {item.canEdit && (
                                  <button
                                    type="button"
                                    className="backend-link"
                                    onClick={() => openHarvestEditor(item)}
                                  >
                                    Edytuj
                                  </button>
                                )}
                                {item.canDelete && (
                                  <button
                                    type="button"
                                    className="backend-pill danger"
                                    onClick={() => deleteHarvest(item)}
                                  >
                                    Usuń
                                  </button>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              )}

              {activeTab === "inspections" && (
                <div className="backend-stack">
                  <form className="backend-form grid-2" onSubmit={saveInspection}>
                    <div className="backend-field">
                      <label>Ul ID</label>
                      <input
                        value={inspectionForm.ulId}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({ ...prev, ulId: event.target.value }))
                        }
                        required
                        type="number"
                      />
                    </div>
                    <div className="backend-field">
                      <label>Data przeglądu</label>
                      <input
                        type="date"
                        value={inspectionForm.dataPrzegladu}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({
                            ...prev,
                            dataPrzegladu: event.target.value,
                          }))
                        }
                        required
                      />
                    </div>
                    <div className="backend-field">
                      <label>Stan rodziny</label>
                      <select
                        value={inspectionForm.stanRodziny}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({
                            ...prev,
                            stanRodziny: event.target.value,
                          }))
                        }
                      >
                        {familyStateOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </div>
                    <div className="backend-field">
                      <label>Obecność matki</label>
                      <select
                        value={inspectionForm.obecnoscMatki ? "true" : "false"}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({
                            ...prev,
                            obecnoscMatki: event.target.value === "true",
                          }))
                        }
                      >
                        <option value="true">Tak</option>
                        <option value="false">Nie</option>
                      </select>
                    </div>
                    <div className="backend-field">
                      <label>Ilość czerwiu</label>
                      <input
                        type="number"
                        min="0"
                        max="50"
                        value={inspectionForm.iloscCzerwiu}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({
                            ...prev,
                            iloscCzerwiu: event.target.value,
                          }))
                        }
                        required
                      />
                    </div>
                    <div className="backend-field" style={{ gridColumn: "1 / -1" }}>
                      <label>Notatki</label>
                      <textarea
                        value={inspectionForm.notatki}
                        onChange={(event) =>
                          setInspectionForm((prev) => ({
                            ...prev,
                            notatki: event.target.value,
                          }))
                        }
                        maxLength={2000}
                      />
                    </div>
                    <div className="backend-actions" style={{ gridColumn: "1 / -1" }}>
                      <button type="submit" className="backend-btn" disabled={saving}>
                        {editingInspectionId ? "Zapisz przegląd" : "Dodaj przegląd"}
                      </button>
                      {editingInspectionId && (
                        <button
                          type="button"
                          className="backend-ghost"
                          onClick={() => {
                            setEditingInspectionId(null);
                            setInspectionForm(emptyInspectionForm);
                          }}
                        >
                          Anuluj
                        </button>
                      )}
                    </div>
                  </form>

                  <table className="backend-table">
                    <thead>
                      <tr>
                        <th>Ula</th>
                        <th>Data</th>
                        <th>Stan</th>
                        <th>Matka</th>
                        <th>Czerw</th>
                        <th>Notatki</th>
                        <th>Akcje</th>
                      </tr>
                    </thead>
                    <tbody>
                      {inspections.length === 0 ? (
                        <tr>
                          <td colSpan="7">
                            <div className="backend-empty">Brak przeglądów.</div>
                          </td>
                        </tr>
                      ) : (
                        inspections.map((item) => (
                          <tr key={item.id}>
                            <td>{item.numerUla}</td>
                            <td>{formatDate(item.dataPrzegladu)}</td>
                            <td>{familyStateLabel(item.stanRodziny)}</td>
                            <td>{item.obecnoscMatki ? "Tak" : "Nie"}</td>
                            <td>{item.iloscCzerwiu}</td>
                            <td>{item.notatki || "-"}</td>
                            <td>
                              <div className="backend-actions">
                                {item.canEdit && (
                                  <button
                                    type="button"
                                    className="backend-link"
                                    onClick={() => openInspectionEditor(item)}
                                  >
                                    Edytuj
                                  </button>
                                )}
                                {item.canAddSpecialistNote && (
                                  <button
                                    type="button"
                                    className="backend-ghost"
                                    onClick={() => addSpecialistNote(item)}
                                  >
                                    Uwaga specjalistyczna
                                  </button>
                                )}
                                {item.canDelete && (
                                  <button
                                    type="button"
                                    className="backend-pill danger"
                                    onClick={() => deleteInspection(item)}
                                  >
                                    Usuń
                                  </button>
                                )}
                              </div>
                            </td>
                          </tr>
                        ))
                      )}
                    </tbody>
                  </table>
                </div>
              )}
            </section>
          </>
        ) : (
          <section className="backend-section" style={{ marginTop: 18 }}>
            <div className="backend-empty">Nie znaleziono pasieki.</div>
          </section>
        )}
      </div>
    </>
  );
}

export default ApiaryDetailsPage;
