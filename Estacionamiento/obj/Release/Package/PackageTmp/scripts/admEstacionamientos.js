function MostrarEstacionamientos(lista) {
  var filas = "";
  $.each(lista || [], function (i, estaciona) {
    filas +=
      '<tr style="border-bottom: 1px solid #E5E7EB"><td><p>' +
      estaciona.Lugares.sLugar +
      "</p></td><td><p>" +
      estaciona.LugarDireccion.sLugar +
      "</p></td><td><p>" +
      estaciona.Usuarios.sNombre +
      "</p></td><td><p>" +
      estaciona.Usuarios.sCorreo +
      "</p></td><td><p>" +
      estaciona.dtFechaAsignacionJSON +
      "</p></td><td><p>" +
      estaciona.sObservaciones +
      '</p></td><td><button type="button" class="btn btn-action btn-delete" id="btnLiberarCajon" onclick="ObtenerCajon(' +
      estaciona.uiRegistroCajones +
      ')" title="Liberar cajón">Liberar Cajón <span class="fas fa-lock-open"></span></button></td></tr>';
  });
  if (!lista || lista.length === 0) {
    filas =
      '<tr class="empty-row"><td colspan="7"><div class="empty-state"><h3 style="margin:0; font-size:16px; font-weight:600; color:#6b7280;">No se han encontrado datos</h3><p style="margin:4px 0 0; color:#9ca3af; font-size:13px;">Intenta ajustar la búsqueda o selecciona otra dirección.</p></div></td></tr>';
  }
  $("#tbodyUsuariosEstacionamiento").empty().html(filas);
      (estaciona.dtFechaAsignacionJSON || "-") +
      "</p></td><td><p>" +
      estaciona.sObservaciones +
      '</p></td><td><button type="button" class="btn btn-action btn-delete" id="btnLiberar" onclick="ObtenerCajon(' +
      estaciona.uiRegistroCajones +
      ')" title="Liberar cajón">Liberar <span class="fas fa-lock-open"></span></button></td></tr>';
      $inpU.focus();
      var vU = $inpU.val() || "";
      var elU = $inpU.get(0);
      if (elU && elU.setSelectionRange)
        elU.setSelectionRange(vU.length, vU.length);
    }
  } catch (e) {}
}

function MostrarEstacionamientosVisitas(lista) {
  var filas = "";
  $.each(lista || [], function (i, estaciona) {
    filas +=
      '<tr style="border-bottom: 1px solid #E5E7EB"><td><p>' +
      estaciona.Lugares.sLugar +
      "</p></td><td><p>" +
      estaciona.LugarDireccion.sLugar +
      '</p><td><button type="button" class="btn btn-action btn-edit" id="btnEditarCajon" onclick="EditarCajon(' +
      estaciona.uiRegistroCajones +
      ')" title="Editar cajón">Editar <span class="fas fa-car-side"></span></button></td></td><td><p>' +
      estaciona.Usuarios.sNombre +
      "</p></td><td><p>" +
      estaciona.Usuarios.nombreResponsable +
      "</p></td><td><p>" +
      estaciona.horaVisita +
      "</p></td><td><p>" +
      estaciona.horaSalida +
      '</p></td><td><button type="button" class="btn btn-action btn-edit" id="btnEditarHora" onclick="EditarHoraSalida(' +
      estaciona.uiRegistroCajones +
      ')" title="Editar hora de salida">Editar <span class="far fa-edit"></span></button></td><td><p>' +
      estaciona.sPlaca +
      "</p></td><td><p>" +
      estaciona.sModelo +
      "</p></td><td><p>" +
      estaciona.sColor +
      "</p></td><td><p>" +
      estaciona.dtFechaVisitaJSON +
      "</p></td><td><p>" +
      estaciona.sObservaciones +
      '</p></td><td><button type="button" class="btn btn-action btn-delete" id="btnLiberar" onclick="ObtenerCajon(' +
      estaciona.uiRegistroCajones +
      ')" title="Liberar cajón">Liberar Cajón <span class="fas fa-lock-open"></span></button></td></tr>';
  });
  if (!lista || lista.length === 0) {
    filas =
      '<tr class="empty-row"><td colspan="14"><div class="empty-state"><h3 style="margin:0; font-size:16px; font-weight:600; color:#6b7280;">No se han encontrado datos</h3><p style="margin:4px 0 0; color:#9ca3af; font-size:13px;">Aún no hay visitas registradas o los filtros no coinciden.</p></div></td></tr>';
  }
  $("#tbodyVisitantesEstacionamiento").empty().html(filas);
  // Restaurar foco al campo de texto de filtros de Visitantes para permitir continuar tipeando
  try {
    var $inpV = $("#filtroTextoVisitantes");
    if ($inpV && $inpV.length && $inpV.is(":visible")) {
      $inpV.focus();
      var vV = $inpV.val() || "";
      var elV = $inpV.get(0);
      if (elV && elV.setSelectionRange)
        elV.setSelectionRange(vV.length, vV.length);
    }
  } catch (e) {}
}

function MostrarEstacionamientosVisitasAsigna(lista) {
  var filas = "";
  $.each(lista || [], function (i, estaciona) {
    filas +=
      '<tr style="border-bottom: 1px solid #E5E7EB"><td><p>' +
      estaciona.Lugares.sLugar +
      "</p></td><td><p>" +
      estaciona.LugarDireccion.sLugar +
      "</p></td><td><p>" +
      estaciona.Usuarios.sNombre +
      "</p></td><td><p>" +
      estaciona.Usuarios.nombreResponsable +
      "</p></td><td><p>" +
      estaciona.horaVisita +
      "</p></td><td><p>" +
      estaciona.horaSalida +
      "</p></td><td><p>" +
      estaciona.sPlaca +
      "</p></td><td><p>" +
      estaciona.sModelo +
      "</p></td><td><p>" +
      estaciona.sColor +
      "</p></td><td><p>" +
      estaciona.dtFechaVisitaJSON +
      "</p></td><td><p>" +
      estaciona.sObservaciones +
      '</p></td><td><button type="button" class="btn btn-action btn-assign" id="btnAsignarLugar" onclick="AsignarCajones(' +
      estaciona.uiRegistroCajones +
      ')" title="Asignar lugar">Asignar Lugar <span class="far fa-arrow-alt-circle-up"></span></button></td><td><button type="button" class="btn btn-action btn-delete" id="btnLiberar" onclick="EliminarSolicitud(' +
      estaciona.uiRegistroCajones +
      ')" title="Eliminar solicitud">Eliminar Solicitud <span class="far fa-window-close"></span></button></td></tr>';
  });
  if (!lista || lista.length === 0) {
    filas =
      '<tr class="empty-row"><td colspan="13"><div class="empty-state"><h3 style="margin:0; font-size:16px; font-weight:600; color:#6b7280;">No hay solicitudes por asignar</h3><p style="margin:4px 0 0; color:#9ca3af; font-size:13px;">Cuando existan solicitudes pendientes aparecerán aquí.</p></div></td></tr>';
  }
  $("#tbodyVisitantesEstacionamientoAsigna").empty().html(filas);

  // Restaurar foco al campo de texto de filtros de Visitantes Por Asignar (si está visible)
  try {
    var $inp = $("#filtroTextoVisitantesAsigna");
    if ($inp && $inp.length && $inp.is(":visible")) {
      $inp.focus();
      var v = $inp.val() || "";
      var el = $inp.get(0);
      if (el && el.setSelectionRange) el.setSelectionRange(v.length, v.length);
    }
  } catch (e) {}
}

function Limpiar() {
  $(
    "#numEmpleado, #nombreUser, #correoUser, #listCatalogoSeleccion, #lugar, #observacionesEsta, #txtParametroUser, #noEmpleadoSolicita, #nombreEmpleadoVisita, #correoEmpleadoVisita, #nombreVisita, #fechaVisita, #horaEntrada, #horaSalida, #observacionesEstaVisita, #lugarModal"
  ).val("");
}

function LugaresPorAsignar() {
  $.ajax({
    url: "/Estacionamiento/MostrarAsignacionesEstacionamientosVisitantesSinLugar_AJAX",
    type: "GET",
    contentType: "application/json;charset=utf-8",
    dataType: "JSON",
    cache: false,
    success: function (r) {
      if (r.error) {
        alert(r.mensaje);
      } else {
        MostrarEstacionamientosVisitasAsigna(r.listaEstacionamientos);
        $("#divTablaVisitantesAsigna").show();
        $("#filtrosVisitantesAsigna").show();
        $("#filtrosUsuarios, #filtrosVisitantes").hide();
      }
    },
    error: function (error) {
      alert(error.responseText);
    },
  });
}

function LugaresPorAsignarFiltrado() {
  var texto = $("#filtroTextoVisitantesAsigna").val() || "";
  var desde = $("#filtroDesdeVisitantesAsigna").val() || "";
  var hasta = $("#filtroHastaVisitantesAsigna").val() || "";
  if ((desde && !hasta) || (!desde && hasta)) {
    alertify.error(
      "Para filtrar por fecha, captura el rango completo (Desde y Hasta)."
    );
    return;
  }
  $.ajax({
    url: "/Estacionamiento/MostrarAsignacionesEstacionamientosVisitantesSinLugarFiltrado_AJAX",
    type: "GET",
    data: { texto: texto, fechaDesde: desde, fechaHasta: hasta },
    contentType: "application/json;charset=utf-8",
    dataType: "JSON",
    cache: false,
    success: function (r) {
      if (r.error) {
        alert(r.mensaje);
      } else {
        MostrarEstacionamientosVisitasAsigna(r.listaEstacionamientos);
        $("#divTablaVisitantesAsigna").show();
      }
    },
    error: function (e) {
      console.error(e);
    },
  });
}

function LugaresAsignadosVisitantes() {
  $.ajax({
    url: "/Estacionamiento/MostrarAsignacionesEstacionamientosVisitantes_AJAX",
    type: "GET",
    contentType: "application/json;charset=utf-8",
    dataType: "JSON",
    cache: false,
    success: function (r) {
      if (r.error) {
        var alertDlg = alertify
          .alert("Error", r.mensaje)
          .set("label", "Aceptar");
        alertDlg.set({ transition: "zoom" });
        alertDlg.set("modal", false);
      } else {
        MostrarEstacionamientosVisitas(r.listaEstacionamientos);
        $("#divTabla").hide();
        $("#divTablaVisitantes").show();
        $("#filtrosVisitantes").show();
        $("#filtrosUsuarios").hide();
        $("#btnSeleccionInfo").show();
        $("#btnVerInformacionUsuarios").show();
        $("#btnVerInformacionVisitantes").hide();
      }
    },
    error: function (error) {
      alert(error.responseText);
    },
  });
}

function MostrarInformacionEmpleados() {
  $.ajax({
    url: "/Estacionamiento/MostrarAsignacionesEstacionamientos_AJAX",
    type: "GET",
    contentType: "application/json;charset=utf-8",
    dataType: "JSON",
    cache: false,
    success: function (r) {
      if (r.error) {
        var alertDlg = alertify
          .alert("Error", r.mensaje)
          .set("label", "Aceptar");
        alertDlg.set({ transition: "zoom" });
        alertDlg.set("modal", false);
      } else {
        MostrarEstacionamientos(r.listaEstacionamientos);
        $("#divTabla").show();
        $("#divTablaVisitantes").hide();
        $("#filtrosUsuarios").show();
        $("#filtrosVisitantes").hide();
        $("#btnSeleccionInfo").show();
        $("#catalogoLugares").hide();
        $("#divUsuarios").hide();
      }
    },
    error: function (error) {
      alert(error.responseText);
    },
  });
}

$(document).ready(function () {
  MostrarInformacionEmpleados();

  $("#btnAgregarLugares").click(function () {
    $("#catalogoLugares").hide();
    $(
      "#tiposUser, #divUsuarios, #divTabla, #divUserAD, #datosVisitante, #btnGuardarLugarEmpleado, #btnSeleccionInfo, #divTablaVisitantes"
    ).hide();
  });

  $("#btnEmpleados").click(function () {
    $.ajax({
      url: "/Estacionamiento/MostrarCatalogoLugares_AJAX",
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
          return;
        }
        $("#listCatalogoSeleccion").val("").empty();
        $("#listCatalogo").empty();
        $("#catalogoLugares").show();
        $("#tiposUser").hide();
        $("#divUsuarios").show();
        $("#divTablaVisitantesAsigna").hide();
        $("#datosVisitante, #divTabla, #divTablaVisitantes").hide();
        $.each(r.listaCalagos, function (i, catalogo) {
          var option = $(document.createElement("option"));
          option.html(catalogo.sNombreCliente);
          option.val(catalogo.uiRegistroCatalogo + "-" + catalogo.sLugar);
          $("#listCatalogo").append(option);
        });
        Limpiar();
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#listCatalogoSeleccion").change(function () {
    var arreglo = $("#listCatalogoSeleccion").val();
    var datos = arreglo.split("-");
    var datoDireccion = datos[0];
    $.ajax({
      url:
        "/Estacionamiento/MostrarCatalogoLugaresPorDireccion_AJAX?direccion=" +
        datoDireccion,
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
          return;
        }
        $("#lugar").val("").empty();
        $("#listCatalogoLugares").empty();
        $.each(r.listaCalagos, function (i, catalogo) {
          var option = $(document.createElement("option"));
          option.html(catalogo.sLugar);
          option.val(catalogo.sLugar);
          $("#listCatalogoLugares").append(option);
        });
        $("#divLugares").show();
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#listCatalogoSeleccion").focus(function () {
    $(this).val("");
  });

  $("#listCatalogoSeleccionModal").change(function () {
    var arreglo = $("#listCatalogoSeleccionModal").val();
    var datos = arreglo.split("-");
    var datoDireccion = datos[0];
    $.ajax({
      url:
        "/Estacionamiento/MostrarCatalogoLugaresPorDireccionVisitantes_AJAX?direccion=" +
        datoDireccion,
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
          return;
        }
        $("#lugarModal").val("").empty();
        $("#listCatalogoLugaresModal").empty();
        $.each(r.listaCalagos, function (i, catalogo) {
          var option = $(document.createElement("option"));
          option.html(catalogo.sLugar);
          option.val(catalogo.sLugar);
          $("#listCatalogoLugaresModal").append(option);
        });
        $("#divLugaresModal").show();
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#listCatalogoSeleccionModal").focus(function () {
    $(this).val("");
  });

  $("#btnBuscarUser").click(function () {
    var parametroBusqueda = $("#txtParametroUser").val();
    if (!parametroBusqueda) {
      var alertDlg = alertify
        .alert("Error", "Debes ingresar un número de usuario para Búsqueda")
        .set("label", "Aceptar");
      alertDlg.set({ transition: "zoom" });
      alertDlg.set("modal", false);
      return;
    }
    $("#gifCargando").show();
    $("#btnBuscarUser").prop("disabled", true);
    $.ajax({
      url:
        "/Estacionamiento/BuscarUserAD_AJAX?parametroBusqueda=" +
        parametroBusqueda,
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          var alertDlg = alertify
            .alert("Error", r.mensaje)
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
          $("#txtParametroUser").val("");
          $("#btnBuscarUser").prop("disabled", false);
          $("#gifCargando").hide();
        } else {
          $("#divUserAD").show();
          $("#numEmpleado").val(r.Usuario.uiNumeroEmpleado);
          $("#nombreUser").val(r.Usuario.sNombre);
          $("#correoUser").val(r.Usuario.sCorreo);
          // Campos adicionales enriquecidos
          $("#puestoUser").val(r.Usuario.Puesto || "");
          $("#jefeUser").val(r.Usuario.JefeDirecto || "");
          $("#uenUser").val(r.Usuario.UEN || "");

          $(
            "#numEmpleado, #nombreUser, #correoUser, #puestoUser, #jefeUser, #uenUser"
          ).prop("disabled", true);
          $("#btnBuscarUser").prop("disabled", false);
          $("#gifCargando").hide();
          $("#btnGuardarLugarEmpleado").show();
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#txtParametroUser").focus(function () {
    $(this).val("");
  });

  $("#btnAsignarLugarEmpelado").click(function () {
    $("#gifCargando2").show();
    var hayError = false,
      textoError = "";
    if (!$("#listCatalogoSeleccion").val()) {
      hayError = true;
      textoError += "Seleccione dirección de Estacionamiento\n";
      $("#listCatalogoSeleccion").css("border", "2px solid red");
    } else {
      $("#listCatalogoSeleccion").css("border", "");
    }
    if (!$("#lugar").val()) {
      hayError = true;
      textoError += "Ingrese lugar\n";
      $("#lugar").css("border", "2px solid red");
    } else {
      $("#lugar").css("border", "");
    }
    if (hayError) {
      alertify.error(textoError);
      return false;
    }

    $("#numEmpleado, #nombreUser, #correoUser").prop("disabled", false);
    var direccion = $("#listCatalogoSeleccion").val();
    var arrregloDireccion = direccion.split("-");
    var valorDireccion = arrregloDireccion[0];

    var objetoEstacionamiento = {
      iCajon: valorDireccion,
      sLugar: $("#lugar").val(),
      iUsuario: $("#numEmpleado").val(),
      sObservaciones: $("#observacionesEsta").val(),
      nombreUsuario: $("#nombreUser").val(),
      correoUsuario: $("#correoUser").val(),
    };

    $.ajax({
      url: "/Estacionamiento/AsignarEstacionamientoEmpleado_AJAX",
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataTYpe: "JSON",
      data: JSON.stringify(objetoEstacionamiento),
      cache: false,
      success: function (r) {
        if (r.error) {
          var alertDlg = alertify
            .alert("Error", r.mensaje)
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
          $("#gifCargando2").hide();
          $("#numEmpleado, #nombreUser, #correoUser").prop("disabled", true);
        } else if (r.validacion) {
          var alertDlg = alertify
            .alert(
              "Error",
              "Este usuario ya cuenta con una asignación en esa Dirección"
            )
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
          $("#gifCargando2").hide();
          $("#numEmpleado, #nombreUser, #correoUser").prop("disabled", true);
        } else {
          $("#spanMensajeExito").text(
            "Se asignó el cajón de estacionamiento al usuario: " +
              $("#nombreUser").val() +
              " correctamente"
          );
          $("#divMensajeExito").show();
          setTimeout(function () {
            $("#divMensajeExito").hide("fade");
          }, 4000);
          Limpiar();
          $(
            "#catalogoLugares, #divUserAD, #divUsuarios, #btnGuardarLugarEmpleado"
          ).hide();
          $("#gifCargando2").hide();
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#btnVisitantes").click(function () {
    LugaresPorAsignar();
  });

  $("#noEmpleadoSolicita").focus(function () {
    $(this).val("");
  });

  $("#noEmpleadoSolicita").focusout(function () {
    var parametroBusqueda = $("#noEmpleadoSolicita").val();
    if (!parametroBusqueda) {
      alertify.error("No ha ingresado un número de empleado");
      return;
    }
    $.ajax({
      url:
        "/Estacionamiento/BuscarUserAD_AJAX?parametroBusqueda=" +
        parametroBusqueda,
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          var alertDlg = alertify
            .alert("Error", r.mensaje)
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
          $("#txtParametroUser").val("");
          $("#btnBuscarUser").prop("disabled", false);
          $("#gifCargando").hide();
        } else {
          $("#divNombreEmpleado").show();
          $("#nombreEmpleadoVisita").val(r.Usuario.sNombre);
          $("#correoEmpleadoVisita").val(r.Usuario.sCorreo);
          $("#noEmpleadoSolicita").css("border", "");
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#noEmpleadoSolicita").keypress(function (e) {
    if (e.which != 13) return;
    var parametroBusqueda = $("#noEmpleadoSolicita").val();
    $.ajax({
      url:
        "/Estacionamiento/BuscarUserAD_AJAX?parametroBusqueda=" +
        parametroBusqueda,
      type: "GET",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          var alertDlg = alertify
            .alert("Error", r.mensaje)
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
          $("#txtParametroUser").val("");
          $("#btnBuscarUser").prop("disabled", false);
          $("#gifCargando").hide();
        } else {
          $("#divNombreEmpleado").show();
          $("#nombreEmpleadoVisita").val(r.Usuario.sNombre);
          $("#correoEmpleadoVisita").val(r.Usuario.sCorreo);
          $("#noEmpleadoSolicita").css("border", "");
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#btnAsignarLugarVisitante").click(function () {
    var hayError = false,
      textoError = "";
    function req(id, msg) {
      if (!$(id).val()) {
        hayError = true;
        textoError += msg + "\n";
        $(id).css("border", "2px solid red");
      } else {
        $(id).css("border", "");
      }
    }
    req("#listCatalogoSeleccion", "Seleccione dirección de Estacionamiento");
    req("#lugar", "Ingrese lugar");
    req("#nombreVisita", "Ingrese nombre de Visitante");
    req("#fechaVisita", "Ingrese Fecha de Visita");
    req("#horaEntrada", "Ingrese Hora de Entrada");
    req("#horaSalida", "Ingrese Hora de Salida");
    req("#noEmpleadoSolicita", "Ingrese Número de Empleado que Visita");
    if (hayError) {
      alertify.error(textoError);
      return false;
    }

    $("#nombreEmpleadoVisita, #correoEmpleadoVisita").prop("disabled", false);
    var direccion = $("#listCatalogoSeleccion").val();
    var arrregloDireccion = direccion.split("-");
    var valorDireccion = arrregloDireccion[0];

    var objetoEstacionamiento = {
      iCajon: valorDireccion,
      sLugar: $("#lugar").val(),
      iUsuario: $("#noEmpleadoSolicita").val(),
      nombreUsuario: $("#nombreEmpleadoVisita").val(),
      correoUsuario: $("#correoEmpleadoVisita").val(),
      nombreVisitante: $("#nombreVisita").val(),
      dtFechaVisita: $("#fechaVisita").val(),
      horaVisita: $("#horaEntrada").val(),
      horaSalida: $("#horaSalida").val(),
      sObservaciones: $("#observacionesEstaVisita").val(),
      sPlaca: $("#placa").val(),
      sModelo: $("#modelo").val(),
      sColor: $("#color").val(),
    };

    $.ajax({
      url: "/Estacionamiento/AsignarEstacionamientoVisitante_AJAX",
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataTYpe: "JSON",
      data: JSON.stringify(objetoEstacionamiento),
      cache: false,
      success: function (r) {
        if (r.error) {
          var alertDlg = alertify
            .alert("Error", r.mensaje)
            .set("label", "Aceptar");
          alertDlg.set({ transition: "zoom" });
          alertDlg.set("modal", false);
        } else {
          Limpiar();
          $("#catalogoLugares, #datosVisitante, #divNombreEmpleado").hide();
          $("#nombreEmpleadoVisita, #correoEmpleadoVisita").prop(
            "disabled",
            true
          );
          $("#spanMensajeExito").text(
            "Se asignó el Cajón de Estacionamiento Correctamente"
          );
          $("#divMensajeExito").show();
          setTimeout(function () {
            $("#divMensajeExito").hide("fade");
          }, 4000);
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#btnVerLugares").click(function () {
    $(
      "#tiposUser, #datosVisitante, #btnGuardarLugarEmpleado, #divUserAD, #divTablaVisitantesAsigna"
    ).hide();
    MostrarInformacionEmpleados();
  });

  $("#btnVerInformacionVisitantes").click(function () {
    LugaresAsignadosVisitantes();
  });
  $("#btnVerInformacionUsuarios").click(function () {
    MostrarInformacionEmpleados();
    $("#btnVerInformacionVisitantes").show();
    $("#btnVerInformacionUsuarios").hide();
  });
  $("#btnCerrarModalHora").click(function () {
    $("#modalHora").modal("hide");
  });

  $("#btnEditarHorario").click(function () {
    var objetoEstaciona = {
      uiRegistroCajones: $("#txtRegistroCajon").val(),
      horaSalida: $("#horaSalidaUpdate").val(),
    };
    $.ajax({
      url: "/Estacionamiento/EditarHoraSalida_AJAX",
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      data: JSON.stringify(objetoEstaciona),
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          $("#modalHora").modal("hide");
          alertify.alert(
            "Se editó la Hora de Salida exitosamente",
            function () {
              alertify.success("OK");
            }
          );
          LugaresAsignadosVisitantes();
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });

  $("#btnCerrarModalLugares").click(function () {
    $("#modalCajones").modal("hide");
  });

  $("#btnAsignarLugares").click(function () {
    var hayError = false,
      textoError = "";
    if (!$("#listCatalogoSeleccionModal").val()) {
      hayError = true;
      textoError += "Seleccione dirección de Estacionamiento\n";
      $("#listCatalogoSeleccionModal").css("border", "2px solid red");
    } else {
      $("#listCatalogoSeleccionModal").css("border", "");
    }
    if (!$("#lugarModal").val()) {
      hayError = true;
      textoError += "Ingrese lugar\n";
      $("#lugarModal").css("border", "2px solid red");
    } else {
      $("#lugarModal").css("border", "");
    }
    if (hayError) {
      alertify.error(textoError);
      return false;
    }

    var direccion = $("#listCatalogoSeleccionModal").val();
    var arrregloDireccion = direccion.split("-");
    var valorDireccion = arrregloDireccion[0];
    var objetoAsignarLugar = {
      iCajon: valorDireccion,
      sLugar: $("#lugarModal").val(),
      uiRegistroCajones: $("#txtRegistroCajonNuevo").val(),
    };
    $.ajax({
      url: "/Estacionamiento/AsignarCajonVisitante_AJAX",
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      data: JSON.stringify(objetoAsignarLugar),
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          alertify.alert("Se asignó el cajón exitosamente", function () {
            alertify.success("OK");
          });
          LugaresPorAsignar();
          $("#modalCajones").modal("hide");
        }
      },
    });
  });

  $("#btnCambiarCajon").click(function () {
    var hayError = false,
      textoError = "";
    if (!$("#listCatalogoSeleccionModal").val()) {
      hayError = true;
      textoError += "Seleccione dirección de Estacionamiento\n";
      $("#listCatalogoSeleccionModal").css("border", "2px solid red");
    } else {
      $("#listCatalogoSeleccionModal").css("border", "");
    }
    if (!$("#lugarModal").val()) {
      hayError = true;
      textoError += "Ingrese lugar\n";
      $("#lugarModal").css("border", "2px solid red");
    } else {
      $("#lugarModal").css("border", "");
    }
    if (hayError) {
      alertify.error(textoError);
      return false;
    }
    var direccion = $("#listCatalogoSeleccionModal").val();
    var arrregloDireccion = direccion.split("-");
    var valorDireccion = arrregloDireccion[0];
    var objetoAsignarLugar = {
      iCajon: valorDireccion,
      sLugar: $("#lugarModal").val(),
      uiRegistroCajones: $("#txtRegistroCajonCambio").val(),
    };
    $.ajax({
      url: "/Estacionamiento/CambiarCajonVisitante_AJAX",
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      data: JSON.stringify(objetoAsignarLugar),
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          alertify.alert("Se asignó el cajón exitosamente", function () {
            alertify.success("OK");
          });
          LugaresAsignadosVisitantes();
          $("#modalCajones").modal("hide");
          $("#txtRegistroCajonCambio").val("");
        }
      },
    });
  });
});

// --- Filtros Dinámicos ---
(function () {
  function debounce(fn, wait) {
    var t;
    return function () {
      clearTimeout(t);
      var ctx = this,
        args = arguments;
      t = setTimeout(function () {
        fn.apply(ctx, args);
      }, wait || 350);
    };
  }

  function cargarUsuariosFiltrado() {
    var texto = $("#filtroTextoUsuarios").val() || "";
    var desde = $("#filtroDesdeUsuarios").val() || "";
    var hasta = $("#filtroHastaUsuarios").val() || "";
    if ((desde && !hasta) || (!desde && hasta)) {
      alertify.error(
        "Para filtrar por fecha, captura el rango completo (Desde y Hasta)."
      );
      return;
    }
    $.ajax({
      url: "/Estacionamiento/MostrarAsignacionesEstacionamientosFiltrado_AJAX",
      type: "GET",
      data: { texto: texto, fechaDesde: desde, fechaHasta: hasta },
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          MostrarEstacionamientos(r.listaEstacionamientos);
        }
      },
      error: function (e) {
        console.error(e);
      },
    });
  }

  function cargarVisitantesFiltrado() {
    var texto = $("#filtroTextoVisitantes").val() || "";
    var desde = $("#filtroDesdeVisitantes").val() || "";
    var hasta = $("#filtroHastaVisitantes").val() || "";
    if ((desde && !hasta) || (!desde && hasta)) {
      alertify.error(
        "Para filtrar por fecha de visita, captura el rango completo (Desde y Hasta)."
      );
      return;
    }
    $.ajax({
      url: "/Estacionamiento/MostrarAsignacionesEstacionamientosVisitantesFiltrado_AJAX",
      type: "GET",
      data: { texto: texto, fechaDesde: desde, fechaHasta: hasta },
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          MostrarEstacionamientosVisitas(r.listaEstacionamientos);
        }
      },
      error: function (e) {
        console.error(e);
      },
    });
  }

  // Handlers Usuarios
  $(document).on(
    "input",
    "#filtroTextoUsuarios",
    debounce(cargarUsuariosFiltrado, 400)
  );
  $(document).on(
    "change",
    "#filtroDesdeUsuarios, #filtroHastaUsuarios",
    cargarUsuariosFiltrado
  );
  $(document).on("click", "#btnLimpiarUsuarios", function () {
    $("#filtroTextoUsuarios,#filtroDesdeUsuarios,#filtroHastaUsuarios").val("");
    cargarUsuariosFiltrado();
  });

  // Handlers Visitantes
  $(document).on(
    "input",
    "#filtroTextoVisitantes",
    debounce(cargarVisitantesFiltrado, 400)
  );
  $(document).on(
    "change",
    "#filtroDesdeVisitantes, #filtroHastaVisitantes",
    cargarVisitantesFiltrado
  );
  $(document).on("click", "#btnLimpiarVisitantes", function () {
    $(
      "#filtroTextoVisitantes,#filtroDesdeVisitantes,#filtroHastaVisitantes"
    ).val("");
    cargarVisitantesFiltrado();
  });

  // Handlers Visitantes Por Asignar
  $(document).on(
    "input",
    "#filtroTextoVisitantesAsigna",
    debounce(LugaresPorAsignarFiltrado, 400)
  );
  $(document).on(
    "change",
    "#filtroDesdeVisitantesAsigna, #filtroHastaVisitantesAsigna",
    LugaresPorAsignarFiltrado
  );
  $(document).on("click", "#btnLimpiarVisitantesAsigna", function () {
    $(
      "#filtroTextoVisitantesAsigna,#filtroDesdeVisitantesAsigna,#filtroHastaVisitantesAsigna"
    ).val("");
    LugaresPorAsignarFiltrado();
  });
})();

function ObtenerCajon(ID) {
  var confirmDlg = alertify
    .confirm(
      "Liberar Cajón",
      "¿Estás seguro de liberar el Cajón de Estacionamiento",
      null,
      null
    )
    .set("labels", { ok: "Confirmar", cancel: "Cancelar" });
  confirmDlg.set({ transition: "slide" });
  confirmDlg.set("onok", function () {
    $.ajax({
      url: "/Estacionamiento/LiberarCajon_AJAX?registroCajon=" + ID,
      type: "POST",
      contentType: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mesaje);
        } else {
          MostrarInformacionEmpleados();
          alertify.success(
            "Se liberó el cajón de estacionamiento exitosamente"
          );
          $("#btnVerInformacionVisitantes").show();
          $("#btnVerInformacionUsuarios").hide();
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });
}

function EditarHoraSalida(ID) {
  $("#modalHora").modal("show");
  $("#txtRegistroCajon").val(ID);
}
function AsignarCajones(ID) {
  /* Mantener funciones ya movidas en los handlers arriba */
}
function EliminarSolicitud(ID) {
  var confirmDlg = alertify
    .confirm(
      "Eliminar Solicitud",
      "¿Estás seguro de eliminar la soliictud de Cajón de Estacionamiento?",
      null,
      null
    )
    .set("labels", { ok: "Confirmar", cancel: "Cancelar" });
  confirmDlg.set({ transition: "slide" });
  confirmDlg.set("onok", function () {
    $.ajax({
      url: "/Estacionamiento/EliminarSolicitud_AJAX?solicitud=" + ID,
      type: "GET",
      contentTYpe: "application/json;charset=utf-8",
      dataType: "JSON",
      cache: false,
      success: function (r) {
        if (r.error) {
          alert(r.mensaje);
        } else {
          alertify.success("Se eliminó la solicitud correctamente");
          LugaresPorAsignar();
        }
      },
      error: function (error) {
        alert(error.responseText);
      },
    });
  });
}

function EditarCajon(ID) {
  $.ajax({
    url: "/Estacionamiento/MostrarCatalogoLugaresVisitante_AJAX",
    type: "GET",
    contentType: "application/json;charset=utf-8",
    dataType: "JSON",
    cache: false,
    success: function (r) {
      if (r.error) {
        alert(r.mensaje);
      } else {
        $("#modalCajones").modal("show");
        $("#btnCambiarCajon").show();
        $("#btnAsignarLugares").hide();
        $("#txtRegistroCajonCambio").val(ID);
        $("#listCatalogoSeleccionModal").val("").empty();
        $("#listCatalogoModal").empty();
        $.each(r.listaCalagos, function (i, catalogo) {
          var option = $(document.createElement("option"));
          option.html(catalogo.sNombreCliente);
          option.val(catalogo.uiRegistroCatalogo + "-" + catalogo.sLugar);
          $("#listCatalogoModal").append(option);
        });
        Limpiar();
      }
    },
    error: function (error) {
      alert(error.responseText);
    },
  });
}
