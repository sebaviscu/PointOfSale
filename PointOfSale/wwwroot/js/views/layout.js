let razonesList;


const BASIC_MODEL_MOVIMIENTIO_CAJA = {
    idMovimientoCaja: 0,
    comentario: '',
    registrationDate: null,
    registrationUser: '',
    idRazonMovimientoCaja: 0,
    importe: 0
}
$(document).ready(function () {

    $('#modalDataAbrirTurno').modal({
        backdrop: 'static',
        keyboard: false
    });


    $("#limpiarNotificaciones").on("click", function () {

        fetch(`/Notification/LimpiarTodoNotificacion`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        })
            .then(response => {
                return response.json();
            }).then(responseJson => {
                if (responseJson.state) {
                    $(".dropdown-menu .dropdown-header").remove();
                    $("#listaNotificaciones").remove();

                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }

            }).catch((error) => {
            })
    })


    $(".notificacion").on("click", function () {
        if ($(this).attr('accion') != '') {

            fetch(`/Notification/UpdateNotificacion?idNotificacion=${$(this)[0].id}`, {
                method: "PUT",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            })
                .then(response => {
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {
                        window.location.href = responseJson.object.accion;

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }

                }).catch((error) => {
                })
        }
    })

    $("#btnChangePV").on("click", function () {

        fetch("/Tienda/GetTienda")
            .then(response => {
                return response.json();
            }).then(responseJson => {

                if (responseJson.data.length > 0) {
                    $("#modalCambioTienda").modal("show");
                    let idActual = 0;
                    responseJson.data.forEach((item) => {
                        let actual = '';
                        if (item.tiendaActual) {
                            idActual = item.idTienda;
                            actual = ' (Actual)';
                        }
                        $("#cboTiendas").append(
                            $("<option>").val(item.idTienda).text(item.nombre + ' ' + actual)
                        )
                    });
                    $("#cboTiendas").val(idActual);
                }
            })
    })

    $("#btnCambiarTienda").on("click", function () {

        //$("#modalCambioTienda").modal("hide");
        $("#modalCambioTienda").LoadingOverlay("show")

        var idTienda = $("#cboTiendas").val();
        fetch(`/Tienda/ChangeTienda?idTienda=${idTienda}`, {
            method: "DELETE"
        }).then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
            .catch((error) => {
            })
    })
});

$("#btnAbrirCerrarTurno").on("click", function () {

    fetch(`/Turno/GetTurnoActual`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                if (responseJson.object == null) {
                    openModalDataAbrirTurno();
                }
                else {
                    $("#txtIdTurnoLayout").val(responseJson.object.idTurno);

                    let fechaInicio = moment(responseJson.object.fechaInicio);
                    let fechaFin = responseJson.object.FechaFin != null
                        ? moment(responseJson.object.FechaFin)
                        : moment().tz('America/Argentina/Buenos_Aires');

                    if (fechaInicio.isValid()) {
                        let fechaFormatted = fechaInicio.format('DD/MM/YYYY');
                        let horaInicioFormatted = fechaInicio.format('HH:mm:ss');

                        $("#txtInicioTurnoCierre").val(fechaFormatted);
                        $("#txtHoraInicioTurnoCierre").val(horaInicioFormatted);
                    }

                    if (fechaFin.isValid()) {
                        let horaFinFormatted = fechaFin.format('HH:mm:ss');

                        $("#txtCierraTurnoCierre").val(horaFinFormatted);
                    }

                    if (responseJson.object.observacionesApertura != '') {
                        $("#txtObservacionesApertura").val(responseJson.object.observacionesApertura);
                        $('#divObservacionesApertura').css('display', '');
                    }
                    else {
                        $('#divObservacionesApertura').css('display', 'none');
                    }
                    $("#modalDataCerrarTurno").modal("show");

                    renderVentasPorTipoVenta(responseJson.object.ventasPorTipoVenta, responseJson.object.totalInicioCaja);

                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });

})



$("#btnAbrirMovimientoCaja").on("click", function () {
    cargarRazones();
    disablesMovimientoCajaModal(false);

    $("#modalMovimientoCaja").modal("show");
});

function cargarRazones() {
    showLoading();
    fetch("/MovimientoCaja/GetRazonMovimientoCaja")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                if (responseJson.object.length > 0) {
                    razonesList = responseJson.object;

                    $("#cboTipoRazonMovimiento").trigger("change");
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
            removeLoading();
        });
}

$("#cboTipoRazonMovimiento").on("change", function () {
    filterRazones();
});

function filterRazones() {
    let selectedTipo = $("#cboTipoRazonMovimiento").val();
    $("#cboRazonMovimiento").empty();

    razonesList.forEach((item) => {
        if (item.tipo == selectedTipo) {
            $("#cboRazonMovimiento").append(
                $("<option>").val(item.idRazonMovimientoCaja).text(item.descripcion)
            );
        }
    });
}

$("#btnSaveMovimientoCaja").on("click", function () {
    const inputs = $("input.input-validate-movimientoCaja").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if ($("#txtComentarioMovimientoCaja").val().length < 10) {
        toastr.warning("La descripción debe ser mayor a 10 caracteres", "");
    }

    const model = structuredClone(BASIC_MODEL_MOVIMIENTIO_CAJA);
    model["idRazonMovimientoCaja"] = parseInt($("#cboRazonMovimiento").val());
    model["importe"] = parseFloat($("#txtImporteMovimientoCaja").val());
    model["comentario"] = $("#txtComentarioMovimientoCaja").val();

    $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("show")

    fetch("/MovimientoCaja/CreateMovimientoCaja", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {
            //location.reload()
            $("#modalMovimientoCaja").modal("hide");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("hide")
    })
})
function disablesMovimientoCajaModal(type) {
    $('#btnSaveMovimientoCaja').css('display', type ? 'none' : '');
    $('#divFechaUsuarioMovimientoCaja').css('display', type ? '' : 'none');

    $('#cboTipoRazonMovimiento').prop('disabled', type);
    $('#cboRazonMovimiento').prop('disabled', type);
    $('#txtImporteMovimientoCaja').prop('disabled', type);
    $('#txtComentarioMovimientoCaja').prop('disabled', type);

}

function horaActual() {
    let dateTimeModifHoy = new Date();

    let options = {
        timeZone: 'America/Argentina/Buenos_Aires',
        year: 'numeric',
        month: '2-digit',
        day: '2-digit',
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    };

    let formatter = new Intl.DateTimeFormat('es-AR', options);
    let dateTimeArgentina = formatter.format(dateTimeModifHoy);

    return dateTimeArgentina;
}

function openModalDataAbrirTurno() {
    let dateTimeArgentina = moment().tz('America/Argentina/Buenos_Aires');

    // Mostrar el modal
    $("#modalDataAbrirTurno").modal("show");

    // Esperar hasta que el modal esté completamente mostrado
    $('#modalDataAbrirTurno').on('shown.bs.modal', function () {
        $("#txtInicioTurnoAbrir").val(dateTimeArgentina.format('DD/MM/YYYY'));
        $("#txtHoraInicioTurnoAbrir").val(dateTimeArgentina.format('HH:mm:ss'));
    });
}


function renderVentasPorTipoVenta(ventasPorTipoVenta, importeInicioCaja) {
    $("#contMetodosPagoLayout").empty();
    let contenedor = $("#contMetodosPagoLayout");
    let totalImporte = importeInicioCaja;

    crearFilaTotalesTurno(contenedor, "TOTAL INICIO CAJA", importeInicioCaja, "txtInicioCajaCierre");

    ventasPorTipoVenta.forEach(function (venta) {
        crearFilaTotalesTurno(contenedor, venta.descripcion, venta.total);
        totalImporte += parseFloat(venta.total);
    });

    contenedor.append($('<hr>'));

    crearFilaTotalesTurno(contenedor, "TOTAL", totalImporte.toFixed(2), "txtTotalSumado");
}

function crearFilaTotalesTurno(contenedor, descripcion, total, inputId = null) {
    let formGroup = $('<div>', { class: 'form-group row align-items-center', style: 'margin-bottom:2px' });  // Reducir el margen inferior

    let label = $('<label>', {
        class: 'col-sm-7 col-form-label',
        text: descripcion + ":",
        style: 'font-size: 20px; padding-right: 0px; padding-top: 0px;'
    });

    let inputDiv = $('<div>', { class: 'col-sm-5' });

    let inputGroup = $('<div>', { class: 'input-group input-group-sm', style: 'margin-top: 0px;' });

    let inputGroupPrepend = $('<div>', { class: 'input-group-prepend' });

    let span = $('<span>', {
        class: 'input-group-text',
        text: '$'
    });

    let inputAttributes = {
        type: 'number',
        step: 'any',
        class: 'form-control form-control-sm',
        min: '0',
        value: total,
        disabled: true,
        style: 'text-align: end;'
    };

    if (inputId) {
        inputAttributes.id = inputId;
    }

    let input = $('<input>', inputAttributes);

    inputGroupPrepend.append(span);
    inputGroup.append(inputGroupPrepend).append(input);
    inputDiv.append(inputGroup);

    formGroup.append(label).append(inputDiv);
    contenedor.append(formGroup);
}


$("#btnAbrirTurno").on("click", function () {
    let desc = $("#txtObservacionesInicioCajaCierre").val();
    let importeInicioTurno = $("#txtInicioCajaAbrir").val();

    let modelTurno = {
        observacionesApertura: desc,
        TotalInicioCaja: parseFloat(importeInicioTurno)
    };
    $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("show")

    fetch("/Turno/AbrirTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(modelTurno)
    }).then(response => {
        $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            $("#modalDataAbrirTurno").modal("hide");
            location.reload();

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("hide")
    })
})

$("#btnFinalizarTurno").on("click", function () {

    let modelTurno = {
        observacionesCierre: $("#txtObservacionesCierre").val(),
        idTurno: parseInt($("#txtIdTurnoLayout").val())
    };
    $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("show")

    fetch("/Turno/CerrarTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(modelTurno)
    }).then(response => {
        $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            $("#modalDataCerrarTurno").modal("hide");
            location.reload();

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("hide")
    })

})


function abrirTurnoDesdeViewTurnos(idTurno) {
    fetch(`/Turno/GetOneTurno?idTurno=` + idTurno, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                $("#txtIdTurnoLayout").val(responseJson.object.idTurno);

                let fechaInicio = moment(responseJson.object.fechaInicio);
                let fechaFin = responseJson.object.fechaFin
                    ? moment(responseJson.object.fechaFin)
                    : '';

                if (fechaInicio.isValid()) {
                    let fechaFormatted = fechaInicio.format('DD/MM/YYYY');
                    let horaInicioFormatted = fechaInicio.format('HH:mm:ss');

                    $("#txtInicioTurnoCierre").val(fechaFormatted);
                    $("#txtHoraInicioTurnoCierre").val(horaInicioFormatted);
                }

                if (fechaFin != '') {
                    let horaFinFormatted = fechaFin.format('HH:mm:ss');

                    $("#txtCierraTurnoCierre").val(horaFinFormatted);
                }

                if (responseJson.object.observacionesApertura != '') {
                    $("#txtObservacionesApertura").val(responseJson.object.observacionesApertura);
                    $('#divObservacionesApertura').css('display', '');
                }
                else {
                    $('#divObservacionesApertura').css('display', 'none');
                }
                $("#txtObservacionesCierre").val(responseJson.object.observacionesCierre);
                $("#txtObservacionesCierre").prop("disabled", true);
                $("#btnFinalizarTurno").hide();

                $("#modalDataCerrarTurno").modal("show");

                renderVentasPorTipoVenta(responseJson.object.ventasPorTipoVenta, responseJson.object.totalInicioCaja);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
}

$('#modalDataCerrarTurno').on('hidden.bs.modal', function () {
    $("#txtObservacionesCierre").prop("disabled", false);

    $("#btnFinalizarTurno").show();
});
function generarDatos() {
    showLoading();

    fetch(`/Access/GenerarDatos`, {
        method: "POST"

    }).then(responseJson => {

        removeLoading();
        if (responseJson.status == 200) {
            swal("Exitoso!", "Datos cerados", "success");

        } else {
            swal("Lo sentimos", "", "error");
        }
    })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });

}

function showLoading() {
    if (document.getElementById("divLoadingFrame") != null) {
        return;
    }
    var style = document.createElement("style");
    style.id = "styleLoadingWindow";
    style.innerHTML = `
        .loading-frame {
            position: fixed;
            background-color: rgba(80, 80, 80, 0.3);
            left: 0;
            top: 0;
            right: 0;
            bottom: 0;
            z-index: 4;
        }

        .loading-track {
            height: 50px;
            display: inline-block;
            position: absolute;
            top: calc(50% - 50px);
            left: 50%;
        }

        .loading-dot {
            height: 5px;
            width: 5px;
            background-color: white;
            border-radius: 100%;
            opacity: 0;
        }

        .loading-dot-animated {
            animation-name: loading-dot-animated;
            animation-direction: alternate;
            animation-duration: .75s;
            animation-iteration-count: infinite;
            animation-timing-function: ease-in-out;
        }

        @keyframes loading-dot-animated {
            from {
                opacity: 0;
            }

            to {
                opacity: 1;
            }
        }
    `
    document.body.appendChild(style);
    var frame = document.createElement("div");
    frame.id = "divLoadingFrame";
    frame.classList.add("loading-frame");
    for (var i = 0; i < 10; i++) {
        var track = document.createElement("div");
        track.classList.add("loading-track");
        var dot = document.createElement("div");
        dot.classList.add("loading-dot");
        track.style.transform = "rotate(" + String(i * 36) + "deg)";
        track.appendChild(dot);
        frame.appendChild(track);
    }
    document.body.appendChild(frame);
    var wait = 0;
    var dots = document.getElementsByClassName("loading-dot");
    for (var i = 0; i < dots.length; i++) {
        window.setTimeout(function (dot) {
            dot.classList.add("loading-dot-animated");
        }, wait, dots[i]);
        wait += 150;
    }
};

function removeLoading() {
    document.body.removeChild(document.getElementById("divLoadingFrame"));
    document.body.removeChild(document.getElementById("styleLoadingWindow"));
};

function setupPasswordToggle($passwordInput, $toggleButton) {
    $toggleButton.on('mousedown', function () {
        $passwordInput.attr('type', 'text');
    });

    $toggleButton.on('mouseup mouseleave', function () {
        $passwordInput.attr('type', 'password');
    });

    // Evitar que el botón reciba el foco
    $toggleButton.on('click', function (e) {
        e.preventDefault();
    });
}

function formatCuil(value) {
    value = value.replace(/\D/g, '');
    let formattedValue = '';

    if (value.length > 0) formattedValue += value.substring(0, 2);
    if (value.length > 2) formattedValue += '-' + value.substring(2, 10);
    if (value.length > 10) formattedValue += '-' + value.substring(10, 11);

    return formattedValue;
}

function validateCuilCuit() {
    let cuilValue = $('#txtCuilCliente').val().replace(/\D/g, '');
    return cuilValue.length == 11;
}

function validateDni() {
    let dniValue = $('#txtCuilCliente').val().replace(/\D/g, '');
    return dniValue.length == 8;
}



function inicializarClientesFactura() {

    let isNuevoCliente = $('#switchNuevoCliente').is(':checked');
    toggleFields(isNuevoCliente);
    $('#switchNuevoCliente').change(function () {
        let isNuevoCliente = $(this).is(':checked');
        toggleFields(isNuevoCliente);
        resetModalClientesFactura();
    });

    $('[data-bs-toggle="popover"]').popover({
        html: true
    });

    $('#modalDatosFactura').on('shown.bs.modal', function () {
        if (!$('#cboClienteFactura').data('select2')) {
            funClientesFactura();
        }
        setTimeout(function () {
            $('#cboClienteFactura').select2('open');
        }, 100);
    });

    $('#modalDatosFactura').on('hidden.bs.modal', function () {
        resetModalClientesFactura();
    });

    $("#cboCondicionIvaCliente").on("change", function () {
        $('#txtCuilCliente').val('');
    });

    seleccionaCliente();
}
function funClientesFactura() {
    showLoading();

    $('#cboClienteFactura').select2({
        ajax: {
            url: "/Sales/GetClientesByFacturar",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idCliente,
                            text: item.nombre,
                            cuil: item.cuil,
                            telefono: item.telefono,
                            direccion: item.direccion,
                            condicionIva: item.condicionIva,
                            comentario: item.comentario,
                            color: '',
                            total: ''
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando cliente...',
        minimumInputLength: 3,
        templateResult: formatResultsClients,
        allowClear: true,
        dropdownParent: $('#modalDatosFactura .modal-content')
    });

    $('#cboClienteFactura').on('select2:select', function (e) {
        let data = e.params.data;
        $('#txtNombreCliente').val(data.text);
        $('#txtCuilCliente').val(data.cuil);
        //$('#txtIdCuilFacturaCliente').val(data.cuil);
        $('#txtTelefonoCliente').val(data.telefono);
        $('#txtDireccionCliente').val(data.direccion);
        $('#cboCondicionIvaCliente').val(data.condicionIva);
        $('#txtComentarioCliente').val(data.comentario);
        $('#txtIdClienteFactura').val(data.id);
    });

    $('#txtCuilCliente').on('input', function () {
        if ($('#cboCondicionIvaCliente').val() == '4') { // Resp. Inscripto
            let formattedValue = formatCuil($(this).val());
            $(this).val(formattedValue);
        }
    });

    $('#txtCuilCliente').on('keypress', function (e) {
        let charCode = (e.which) ? e.which : e.keyCode;
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
        }
    });
    removeLoading();
}


function removeHyphens(value) {
    return value.replace(/-/g, '');
}

function resetModalClientesFactura() {
    $('#cboClienteFactura').val(null).trigger('change');
    $('#txtNombreCliente').val('');
    $('#txtCuilCliente').val('');
    $('#txtTelefonoCliente').val('');
    $('#txtDireccionCliente').val('');
    $('#cboCondicionIvaCliente').val('');
    $('#txtComentarioCliente').val('');
    $('#txtIdClienteFacturaCliente').val(0);
}
function toggleFields(isNuevoCliente) {
    $('#txtNombreCliente, #txtCuilCliente, #cboCondicionIvaCliente, #txtTelefonoCliente, #txtDireccionCliente, #txtComentarioCliente').prop('disabled', !isNuevoCliente);
    $('#cboClienteFactura').prop('disabled', isNuevoCliente);

    if (isNuevoCliente) {
        $('#switchGuardarNuevoClienteDiv').css('display', 'block');
    } else {
        $('#switchGuardarNuevoClienteDiv').css('display', 'none');
        $('#switchGuardarNuevoCliente').prop('checked', false);
    }
}

async function seleccionaCliente() {

    $("#btnSeleccionarClienteFactura").on("click", function () {

        const inputs = $("input.input-validate-cliente").serializeArray();
        const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

        if (inputs_without_value.length > 0) {
            const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
            toastr.warning(msg, "");
            $(`input[name="${inputs_without_value[0].name}"]`).focus();
            return;
        }

        let isValid = true;
        let cuilInput = $('#txtCuilCliente').val();

        if ($('#cboCondicionIvaCliente').val() == '4') { // Resp. Inscripto
            isValid = validateCuilCuit();
            cuilInput = removeHyphens(cuilInput);
        } else if ($('#cboCondicionIvaCliente').val() == '1') { // Consumidor Final
            isValid = validateDni();
        }

        if (!isValid) {
            const msg = `El campo CUIL / CUIT / DNI debe ser completado de forma corecta, dependiendo de la condicion del IVA`;
            toastr.warning(msg, "");
            return;
        }

        let nuevoCliente = document.getElementById("switchNuevoCliente").checked
        let guardarNuevouevoCliente = document.getElementById("switchGuardarNuevoCliente").checked

        let BASIC_MODEL_CLIENTE_SALE = {
            idCliente: 0,
            nombre: '',
            cuil: null,
            telefono: null,
            direccion: null,
            condicionIva: null,
            isActive: true
        }

        const model = structuredClone(BASIC_MODEL_CLIENTE_SALE);
        model["nombre"] = $("#txtNombreCliente").val();
        model["direccion"] = $("#txtDireccionCliente").val();
        model["cuil"] = $("#txtCuilCliente").val();
        model["telefono"] = $("#txtTelefonoCliente").val();
        model["condicionIva"] = $("#cboCondicionIvaCliente").val();
        model["idCliente"] = $("#txtIdClienteFactura").val();

        if (nuevoCliente) {

            if (guardarNuevouevoCliente) {

                fetch("/Admin/CreateCliente", {
                    method: "POST",
                    headers: { 'Content-Type': 'application/json;charset=utf-8' },
                    body: JSON.stringify(model)
                }).then(response => {
                    return response.json();
                }).then(responseJson => {

                    if (responseJson.state) {

                        model["idCliente"] = responseJson.object.idCliente;

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                }).catch((error) => {

                })
            }
        }

        $('#txtClienteParaFactura').val(`${model.nombre}  (CUIT: ${model.cuil})`);
        $('#txtClienteParaFactura').attr('cuil', model.cuil);
        $('#txtClienteParaFactura').attr('idcliente', model.idCliente);

        $('#btnFinalizarVentaParcial').prop('disabled', false);
        $("#txtMinimoIdentificarConsumidor").toggle(false);

        $("#modalDatosFactura").modal("hide")
    });
}

function formatResultsClients(data) {

    if (data.loading)
        return data.text;

    let container = $(
        `<table width="100%">
            <tr>
                <td class="col-sm-8">
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <em style="font-weight: bolder;margin:2px">${data.cuil}</em>
                </td>
            </tr>
         </table>`
    );

    return container;
}