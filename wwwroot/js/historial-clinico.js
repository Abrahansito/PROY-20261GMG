const API_BASE_URL = "http://localhost:5122";
let idPacienteActual = null;
let idMedicoActual = null;
console.log("idPacienteActual inicial:", idPacienteActual);

// Inicializar cuando cargue la página
document.addEventListener("DOMContentLoaded", function () {
  obtenerParametrosURL();
  cargarDatosPaciente();
  idPacienteActual = idPacienteActual; // Asegurar que la variable global esté actualizada
  console.log("idPacienteActual después de cargar:", idPacienteActual);
});

// Obtener parámetros de la URL
function obtenerParametrosURL() {
  const params = new URLSearchParams(window.location.search);
  idPacienteActual = params.get("idPaciente");
  idMedicoActual = params.get("idMedico");

  if (!idPacienteActual) {
    alert("No se especificó el ID del paciente");
    window.history.back();
  }
}

// Cargar datos del paciente y su historial
async function cargarDatosPaciente() {
  try {
    const res = await fetch(
      `${API_BASE_URL}/api/historia-clinica/paciente/${idPacienteActual}`
    );
    const result = await res.json();

    if (result.success && result.data) {
      mostrarInformacionPaciente(result.data);
      mostrarHistorialDiagnosticos(result.data.diagnosticos);
    } else {
      document.getElementById("patientInfo").innerHTML =
        '<div class="no-data">No se pudo cargar la información del paciente</div>';
    }
  } catch (error) {
    console.error("Error:", error);
    document.getElementById("patientInfo").innerHTML =
      '<div class="no-data">Error al cargar los datos</div>';
  }
}

// Mostrar información del paciente
function mostrarInformacionPaciente(data) {
  document.getElementById("patientInfo").innerHTML = `
        <div class="info-item">
            <span class="info-label">DNI</span>
            <span class="info-value">${data.numeroDocumento}</span>
        </div>
        <div class="info-item">
            <span class="info-label">Nombre Completo</span>
            <span class="info-value">${data.nombreCompleto}</span>
        </div>
        <div class="info-item">
            <span class="info-label">Sexo</span>
            <span class="info-value">${data.sexo}</span>
        </div>
        <div class="info-item">
            <span class="info-label">Edad</span>
            <span class="info-value">${data.edad} años</span>
        </div>
        <div class="info-item">
            <span class="info-label">Seguro</span>
            <span class="info-value">${data.seguro}</span>
        </div>
    `;
}

// Mostrar historial de diagnósticos
function mostrarHistorialDiagnosticos(diagnosticos) {
  const container = document.getElementById("diagnosticsContainer");

  if (!diagnosticos || diagnosticos.length === 0) {
    container.innerHTML =
      '<div class="no-data">No hay diagnósticos registrados</div>';
    return;
  }

  const tabla = `
        <table class="diagnostics-table">
            <thead>
                <tr>
                    <th>Fecha</th>
                    <th>Diagnóstico</th>
                    <th>Médico</th>
                    <th>Consultorio</th>
                    <th>Observaciones</th>
                    <th>Acción</th>
                </tr>
            </thead>
            <tbody>
                ${diagnosticos
                  .map((d) => {
                    const fecha = new Date(
                      d.fechaDiagnostico
                    ).toLocaleDateString("es-PE");
                    const observaciones =
                      d.observacionesMedicas.length > 50
                        ? d.observacionesMedicas.substring(0, 50) + "..."
                        : d.observacionesMedicas || "Sin observaciones";

                    return `
                        <tr>
                            <td>${fecha}</td>
                            <td>${d.diagnosticoPrincipal}</td>
                            <td>${d.nombreCompletoMedico}</td>
                            <td>${d.consultorio}</td>
                            <td>${observaciones}</td>
                            <td>
                                <button class="btn-details" onclick="verDetalle(${d.idDiagnostico})">
                                    Ver detalles
                                </button>
                            </td>
                        </tr>
                    `;
                  })
                  .join("")}
            </tbody>
        </table>
    `;

  container.innerHTML = tabla;
}

// Ver detalle de un diagnóstico
async function verDetalle(idDiagnostico) {
  try {
    const res = await fetch(
      `${API_BASE_URL}/api/historia-clinica/diagnostico/${idDiagnostico}`
    );
    const result = await res.json();

    if (result.success && result.data) {
      mostrarModalDetalle(result.data);
    } else {
      alert("No se pudo cargar el detalle del diagnóstico");
    }
  } catch (error) {
    console.error("Error:", error);
    alert("Error al cargar el detalle");
  }
}

// Mostrar modal con detalle
function mostrarModalDetalle(diagnostico) {
  const fecha = new Date(diagnostico.fechaDiagnostico).toLocaleDateString(
    "es-PE",
    {
      year: "numeric",
      month: "long",
      day: "numeric",
    }
  );

  document.getElementById("modalBody").innerHTML = `
        <div class="detail-section">
            <h3>Fecha del diagnóstico</h3>
            <p>${fecha}</p>
        </div>
        <div class="detail-section">
            <h3>Diagnóstico principal:</h3>
            <p>${diagnostico.diagnosticoPrincipal}</p>
        </div>
        <div class="detail-section">
            <h3>Tratamiento específico:</h3>
            <p>${diagnostico.tratamientoEspecifico || "No especificado"}</p>
        </div>
        <div class="detail-section">
            <h3>Observaciones médicas:</h3>
            <p>${diagnostico.observacionesMedicas || "Sin observaciones"}</p>
        </div>
    `;

  document.getElementById("detailModal").classList.add("active");
}

// Cerrar modal
function cerrarModal() {
  document.getElementById("detailModal").classList.remove("active");
}

// Cerrar modal al hacer clic fuera
document.getElementById("detailModal").addEventListener("click", function (e) {
  if (e.target === this) {
    cerrarModal();
  }
});

// ========== FUNCIONES DE NAVEGACIÓN ==========

function irARecetas() {
  console.log("Navegando a recetas - idPaciente:", idPacienteActual);

  if (idPacienteActual) {
    window.location.href = `/Home/HistorialRecetas/${idPacienteActual}`;
  }
}

function irALaboratorio() {
  window.location.href = `/laboratorio?idPaciente=${idPacienteActual}`;
}

function irAConsulta() {
  window.location.href = `/consulta?idPaciente=${idPacienteActual}`;
}

function irATriaje() {
  window.location.href = `/triaje/historial?idPaciente=${idPacienteActual}`;
}

function irACitas() {
  window.location.href = `/citas?idPaciente=${idPacienteActual}`;
}

function irADerivaciones() {
  window.location.href = `/HistorialDerivacion/Historial?idCita=${idPacienteActual}`;
}

function terminarCita() {
  if (confirm("¿Está seguro de que desea terminar la cita?")) {
    // Aquí podrías actualizar el estado de la cita
    window.location.href = "/citas";
  }
}
