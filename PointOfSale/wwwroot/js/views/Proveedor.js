let tableData;
let rowSelected;
var tableDataMovimientos;
let tableDataGastos;

const monthNames = ["Ene", "Feb", "Mar", "Abr", "May", "Jun",
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
    comentario: null,
    estadoPago: 0
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

    cargarTablaGastos();
    cargarTablaDinamica();

    fetch("/Admin/GetProveedores")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.data.length > 0) {
                proveedoresList = responseJson.data;

                responseJson.data.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });
            }
        })

    $(document).on("click", 'span.btn-pago-pagado', function (e) {
        let row;

        if ($(this).closest('tr').hasClass('child')) {
            row = $(this).closest('tr').prev();
        } else {
            row = $(this).closest('tr');
        }
        var data;

        if (tableDataMovimientos == undefined)
            data = tableDataGastos.row(row).data();
        else
            data = tableDataMovimientos.row(row).data();



        swal({
            title: "¿Desea cambiar el estado a Pagado? ",
            text: ` \n Factura: ${data.tipoFacturaString} ${data.nroFactura} \n Importe: ${data.importeString}  \n  \n `,
            type: "warning",
            showCancelButton: true,
            confirmButtonClass: "btn-danger",
            confirmButtonText: "Si, cambiar estado",
            cancelButtonText: "No, cancelar",
            closeOnConfirm: false,
            closeOnCancel: true
        },
            function (respuesta) {

                if (respuesta) {

                    $(".showSweetAlert").LoadingOverlay("show")

                    fetch(`/Admin/UpdatePagoProveedor?idMovimiento=${data.idProveedorMovimiento}`, {
                        method: "PUT"
                    }).then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                        return response.ok ? response.json() : Promise.reject(response);
                    }).then(responseJson => {
                        if (responseJson.state) {

                            location.reload()

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
    removeLoading();
})


$("#btnNuevoGasto").on("click", function () {
    $("#modalPago").modal("show")
})

$('#cboProveedor').change(function () {
    var idProv = $(this).val();
    var proveedor = proveedoresList.find(_ => _.idProveedor == idProv);

    if (proveedor != null) {
        $("#txtCuilPago").val(proveedor.cuil);
        $("#txtDireccionPago").val(proveedor.direccion);
    }
    else {
        $("#txtCuilPago").val('');
        $("#txtDireccionPago").val('');
    }
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
            { "data": "registrationDateString" },
            { "data": "importeString" },
            { "data": "tipoFacturaString" },
            { "data": "nroFactura" },
            {
                "data": "estadoPago",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-success">Pagado</span>';
                    else
                        return '<span class="btn btn-pago-pagado badge rounded-pill bg-warning text-dark">Pendiente</span>';
                }
            },
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

$("#btnSavePago").on("click", function () {
    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if ($("#cboProveedor").val() == '') {
        const msg = `Debe seleccionar un proveedor`;
        toastr.warning(msg, "");
        return;
    }

    const model = structuredClone(BASIC_MODEL_PAGO);
    //model["idProveedor"] = $("#txtIdProveedor").val();
    model["idProveedor"] = $("#cboProveedor").val();
    model["tipoFactura"] = $("#cboTipoFactura").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
    model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
    model["importe"] = $("#txtImporte").val();
    model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;
    model["comentario"] = $("#txtComentario").val();
    model["estadoPago"] = parseInt($("#cboEstado").val());

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


function cargarTablaGastos() {
    tableDataGastos = $("#tbDataGastos").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetAllMovimientoProveedor",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idProveedorMovimiento",
                "visible": false,
                "searchable": false
            },
            { "data": "registrationDateString" },
            { "data": "nombreProveedor" },
            { "data": "tipoFacturaString" },
            { "data": "nroFactura" },
            { "data": "comentario" },
            {
                "data": "estadoPago",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-success">Pagado</span>';
                    else
                        return '<span class="btn btn-sm btn-pago-pagado rounded-pill bg-warning text-dark">Pendiente</span>';
                }
            },
            { "data": "importeString", "className": "text-center" }
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