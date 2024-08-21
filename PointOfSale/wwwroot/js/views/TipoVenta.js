let tableData;
let rowSelectedFormaPago;

const BASIC_MODEL_TIPO_VENTA = {
    idTypeDocumentSale: 0,
    description: "",
    isActive: 1,
    web: 1,
    tipoFactura: 0
}


$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetTipoVenta",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTypeDocumentSale",
                "visible": false,
                "searchable": false
            },
            { "data": "description" },
            { "data": "tipoFacturaString" },
            {
                "data": "web", render: function (data) {
                    if (data == 1)
                        return '<input type="checkbox" checked disabled>';
                    else
                        return '<input type="checkbox" disabled>';
                }
            },
            {
                "data": "comision",
                "render": function (data, type, row) {
                    return data ? data + ' %' : '';
                }
            },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Formas de Pago',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });
})

const openModal = (model = BASIC_MODEL_TIPO_VENTA) => {
    $("#txtId").val(model.idTypeDocumentSale);
    $("#txtNombre").val(model.description);
    $("#cboState").val(model.isActive ? 1 : 0);
    $("#cboTipoFactura").val(model.tipoFactura);
    $("#cboWeb").val(model.web ? 1 : 0);
    $("#txtComision").val(model.comision);

    document.querySelector('#cboWeb').checked = model.web


    $("#modalData").modal("show")
}

$("#btnNew").on("click", function () {
    openModal()
})

$("#btnSave").on("click", function () {
    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_TIPO_VENTA);
    model["idTypeDocumentSale"] = parseInt($("#txtId").val());
    model["description"] = $("#txtNombre").val();
    model["isActive"] = $("#cboState").val() === '1' ? true : false;
    model["web"] = document.querySelector('#cboWeb').checked;
    model["tipoFactura"] = parseInt($("#cboTipoFactura").val());
    model["comision"] = $("#txtComision").val();

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idTypeDocumentSale == 0) {
        fetch("/Admin/CreateTipoVenta", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableData.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "Tipo de Venta fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateTipoVenta", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelectedFormaPago).data(responseJson.object).draw(false);
                rowSelectedFormaPago = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "Tipo de Venta fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedFormaPago = $(this).closest('tr').prev();
    } else {
        rowSelectedFormaPago = $(this).closest('tr');
    }

    const data = tableData.row(rowSelectedFormaPago).data();

    openModal(data);
})



$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableData.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Tipo de Venta "${data.nombre}"`,
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

                fetch(`/Admin/DeleteTipoVenta?idTypeDocumentSale=${data.idTypeDocumentSale}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "Tipo de Venta  fué eliminada", "success");

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                })
                    .catch((error) => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                    })
            }
        });
})