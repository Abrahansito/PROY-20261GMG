const API_BASE_URL = "http://localhost:5122";
let idPacienteActual = null;

document.addEventListener("DOMContentLoaded", function () {
  obtenerParametrosURL();
  cargarInformacionPaciente();
});

function obtenerParametrosURL() {
  const params = new URLSearchParams(window.location.search);
  idPacienteActual = params.get("idPaciente");

  if (!idPacienteActual) {
    showError("No se especificó el ID del paciente");
    setTimeout(() => window.history.back(), 2000);
  }
}

async function cargarInformacionPaciente() {
  try {
    const res = await fetch(
      `${API_BASE_URL}/laboratorio/api/historial/${idPacienteActual}`
    );

    // Verificar si la respuesta es OK (status 200-299)
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`);
    }

    const result = await res.json();

    if (result.success && result.paciente) {
      mostrarInformacionPaciente(result.paciente);
    } else {
      showError(
        result.message || "No se pudo cargar la información del paciente"
      );
    }
  } catch (error) {
    console.error("Error:", error);
    showError(
      "Error al cargar las órdenes de laboratorio. Por favor, intente de nuevo más tarde."
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

async function guardarOrden(event) {
  event.preventDefault();

  const tipoExamen = document.getElementById("tipoExamen").value;
  const observaciones = document.getElementById("observaciones").value.trim();

  if (!tipoExamen) {
    showWarning("Por favor seleccione un tipo de examen");
    return;
  }

  const ordenDTO = {
    idPaciente: parseInt(idPacienteActual),
    idMedico: 1, // TODO: Obtener del contexto del usuario logueado
    tipoExamen: tipoExamen,
    observacionesAdicionales: observaciones,
    estado: "Pendiente", // Estado por defecto
  };

  try {
    const res = await fetch(`${API_BASE_URL}/laboratorio/api/crear`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(ordenDTO),
    });

    // Verificar si la respuesta es OK (status 200-299)
    if (!res.ok) {
      throw new Error(`HTTP error! status: ${res.status}`);
    }

    const result = await res.json();

    if (result.success) {
      showSuccess("Orden de laboratorio creada exitosamente");
      setTimeout(() => {
        window.location.href = `/laboratorio?idPaciente=${idPacienteActual}`;
      }, 1500);
    } else {
      showError(
        result.message ||
          "Ocurrió un error al guardar la orden. Por favor, intente nuevamente."
      );
    }
  } catch (error) {
    console.error("Error:", error);
    showError(
      "Ocurrió un error al guardar la orden. Por favor, intente nuevamente."
    );
  }
}

function volver() {
  if (
    confirm(
      "¿Está seguro de que desea salir? Los datos no guardados se perderán."
    )
  ) {
    window.location.href = `/laboratorio?idPaciente=${idPacienteActual}`;
  }
}

// Funciones de utilidad para mostrar mensajes
function showSuccess(message) {
  mostrarModal(message, "success");
}

function showError(message) {
  mostrarModal(message, "error");
}

function showWarning(message) {
  mostrarModal(message, "warning");
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

  // Icono y color según el tipo
  let icon, buttonColor, titulo;

  if (type === "success") {
    icon = '<div style="font-size: 48px; margin-bottom: 15px;">✅</div>';
    buttonColor = "#10b981";
    titulo = "Éxito";
  } else if (type === "warning") {
    icon = '<div style="font-size: 48px; margin-bottom: 15px;">⚠️</div>';
    buttonColor = "#f59e0b";
    titulo = "Advertencia";
  } else {
    icon = '<div style="font-size: 48px; margin-bottom: 15px;">⚠️</div>';
    buttonColor = "#ef4444";
    titulo = "Error";
  }

  modal.innerHTML = `
        ${icon}
        <h3 style="margin: 0 0 15px 0; color: #1f2937; font-size: 20px; font-weight: 600;">
            ${titulo}
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
