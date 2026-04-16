// ===================================
// HISTORIAL DE RECETAS MÉDICAS - JS
// ===================================

(function () {
  ("use strict");

  // Variables globales
  let idPaciente = null;
  let recetaSeleccionada = null;

  // Elementos del DOM
  const btnNuevaReceta = document.getElementById("btnNuevaReceta");
  const btnCloseDetails = document.getElementById("btnCloseDetails");
  const recetasTableBody = document.getElementById("recetasTableBody");
  const detailsContent = document.getElementById("detailsContent");

  // ===== INICIALIZACIÓN =====
  document.addEventListener("DOMContentLoaded", function () {
    initializeApp();
  });

  function initializeApp() {
    // Obtener ID del paciente
    const idPacienteInput = document.getElementById("idPaciente");
    if (idPacienteInput) {
      idPaciente = parseInt(idPacienteInput.value);
    }

    // Validar que exista el ID del paciente
    if (!idPaciente || isNaN(idPaciente)) {
      mostrarError("No se encontró el ID del paciente");
      return;
    }

    // Cargar recetas
    cargarRecetas();

    // Event Listeners
    setupEventListeners();
  }

  function setupEventListeners() {
    // Botón nueva receta
    if (btnNuevaReceta) {
      btnNuevaReceta.addEventListener("click", function () {
        window.location.href = "/Home/Receta";
      });
    }

    // Botón cerrar detalles
    if (btnCloseDetails) {
      btnCloseDetails.addEventListener("click", function () {
        cerrarDetalles();
      });
    }
  }

  // ===== CARGAR RECETAS =====
  async function cargarRecetas() {
    try {
      mostrarCargando();

      const response = await fetch(`/RecetaMedica/paciente/${idPaciente}`);
      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al cargar las recetas");
      }

      if (data.success) {
        const recetas = data.data || [];
        renderizarRecetas(recetas);
      } else {
        throw new Error(data.message || "Error al obtener las recetas");
      }
    } catch (error) {
      console.error("Error al cargar recetas:", error);
      mostrarErrorEnTabla(error.message);
    }
  }

  // ===== RENDERIZAR RECETAS =====
  function renderizarRecetas(recetas) {
    if (!recetas || recetas.length === 0) {
      mostrarSinDatos();
      return;
    }

    let html = "";
    recetas.forEach((receta) => {
      const fecha = formatearFecha(receta.fechaEmision);
      const cantidadMedicamentos = receta.detalles ? receta.detalles.length : 0;
      const codigoReceta = receta.idReceta
        ? `RX-${new Date(receta.fechaEmision).getFullYear()}-${String(
            receta.idReceta
          ).padStart(4, "0")}`
        : "N/A";

      html += `
                <tr data-id="${receta.idReceta}">
                    <td>${fecha}</td>
                    <td>${codigoReceta}</td>
                    <td>${receta.nombreCompletoMedico || "N/A"}</td>
                    <td class="text-center-recetas">${cantidadMedicamentos}</td>
                    <td>
                        <div class="action-buttons">
                            <button type="button" class="btn-action btn-imprimir" 
                                    onclick="imprimirReceta(${
                                      receta.idReceta
                                    })">
                                <i class="fas fa-print"></i> Imprimir
                            </button>
                            <button type="button" class="btn-action btn-ver-detalles" 
                                    onclick="verDetalles(${receta.idReceta})">
                                <i class="fas fa-eye"></i> Ver detalles
                            </button>
                        </div>
                    </td>
                </tr>
            `;
    });

    recetasTableBody.innerHTML = html;
  }

  // ===== VER DETALLES DE RECETA =====
  window.verDetalles = async function (idReceta) {
    try {
      // Mostrar loading en el panel de detalles
      detailsContent.innerHTML =
        '<div class="text-center-recetas"><div class="loading-spinner-recetas"></div><p>Cargando detalles...</p></div>';

      const response = await fetch(`/RecetaMedica/${idReceta}`);
      const data = await response.json();

      if (!response.ok) {
        throw new Error(data.message || "Error al cargar los detalles");
      }

      if (data.success) {
        recetaSeleccionada = data.data;
        renderizarDetalles(recetaSeleccionada);
        marcarFilaSeleccionada(idReceta);
      } else {
        throw new Error(data.message || "Error al obtener los detalles");
      }
    } catch (error) {
      console.error("Error al cargar detalles:", error);
      detailsContent.innerHTML = `
                <div class="empty-state-recetas">
                    <i class="fas fa-exclamation-triangle"></i>
                    <p>Error al cargar los detalles: ${error.message}</p>
                </div>
            `;
    }
  };

  // ===== RENDERIZAR DETALLES =====
  function renderizarDetalles(receta) {
    const fecha = formatearFecha(receta.fechaEmision);
    const codigoReceta = `RX-${new Date(
      receta.fechaEmision
    ).getFullYear()}-${String(receta.idReceta).padStart(4, "0")}`;

    let html = `
            <div class="receta-info-grid">
                <div class="info-item">
                    <span class="info-label">Código:</span>
                    <span class="info-value">${codigoReceta}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Fecha de emisión:</span>
                    <span class="info-value">${fecha}</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Médico tratante:</span>
                    <span class="info-value">${
                      receta.nombreCompletoMedico || "N/A"
                    }</span>
                </div>
                <div class="info-item">
                    <span class="info-label">Diagnóstico:</span>
                    <span class="info-value">${
                      receta.especialidadMedico || "N/A"
                    }</span>
                </div>
            </div>

            <div class="medicamentos-section">
                <h3 class="section-subtitle">Lista de medicamentos recetados:</h3>
        `;

    if (receta.detalles && receta.detalles.length > 0) {
      receta.detalles.forEach((detalle) => {
        html += `
                    <div class="medicamento-card">
                        <div class="medicamento-header">
                            <div>
                                <h4 class="medicamento-nombre">${
                                  detalle.productoFarmaceutico
                                }</h4>
                                <span class="medicamento-concentracion">${
                                  detalle.concentracion
                                }</span>
                            </div>
                        </div>
                        <div class="medicamento-details">
                            <div class="detail-row">
                                <span class="detail-label">Frecuencia:</span>
                                <span class="detail-value">${
                                  detalle.frecuencia
                                }</span>
                            </div>
                            <div class="detail-row">
                                <span class="detail-label">Duración:</span>
                                <span class="detail-value">${
                                  detalle.duracion
                                }</span>
                            </div>
                            <div class="detail-row">
                                <span class="detail-label">Vía de administración:</span>
                                <span class="detail-value">${
                                  detalle.viaAdministracion
                                }</span>
                            </div>
                            ${
                              detalle.observaciones
                                ? `
                            <div class="detail-row">
                                <span class="detail-label">Observaciones:</span>
                                <span class="detail-value">${detalle.observaciones}</span>
                            </div>
                            `
                                : ""
                            }
                        </div>
                    </div>
                `;
      });
    } else {
      html +=
        '<p class="text-center-recetas">No hay medicamentos en esta receta</p>';
    }

    html += "</div>";

    if (
      receta.observacionesGenerales &&
      receta.observacionesGenerales.trim() !== ""
    ) {
      html += `
                <div class="observaciones-box">
                    <div class="observaciones-title">Observaciones médicas:</div>
                    <p class="observaciones-text">${receta.observacionesGenerales}</p>
                </div>
            `;
    }

    detailsContent.innerHTML = html;
  }

  // ===== IMPRIMIR RECETA =====
  // En historial-recetas.js, reemplaza la función imprimirReceta:
  window.imprimirReceta = function (idReceta) {
    // Descargar directamente el PDF
    window.location.href = `/RecetaMedica/imprimir/${idReceta}`;
  };

  // ===== MARCAR FILA SELECCIONADA =====
  function marcarFilaSeleccionada(idReceta) {
    // Remover selección previa
    const filasAnteriores = document.querySelectorAll(
      ".tabla-recetas tbody tr.selected"
    );
    filasAnteriores.forEach((fila) => fila.classList.remove("selected"));

    // Marcar fila actual
    const filaActual = document.querySelector(
      `.tabla-recetas tbody tr[data-id="${idReceta}"]`
    );
    if (filaActual) {
      filaActual.classList.add("selected");
    }
  }

  // ===== CERRAR DETALLES =====
  function cerrarDetalles() {
    detailsContent.innerHTML = `
            <div class="empty-state-recetas">
                <i class="fas fa-file-prescription"></i>
                <p>Seleccione una receta para ver sus detalles</p>
            </div>
        `;

    // Remover selección
    const filasSeleccionadas = document.querySelectorAll(
      ".tabla-recetas tbody tr.selected"
    );
    filasSeleccionadas.forEach((fila) => fila.classList.remove("selected"));

    recetaSeleccionada = null;
  }

  // ===== UTILIDADES =====
  function formatearFecha(fecha) {
    if (!fecha) return "N/A";
    const date = new Date(fecha);
    const dia = String(date.getDate()).padStart(2, "0");
    const mes = String(date.getMonth() + 1).padStart(2, "0");
    const anio = date.getFullYear();
    return `${dia}/${mes}/${anio}`;
  }

  function mostrarCargando() {
    recetasTableBody.innerHTML = `
            <tr class="loading-row">
                <td colspan="5" class="text-center-recetas">
                    <div class="loading-spinner-recetas"></div>
                    <p>Cargando recetas...</p>
                </td>
            </tr>
        `;
  }

  function mostrarSinDatos() {
    recetasTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center-recetas">
                    No se encontraron recetas médicas para este paciente
                </td>
            </tr>
        `;
  }

  function mostrarErrorEnTabla(mensaje) {
    recetasTableBody.innerHTML = `
            <tr>
                <td colspan="5" class="text-center-recetas" style="color: #ef4444;">
                    <i class="fas fa-exclamation-triangle"></i> ${mensaje}
                </td>
            </tr>
        `;
  }

  // ===== ALERTAS =====
  function mostrarExito(mensaje) {
    mostrarAlerta(mensaje, "success");
  }

  function mostrarError(mensaje) {
    mostrarAlerta(mensaje, "error");
  }

  function mostrarAlerta(mensaje, tipo) {
    // Crear elemento de alerta
    const alerta = document.createElement("div");
    alerta.className = `alert alert-${tipo}`;
    alerta.style.cssText = `
            position: fixed;
            top: 20px;
            right: 20px;
            padding: 1rem 1.5rem;
            background-color: ${tipo === "success" ? "#10b981" : "#ef4444"};
            color: white;
            border-radius: 8px;
            box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
            z-index: 9999;
            animation: slideIn 0.3s ease;
        `;
    alerta.innerHTML = `<i class="fas fa-${
      tipo === "success" ? "check-circle" : "exclamation-circle"
    }"></i> ${mensaje}`;

    document.body.appendChild(alerta);

    // Remover después de 3 segundos
    setTimeout(() => {
      alerta.style.animation = "slideOut 0.3s ease";
      setTimeout(() => alerta.remove(), 300);
    }, 3000);
  }

  // Añadir estilos de animación al documento
  if (!document.getElementById("alert-animations")) {
    const style = document.createElement("style");
    style.id = "alert-animations";
    style.textContent = `
            @keyframes slideIn {
                from {
                    transform: translateX(100%);
                    opacity: 0;
                }
                to {
                    transform: translateX(0);
                    opacity: 1;
                }
            }
            @keyframes slideOut {
                from {
                    transform: translateX(0);
                    opacity: 1;
                }
                to {
                    transform: translateX(100%);
                    opacity: 0;
                }
            }
        `;
    document.head.appendChild(style);
  }
})();
