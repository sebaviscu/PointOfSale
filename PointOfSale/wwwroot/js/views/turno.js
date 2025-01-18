let tableDataTurno;
let rowSelectedTurno;
let isHealthySaleHistory = false;
let billetesArrayTurno = null;

const BASIC_MODEL_TURNO = {
    idturno: 0,
    descripcion: "",
    fecha: "",
    horaInicio: "",
    horaFin: ""
}


$(document).ready(function () {


    tableDataTurno = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/turno/GetTurnos",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [1],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idTurno",
                "visible": false,
                "searchable": false
            },
            { "data": "fechaInicio" },
            { "data": "horaInicio" },
            { "data": "horaFin" },
            { "data": "modificationUser" },
            { "data": "total" },
            {
                "data": "diferenciaCierreCaja", render: function (data) {
                    return '$ ' + data;
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-sm me-2 btn-edit"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, 2, 3, 4, 5, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Turnos',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    healthcheck();
})


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTurno = $(this).closest('tr').prev();
    } else {
        rowSelectedTurno = $(this).closest('tr');
    }

    const data = tableDataTurno.row(rowSelectedTurno).data();

    openModalTurno(data);
})


const openModalTurno = (model = BASIC_MODEL_TURNO) => {
    showLoading();

    fetch(`/Turno/GetTurno?idTurno=` + model.idTurno, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                let result = responseJson.object.turno;

                $("#txtIdTurnoLayout").val(result.idTurno);

                let fechaInicio = moment(result.fechaInicio);
                let fechaFin = result.fechaFin
                    ? moment(result.fechaFin)
                    : '';

                if (fechaInicio.isValid()) {
                    let fechaFormatted = fechaInicio.format('DD/MM/YYYY');
                    let horaInicioFormatted = fechaInicio.format('HH:mm');

                    $("#txtFecha").val(fechaFormatted);
                    $("#txtHoraInicio").val(horaInicioFormatted);
                }

                if (fechaFin != '') {
                    let horaFinFormatted = fechaFin.format('HH:mm');

                    $("#txtHoraCierre").val(horaFinFormatted);
                }

                if (result.observacionesApertura != '') {
                    $("#txtObservacionesAperturaTurno").val(result.observacionesApertura);
                    $('#divObservacionesApertura').css('display', '');
                }
                else {
                    $('#divObservacionesApertura').css('display', 'none');
                }

                if (result.observacionesCierre != '') {
                    $("#txtObservacionesCierreTurno").val(result.observacionesCierre);
                    $('#divObservacionesCierre').css('display', '');
                }
                else {
                    $('#divObservacionesCierre').css('display', 'none');
                }



                let contenedor = $("#contMetodosPagoLayoutList");

                renderVentasPorTipoVentaTurno(contenedor, result.ventasPorTipoDeVenta, result.ventasPorTipoVentaPreviaValidacion, result.totalInicioCaja, result.validacionRealizada, responseJson.object.totalMovimientosCaja);

                contenedor.append($('<hr>'));

                $('#divValidacionRealizada').css('display', result.validacionRealizada != '' ? '' : 'none');

                crearFilaTabla(contenedor, "TOTAL", responseJson.object.turno.totalCierreCajaReal, responseJson.object.turno.totalCierreCajaSistema, true, "txtTotal");
                if (responseJson.object.turno.erroresCierreCaja != null && responseJson.object.turno.erroresCierreCaja != "") {

                    $("#txtErrorTurno").html(responseJson.object.turno.erroresCierreCaja);
                    const divError = document.getElementById("divErrorTurno");
                    divError.classList.add('d-flex');
                    divError.style.display = '';
                }
                else {
                    $("#txtErrorTurno").text('');
                    const divError = document.getElementById("divErrorTurno");
                    divError.classList.remove('d-flex');
                    divError.style.display = 'none';
                }

                billetesArrayTurno = responseJson.object.turno.billetesEfectivo != null && responseJson.object.turno.billetesEfectivo != "" ? JSON.parse(responseJson.object.turno.billetesEfectivo) : [];

                $("#btnFinalizar").hide();
                $("#modalDataTurno").modal("show");

                if (responseJson.object.controlCierreCaja) {
                    $('.input-usuario').css('display', '');
                    $('#titles-sales').css('display', '');
                }
                else {
                    $('.input-usuario').css('display', 'none');
                    $('#titles-sales').css('display', 'none');
                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
            removeLoading();
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
}

function renderVentasPorTipoVentaTurno(contenedor, ventasPorTipoVenta, ventasPorTipoVentaPreviaValidacion, importeInicioCaja, turnoCerrado, totalMovimientosCaja = null) {
    contenedor.empty();

    crearFilaTabla(contenedor, "TOTAL INICIO CAJA", importeInicioCaja, importeInicioCaja, true, "txtInicioCajaCierre");

    if (totalMovimientosCaja != null && totalMovimientosCaja != 0) {
        crearFilaTabla(contenedor, "MOV. DE CAJA", totalMovimientosCaja, totalMovimientosCaja, true, 'txtMovimientoCaja');
    }

    contenedor.append($('<hr style="margin-top: 0px;">'));

    let total = 0;
    ventasPorTipoVenta.forEach(function (venta) {

        total += parseFloat(venta.totalUsuario);
        let id = 'txt' + venta.descripcion;
        let totalVenta = turnoCerrado ? parseFloat(venta.totalUsuario) : '';

        let ventaValidacion = ventasPorTipoVentaPreviaValidacion.find(_ => _.descripcion == venta.descripcion);
        let totalValidacion = turnoCerrado ? parseFloat(ventaValidacion.total) : '';

        crearFilaTabla(contenedor, venta.descripcion, totalVenta, totalValidacion, turnoCerrado, id);
    });

    return total;
}


function crearFilaTabla(contenedor, descripcion, total, totalValidacion, disabled, inputId = null) {
    // Crear el grupo de la fila
    let formGroup = $('<div>', { class: 'form-group row align-items-center', style: 'margin-bottom:10px;' });

    // Crear títulos para las columnas
    let titlesRow = $(
        '<div class="row" id="titles-sales" style="width: 100%; margin-bottom: 5px;">' +
        '   <div class="offset-sm-4 col-sm-5 text-center"><strong>Usuario</strong></div>' +
        '   <div class="col-sm-3 text-center"><strong>Sistema</strong></div>' +
        '</div>'
    );

    // Agregar títulos al contenedor si aún no están presentes
    if (contenedor.find('.row').length === 0) {
        contenedor.append(titlesRow);
    }

    // Crear el label para la descripción
    let label = $('<label>', {
        class: 'col-sm-4 col-form-label',
        text: descripcion + ":",
        style: 'font-size: 20px; padding-right: 0px; padding-top: 0px; text-align: left;'
    });

    // Contenedor para los inputs
    let inputDiv = $('<div>', { class: 'col-sm-8 d-flex justify-content-between', style: 'padding-right: 0px; padding-left: 0px;' });

    // Grupo para el input total (Usuario)
    let inputGroupTotal = $('<div>', { class: 'input-group input-group-sm input-usuario', style: 'width: 48%;' });

    let inputGroupPrependTotal = $('<div>', { class: 'input-group-prepend' });

    let spanTotal = $('<span>', {
        class: 'input-group-text',
        text: '$'
    });

    let inputTotal = $('<input>', {
        type: 'number',
        step: 'any',
        class: 'form-control form-control-sm',
        min: '0',
        value: total,
        disabled: disabled,
        style: 'text-align: end;',
        id: inputId ? inputId : null
    });


    inputGroupPrependTotal.append(spanTotal);
    inputGroupTotal.append(inputGroupPrependTotal).append(inputTotal);

    // Botón para contar billetes si la descripción es "efectivo"
    if (descripcion.toLowerCase() === "efectivo") {
        let inputGroupAppend = $('<div>', { class: 'input-group-append' });

        let button = $('<button>', {
            id: 'btnBilletes',
            type: 'button',
            class: 'btn btn-outline-success mdi mdi-cash',
            style: 'padding: 0 10px;',
            title: 'Contar billetes',
            'data-toggle': 'tooltip',
            'data-descripcion': descripcion,
            'data-total': total,
            click: function () {
                $('#modalBilletes').modal('show');
            }
        });

        inputGroupAppend.append(button);
        inputGroupTotal.append(inputGroupAppend);
    }

    // Grupo para el input totalValidacion (Sistemas)
    let inputGroupValidacion = $('<div>', { class: 'input-group input-group-sm input-sistema', style: 'width: 48%; margin-left: 20px;' });

    let inputGroupPrependValidacion = $('<div>', { class: 'input-group-prepend' });

    let spanValidacion = $('<span>', {
        class: 'input-group-text',
        text: '$'
    });

    let inputValidacion = $('<input>', {
        type: 'number',
        step: 'any',
        class: 'form-control form-control-sm',
        min: '0',
        value: totalValidacion,
        disabled: disabled,
        style: 'text-align: end;',
        id: inputId ? inputId + '_validacion' : null
    });

    inputGroupPrependValidacion.append(spanValidacion);
    inputGroupValidacion.append(inputGroupPrependValidacion).append(inputValidacion);

    // Agregar ambos grupos al div de inputs
    inputDiv.append(inputGroupTotal).append(inputGroupValidacion);

    // Construir la fila final
    formGroup.append(label).append(inputDiv);
    contenedor.append(formGroup);
}

$('#modalBilletes').on('show.bs.modal', function (event) {

    if (billetesArrayTurno.length > 0) {
        billetesArrayTurno.forEach(billete => {
            // Buscar el input correspondiente por el atributo data-valor
            let inputCantidad = $(`[data-valor="${billete.valorNominal}"]`);

            // Verificar si se encontró el input y actualizar el valor
            if (inputCantidad.length) {
                inputCantidad.val(billete.cantidad);
            }
        });

        $(".txtCantBillete").each(function () {

            $(this).prop('disabled', true).trigger('input');
        });
    }

    $("#btnGuardarBilletes").hide()

});

$('#modalBilletes').on('hidden.bs.modal', function (event) {
    $("#btnGuardarBilletes").show()

    $(".txtCantBillete").each(function () {

        $(this).val('0');
        $(this).disabled = false;
    });

});

//function crearFilaTabla2(contenedor, descripcion, total, totalValidacion, disabled, inputId = null) {
//    let formGroup = $('<div>', { class: 'form-group row align-items-center', style: 'margin-bottom:2px' });

//    let label = $('<label>', {
//        class: 'col-sm-7 col-form-label',
//        text: descripcion + ":",
//        style: 'font-size: 20px; padding-right: 0px; padding-top: 0px;'
//    });

//    let inputDiv = $('<div>', { class: 'col-sm-5', style: 'padding-right: 0px; padding-left: 0px;' });


//    let inputGroup = $('<div>', { class: 'input-group input-group-sm', style: 'margin-top: 0px;' });

//    let inputGroupPrepend = $('<div>', { class: 'input-group-prepend' });

//    let span = $('<span>', {
//        class: 'input-group-text',
//        text: '$'
//    });

//    let classInput = 'form-control form-control-sm';

//    if (total == '' && total != '0') {
//        classInput += ' validate-importe';
//    }

//    let inputAttributes = {
//        type: 'number',
//        step: 'any',
//        class: classInput,
//        min: '0',
//        value: total,
//        disabled: disabled,
//        style: 'text-align: start;'
//        //,change: function () { actualizarTotal(); }
//    };

//    if (inputId != '') {
//        inputAttributes.id = inputId;
//    }

//    let input = $('<input>', inputAttributes);

//    inputGroupPrepend.append(span);
//    inputGroup.append(inputGroupPrepend).append(input);

//    if (descripcion.toLowerCase() == "efectivo") {
//        let inputGroupAppend = $('<div>', { class: 'input-group-append' });

//        let button = $('<button>', {
//            id: 'btnBilletes',
//            type: 'button',
//            class: 'btn btn-outline-success mdi mdi-cash',
//            style: 'padding: 0 10px;',
//            title: 'Contar billetes',
//            'data-toggle': 'tooltip',
//            click: function () {
//                $('#modalBilletes').modal('show');
//            }
//        });

//        inputGroupAppend.append(button);
//        inputGroup.append(inputGroupAppend);
//    }

//    inputDiv.append(inputGroup);
//    formGroup.append(label).append(inputDiv);
//    contenedor.append(formGroup);
//}



$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL_TURNO);
    model["idTurno"] = parseInt($("#txtIdTurnoLayout").val());
    model["descripcion"] = $("#txtDescripcion").val();

    fetch("/Turno/UpdateTurno", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            tableDataTurno.row(rowSelectedTurno).data(responseJson.object).draw(false);
            rowSelectedTurno = null;
            $("#modalData").modal("hide");
            swal("Exitoso!", "Turno fué modificada", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
})

$("#btnImprimirCierreCaja").click(function () {
    showLoading();
    let idTurno = parseInt($("#txtIdTurnoLayout").val());

    fetch(`/Turno/ImprimirTicketCierre?idTurno=${idTurno}`, {
    }).then(response => {

        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {
            $("#modalData").modal("hide");

            if (isHealthySaleHistory && responseJson.object.nombreImpresora != '') {

                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);

                swal("Exitoso!", "Imprimiendo cierre de turno!", "success");
                removeLoading();
            }

        } else {
            swal("Lo sentimos", "Error al imprimir cierre de turno " + responseJson.message, "error");
        }
    })

})


async function healthcheck() {
    isHealthySaleHistory = await getHealthcheck();

    if (isHealthySaleHistory) {
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}