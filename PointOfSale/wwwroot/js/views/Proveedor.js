let tableData;
let rowSelected;
let tableDataMovimientos;
const monthNames = ["Ene", "Feb", "Mar", "Apr", "May", "Jun",
    "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
];

const BASIC_MODEL = {
    idProveedor: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    registrationDate: null,
    modificationDate: null,
    modificationUser: null
}

const BASIC_MODEL_PAGO = {
    idProveedor: 0,
    tipoFactura: null,
    nroFactura: null,
    iva: 0,
    ivaImporte: 0,
    importe: 0,
    importeSinIva: 0,
    comentario: null
}

$(document).ready(function () {

    showLoading();

    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetProveedores",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idProveedor",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            { "data": "cuil" },
            { "data": "telefono" },
            {
                "defaultContent":
                    '<button class="btn btn-info btn-sm btn-pago"><i class="mdi mdi-cash-usd"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "100px",
                "className": "text-center"
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
                filename: 'Reporte Proveedors',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });

    cargarTablaDinamica();
    removeLoading();
})

const openModal = (model = BASIC_MODEL) => {
    $("#txtId").val(model.idProveedor);
    $("#txtNombre").val(model.nombre);
    $("#txtCuil").val(model.cuil);
    $("#txtDireccion").val(model.direccion);
    $("#txtTelefono").val(model.telefono);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    if (tableDataMovimientos != null)
        tableDataMovimientos.destroy();

    var url = "/Admin/GetMovimientoProveedor?idProveedor=" + model.idProveedor;

    tableDataMovimientos = $("#tbMovimientos").DataTable({
        responsive: true,
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idProveedorMovimiento",
                "visible": false,
                "searchable": false
            },
            { "data": "importeString" },
            { "data": "registrationDateString" },
            { "data": "tipoFacturaString" },
            { "data": "nroFactura" },
            { "data": "registrationUser" }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Clientes',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });


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

    const model = structuredClone(BASIC_MODEL);
    model["nombre"] = $("#txtNombre").val();
    model["direccion"] = $("#txtDireccion").val();
    model["cuil"] = $("#txtCuil").val();
    model["telefono"] = $("#txtTelefono").val();
    model["idProveedor"] = $("#txtId").val();

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idProveedor == 0) {
        fetch("/Admin/CreateProveedor", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                tableData.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "Proveedor fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateProveedor", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelected).data(responseJson.object).draw(false);
                rowSelected = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "Proveedor fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbData tbody").on("click", ".btn-pago", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();

    openModalPago(data);
})


$("#btnSavePago").on("click", function () {
    const inputs = $("input.input-validate.pagoValid").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_PAGO);
    model["idProveedor"] = $("#txtIdProveedor").val();
    model["tipoFactura"] = $("#cboTipoFactura").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
    model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
    model["importe"] = $("#txtImporte").val();
    model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;
    model["comentario"] = $("#txtComentario").val();

    $("#modalPago").find("div.modal-content").LoadingOverlay("show")

    fetch("/Admin/RegistrarPagoProveedor", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {
            $("#modalPago").modal("hide");
            swal("Exitoso!", "Proveedor fué creada", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
    })


})

const openModalPago = (model = BASIC_MODEL_PAGO) => {
    $("#txtIdProveedor").val(model.idProveedor);
    $("#txtNombrePago").val(model.nombre);
    $("#txtCuilPago").val(model.cuil);
    $("#txtDireccionPago").val(model.direccion);

    $("#modalPago").modal("show")
}

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();

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
        text: `Eliminar Proveedor "${data.nombre}"`,
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

                fetch(`/Admin/DeleteProveedor?idProveedor=${data.idProveedor}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "Proveedor  fué eliminada", "success");

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


function calcularImportes() {
    var importe = $("#txtImporte").val();
    var iva = $("#txtIva").val();

    if (importe !== '' && iva !== '') {

        var importeSinIva = parseFloat(importe) * (1 - (parseFloat(iva) / 100));
        $("#txtImporteSinIva").val(importeSinIva);
        $("#txtImporteIva").val(importe - importeSinIva);
    }
}

function cargarTablaDinamica() {
    fetch(`/Admin/GetProveedorTablaDinamica`, {
        method: "GET"
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.data !== []) {

            var today = new Date();
            var month = "fecha.Month." + monthNames[today.getMonth()];
            var year = "fecha.Year." + today.getFullYear();

            var pivot = new WebDataRocks({
                container: "#wdr-component",
                toolbar: true,
                report: {
                    dataSource: {
                        data: responseJson.data
                    },
                    formats: [
                        {
                            name: "currency",
                            currencySymbol: "$"
                        }
                    ],
                    "slice": {
                        reportFilters: [
                            {
                                uniqueName: "fecha.Day"
                            },
                            {
                                uniqueName: "fecha.Month",
                                "filter": {
                                    "members": [
                                        month
                                    ]
                                }
                            },
                            {
                                uniqueName: "fecha.Year",
                                "filter": {
                                    "members": [
                                        year
                                    ]
                                }
                            }
                        ],
                        "rows": [
                            {
                                "uniqueName": "nombre_Proveedor",
                                sort: "desc"
                            },
                            {
                                "uniqueName": "tipo_Factura",
                                sort: "desc"
                            },
                            {
                                "uniqueName": "nro_Factura",
                                sort: "desc"
                            }
                        ],
                        columns: [
                            {
                                uniqueName: "[Measures]"
                            }
                        ],
                        measures: [
                            {
                                uniqueName: "importe",
                                caption: "Importe",
                                aggregation: "sum",
                                active: true,
                                format: "currency"
                            },
                            {
                                uniqueName: "importe_Sin_Iva",
                                caption: "Importe sin IVA",
                                aggregation: "sum",
                                active: true,
                                format: "currency"
                            },
                            {
                                uniqueName: "iva",
                                caption: "IVA",
                                aggregation: "sum",
                                active: true,
                                format: "currency"
                            },
                            {
                                uniqueName: "iva_Importe",
                                caption: "Importe IVA",
                                aggregation: "sum",
                                active: true,
                                format: "currency"
                            }
                        ]
                    }
                },
                global: {
                    localization: "https://cdn.webdatarocks.com/loc/es.json"
                }
            });

        }
    })
}