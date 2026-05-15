(function () {
  "use strict";

  function getMessage(payload, fallback) {
    if (!payload) return fallback;
    if (typeof payload === "string") return payload;
    return payload.message || payload.mensaje || fallback;
  }

  function notify(message, type) {
    if (!message) return;

    if (typeof window.showAlert === "function") {
      window.showAlert(message, type || "error");
      return;
    }

    console.error(message);
  }

  function handleAjaxError(event, xhr) {
    if (!xhr || xhr.status === 0) return;

    var response = xhr.responseJSON;

    if (!response && xhr.responseText) {
      try {
        response = JSON.parse(xhr.responseText);
      } catch (error) {
        response = null;
      }
    }

    notify(getMessage(response, "Ocurrio un error al procesar la solicitud."), "error");
  }

  window.addEventListener("error", function (event) {
    if (!event || !event.message) return;
    console.error("Error de JavaScript:", event.message);
  });

  window.addEventListener("unhandledrejection", function (event) {
    var reason = event && event.reason;
    var message = reason && reason.message ? reason.message : "Promesa rechazada sin manejar.";
    console.error(message);
  });

  if (window.jQuery) {
    window.jQuery(document).ajaxError(handleAjaxError);
  }
})();
