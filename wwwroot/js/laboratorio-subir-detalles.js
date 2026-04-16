const API_BASE_URL = "http://localhost:5122";
let idOrdenActual = null;
let ordenData = null;

// Inyectar estilos del modal
const modalStyles = `
<style>
.lab-result-modal-overlay {
    display: none;
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background-color: rgba(0, 0, 0, 0.6);
    z-index: 99999;
    justify-content: center;
    align-items: center;
    animation: labResultModalFadeIn 0.3s ease;
}

.lab-result-modal-overlay.lab-result-modal-active {
    display: flex !important;
}

.lab-result-modal-container {
    background: white;
    border-radius: 16px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.3);
    max-width: 500px;
    width: 90%;
    overflow: hidden;
    animation: labResultModalSlideUp 0.3s ease;
}

.lab-result-modal-header {
    background: linear-gradient(135deg, #dc3545 0%, #c82333 100%);
    padding: 24px;
    display: flex;
    align-items: center;
    gap: 16px;
}

.lab-result-modal-header.success {
    background: linear-gradient(135deg, #28a745 0%, #218838 100%);
}

.lab-result-modal-icon {
    width: 48px;
    height: 48px;
    background: rgba(255, 255, 255, 0.2);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 24px;
}

.lab-result-modal-title {
    color: white;
    font-size: 22px;
    font-weight: 600;
    margin: 0;
}

.lab-result-modal-body {
    padding: 32px 24px;
}

.lab-result-modal-message {
    color: #495057;
    font-size: 16px;
    line-height: 1.6;
    margin: 0;
}

.lab-result-modal-footer {
    padding: 0 24px 24px 24px;
    display: flex;
    justify-content: flex-end;
    gap: 12px;
}

.lab-result-modal-btn {
    padding: 12px 24px;
    border: none;
    border-radius: 8px;
    font-size: 15px;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
}

.lab-result-modal-btn-primary {
    background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
    color: white;
}

.lab-result-modal-btn-primary:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 123, 255, 0.4);
}

.lab-result-modal-btn-success {
    background: linear-gradient(135deg, #28a745 0%, #218838 100%);
    color: white;
}

.lab-result-modal-btn-success:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(40, 167, 69, 0.4);
}

@keyframes labResultModalFadeIn {
    from { opacity: 0; }
    to { opacity: 1; }
}

@keyframes labResultModalSlideUp {
    from {
        transform: translateY(30px);
        opacity: 0;
    }
    to {
        transform: translateY(0);
        opacity: 1;
    }
}
</style>
`;

// HTML del modal de error
const modalErrorHTML = `
<div id="labResultModalError" class="lab-result-modal-overlay">
    <div class="lab-result-modal-container">
        <div class="lab-result-modal-header">
            <div class="lab-result-modal-icon">⚠️</div>
            <h2 class="lab-result-modal-title">Error al Guardar</h2>
        </div>
        <div class="lab-result-modal-body">
            <p class="lab-result-modal-message" id="labResultModalErrorMessage">
                No se pudo cargar la información de la orden de examen. Por favor, regrese e intente nuevamente.
            </p>
        </div>
        <div class="lab-result-modal-footer">
            <button type="button" class="lab-result-modal-btn lab-result-modal-btn-primary" onclick="cerrarModalError()">
                Entendido
            </button>
        </div>
    </div>
</div>
`;

// HTML del modal de éxito
const modalSuccessHTML = `
<div id="labResultModalSuccess" class="lab-result-modal-overlay">
    <div class="lab-result-modal-container">
        <div class="lab-result-modal-header success">
            <div class="lab-result-modal-icon">✓</div>
            <h2 class="lab-result-modal-title">Guardado Exitoso</h2>
        </div>
        <div class="lab-result-modal-body">
            <p class="lab-result-modal-message" id="labResultModalSuccessMessage">
                Resultados guardados exitosamente
            </p>
        </div>
        <div class="lab-result-modal-footer">
            <button type="button" class="lab-result-modal-btn lab-result-modal-btn-success" onclick="cerrarModalSuccess()">
                Aceptar
            </button>
        </div>
    </div>
</div>
`;

document.addEventListener("DOMContentLoaded", function () {
  // Inyectar estilos y modales en el documento
  document.head.insertAdjacentHTML("beforeend", modalStyles);
  document.body.insertAdjacentHTML("beforeend", modalErrorHTML);
  document.body.insertAdjacentHTML("beforeend", modalSuccessHTML);

  obtenerParametrosURL();
  cargarInformacionOrden();
});

function obtenerParametrosURL() {
  const params = new URLSearchParams(window.location.search);
  idOrdenActual = params.get("idOrden");

  if (!idOrdenActual) {
    showError("No se especificó el ID de la orden");
    setTimeout(() => window.history.back(), 2000);
  }
}

async function cargarInformacionOrden() {
  try {
    const res = await fetch(
      `${API_BASE_URL}/laboratorio/api/orden/${idOrdenActual}`
    );
    const result = await res.json();

    if (result.success && result.orden) {
      ordenData = result.orden;
      mostrarInformacionOrden(result.orden);
    } else {
      showError("No se pudo cargar la información de la orden");
      setTimeout(() => window.history.back(), 2000);
    }
  } catch (error) {
    console.error("Error:", error);
    showError("Error al cargar datos de la orden");
  }
}

function mostrarInformacionOrden(orden) {
  const fecha = new Date(orden.fechaSolicitud).toLocaleDateString("es-PE");

  document.getElementById("ordenInfo").innerHTML = `
        <div class="info-item">
            <span class="info-label"># DE ORDEN</span>
            <span class="info-value">${orden.numeroOrden}</span>
        </div>

        <div class="info-item">
            <span class="info-label">TIPO DE EXAMEN</span>
            <span class="info-value">${orden.tipoExamen}</span>
        </div>

        <div class="info-item">
            <span class="info-label">PACIENTE</span>
            <span class="info-value">${orden.nombreCompletoPaciente}</span>
        </div>

        <div class="info-item">
            <span class="info-label">DNI PACIENTE</span>
            <span class="info-value">${orden.dniPaciente || "N/A"}</span>
        </div>

        <div class="info-item">
            <span class="info-label">FECHA DE ORDEN</span>
            <span class="info-value">${fecha}</span>
        </div>

        <div class="info-item">
            <span class="info-label">ESTADO ACTUAL</span>
            <span class="info-badge">${orden.estado}</span>
        </div>

        <div class="info-item">
            <span class="info-label">OBSERVACIONES DE LA ORDEN</span>
            <span class="info-value">${
              orden.observacionesAdicionales || "Sin observaciones"
            }</span>
        </div>
    `;
}

async function guardarDetalles(event) {
  event.preventDefault();

  const resultados = document.getElementById("resultados").value.trim();
  const observacionesFinales = document
    .getElementById("observacionesFinales")
    .value.trim();

  if (!resultados) {
    showWarning("Por favor ingrese los resultados del examen");
    return;
  }

  const detallesDTO = {
    idOrden: parseInt(idOrdenActual),
    resultados: resultados,
    fechaResultado: new Date().toISOString(),
    observacionesFinales: observacionesFinales,
  };

  try {
    const res = await fetch(
      `${API_BASE_URL}/laboratorio/api/actualizar-resultados`,
      {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify(detallesDTO),
      }
    );

    const result = await res.json();

    if (result.success) {
      // Limpiar los campos del formulario
      document.getElementById("resultados").value = "";
      document.getElementById("observacionesFinales").value = "";

      // Mostrar modal de éxito
      mostrarModalSuccess(
        result.message || "Resultados guardados exitosamente"
      );
    } else {
      // Mostrar modal de error
      mostrarModalError(
        result.message ||
          "No se pudo guardar la información de la orden de examen. Por favor, regrese e intente nuevamente."
      );
    }
  } catch (error) {
    console.error("Error:", error);
    mostrarModalError(
      "No se pudo cargar la información de la orden de examen. Por favor, regrese e intente nuevamente."
    );
  }
}

function mostrarModalError(mensaje) {
  const modal = document.getElementById("labResultModalError");
  const modalMessage = document.getElementById("labResultModalErrorMessage");

  if (modalMessage) {
    modalMessage.textContent = mensaje;
  }

  if (modal) {
    modal.classList.add("lab-result-modal-active");
  }
}

function cerrarModalError() {
  const modal = document.getElementById("labResultModalError");
  if (modal) {
    modal.classList.remove("lab-result-modal-active");
  }
}

function mostrarModalSuccess(mensaje) {
  const modal = document.getElementById("labResultModalSuccess");
  const modalMessage = document.getElementById("labResultModalSuccessMessage");

  if (modalMessage) {
    modalMessage.textContent = mensaje;
  }

  if (modal) {
    modal.classList.add("lab-result-modal-active");
  }
}

function cerrarModalSuccess() {
  const modal = document.getElementById("labResultModalSuccess");
  if (modal) {
    modal.classList.remove("lab-result-modal-active");
  }
}

function volver() {
  if (
    confirm(
      "¿Está seguro de que desea salir? Los datos no guardados se perderán."
    )
  ) {
    window.location.href = `/laboratorio?idPaciente=${
      ordenData?.idPaciente || ""
    }`;
  }
}
