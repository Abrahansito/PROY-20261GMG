const API_BASE_URL = "http://localhost:5122";
let idPacienteActual = null;
let triajesData = [];
let paginaActual = 1;
const itemsPorPagina = 5;

document.addEventListener("DOMContentLoaded", function () {
  obtenerParametrosURL();
  cargarHistorialTriaje();
});

function obtenerParametrosURL() {
  const params = new URLSearchParams(window.location.search);
  idPacienteActual = params.get("idPaciente");

  if (!idPacienteActual) {
    showError("No se especificó el ID del paciente");
    setTimeout(() => window.history.back(), 2000);
  }
}

async function cargarHistorialTriaje() {
  try {
    const res = await fetch(
      `${API_BASE_URL}/api/triaje/historial-paciente/${idPacienteActual}`
    );

    // Verificar si la respuesta es OK (status 200-299)
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`);
    }

    const result = await res.json();

    if (result.success) {
      mostrarInformacionPaciente(result.paciente);
      triajesData = result.triajes || [];
      mostrarHistorialTriajes();
    } else {
      // El servidor respondió correctamente pero con success: false
      showError(
        result.message ||
          "Error al cargar el historial de triajes. Por favor, intente de nuevo más tarde."
      );
    }
  } catch (error) {
    // Error de red, timeout, o error al parsear JSON
    console.error("Error:", error);
    showError(
      "Error al cargar el historial de triajes. Por favor, intente de nuevo más tarde."
    );
  }
}

function mostrarInformacionPaciente(paciente) {
  document.getElementById("patientInfo").innerHTML = `
        <h2 class="patient-info-title">Información del Paciente</h2>
        <div class="patient-info-grid">
            <div class="patient-info-item">
                <span class="patient-info-label">DNI</span>
                <span class="patient-info-value">${paciente.dni}</span>
            </div>
            <div class="patient-info-item">
                <span class="patient-info-label">Nombre Completo</span>
                <span class="patient-info-value">${paciente.nombreCompleto}</span>
            </div>
            <div class="patient-info-item">
                <span class="patient-info-label">Sexo</span>
                <span class="patient-info-value">${paciente.sexo}</span>
            </div>
            <div class="patient-info-item">
                <span class="patient-info-label">Edad</span>
                <span class="patient-info-value">${paciente.edad} años</span>
            </div>
            <div class="patient-info-item">
                <span class="patient-info-label">Seguro</span>
                <span class="patient-info-value">${paciente.seguro}</span>
            </div>
        </div>
    `;
}

function mostrarHistorialTriajes() {
  const container = document.getElementById("triajeHistoryContainer");

  if (triajesData.length === 0) {
    container.innerHTML = `
            <div class="no-data">
                <div class="no-data-icon">📋</div>
                <p>No hay triajes registrados para este paciente</p>
            </div>
        `;
    return;
  }

  try {
    // Calcular paginación
    const inicio = (paginaActual - 1) * itemsPorPagina;
    const fin = inicio + itemsPorPagina;
    const triajesPagina = triajesData.slice(inicio, fin);

    const tabla = `
        <table class="triaje-table">
            <thead>
                <tr>
                    <th>CÓD. TRIAJE</th>
                    <th>FECHA Y HORA</th>
                    <th>PRESIÓN ARTERIAL</th>
                    <th>TEMPERATURA</th>
                    <th>ACCIONES</th>
                </tr>
            </thead>
            <tbody>
                ${triajesPagina
                  .map((triaje, index) => {
                    try {
                      return `
                    <tr>
                        <td><strong>${
                          triaje.codigoTriaje || "N/A"
                        }</strong></td>
                        <td>${triaje.fechaHora || "N/A"}</td>
                        <td>${triaje.presionArterial || "N/A"}</td>
                        <td>${triaje.temperatura || "N/A"}</td>
                        <td>
                            <button class="btn-details" onclick="toggleDetalles(${
                              inicio + index
                            })">
                                Ver detalles
                            </button>
                        </td>
                    </tr>
                    <tr id="detalles-${inicio + index}" style="display: none;">
                        <td colspan="5">
                            <div class="triaje-details active">
                                <h3 class="details-title">Detalles del Triaje ${
                                  triaje.codigoTriaje || "N/A"
                                }</h3>
                                <div class="details-grid">
                                    <div class="details-section">
                                        <h4 class="details-section-title">Signos Vitales</h4>
                                        <div class="details-row">
                                            <span class="details-label">Temperatura (°C):</span>
                                            <span class="details-value">${
                                              triaje.signos?.temperatura ||
                                              "N/A"
                                            }</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Presión arterial:</span>
                                            <span class="details-value">${
                                              triaje.signos?.presionArterial ||
                                              "N/A"
                                            }/80 mmHg</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Frecuencia cardiaca:</span>
                                            <span class="details-value">${
                                              triaje.signos
                                                ?.frecuenciaCardiaca || "N/A"
                                            } lpm</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Saturación O₂:</span>
                                            <span class="details-value">${
                                              triaje.signos?.saturacion || "N/A"
                                            }%</span>
                                        </div>
                                    </div>

                                    <div class="details-section">
                                        <h4 class="details-section-title">Medidas Antropométricas</h4>
                                        <div class="details-row">
                                            <span class="details-label">Peso (kg):</span>
                                            <span class="details-value">${
                                              triaje.medidas?.peso || "N/A"
                                            }</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Talla (cm):</span>
                                            <span class="details-value">${
                                              triaje.medidas?.talla || "N/A"
                                            }</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">IMC:</span>
                                            <span class="details-value">${
                                              triaje.medidas?.imc || "N/A"
                                            } ${
                        triaje.medidas?.imcClasificacion
                          ? `(${triaje.medidas.imcClasificacion})`
                          : ""
                      }</span>
                                        </div>
                                    </div>

                                    <div class="details-section">
                                        <h4 class="details-section-title">Información del Triaje</h4>
                                        <div class="details-row">
                                            <span class="details-label">Fecha:</span>
                                            <span class="details-value">${
                                              triaje.informacion?.fecha || "N/A"
                                            }</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Hora:</span>
                                            <span class="details-value">${
                                              triaje.informacion?.hora || "N/A"
                                            }</span>
                                        </div>
                                        <div class="details-row">
                                            <span class="details-label">Realizado por:</span>
                                            <span class="details-value">${
                                              triaje.informacion
                                                ?.realizadoPor || "N/A"
                                            }</span>
                                        </div>
                                    </div>
                                </div>

                                ${
                                  triaje.observaciones
                                    ? `
                                    <div class="observaciones">
                                        <h4 class="observaciones-title">Observaciones</h4>
                                        <p class="observaciones-text">${triaje.observaciones}</p>
                                    </div>
                                `
                                    : ""
                                }
                            </div>
                        </td>
                    </tr>
                `;
                    } catch (error) {
                      console.error(
                        `Error al renderizar triaje en índice ${index}:`,
                        error
                      );
                      return `
                    <tr>
                        <td colspan="5" class="text-center" style="padding: 20px; color: #dc2626;">
                            Error al mostrar este registro
                        </td>
                    </tr>
                `;
                    }
                  })
                  .join("")}
            </tbody>
        </table>
        ${renderizarPaginacion()}
    `;

    container.innerHTML = tabla;
  } catch (error) {
    console.error("Error al renderizar historial de triajes:", error);
    container.innerHTML = `
      <div class="no-data">
          <div class="no-data-icon">⚠️</div>
          <p>Error al mostrar el historial de triajes</p>
      </div>
    `;
    showError(
      "No se pudieron cargar los detalles para este triaje. Intente nuevamente."
    );
  }
}

function toggleDetalles(index) {
  const detalleRow = document.getElementById(`detalles-${index}`);

  // Verificar si el triaje tiene los datos completos
  const triaje = triajesData[index];
  if (!triaje || !triaje.signos || !triaje.medidas || !triaje.informacion) {
    showError(
      "No se pudieron cargar los detalles para este triaje. Intente nuevamente."
    );
    return;
  }

  if (detalleRow.style.display === "none") {
    // Cerrar todos los demás detalles
    document.querySelectorAll('[id^="detalles-"]').forEach((row) => {
      row.style.display = "none";
    });
    // Abrir el seleccionado
    detalleRow.style.display = "table-row";
  } else {
    detalleRow.style.display = "none";
  }
}

function renderizarPaginacion() {
  const totalPaginas = Math.ceil(triajesData.length / itemsPorPagina);

  if (totalPaginas <= 1) return "";

  let html = '<div class="pagination">';

  // Botón anterior
  html += `
        <button class="pagination-btn" onclick="cambiarPagina(${
          paginaActual - 1
        })" ${paginaActual === 1 ? "disabled" : ""}>
            ← Anterior
        </button>
    `;

  // Números de página
  for (let i = 1; i <= totalPaginas; i++) {
    html += `
            <button class="pagination-btn ${
              i === paginaActual ? "active" : ""
            }" onclick="cambiarPagina(${i})">
                ${i}
            </button>
        `;
  }

  // Botón siguiente
  html += `
        <button class="pagination-btn" onclick="cambiarPagina(${
          paginaActual + 1
        })" ${paginaActual === totalPaginas ? "disabled" : ""}>
            Siguiente →
        </button>
    `;

  html += "</div>";
  return html;
}

function cambiarPagina(nuevaPagina) {
  const totalPaginas = Math.ceil(triajesData.length / itemsPorPagina);

  if (nuevaPagina < 1 || nuevaPagina > totalPaginas) return;

  paginaActual = nuevaPagina;
  mostrarHistorialTriajes();

  // Scroll al inicio de la tabla
  document
    .querySelector(".triaje-history-card")
    .scrollIntoView({ behavior: "smooth" });
}

// Funciones de utilidad para mostrar mensajes
function showSuccess(message) {
  mostrarModal(message, "success");
}

function showError(message) {
  mostrarModal(message, "error");
}

function mostrarModal(message, type) {
  // Crear el overlay del modal
  const modalOverlay = document.createElement("div");
  modalOverlay.className = "modal-overlay";
  modalOverlay.style.cssText = `
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 9999;
    animation: fadeIn 0.3s ease;
  `;

  // Crear el modal
  const modal = document.createElement("div");
  modal.className = "modal-message " + type;
  modal.style.cssText = `
    background: white;
    padding: 30px;
    border-radius: 12px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    max-width: 450px;
    width: 90%;
    text-align: center;
    animation: slideDown 0.3s ease;
  `;

  // Icono según el tipo
  const icon =
    type === "success"
      ? '<div style="font-size: 48px; margin-bottom: 15px;">✅</div>'
      : '<div style="font-size: 48px; margin-bottom: 15px;">⚠️</div>';

  // Color del botón según el tipo
  const buttonColor = type === "success" ? "#10b981" : "#ef4444";

  modal.innerHTML = `
    ${icon}
    <h3 style="margin: 0 0 15px 0; color: #1f2937; font-size: 20px; font-weight: 600;">
      ${type === "success" ? "Éxito" : "Error"}
    </h3>
    <p style="margin: 0 0 25px 0; color: #6b7280; font-size: 15px; line-height: 1.6;">
      ${message}
    </p>
    <button onclick="cerrarModal()" style="
      background: ${buttonColor};
      color: white;
      border: none;
      padding: 12px 30px;
      border-radius: 8px;
      font-size: 15px;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.2s;
    " onmouseover="this.style.opacity='0.9'" onmouseout="this.style.opacity='1'">
      Aceptar
    </button>
  `;

  // Agregar estilos de animación si no existen
  if (!document.querySelector("style[data-modal-animations]")) {
    const style = document.createElement("style");
    style.setAttribute("data-modal-animations", "true");
    style.textContent = `
      @keyframes fadeIn {
        from { opacity: 0; }
        to { opacity: 1; }
      }
      @keyframes slideDown {
        from {
          opacity: 0;
          transform: translateY(-20px);
        }
        to {
          opacity: 1;
          transform: translateY(0);
        }
      }
    `;
    document.head.appendChild(style);
  }

  modalOverlay.appendChild(modal);
  document.body.appendChild(modalOverlay);

  // Cerrar al hacer clic en el overlay
  modalOverlay.addEventListener("click", function (e) {
    if (e.target === modalOverlay) {
      cerrarModal();
    }
  });
}

function cerrarModal() {
  const modal = document.querySelector(".modal-overlay");
  if (modal) {
    modal.style.animation = "fadeIn 0.3s ease reverse";
    setTimeout(() => modal.remove(), 300);
  }
}
