const API_BASE_URL = "http://localhost:5122";

// Inicialización
document.addEventListener("DOMContentLoaded", function () {
  loadResumenPacientes();
  initializePacienteEvents();
});

// Inicializar eventos
function initializePacienteEvents() {
  const searchForm = document.getElementById("searchPacienteForm");
  const clearBtn = document.getElementById("clearPacienteBtn");

  if (searchForm) {
    searchForm.addEventListener("submit", handlePacienteSearch);
  }
  if (clearBtn) {
    clearBtn.addEventListener("click", clearPacienteFilters);
  }
}

// Cargar resumen de pacientes
async function loadResumenPacientes() {
  try {
    const data = {
      totalPacientes: 0,
      hombres: 0,
      mujeres: 0,
      registradosHoy: 0,
    };
    renderSummaryCardsPaciente(data);
  } catch (error) {
    showAlert("Error al cargar el resumen de pacientes.", "error");
  }
}

// Renderizar tarjetas resumen
function renderSummaryCardsPaciente(data) {
  const container = document.getElementById("summaryCardsPaciente");
  if (!container) return;
  container.innerHTML = `
    <div class="summary-card">
        <div class="card-icon blue"><i class="fas fa-users"></i></div>
        <div class="card-value">${data.totalPacientes ?? 0}</div>
        <div class="card-label">Total Pacientes</div>
    </div>
    <div class="summary-card">
        <div class="card-icon green"><i class="fas fa-mars"></i></div>
        <div class="card-value">${data.hombres ?? 0}</div>
        <div class="card-label">Hombres</div>
    </div>
    <div class="summary-card">
        <div class="card-icon purple"><i class="fas fa-venus"></i></div>
        <div class="card-value">${data.mujeres ?? 0}</div>
        <div class="card-label">Mujeres</div>
    </div>
    <div class="summary-card">
        <div class="card-icon orange"><i class="fas fa-calendar-day"></i></div>
        <div class="card-value">${data.registradosHoy ?? 0}</div>
        <div class="card-label">Registrados Hoy</div>
    </div>
  `;
}

// Manejar búsqueda de pacientes
async function handlePacienteSearch(e) {
  e.preventDefault();
  clearPacienteErrors();

  const tipoDocumento = document.getElementById("tipoBusqueda").value;
  const numeroDocumento = document
    .getElementById("numeroDocumento")
    .value.trim();

  let hasError = false;
  if (!tipoDocumento) {
    displayPacienteFieldError(
      "TipoDocumento",
      "Seleccione un tipo de documento."
    );
    hasError = true;
  }
  if (!numeroDocumento) {
    displayPacienteFieldError(
      "NumeroDocumento",
      "Ingrese el número de documento."
    );
    hasError = true;
  }
  if (hasError) return;

  try {
    const res = await fetch(
      `${API_BASE_URL}/pacientes/search?tipoDocumento=${encodeURIComponent(
        tipoDocumento
      )}&numeroDocumento=${encodeURIComponent(numeroDocumento)}`
    );
    const result = await res.json();

    if (!result.success || !result.data) {
      renderPacienteTable([]);
      renderCitasPendientesTable([]);
      renderDerivacionesTable([]);
      showAlert(result.message || "No se encontró ningún paciente.", "info");
      return;
    }

    renderPacienteTable([result.data]);
    showAlert(result.message || "Paciente encontrado.", "success");

    const idPaciente = result.data.IdPaciente || result.data.idPaciente;
    await loadCitasPendientes(idPaciente);
    await loadDerivaciones(idPaciente);
  } catch (error) {
    console.error("Error:", error);
    showAlert("Error al buscar pacientes.", "error");
  }
}

// Renderizar tabla de pacientes
function renderPacienteTable(data) {
  const tbody = document.getElementById("pacienteTableBody");
  const addHistoriaSection = document.getElementById("addHistoriaSection");
  if (!tbody) return;

  if (!data || data.length === 0) {
    tbody.innerHTML = `<tr><td colspan="6" class="no-data">No hay datos disponibles. Use los filtros para buscar pacientes.</td></tr>`;
    if (addHistoriaSection) addHistoriaSection.style.display = "block";
    return;
  }

  if (addHistoriaSection) addHistoriaSection.style.display = "none";

  tbody.innerHTML = data
    .map(
      (p) => `
    <tr>
        <td>${p.TipoDocumento || p.tipoDocumento || ""}</td>
        <td>${p.NumeroDocumento || p.numeroDocumento || ""}</td>
        <td>${
          (p.ApellidoPaterno || p.apellidoPaterno || "") +
          " " +
          (p.ApellidoMaterno || p.apellidoMaterno || "") +
          ", " +
          (p.Nombre || p.nombre || "")
        }</td>
        <td>${
          (p.Edad !== undefined
            ? p.Edad
            : p.edad !== undefined
            ? p.edad
            : "-") + " años"
        }</td>
        <td>${
          p.Sexo === "M" || p.sexo === "M"
            ? "Masculino"
            : p.Sexo === "F" || p.sexo === "F"
            ? "Femenino"
            : ""
        }</td>
        <td>
            <div style="display: flex; gap: 8px;">
                <button style="display: inline-flex; align-items: center; justify-content: center; width: 40px; height: 40px; border-radius: 6px; border: none; cursor: pointer; background-color: #007bff; color: white; transition: all 0.2s ease;" 
                        title="Editar Historia Clínica" 
                        onclick="editarHistoriaClinica(${
                          p.IdPaciente || p.idPaciente
                        })"
                        onmouseover="this.style.backgroundColor='#0056b3'; this.style.transform='translateY(-2px)';"
                        onmouseout="this.style.backgroundColor='#007bff'; this.style.transform='translateY(0)';">
                    <i class="fas fa-edit" style="margin: 0;"></i>
                </button>
                <button style="display: inline-flex; align-items: center; justify-content: center; padding: 8px 12px; border-radius: 6px; border: none; cursor: pointer; background-color: #28a745; color: white; transition: all 0.2s ease; font-size: 14px; font-weight: 500;" 
                        title="Solicitar Cita" 
                        onclick="solicitarCita(${p.IdPaciente || p.idPaciente})"
                        onmouseover="this.style.backgroundColor='#218838'; this.style.transform='translateY(-2px)';"
                        onmouseout="this.style.backgroundColor='#28a745'; this.style.transform='translateY(0)';">
                    <i class="fas fa-calendar-plus" style="margin-right: 6px;"></i>
                    Solicitar Cita
                </button>
            </div>
        </td>
    </tr>
`
    )
    .join("");
}

function solicitarCita(idPaciente) {
  console.log("Solicitar cita para paciente ID:", idPaciente);
  if (idPaciente) {
    const url = `/Home/VisualCitas?idPaciente=${encodeURIComponent(
      idPaciente
    )}`;
    console.log("Redirigiendo a:", url);
    window.location.href = url;
  } else {
    console.error("No se pudo obtener el ID del paciente");
    alert("Error: No se pudo identificar al paciente");
  }
}

// Mostrar errores de campo
function displayPacienteFieldError(field, message) {
  const el = document.getElementById(`error-${field}`);
  if (el) {
    el.textContent = message;
    el.classList.add("show");
  }
}

// Limpiar errores
function clearPacienteErrors() {
  document.querySelectorAll(".error-message").forEach((e) => {
    e.textContent = "";
    e.classList.remove("show");
  });
}

// Limpiar filtros
function clearPacienteFilters() {
  document.getElementById("tipoBusqueda").value = "";
  document.getElementById("numeroDocumento").value = "";
  clearPacienteErrors();
  renderPacienteTable([]);
  renderCitasPendientesTable([]);
  renderDerivacionesTable([]);
  const addHistoriaSection = document.getElementById("addHistoriaSection");
  if (addHistoriaSection) addHistoriaSection.style.display = "none";
}

// Mostrar alertas
function showAlert(message, type = "success") {
  const alertContainer = document.getElementById("alertContainer");
  if (!alertContainer) return;
  const alert = document.createElement("div");
  alert.className = `alert alert-${type}`;
  alert.innerHTML = `
    <span>${message}</span>
    <button class="alert-close" onclick="this.parentElement.remove()">×</button>
  `;
  alertContainer.appendChild(alert);
  setTimeout(() => alert.remove(), 4000);
}

// Cargar citas pendientes de un paciente
async function loadCitasPendientes(idPaciente) {
  try {
    const res = await fetch(
      `${API_BASE_URL}/pacientes/${idPaciente}/citas-pendientes`
    );
    const result = await res.json();

    if (!result.success || !result.data) {
      renderCitasPendientesTable([]);
      return;
    }

    renderCitasPendientesTable(result.data);
  } catch (error) {
    console.error("Error al cargar citas pendientes:", error);
    renderCitasPendientesTable([]);
  }
}

// Renderizar tabla de citas pendientes
function renderCitasPendientesTable(citas) {
  const tbody = document.getElementById("citasPendientesBody");
  if (!tbody) return;

  if (!citas || citas.length === 0) {
    tbody.innerHTML = `<tr><td colspan="8" class="no-data">No hay citas pendientes.</td></tr>`;
    return;
  }

  tbody.innerHTML = citas
    .map((c) => {
      const fechaFormateada = c.fechaCita
        ? new Date(c.fechaCita).toLocaleDateString("es-PE")
        : "-";
      const horaFormateada = c.horaCita || "-";
      const estado = c.estadoCita || "Sin Estado";
      const estadoClass = getEstadoClass(estado);
      const idCita = c.idCita || c.IdCita;
      const idPaciente = c.idPaciente || c.IdPaciente;

      return `
        <tr>
            <td>${c.tipoDocumento || ""}</td>
            <td>${c.numeroDocumento || ""}</td>
            <td>${c.nombreCompletoPaciente || ""}</td>
            <td>${fechaFormateada}</td>
            <td>${horaFormateada}</td>
            <td><span class="estado-pill ${estadoClass}">${estado}</span></td>
            <td>${c.nombreCompletoMedico || ""}</td>
            <td>
                <div class="action-buttons">
                <button class="btn-action btn-recordatorio" 
                            onclick="enviarRecordatorio(${idCita})"
                            title="Enviar Recordatorio">
                        <i class="fas fa-bell"></i>
                        Recordatorio
                    </button>
                    <button class="btn-action btn-reprogramar" 
                            onclick="reprogramarCita(${idPaciente}, ${idCita})"
                            title="Reprogramar Cita">
                        <i class="fas fa-calendar-alt"></i>
                        Reprogramar
                    </button>
          <button class="btn-action btn-cancelar" 
              onclick="openCancelModal(${idCita})"
              title="Cancelar Cita">
                        <i class="fas fa-times-circle"></i>
                        Cancelar
                    </button>
                </div>
            </td>
        </tr>
      `;
    })
    .join("");
}

// Cargar derivaciones de un paciente
async function loadDerivaciones(idPaciente) {
  try {
    const res = await fetch(
      `${API_BASE_URL}/pacientes/${idPaciente}/derivaciones`
    );
    const result = await res.json();

    if (!result.success || !result.data) {
      renderDerivacionesTable([]);
      return;
    }

    renderDerivacionesTable(result.data);
  } catch (error) {
    console.error("Error al cargar derivaciones:", error);
    renderDerivacionesTable([]);
  }
}

// Renderizar tabla de derivaciones
function renderDerivacionesTable(derivaciones) {
  const tbody = document.getElementById("derivacionesBody");
  if (!tbody) return;

  if (!derivaciones || derivaciones.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" class="no-data">No hay derivaciones.</td></tr>`;
    return;
  }

  tbody.innerHTML = derivaciones
    .map((d) => {
      const fechaFormateada = d.fechaDerivacion
        ? new Date(d.fechaDerivacion).toLocaleDateString("es-PE")
        : "-";
      const estado = d.estadoDerivacion || "Sin Estado";
      const estadoClass = getEstadoClass(estado);

      return `
        <tr>
            <td>${d.tipoDocumento || ""}</td>
            <td>${d.numeroDocumento || ""}</td>
            <td>${d.medicoSolicitante || ""}</td>
            <td>${d.especialidadDestino || ""}</td>
            <td>${fechaFormateada}</td>
            <td><span class="estado-pill ${estadoClass}">${estado}</span></td>
            <td>
                <button class="btn-detalle" onclick="toggleDetalleDerivacion(${
                  d.idDerivacion
                })" id="btnDetalle${d.idDerivacion}">
                    <i class="fas fa-eye"></i>
                    Ver detalles
                </button>
            </td>
        </tr>
        <tr id="detalleRow${d.idDerivacion}" style="display: none;">
            <td colspan="7">
                <div class="derivacion-detalle" id="detalle${d.idDerivacion}">
                    <h4 style="margin-top: 0; color: #007bff; font-size: 16px; margin-bottom: 15px;">
                        <i class="fas fa-file-medical"></i>
                        Detalles de la Derivación #${d.idDerivacion}
                    </h4>
                    <div class="detalle-grid">
                        <div class="detalle-item">
                            <span class="detalle-label">Paciente</span>
                            <span class="detalle-value">${
                              d.nombreCompletoPaciente || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Médico Solicitante</span>
                            <span class="detalle-value">${
                              d.medicoSolicitante || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Tipo Documento</span>
                            <span class="detalle-value">${
                              d.tipoDocumento || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Nro. Documento</span>
                            <span class="detalle-value">${
                              d.numeroDocumento || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Nro. Documento Médico</span>
                            <span class="detalle-value">${
                              d.numeroDocumentoMedico || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Especialidad Solicitante</span>
                            <span class="detalle-value">${
                              d.especialidadSolicitante || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Servicio Origen</span>
                            <span class="detalle-value">${
                              d.servicioOrigen || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Especialidad Destino</span>
                            <span class="detalle-value">${
                              d.especialidadDestino || ""
                            }</span>
                        </div>
                        <div class="detalle-item">
                            <span class="detalle-label">Fecha de Solicitud</span>
                            <span class="detalle-value">${fechaFormateada}</span>
                        </div>
                        <div class="detalle-item detalle-motivo">
                            <span class="detalle-label">Motivo de la Derivación</span>
                            <span class="detalle-value">${
                              d.motivoDerivacion || ""
                            }</span>
                        </div>
                    </div>
                </div>
            </td>
        </tr>
      `;
    })
    .join("");
}

// Toggle detalles de derivación
function toggleDetalleDerivacion(idDerivacion) {
  const detalleRow = document.getElementById(`detalleRow${idDerivacion}`);
  const detalle = document.getElementById(`detalle${idDerivacion}`);
  const btn = document.getElementById(`btnDetalle${idDerivacion}`);

  if (!detalleRow || !detalle || !btn) return;

  if (detalleRow.style.display === "none") {
    detalleRow.style.display = "table-row";
    detalle.classList.add("show");
    btn.classList.add("active");
    btn.innerHTML = '<i class="fas fa-eye-slash"></i> Ocultar detalles';
  } else {
    detalleRow.style.display = "none";
    detalle.classList.remove("show");
    btn.classList.remove("active");
    btn.innerHTML = '<i class="fas fa-eye"></i> Ver detalles';
  }
}

// Obtener clase CSS para el estado
function getEstadoClass(estado) {
  if (!estado) return "estado-default";

  const estadoNormalizado = estado.trim().toLowerCase();

  switch (estadoNormalizado) {
    case "confirmada":
    case "activo":
    case "atendida":
      return "estado-activo";
    case "pendiente":
      return "estado-pendiente";
    case "cancelada":
    case "inactivo":
      return "estado-inactivo";
    case "en curso":
      return "estado-en-curso";
    default:
      return "estado-default";
  }
}

// Reprogramar cita
function reprogramarCita(idPaciente, idCita) {
  console.log(
    "Reprogramar cita - Paciente ID:",
    idPaciente,
    "Cita ID:",
    idCita
  );

  if (!idPaciente || !idCita) {
    showAlert("Error: No se pudo identificar la cita o el paciente.", "error");
    return;
  }

  // Redirigir a VisualCitas con ambos parámetros
  const url = `/Home/VisualCitas?idPaciente=${encodeURIComponent(
    idPaciente
  )}&idCita=${encodeURIComponent(idCita)}`;
  console.log("Redirigiendo a:", url);
  window.location.href = url;
}

// Modal de confirmación para cancelar cita
let cancelModalCitaId = null;

function openCancelModal(idCita) {
  cancelModalCitaId = idCita;
  const modal = document.getElementById("cancelConfirmModal");
  const msg = document.getElementById("cancelModalMessage");
  if (msg)
    msg.innerText = `¿Está seguro de que desea cancelar la cita #${idCita}? Esta acción liberará el cupo en la semana del médico.`;
  if (modal) modal.style.display = "block";
}

function closeCancelModal() {
  cancelModalCitaId = null;
  const modal = document.getElementById("cancelConfirmModal");
  if (modal) modal.style.display = "none";
}

async function confirmCancelCita() {
  if (!cancelModalCitaId) return;

  try {
    const res = await fetch(
      `${API_BASE_URL}/citas/cancelar/${cancelModalCitaId}`,
      {
        method: "DELETE",
        headers: {
          "Content-Type": "application/json",
        },
      }
    );

    const result = await res.json();

    if (result && result.success) {
      showAlert(
        result.message || result.mensaje || "Cita cancelada exitosamente.",
        "success"
      );
      const pacienteId = getCurrentPacienteId();
      if (pacienteId) await loadCitasPendientes(pacienteId);
    } else {
      const msg =
        (result && (result.message || result.mensaje)) ||
        "No se pudo cancelar la cita.";
      // Si el backend indica que existe triaje, mostrar modal específico
      if (msg.toLowerCase().includes("triaje")) {
        openBlockedModal(msg);
      } else {
        showAlert(msg, "error");
      }
    }
  } catch (error) {
    console.error("Error al cancelar cita:", error);
    showAlert("Error al cancelar la cita.", "error");
  } finally {
    closeCancelModal();
  }
}

// Registrar el listener para el botón de confirmación del modal
document.addEventListener("DOMContentLoaded", function () {
  const confirmBtn = document.getElementById("confirmCancelBtn");
  if (confirmBtn) {
    confirmBtn.addEventListener("click", function () {
      confirmCancelCita();
    });
  }
});

// Obtener el ID del paciente actual (del último buscado)
function getCurrentPacienteId() {
  // Puedes guardar esto en una variable global cuando buscas
  const tbody = document.getElementById("pacienteTableBody");
  if (!tbody) return null;

  const firstRow = tbody.querySelector("tr");
  if (!firstRow || firstRow.classList.contains("no-data")) return null;

  // Intentar extraer del botón de editar
  const editBtn = firstRow.querySelector(
    "button[onclick*='editarHistoriaClinica']"
  );
  if (editBtn) {
    const match = editBtn.getAttribute("onclick").match(/\d+/);
    return match ? parseInt(match[0]) : null;
  }

  return null;
}

// Modal para mostrar que la cancelación está bloqueada por triaje
function openBlockedModal(message) {
  const modal = document.getElementById("cancelBlockedModal");
  const msgEl = document.getElementById("blockedModalMessage");
  if (msgEl)
    msgEl.innerText =
      message ||
      "El paciente cuenta con triaje; no se puede cancelar esta cita.";
  if (modal) modal.style.display = "block";
}

function closeBlockedModal() {
  const modal = document.getElementById("cancelBlockedModal");
  if (modal) modal.style.display = "none";
} // Función para enviar recordatorio - Redirige a la vista de confirmación
async function enviarRecordatorio(idCita) {
  console.log("=== Redirigiendo a vista de confirmación ===");
  console.log("ID de Cita:", idCita);

  if (!idCita) {
    showAlert("Error: No se pudo identificar la cita.", "error");
    return;
  }

  // Obtener el botón que se presionó
  const btn = event.target.closest("button");
  if (!btn) {
    console.error("No se pudo encontrar el botón");
    return;
  }

  // Guardar el contenido original del botón
  const originalHtml = btn.innerHTML;

  // Deshabilitar el botón y mostrar indicador de carga
  btn.disabled = true;
  btn.style.opacity = "0.6";
  btn.style.cursor = "not-allowed";
  btn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Cargando...';

  try {
    // Redirigir a la página de confirmación
    // PRUEBA 2: Visualizar resumen de recordatorio
    window.location.href = `/pacientes/confirmar-recordatorio/${idCita}`;
  } catch (error) {
    console.error("Error al cargar vista de confirmación:", error);
    showAlert("Error al cargar la vista de confirmación.", "error");

    // Restaurar el botón
    btn.disabled = false;
    btn.style.opacity = "1";
    btn.style.cursor = "pointer";
    btn.innerHTML = originalHtml;
  }
}
