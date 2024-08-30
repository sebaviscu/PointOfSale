let tableMovimientoCaja;
let rowSelectedMovimientoCaja;

const BASIC_MODEL_RAZON_MOVIMIENTIO_CAJA = {
    idRazonMovimientoCaja: 0,
    descripcion: '',
    tipo: 0,
    estado: 0
}


$(document).ready(function () {
    
    let url = `/MovimientoCaja/GetMovimientosCaja`;

    tableMovimientoCaja = $("#tbData").DataTable({
        responsive: true,
        "columnDefs": [
            {
                "targets": [1],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idMovimientoCaja",
                "visible": false,
                "searchable": false
            },
            { "data": "registrationDate" },
            { "data": "registrationUser" },
            { "data": "razonMovimientoCaja.tipoString" },
            {
                "data": "importe",
                "render": function (data, type, row) {
                    return '$' + parseFloat(data).toFixed(0);
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-view btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "50px"
            }
        ],
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Movimientos de Caja',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });
})

$("#btnNewRazon").on("click", function () {
    $("#modalRazon").modal("show")
})


$("#btnSave").on("click", function () {
    const inputs = $("input.input-validate-TipoDeGastos").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if ($("#txtDescripcion").val().length < 10){
        toastr.warning("La descripción debe ser mayor a 10 caracteres", "");
    }

    const model = structuredClone(BASIC_MODEL_RAZON_MOVIMIENTIO_CAJA);
    model["idRazonMovimientoCaja"] = parseInt($("#txtIdRazon").val());
    model["descripcion"] = $("#txtDescripcion").val();
    model["tipo"] = parseInt($("#cboTipoMovimientoCaja").val());
    model["estado"] = parseInt($("#cboState").val());


    $("#modalRazon").find("div.modal-content").LoadingOverlay("show")


    if (model.idRazonMovimientoCaja == 0) {
        fetch("/MovimientoCaja/CreateRazonMovimientoCaja", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalRazon").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalRazon").find("div.modal-content").LoadingOverlay("hide")
        })
    }
    else
    {

    }
})

$("#tbData tbody").on("click", ".btn-view", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedMovimientoCaja = $(this).closest('tr').prev();
    } else {
        rowSelectedMovimientoCaja = $(this).closest('tr');
    }

    const data = tableMovimientoCaja.row(rowSelectedMovimientoCaja).data();

    openModalMovimientoCaja(data);
})


const openModalMovimientoCaja = (model = BASIC_MODEL_MOVIMIENTIO_CAJA) => {
    cargarRazones();
    disablesMovimientoCajaModal(true);


    $("#IdMovimientoCaja").val(model.idMovimientoCaja);
    $("#txtComentarioMovimientoCaja").val(model.comentario);
    $("#txtFechaMovimientoCaja").val(moment(model.registrationDate).format('DD/MM/YYYY HH:mm:ss'));
    $("#txtUsuarioMovimientoCaja").val(model.registrationUser);
    $("#txtImporteMovimientoCaja").val(model.importe);
    $("#cboRazonMovimiento").val(model.idRazonMovimientoCaja);
    $("#cboTipoRazonMovimiento").val(model.razonMovimientoCaja.tipo);

    $("#modalMovimientoCaja").modal("show")
}
