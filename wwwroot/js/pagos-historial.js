// ...existing code...
(function () {
  // Selector del formulario y tabla
  const form = document.getElementById("form-filtros");
  const tabla = document.getElementById("tabla-pagos");
  const tbody = tabla.querySelector("tbody");

  function buildRow(pago, index) {
    const tr = document.createElement("tr");
    tr.innerHTML = `
            <td>${index}</td>
            <td>${pago.id}</td>
            <td>${pago.fechaPago || "-"}</td>
            <td>${pago.fechaCita || "-"}</td>
            <td>${pago.horaCita || "-"}</td>
            <td>${
              pago.tipoDoc && pago.numeroDoc
                ? pago.tipoDoc + " - " + pago.numeroDoc
                : "-"
            }</td>
            <td>${pago.nombres || "-"}</td>
            <td>${
              pago.total != null ? parseFloat(pago.total).toFixed(2) : "0.00"
            }</td>
            <td>${pago.estado || "-"}</td>
            <td>${
              pago.estado === "Pendiente"
                ? `<a href="/pagos/resumen/${encodeURIComponent(
                    pago.id
                  )}" class="btn btn-primary btn-sm">Pagar cita</a>`
                : `<a href="/pagos/resumen/${encodeURIComponent(
                    pago.id
                  )}" class="btn btn-info btn-sm">Detalles</a>`
            }</td>
        `;
    return tr;
  }

  async function fetchPagos(params) {
    // Ruta ajustada a /pagos/search (coincide con el controlador)
    const url = new URL(window.location.origin + "/pagos/search");
    Object.keys(params).forEach((k) => {
      if (params[k] !== undefined && params[k] !== null && params[k] !== "")
        url.searchParams.append(k, params[k]);
    });
    const res = await fetch(url.toString(), {
      headers: { Accept: "application/json" },
    });
    if (!res.ok) throw new Error("Error fetching pagos");
    const json = await res.json();
    // El endpoint devuelve GenericResponse { success, message, data }
    if (!json || json.success === false) {
      throw new Error(json?.message || "Error en respuesta del servidor");
    }
    return json.data || [];
  }

  function clearTable() {
    tbody.innerHTML = "";
  }

  function renderNoRecords() {
    clearTable();
    const tr = document.createElement("tr");
    tr.innerHTML = '<td colspan="10" class="text-center">No hay registros</td>';
    tbody.appendChild(tr);
  }

  // Manejar submit para realizar búsqueda sin recargar (o con recarga si quieres mantener URL)
  form.addEventListener("submit", async function (e) {
    e.preventDefault();
    const formData = new FormData(form);
    const params = Object.fromEntries(formData.entries());
    try {
      const data = await fetchPagos(params);
      clearTable();
      if (!data || data.length === 0) {
        renderNoRecords();
        return;
      }
      let idx = 1;
      for (const pago of data) {
        tbody.appendChild(buildRow(pago, idx));
        idx++;
      }
    } catch (err) {
      console.error(err);
      renderNoRecords();
    }
  });

  // Manejar botón Limpiar: resetear formulario, limpiar tabla y quitar querystring
  const btnLimpiar = document.getElementById("btn-limpiar");
  if (btnLimpiar) {
    btnLimpiar.addEventListener("click", function () {
      // Reset visual breve
      form.reset();
      // Limpiar tabla inmediatamente para mejor UX
      renderNoRecords();
      // Redirigir al endpoint servidor que muestra la vista sin filtros
      // Usamos la ruta absoluta definida en el controlador: /pagos/all
      const clearUrl = window.location.origin + "/pagos/all";
      window.location.href = clearUrl;
    });
  }

  // Cargar datos iniciales (si hay querystring en la URL)
  document.addEventListener("DOMContentLoaded", function () {
    // Si el formulario tiene valores (o la URL tiene query), disparar la búsqueda para llenar la tabla
    const formData = new FormData(form);
    const hasFilters = Array.from(formData.entries()).some(
      ([k, v]) => v && v.toString().trim() !== ""
    );
    if (hasFilters) {
      form.dispatchEvent(new Event("submit"));
    }
  });
})();
// ...existing code...
