let tableTipoGastos;
let rowSelectedTipoGastos;
let tableFormatoVenta;
let rowSelectedFormatoVenta;
let tableDataTipoVenta;
let rowSelectedFormaPago;
let tableDataCategory;
let rowSelectedCategory;
let rowSelectedTag;
let tableTags;
let tableRazonMovimientoCaja;
let rowSelectedRazonMovimientoCaja;

const BASIC_MODEL_RAZON_MOVIMIENTIO_CAJA = {
    idRazonMovimientoCaja: 0,
    descripcion: '',
    tipo: 0,
    estado: 0
}

const BASIC_MODEL_CATEGORIA = {
    idCategory: 0,
    description: "",
    isActive: 1,
    modificationDate: null,
    modificationUser: null
}

const BASIC_MODEL_TIPO_VENTA = {
    idTypeDocumentSale: 0,
    description: "",
    isActive: 1,
    web: 1,
    tipoFactura: 0
}

const BASIC_MODEL_TIPO_DE_GASTOS = {
    IdTipoGastos: 0,
    gastoParticular: -1,
    descripcion: ""
}

$(document).ready(function () {

    tableDataCategory = $("#tbDataCategoria").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetCategories",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idCategory",
                "visible": false,
                "searchable": false
            },
            { "data": "description" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit-categoria btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete-categoria btn-sm"><i class="mdi mdi-trash-can"></i></button>',
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
                filename: 'Reporte categories',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });

    tableTags = $("#tbTags").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetTags",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTag",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            {
                "data": "color",
                "render": function (data, type, row) {
                    return `<div style="width: 30px; height: 30px; background-color: ${data}; border-radius: 50%;"></div>`;
                },
                "orderable": false,
                "searchable": false,
                "width": "40px"
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit-tag btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete-tag btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Tags',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });

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
                    return data != null ? data + ' %' : '0 %';
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
                "data": null,
                "render": function (data, type, row) {
                    let editButton = '<button class="btn btn-primary btn-edit-formato-venta btn-sm me-2"><i class="mdi mdi-pencil"></i></button>';
                    let deleteButton = '<button class="btn btn-danger btn-delete-formato-venta btn-sm"><i class="mdi mdi-trash-can"></i></button>';

                    return row.formato == 'Unidad' || row.formato == '1 kg' ? editButton : editButton + deleteButton;
                },
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[2, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Formatos de Venta',
                exportOptions: {
                    columns: [1, 2, 3]
                }
            }, 'pageLength'
        ]
    });

    tableDataTipoVenta = $("#tbFormaPago").DataTable({
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
                "data": null,
                "render": function (data, type, row) {
                    let editButton = '<button class="btn btn-primary btn-edit-forma-pago btn-sm me-2"><i class="mdi mdi-pencil"></i></button>';
                    let deleteButton = '<button class="btn btn-danger btn-delete-forma-pago btn-sm"><i class="mdi mdi-trash-can"></i></button>';

                    return row.idTypeDocumentSale == 1 ? editButton : editButton + deleteButton;
                },
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
                filename: 'Reporte Formas de Pago',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });

    tableRazonMovimientoCaja = $("#tbRazonMovCaja").DataTable({
        responsive: true,
        "ajax": {
            "url": `/MovimientoCaja/GetRazonMovimientoCaja`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idRazonMovimientoCaja",
                "visible": false,
                "searchable": false
            },
            { "data": "tipoString" },
            { "data": "descripcion" },
            {
                "data": "estado", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "data": null,
                "render": function (data, type, row) {
                    let editButton = '<button class="btn btn-primary btn-edit-razon-mov-caja btn-sm me-2"><i class="mdi mdi-pencil"></i></button>';
                    let deleteButton = '<button class="btn btn-danger btn-delete-razon-mov-caja btn-sm"><i class="mdi mdi-trash-can"></i></button>';

                    return row.idTypeDocumentSale == 1 ? editButton : editButton + deleteButton;
                },
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, 2, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Razones Movimientos de Caja',
                exportOptions: {
                    columns: [1, 2, 3]
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
        text: `Eliminar Tipo de Gastos "${data.descripcion}"`,
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

                fetch(`/Gastos/DeleteTipoDeGastos?idTipoGastos=${data.idTipoGastos}`, {
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


    if (model.idTipoGastos == 0) {
        fetch("/Gastos/CreateTipoDeGastos", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                tableTipoGastos.row.add(responseJson.object).draw(false);
                swal("Exitoso!", "El Tipo de Gastos fue creado con éxito.", "success");
                $('#modalDataTipoDeGasto').modal('hide');
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("hide")
        })
    }
    else {
        fetch("/Gastos/UpdateTipoDeGastos", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                tableTipoGastos.row(rowSelectedTipoGastos).data(responseJson.object).draw(false);
                rowSelectedTipoGastos = null;
                swal("Exitoso!", "El Tipo de Gastos fue actualizado con éxito.", "success");
                $('#modalDataTipoDeGasto').modal('hide');
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            swal("Error", "Ocurrió un error al actualizar el Tipo de Gastos.", "error");
            console.error('Error:', error);
        });
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

    $("#txtDescripcionFormatoVenta").prop("disabled", data.formato == "Unidad");


    $("#modalFormatoVenta").modal("show")
});

$('#btnAddFormtatoVenta').on('click', function () {
    $('#txtIdFormatoVenta').val('');
    $('#txtDescripcionFormatoVenta').val('');
    $('#txtValorFormatoVenta').val('');
    $('#cboStateFormatoVenta').val('1');

    if (tableFormatoVenta) {
        tableFormatoVenta.ajax.reload(null, false);
    }
    $('#modalFormatoVenta').modal('show');
});

$('#btnSaveFormatoVenta').on('click', function () {
    let tagData = {
        idFormatosVenta: parseInt($('#txtIdFormatoVenta').val() == '' ? 0 : $('#txtIdFormatoVenta').val()),
        formato: $('#txtDescripcionFormatoVenta').val(),
        valor: $('#txtValorFormatoVenta').val(),
        estado: $('#cboStateFormatoVenta').val() == 1 ? true : false
    };


    if (tagData.idFormatosVenta == 0) {
        fetch("/Tablas/CreateFormatosVenta", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableFormatoVenta.row.add(responseJson.object).draw(false);
                    swal("Exitoso!", "El tag fue creado con éxito.", "success");
                    $('#modalFormatoVenta').modal('hide');

                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al crear el Formato de Venta.", "error");
                console.error('Error:', error);
            });
    } else {

        fetch("/Tablas/UpdateFormatosVenta", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableFormatoVenta.row(rowSelectedFormatoVenta).data(responseJson.object).draw(false);
                    rowSelectedFormatoVenta = null;
                    swal("Exitoso!", "El Formato de Venta fue actualizado con éxito.", "success");
                    $('#modalFormatoVenta').modal('hide');
                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al actualizar el Formato de Venta.", "error");
                console.error('Error:', error);
            });
    }
    $('#txtTagName').val('');
    $('#txtTagColor').val('');
    $('#txtTagId').val(0);
})

$('#tbFormatoVenta').on('click', '.btn-delete-formato-venta', function () {
    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableFormatoVenta.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Formato de Venta "${data.formato}"`,
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

                fetch(`/Tablas/DeleteFormatosVenta?idFormatosVenta=${data.idFormatosVenta}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableFormatoVenta.row(row).remove().draw();
                        swal("Exitoso!", "Formato de Venta fué eliminado", "success");

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



const openModalTipoVenta = (model = BASIC_MODEL_TIPO_VENTA) => {
    $("#txtIdFormaPago").val(model.idTypeDocumentSale);
    $("#txtNombreFormaPago").val(model.description);
    $("#cboStateFormaPago").val(model.isActive ? 1 : 0);
    $("#cboTipoFactura").val(model.tipoFactura);
    $("#cboWeb").val(model.web ? 1 : 0);
    $("#txtComision").val(model.comision);

    document.querySelector('#cboWeb').checked = model.web

    $("#txtNombreFormaPago").prop("disabled", model.idTypeDocumentSale == 1);


    $("#modalDataFormaPago").modal("show")
}

$("#btnNewFormaPago").on("click", function () {
    openModalTipoVenta()
})

$("#btnSaveFormaPago").on("click", function () {
    const inputs = $("input.input-validate-forma-pago").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_TIPO_VENTA);
    model["idTypeDocumentSale"] = parseInt($("#txtIdFormaPago").val());
    model["description"] = $("#txtNombreFormaPago").val();
    model["isActive"] = $("#cboStateFormaPago").val() === '1' ? true : false;
    model["web"] = document.querySelector('#cboWeb').checked;
    model["tipoFactura"] = parseInt($("#cboTipoFactura").val());
    model["comision"] = $("#txtComision").val();

    $("#modalDataFormaPago").find("div.modal-content").LoadingOverlay("show")


    if (model.idTypeDocumentSale == 0) {
        fetch("/Admin/CreateTipoVenta", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataFormaPago").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataTipoVenta.row.add(responseJson.object).draw(false);
                $("#modalDataFormaPago").modal("hide");
                swal("Exitoso!", "Forma de Pago fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataFormaPago").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateTipoVenta", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataFormaPago").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataTipoVenta.row(rowSelectedFormaPago).data(responseJson.object).draw(false);
                rowSelectedFormaPago = null;
                $("#modalDataFormaPago").modal("hide");
                swal("Exitoso!", "Forma de Pago fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataFormaPago").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbFormaPago tbody").on("click", ".btn-edit-forma-pago", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedFormaPago = $(this).closest('tr').prev();
    } else {
        rowSelectedFormaPago = $(this).closest('tr');
    }

    const data = tableDataTipoVenta.row(rowSelectedFormaPago).data();

    openModalTipoVenta(data);
})

$("#tbFormaPago tbody").on("click", ".btn-delete-forma-pago", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataTipoVenta.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Forma de Pago "${data.description}"`,
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

                        tableDataTipoVenta.row(row).remove().draw();
                        swal("Exitoso!", "Forma de Pago  fué eliminada", "success");

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



const openModalCategory = (model = BASIC_MODEL_CATEGORIA) => {
    $("#txtIdCategoria").val(model.idCategory);
    $("#txtDescriptionCategoria").val(model.description);
    $("#cboStateCategoria").val(model.isActive);

    if (model.modificationUser == null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalDataCategoria").modal("show")

}

$("#btnNewCategoria").on("click", function () {
    openModalCategory()
})

$("#btnSaveCategoria").on("click", function () {
    const inputs = $("input.input-validate-categoria").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_CATEGORIA);
    model["idCategory"] = parseInt($("#txtIdCategoria").val());
    model["description"] = $("#txtDescriptionCategoria").val();
    model["isActive"] = $("#cboStateCategoria").val();


    $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("show")


    if (model.idCategory == 0) {
        fetch("/Inventory/CreateCategory", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataCategory.row.add(responseJson.object).draw(false);
                $("#modalDataCategoria").modal("hide");
                swal("Exitoso!", "La categoria fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Inventory/UpdateCategory", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataCategory.row(rowSelectedCategory).data(responseJson.object).draw(false);
                rowSelectedCategory = null;
                $("#modalDataCategoria").modal("hide");
                swal("Exitoso!", "La categoria fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataCategoria").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbDataCategoria tbody").on("click", ".btn-edit-categoria", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedCategory = $(this).closest('tr').prev();
    } else {
        rowSelectedCategory = $(this).closest('tr');
    }

    const data = tableDataCategory.row(rowSelectedCategory).data();

    openModalCategory(data);
})
$("#tbDataCategoria tbody").on("click", ".btn-delete-categoria", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataCategory.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar categoria "${data.description}"`,
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

                fetch(`/Inventory/DeleteCategory?idCategory=${data.idCategory}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataCategory.row(row).remove().draw();
                        swal("Exitoso!", "Categoria fué eliminada", "success");

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



$('#btnAddNewTag').on('click', function () {
    $('#txtTagName').val('');
    $('#txtTagColor').val('');

    if (tableTags) {
        tableTags.ajax.reload(null, false);
    }

    $('#modalTagData').modal('show');
});

$('#btnSaveTag').on('click', function () {
    const inputs = $("input.input-validate-tag").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    let tagData = {
        idTag: parseInt($('#txtTagId').val()),
        nombre: $('#txtTagName').val(),
        color: $('#txtTagColor').val()
    };


    if (tagData.idTag == 0) {
        fetch("/Inventory/CreateTag", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableTags.row.add(responseJson.object).draw(false);
                    swal("Exitoso!", "El tag fue creado con éxito.", "success");
                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al crear el tag.", "error");
                console.error('Error:', error);
            });
    } else {

        fetch("/Inventory/UpdateTag", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableTags.row(rowSelectedTag).data(responseJson.object).draw(false);
                    rowSelectedTag = null;
                    swal("Exitoso!", "El tag fue actualizado con éxito.", "success");
                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al actualizar el tag.", "error");
                console.error('Error:', error);
            });
    }

    $('#modalTagData').modal('hide');
    $('#txtTagName').val('');
    $('#txtTagColor').val('');
    $('#txtTagId').val(0);
})

$('#tbTags').on('click', '.btn-delete-tag', function () {
    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableTags.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar tag "${data.nombre}"`,
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

                        tableTags.row(row).remove().draw();
                        swal("Exitoso!", "Tag fué eliminada", "success");

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

$('#tbTags').on('click', '.btn-edit-tag', function () {
    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTag = $(this).closest('tr').prev();
    } else {
        rowSelectedTag = $(this).closest('tr');
    }

    const data = tableTags.row(rowSelectedTag).data();

    $('#txtTagName').val(data.nombre);
    $('#txtTagColor').val(data.color);
    $('#txtTagId').val(data.idTag);

    $("#modalTagData").modal("show")
});



$("#btnNewRazon").on("click", function () {
    $("#modalRazon").modal("show");
})

$("#btnSaveRazonMovimientoCaja").on("click", function () {
    const inputs = $("input.input-validate-razon-mov-caja").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if ($("#txtDescripcionRazonMovimientoCaja").val().length < 10) {
        toastr.warning("La descripción debe ser mayor a 10 caracteres", "");
    }

    const model = structuredClone(BASIC_MODEL_RAZON_MOVIMIENTIO_CAJA);
    model["idRazonMovimientoCaja"] = parseInt($("#txtIdRazon").val());
    model["descripcion"] = $("#txtDescripcionRazonMovimientoCaja").val();
    model["tipo"] = parseInt($("#cboTipoRazonMovimientoCaja").val());
    model["estado"] = parseInt($("#cboStateRazonMovimientoCaja").val());


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
                tableRazonMovimientoCaja.row.add(responseJson.object).draw(false);
                swal("Exitoso!", "La Razon fue creado con éxito.", "success");
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalRazon").find("div.modal-content").LoadingOverlay("hide")
        })
    }
    else {
        fetch("/MovimientoCaja/UpdateRazonMovimientoCaja", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalRazon").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                tableRazonMovimientoCaja.row(rowSelectedRazonMovimientoCaja).data(responseJson.object).draw(false);
                rowSelectedRazonMovimientoCaja = null;
                swal("Exitoso!", "La Razon fue actualizado con éxito.", "success");
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalRazon").find("div.modal-content").LoadingOverlay("hide")
            swal("Error", "Ocurrió un error al actualizar La Razon.", "error");
        });
    }
    $("#modalRazon").modal("hide");

})

$('#tbRazonMovCaja').on('click', '.btn-edit-razon-mov-caja', function () {
    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTag = $(this).closest('tr').prev();
    } else {
        rowSelectedRazonMovimientoCaja = $(this).closest('tr');
    }

    const data = tableRazonMovimientoCaja.row(rowSelectedRazonMovimientoCaja).data();

    $('#txtIdRazon').val(data.idRazonMovimientoCaja);
    $('#cboTipoRazonMovimientoCaja').val(data.tipo);
    $('#txtDescripcionRazonMovimientoCaja').val(data.descripcion);
    $('#cboStateRazonMovimientoCaja').val(data.estado ? 1 : 0);

    $("#modalRazon").modal("show")
});

$('#tbRazonMovCaja').on('click', '.btn-delete-razon-mov-caja', function () {
    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableRazonMovimientoCaja.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Razon "${data.nombre}"`,
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

                fetch(`/MovimientoCaja/DeleteRazonMovimientoCaja?idRazonMovimientoCaja=${data.idTag}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableRazonMovimientoCaja.row(row).remove().draw();
                        swal("Exitoso!", "Razon fué eliminada", "success");

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
