const API_BASE_URL = "http://localhost:5122";

// Inicializar cuando cargue la página
document.addEventListener("DOMContentLoaded", function () {
  cargarDatosPaciente();
  document.getElementById("pesoTriaje").addEventListener("change", calcularIMC);
  document
    .getElementById("tallaTriaje")
    .addEventListener("change", calcularIMC);
  document
    .getElementById("riesgoEnfermedad")
    .addEventListener("change", mostrarRiesgoInfo);
});

// Cargar datos del paciente desde parámetros
function cargarDatosPaciente() {
  const params = new URLSearchParams(window.location.search);
  const idPaciente = params.get("idPaciente");

  if (!idPaciente) {
    alert("No se especificó el paciente");
    window.history.back();
    return;
  }

  document.getElementById("idPaciente").value = idPaciente;
  obtenerDatosPaciente(idPaciente);
}

// Obtener datos del paciente desde la API
async function obtenerDatosPaciente(idPaciente) {
  try {
    const res = await fetch(`${API_BASE_URL}/pacientes/${idPaciente}`);
    const result = await res.json();

    if (result.success && result.data) {
      const paciente = result.data;
      mostrarDatosPaciente(paciente);
    } else {
      alert("No se pudo cargar los datos del paciente");
    }
  } catch (error) {
    console.error("Error:", error);
    alert("Error al cargar los datos del paciente");
  }
}

// Mostrar datos del paciente en la vista
function mostrarDatosPaciente(paciente) {
  const nombre = `${
    paciente.apellidoPaterno || paciente.ApellidoPaterno || ""
  } ${paciente.apellidoMaterno || paciente.ApellidoMaterno || ""}, ${
    paciente.nombre || paciente.Nombre || ""
  }`;

  document.getElementById("pacienteName").textContent = nombre;
  document.getElementById("pacienteDocumento").textContent =
    paciente.numeroDocumento || paciente.NumeroDocumento || "-";
  document.getElementById("pacienteSexo").textContent =
    paciente.sexo === "M" || paciente.Sexo === "M" ? "Masculino" : "Femenino";
  document.getElementById("pacienteEdad").textContent =
    (paciente.edad || paciente.Edad) + " años";
  document.getElementById("pacienteTipo").textContent =
    paciente.tipoDocumento || paciente.TipoDocumento || "-";
}

// Calcular IMC y clasificación
// CORREGIR: Agregar validación
function calcularIMC() {
  const peso = parseFloat(document.getElementById("pesoTriaje").value);
  const talla = parseFloat(document.getElementById("tallaTriaje").value) / 100;

  if (peso && talla && talla > 0) {
    // ✅ Agregar validación de talla > 0
    const imc = (peso / (talla * talla)).toFixed(2);
    document.getElementById("imc").value = imc;

    let clasificacion = "";
    if (imc < 18.5) clasificacion = "Bajo peso";
    else if (imc < 25) clasificacion = "Peso normal";
    else if (imc < 30) clasificacion = "Sobrepeso";
    else if (imc < 35) clasificacion = "Obesidad I";
    else clasificacion = "Obesidad II";

    document.getElementById("clasificacionImc").value = clasificacion;
    document.getElementById(
      "imcInfo"
    ).innerHTML = `<strong>IMC: ${imc}</strong> - ${clasificacion}`;
  } else {
    // Limpiar si no hay datos válidos
    document.getElementById("imc").value = "";
    document.getElementById("clasificacionImc").value = "";
    document.getElementById("imcInfo").innerHTML =
      "El IMC se calculará automáticamente cuando completes peso y talla";
  }
}

// Mostrar información del riesgo
function mostrarRiesgoInfo() {
  const riesgo = document.getElementById("riesgoEnfermedad").value;
  const riesgoInfo = document.getElementById("riesgoInfo");

  if (!riesgo) {
    riesgoInfo.style.display = "none";
    return;
  }

  riesgoInfo.style.display = "block";
  riesgoInfo.innerHTML = `Riesgo de enfermedad seleccionado: <strong>${riesgo}</strong>`;
}

// Guardar triaje
async function guardarTriaje(e) {
  e.preventDefault();

  // ✅ AGREGAR VALIDACIÓN
  if (!validarFormulario()) {
    return;
  }

  const formData = {
    idPaciente: parseInt(document.getElementById("idPaciente").value),
    temperatura: parseFloat(document.getElementById("temperatura").value),
    presionArterial: parseInt(document.getElementById("presionArterial").value),
    saturacion: parseInt(document.getElementById("saturacion").value),
    frecuenciaCardiaca: parseInt(
      document.getElementById("frecuenciaCardiaca").value
    ),
    frecuenciaRespiratoria: parseInt(
      document.getElementById("frecuenciaRespiratoria").value
    ),
    peso: parseFloat(document.getElementById("pesoTriaje").value),
    talla: parseFloat(document.getElementById("tallaTriaje").value),
    perimAbdominal: parseFloat(document.getElementById("perimAbdominal").value),
    superficieCorporal: parseFloat(
      document.getElementById("superficieCorporal").value
    ),
    imc: parseFloat(document.getElementById("imc").value) || 0,
    clasificacionImc: document.getElementById("clasificacionImc").value,
    riesgoEnfermedad: document.getElementById("riesgoEnfermedad").value,
    estadoTriage: document.getElementById("estadoTriage").value,
    observaciones: document.getElementById("observaciones").value,
  };

  console.log("📤 Enviando datos:", formData); // ✅ Agregar log

  try {
    const res = await fetch(`${API_BASE_URL}/triaje/register`, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(formData),
    });

    const result = await res.json();
    console.log("📥 Respuesta:", result); // ✅ Agregar log

    if (result.success) {
      alert("✅ Triaje registrado correctamente");
      window.location.href = "/triaje/listado"; // ✅ Mejor que history.back()
    } else {
      alert("❌ " + (result.message || "Error al registrar el triaje"));
    }
  } catch (error) {
    console.error("❌ Error:", error);
    alert("❌ Error de conexión al guardar el triaje");
  }
}

// ✅ AGREGAR FUNCIÓN DE VALIDACIÓN
function validarFormulario() {
  const camposRequeridos = [
    "temperatura",
    "presionArterial",
    "frecuenciaCardiaca",
    "saturacion",
    "frecuenciaRespiratoria",
    "pesoTriaje",
    "tallaTriaje",
    "perimAbdominal",
    "superficieCorporal",
    "riesgoEnfermedad",
    "estadoTriage",
  ];

  for (let campo of camposRequeridos) {
    const elemento = document.getElementById(campo);
    if (!elemento.value.trim()) {
      alert(`Por favor complete el campo: ${elemento.labels[0].textContent}`);
      elemento.focus();
      return false;
    }
  }

  // Validar IMC calculado
  const imc = document.getElementById("imc").value;
  if (!imc || imc === "0") {
    alert("Por favor complete peso y talla para calcular el IMC");
    document.getElementById("pesoTriaje").focus();
    return false;
  }

  return true;
}

// Cancelar
function cancelarTriaje() {
  if (confirm("¿Estás seguro de que deseas cancelar?")) {
    window.history.back();
  }
}
