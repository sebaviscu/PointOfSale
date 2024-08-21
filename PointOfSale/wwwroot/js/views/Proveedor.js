let tableData;
let rowSelectedProveedor;
let tableDataMovimientos;
let tableDataGastos;
let proveedoresList;

const monthNames = ["Ene", "Feb", "Mar", "Abr", "May", "Jun",
    "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
];

const BASIC_MODEL_PROVEEDOR = {
    idProveedor: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    nombreContacto: null,
    telefono2: null,
    email: null,
    web: null,
    comentario: null,
    iva: null,
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
    estadoPago: 0,
    facturaPendiente: 0,
    modificationDate: null,
    modificationUser: null
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
            { "data": "nombreContacto" },
            { "data": "telefono" },
            { "data": "web" },
            { "data": "comentario" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
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
                filename: 'Reporte Proveedors',
                exportOptions: {
                    columns: [1, 2, 3]
                }
            }, 'pageLength'
        ]
    });

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            document.getElementById("divSwitchVisionGlobalProveedores").style.display = responseJson.data != null && responseJson.data.length > 1 ? '' : 'none';

        })

    cargarTablaGastos(false);
    cargarTablaDinamica(false);

    fetch("/Admin/GetProveedores")
        .then(response => {
            return response.json();
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
        let data;

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

                    fetch(`/Admin/CambioEstadoPagoProveedor?idMovimiento=${data.idProveedorMovimiento}`, {
                        method: "PUT"
                    }).then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                        return response.json();
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

$('#switchVisionGlobalProveedores').change(function () {
    let visionGlobal = $(this).is(':checked')
    cargarTablaGastosProveedores(visionGlobal);
    cargarTablaDinamicaProveedores(visionGlobal);
});

$("#btnNuevoGasto").on("click", function () {
    $("#modalPago").modal("show")
})

$('#cboProveedor').change(function () {
    let idProv = $(this).val();
    let proveedor = proveedoresList.find(_ => _.idProveedor == idProv);

    if (proveedor != null) {
        $("#txtCuilPago").val(proveedor.cuil);
        $("#txtDireccionPago").val(proveedor.direccion);
        $("#txtIva").val(proveedor.iva != null ? proveedor.iva : '');
        $("#cboTipoFacturaPago").val(proveedor.tipoFactura ?? '');
    }
    else {
        $("#txtCuilPago").val('');
        $("#txtDireccionPago").val('');
        $("#txtIva").val('');
        $("#cboTipoFacturaPago").val('');
    }
})

const openModal = (model = BASIC_MODEL_PROVEEDOR) => {
    $("#txtId").val(model.idProveedor);
    $("#txtNombre").val(model.nombre);
    $("#txtCuil").val(model.cuil);
    $("#txtDireccion").val(model.direccion);
    $("#txtTelefono").val(model.telefono);

    $("#txtTelefono2").val(model.telefono2);
    $("#txtContacto").val(model.nombreContacto);
    $("#txtWeb").val(model.web);
    $("#txtEmail").val(model.email);
    $("#txtIvaProveedor").val(model.iva);
    $("#txtComentario").val(model.comentario);
    $("#cboTipoFactura").val(model.tipoFactura);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    if (tableDataMovimientos != null)
        tableDataMovimientos.destroy();

    let url = "/Admin/GetMovimientoProveedor?idProveedor=" + model.idProveedor;

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
                filename: 'Reporte Pago Proveedores',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5]
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


    const model = structuredClone(BASIC_MODEL_PROVEEDOR);
    model["idProveedor"] = $("#txtId").val();
    model["nombre"] = $("#txtNombre").val();
    model["direccion"] = $("#txtDireccion").val();
    model["cuil"] = $("#txtCuil").val();
    model["telefono"] = $("#txtTelefono").val();
    model["telefono2"] = $("#txtTelefono2").val();
    model["nombreContacto"] = $("#txtContacto").val();
    model["web"] = $("#txtWeb").val();
    model["email"] = $("#txtEmail").val();
    model["iva"] = $("#txtIvaProveedor").val();
    model["comentario"] = $("#txtComentario").val();
    model["tipoFactura"] = parseInt($("#cboTipoFactura").val());

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idProveedor == 0) {
        fetch("/Admin/CreateProveedor", {
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
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelectedProveedor).data(responseJson.object).draw(false);
                rowSelectedProveedor = null;
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
    const inputs = $("input.input-validate-pago").serializeArray();
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
    model["idProveedorMovimiento"] = parseInt($("#txtIdPagoProveedor").val());
    model["idProveedor"] = $("#cboProveedor").val();
    model["tipoFactura"] = $("#cboTipoFacturaPago").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
    model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
    model["importe"] = $("#txtImporte").val();
    model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;
    model["comentario"] = $("#txtComentarioPago").val();
    model["estadoPago"] = parseInt($("#cboEstado").val());
    model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;

    $("#modalPago").find("div.modal-content").LoadingOverlay("show")


    if (model.idProveedorMovimiento == 0) {
        fetch("/Admin/RegistrarPagoProveedor", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                tableDataGastos.row.add(responseJson.object).draw(false);

                $("#modalPago").modal("hide");
                swal("Exitoso!", "Pago a proveedor fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
        })

    } else {

        fetch("/Admin/UpdatePagoProveedor", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                tableDataGastos.row(rowSelectedProveedor).data(responseJson.object).draw(false);
                rowSelectedProveedor = null;
                $("#modalPago").modal("hide");
                swal("Exitoso!", "Pago a proveedor fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalPago").find("div.modal-content").LoadingOverlay("hide")
        })
    }
})

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedProveedor = $(this).closest('tr').prev();
    } else {
        rowSelectedProveedor = $(this).closest('tr');
    }

    const data = tableData.row(rowSelectedProveedor).data();

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
                    return response.json();
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


function cargarTablaGastosProveedores(isGlobal) {

    let url = `/Admin/GetAllMovimientoProveedor?visionGlobal=${isGlobal}`;

    if ($.fn.DataTable.isDataTable('#tbDataGastos')) {
        $('#tbDataGastos').DataTable().clear().destroy();  // Destruye la instancia existente
    }

    tableDataGastos = $("#tbDataGastos").DataTable({
        responsive: true,
        pageLength: 10,
        "rowCallback": function (row, data) {
            if (data.facturaPendiente == 1) {
                $('td:eq(2)', row).addClass('factura-pendiente');
            }
        },
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
            { "data": "nombreProveedor" },
            {
                "data": "nroFactura", render: function (data, type, row) {
                    return "<span>" + row.tipoFacturaString + " </span>" +
                        "<span> " + row.nroFactura + "</span>";
                }
            },
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
            { "data": "importeString", "className": "text-center" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit-pago btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete-pago btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "130px",
                "className": "text-center"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Gastos Proveedores',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });
}

$("#tbDataGastos tbody").on("click", ".btn-edit-pago", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedProveedor = $(this).closest('tr').prev();
    } else {
        rowSelectedProveedor = $(this).closest('tr');
    }

    const data = tableDataGastos.row(rowSelectedProveedor).data();

    openModalPago(data);
})


$("#tbDataGastos tbody").on("click", ".btn-delete-pago", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataGastos.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar el pago`,
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

                fetch(`/Admin/DeletePagoProveedor?idPagoProveedor=${data.idProveedorMovimiento}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataGastos.row(row).remove().draw();
                        swal("Exitoso!", "El pago a producto fué eliminado", "success");

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


const openModalPago = (model = BASIC_MODEL_PAGO) => {

    $("#txtIdPagoProveedor").val(model.idProveedorMovimiento);
    $("#txtCuilPago").val(model.proveedor.cuil);
    $("#txtDireccionPago").val(model.proveedor.direccion);
    $("#cboTipoFactura").val(model.tipoFactura);
    $("#txtNroFactura").val(model.nroFactura);
    $("#txtIva").val(model.iva);
    $("#txtImporteIva").val(model.ivaImporte);
    $("#txtImporte").val(model.importe);
    $("#txtImporteSinIva").val(model.importeSinIva);
    $("#txtComentarioPago").val(model.comentario);
    $("#cboEstado").val(model.estadoPago);
    $("#cboProveedor").val(model.idProveedor);

    document.getElementById("cbxFacturaPendiente").checked = model.facturaPendiente;


    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalPago").modal("show")
}
function calcularIva() {
    let importeText = $('#txtImporte').val();
    let importe = parseFloat(importeText == '' ? 0 : importeText);
    let iva = parseFloat($('#txtIva').val());

    if (!isNaN(importe) && !isNaN(iva)) {
        let importeSinIva = importe / (1 + (iva / 100));
        let importeIva = importe - importeSinIva;

        $('#txtImporteSinIva').val(importeSinIva.toFixed(2));
        $('#txtImporteIva').val(importeIva.toFixed(2));
    }
}

$('#txtIva').change(function () {
    calcularIva();
});

$('#txtImporte').keyup(function () {
    calcularIva();
});


function cargarTablaDinamicaProveedores(isGlobal) {

    let url = `/Admin/GetProveedorTablaDinamica?visionGlobal=${isGlobal}`;
    $("#wdr-component").LoadingOverlay("show")

    fetch(url, {
        method: "GET"
    }).then(response => {
        $("#wdr-component").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {

            if (responseJson.object != []) {

                let today = new Date();
                let month = "fecha.Month." + monthNames[today.getMonth()];
                let year = "fecha.Year." + today.getFullYear();

                if (window.pivot) {
                    window.pivot.dispose();
                }

                window.pivot = new WebDataRocks({
                    container: "#wdr-component",
                    toolbar: true,
                    report: {
                        dataSource: {
                            data: responseJson.object
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


        }
        else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
}