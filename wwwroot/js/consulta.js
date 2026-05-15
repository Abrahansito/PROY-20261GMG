const API_BASE_URL = "http://localhost:5122";
let idPacienteActual = null;
let idMedicoActual = null;
let idCitaActual = null;
let idConsultaActual = null;

// Datos temporales de las pestañas
let datosConsulta = {
    motivoConsulta: '',
    sintomasPresentados: '',
    diagnosticoPrincipal: '',
    codigoCie10: '',
    observaciones: '',
    descripcionEvolucion: '',
    indicacionesRecomendaciones: ''
};

// Inicializar
document.addEventListener('DOMContentLoaded', function() {
    obtenerParametrosURL();
    
    // Si hay idConsulta, cargar datos existentes
    if (idConsultaActual) {
        cargarDatosConsulta();
    }
});

// Obtener parámetros de la URL
function obtenerParametrosURL() {
    const params = new URLSearchParams(window.location.search);
    idPacienteActual = params.get('idPaciente');
    idMedicoActual = params.get('idMedico');
    idCitaActual = params.get('idCita');
    idConsultaActual = params.get('idConsulta'); // Para edición
    
    if (!idPacienteActual) {
        showAlert('No se especificó el ID del paciente', 'error');
        setTimeout(() => window.history.back(), 2000);
    }
}

// Cambiar de tab
function cambiarTab(tab) {
    // Remover active de todos los tabs
    document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.tab-content').forEach(c => c.classList.remove('active'));
    
    // Activar tab seleccionado
    document.getElementById(`tab-${tab}`).classList.add('active');
    document.getElementById(`content-${tab}`).classList.add('active');
    
    // Restaurar datos guardados temporalmente
    restaurarDatosTab(tab);
}

// Guardar datos del tab actual antes de cambiar
function guardarDatosTemporales(tab) {
    if (tab === 'sintomas') {
        datosConsulta.motivoConsulta = document.getElementById('motivoConsulta').value;
        datosConsulta.sintomasPresentados = document.getElementById('sintomasPresentados').value;
    } else if (tab === 'diagnostico') {
        datosConsulta.diagnosticoPrincipal = document.getElementById('diagnosticoPrincipal').value;
        datosConsulta.codigoCie10 = document.getElementById('codigoCie10').value;
        datosConsulta.observaciones = document.getElementById('observaciones').value;
    } else if (tab === 'evolucion') {
        datosConsulta.descripcionEvolucion = document.getElementById('descripcionEvolucion').value;
        datosConsulta.indicacionesRecomendaciones = document.getElementById('indicacionesRecomendaciones').value;
    }
}

// Restaurar datos al cambiar de tab
function restaurarDatosTab(tab) {
    if (tab === 'sintomas') {
        document.getElementById('motivoConsulta').value = datosConsulta.motivoConsulta || '';
        document.getElementById('sintomasPresentados').value = datosConsulta.sintomasPresentados || '';
    } else if (tab === 'diagnostico') {
        document.getElementById('diagnosticoPrincipal').value = datosConsulta.diagnosticoPrincipal || '';
        document.getElementById('codigoCie10').value = datosConsulta.codigoCie10 || '';
        document.getElementById('observaciones').value = datosConsulta.observaciones || '';
    } else if (tab === 'evolucion') {
        document.getElementById('descripcionEvolucion').value = datosConsulta.descripcionEvolucion || '';
        document.getElementById('indicacionesRecomendaciones').value = datosConsulta.indicacionesRecomendaciones || '';
    }
}

// Guardar Síntomas
async function guardarSintomas() {
    const motivoConsulta = document.getElementById('motivoConsulta').value.trim();
    const sintomasPresentados = document.getElementById('sintomasPresentados').value.trim();
    
    if (!motivoConsulta || !sintomasPresentados) {
        showAlert('Por favor complete todos los campos obligatorios', 'warning');
        return;
    }
    
    datosConsulta.motivoConsulta = motivoConsulta;
    datosConsulta.sintomasPresentados = sintomasPresentados;
    
    showSuccess('Síntomas guardados. Puede continuar con el diagnóstico.');
    
    // Cambiar a la siguiente pestaña
    setTimeout(() => cambiarTab('diagnostico'), 1000);
}

// Guardar Diagnóstico
function guardarDiagnostico() {
    const diagnosticoPrincipal = document.getElementById('diagnosticoPrincipal').value.trim();
    const observaciones = document.getElementById('observaciones').value.trim();
    
    if (!diagnosticoPrincipal || !observaciones) {
        showAlert('Por favor complete todos los campos obligatorios', 'warning');
        return;
    }
    
    datosConsulta.diagnosticoPrincipal = diagnosticoPrincipal;
    datosConsulta.codigoCie10 = document.getElementById('codigoCie10').value.trim();
    datosConsulta.observaciones = observaciones;
    
    showSuccess('Diagnóstico guardado. Puede continuar con la evolución.');
    
    // Cambiar a la siguiente pestaña
    setTimeout(() => cambiarTab('evolucion'), 1000);
}

// Guardar Evolución (y enviar todo)
async function guardarEvolucion() {
    const descripcionEvolucion = document.getElementById('descripcionEvolucion').value.trim();
    
    if (!descripcionEvolucion) {
        showAlert('Por favor complete la descripción de la evolución', 'warning');
        return;
    }
    
    datosConsulta.descripcionEvolucion = descripcionEvolucion;
    datosConsulta.indicacionesRecomendaciones = document.getElementById('indicacionesRecomendaciones').value.trim();
    
    // Validar que se hayan completado los tabs anteriores
    if (!datosConsulta.motivoConsulta || !datosConsulta.sintomasPresentados) {
        showAlert('Por favor complete la pestaña de Síntomas primero', 'warning');
        cambiarTab('sintomas');
        return;
    }
    
    if (!datosConsulta.diagnosticoPrincipal || !datosConsulta.observaciones) {
        showAlert('Por favor complete la pestaña de Diagnóstico primero', 'warning');
        cambiarTab('diagnostico');
        return;
    }
    
    // Enviar consulta completa al backend
    await enviarConsultaCompleta();
}

// Enviar consulta completa al backend
async function enviarConsultaCompleta() {
    try {
        const consultaDTO = {
            idConsulta: idConsultaActual || 0,
            idPaciente: parseInt(idPacienteActual),
            idMedico: idMedicoActual ? parseInt(idMedicoActual) : null,
            idCita: idCitaActual ? parseInt(idCitaActual) : null,
            motivoConsulta: datosConsulta.motivoConsulta,
            sintomasPresentados: datosConsulta.sintomasPresentados,
            diagnosticoPrincipal: datosConsulta.diagnosticoPrincipal,
            codigoCie10: datosConsulta.codigoCie10,
            observaciones: datosConsulta.observaciones,
            descripcionEvolucion: datosConsulta.descripcionEvolucion,
            indicacionesRecomendaciones: datosConsulta.indicacionesRecomendaciones
        };
        
        const endpoint = idConsultaActual 
            ? `${API_BASE_URL}/api/consulta/actualizar`
            : `${API_BASE_URL}/api/consulta/registrar`;
            
        const method = idConsultaActual ? 'PUT' : 'POST';
        
        const res = await fetch(endpoint, {
            method: method,
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(consultaDTO)
        });
        
        const result = await res.json();
        
        if (result.success) {
            showSuccess('Consulta registrada exitosamente');
            setTimeout(() => {
                window.location.href = construirUrlHistoriaClinica();
            }, 2000);
        } else {
            showError(result.message || 'Error al guardar la consulta');
        }
    } catch (error) {
        console.error('Error:', error);
        showError('Error al conectar con el servidor');
    }
}

// Cargar datos de consulta existente (para edición)
async function cargarDatosConsulta() {
    try {
        const res = await fetch(`${API_BASE_URL}/api/consulta/${idConsultaActual}`);
        const result = await res.json();
        
        if (result.success && result.data) {
            const consulta = result.data;
            
            // Llenar datos temporales
            datosConsulta = {
                motivoConsulta: consulta.motivoConsulta || '',
                sintomasPresentados: consulta.sintomasPresentados || '',
                diagnosticoPrincipal: consulta.diagnosticoPrincipal || '',
                codigoCie10: consulta.codigoCie10 || '',
                observaciones: consulta.observaciones || '',
                descripcionEvolucion: consulta.descripcionEvolucion || '',
                indicacionesRecomendaciones: consulta.indicacionesRecomendaciones || ''
            };
            
            // Restaurar en el tab actual
            restaurarDatosTab('sintomas');
            
            showInfo('Consulta cargada para edición');
        }
    } catch (error) {
        console.error('Error al cargar consulta:', error);
        showError('Error al cargar los datos de la consulta');
    }
}

// Volver
function volver() {
    mostrarConfirmacionConsulta('¿Está seguro de que desea salir? Los datos no guardados se perderán.', function () {
        window.location.href = construirUrlHistoriaClinica();
    });
}

function construirUrlHistoriaClinica() {
    const params = new URLSearchParams();
    if (idPacienteActual) params.set('idPaciente', idPacienteActual);
    if (idCitaActual) params.set('idCita', idCitaActual);
    if (idMedicoActual) params.set('idMedico', idMedicoActual);
    return `/historia-clinica/ver?${params.toString()}`;
}

async function guardarSintomas() {
    const motivoConsulta = document.getElementById('motivoConsulta').value.trim();
    const sintomasPresentados = document.getElementById('sintomasPresentados').value.trim();

    if (!motivoConsulta || !sintomasPresentados) {
        showAlert('Por favor complete todos los campos obligatorios', 'warning');
        return;
    }

    datosConsulta.motivoConsulta = motivoConsulta;
    datosConsulta.sintomasPresentados = sintomasPresentados;

    const guardado = await guardarConsultaEnServidor('Sintomas guardados. Puede continuar con el diagnostico.', false);
    if (guardado) setTimeout(() => cambiarTab('diagnostico'), 700);
}

async function guardarDiagnostico() {
    const diagnosticoPrincipal = document.getElementById('diagnosticoPrincipal').value.trim();
    const observaciones = document.getElementById('observaciones').value.trim();

    if (!datosConsulta.motivoConsulta || !datosConsulta.sintomasPresentados) {
        showAlert('Primero debe guardar los sintomas del paciente', 'warning');
        cambiarTab('sintomas');
        return;
    }

    if (!diagnosticoPrincipal || !observaciones) {
        showAlert('Por favor complete todos los campos obligatorios', 'warning');
        return;
    }

    datosConsulta.diagnosticoPrincipal = diagnosticoPrincipal;
    datosConsulta.codigoCie10 = document.getElementById('codigoCie10').value.trim();
    datosConsulta.observaciones = observaciones;

    const guardado = await guardarConsultaEnServidor('Diagnostico guardado. Puede continuar con la evolucion.', false);
    if (guardado) setTimeout(() => cambiarTab('evolucion'), 700);
}

async function guardarEvolucion() {
    const descripcionEvolucion = document.getElementById('descripcionEvolucion').value.trim();

    if (!descripcionEvolucion) {
        showAlert('Por favor complete la descripcion de la evolucion', 'warning');
        return;
    }

    datosConsulta.descripcionEvolucion = descripcionEvolucion;
    datosConsulta.indicacionesRecomendaciones = document.getElementById('indicacionesRecomendaciones').value.trim();

    if (!datosConsulta.motivoConsulta || !datosConsulta.sintomasPresentados) {
        showAlert('Por favor complete la pestaña de Sintomas primero', 'warning');
        cambiarTab('sintomas');
        return;
    }

    if (!datosConsulta.diagnosticoPrincipal || !datosConsulta.observaciones) {
        showAlert('Por favor complete la pestaña de Diagnostico primero', 'warning');
        cambiarTab('diagnostico');
        return;
    }

    await guardarConsultaEnServidor('Consulta registrada exitosamente', true);
}

async function guardarConsultaEnServidor(mensajeExito, redirigir) {
    try {
        const consultaDTO = {
            idConsulta: idConsultaActual ? parseInt(idConsultaActual) : 0,
            idPaciente: parseInt(idPacienteActual),
            idMedico: idMedicoActual ? parseInt(idMedicoActual) : 0,
            idCita: idCitaActual ? parseInt(idCitaActual) : null,
            motivoConsulta: datosConsulta.motivoConsulta,
            sintomasPresentados: datosConsulta.sintomasPresentados,
            diagnosticoPrincipal: datosConsulta.diagnosticoPrincipal,
            codigoCie10: datosConsulta.codigoCie10,
            observaciones: datosConsulta.observaciones,
            descripcionEvolucion: datosConsulta.descripcionEvolucion,
            indicacionesRecomendaciones: datosConsulta.indicacionesRecomendaciones
        };

        const endpoint = idConsultaActual
            ? `${API_BASE_URL}/api/consulta/actualizar`
            : `${API_BASE_URL}/api/consulta/registrar`;

        const method = idConsultaActual ? 'PUT' : 'POST';
        const res = await fetch(endpoint, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(consultaDTO)
        });

        const result = await res.json();
        if (!result.success) {
            showError(result.message || 'Error al guardar la consulta');
            return false;
        }

        const consulta = result.data || result.Data;
        if (consulta) {
            idConsultaActual = consulta.idConsulta || consulta.IdConsulta || idConsultaActual;
            idMedicoActual = consulta.idMedico || consulta.IdMedico || idMedicoActual;
            idCitaActual = consulta.idCita || consulta.IdCita || idCitaActual;
        }

        if (redirigir) {
            showSuccess(mensajeExito, function () {
                window.location.href = construirUrlHistoriaClinica();
            });
        } else {
            showSuccess(mensajeExito);
        }

        return true;
    } catch (error) {
        console.error('Error:', error);
        showError('Error al conectar con el servidor');
        return false;
    }
}

function showAlert(message, type = 'info') {
    mostrarMensajeConsulta(message, type);
}

function showSuccess(message, onAccept = null) {
    mostrarMensajeConsulta(message, 'success', onAccept);
}

function showError(message) {
    mostrarMensajeConsulta(message, 'error');
}

function showInfo(message) {
    mostrarMensajeConsulta(message, 'info');
}

function mostrarMensajeConsulta(message, type = 'info', onAccept = null) {
    cerrarModalConsulta();

    const config = {
        success: { title: 'Operación exitosa', icon: '✓', className: 'success' },
        warning: { title: 'Campos pendientes', icon: '!', className: 'warning' },
        error: { title: 'No se pudo completar', icon: '!', className: 'error' },
        info: { title: 'Información', icon: 'i', className: 'info' }
    };

    const current = config[type] || config.info;
    const overlay = document.createElement('div');
    overlay.className = 'consulta-modal-overlay consulta-modal-active';
    overlay.innerHTML = `
        <div class="consulta-modal">
            <div class="consulta-modal-header ${current.className}">
                <div class="consulta-modal-icon">${current.icon}</div>
                <h2>${current.title}</h2>
            </div>
            <div class="consulta-modal-body">
                <p>${message}</p>
            </div>
            <div class="consulta-modal-footer">
                <button type="button" class="consulta-modal-btn consulta-modal-btn-primary" id="consultaModalAccept">
                    Aceptar
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(overlay);
    const acceptButton = document.getElementById('consultaModalAccept');
    acceptButton.focus();
    acceptButton.addEventListener('click', function () {
        cerrarModalConsulta();
        if (typeof onAccept === 'function') onAccept();
    });
}

function mostrarConfirmacionConsulta(message, onConfirm) {
    cerrarModalConsulta();

    const overlay = document.createElement('div');
    overlay.className = 'consulta-modal-overlay consulta-modal-active';
    overlay.innerHTML = `
        <div class="consulta-modal">
            <div class="consulta-modal-header warning">
                <div class="consulta-modal-icon">?</div>
                <h2>Confirmar salida</h2>
            </div>
            <div class="consulta-modal-body">
                <p>${message}</p>
            </div>
            <div class="consulta-modal-footer">
                <button type="button" class="consulta-modal-btn consulta-modal-btn-secondary" id="consultaModalCancel">
                    Cancelar
                </button>
                <button type="button" class="consulta-modal-btn consulta-modal-btn-primary" id="consultaModalConfirm">
                    Aceptar
                </button>
            </div>
        </div>
    `;

    document.body.appendChild(overlay);
    document.getElementById('consultaModalCancel').addEventListener('click', cerrarModalConsulta);
    document.getElementById('consultaModalConfirm').addEventListener('click', function () {
        cerrarModalConsulta();
        if (typeof onConfirm === 'function') onConfirm();
    });
}

function cerrarModalConsulta() {
    const modal = document.querySelector('.consulta-modal-overlay');
    if (modal) modal.remove();
}

volver = function () {
    mostrarConfirmacionConsulta('¿Está seguro de que desea salir? Los datos no guardados se perderán.', function () {
        window.location.href = construirUrlHistoriaClinica();
    });
};
