const requestedRoleLabels = {
  Owner: "Właściciel",
  Worker: "Pracownik",
  Inspector: "Inspektor",
};

const accountStatusLabels = {
  Pending: "Oczekuje",
  Approved: "Zatwierdzone",
  Rejected: "Odrzucone",
};

const membershipRoleLabels = {
  Worker: "Pracownik",
  Inspector: "Inspektor",
};

const membershipStatusLabels = {
  Pending: "Oczekuje",
  Approved: "Zatwierdzone",
  Rejected: "Odrzucone",
};

const hiveTypeLabels = {
  Wielkopolski: "Wielkopolski",
  Dadant: "Dadant",
  Warszawski: "Warszawski",
};

const hiveStatusLabels = {
  Aktywny: "Aktywny",
  Pusty: "Pusty",
  Zniszczony: "Zniszczony",
};

const familyStateLabels = {
  BardzoSlaby: "Bardzo słaby",
  Slaby: "Słaby",
  Sredni: "Średni",
  Dobry: "Dobry",
  BardzoDobry: "Bardzo dobry",
};

const honeyTypeLabels = {
  Wielokwiatowy: "Wielokwiatowy",
  Lipowy: "Lipowy",
  Akacjowy: "Akacjowy",
  Gryczany: "Gryczany",
  Rzepakowy: "Rzepakowy",
  Spadziowy: "Spadziowy",
};

const enumEntries = (labels) =>
  Object.entries(labels).map(([value, label]) => ({ value, label }));

export const requestedRoleOptions = enumEntries(requestedRoleLabels);
export const accountStatusOptions = enumEntries(accountStatusLabels);
export const membershipRoleOptions = enumEntries(membershipRoleLabels);
export const membershipStatusOptions = enumEntries(membershipStatusLabels);
export const hiveTypeOptions = enumEntries(hiveTypeLabels);
export const hiveStatusOptions = enumEntries(hiveStatusLabels);
export const familyStateOptions = enumEntries(familyStateLabels);
export const honeyTypeOptions = enumEntries(honeyTypeLabels);

export const roleLabel = (value) => requestedRoleLabels[value] || value || "-";
export const accountStatusLabel = (value) =>
  accountStatusLabels[value] || value || "-";
export const membershipRoleLabel = (value) =>
  membershipRoleLabels[value] || value || "-";
export const membershipStatusLabel = (value) =>
  membershipStatusLabels[value] || value || "-";
export const hiveTypeLabel = (value) => hiveTypeLabels[value] || value || "-";
export const hiveStatusLabel = (value) =>
  hiveStatusLabels[value] || value || "-";
export const familyStateLabel = (value) =>
  familyStateLabels[value] || value || "-";
export const honeyTypeLabel = (value) => honeyTypeLabels[value] || value || "-";

