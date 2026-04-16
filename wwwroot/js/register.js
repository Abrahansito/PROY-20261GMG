// register.js - Manejo del registro de usuarios del hospital

let selectedPersonal = null;
let personalData = [];

document.addEventListener("DOMContentLoaded", function () {
  initializeRegisterForm();
});

function initializeRegisterForm() {
  const roleSelect = document.getElementById("roleSelect");
  const modal = document.getElementById("personalModal");
  const closeModalBtn = document.getElementById("closeModalBtn");
  const cancelModalBtn = document.getElementById("cancelModalBtn");
  const confirmSelectionBtn = document.getElementById("confirmSelectionBtn");
  const changePersonalBtn = document.getElementById("changePersonalBtn");
  const searchInput = document.getElementById("searchPersonalInput");

  // Evento: Cuando se selecciona un rol
  roleSelect.addEventListener("change", function () {
    const roleId = this.value;
    const roleName = this.options[this.selectedIndex].text;

    if (roleId) {
      openModalAndLoadPersonal(roleId, roleName);
    } else {
      clearSelectedPersonal();
    }
  });

  // Cerrar modal
  closeModalBtn.addEventListener("click", closeModal);
  cancelModalBtn.addEventListener("click", closeModal);

  // Cerrar modal al hacer click fuera
  modal.addEventListener("click", function (e) {
    if (e.target === modal) {
      closeModal();
    }
  });

  // Confirmar selección
  confirmSelectionBtn.addEventListener("click", confirmPersonalSelection);

  // Cambiar personal seleccionado
  changePersonalBtn.addEventListener("click", function () {
    const roleSelect = document.getElementById("roleSelect");
    const roleId = roleSelect.value;
    const roleName = roleSelect.options[roleSelect.selectedIndex].text;
    openModalAndLoadPersonal(roleId, roleName);
  });

  // Búsqueda en tiempo real
  searchInput.addEventListener("input", function () {
    filterPersonalList(this.value);
  });
}

function openModalAndLoadPersonal(roleId, roleName) {
  const modal = document.getElementById("personalModal");
  const container = document.getElementById("personalListContainer");

  modal.classList.add("active");

  // Mostrar loading
  container.innerHTML = `
        <div class="loading-spinner">
            <i class="fas fa-spinner fa-spin"></i>
            <p>Cargando personal de ${roleName}...</p>
        </div>
    `;

  // Limpiar búsqueda
  document.getElementById("searchPersonalInput").value = "";

  // Cargar personal desde el servidor
  loadPersonalByRole(roleId, roleName);
}

async function loadPersonalByRole(roleId, roleName) {
  try {
    const response = await fetch(
      `/Identity/Account/Register?handler=PersonalByRole&roleId=${roleId}`
    );

    if (!response.ok) {
      throw new Error("Error al cargar el personal");
    }

    const data = await response.json();
    personalData = data;

    displayPersonalList(data);
  } catch (error) {
    console.error("Error:", error);
    showErrorInModal(
      "Error al cargar el personal. Por favor, intente nuevamente."
    );
  }
}

function displayPersonalList(data) {
  const container = document.getElementById("personalListContainer");

  if (data.length === 0) {
    container.innerHTML = `
            <div class="no-results">
                <i class="fas fa-user-slash"></i>
                <h3>No se encontró personal</h3>
                <p>No hay personal disponible para este rol.</p>
            </div>
        `;
    return;
  }

  let html = "";
  data.forEach((person) => {
    const isSelected = selectedPersonal && selectedPersonal.id === person.id;

    html += `
            <div class="personal-item ${
              isSelected ? "selected" : ""
            }" data-id="${person.id}">
                <div class="personal-item-avatar">
                    <i class="fas ${getIconByRole(
                      person.cargo || person.cargoMedico
                    )}"></i>
                </div>
                <div class="personal-item-info">
                    <div class="personal-item-name">${person.nombre}</div>
                    <div class="personal-item-details">
                        <span><i class="fas fa-id-card"></i> DNI: ${
                          person.numeroDni
                        }</span>
                        <span><i class="fas fa-briefcase"></i> ${
                          person.cargo || person.cargoMedico
                        }</span>
                        ${
                          person.areaServicio
                            ? `<span><i class="fas fa-hospital"></i> ${person.areaServicio}</span>`
                            : ""
                        }
                        ${
                          person.numeroColegiatura
                            ? `<span><i class="fas fa-certificate"></i> Col: ${person.numeroColegiatura}</span>`
                            : ""
                        }
                        ${
                          person.numeroColegiaturaEnfermeria
                            ? `<span><i class="fas fa-certificate"></i> Col: ${person.numeroColegiaturaEnfermeria}</span>`
                            : ""
                        }
                    </div>
                </div>
                <div class="personal-item-badge">${
                  person.cargo || person.cargoMedico
                }</div>
            </div>
        `;
  });

  container.innerHTML = html;

  // ✅ MEJOR: Agregar event listeners después de crear el HTML
  document.querySelectorAll(".personal-item").forEach((item) => {
    item.addEventListener("click", function () {
      const personId = parseInt(this.getAttribute("data-id"));
      selectPersonal(personId);
    });
  });
}

// ✅ CORREGIDO: Ahora actualiza visualmente la selección
function selectPersonal(personId) {
  console.log("selectPersonal llamado con ID:", personId); // Debug
  console.log("Datos de personal disponibles:", personalData); // Debug
  const person = personalData.find((p) => p.id == personId);
  if (!person) {
    console.log("No se encontró persona con ID:", personId);
    return;
  }

  selectedPersonal = person;
  console.log("Personal seleccionado:", selectedPersonal); // Debug

  // Remover clase 'selected' de todos los items
  document.querySelectorAll(".personal-item").forEach((item) => {
    item.classList.remove("selected");
  });

  // Agregar clase 'selected' al item clickeado
  const selectedItem = document.querySelector(
    `.personal-item[data-id="${personId}"]`
  );
  if (selectedItem) {
    selectedItem.classList.add("selected");
  }

  // Habilitar botón de confirmar
  const confirmBtn = document.getElementById("confirmSelectionBtn");
  confirmBtn.disabled = false;
  console.log("Botón confirmación habilitado"); // Debug
}

// ✅ Exponer función al scope global para que funcione el onclick
window.selectPersonal = selectPersonal;

// ✅ CORREGIDO: Nombre de variable correcto
function confirmPersonalSelection() {
  if (!selectedPersonal) {
    alert("Por favor, seleccione un personal.");
    return;
  }

  // ✅ Guarda el ID en el input oculto
  document.getElementById("personalIdInput").value = selectedPersonal.id;

  // ✅ Muestra el nombre en la pantalla
  document.getElementById("selectedPersonalName").textContent =
    selectedPersonal.nombre;
  document.getElementById("selectedPersonalCargo").textContent =
    selectedPersonal.cargo || selectedPersonal.cargoMedico;
  document.getElementById(
    "selectedPersonalDni"
  ).textContent = `DNI: ${selectedPersonal.numeroDni}`;

  // ✅ CORRECCIÓN: Variable correcta 'selectedPersonalArea'
  const selectedArea = document.getElementById("selectedPersonalArea");
  selectedArea.style.display = "block";

  closeModal();
}

function clearSelectedPersonal() {
  selectedPersonal = null;
  document.getElementById("personalIdInput").value = "";
  document.getElementById("selectedPersonalArea").style.display = "none";
}

function closeModal() {
  const modal = document.getElementById("personalModal");
  modal.classList.remove("active");

  // Resetear botón de confirmar
  document.getElementById("confirmSelectionBtn").disabled = true;
}

function filterPersonalList(searchTerm) {
  if (!searchTerm) {
    displayPersonalList(personalData);
    return;
  }

  const filtered = personalData.filter((person) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      person.nombre.toLowerCase().includes(searchLower) ||
      person.numeroDni.includes(searchTerm) ||
      (person.cargo && person.cargo.toLowerCase().includes(searchLower)) ||
      (person.cargoMedico &&
        person.cargoMedico.toLowerCase().includes(searchLower))
    );
  });

  displayPersonalList(filtered);
}

function getIconByRole(cargo) {
  const icons = {
    MEDICO: "fa-user-md",
    ENFERMERIA: "fa-user-nurse",
    CAJERO: "fa-cash-register",
    ADMINISTRADOR: "fa-user-shield",
  };

  const cargoUpper = (cargo || "").toUpperCase();

  for (let key in icons) {
    if (cargoUpper.includes(key)) {
      return icons[key];
    }
  }

  return "fa-user";
}

function showErrorInModal(message) {
  const container = document.getElementById("personalListContainer");
  container.innerHTML = `
        <div class="no-results">
            <i class="fas fa-exclamation-triangle" style="color: var(--danger-color);"></i>
            <h3>Error</h3>
            <p>${message}</p>
        </div>
    `;
}

// Función para mostrar/ocultar contraseña
function togglePasswordVisibility(inputId, iconId) {
  const input = document.getElementById(inputId);
  const icon = document.getElementById(iconId);

  if (input.type === "password") {
    input.type = "text";
    icon.classList.remove("fa-eye");
    icon.classList.add("fa-eye-slash");
  } else {
    input.type = "password";
    icon.classList.remove("fa-eye-slash");
    icon.classList.add("fa-eye");
  }
}

// Validación antes de enviar
document.addEventListener("DOMContentLoaded", function () {
  const form = document.getElementById("registerForm");

  if (form) {
    form.addEventListener("submit", function (e) {
      const personalId = document.getElementById("personalIdInput").value;
      const roleId = document.getElementById("roleSelect").value;

      if (roleId && !personalId) {
        e.preventDefault();
        alert("Por favor, seleccione un personal antes de registrar.");
        return false;
      }
    });
  }
});
