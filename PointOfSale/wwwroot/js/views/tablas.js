let tableTipoGastos;
let rowSelectedTipoGastos;
let tableFormatoVenta;
let rowSelectedFormatoVenta;

$(document).ready(function () {


    tableTipoGastos = $("#tbTipoGastos").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Gastos/GetTipoDeGasto",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTipoGastos",
                "visible": false,
                "searchable": false
            },
            { "data": "descripcion" },
            { "data": "gastoParticularString" },
            {
                "data": "iva",
                "render": function (data, type, row) {
                    return data != null ? data + ' %' : '0 %' ;
                }
            },
            { "data": "tipoFacturaString" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit-tipo-gastos btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete-tipo-gastos btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Tipo de Gastos',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });

    tableFormatoVenta = $("#tbFormatoVenta").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Tablas/GetFormtadoVenta",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idFormatosVenta",
                "visible": false,
                "searchable": false
            },
            { "data": "formato" },
            { "data": "valor" },
            {
                "data": "estado", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit-formato-venta btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete-formato-venta btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Formatos de Venta',
                exportOptions: {
                    columns: [1, 2,3]
                }
            }, 'pageLength'
        ]
    });

})



$('#tbTipoGastos').on('click', '.btn-delete-tipo-gastos', function () {
    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableTipoGastos.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Tipo de Gastos "${data.nombre}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Inventory/DeleteTag?idTag=${data.idTag}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableTipoGastos.row(row).remove().draw();
                        swal("Exitoso!", "Tipo de Gastos fué eliminado", "success");

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                })
                    .catch((error) => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                    })
            }
        });
});

$('#tbTipoGastos').on('click', '.btn-edit-tipo-gastos', function () {
    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTipoGastos = $(this).closest('tr').prev();
    } else {
        rowSelectedTipoGastos = $(this).closest('tr');
    }

    const data = tableTipoGastos.row(rowSelectedTipoGastos).data();

    $("#txtIdTipoGastos").val(data.idTipoGastos);
    $("#txtIvaTipoGasto").val(data.iva);
    $("#cboTipoDeGasto").val(data.gastoParticular);
    $("#txtDescripcionTipoDeGasto").val(data.descripcion);
    $("#cboTipoFacturaTipoGasto").val(data.tipoFactura);

    $("#modalDataTipoDeGasto").modal("show")
});

$("#btnSaveTipoDeGastos").on("click", function () {
    const inputs = $("input.input-validate-TipoDeGastos").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_TIPO_DE_GASTOS);
    model["idTipoGastos"] = parseInt($("#txtIdTipoGastos").val());
    model["gastoParticular"] = $("#cboTipoDeGasto").val();
    model["descripcion"] = $("#txtDescripcionTipoDeGasto").val();
    model["tipoFactura"] = parseInt($("#cboTipoFacturaTipoGasto").val());
    model["iva"] = $("#txtIvaTipoGasto").val() != '' ? parseInt($("#txtIvaTipoGasto").val()) : 0;

    $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("show")


    if (model.IdTipoGastos == 0) {
        fetch("/Gastos/CreateTipoDeGastos", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }
})


$('#tbFormatoVenta').on('click', '.btn-edit-formato-venta', function () {
    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedFormatoVenta = $(this).closest('tr').prev();
    } else {
        rowSelectedFormatoVenta = $(this).closest('tr');
    }

    const data = tableFormatoVenta.row(rowSelectedFormatoVenta).data();

    $("#txtIdFormatoVenta").val(data.idFormatosVenta);
    $("#txtDescripcionFormatoVenta").val(data.formato);
    $("#txtValorFormatoVenta").val(data.valor);
    $("#cboStateFormatoVenta").val(data.estado ? 1 : 0);

    $("#modalFormatoVenta").modal("show")
});
