let tableDataClients;
let tableDataMovimientosClientes;
let rowSelectedClient;

const BASIC_MODEL_CLIENTE = {
    idCliente: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    comentario: null,
    condicionIva: null,
    isActive: true,
    registrationDate: null,
    modificationDate: null,
    modificationUser: null
}

$(document).ready(function () {

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            $("#cboTypeDocumentSale").append(
                $("<option>").val('').text('')
            )
            if (responseJson.state > 0) {
                if (responseJson.object.length > 0) {
                    responseJson.object.forEach((item) => {
                        $("#cboTypeDocumentSale").append(
                            $("<option>").val(item.idTypeDocumentSale).text(item.description)
                        )
                    });
                }
            }
            else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    tableDataClients = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetCliente",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idCliente",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            { "data": "cuil" },
            { "data": "telefono" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            { "data": "total" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "100px"
            }
        ],
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Clientes',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });

    validateCuil();
})

function validateCuil() {
    $('#txtCuil').on('input', function () {
        let formattedValue = formatCuil($(this).val());
        $(this).val(formattedValue);
    });

    $('#txtCuil').on('keypress', function (e) {
        let charCode = (e.which) ? e.which : e.keyCode;
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
        }
    });

    $('#txtCuil').on('blur', function () {
        let value = $(this).val().replace(/\D/g, '');
        if (value.length !== 11) {
            alert('El CUIL debe tener exactamente 11 dígitos.');
        }
    });
}

function formatCuil(value) {
    if (value == null)
        return '';

    value = value.replace(/\D/g, ''); // Eliminar todo lo que no sea dígito
    let formattedValue = '';

    // Formatear la cadena de acuerdo al patrón CUIL
    if (value.length > 0) formattedValue += value.substring(0, 2);
    if (value.length > 2) formattedValue += '-' + value.substring(2, 10);
    if (value.length > 10) formattedValue += '-' + value.substring(10, 11);

    return formattedValue;
}

const openModalCliente = (model = BASIC_MODEL_CLIENTE) => {
    $("#txtId").val(model.idCliente);
    $("#txtNombre").val(model.nombre);
    $("#txtCuil").val(formatCuil(model.cuil));
    $("#txtDireccion").val(model.direccion);
    $("#txtTelefono").val(model.telefono);
    $("#txtTotal").val(model.total);
    $("#txtComentario").val(model.comenario);
    $("#cboCondicionIva").val(model.condicionIva);
    $("#cboEstado").val(model.isActive ? 1 : 0);

    if (model.modificationUser == null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    cargarTablaMovimientos(model.idCliente);

    $("#modalData").modal("show")
}

function cargarTablaMovimientos(idCliente) {
    if (tableDataDetallesIVA != null)
        tableDataDetallesIVA.destroy();

    let url = '/Admin/GetMovimientoCliente?idCliente=' + idCliente;

    tableDataDetallesIVA = $("#tbMovimientos").DataTable({
        responsive: true,
        pageLength: 5,
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [3],
                "render": function (data, type, row) {
                    if (type == 'display' || type == 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idClienteMovimiento",
                "visible": false,
                "searchable": false
            },
            { "data": "totalString" },
            {
                "data": "sale.saleNumber",
                "defaultContent": ""
            },
            { "data": "registrationDate" },
            { "data": "registrationUser" },
            {
                "data": "idSale",
                "visible": false,
                "searchable": false
            }
            ,
            {
                "defaultContent": '<button class="btn btn-info btn-open-sale btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "40px"
            }
        ],
        order: [[3, "desc"]],
        dom: "frtip"
    });
}

$("#btnNew").on("click", function () {
    openModalCliente()
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

    const model = structuredClone(BASIC_MODEL_CLIENTE);
    model["nombre"] = $("#txtNombre").val();
    model["direccion"] = $("#txtDireccion").val();
    model["cuil"] = $("#txtCuil").val() != '' ? $("#txtCuil").val().replace(/-/g, '') : null;
    model["telefono"] = $("#txtTelefono").val();
    model["idCliente"] = $("#txtId").val();
    model["comentario"] = $("#txtComentario").val();
    model["condicionIva"] = $("#cboCondicionIva").val();
    model["isActive"] = $("#cboEstado").val() == 1 ? true : false;

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idCliente == 0) {
        fetch("/Admin/CreateCliente", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataClients.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "Cliente fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateCliente", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataClients.row(rowSelectedClient).data(responseJson.object).draw(false);
                rowSelectedClient = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "Cliente fué modificada", "success");

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
        rowSelectedClient = $(this).closest('tr').prev();
    } else {
        rowSelectedClient = $(this).closest('tr');
    }

    const data = tableDataClients.row(rowSelectedClient).data();

    openModalCliente(data);
})

$('#tbMovimientos tbody').on('click', 'button.btn-open-sale', function (event) {
    event.preventDefault();
    let data = tableDataDetallesIVA.row($(this).parents('tr')).data();
    let saleNumber = data['sale']['saleNumber'];

    let urlString = '/Reports/ReportSale?saleNumber=' + encodeURIComponent(saleNumber);

    window.open(urlString, '_blank');
});


$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataClients.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Cliente "${data.nombre}"`,
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

                fetch(`/Admin/DeleteCliente?idCliente=${data.idCliente}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataClients.row(row).remove().draw();
                        swal("Exitoso!", "Cliente  fué eliminada", "success");

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


$(document).on("click", "button.finalizeSale", function () {
    if (document.getElementById("cboTypeDocumentSale").value == '') {
        const msg = `Debe completaro el campo Tipo de Venta`;
        toastr.warning(msg, "");
        return;
    }

    if (document.getElementById("txtImporte").value == '') {
        const msg = `Debe completaro el campo Importe`;
        toastr.warning(msg, "");
        return;
    }

    let formasDePago = [];


    let subTotal = {
        total: parseFloat($("#txtImporte").val()).toFixed(2),
        formaDePago: $("#cboTypeDocumentSale").val()
    };

    formasDePago.push(subTotal);

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSale").val(),
        clientId: $("#txtId").val(),
        total: $("#txtImporte").val(),
        tipoMovimiento: 1,
        multiplesFormaDePago: formasDePago,
    }

    $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("show")

    fetch("/Sales/RegisterSale", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(sale)
    }).then(response => {

        $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {

            $("#cboTypeDocumentSale").val($("#cboTypeDocumentSale option:first").val());

            let idCliente = $("#txtId").val();
            cargarTablaMovimientos(idCliente);

            swal("Registrado!", "success");

        } else {
            swal("Lo sentimos", "La venta no fué registrada", "error");
        }
    }).catch((error) => {
        $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("hide")
    })


})