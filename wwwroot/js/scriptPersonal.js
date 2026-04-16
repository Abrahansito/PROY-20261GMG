// Global variables
let resumenData = null;
let consultoriosList = [];
let lastSearchPerformed = false;
let currentFilters = {
  Nombre: null,
  Dni: null,
  Estado: null,
  TipoPersonal: "TODOS",
  IdConsultorio: null,
};
const API_BASE_URL = "http://localhost:5122";
const FILTERS_STORAGE_KEY = "personal_filters";

// ==================== GESTIÓN DE LOCALSTORAGE ====================
function saveFiltersToStorage() {
  try {
    const filtersToSave = {
      ...currentFilters,
    };
    const filtersJSON = JSON.stringify(filtersToSave);
    console.log("💾 Guardando filtros en localStorage:", filtersJSON);
    localStorage.setItem(FILTERS_STORAGE_KEY, filtersJSON);
  } catch (error) {
    console.error("Error al guardar filtros en localStorage:", error);
  }
}

function loadFiltersFromStorage() {
  try {
    const savedFilters = localStorage.getItem(FILTERS_STORAGE_KEY);
    if (savedFilters) {
      const parsed = JSON.parse(savedFilters);
      console.log("📂 Filtros cargados desde localStorage:", parsed);
      // Remover timestamp antes de asignar
      delete parsed.timestamp;
      currentFilters = parsed;
      return true;
    }
    return false;
  } catch (error) {
    console.error("Error al cargar filtros desde localStorage:", error);
    return false;
  }
}

function clearFiltersFromStorage() {
  try {
    localStorage.removeItem(FILTERS_STORAGE_KEY);
    console.log("🗑️ Filtros eliminados de localStorage");
  } catch (error) {
    console.error("Error al eliminar filtros de localStorage:", error);
  }
}

// ==================== SISTEMA DE ALERTAS MEJORADO ====================
class AlertManager {
  constructor() {
    this.container = null;
    this.init();
  }

  init() {
    if (!document.getElementById("alertContainer")) {
      const container = document.createElement("div");
      container.id = "alertContainer";
      container.className = "alert-container";
      document.body.appendChild(container);
    }
    this.container = document.getElementById("alertContainer");
  }

  show(message, type = "success", duration = 5000) {
    const alert = document.createElement("div");
    alert.className = `alert alert-${type} alert-enter`;
    const icon = this.getIcon(type);

    alert.innerHTML = `
      <div class="alert-icon">${icon}</div>
      <div class="alert-content">
        <div class="alert-message">${message}</div>
      </div>
      <button class="alert-close" aria-label="Cerrar">&times;</button>
    `;

    this.container.appendChild(alert);
    setTimeout(() => alert.classList.add("alert-show"), 10);

    const closeBtn = alert.querySelector(".alert-close");
    closeBtn.addEventListener("click", () => this.remove(alert));

    if (duration > 0) {
      setTimeout(() => this.remove(alert), duration);
    }

    return alert;
  }

  remove(alert) {
    alert.classList.remove("alert-show");
    alert.classList.add("alert-exit");
    setTimeout(() => {
      if (alert.parentNode) {
        alert.parentNode.removeChild(alert);
      }
    }, 300);
  }

  getIcon(type) {
    const icons = {
      success:
        '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>',
      error:
        '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>',
      warning:
        '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>',
      info: '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>',
    };
    return icons[type] || icons.info;
  }

  success(message, duration = 4000) {
    return this.show(message, "success", duration);
  }

  error(message, duration = 4000) {
    return this.show(message, "error", duration);
  }

  warning(message, duration = 4000) {
    return this.show(message, "warning", duration);
  }

  info(message, duration = 4000) {
    return this.show(message, "info", duration);
  }

  clear() {
    if (this.container) {
      this.container.innerHTML = "";
    }
  }
}

const alertManager = new AlertManager();

// ==================== MANEJO DE RESPUESTAS DEL BACKEND ====================
function handleApiResponse(
  response,
  successCallback = null,
  errorCallback = null
) {
  if (!response) {
    alertManager.error("No se recibió respuesta del servidor");
    return false;
  }

  if (response.success === true) {
    if (response.message) {
      alertManager.success(response.message);
    }
    if (successCallback && typeof successCallback === "function") {
      successCallback(response.data);
    }
    return true;
  } else if (response.success === false) {
    if (response.message) {
      alertManager.error(response.message);
    }
    if (errorCallback && typeof errorCallback === "function") {
      errorCallback(response.data);
    }
    return false;
  } else {
    alertManager.warning("Respuesta inesperada del servidor");
    return false;
  }
}
document.addEventListener("DOMContentLoaded", async function () {
  console.log("🚀 Iniciando aplicación...");
  console.log("=".repeat(50));

  // Cargar resumen primero
  await loadResumen();
  console.log("✅ Resumen cargado");

  // Cargar filtros guardados
  console.log("📂 Intentando cargar filtros desde localStorage...");
  const hasStoredFilters = loadFiltersFromStorage();
  console.log("¿Hay filtros guardados?", hasStoredFilters);
  console.log(
    "currentFilters actual:",
    JSON.stringify(currentFilters, null, 2)
  );

  if (hasStoredFilters) {
    console.log(
      "✅ Se encontraron filtros guardados, aplicando al formulario..."
    );
    applyCurrentFiltersToForm();

    // 🔥 CAMBIO IMPORTANTE: Siempre ejecutar búsqueda si hay filtros guardados
    // Incluso si todos los filtros están vacíos (para traer todos los registros)
    console.log("🔍 Ejecutando búsqueda automática con filtros guardados...");
    console.log("Filtros a enviar:", JSON.stringify(currentFilters, null, 2));

    try {
      await executeSearchWithCurrentFilters();
      console.log("✅ Búsqueda completada exitosamente");
      lastSearchPerformed = true;
    } catch (error) {
      console.error("❌ Error al ejecutar búsqueda:", error);
    }
  } else {
    console.log("ℹ️ No se encontraron filtros guardados en localStorage");
  }

  console.log("=".repeat(50));
  initializeEventListeners();
});

function initializeEventListeners() {
  const searchForm = document.getElementById("searchForm");
  const clearBtn = document.getElementById("clearBtn");
  const addMedicoBtn = document.getElementById("addMedicoBtn");
  const addEnfermeriaBtn = document.getElementById("addEnfermeriaBtn");
  const addTecnicoBtn = document.getElementById("addTecnicoBtn");

  if (searchForm) {
    searchForm.addEventListener("submit", handleSearch);
  }

  if (clearBtn) {
    clearBtn.addEventListener("click", clearFilters);
  }

  if (addMedicoBtn) {
    addMedicoBtn.addEventListener("click", showAddMedicoModal);
  }

  if (addEnfermeriaBtn) {
    addEnfermeriaBtn.addEventListener("click", showAddEnfermeriaModal);
  }

  if (addTecnicoBtn) {
    addTecnicoBtn.addEventListener("click", showAddTecnicoModal);
  }
}

// ==================== GESTIÓN DE FILTROS PERSISTENTES ====================
function updateCurrentFilters() {
  currentFilters = {
    Nombre: document.getElementById("nombre").value.trim() || null,
    Dni: document.getElementById("dni").value.trim() || null,
    Estado: document.getElementById("estado").value || null,
    TipoPersonal:
      document.getElementById("tipoPersonal").value === ""
        ? "TODOS"
        : document.getElementById("tipoPersonal").value,
    IdConsultorio:
      document.getElementById("consultorio").value === ""
        ? null
        : parseInt(document.getElementById("consultorio").value),
  };

  console.log("🔄 Filtros actualizados:", currentFilters);
  saveFiltersToStorage();
}

function applyCurrentFiltersToForm() {
  console.log("📝 Aplicando filtros al formulario:", currentFilters);

  document.getElementById("nombre").value = currentFilters.Nombre || "";
  document.getElementById("dni").value = currentFilters.Dni || "";
  document.getElementById("estado").value = currentFilters.Estado || "";
  document.getElementById("tipoPersonal").value =
    currentFilters.TipoPersonal === "TODOS" ? "" : currentFilters.TipoPersonal;
  document.getElementById("consultorio").value =
    currentFilters.IdConsultorio || "";

  console.log("✅ Filtros aplicados al formulario");
  console.log("Formulario actual:", {
    Nombre: document.getElementById("nombre").value,
    Dni: document.getElementById("dni").value,
    Estado: document.getElementById("estado").value,
    TipoPersonal: document.getElementById("tipoPersonal").value,
    IdConsultorio: document.getElementById("consultorio").value,
  });
}

// ==================== CARGA DE DATOS ====================
async function loadResumen() {
  try {
    const response = await fetch(`${API_BASE_URL}/personal/resumen`);
    const result = await response.json();

    if (result.success) {
      resumenData = result.data;
      consultoriosList = result.data.consultoriosList || [];
      renderSummaryCards(result.data);
      populateDropdowns(result.data);
      console.log("✅ Resumen cargado correctamente");
    } else {
      alertManager.error(result.message);
    }
  } catch (error) {
    console.error("Error loading resumen:", error);
    alertManager.error("Error al cargar el resumen del personal");
  }
}

function renderSummaryCards(data) {
  const summaryCards = document.getElementById("summaryCards");

  summaryCards.innerHTML = `
    <div class="summary-card">
      <div class="card-icon blue">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2"></path>
          <circle cx="9" cy="7" r="4"></circle>
          <path d="M22 21v-2a4 4 0 0 0-3-3.87"></path>
          <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
        </svg>
      </div>
      <div class="card-value">${data.medicosActivos}</div>
      <div class="card-label">Médicos Activos</div>
    </div>
    <div class="summary-card">
      <div class="card-icon green">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"></path>
          <circle cx="12" cy="7" r="4"></circle>
        </svg>
      </div>
      <div class="card-value">${data.tecnicosActivos}</div>
      <div class="card-label">Técnicos Activos</div>
    </div>
    <div class="summary-card">
      <div class="card-icon orange">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="2" y="7" width="20" height="14" rx="2" ry="2"></rect>
          <path d="M16 21V5a2 2 0 0 0-2-2h-4a2 2 0 0 0-2 2v16"></path>
        </svg>
      </div>
      <div class="card-value">${data.consultorios}</div>
      <div class="card-label">Consultorios</div>
    </div>
    <div class="summary-card">
      <div class="card-icon purple">
        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <rect x="3" y="4" width="18" height="18" rx="2" ry="2"></rect>
          <line x1="16" y1="2" x2="16" y2="6"></line>
          <line x1="8" y1="2" x2="8" y2="6"></line>
          <line x1="3" y1="10" x2="21" y2="10"></line>
        </svg>
      </div>
      <div class="card-value">${data.personalCaja}</div>
      <div class="card-label">Personal de Caja</div>
    </div>
  `;
}

function populateDropdowns(data) {
  const tipoPersonalSelect = document.getElementById("tipoPersonal");
  const consultorioSelect = document.getElementById("consultorio");

  if (data.cargos && tipoPersonalSelect) {
    data.cargos.forEach((cargo) => {
      const option = document.createElement("option");
      option.value = cargo.nombreCargo;
      option.textContent = cargo.nombreCargo;
      tipoPersonalSelect.appendChild(option);
    });
  }

  if (data.consultoriosList && consultorioSelect) {
    data.consultoriosList.forEach((consultorio) => {
      const option = document.createElement("option");
      option.value = consultorio.idConsultorio;
      option.textContent = consultorio.nombre;
      consultorioSelect.appendChild(option);
    });
  }
}

// ==================== BÚSQUEDA Y FILTRADO ====================
async function handleSearch(e) {
  e.preventDefault();
  clearErrors();

  console.log("=== BÚSQUEDA INICIADA ===");
  updateCurrentFilters();

  try {
    const response = await fetch(`${API_BASE_URL}/personal/buscar`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(currentFilters),
    });

    const result = await response.json();

    if (result.success) {
      alertManager.success(result.message);
      renderTable(result.data);
      lastSearchPerformed = true;
    } else {
      alertManager.error(result.message);
      lastSearchPerformed = false;

      if (result.data && Array.isArray(result.data)) {
        result.data.forEach((error) => {
          displayFieldError(error.field, error.errors);
        });
      }
    }
  } catch (error) {
    console.error("Error searching personal:", error);
    alertManager.error("Error al buscar personal");
    lastSearchPerformed = false;
  }
}

// ==================== EJECUTAR BÚSQUEDA CON FILTROS ACTUALES ====================
async function executeSearchWithCurrentFilters() {
  console.log("=== EJECUTANDO BÚSQUEDA CON FILTROS ACTUALES ===");
  console.log("Filtros:", JSON.stringify(currentFilters, null, 2));

  try {
    const response = await fetch(`${API_BASE_URL}/personal/buscar`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(currentFilters),
    });

    const result = await response.json();

    if (result.success) {
      console.log("✅ Búsqueda exitosa -", result.data.length, "registros");
      renderTable(result.data);
      lastSearchPerformed = true;
    } else {
      console.log("❌ Error en búsqueda:", result.message);
      alertManager.error(result.message);
    }
  } catch (error) {
    console.error("Error al ejecutar búsqueda:", error);
    alertManager.error("Error al actualizar la lista de personal");
  }
}

function displayFieldError(field, errors) {
  const errorElement = document.getElementById(`error-${field}`);
  const inputElement =
    document.getElementById(field.toLowerCase()) ||
    document.querySelector(`[name="${field}"]`);

  if (errorElement && errors && errors.length > 0) {
    errorElement.textContent = errors[0];
    errorElement.classList.add("show");
  }

  if (inputElement) {
    inputElement.classList.add("error");
  }
}

function clearErrors() {
  const errorMessages = document.querySelectorAll(".error-message");
  const errorInputs = document.querySelectorAll(".form-control.error");

  errorMessages.forEach((el) => {
    el.classList.remove("show");
    el.textContent = "";
  });

  errorInputs.forEach((el) => {
    el.classList.remove("error");
  });
}

function clearFilters() {
  console.log("🗑️ Limpiando filtros...");

  document.getElementById("searchForm").reset();
  clearErrors();
  lastSearchPerformed = false;

  currentFilters = {
    Nombre: null,
    Dni: null,
    Estado: null,
    TipoPersonal: "TODOS",
    IdConsultorio: null,
  };

  clearFiltersFromStorage();

  const tableBody = document.getElementById("tableBody");
  tableBody.innerHTML = `
    <tr>
      <td colspan="6" class="no-data">No se han aplicado filtros. Use los filtros para buscar personal.</td>
    </tr>
  `;
}

// ==================== RENDERIZADO DE TABLA ====================
function renderTable(data) {
  const tableBody = document.getElementById("tableBody");

  tableBody.innerHTML = data
    .map(
      (personal) => `
    <tr>
      <td>${personal.dni}</td>
      <td>${personal.nombresApellidos}</td>
      <td>${personal.cargo}</td>
      <td>
        <span class="status-badge ${
          personal.estado.toLowerCase() === "activo" ? "active" : "inactive"
        }">
          ${personal.estado}
        </span>
      </td>
      <td>${personal.telefono}</td>
      <td>
        <div class="action-buttons">
          <button class="action-btn view" onclick="viewPersonal(${
            personal.id
          }, '${personal.cargo}')" title="Ver">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>
              <circle cx="12" cy="12" r="3"></circle>
            </svg>
          </button>
          <button class="action-btn edit" onclick="editPersonal(${
            personal.id
          }, '${personal.cargo}')" title="Editar">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7"></path>
              <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z"></path>
            </svg>
          </button>
          <button class="action-btn delete" onclick="deletePersonal(${
            personal.id
          }, '${personal.cargo}', '${personal.nombresApellidos}', '${
        personal.dni
      }')" title="Eliminar">
            <svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <polyline points="3 6 5 6 21 6"></polyline>
              <path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path>
              <line x1="10" y1="11" x2="10" y2="17"></line>
              <line x1="14" y1="11" x2="14" y2="17"></line>
            </svg>
          </button>
        </div>
      </td>
    </tr>
  `
    )
    .join("");
}

// ==================== VER DETALLES ====================
async function viewPersonal(id, cargo) {
  try {
    let endpoint = "";
    let title = "";

    if (cargo === "MEDICO GENERAL") {
      endpoint = `/medicos/${id}`;
      title = "Detalles del Médico";
    } else if (cargo === "ENFERMERIA") {
      endpoint = `/enfermerias/${id}`;
      title = "Detalles de Enfermería";
    } else if (
      cargo === "CAJERO" ||
      cargo === "ADMINISTRADOR" ||
      cargo === "ADMISION"
    ) {
      endpoint = `/personal-tecnico/${id}`;
      title = "Detalles del Personal Técnico";
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`);
    const result = await response.json();

    if (result.success) {
      showViewModal(result.data, cargo, title);
    } else {
      alertManager.error(result.message);
    }
  } catch (error) {
    console.error("Error al obtener detalles:", error);
    alertManager.error("Error al cargar los detalles del personal");
  }
}

function showViewModal(data, cargo, title) {
  const modal = document.getElementById("viewModal");
  const modalTitle = document.getElementById("viewModalTitle");
  const modalBody = document.getElementById("viewModalBody");

  modalTitle.textContent = title;

  let html = "";

  if (cargo === "MEDICO GENERAL") {
    html = `
      <div class="personal-detail-grid">
        <div class="personal-detail-item"><div class="personal-detail-label">DNI:</div><div class="personal-detail-value">${
          data.numeroDni
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Nombre:</div><div class="personal-detail-value">${
          data.nombre
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Paterno:</div><div class="personal-detail-value">${
          data.apellidoPaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Materno:</div><div class="personal-detail-value">${
          data.apellidoMaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Sexo:</div><div class="personal-detail-value">${
          data.sexo === "M" ? "Masculino" : "Femenino"
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Nacimiento:</div><div class="personal-detail-value">${formatDate(
          data.fechaNacimiento
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Dirección:</div><div class="personal-detail-value">${
          data.direccion
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Teléfono:</div><div class="personal-detail-value">${
          data.telefono
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Correo Electrónico:</div><div class="personal-detail-value">${
          data.correoElectronico
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Estado Laboral:</div><div class="personal-detail-value"><span class="status-badge ${
          data.estadoLaboral.toLowerCase() === "activo" ? "active" : "inactive"
        }">${data.estadoLaboral}</span></div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Ingreso:</div><div class="personal-detail-value">${formatDate(
          data.fechaIngreso
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Turno:</div><div class="personal-detail-value">${
          data.turno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Área/Servicio:</div><div class="personal-detail-value">${
          data.areaServicio
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Cargo:</div><div class="personal-detail-value">${
          data.cargoMedico
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Número de Colegiatura:</div><div class="personal-detail-value">${
          data.numeroColegiatura
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Tipo de Médico:</div><div class="personal-detail-value">${
          data.tipoMedico
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">ID Consultorio:</div><div class="personal-detail-value">${
          data.idConsultorio || "N/A"
        }</div></div>
      </div>
    `;
  } else if (cargo === "ENFERMERIA") {
    const personal = data.personal;
    html = `
      <h4 style="margin-bottom: 15px; color: #2c3e50;">Datos Personales</h4>
      <div class="personal-detail-grid">
        <div class="personal-detail-item"><div class="personal-detail-label">DNI:</div><div class="personal-detail-value">${
          personal.numeroDni
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Nombre:</div><div class="personal-detail-value">${
          personal.nombre
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Paterno:</div><div class="personal-detail-value">${
          personal.apellidoPaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Materno:</div><div class="personal-detail-value">${
          personal.apellidoMaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Sexo:</div><div class="personal-detail-value">${
          personal.sexo === "M" ? "Masculino" : "Femenino"
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Nacimiento:</div><div class="personal-detail-value">${formatDate(
          personal.fechaNacimiento
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Dirección:</div><div class="personal-detail-value">${
          personal.direccion
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Teléfono:</div><div class="personal-detail-value">${
          personal.telefono
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Email:</div><div class="personal-detail-value">${
          personal.email
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Estado Laboral:</div><div class="personal-detail-value"><span class="status-badge ${
          personal.estadoLaboral.toLowerCase() === "activo"
            ? "active"
            : "inactive"
        }">${personal.estadoLaboral}</span></div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Ingreso:</div><div class="personal-detail-value">${formatDate(
          personal.fechaIngreso
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Turno:</div><div class="personal-detail-value">${
          personal.turno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Área/Servicio:</div><div class="personal-detail-value">${
          personal.areaServicio
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Cargo:</div><div class="personal-detail-value">${
          personal.cargo
        }</div></div>
      </div>
      <h4 style="margin: 25px 0 15px; color: #2c3e50;">Información de Enfermería</h4>
      <div class="personal-detail-grid">
        <div class="personal-detail-item"><div class="personal-detail-label">ID Enfermería:</div><div class="personal-detail-value">${
          data.idEnfermeria
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Número de Colegiatura:</div><div class="personal-detail-value">${
          data.numeroColegiaturaEnfermeria
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Nivel Profesional:</div><div class="personal-detail-value">${
          data.nivelProfesional
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">ID Consultorio:</div><div class="personal-detail-value">${
          data.idConsultorio || "N/A"
        }</div></div>
      </div>
    `;
  } else {
    html = `
      <div class="personal-detail-grid">
        <div class="personal-detail-item"><div class="personal-detail-label">DNI:</div><div class="personal-detail-value">${
          data.numeroDni
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Nombre:</div><div class="personal-detail-value">${
          data.nombre
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Paterno:</div><div class="personal-detail-value">${
          data.apellidoPaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Apellido Materno:</div><div class="personal-detail-value">${
          data.apellidoMaterno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Sexo:</div><div class="personal-detail-value">${
          data.sexo === "M" ? "Masculino" : "Femenino"
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Nacimiento:</div><div class="personal-detail-value">${formatDate(
          data.fechaNacimiento
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Dirección:</div><div class="personal-detail-value">${
          data.direccion
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Teléfono:</div><div class="personal-detail-value">${
          data.telefono
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Email:</div><div class="personal-detail-value">${
          data.email
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Estado Laboral:</div><div class="personal-detail-value"><span class="status-badge ${
          data.estadoLaboral.toLowerCase() === "activo" ? "active" : "inactive"
        }">${data.estadoLaboral}</span></div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Fecha de Ingreso:</div><div class="personal-detail-value">${formatDate(
          data.fechaIngreso
        )}</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Turno:</div><div class="personal-detail-value">${
          data.turno
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Área/Servicio:</div><div class="personal-detail-value">${
          data.areaServicio
        }</div></div>
        <div class="personal-detail-item"><div class="personal-detail-label">Cargo:</div><div class="personal-detail-value">${
          data.cargo
        }</div></div>
      </div>
    `;
  }

  modalBody.innerHTML = html;
  modal.classList.add("show");
}

function closeViewModal() {
  const modal = document.getElementById("viewModal");
  modal.classList.remove("show");
}

// ==================== EDITAR ====================
async function editPersonal(id, cargo) {
  try {
    let endpoint = "";
    let title = "";

    if (cargo === "MEDICO GENERAL") {
      endpoint = `/medicos/${id}`;
      title = "Actualizar Médico";
    } else if (cargo === "ENFERMERIA") {
      endpoint = `/enfermerias/${id}`;
      title = "Actualizar Enfermería";
    } else if (
      cargo === "CAJERO" ||
      cargo === "ADMINISTRADOR" ||
      cargo === "ADMISION"
    ) {
      endpoint = `/personal-tecnico/${id}`;
      title = "Actualizar Personal Técnico";
    }

    const response = await fetch(`${API_BASE_URL}${endpoint}`);
    const result = await response.json();

    handleApiResponse(result, (data) => {
      showEditModal(data, cargo, title);
    });
  } catch (error) {
    console.error("Error al obtener datos para editar:", error);
    alertManager.error("Error al cargar los datos del personal");
  }
}

function showEditModal(data, cargo, title) {
  const modal = document.getElementById("editModal");
  const modalTitle = document.getElementById("editModalTitle");

  modalTitle.textContent = title;

  populateConsultorioSelects();

  document.getElementById("medicoSection").style.display = "none";
  document.getElementById("enfermeriaSection").style.display = "none";

  const turnoSelect = document.getElementById("edit_turno");
  turnoSelect.innerHTML = '<option value="">Seleccione...</option>';

  if (cargo === "ADMINISTRADOR") {
    turnoSelect.innerHTML += '<option value="Mañana">Mañana</option>';
    turnoSelect.innerHTML += '<option value="Tarde">Tarde</option>';
    turnoSelect.innerHTML += '<option value="Noche">Noche</option>';
  } else {
    turnoSelect.innerHTML += '<option value="Mañana">Mañana</option>';
    turnoSelect.innerHTML += '<option value="Tarde">Tarde</option>';
  }

  if (cargo === "MEDICO GENERAL") {
    document.getElementById("edit_id").value = data.idMedico;
    document.getElementById("edit_cargo").value = cargo;
    document.getElementById("edit_dni").value = data.numeroDni;
    document.getElementById(
      "edit_nombre"
    ).value = `${data.nombre} ${data.apellidoPaterno} ${data.apellidoMaterno}`;
    document.getElementById("edit_fechaNacimiento").value = formatDateForInput(
      data.fechaNacimiento
    );
    document.getElementById("edit_sexo").value = data.sexo;
    document.getElementById("edit_direccion").value = data.direccion;
    document.getElementById("edit_telefono").value = data.telefono;
    document.getElementById("edit_email").value = data.correoElectronico;
    document.getElementById("edit_estadoLaboral").value = data.estadoLaboral;
    document.getElementById("edit_fechaIngreso").value = formatDateForInput(
      data.fechaIngreso
    );
    document.getElementById("edit_turno").value = data.turno;
    document.getElementById("edit_areaServicio").value = data.areaServicio;
    document.getElementById("edit_cargoDisplay").value = data.cargoMedico;

    document.getElementById("medicoSection").style.display = "block";
    document.getElementById("edit_numeroColegiatura").value =
      data.numeroColegiatura;
    document.getElementById("edit_tipoMedico").value = data.tipoMedico;
    document.getElementById("edit_consultorioMedico").value =
      data.idConsultorio || "";
  } else if (cargo === "ENFERMERIA") {
    const personal = data.personal;
    document.getElementById("edit_id").value = data.idEnfermeria;
    document.getElementById("edit_cargo").value = cargo;
    document.getElementById("edit_dni").value = personal.numeroDni;
    document.getElementById(
      "edit_nombre"
    ).value = `${personal.nombre} ${personal.apellidoPaterno} ${personal.apellidoMaterno}`;
    document.getElementById("edit_fechaNacimiento").value = formatDateForInput(
      personal.fechaNacimiento
    );
    document.getElementById("edit_sexo").value = personal.sexo;
    document.getElementById("edit_direccion").value = personal.direccion;
    document.getElementById("edit_telefono").value = personal.telefono;
    document.getElementById("edit_email").value = personal.email;
    document.getElementById("edit_estadoLaboral").value =
      personal.estadoLaboral;
    document.getElementById("edit_fechaIngreso").value = formatDateForInput(
      personal.fechaIngreso
    );
    document.getElementById("edit_turno").value = personal.turno;
    document.getElementById("edit_areaServicio").value = personal.areaServicio;
    document.getElementById("edit_cargoDisplay").value = personal.cargo;

    document.getElementById("enfermeriaSection").style.display = "block";
    document.getElementById("edit_numeroColegiaturaEnf").value =
      data.numeroColegiaturaEnfermeria;
    document.getElementById("edit_nivelProfesional").value =
      data.nivelProfesional;
    document.getElementById("edit_consultorioEnf").value =
      data.idConsultorio || "";

    document
      .getElementById("edit_id")
      .setAttribute("data-id-personal", data.idPersonal);
  } else {
    document.getElementById("edit_id").value = data.idPersonal;
    document.getElementById("edit_cargo").value = cargo;
    document.getElementById("edit_dni").value = data.numeroDni;
    document.getElementById(
      "edit_nombre"
    ).value = `${data.nombre} ${data.apellidoPaterno} ${data.apellidoMaterno}`;
    document.getElementById("edit_fechaNacimiento").value = formatDateForInput(
      data.fechaNacimiento
    );
    document.getElementById("edit_sexo").value = data.sexo;
    document.getElementById("edit_direccion").value = data.direccion;
    document.getElementById("edit_telefono").value = data.telefono;
    document.getElementById("edit_email").value = data.email;
    document.getElementById("edit_estadoLaboral").value = data.estadoLaboral;
    document.getElementById("edit_fechaIngreso").value = formatDateForInput(
      data.fechaIngreso
    );
    document.getElementById("edit_turno").value = data.turno;
    document.getElementById("edit_areaServicio").value = data.areaServicio;
    document.getElementById("edit_cargoDisplay").value = data.cargo;
  }

  modal.classList.add("show");
}

function populateConsultorioSelects() {
  const medicoSelect = document.getElementById("edit_consultorioMedico");
  const enfSelect = document.getElementById("edit_consultorioEnf");

  const options =
    '<option value="">Seleccione...</option>' +
    consultoriosList
      .map((c) => `<option value="${c.idConsultorio}">${c.nombre}</option>`)
      .join("");

  medicoSelect.innerHTML = options;
  enfSelect.innerHTML = options;
}

// ==================== GUARDAR EDICIÓN (SIN RECARGA) ====================
async function saveEdit() {
  const cargo = document.getElementById("edit_cargo").value;
  const id = document.getElementById("edit_id").value;

  const nombreCompleto = document
    .getElementById("edit_nombre")
    .value.trim()
    .split(" ");
  const nombre = nombreCompleto[0] || "";
  const apellidoPaterno = nombreCompleto[1] || "";
  const apellidoMaterno = nombreCompleto.slice(2).join(" ") || "";

  let payload = {};
  let endpoint = "";

  if (cargo === "MEDICO GENERAL") {
    payload = {
      idMedico: parseInt(id),
      numeroDni: document.getElementById("edit_dni").value,
      nombre: nombre,
      apellidoPaterno: apellidoPaterno,
      apellidoMaterno: apellidoMaterno,
      sexo: document.getElementById("edit_sexo").value,
      fechaNacimiento: document.getElementById("edit_fechaNacimiento").value,
      direccion: document.getElementById("edit_direccion").value,
      telefono: document.getElementById("edit_telefono").value,
      correoElectronico: document.getElementById("edit_email").value,
      estadoLaboral: document.getElementById("edit_estadoLaboral").value,
      fechaIngreso: document.getElementById("edit_fechaIngreso").value,
      turno: document.getElementById("edit_turno").value,
      areaServicio: document.getElementById("edit_areaServicio").value,
      cargoMedico: "MEDICO GENERAL",
      numeroColegiatura: document.getElementById("edit_numeroColegiatura")
        .value,
      tipoMedico: document.getElementById("edit_tipoMedico").value,
      idConsultorio:
        parseInt(document.getElementById("edit_consultorioMedico").value) ||
        null,
    };
    endpoint = "/medicos/update";
  } else if (cargo === "ENFERMERIA") {
    const idPersonal = document
      .getElementById("edit_id")
      .getAttribute("data-id-personal");

    payload = {
      idEnfermeria: parseInt(id),
      idPersonal: parseInt(idPersonal),
      numeroColegiaturaEnfermeria: document.getElementById(
        "edit_numeroColegiaturaEnf"
      ).value,
      nivelProfesional: document.getElementById("edit_nivelProfesional").value,
      numeroDni: document.getElementById("edit_dni").value,
      nombre: nombre,
      apellidoPaterno: apellidoPaterno,
      apellidoMaterno: apellidoMaterno,
      fechaNacimiento: document.getElementById("edit_fechaNacimiento").value,
      sexo: document.getElementById("edit_sexo").value,
      direccion: document.getElementById("edit_direccion").value,
      telefono: document.getElementById("edit_telefono").value,
      email: document.getElementById("edit_email").value,
      estadoLaboral: document.getElementById("edit_estadoLaboral").value,
      fechaIngreso: document.getElementById("edit_fechaIngreso").value,
      turno: document.getElementById("edit_turno").value,
      areaServicio: document.getElementById("edit_areaServicio").value,
      cargo: "ENFERMERIA",
      idConsultorio:
        parseInt(document.getElementById("edit_consultorioEnf").value) || null,
    };
    endpoint = "/enfermerias/update";
  } else {
    payload = {
      idPersonalT: parseInt(id),
      numeroDni: document.getElementById("edit_dni").value,
      nombre: nombre,
      apellidoPaterno: apellidoPaterno,
      apellidoMaterno: apellidoMaterno,
      fechaNacimiento: document.getElementById("edit_fechaNacimiento").value,
      sexo: document.getElementById("edit_sexo").value,
      direccion: document.getElementById("edit_direccion").value,
      telefono: document.getElementById("edit_telefono").value,
      email: document.getElementById("edit_email").value,
      estadoLaboral: document.getElementById("edit_estadoLaboral").value,
      fechaIngreso: document.getElementById("edit_fechaIngreso").value,
      turno: document.getElementById("edit_turno").value,
      areaServicio: document.getElementById("edit_areaServicio").value,
      cargo: cargo,
    };
    endpoint = "/personal-tecnico/update";
  }

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: "PUT",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    const result = await response.json();

    handleApiResponse(result, async () => {
      closeEditModal();
      await loadResumen();
      await executeSearchWithCurrentFilters();
    });
  } catch (error) {
    console.error("Error al actualizar:", error);
    alertManager.error("Error al actualizar el personal");
  }
}

function closeEditModal() {
  const modal = document.getElementById("editModal");
  modal.classList.remove("show");
  document.getElementById("editForm").reset();
}

// ==================== ELIMINAR ====================
function deletePersonal(id, cargo, nombre, dni) {
  const modal = document.getElementById("deleteModal");

  document.getElementById("delete_id").value = id;
  document.getElementById("delete_tipoPersonal").value = cargo;
  document.getElementById("delete_nombre").textContent = nombre;
  document.getElementById("delete_cargo").textContent = cargo;
  document.getElementById("delete_dni").textContent = dni;

  modal.classList.add("show");
}

// ==================== CONFIRMAR ELIMINACIÓN (SIN RECARGA) ====================
async function confirmDelete() {
  const event = window.event || arguments.callee.caller.arguments[0];

  if (event) {
    event.preventDefault();
    event.stopPropagation();
  }

  const id = document.getElementById("delete_id").value;
  const cargo = document.getElementById("delete_tipoPersonal").value;

  let endpoint = "";

  if (cargo === "MEDICO GENERAL") {
    endpoint = `/medicos/delete/${id}`;
  } else if (cargo === "ENFERMERIA") {
    endpoint = `/enfermerias/delete/${id}`;
  } else {
    endpoint = `/personal-tecnico/delete/${id}`;
  }

  try {
    const response = await fetch(`${API_BASE_URL}${endpoint}`, {
      method: "DELETE",
    });

    const result = await response.json();

    handleApiResponse(result, async () => {
      closeDeleteModal();
      await loadResumen();
      await executeSearchWithCurrentFilters();
    });
  } catch (error) {
    console.error("Error al eliminar:", error);
    alertManager.error("Error al eliminar el personal");
  }
}

function closeDeleteModal() {
  const modal = document.getElementById("deleteModal");
  modal.classList.remove("show");
}

// ==================== REGISTRAR MÉDICO (SIN RECARGA) ====================
function showAddMedicoModal() {
  const modal = document.getElementById("addMedicoModal");
  document.getElementById("addMedicoForm").reset();
  clearModalErrors("add_medico");

  const consultorioSelect = document.getElementById("add_medico_consultorio");
  consultorioSelect.innerHTML =
    '<option value="">Seleccione...</option>' +
    consultoriosList
      .map((c) => `<option value="${c.idConsultorio}">${c.nombre}</option>`)
      .join("");

  modal.classList.add("show");
}

function closeAddMedicoModal() {
  const modal = document.getElementById("addMedicoModal");
  modal.classList.remove("show");
  document.getElementById("addMedicoForm").reset();
  clearModalErrors("add_medico");
}

async function saveAddMedico() {
  clearModalErrors("add_medico");

  const payload = {
    NumeroDni: document.getElementById("add_medico_dni").value.trim(),
    Nombre: document.getElementById("add_medico_nombre").value.trim(),
    ApellidoPaterno: document
      .getElementById("add_medico_apellidoPaterno")
      .value.trim(),
    ApellidoMaterno:
      document.getElementById("add_medico_apellidoMaterno").value.trim() ||
      null,
    Sexo: document.getElementById("add_medico_sexo").value,
    FechaNacimiento: document.getElementById("add_medico_fechaNacimiento")
      .value,
    Direccion:
      document.getElementById("add_medico_direccion").value.trim() || null,
    Telefono:
      document.getElementById("add_medico_telefono").value.trim() || null,
    CorreoElectronico: document.getElementById("add_medico_email").value.trim(),
    EstadoLaboral: document.getElementById("add_medico_estadoLaboral").value,
    FechaIngreso: document.getElementById("add_medico_fechaIngreso").value,
    Turno: document.getElementById("add_medico_turno").value,
    AreaServicio: document
      .getElementById("add_medico_areaServicio")
      .value.trim(),
    CargoMedico: "MEDICO GENERAL",
    NumeroColegiatura: document
      .getElementById("add_medico_numeroColegiatura")
      .value.trim(),
    TipoMedico: document.getElementById("add_medico_tipoMedico").value,
    IdConsultorio:
      parseInt(document.getElementById("add_medico_consultorio").value) || null,
  };

  try {
    const response = await fetch(`${API_BASE_URL}/medicos/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    const result = await response.json();

    if (result.success) {
      alertManager.success(result.message);
      closeAddMedicoModal();
      await loadResumen();
      await executeSearchWithCurrentFilters();
    } else {
      if (result.message) {
        alertManager.error(result.message);
      }

      if (result.data && Array.isArray(result.data)) {
        result.data.forEach((error) => {
          displayModalFieldError("add_medico", error.field, error.errors);
        });
      }
    }
  } catch (error) {
    console.error("Error al registrar médico:", error);
    alertManager.error("Error al registrar el médico");
  }
}

// ==================== REGISTRAR ENFERMERÍA (SIN RECARGA) ====================
function showAddEnfermeriaModal() {
  const modal = document.getElementById("addEnfermeriaModal");
  document.getElementById("addEnfermeriaForm").reset();
  clearModalErrors("add_enf");

  const consultorioSelect = document.getElementById("add_enf_consultorio");
  consultorioSelect.innerHTML =
    '<option value="">Seleccione...</option>' +
    consultoriosList
      .map((c) => `<option value="${c.idConsultorio}">${c.nombre}</option>`)
      .join("");

  modal.classList.add("show");
}

function closeAddEnfermeriaModal() {
  const modal = document.getElementById("addEnfermeriaModal");
  modal.classList.remove("show");
  document.getElementById("addEnfermeriaForm").reset();
  clearModalErrors("add_enf");
}

async function saveAddEnfermeria() {
  clearModalErrors("add_enf");

  const payload = {
    NumeroDni: document.getElementById("add_enf_dni").value.trim(),
    Nombre: document.getElementById("add_enf_nombre").value.trim(),
    ApellidoPaterno: document
      .getElementById("add_enf_apellidoPaterno")
      .value.trim(),
    ApellidoMaterno:
      document.getElementById("add_enf_apellidoMaterno").value.trim() || null,
    FechaNacimiento: document.getElementById("add_enf_fechaNacimiento").value,
    Sexo: document.getElementById("add_enf_sexo").value,
    Direccion:
      document.getElementById("add_enf_direccion").value.trim() || null,
    Telefono: document.getElementById("add_enf_telefono").value.trim() || null,
    Email: document.getElementById("add_enf_email").value.trim(),
    EstadoLaboral: document.getElementById("add_enf_estadoLaboral").value,
    FechaIngreso: document.getElementById("add_enf_fechaIngreso").value,
    Turno: document.getElementById("add_enf_turno").value,
    AreaServicio: document.getElementById("add_enf_areaServicio").value.trim(),
    Cargo: "ENFERMERIA",
    NumeroColegiaturaEnfermeria:
      document.getElementById("add_enf_numeroColegiatura").value.trim() || null,
    NivelProfesional:
      document.getElementById("add_enf_nivelProfesional").value || null,
    IdConsultorio:
      parseInt(document.getElementById("add_enf_consultorio").value) || null,
  };

  try {
    const response = await fetch(`${API_BASE_URL}/enfermerias/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    const result = await response.json();

    if (result.success) {
      alertManager.success(result.message);
      closeAddEnfermeriaModal();
      await loadResumen();
      await executeSearchWithCurrentFilters();
    } else {
      if (result.message) {
        alertManager.error(result.message);
      }

      if (result.data && Array.isArray(result.data)) {
        result.data.forEach((error) => {
          displayModalFieldError("add_enf", error.field, error.errors);
        });
      }
    }
  } catch (error) {
    console.error("Error al registrar enfermería:", error);
    alertManager.error("Error al registrar la enfermería");
  }
}

// ==================== REGISTRAR PERSONAL TÉCNICO (SIN RECARGA) ====================
function showAddTecnicoModal() {
  const modal = document.getElementById("addTecnicoModal");
  document.getElementById("addTecnicoForm").reset();
  clearModalErrors("add_tec");
  modal.classList.add("show");
}

function closeAddTecnicoModal() {
  const modal = document.getElementById("addTecnicoModal");
  modal.classList.remove("show");
  document.getElementById("addTecnicoForm").reset();
  clearModalErrors("add_tec");
}

async function saveAddTecnico() {
  clearModalErrors("add_tec");

  const payload = {
    NumeroDni: document.getElementById("add_tec_dni").value.trim(),
    Nombre: document.getElementById("add_tec_nombre").value.trim(),
    ApellidoPaterno: document
      .getElementById("add_tec_apellidoPaterno")
      .value.trim(),
    ApellidoMaterno:
      document.getElementById("add_tec_apellidoMaterno").value.trim() || null,
    FechaNacimiento: document.getElementById("add_tec_fechaNacimiento").value,
    Sexo: document.getElementById("add_tec_sexo").value,
    Direccion:
      document.getElementById("add_tec_direccion").value.trim() || null,
    Telefono: document.getElementById("add_tec_telefono").value.trim() || null,
    Email: document.getElementById("add_tec_email").value.trim(),
    EstadoLaboral: document.getElementById("add_tec_estadoLaboral").value,
    FechaIngreso: document.getElementById("add_tec_fechaIngreso").value,
    Turno: document.getElementById("add_tec_turno").value,
    AreaServicio: document.getElementById("add_tec_areaServicio").value.trim(),
    Cargo: document.getElementById("add_tec_cargo").value,
  };

  try {
    const response = await fetch(`${API_BASE_URL}/personal-tecnico/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(payload),
    });

    const result = await response.json();

    if (result.success) {
      alertManager.success(result.message);
      closeAddTecnicoModal();
      await loadResumen();
      await executeSearchWithCurrentFilters();
    } else {
      if (result.message) {
        alertManager.error(result.message);
      }

      if (result.data && Array.isArray(result.data)) {
        result.data.forEach((error) => {
          displayModalFieldError("add_tec", error.field, error.errors);
        });
      }
    }
  } catch (error) {
    console.error("Error al registrar personal técnico:", error);
    alertManager.error("Error al registrar el personal técnico");
  }
}

// ==================== FUNCIONES AUXILIARES PARA MODALES ====================
function displayModalFieldError(prefix, field, errors) {
  const errorElementId = `error-${prefix}_${field}`;
  const errorElement = document.getElementById(errorElementId);
  const inputElementId = `${prefix}_${field.toLowerCase()}`;
  const inputElement = document.getElementById(inputElementId);

  if (errorElement && errors && errors.length > 0) {
    errorElement.textContent = errors[0];
    errorElement.classList.add("show");
  }

  if (inputElement) {
    inputElement.classList.add("error");
  }
}

function clearModalErrors(prefix) {
  const errorMessages = document.querySelectorAll(`[id^="error-${prefix}_"]`);
  const errorInputs = document.querySelectorAll(`[id^="${prefix}_"].error`);

  errorMessages.forEach((el) => {
    el.classList.remove("show");
    el.textContent = "";
  });

  errorInputs.forEach((el) => {
    el.classList.remove("error");
  });
}

// ==================== UTILITY FUNCTIONS ====================
function formatDate(dateString) {
  if (!dateString) return "N/A";
  const date = new Date(dateString);
  return date.toLocaleDateString("es-ES", {
    year: "numeric",
    month: "long",
    day: "numeric",
  });
}

function formatDateForInput(dateString) {
  if (!dateString) return "";
  const date = new Date(dateString);
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

// ==================== EVENT LISTENERS PARA CERRAR MODALES ====================
window.addEventListener("click", function (event) {
  const viewModal = document.getElementById("viewModal");
  const editModal = document.getElementById("editModal");
  const deleteModal = document.getElementById("deleteModal");
  const addMedicoModal = document.getElementById("addMedicoModal");
  const addEnfermeriaModal = document.getElementById("addEnfermeriaModal");
  const addTecnicoModal = document.getElementById("addTecnicoModal");

  if (event.target === viewModal) closeViewModal();
  if (event.target === editModal) closeEditModal();
  if (event.target === deleteModal) closeDeleteModal();
  if (event.target === addMedicoModal) closeAddMedicoModal();
  if (event.target === addEnfermeriaModal) closeAddEnfermeriaModal();
  if (event.target === addTecnicoModal) closeAddTecnicoModal();
});

window.addEventListener("keydown", function (event) {
  if (event.key === "Escape") {
    const viewModal = document.getElementById("viewModal");
    const editModal = document.getElementById("editModal");
    const deleteModal = document.getElementById("deleteModal");
    const addMedicoModal = document.getElementById("addMedicoModal");
    const addEnfermeriaModal = document.getElementById("addEnfermeriaModal");
    const addTecnicoModal = document.getElementById("addTecnicoModal");

    if (viewModal.classList.contains("show")) closeViewModal();
    if (editModal.classList.contains("show")) closeEditModal();
    if (deleteModal.classList.contains("show")) closeDeleteModal();
    if (addMedicoModal.classList.contains("show")) closeAddMedicoModal();
    if (addEnfermeriaModal.classList.contains("show"))
      closeAddEnfermeriaModal();
    if (addTecnicoModal.classList.contains("show")) closeAddTecnicoModal();
  }
});

// ==================== FUNCIÓN DE TEST ====================
function testCompleteFlow() {
  console.log("🧪 INICIANDO TEST COMPLETO");

  currentFilters = { Nombre: "test", TipoPersonal: "MEDICO GENERAL" };
  saveFiltersToStorage();

  executeSearchWithCurrentFilters().then(() => {
    console.log("🧪 TEST COMPLETADO - Verifica si la página se recargó");
  });
}

// ==================== HACER FUNCIONES GLOBALES ====================
window.viewPersonal = viewPersonal;
window.editPersonal = editPersonal;
window.deletePersonal = deletePersonal;
window.confirmDelete = confirmDelete;
window.closeViewModal = closeViewModal;
window.closeEditModal = closeEditModal;
window.closeDeleteModal = closeDeleteModal;
window.closeAddMedicoModal = closeAddMedicoModal;
window.closeAddEnfermeriaModal = closeAddEnfermeriaModal;
window.closeAddTecnicoModal = closeAddTecnicoModal;
window.saveEdit = saveEdit;
window.saveAddMedico = saveAddMedico;
window.saveAddEnfermeria = saveAddEnfermeria;
window.saveAddTecnico = saveAddTecnico;
