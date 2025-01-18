let billetesArray = [];


$(document).ready(function () {


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

                        $("#txtInicioTurnoCierreSimple").val(fechaFormatted);
                        $("#txtHoraInicioTurnoCierreSimple").val(horaInicioFormatted);
                    }

                    if (fechaFin != '' && fechaFin.isValid()) {
                        let horaFinFormatted = fechaFin.format('HH:mm');

                        $("#txtCierraTurnoCierreSimple").val(horaFinFormatted);
                    }

                    if (resultado.observacionesApertura != '') {
                        $("#txtObservacionesAperturaSimple").val(resultado.observacionesApertura);
                        $('#divObservacionesAperturaSimple').css('display', '');
                    }
                    else {
                        $('#divObservacionesAperturaSimple').css('display', 'none');
                    }
                    $("#modalDataCerrarTurnoSimple").modal("show");

                    let contenedor = $("#contMetodosPagoLayoutSimple");


                    renderVentasPorTipoVenta(contenedor, resultado.ventasPorTipoVentaPreviaValidacion, resultado.totalInicioCaja, resultado.validacionRealizada, responseJson.object.totalMovimientosCaja);

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
    let total = renderVentasPorTipoVenta(contenedor, ventasPorTipoVenta, importeInicioCaja, turnoCerrado, totalMovimientosCaja);
    return total;
}

function renderVentasPorTipoVenta(contenedor, ventasPorTipoVenta, importeInicioCaja, turnoCerrado, totalMovimientosCaja = null) {
    contenedor.empty();

    crearFilaTablaTurno(contenedor, "TOTAL INICIO CAJA", importeInicioCaja, true, "txtInicioCajaCierreSimple");

    if (totalMovimientosCaja != null && totalMovimientosCaja != 0) {
        crearFilaTablaTurno(contenedor, "MOV. DE CAJA", totalMovimientosCaja, true, 'txtMovimientoCajaSimple');
    }

    contenedor.append($('<hr style="margin-top: 0px;">'));

    let total = 0;
    ventasPorTipoVenta.forEach(function (venta) {
        let totalFormaPAgo = turnoCerrado ? parseFloat(venta.totalUsuario) : parseFloat(venta.total);
        total += totalFormaPAgo;
        let id = 'txt' + venta.descripcion;

        crearFilaTablaTurno(contenedor, venta.descripcion, totalFormaPAgo, true, id);
    });

    contenedor.append($('<hr style="margin-top: 0px;">'));

    crearFilaTablaTurno(contenedor, "TOTAL", total, true, "txtTotalCierreCajaSimple");

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

    if (inputId != 'txtTotalCierreCajaSimple' && inputId != 'txtMovimientoCajaSimple' && inputId != 'txtInicioCajaCierreSimple') {
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

    inputDiv.append(inputGroup);
    formGroup.append(label).append(inputDiv);
    contenedor.append(formGroup);
}

function actualizarTotal() {
    let total = 0;
    $('#contMetodosPagoLayoutSimple input[type="number"]').each(function () {
        if ($(this).attr('id') != 'txtTotalSumado') {
            let value = parseFloat($(this).val()) || 0;
            total += value;
        }

    });
    $('#txtTotalSumado').val(total.toFixed(0));
}




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

$("#btnFinalizarTurnoSimple").on("click", function () {

    let listaVentas = obtenerValoresInputsCerrarTurnoSimple();
    if (listaVentas == null) {
        return;
    }

    let impirmirCierreCaja = document.getElementById('switchCierreCajaSimple').checked;

    let totalCierreCajaReal = $("#txtTotalCierreCajaSimple").val();

    let cajaUsuario = $("#txtTotalDiferenciaCaja").val();

    if (cajaUsuario == null || cajaUsuario == 0) {
        cajaUsuario = totalCierreCajaReal;
    }

    let modelTurno = {
        observacionesCierre: $("#txtObservacionesCierre").val(),
        idTurno: parseInt($("#txtIdTurnoLayout").val()),
        ventasPorTipoVentaPreviaValidacion: listaVentas,
        impirmirCierreCaja: impirmirCierreCaja,
        totalCierreCajaSistema: totalCierreCajaReal,
        totalCierreCajaReal: cajaUsuario,
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


function obtenerValoresInputsCerrarTurnoSimple() {
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

    $('#contMetodosPagoLayoutSimple .validate-importe').each(function () {
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

$("#btnGuardarBilletes").on("click", function () {
    let totalBilletes = $("#totalSumBilletes").val();

    $("#txtEfectivo").val(parseInt(totalBilletes));

    actualizarTotal();


    $(".txtCantBillete").each(function () {

        const cantidad = $(this).val();
        const valorNominal = $(this).data('valor');

        if (cantidad > 0) {
            billetesArray.push({ valorNominal, cantidad });
        }
    });

    $("#modalBilletes").modal("hide");

})
