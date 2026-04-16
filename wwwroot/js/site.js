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
    logoutBtn.addEventListener("click", function () {
      if (confirm("¿Está seguro que desea cerrar sesión?")) {
        window.location.href = "/Account/Logout";
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
