const API_BASE_URL = "http://localhost:5122";
let tabActual = "por-triar";
let pacientesData = [];

document.addEventListener("DOMContentLoaded", function () {
  console.log("Página cargada, iniciando...");
  cargarPacientesPorTriar();
});

// Cambiar de tab
function cambiarTab(tab) {
  console.log("Cambiando a tab:", tab);
  tabActual = tab;

  // Actualizar estilos de tabs
  document
    .querySelectorAll(".tab-btn")
    .forEach((btn) => btn.classList.remove("active"));
  document.getElementById(`tab-${tab}`).classList.add("active");

  // Mostrar/ocultar campos de fecha según el tab
  const fechaInicioGroup = document.getElementById("fechaInicioGroup");
  const fechaFinGroup = document.getElementById("fechaFinGroup");

  if (tab === "triados") {
    fechaInicioGroup.style.display = "flex";
    fechaFinGroup.style.display = "flex";
  } else {
    fechaInicioGroup.style.display = "none";
    fechaFinGroup.style.display = "none";
  }

  // Limpiar campos de búsqueda
  limpiarCamposFiltro();

  // Cargar datos según el tab
  if (tab === "por-triar") {
    cargarPacientesPorTriar();
  } else if (tab === "fuera-horario") {
    cargarPacientesFueraHorario();
  } else if (tab === "triados") {
    cargarPacientesTriados();
  }
}

// Limpiar filtros y recargar datos
function limpiarFiltros() {
  console.log("Limpiando filtros y recargando datos...");

  // Limpiar campos
  limpiarCamposFiltro();

  // Recargar datos según el tab actual
  if (tabActual === "por-triar") {
    cargarPacientesPorTriar();
  } else if (tabActual === "fuera-horario") {
    cargarPacientesFueraHorario();
  } else if (tabActual === "triados") {
    cargarPacientesTriados();
  }
}

// Función auxiliar para limpiar campos del filtro
function limpiarCamposFiltro() {
  document.getElementById("tipoBusqueda").value = "";
  document.getElementById("numeroDocumento").value = "";
  if (document.getElementById("fechaInicio"))
    document.getElementById("fechaInicio").value = "";
  if (document.getElementById("fechaFin"))
    document.getElementById("fechaFin").value = "";
}

// Buscar pacientes
function buscarPacientes(e) {
  e.preventDefault();
  console.log("Buscando pacientes en tab:", tabActual);

  const tipoDoc = document.getElementById("tipoBusqueda").value;
  const numeroDoc = document.getElementById("numeroDocumento").value.trim();

  // Validar que se haya seleccionado tipo de documento
  if (tipoDoc === "Seleccione" || !tipoDoc) {
    alert("Por favor seleccione un tipo de documento");
    return;
  }

  // Validar que se haya ingresado número de documento
  if (!numeroDoc) {
    alert("Por favor ingrese el número de documento");
    return;
  }

  if (tabActual === "triados") {
    const fechaInicio = document.getElementById("fechaInicio")?.value;
    const fechaFin = document.getElementById("fechaFin")?.value;
    buscarTriajes(tipoDoc, numeroDoc, fechaInicio, fechaFin);
  } else if (tabActual === "por-triar") {
    buscarCitasPendientes(tipoDoc, numeroDoc);
  } else if (tabActual === "fuera-horario") {
    buscarCitasFueraHorario(tipoDoc, numeroDoc);
  }
}

// Buscar citas pendientes
async function buscarCitasPendientes(tipoDoc, numeroDoc) {
  console.log(`Buscando citas pendientes: ${tipoDoc} - ${numeroDoc}`);
  try {
    const url = `${API_BASE_URL}/citas/buscar-pendientes?tipoDoc=${encodeURIComponent(
      tipoDoc
    )}&numeroDoc=${encodeURIComponent(numeroDoc)}`;
    console.log("URL:", url);

    const res = await fetch(url);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      if (result.data.length === 0) {
        alert("No se encontraron citas pendientes con ese documento");
        renderizarTabla([], "triar");
      } else {
        renderizarTabla(result.data, "triar");
      }
    } else {
      alert(result.message || "Error en la búsqueda");
      renderizarTabla([], "triar");
    }
  } catch (error) {
    console.error("Error:", error);
    alert("Error al realizar la búsqueda");
    renderizarTabla([], "triar");
  }
}

// Buscar citas fuera de horario
async function buscarCitasFueraHorario(tipoDoc, numeroDoc) {
  console.log(`Buscando citas fuera de horario: ${tipoDoc} - ${numeroDoc}`);
  try {
    const url = `${API_BASE_URL}/citas/buscar-fuera-horario?tipoDoc=${encodeURIComponent(
      tipoDoc
    )}&numeroDoc=${encodeURIComponent(numeroDoc)}`;
    console.log("URL:", url);

    const res = await fetch(url);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      if (result.data.length === 0) {
        alert("No se encontraron citas fuera de horario con ese documento");
        renderizarTabla([], "triar");
      } else {
        renderizarTabla(result.data, "triar");
      }
    } else {
      alert(result.message || "Error en la búsqueda");
      renderizarTabla([], "triar");
    }
  } catch (error) {
    console.error("Error:", error);
    alert("Error al realizar la búsqueda");
    renderizarTabla([], "triar");
  }
}

// Buscar triajes
async function buscarTriajes(tipoDoc, numeroDoc, fechaInicio, fechaFin) {
  console.log(
    `Buscando triajes: ${tipoDoc} - ${numeroDoc}, Fechas: ${fechaInicio} a ${fechaFin}`
  );
  try {
    let url = `${API_BASE_URL}/triaje/buscar?`;

    if (tipoDoc && tipoDoc !== "Seleccione") {
      url += `tipoDoc=${encodeURIComponent(tipoDoc)}&`;
    }
    if (numeroDoc) {
      url += `numeroDoc=${encodeURIComponent(numeroDoc)}&`;
    }
    if (fechaInicio) {
      url += `fechaInicio=${encodeURIComponent(fechaInicio)}&`;
    }
    if (fechaFin) {
      url += `fechaFin=${encodeURIComponent(fechaFin)}&`;
    }

    url = url.slice(0, -1);
    console.log("URL:", url);

    const res = await fetch(url);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      if (result.data.length === 0) {
        mostrarErrorEnTabla("No se encontraron triajes con esos criterios");
      } else {
        renderizarTabla(result.data, "editar");
      }
    } else {
      mostrarErrorEnTabla(result.message || "Error en la búsqueda");
    }
  } catch (error) {
    console.error("Error:", error);
    mostrarErrorEnTabla("Error de conexión. No se pudo realizar la búsqueda.");
  }
}

async function guardarTriaje(event) {
  event.preventDefault();
  console.log("Guardando triaje...");
  try {
    const formData = {
      idPaciente: document.getElementById("idPaciente").value,
      presionArterial: document.getElementById("presionArterial").value,
      temperatura:
        parseFloat(document.getElementById("temperatura").value) || 0,
      frecuenciaCardiaca:
        parseInt(document.getElementById("frecuenciaCardiaca").value) || 0,
      saturacionOxigeno:
        parseInt(document.getElementById("saturacionOxigeno").value) || 0,
      peso: parseFloat(document.getElementById("peso").value) || 0,
      talla: parseFloat(document.getElementById("talla").value) || 0,
      observaciones: document.getElementById("observaciones").value,
    };

    console.log("Enviando datos:", formData);

    const response = await fetch(`${API_BASE_URL}/triaje/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(formData),
    });

    const result = await response.json();
    console.log("Respuesta:", result);

    if (result.success) {
      alert("Triaje registrado exitosamente");
      window.location.href = "/triaje/listado";
    } else {
      alert("Error: " + result.message);
    }
  } catch (error) {
    console.error("Error:", error);
    alert("Error al guardar el triaje");
  }
}

// Cargar pacientes por triar
async function cargarPacientesPorTriar() {
  console.log("Llamando a /citas/pendientesportriage...");
  try {
    const res = await fetch(`${API_BASE_URL}/citas/pendientesportriage`);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      renderizarTabla(result.data, "triar");
    } else {
      console.warn("No se pudieron cargar las citas:", result.message);
      renderizarTabla([], "triar");
    }
  } catch (error) {
    console.error("Error:", error);
    renderizarTabla([], "triar");
  }
}

// Cargar pacientes fuera de horario
async function cargarPacientesFueraHorario() {
  console.log("Llamando a /citas/fuera-horario...");
  try {
    const res = await fetch(`${API_BASE_URL}/citas/fuera-horario`);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      renderizarTabla(result.data, "triar");
    } else {
      renderizarTabla([], "triar");
    }
  } catch (error) {
    console.error("Error:", error);
    renderizarTabla([], "triar");
  }
}

// Cargar pacientes ya triados (CON MANEJO DE ERRORES)
async function cargarPacientesTriados() {
  console.log("Llamando a /triaje/all...");
  try {
    const res = await fetch(`${API_BASE_URL}/triaje/all`);
    const result = await res.json();
    console.log("Resultado:", result);

    if (result.success && result.data) {
      renderizarTabla(result.data, "editar");
    } else {
      // ✅ Mostrar mensaje de error en la tabla
      mostrarErrorEnTabla(result.message || "No se pudieron cargar los datos");
    }
  } catch (error) {
    console.error("Error:", error);
    // ✅ Mostrar mensaje de error en la tabla
    mostrarErrorEnTabla(
      "Error de conexión. No se pudieron cargar los datos. Por favor, intente nuevamente."
    );
  }
}

// Renderizar tabla
function renderizarTabla(data, tipo) {
  console.log("Renderizando:", data?.length, "registros, tipo:", tipo);
  const tbody = document.getElementById("tablaPacientesBody");

  if (!data || data.length === 0) {
    tbody.innerHTML =
      '<tr><td colspan="7" class="no-data">No hay datos disponibles</td></tr>';
    return;
  }

  if (tipo === "editar") {
    tbody.innerHTML = data
      .map((item) => {
        const fechaFormateada = item.fechaCita
          ? new Date(item.fechaCita).toLocaleDateString("es-PE")
          : "-";
        return `
                <tr>
                    <td>${item.numeroDocumento || ""}</td>
                    <td>${item.nombreCompletoPaciente || ""}</td>
                    <td>${item.consultorio || ""}</td>
                    <td>${item.horaCita || ""}</td>
                    <td>${fechaFormateada}</td>
                    <td>${item.nombreCompletoMedico || ""}</td>
                    <td><button class="btn-action btn-editar" onclick="irAEditarTriaje(${
                      item.idTriaje
                    })">Editar</button></td> 
                </tr>
            `;
      })
      .join("");
  } else {
    tbody.innerHTML = data
      .map((item) => {
        const fechaFormateada = item.fechaCita
          ? new Date(item.fechaCita).toLocaleDateString("es-PE")
          : "-";
        return `
                <tr>
                    <td>${item.numeroDocumento || ""}</td>
                    <td>${item.nombreCompletoPaciente || ""}</td>
                    <td>${item.consultorio || ""}</td>
                    <td>${item.horaCita || ""}</td>
                    <td>${fechaFormateada}</td>
                    <td>${item.nombreCompletoMedico || ""}</td>
                    <td><button class="btn-action btn-triar" onclick="irARegistrarTriaje(${
                      item.idPaciente
                    })">Triar</button></td>
                </tr>
            `;
      })
      .join("");
  }
}

// ✅ NUEVA FUNCIÓN: Mostrar error en la tabla
function mostrarErrorEnTabla(mensaje) {
  const tbody = document.getElementById("tablaPacientesBody");
  tbody.innerHTML = `
    <tr>
      <td colspan="7" style="color: #dc3545; padding: 30px; text-align: center; font-weight: 500; font-size: 15px;">
        <i class="fas fa-exclamation-triangle" style="margin-right: 10px; font-size: 18px;"></i>
        ${mensaje}
      </td>
    </tr>
  `;
}

// Ir a registrar triaje
function irARegistrarTriaje(idPaciente) {
  console.log("Redirigiendo a registrar triaje, paciente:", idPaciente);
  window.location.href = `/triaje/registrar?idPaciente=${idPaciente}`;
}

// Ir a editar triaje
function irAEditarTriaje(idTriaje) {
  console.log("Redirigiendo a editar triaje:", idTriaje);
  window.location.href = `/triaje/editar/${idTriaje}`;
}

// Paginación
function cambiarPagina(direccion) {
  console.log("Cambiar página:", direccion);
}
