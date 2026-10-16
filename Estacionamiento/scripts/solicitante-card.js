(function ($) {
  var debounceTimer = null;

  function mostrarCard(usuario) {
    if (!usuario) {
      $("#solicitanteNombre").text("No encontrado");
      $("#solicitantePuesto").html("<strong>Puesto:</strong> -");
      $("#solicitanteJefe").html("<strong>Jefe Directo:</strong> -");
      $("#solicitanteUEN").html("<strong>UEN:</strong> -");
      $("#solicitanteNumero").html("<strong>No. Empleado:</strong> -");
      $("#solicitanteCorreo").html("<strong>Correo:</strong> -");
      $("#solicitanteCard").show();
      // hide spinner when we have a displayed result
      $("#solicitanteSpinner").hide();
      return;
    }
    $("#solicitanteNombre").text(usuario.sNombre || "-");
    $("#solicitantePuesto").html(
      "<strong>Puesto:</strong> " + (usuario.Puesto || "-")
    );
    $("#solicitanteJefe").html(
      "<strong>Jefe Directo:</strong> " + (usuario.JefeDirecto || "-")
    );
    $("#solicitanteUEN").html("<strong>UEN:</strong> " + (usuario.UEN || "-"));
    $("#solicitanteNumero").html(
      "<strong>No. Empleado:</strong> " + (usuario.uiNumeroEmpleado || "-")
    );
    $("#solicitanteCorreo").html(
      "<strong>Correo:</strong> " + (usuario.sCorreo || "-")
    );
    $("#solicitanteCard").show();
  }

  function cargarSolicitante(numero) {
    if (!numero) {
      mostrarCard(null);
      return;
    }
    // show spinner while loading
    $("#solicitanteSpinner").show();
    $.ajax({
      url: "/Estacionamiento/BuscarUserAD_AJAX",
      type: "GET",
      data: { parametroBusqueda: numero },
      dataType: "json",
      cache: false,
      success: function (r) {
        if (r && !r.error && r.Usuario) {
          mostrarCard(r.Usuario);
        } else {
          mostrarCard(null);
        }
      },
      error: function () {
        // hide spinner and show not-found state
        $("#solicitanteSpinner").hide();
        mostrarCard(null);
      },
    });
  }

  $(function () {
    var $input = $("#noEmpleadoSolicita");
    if (!$input.length) $input = $("input[name='noEmpleadoSolicita']");

    $input.on("input", function () {
      clearTimeout(debounceTimer);
      var val = $(this).val();
      // show searching placeholder immediately
      if (val && val.length > 0) {
        $("#solicitanteNombre").text("Buscando...");
        $("#solicitantePuesto").html("<strong>Puesto:</strong> Buscando...");
        $("#solicitanteJefe").html(
          "<strong>Jefe Directo:</strong> Buscando..."
        );
        $("#solicitanteUEN").html("<strong>UEN:</strong> Buscando...");
        $("#solicitanteNumero").html("<strong>No. Empleado:</strong> " + val);
        $("#solicitanteCorreo").html("<strong>Correo:</strong> Buscando...");
        $("#solicitanteCard").show();
      }
      debounceTimer = setTimeout(function () {
        cargarSolicitante(val);
      }, 400);
    });

    $input.on("blur", function () {
      var val = $(this).val();
      cargarSolicitante(val);
    });

    $(document).on("click", "#btnSolicitanteLimpiar", function () {
      $input.val("");
      mostrarCard(null);
      $input.focus();
    });
  });
})(jQuery);
