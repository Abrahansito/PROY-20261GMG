// Estado de la aplicación
const appState = {
  medicamentos: [],
  recetaId: null,
  idCita: 1,
  idMedico: 1,
  idPaciente: 1,
  idHistoriaClinica: 1,
};

let elementos = {};

// ===== Inicialización =====
document.addEventListener("DOMContentLoaded", () => {
  inicializarElementos();
  inicializarEventos();
  cargarMedicamentosPendientes();
  console.log("✅ Aplicación inicializada - Generar Receta Médica");
});

function inicializarElementos() {
  elementos = {
    form: document.getElementById("recetaForm"),
    btnAgregar: document.getElementById("btnAgregarMedicamento"),
    btnGuardar: document.getElementById("btnGuardarBasico"), // CORREGIDO
    btnImprimir: document.getElementById("btnImprimirReceta"),
    btnVolver: document.getElementById("btnVolver"),
    tablaMedicamentos: document.getElementById("medicamentosBody"),
    productoInput: document.getElementById("productoFarmaceutico"),
    concentracionInput: document.getElementById("concentracion"),
    frecuenciaInput: document.getElementById("frecuencia"),
    duracionInput: document.getElementById("duracion"),
    viaInput: document.getElementById("viaAdministracion"),
    observacionesInput: document.getElementById("observaciones"),
    observacionesGeneralesInput: document.getElementById(
      "observacionesGenerales"
    ),
  };

  // Obtener IDs ocultos
  appState.idCita = parseInt(document.getElementById("idCita")?.value) || 1;
  appState.idMedico = parseInt(document.getElementById("idMedico")?.value) || 1;
  appState.idPaciente =
    parseInt(document.getElementById("idPaciente")?.value) || 1;
  appState.idHistoriaClinica =
    parseInt(document.getElementById("idHistoriaClinica")?.value) || null;
}

function inicializarEventos() {
  if (elementos.btnAgregar)
    elementos.btnAgregar.addEventListener("click", agregarMedicamento);
  if (elementos.form)
    elementos.form.addEventListener("submit", (e) => e.preventDefault());
  if (elementos.btnGuardar)
    elementos.btnGuardar.addEventListener("click", guardarReceta);
  if (elementos.btnVolver)
    elementos.btnVolver.addEventListener(
      "click",
      () => (window.location.href = "/")
    );
}

// ===== Cargar medicamentos pendientes =====
async function cargarMedicamentosPendientes() {
  try {
    const url = `/RecetaMedica/MedicamentosPendientes?idCita=${
      appState.idCita
    }&idMedico=${appState.idMedico}&idPaciente=${
      appState.idPaciente
    }&idHistoriaClinica=${appState.idHistoriaClinica || ""}`;
    const response = await fetch(url);
    const result = await response.json();

    if (result.success && result.data?.length > 0) {
      appState.medicamentos = result.data;
      actualizarTablaMedicamentos();
    }
  } catch (error) {
    console.error("❌ Error al cargar medicamentos:", error);
  }
}

// ===== Validar campos =====
function validarCamposMedicamento() {
  let esValido = true;
  const campos = [
    {
      input: elementos.productoInput,
      error: "errorProducto",
      mensaje: "El producto farmacéutico es obligatorio",
    },
    {
      input: elementos.concentracionInput,
      error: "errorConcentracion",
      mensaje: "La concentración es obligatoria",
    },
    {
      input: elementos.frecuenciaInput,
      error: "errorFrecuencia",
      mensaje: "La frecuencia es obligatoria",
    },
    {
      input: elementos.duracionInput,
      error: "errorDuracion",
      mensaje: "La duración es obligatoria",
    },
    {
      input: elementos.viaInput,
      error: "errorVia",
      mensaje: "La vía de administración es obligatoria",
    },
  ];

  campos.forEach((campo) => {
    const errorEl = document.getElementById(campo.error);
    if (!campo.input.value.trim()) {
      campo.input.classList.add("error");
      errorEl.textContent = campo.mensaje;
      errorEl.classList.add("show");
      esValido = false;
    } else {
      campo.input.classList.remove("error");
      errorEl.classList.remove("show");
    }
  });
  return esValido;
}

// ===== Agregar medicamento =====
async function agregarMedicamento() {
  if (!validarCamposMedicamento()) {
    mostrarAlerta("Complete todos los campos obligatorios", "error");
    return;
  }

  const data = {
    IdCita: appState.idCita,
    IdMedico: appState.idMedico,
    IdPaciente: appState.idPaciente,
    IdHistoriaClinica: appState.idHistoriaClinica,
    Detalle: {
      ProductoFarmaceutico: elementos.productoInput.value.trim(),
      Concentracion: elementos.concentracionInput.value.trim(),
      Frecuencia: elementos.frecuenciaInput.value.trim(),
      Duracion: elementos.duracionInput.value.trim(),
      ViaAdministracion: elementos.viaInput.value,
      Observaciones: elementos.observacionesInput.value.trim(),
    },
  };

  try {
    elementos.btnAgregar.disabled = true;
    elementos.btnAgregar.innerHTML =
      '<i class="fas fa-spinner fa-spin"></i> Guardando...';

    const response = await fetch("/RecetaMedica/AgregarMedicamento", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    const result = await response.json();
    if (result.success) {
      appState.medicamentos.push(result.data);
      actualizarTablaMedicamentos();
      limpiarFormularioMedicamento();
      mostrarAlerta("✅ Medicamento guardado (Pendiente)", "success");
    } else {
      mostrarAlerta(result.message || "Error al agregar medicamento", "error");
    }
  } catch {
    mostrarAlerta("Error de conexión al agregar", "error");
  } finally {
    elementos.btnAgregar.disabled = false;
    elementos.btnAgregar.innerHTML =
      '<i class="fas fa-plus"></i> Agregar Medicamento';
  }
}

// ===== Actualizar tabla =====
function actualizarTablaMedicamentos() {
  if (appState.medicamentos.length === 0) {
    elementos.tablaMedicamentos.innerHTML = `<tr><td colspan="8" class="text-center-receta">No hay medicamentos agregados</td></tr>`;
    return;
  }

  elementos.tablaMedicamentos.innerHTML = appState.medicamentos
    .map(
      (m, i) => `
    <tr>
      <td>${i + 1}</td>
      <td>${m.productoFarmaceutico}</td>
      <td>${m.concentracion}</td>
      <td>${m.frecuencia}</td>
      <td>${m.duracion}</td>
      <td>${m.viaAdministracion}</td>
      <td>${m.observaciones || "-"}</td>
      <td><button type="button" class="btn-delete-receta" onclick="eliminarMedicamento(${
        m.idDetalle
      })"><i class="fas fa-trash"></i> Eliminar</button></td>
    </tr>
  `
    )
    .join("");
}

// ===== Eliminar medicamento =====
async function eliminarMedicamento(idDetalle) {
  if (!confirm("¿Desea eliminar este medicamento?")) return;

  try {
    const response = await fetch(
      `/RecetaMedica/EliminarMedicamento/${idDetalle}`,
      { method: "DELETE" }
    );
    const result = await response.json();
    if (result.success) {
      appState.medicamentos = appState.medicamentos.filter(
        (m) => m.idDetalle !== idDetalle
      );
      actualizarTablaMedicamentos();
      mostrarAlerta("Medicamento eliminado", "info");
    }
  } catch {
    mostrarAlerta("Error al eliminar medicamento", "error");
  }
}

// ===== Limpiar formulario =====
function limpiarFormularioMedicamento() {
  [
    "productoInput",
    "concentracionInput",
    "frecuenciaInput",
    "duracionInput",
    "viaInput",
    "observacionesInput",
  ].forEach((k) => (elementos[k].value = ""));
}

// ===== Guardar receta =====
async function guardarReceta() {
  if (appState.medicamentos.length === 0) {
    mostrarAlerta("Debe agregar al menos un medicamento", "error");
    return;
  }

  const data = {
    IdCita: appState.idCita,
    IdMedico: appState.idMedico,
    IdPaciente: appState.idPaciente,
    IdHistoriaClinica: appState.idHistoriaClinica,
    ObservacionesGenerales: elementos.observacionesGeneralesInput.value.trim(),
  };

  try {
    elementos.btnGuardar.disabled = true;
    elementos.btnGuardar.innerHTML =
      '<i class="fas fa-spinner fa-spin"></i> Guardando...';

    const response = await fetch("/RecetaMedica/GuardarReceta", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify(data),
    });

    const result = await response.json();
    if (response.ok && result.success) {
      appState.recetaId = result.data?.idReceta ?? null;
      mostrarAlerta(
        "✅ Receta guardada correctamente (Estado: COMPLETADO)",
        "success"
      );
    } else {
      mostrarAlerta(result.message || "Error al guardar receta", "error");
    }
  } catch {
    mostrarAlerta("Error de conexión al guardar receta", "error");
  } finally {
    elementos.btnGuardar.disabled = false;
    elementos.btnGuardar.innerHTML =
      '<i class="fas fa-save"></i> Guardar Receta';
  }
}

// ===== Alertas =====
function mostrarAlerta(mensaje, tipo = "info") {
  const cont = document.getElementById("alertContainerReceta");
  if (!cont) return;
  const div = document.createElement("div");
  div.className = `alert-receta ${tipo}`;
  div.innerHTML = `
    <i class="fas fa-${
      tipo === "success"
        ? "check-circle"
        : tipo === "error"
        ? "exclamation-circle"
        : "info-circle"
    }"></i>
    <span>${mensaje}</span>
    <button class="alert-close-receta" onclick="this.parentElement.remove()">×</button>
  `;
  cont.appendChild(div);
  setTimeout(() => div.remove(), 5000);
}

window.eliminarMedicamento = eliminarMedicamento;
