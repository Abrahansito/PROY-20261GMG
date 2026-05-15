// Sidebar Toggle Functionality
document.addEventListener("DOMContentLoaded", function () {
  const hamburgerBtn = document.getElementById("hamburgerBtn");
  const closeSidebarBtn = document.getElementById("closeSidebarBtn");
  const sidebar = document.getElementById("sidebar");
  const mainContent = document.getElementById("mainContent");
  const logoutBtn = document.getElementById("logoutBtn");

  // Initialize sidebar state based on screen size
  function initializeSidebar() {
    if (window.innerWidth <= 768) {
      sidebar.classList.add("hidden");
      sidebar.classList.remove("visible");
    } else {
      sidebar.classList.remove("hidden");
      sidebar.classList.add("visible");
    }
  }

  // Toggle sidebar
  function toggleSidebar() {
    if (window.innerWidth <= 768) {
      sidebar.classList.toggle("visible");
      sidebar.classList.toggle("hidden");
    } else {
      sidebar.classList.toggle("hidden");
      mainContent.classList.toggle("expanded");
    }
  }

  // Hamburger button click
  if (hamburgerBtn) {
    hamburgerBtn.addEventListener("click", toggleSidebar);
  }

  // Close sidebar button click
  if (closeSidebarBtn) {
    closeSidebarBtn.addEventListener("click", toggleSidebar);
  }

  // Logout button click
  if (logoutBtn) {
    logoutBtn.addEventListener("click", async function (event) {
      event.preventDefault();
      const confirmado = await window.sigmegConfirm("¿Está seguro que desea cerrar sesión?");
      if (confirmado) {
        const logoutForm = logoutBtn.closest("form");
        if (logoutForm) {
          logoutForm.submit();
        } else {
          window.location.href = "/Identity/Account/Login";
        }
      }
    });
  }

  // Close sidebar when clicking outside on mobile
  document.addEventListener("click", function (event) {
    if (window.innerWidth <= 768) {
      const isClickInsideSidebar = sidebar.contains(event.target);
      const isClickOnHamburger = hamburgerBtn.contains(event.target);

      if (
        !isClickInsideSidebar &&
        !isClickOnHamburger &&
        sidebar.classList.contains("visible")
      ) {
        sidebar.classList.remove("visible");
        sidebar.classList.add("hidden");
      }
    }
  });

  // Handle window resize
  window.addEventListener("resize", function () {
    initializeSidebar();
  });

  // Initialize on load
  initializeSidebar();
});

// Alert Functions - MANTENER COMPATIBILIDAD
function showAlert(message, type = "success") {
  if (window.alertManager) {
    if (type === "success") {
      window.alertManager.success(message);
    } else if (type === "error") {
      window.alertManager.error(message);
    } else if (type === "warning") {
      window.alertManager.warning(message);
    } else {
      window.alertManager.info(message);
    }
    return;
  }

  // Fallback si alertManager no está disponible
  const alertContainer = document.getElementById("alertContainer");
  if (!alertContainer) return;

  const alert = document.createElement("div");
  alert.className = `alert alert-${type}`;

  const icon =
    type === "success"
      ? '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><polyline points="20 6 9 17 4 12"></polyline></svg>'
      : '<svg xmlns="http://www.w3.org/2000/svg" width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>';

  alert.innerHTML = `
    ${icon}
    <span>${message}</span>
    <button class="alert-close" onclick="this.parentElement.remove()">×</button>
  `;

  alertContainer.appendChild(alert);

  setTimeout(() => {
    alert.remove();
  }, 5000);
}

window.showAlert = showAlert;

// SIGMEG modal notifications
(function () {
  const modalId = "sigmegMessageModal";
  let activeResolver = null;

  function ensureModal() {
    let modal = document.getElementById(modalId);
    if (modal) return modal;

    const style = document.createElement("style");
    style.textContent = `
      .sigmeg-modal-backdrop{position:fixed;inset:0;z-index:3000;display:none;align-items:center;justify-content:center;padding:24px;background:rgba(15,23,42,.58);backdrop-filter:blur(2px)}
      .sigmeg-modal-backdrop.show{display:flex}
      .sigmeg-modal-card{width:min(540px,100%);overflow:hidden;border-radius:14px;background:#fff;box-shadow:0 24px 70px rgba(15,23,42,.28);border:1px solid rgba(148,163,184,.32);font-family:inherit}
      .sigmeg-modal-header{display:flex;align-items:center;gap:14px;padding:22px 28px;color:#fff;background:#24435d}
      .sigmeg-modal-header.success{background:#16a34a}.sigmeg-modal-header.error{background:#dc2626}.sigmeg-modal-header.warning{background:#d97706}.sigmeg-modal-header.info{background:#2563eb}
      .sigmeg-modal-icon{display:inline-flex;align-items:center;justify-content:center;width:42px;height:42px;flex:0 0 42px;border-radius:999px;background:rgba(255,255,255,.22);font-size:24px;font-weight:700}
      .sigmeg-modal-title{margin:0;font-size:22px;font-weight:700;letter-spacing:0}
      .sigmeg-modal-body{padding:26px 30px 10px;color:#334155;font-size:17px;line-height:1.5;white-space:pre-wrap}
      .sigmeg-modal-actions{display:flex;justify-content:flex-end;gap:12px;padding:20px 30px 28px}
      .sigmeg-modal-btn{min-width:116px;border:0;border-radius:8px;padding:12px 20px;font-weight:700;cursor:pointer;transition:transform .12s ease,box-shadow .12s ease,background .12s ease}
      .sigmeg-modal-btn:hover{transform:translateY(-1px);box-shadow:0 10px 22px rgba(15,23,42,.16)}
      .sigmeg-modal-btn.primary{color:#fff;background:#2563eb}.sigmeg-modal-btn.primary.success{background:#16a34a}.sigmeg-modal-btn.primary.error{background:#dc2626}.sigmeg-modal-btn.primary.warning{background:#d97706}
      .sigmeg-modal-btn.secondary{color:#334155;background:#e2e8f0}
    `;
    document.head.appendChild(style);

    modal = document.createElement("div");
    modal.id = modalId;
    modal.className = "sigmeg-modal-backdrop";
    modal.innerHTML = `
      <div class="sigmeg-modal-card" role="dialog" aria-modal="true" aria-labelledby="sigmegModalTitle">
        <div class="sigmeg-modal-header info" id="sigmegModalHeader">
          <span class="sigmeg-modal-icon" id="sigmegModalIcon">i</span>
          <h2 class="sigmeg-modal-title" id="sigmegModalTitle">Aviso</h2>
        </div>
        <div class="sigmeg-modal-body" id="sigmegModalMessage"></div>
        <div class="sigmeg-modal-actions">
          <button type="button" class="sigmeg-modal-btn secondary" id="sigmegModalCancel">Cancelar</button>
          <button type="button" class="sigmeg-modal-btn primary info" id="sigmegModalAccept">Aceptar</button>
        </div>
      </div>`;
    document.body.appendChild(modal);

    modal.querySelector("#sigmegModalAccept").addEventListener("click", () => closeModal(true));
    modal.querySelector("#sigmegModalCancel").addEventListener("click", () => closeModal(false));
    modal.addEventListener("click", (event) => {
      if (event.target === modal) closeModal(false);
    });
    document.addEventListener("keydown", (event) => {
      if (event.key === "Escape" && modal.classList.contains("show")) closeModal(false);
    });
    return modal;
  }

  function closeModal(value) {
    const modal = document.getElementById(modalId);
    if (modal) modal.classList.remove("show");
    if (activeResolver) {
      activeResolver(value);
      activeResolver = null;
    }
  }

  function inferType(message) {
    const text = String(message || "").toLowerCase();
    if (text.includes("correctamente") || text.includes("exitosamente") || text.includes("guardado") || text.includes("registrado")) return "success";
    if (text.includes("error") || text.includes("no se pudo") || text.includes("no se encontraron")) return "error";
    if (text.includes("por favor") || text.includes("seguro") || text.includes("obligatorio")) return "warning";
    return "info";
  }

  function getDefaults(type, isConfirm) {
    const defaults = {
      success: { title: "Operación completada", icon: "✓" },
      error: { title: "No se pudo completar", icon: "!" },
      warning: { title: isConfirm ? "Confirmar acción" : "Atención", icon: "!" },
      info: { title: "Aviso", icon: "i" },
    };
    return defaults[type] || defaults.info;
  }

  function openModal(message, options = {}) {
    const type = options.type || inferType(message);
    const isConfirm = Boolean(options.confirm);
    const defaults = getDefaults(type, isConfirm);
    const modal = ensureModal();
    const header = modal.querySelector("#sigmegModalHeader");
    const icon = modal.querySelector("#sigmegModalIcon");
    const title = modal.querySelector("#sigmegModalTitle");
    const body = modal.querySelector("#sigmegModalMessage");
    const accept = modal.querySelector("#sigmegModalAccept");
    const cancel = modal.querySelector("#sigmegModalCancel");

    header.className = `sigmeg-modal-header ${type}`;
    accept.className = `sigmeg-modal-btn primary ${type}`;
    icon.textContent = options.icon || defaults.icon;
    title.textContent = options.title || defaults.title;
    body.textContent = message || "";
    accept.textContent = options.acceptText || "Aceptar";
    cancel.textContent = options.cancelText || "Cancelar";
    cancel.style.display = isConfirm ? "inline-flex" : "none";
    modal.classList.add("show");
    accept.focus();

    return new Promise((resolve) => {
      activeResolver = resolve;
    });
  }

  window.sigmegAlert = function (message, type = null, title = null) {
    return openModal(message, { type: type || inferType(message), title });
  };

  window.sigmegConfirm = function (message, options = {}) {
    return openModal(message, { ...options, confirm: true, type: options.type || "warning", title: options.title || "Confirmar acción" });
  };

  window.alert = function (message) {
    window.sigmegAlert(message);
  };
})();
