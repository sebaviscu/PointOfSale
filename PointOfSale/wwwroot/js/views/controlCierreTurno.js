let billetesArray = [];


$(document).ready(function () {

    //$('#modalDataAbrirTurno').modal({
    //    backdrop: 'static',
    //    keyboard: false
    //});

})



$(document).on('input', '.txtCantBillete', function () {
    const idParts = $(this).attr('id').split('_');
    const columnPrefix = idParts[1];
    const index = idParts[2];
    const cantidad = $(this).val();
    const valorNominal = $(this).data('valor');
    const total = cantidad * valorNominal;

    $(`#txtSumaBillete_${columnPrefix}_${index}`).val(`$${total}`);
    calcularTotal();
});

function calcularTotal() {
    let totalSum = 0;
    $('.txtCantBillete').each(function () {
        const cantidad = parseInt($(this).val(), 10);
        const valorNominal = parseInt($(this).data('valor'), 10);
        totalSum += cantidad * valorNominal;
    });
    $('#totalSumBilletes').val(`${totalSum}`);
}

$("#btnAbrirCerrarTurno").on("click", function () {
    showLoading();

    fetch(`/Turno/GetTurnoActual`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                if (responseJson.object.turno == null) {
                    openModalDataAbrirTurno();
                }
                else {
                    let resultado = responseJson.object.turno;

                    $("#txtIdTurnoLayout").val(resultado.idTurno);

                    let fechaInicio = moment(resultado.fechaInicio);
                    let argentinaTime = '';

                    if (typeof moment !== 'undefined' && typeof moment.tz !== 'undefined') {
                        argentinaTime = moment().tz('America/Argentina/Buenos_Aires');
                    }
                    let fechaFin = resultado.FechaFin != null
                        ? moment(resultado.FechaFin)
                        : argentinaTime;


                    if (fechaInicio.isValid()) {
                        let fechaFormatted = fechaInicio.format('DD/MM/YYYY');
                        let horaInicioFormatted = fechaInicio.format('HH:mm');

                        $("#txtInicioTurnoCierre").val(fechaFormatted);
                        $("#txtHoraInicioTurnoCierre").val(horaInicioFormatted);
                    }

                    if (fechaFin != '' && fechaFin.isValid()) {
                        let horaFinFormatted = fechaFin.format('HH:mm');

                        $("#txtCierraTurnoCierre").val(horaFinFormatted);
                    }

                    if (resultado.observacionesApertura != '') {
                        $("#txtObservacionesApertura").val(resultado.observacionesApertura);
                        $('#divObservacionesApertura').css('display', '');
                    }
                    else {
                        $('#divObservacionesApertura').css('display', 'none');
                    }
                    $("#modalDataCerrarTurno").modal("show");
                    $("#btnValidarFinalizarTurno").show();

                    let contenedor = $("#contMetodosPagoLayout");

                    let validacionRealizada = resultado.validacionRealizada != null ? resultado.validacionRealizada : false;

                    if (validacionRealizada) {
                        renderVentasPorTipoVenta(contenedor, resultado.ventasPorTipoDeVenta, resultado.totalInicioCaja, resultado.validacionRealizada, responseJson.object.totalMovimientosCaja);
                    }
                    else {
                        renderVentasPorTipoVenta(contenedor, resultado.ventasPorTipoVentaPreviaValidacion, resultado.totalInicioCaja, resultado.validacionRealizada, responseJson.object.totalMovimientosCaja);
                    }

                    if (validacionRealizada) {
                        contenedor.append($('<hr style="margin-top: 0px;">'));

                        //crearFilaTotalesTurno(contenedor, "TOTAL Sistema", resultado.totalCierreCajaSistema, true, "txtTotalSumado");
                        //crearFilaTotalesTurno(contenedor, "TOTAL Usuario", resultado.totalCierreCajaReal, true, "txtTotalSumado");

                        $("#btnValidarFinalizarTurno").hide();
                        $("#btnFinalizarTurno").show();
                        $("#divSwitchCierreCaja").show();
                        $("#btnBilletes").hide()
                        mostrarErroresTurno(resultado.erroresCierreCaja);
                    }

                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
            removeLoading();
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
})

//function openModalDataAbrirTurno() {
//    $("#modalDataAbrirTurno").modal("show");

//    if (moment.tz != null) {

//        let dateTimeArgentina = moment().tz('America/Argentina/Buenos_Aires');

//        $('#modalDataAbrirTurno').on('shown.bs.modal', function () {
//            $("#txtInicioTurnoAbrir").val(dateTimeArgentina.format('DD/MM/YYYY'));
//            $("#txtHoraInicioTurnoAbrir").val(dateTimeArgentina.format('HH:mm'));
//        });
//    }
//}

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


function renderVentasPorTipoVenta(contenedor, ventasPorTipoVenta, importeInicioCaja, turnoCerrado, totalMovimientosCaja = null) {
    let total = renderVentasPorTipoVenta_OLIVA(contenedor, ventasPorTipoVenta, importeInicioCaja, turnoCerrado, totalMovimientosCaja);
    return total;
}

function renderVentasPorTipoVenta_OLIVA(contenedor, ventasPorTipoVenta, importeInicioCaja, turnoCerrado, totalMovimientosCaja = null) {
    contenedor.empty();

    crearFilaTablaTurno(contenedor, "TOTAL INICIO CAJA", importeInicioCaja, true, "txtInicioCajaCierre");

    if (totalMovimientosCaja != null && totalMovimientosCaja != 0) {
        crearFilaTablaTurno(contenedor, "MOV. DE CAJA", totalMovimientosCaja, true, 'txtMovimientoCaja');
    }

    contenedor.append($('<hr style="margin-top: 0px;">'));

    let total = 0;
    ventasPorTipoVenta.forEach(function (venta) {
        let totalFormaPAgo = turnoCerrado ? parseFloat(venta.totalUsuario) : parseFloat(venta.total);
        total += totalFormaPAgo;
        let id = 'txt' + venta.descripcion;
        let totalVenta = turnoCerrado ? totalFormaPAgo : '';

        crearFilaTablaTurno(contenedor, venta.descripcion, totalVenta, turnoCerrado, id);
    });

    return total;
}


function crearFilaTablaTurno(contenedor, descripcion, total, disabled, inputId = null) {
    let formGroup = $('<div>', { class: 'form-group row align-items-center', style: 'margin-bottom:2px' });

    let label = $('<label>', {
        class: 'col-sm-7 col-form-label',
        text: descripcion + ":",
        style: 'font-size: 20px; padding-right: 0px; padding-top: 0px;'
    });

    let inputDiv = $('<div>', { class: 'col-sm-5', style: 'padding-right: 0px; padding-left: 0px;' });


    let inputGroup = $('<div>', { class: 'input-group input-group-sm', style: 'margin-top: 0px;' });

    let inputGroupPrepend = $('<div>', { class: 'input-group-prepend' });

    let span = $('<span>', {
        class: 'input-group-text',
        text: '$'
    });

    let classInput = 'form-control form-control-sm';

    if (total == '' && total != '0') {
        classInput += ' validate-importe';
    }

    let inputAttributes = {
        type: 'number',
        step: 'any',
        class: classInput,
        min: '0',
        value: total,
        disabled: disabled,
        style: 'text-align: end;'
        //,change: function () { actualizarTotal(); }
    };

    if (inputId != '') {
        inputAttributes.id = inputId;
    }

    let input = $('<input>', inputAttributes);

    inputGroupPrepend.append(span);
    inputGroup.append(inputGroupPrepend).append(input);

    if (descripcion.toLowerCase() == "efectivo") {
        let inputGroupAppend = $('<div>', { class: 'input-group-append' });

        let button = $('<button>', {
            id: 'btnBilletes',
            type: 'button',
            class: 'btn btn-outline-success mdi mdi-cash',
            style: 'padding: 0 10px;',
            title: 'Contar billetes',
            'data-toggle': 'tooltip',
            click: function () {
                $('#modalBilletes').modal('show');
            }
        });

        inputGroupAppend.append(button);
        inputGroup.append(inputGroupAppend);
    }

    inputDiv.append(inputGroup);
    formGroup.append(label).append(inputDiv);
    contenedor.append(formGroup);
}

function actualizarTotal() {
    let total = 0;
    $('#contMetodosPagoLayout input[type="number"]').each(function () {
        if ($(this).attr('id') != 'txtTotalSumado') {
            let value = parseFloat($(this).val()) || 0;
            total += value;
        }

    });
    $('#txtTotalSumado').val(total.toFixed(0));
}

$("#btnValidarFinalizarTurno").on("click", function () {

    let listaVentas = obtenerValoresInputsCerrarTurno();
    if (listaVentas == null) {
        return;
    }

    let modelTurno = {
        observacionesCierre: $("#txtObservacionesCierre").val(),
        idTurno: parseInt($("#txtIdTurnoLayout").val()),
        billetesEfectivo: JSON.stringify(billetesArray),
        ventasPorTipoVentaPreviaValidacion: listaVentas
    };

    $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("show")

    fetch("/Turno/ValidarCierreTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(modelTurno)
    }).then(response => {
        $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {
            mostrarErroresTurno(responseJson.object.erroresCierreCaja);

            $("#btnValidarFinalizarTurno").hide();
            $("#btnFinalizarTurno").show();
            $("#divSwitchCierreCaja").show();
            $("#btnBilletes").hide()

            $('#contMetodosPagoLayout input[type="number"]').each(function () {
                $(this).prop('disabled', true);
            });

            let contenedor = $("#contMetodosPagoLayout");
            contenedor.append($('<hr style="margin-top: 0px;">'));
            //crearFilaTotalesTurno(contenedor, "TOTAL Sistema", responseJson.object.totalCierreCajaSistema, true, "txtTotalSumado");
            //crearFilaTotalesTurno(contenedor, "TOTAL Usuario", responseJson.object.totalCierreCajaReal, true, "txtTotalSumado");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("hide")
    })
})

function mostrarErroresTurno(errores) {
    if (errores == '') {
        $("#txtError").text('');
        const divError = document.getElementById("divError");
        divError.classList.remove('d-flex');
        divError.style.display = 'none';
    }
    else {
        $("#txtError").html(errores);
        const divError = document.getElementById("divError");
        divError.classList.add('d-flex');
        divError.style.display = '';
        errorCerrarTurno = true;
    }
}

let errorCerrarTurno = false;

$("#btnFinalizarTurno").on("click", function () {

    let listaVentas = obtenerValoresInputsCerrarTurno();
    if (listaVentas == null) {
        return;
    }

    let impirmirCierreCaja = document.getElementById('switchCierreCaja').checked;

    let modelTurno = {
        observacionesCierre: $("#txtObservacionesCierre").val(),
        idTurno: parseInt($("#txtIdTurnoLayout").val()),
        ventasPorTipoVenta: listaVentas,
        impirmirCierreCaja: impirmirCierreCaja,
        billetesEfectivo: JSON.stringify(billetesArray)
    };

    if (errorCerrarTurno) {
        if ($("#txtObservacionesCierre").val().length < 5) {
            toastr.warning("Al tener diferencias, debe agregar una observacion al cierre", "");
            return
        }
    }

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
            billetesArray = [];

            $("#modalDataCerrarTurno").modal("hide");
            if (impirmirCierreCaja && responseJson.object.nombreImpresora != '') {

                swal("Exitoso!", "Imprimiendo cierre de turno!", "success");
                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);
            }

            location.reload();

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataCerrarTurno").find("div.modal-content").LoadingOverlay("hide")
    })

})

$('#modalDataCerrarTurno').on('hidden.bs.modal', function () {
    $("#txtError").text('');
    const divError = document.getElementById("divError");
    divError.classList.remove('d-flex');
    divError.style.display = 'none';
    document.getElementById("btnFinalizarTurno").style.display = 'none';
    document.getElementById("divSwitchCierreCaja").style.display = 'none';

    errorCerrarTurno = false;
    $("#txtObservacionesCierre").prop("disabled", false);
});


function obtenerValoresInputsCerrarTurno() {

    const inputs = $("input.validate-importe").map(function () {
        return $(this).val();  // Obtener el valor de cada input
    }).get();  // Convertir a un array

    const inputs_without_value = inputs.filter(value => value.trim() == "");

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar todos los medios de pagos`;
        toastr.warning(msg, "");
        return null;
    }

    let valores = [];

    $('#contMetodosPagoLayout input[type="number"]').each(function () {
        let inputId = $(this).attr('id');
        let label = inputId.substring(3);
        let valor = parseFloat($(this).val()) || 0;

        if (valor != 0) {
            let ventaPorTipo = {
                Descripcion: label,
                Total: valor
            };

            valores.push(ventaPorTipo);
        }
    });

    return valores;
}

$("#bntBilletes").on("click", function () {
    $("#modalBilletes").modal("show");
})
