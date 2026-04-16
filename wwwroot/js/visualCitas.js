const API_BASE_URL = "http://localhost:5122";

document.addEventListener("DOMContentLoaded", () => {
  const buscarBtn = document.getElementById("buscarMedicosBtn");
  const limpiarBtn = document.getElementById("limpiarFiltrosBtn");
  if (buscarBtn) buscarBtn.addEventListener("click", onBuscarMedicos);
  if (limpiarBtn) limpiarBtn.addEventListener("click", onLimpiarFiltros);
  loadConsultorios();

  // Mostrar información si hay idPaciente o idCita
  mostrarInformacionInicial();
});

// Mostrar información si se recibieron parámetros
function mostrarInformacionInicial() {
  if (window.APP_DATA && window.APP_DATA.idCita) {
    showNotificationModal(
      `Reprogramando cita #${window.APP_DATA.idCita} para el paciente #${
        window.APP_DATA.idPaciente || "N/A"
      }`
    );
  }
}

async function loadConsultorios() {
  try {
    const res = await fetch(`${API_BASE_URL}/personal/resumen`);
    const result = await res.json();
    if (!result.success || !result.data) return;
    const consultorios = result.data.consultoriosList || [];
    const select = document.getElementById("fConsultorio");
    if (!select) return;
    select.innerHTML = '<option value="">Todos los consultorios</option>';
    consultorios.forEach((c) => {
      const opt = document.createElement("option");
      opt.value = c.idConsultorio || c.IdConsultorio || c.Id || "";
      opt.textContent = c.nombre || c.Nombre || "";
      select.appendChild(opt);
    });
  } catch (err) {
    console.error("Error cargando consultorios", err);
    showNotificationModal("Error al cargar los consultorios");
  }
}

function limpiarErrores() {
  document.querySelectorAll(".field-error").forEach((el) => el.remove());
  document.querySelectorAll(".form-control.is-invalid").forEach((el) => {
    el.classList.remove("is-invalid");
  });
}

function mostrarErroresCampo(errores) {
  limpiarErrores();

  if (!errores || !Array.isArray(errores)) return;

  errores.forEach((error) => {
    const fieldId =
      "f" + error.field.charAt(0).toUpperCase() + error.field.slice(1);
    const fieldElement = document.getElementById(fieldId);

    if (fieldElement && error.errors && error.errors.length > 0) {
      fieldElement.classList.add("is-invalid");

      const errorDiv = document.createElement("div");
      errorDiv.className = "field-error text-danger small mt-1";
      errorDiv.textContent = error.errors[0];

      fieldElement.parentElement.appendChild(errorDiv);
    }
  });
}

function mostrarMensajeSuperior(periodoSemana, totalMedicos) {
  const mensajeAnterior = document.querySelector(".mensaje-disponibilidad");
  if (mensajeAnterior) {
    mensajeAnterior.remove();
  }

  const mensajeDiv = document.createElement("div");
  mensajeDiv.className = "mensaje-disponibilidad alert alert-success";
  mensajeDiv.style.cssText =
    "margin: 15px 0; padding: 12px 20px; border-radius: 8px; " +
    "border-left: 4px solid #28a745; background: #d4edda; " +
    "color: #155724; font-size: 0.9rem; line-height: 1.4; " +
    "animation: fadeInUp 0.5s ease-out; max-width: 100%;";

  mensajeDiv.innerHTML = `
    <div style="display: flex; justify-content: space-between; align-items: flex-start; flex-wrap: wrap; gap: 10px;">
      <div style="flex: 1; min-width: 250px;">
        <strong style="display: block; margin-bottom: 5px;">Período seleccionado:</strong>
        <span style="font-size: 0.85rem;">${periodoSemana}</span>
      </div>
      <div style="flex: 1; min-width: 200px;">
        <strong style="display: block; margin-bottom: 5px;">Total de médicos disponibles:</strong>
        <span style="font-size: 1.1rem; font-weight: bold; color: #155724;">${totalMedicos}</span>
      </div>
    </div>
  `;

  const tableSection = document.querySelector(".table-section");
  if (tableSection) {
    tableSection.parentElement.insertBefore(mensajeDiv, tableSection);
  }

  setTimeout(() => {
    mensajeDiv.style.transition = "opacity 0.5s, transform 0.5s";
    mensajeDiv.style.opacity = "0";
    mensajeDiv.style.transform = "translateY(-10px)";
    setTimeout(() => {
      if (mensajeDiv.parentElement) {
        mensajeDiv.parentElement.removeChild(mensajeDiv);
      }
    }, 500);
  }, 6000);
}

function showNotificationModal(message) {
  const modal = document.getElementById("notificationModal");
  const notificationMessage = document.getElementById("notificationMessage");

  if (modal && notificationMessage) {
    notificationMessage.textContent = message;
    modal.style.display = "flex";
    modal.style.animation = "fadeIn 0.3s ease-out";
  }
}

function closeNotificationModal() {
  const modal = document.getElementById("notificationModal");
  if (modal) {
    modal.style.animation = "fadeOut 0.3s ease-in forwards";
    setTimeout(() => {
      modal.style.display = "none";
      modal.style.animation = "";
    }, 300);
  }
}

// Función actualizada para ver horario - Incluye idCita si está disponible
function verHorario(medico) {
  console.log("Datos del médico:", medico);

  const idMedico =
    medico.idMedico || medico.id || medico.Id || medico.IdMedico || medico.ID;

  // Obtener el ID del paciente desde APP_DATA o desde la URL
  let idPaciente = null;
  if (window.APP_DATA && window.APP_DATA.idPaciente) {
    idPaciente = window.APP_DATA.idPaciente;
  } else {
    const urlParams = new URLSearchParams(window.location.search);
    idPaciente = urlParams.get("idPaciente");
  }

  // Obtener el ID de la cita desde APP_DATA
  let idCita = null;
  if (window.APP_DATA && window.APP_DATA.idCita) {
    idCita = window.APP_DATA.idCita;
  }

  const semanaSelect = document.getElementById("fSemana");
  const semana = semanaSelect ? semanaSelect.value : "0";

  console.log(
    "ID Médico:",
    idMedico,
    "ID Paciente:",
    idPaciente,
    "ID Cita:",
    idCita,
    "Semana:",
    semana
  );

  if (idMedico) {
    let url = `/Home/HorarioMedico?idMedico=${encodeURIComponent(idMedico)}`;

    if (idPaciente) {
      url += `&idPaciente=${encodeURIComponent(idPaciente)}`;
    }

    // Agregar idCita si está disponible
    if (idCita) {
      url += `&idCita=${encodeURIComponent(idCita)}`;
    }

    url += `&semana=${encodeURIComponent(semana)}`;

    console.log("Redirigiendo a:", url);
    window.location.href = url;
  } else {
    showNotificationModal(
      "No se pudo obtener la información del médico. Intente nuevamente."
    );
    console.error("No se encontró el ID del médico:", medico);
  }
}

async function onBuscarMedicos(e) {
  e && e.preventDefault && e.preventDefault();

  limpiarErrores();

  const numeroDni = document.getElementById("fNumeroDni")?.value.trim() || null;
  const consultorio = document.getElementById("fConsultorio")?.value || null;
  const estado = document.getElementById("fEstado")?.value || null;
  const turno = document.getElementById("fTurno")?.value || null;
  const semana = document.getElementById("fSemana")?.value || 0;

  const body = {
    semana: parseInt(semana) || 0,
    numeroDni: numeroDni,
    idConsultorio: consultorio ? parseInt(consultorio) : null,
    estado: estado,
    turno: turno,
  };

  try {
    const res = await fetch(`${API_BASE_URL}/medicos/disponibilidad`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(body),
    });

    const result = await res.json();

    if (!result.success) {
      if (result.data && Array.isArray(result.data)) {
        mostrarErroresCampo(result.data);
      } else {
        showNotificationModal(result.message || "Error en la búsqueda");
      }
      renderMedicosTable([]);
      return;
    }

    if (result.data) {
      mostrarMensajeSuperior(
        result.data.periodoSemana || "N/A",
        result.data.totalMedicosDisponibles || 0
      );
      renderMedicosTable(result.data.medicosDisponibles || []);
    }
  } catch (err) {
    console.error("Error en búsqueda:", err);
    showNotificationModal("Error al conectar con el servidor");
    renderMedicosTable([]);
  }
}

function onLimpiarFiltros() {
  ["fNumeroDni", "fConsultorio", "fEstado", "fTurno", "fSemana"].forEach(
    (id) => {
      const el = document.getElementById(id);
      if (el) {
        if (id === "fSemana") {
          el.value = "0";
        } else {
          el.value = "";
        }
      }
    }
  );
  limpiarErrores();

  const mensaje = document.querySelector(".mensaje-disponibilidad");
  if (mensaje) mensaje.remove();

  renderMedicosTable([]);
}

// Ejecutar búsqueda automáticamente al cargar la página
document.addEventListener('DOMContentLoaded', function() {
    // Cargar consultorios si tienes esa función
    if (typeof cargarConsultorios === 'function') {
        loadConsultorios();
    }
    
    // Ejecutar búsqueda automática
    onBuscarMedicos();
});

function renderMedicosTable(medicos) {
  const tbody = document.getElementById("medicosTableBody");
  if (!tbody) return;

  if (!medicos || medicos.length === 0) {
    tbody.innerHTML = `<tr><td colspan="7" class="no-data">No hay médicos disponibles con los filtros aplicados.</td></tr>`;
    return;
  }

  tbody.innerHTML = medicos
    .map(
      (m) => `
    <tr>
      <td>${m.numeroDni || ""}</td>
      <td>${m.nombreCompleto || ""}</td>
      <td>${m.turno || ""}</td>
      <td>${m.consultorio || ""}</td>
      <td>${m.citasActuales || 0}</td>
      <td>${m.citasRestantes || 0}</td>
      <td>
        <button class="btn btn-primary btn-sm" onclick="verHorario(${JSON.stringify(
          m
        ).replace(/"/g, "&quot;")})">
          Ver Horario
        </button>
      </td>
    </tr>
  `
    )
    .join("");
}

// Agregar estilos para animación
if (!document.getElementById("customStyles")) {
  const style = document.createElement("style");
  style.id = "customStyles";
  style.textContent = `
    @keyframes fadeIn {
      from { opacity: 0; }
      to { opacity: 1; }
    }
    
    @keyframes fadeOut {
      from { opacity: 1; }
      to { opacity: 0; }
    }
    
    @keyframes fadeInUp {
      from { 
        opacity: 0; 
        transform: translateY(-20px); 
      }
      to { 
        opacity: 1; 
        transform: translateY(0); 
      }
    }
    
    .is-invalid {
      border-color: #dc3545 !important;
    }
    
    .field-error {
      color: #dc3545;
      font-size: 0.875rem;
      margin-top: 0.25rem;
    }
    
    .custom-modal {
      display: none;
      position: fixed;
      top: 0;
      left: 0;
      width: 100%;
      height: 100%;
      background-color: rgba(0, 0, 0, 0.5);
      z-index: 1050;
      justify-content: center;
      align-items: center;
    }
    
    .modal-content {
      background: white;
      border-radius: 10px;
      box-shadow: 0 10px 30px rgba(0,0,0,0.3);
      width: 90%;
      max-width: 450px;
      transform: scale(0.9);
      animation: modalScaleIn 0.3s ease-out forwards;
    }
    
    @keyframes modalScaleIn {
      to {
        transform: scale(1);
      }
    }
    
    .modal-header {
      padding: 20px 25px 15px 25px;
      border-bottom: 1px solid #e9ecef;
      display: flex;
      justify-content: space-between;
      align-items: center;
      background: #f8f9fa;
      border-radius: 10px 10px 0 0;
    }
    
    .modal-header .modal-title {
      margin: 0;
      font-size: 1.2rem;
      color: #495057;
      display: flex;
      align-items: center;
      gap: 10px;
    }
    
    .close-btn {
      background: none;
      border: none;
      font-size: 1.3rem;
      cursor: pointer;
      color: #6c757d;
      padding: 5px;
      border-radius: 50%;
      width: 30px;
      height: 30px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    
    .close-btn:hover {
      background-color: #e9ecef;
      color: #495057;
    }
    
    .modal-body {
      padding: 25px;
      font-size: 1rem;
      line-height: 1.5;
      color: #495057;
    }
    
    .modal-footer {
      padding: 15px 25px 20px 25px;
      border-top: 1px solid #e9ecef;
      display: flex;
      justify-content: flex-end;
      background: #f8f9fa;
      border-radius: 0 0 10px 10px;
    }
    
    .btn-sm {
      padding: 0.4rem 0.8rem;
      font-size: 0.875rem;
      border-radius: 4px;
    }
    
    @media (max-width: 768px) {
      .mensaje-disponibilidad {
        font-size: 0.85rem !important;
        padding: 10px 15px !important;
      }
      
      .mensaje-disponibilidad div {
        flex-direction: column !important;
        gap: 8px !important;
      }
      
      .modal-content {
        width: 95%;
        margin: 20px;
      }
    }
    
    @media (max-width: 480px) {
      .mensaje-disponibilidad {
        font-size: 0.8rem !important;
      }
      
      .modal-content {
        width: 100%;
        margin: 10px;
      }
      
      .modal-header,
      .modal-body,
      .modal-footer {
        padding: 15px 20px;
      }
    }
  `;
  document.head.appendChild(style);
}
