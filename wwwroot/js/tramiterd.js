/* TrámiteRD — interacciones mínimas (HTML/CSS/JS puro) */
(function () {
  "use strict";

  // Mostrar / ocultar contraseña
  document.querySelectorAll("[data-trd-toggle]").forEach(function (btn) {
    btn.addEventListener("click", function () {
      var input = btn.closest(".trd-input-wrap").querySelector("[data-trd-password]");
      if (!input) return;
      var oculto = input.type === "password";
      input.type = oculto ? "text" : "password";
      btn.textContent = oculto ? "Ocultar" : "Mostrar";
    });
  });

  // Oculta los avisos de validación que vienen vacíos (sin errores)
  document.querySelectorAll(".trd-alert--err").forEach(function (el) {
    if (el.textContent.trim() === "") el.style.display = "none";
  });
})();
