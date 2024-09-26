let tableDataTurno;
let rowSelectedTurno;
let isHealthySaleHistory = false;

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

                $("#btnFinalizar").hide();
                $("#btnValidarTurno").hide();
                $("#modalDataTurno").modal("show");

                let contenedor = $("#contMetodosPagoLayoutList");

                renderVentasPorTipoVenta(contenedor, result.ventasPorTipoVenta, result.totalInicioCaja, true);

                contenedor.append($('<hr>'));
                crearFilaTotalesTurno(contenedor, "TOTAL Sistema", responseJson.object.turno.totalCierreCajaSistema, true, "txtTotalSumado");
                crearFilaTotalesTurno(contenedor, "TOTAL Usuario", responseJson.object.turno.totalCierreCajaReal, true, "txtTotalSumado");
                $("#btnBilletes").hide()


                if (responseJson.object.turno.erroresCierreCaja != null && responseJson.object.turno.erroresCierreCaja != "") {

                    $("#txtError").html(responseJson.object.turno.erroresCierreCaja);
                    const divError = document.getElementById("divError");
                    divError.classList.add('d-flex');
                    divError.style.display = '';
                }
                else {
                    $("#txtError").text('');
                    const divError = document.getElementById("divError");
                    divError.classList.remove('d-flex');
                    divError.style.display = 'none';
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