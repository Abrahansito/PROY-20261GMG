const API_BASE_URL = "http://localhost:5122";

// Inicializar cuando cargue la página
document.addEventListener("DOMContentLoaded", function() {
    cargarDatosTriaje();
    document.getElementById("pesoTriaje").addEventListener("change", calcularIMC);
    document.getElementById("tallaTriaje").addEventListener("change", calcularIMC);
    document.getElementById("riesgoEnfermedad").addEventListener("change", mostrarRiesgoInfo);
});

//SIMPLIFICADO: Solo una llamada
async function cargarDatosTriaje() {
    const params = new URLSearchParams(window.location.search);
    const idTriaje = params.get("idTriaje");
    const idPaciente = params.get("idPaciente");
    
    if (!idTriaje || !idPaciente) {
        alert("No se especificaron los datos necesarios");
        window.history.back();
        return;
    }

    document.getElementById("idTriaje").value = idTriaje;
    document.getElementById("idPaciente").value = idPaciente;

    try {
        const res = await fetch(`${API_BASE_URL}/triaje/${idTriaje}`);
        const result = await res.json();

        if (result.success && result.data) {
            const triaje = result.data;
            
            // Mostrar datos del paciente
            mostrarDatosPaciente(triaje);
            
            // Llenar formulario del triaje
            llenarFormularioTriaje(triaje);
        } else {
            alert("No se pudo cargar los datos del triaje: " + (result.message || "Error desconocido"));
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Error al cargar los datos del triaje");
    }
}

// Mostrar datos del paciente
function mostrarDatosPaciente(data) {
    const nombre = `${data.apellidoPaterno || ""} ${data.apellidoMaterno || ""}, ${data.nombre || ""}`.trim();
    
    document.getElementById("pacienteName").textContent = nombre;
    document.getElementById("pacienteDocumento").textContent = data.numeroDocumento || "-";
    document.getElementById("pacienteSexo").textContent = data.sexo === "M" ? "Masculino" : "Femenino";
    document.getElementById("pacienteEdad").textContent = `${data.edad} años`;
    document.getElementById("pacienteTipo").textContent = data.tipoDocumento || "-";
}

// Llenar formulario con datos del triaje
function llenarFormularioTriaje(triaje) {
    document.getElementById("temperatura").value = triaje.temperatura || "";
    document.getElementById("presionArterial").value = triaje.presionArterial || "";
    document.getElementById("frecuenciaCardiaca").value = triaje.frecuenciaCardiaca || "";
    document.getElementById("saturacion").value = triaje.saturacion || "";
    document.getElementById("frecuenciaRespiratoria").value = triaje.frecuenciaRespiratoria || "";
    document.getElementById("pesoTriaje").value = triaje.peso || "";
    document.getElementById("tallaTriaje").value = triaje.talla || "";
    document.getElementById("perimAbdominal").value = triaje.perimetroAbdominal || "";
    document.getElementById("superficieCorporal").value = triaje.superficieCorporal || "";
    document.getElementById("imc").value = triaje.imc || "";
    document.getElementById("clasificacionImc").value = triaje.clasificacionImc || "";
    document.getElementById("riesgoEnfermedad").value = triaje.riesgoEnfermedad || "";
    document.getElementById("estadoTriage").value = triaje.estadoTriage || "";
    document.getElementById("observaciones").value = triaje.observaciones || "";

    // Actualizar info de IMC
    if (triaje.imc) {
        document.getElementById("imcInfo").innerHTML = 
            `<strong>IMC: ${triaje.imc}</strong> - ${triaje.clasificacionImc}`;
    }

    // Mostrar info de riesgo si existe
    if (triaje.riesgoEnfermedad) {
        mostrarRiesgoInfo();
    }
}

// Calcular IMC
function calcularIMC() {
    const peso = parseFloat(document.getElementById("pesoTriaje").value);
    const talla = parseFloat(document.getElementById("tallaTriaje").value) / 100;

    if (peso && talla) {
        const imc = (peso / (talla * talla)).toFixed(2);
        document.getElementById("imc").value = imc;

        let clasificacion = "";
        if (imc < 18.5) clasificacion = "Bajo peso";
        else if (imc >= 18.5 && imc < 25) clasificacion = "Peso normal";
        else if (imc >= 25 && imc < 30) clasificacion = "Sobrepeso";
        else if (imc >= 30 && imc < 35) clasificacion = "Obesidad I";
        else if (imc >= 35) clasificacion = "Obesidad II";

        document.getElementById("clasificacionImc").value = clasificacion;
        document.getElementById("imcInfo").innerHTML = `<strong>IMC: ${imc}</strong> - ${clasificacion}`;
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

// Actualizar triaje
async function actualizarTriaje(e) {
    e.preventDefault();

    const formData = {
        idTriaje: parseInt(document.getElementById("idTriaje").value),
        idPaciente: parseInt(document.getElementById("idPaciente").value),
        temperatura: parseFloat(document.getElementById("temperatura").value),
        presionArterial: parseInt(document.getElementById("presionArterial").value),
        saturacion: parseInt(document.getElementById("saturacion").value),
        frecuenciaCardiaca: parseInt(document.getElementById("frecuenciaCardiaca").value),
        frecuenciaRespiratoria: parseInt(document.getElementById("frecuenciaRespiratoria").value),
        peso: parseFloat(document.getElementById("pesoTriaje").value),
        talla: parseFloat(document.getElementById("tallaTriaje").value),
        perimAbdominal: parseFloat(document.getElementById("perimAbdominal").value),
        superficieCorporal: parseFloat(document.getElementById("superficieCorporal").value),
        imc: parseFloat(document.getElementById("imc").value),
        clasificacionImc: document.getElementById("clasificacionImc").value,
        riesgoEnfermedad: document.getElementById("riesgoEnfermedad").value,
        estadoTriage: document.getElementById("estadoTriage").value,
        observaciones: document.getElementById("observaciones").value
    };

    try {
        const res = await fetch(`${API_BASE_URL}/triaje/update`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(formData)
        });

        const result = await res.json();

        if (result.success) {
            alert("Triaje actualizado correctamente");
            window.location.href = "/triaje/listado";
        } else {
            alert(result.message || "Error al actualizar el triaje");
        }
    } catch (error) {
        console.error("Error:", error);
        alert("Error al actualizar el triaje");
    }
}

// Cancelar edición
function cancelarEdicion() {
    if (confirm("¿Estás seguro de que deseas cancelar? Los cambios no guardados se perderán.")) {
        window.location.href = "/triaje/listado";
    }
}



