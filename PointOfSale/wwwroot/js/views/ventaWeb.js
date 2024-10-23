let tableDataVentaWeb;
let rowSelectedVentaWeb;
let productSelectedVentaWeb = null;
let isHealthySaleWeb = false;
let formasDePagosListVentaWeb = [];

const BASIC_MODEL_VENTA_WEB = {
    idVentaWeb: 0,
    description: "",
    nombre: "",
    direccion: "",
    telefono: "",
    comentario: "",
    totalString: "",
    formaDePago: null,
    estado: 0,
    detailSales: null,
    idTienda: 0,
    registrationDate: null,
    modificationDate: null,
    modificationUser: null,
    isEdit: null,
    editUser: null,
    editText: null,
    editDate: null,
    idClienteFactura: null,
    cuilFactura: null,
    imprimirTicket: false
}


$(document).ready(function () {
    $('[data-toggle="popover"]').popover({
        html: true,
        trigger: 'focus'
    });

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                formasDePagosListVentaWeb = responseJson.object;

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
        });

    tableDataVentaWeb = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Shop/GetVentasWeb",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [2],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idVentaWeb",
                "visible": false,
                "searchable": false
            },
            { "data": "saleNumber" },
            { "data": "registrationDate" },
            { "data": "nombre" },
            { "data": "direccion" },
            { "data": "telefono" },
            { "data": "totalString" },
            {
                "data": "estado", render: function (data) {
                    if (data == 0)
                        return '<span class="badge badge-info">Nueva</span>';
                    else if (data == 1)
                        return '<span class="badge badge-success">Finalizada</span>';
                    else if (data == 2)
                        return '<span class="badge badge-danger">Cerrada</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "60px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Ventas Web',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboTienda").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state > 0) {
                if (responseJson.object.length > 0) {
                    responseJson.object.forEach((item) => {
                        $("#txtFormaPago").append(
                            $("<option>").val(item.idTypeDocumentSale).text(item.description)
                        )
                    });
                }
            }
            else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        });

    healthcheck();

})


$("#printTicket").click(function () {
    showLoading();

    if (isHealthySaleWeb) {
        let idVentaWeb = parseInt($("#txtId").val());

        fetch(`/Shop/PrintTicketVentaWeb?idVentaWeb=${idVentaWeb}`)
            .then(response => {
                removeLoading();
                return response.json();
            })
            .then(response => {

                if (response.state) {
                    $("#modalData").modal("hide");

                    if (isHealthySaleWeb && response.object.nombreImpresora != '' && response.object.ticket != '') {

                        printTicket(response.object.ticket, response.object.nombreImpresora, response.object.imagesTicket);


                        swal("Exitoso!", "Ticket impreso!", "success");
                    }

                } else {
                    swal("Lo sentimos", "No se ha podido imprimir el ticket. Error: " + response.message, "error");
                }
            })
    }

})

const openModalEditVentaWeb = (model = BASIC_MODEL_VENTA_WEB) => {
    $('#modalData').modal('show');

    $("#btnSave").text("Guardar Cambios");

    document.getElementById("divSearchproducts").style.display = 'none';

    document.querySelector('#txtNombre').disabled = true;
    document.querySelector('#txtTelefono').disabled = true;
    document.querySelector('#txtDireccion').disabled = true;
    document.querySelector('#txtFormaPago').disabled = true;
    document.querySelector('#txtComentario').disabled = true;

    $("#txtId").val(model.idVentaWeb);
    $("#txtFecha").val(model.fecha);
    $("#txtTotal").val(model.total);
    $("#txtNombre").val(model.nombre);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#txtFormaPago").val(model.idFormaDePago);
    $("#txtComentario").val(model.comentario);
    $("#cboState").val(model.estado);
    $("#cboTienda").val(model.idTienda);

    if (model.isEdit) {
        document.getElementById("popoverEdit").style.display = '';

        $('#popoverEdit').attr('data-bs-content', model.editText);

        let popover = new bootstrap.Popover(document.getElementById('popoverEdit'), {
            html: true
        });
        popover.setContent({
            '.popover-body': model.editText
        });
    } else {
        document.getElementById("popoverEdit").style.display = 'none';
    }

    document.querySelector('#cboState').disabled = model.estado > 0;
    document.querySelector('#cboTienda').disabled = model.estado > 0;
    document.querySelector('#btnEdit').disabled = model.estado > 0;
    document.querySelector('#btnSave').disabled = model.estado > 0;

    if (model.modificationUser === null) {
        document.getElementById("divModif").style.display = 'none';
    } else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#tbProducts tbody").html("");

    model.detailSales.forEach((item) => {
        addNewProduct(item.idDetailSale, item.descriptionProduct, item.quantity, item.tipoVentaString, item.price, item.total, item.idProduct);
    });

    $('#modalData').on('shown.bs.modal', function () {
        select2Modal();
    });

    $('#cboSearchProduct').on('select2:select', function (e) {
        let data = e.params.data;
        productSelectedVentaWeb = data;

        if (data.tipoVenta == 2) {
            var peso = $("#txtPeso").val();

            if (peso != "") {
                if (peso === false || isNaN(peso)) return false;
            }

            peso = peso == "" ? 1 : parseFloat(peso);

            let precio = Number.parseFloat(productSelectedVentaWeb.price);
            addNewProduct(0, productSelectedVentaWeb.text, peso, "U", precio, precio * peso, productSelectedVentaWeb.id);

            $('#txtPeso').val('');
            const element = document.getElementById("txtPeso");
            window.setTimeout(() => element.focus(), 0);
            return;
        }

        const element = document.getElementById("txtPeso");
        window.setTimeout(() => element.focus(), 0);

    })

    $("#btnAgregarProducto").on("click", function () {

        functionAddProducto();
    });

    $('#txtPeso').on('keypress', function (e) {
        if ($('#txtPeso').val() != '' && e.which === 13) {

            $(this).attr("disabled", "disabled");

            functionAddProducto();
            $('#txtPeso').val('');

            $(this).removeAttr("disabled");
        }
    });
};

function functionAddProducto() {
    let peso = parseFloat($("#txtPeso").val());

    if (peso === false || peso === "" || isNaN(peso)) return false;

    if (productSelectedVentaWeb === null) {
        return false;
    }
    let precio = Number.parseFloat(productSelectedVentaWeb.price);
    addNewProduct(0, productSelectedVentaWeb.text, peso, "Kg", precio, precio * peso, productSelectedVentaWeb.id);

    updateTotal();
    $('#cboSearchProduct').val("").trigger('change');
    $("#txtPeso").val(0)
}

function addNewProduct(idDetailSale, description, cantidad, tipoVenta, precio, total, idProducto) {
    const isEditVisible = $('#divSearchproducts').is(':visible');

    const newRow = $("<tr>").append(
        $("<td>").html('<button class="btn btn-danger btn-sm delete-row" style="padding-top: 0px;padding-bottom: 0px;">-</button>'),
        $("<td>").text(description),
        $("<td>").text(`${cantidad} / ${tipoVenta}`),
        $("<td>").text(`$ ${Number.parseFloat(precio).toFixed(2)}`),
        $("<td>").text(`$ ${Number.parseFloat(total).toFixed(2)}`).data("idDetailSale", idDetailSale).data("idProducto", idProducto)
    );

    if (!isEditVisible) {
        newRow.find('.delete-row').hide();
    }

    $("#tbProducts tbody").append(newRow);
}

$("#btnEdit").click(function () {
    const isHidden = document.getElementById("divSearchproducts").style.display === 'none';
    document.getElementById("divSearchproducts").style.display = isHidden ? '' : 'none';
    document.getElementById("cboSearchProduct").style.display = isHidden ? '' : 'none';

    const fields = ['#txtNombre', '#txtTelefono', '#txtDireccion', '#txtFormaPago', '#txtComentario'];
    fields.forEach(field => {
        document.querySelector(field).disabled = !isHidden;
    });

    $('.delete-row').each(function () {
        this.style.display = isHidden ? '' : 'none';
    });
});

$(document).on('click', '.delete-row', function () {
    $(this).closest('tr').remove();
    updateTotal();

});

function updateTotal() {
    let totalAmount = 0;

    $("#tbProducts tbody tr").each(function () {
        const total = parseFloat($(this).find("td").eq(4).text().replace('$', '').trim());
        totalAmount += total;
    });

    $("#txtTotal").val(`${totalAmount.toFixed(2)}`);
}

$("#btnSave").on("click", function () {

    let estadoVenta = parseInt($("#cboState").val());
    if (estadoVenta == 1) { // finalizar
        $("#modalData").modal("hide");
        $('#modalPago').modal('show');

        $('#txtClienteParaFactura').attr('cuil', '');
        $('#txtClienteParaFactura').attr('idCliente', '');

        let total = parseFloat($("#txtTotal").val());
        $("#txtTotalFinalizarVenta").val(total);

        let formaPago = parseInt($("#txtFormaPago").val());
        $("#cboTypeDocumentSale").val(formaPago);

        $("#cboTypeDocumentSale").trigger('change');

        return;
    }

    editarVentaWeb();
})

$("#btnFinalizarVenta").on("click", function () {
    $("#modalPago").LoadingOverlay("show")

    editarVentaWeb();
    $("#modalPago").LoadingOverlay("hide")
    $('#modalPago').modal('hide');
})

async function editarVentaWeb() {

    const model = structuredClone(BASIC_MODEL_VENTA_WEB);
    model["idTienda"] = parseInt($("#cboTienda").val());

    if (isNaN(model.idTienda)) {
        $('#cboTienda').focus();
        toastr.warning(`Debe seleccionar una Tienda`, "");
        return;
    }

    model["idVentaWeb"] = parseInt($("#txtId").val());

    let estadoVenta = parseInt($("#cboState").val());
    model["estado"] = estadoVenta;

    model["idFormaDePago"] = estadoVenta == 1 ? $("#cboTypeDocumentSale").val() : parseInt($("#txtFormaPago").val());
    model["imprimirTicket"] = document.querySelector('#cboImprimirTicket').checked;
    model["total"] = parseFloat($("#txtTotal").val());
    model["comentario"] = $("#txtComentario").val();
    model["nombre"] = $("#txtNombre").val();
    model["telefono"] = $("#txtTelefono").val();
    model["direccion"] = $("#txtDireccion").val();


    let cuilParaFactura = $('#txtClienteParaFactura').attr('cuil');
    let idClienteParaFactura = $('#txtClienteParaFactura').attr('idCliente');
    model["cuilFactura"] = cuilParaFactura != '' ? cuilParaFactura : null;
    model["idClienteFactura"] = idClienteParaFactura != '' ? parseInt(idClienteParaFactura) : null;


    let products = [];

    $("#tbProducts tbody tr").each(function () {
        const $row = $(this);
        const product = {
            brandProduct: null,
            categoryProducty: '',
            descriptionProduct: $row.find("td").eq(1).text(),
            idDetailSale: $row.find("td").eq(4).data("idDetailSale"),
            idProduct: $row.find("td").eq(4).data("idProducto"),
            idSale: null,
            idSaleNavigation: null,
            idVentaWeb: 0,
            price: parseFloat($row.find("td").eq(3).text().replace('$', '').trim()),
            producto: null,
            promocion: null,
            quantity: parseFloat($row.find("td").eq(2).text().split(' / ')[0]),
            tipoVenta: $row.find("td").eq(2).text().split(' / ')[1] == "Kg" ? 1 : 2,
            tipoVentaString: $row.find("td").eq(2).text().split(' / ')[1],
            total: parseFloat($row.find("td").eq(4).text().replace('$', '').trim())
        };
        products.push(product);
    });

    model["detailSales"] = products;

    showLoading();
    $("#modalData").LoadingOverlay("show")

    fetch("/Shop/UpdateVentaWeb", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        removeLoading();
        return response.json();
    }).then(responseJson => {

    $("#modalData").LoadingOverlay("hide")
        if (responseJson.state) {

            tableDataVentaWeb.row(rowSelectedVentaWeb).data(responseJson.object).draw(false);
            rowSelectedVentaWeb = null;
            $("#modalData").modal("hide");
            if (isHealthySaleWeb && responseJson.object.nombreImpresora != '' && estadoVenta == 1 && responseJson.object.ticket != '') {
                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);

            } else {

                swal("Exitoso!", "La Venta Web fué modificada", "success");
            }

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        removeLoading();
    })
}

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedVentaWeb = $(this).closest('tr').prev();
    } else {
        rowSelectedVentaWeb = $(this).closest('tr');
    }

    const data = tableDataVentaWeb.row(rowSelectedVentaWeb).data();

    openModalEditVentaWeb(data);
})

function select2Modal() {
    $('#cboSearchProduct').select2({
        ajax: {
            url: "/Sales/GetProducts",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term,
                    listaPrecios: 4
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idProduct,
                            text: item.description,
                            brand: item.brand,
                            category: item.idCategory,
                            photoBase64: item.photoBase64,
                            price: item.price,
                            tipoVenta: item.tipoVenta
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResults,
        allowClear: true,
        dropdownParent: $('#modalData')
    });

    function formatResults(data) {
        if (data.loading)
            return data.text;

        let container = $(
            `<table width="90%">
                <tr>
                    <td style="width:60px">
                        <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                    </td>
                    <td>
                        <p style="font-weight: bolder;margin:2px">${data.text}</p>
                        <p>$ ${parseFloat(data.price).toFixed(2)}</p>
                    </td>
                </tr>
             </table>`
        );

        return container;
    }
}

async function healthcheck() {
    isHealthySaleWeb = await getHealthcheck();

    if (isHealthySaleWeb) {
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
        document.querySelector('#printTicket').disabled = true;
    }
}

$("#cboState").on("change", function () {
    let val = $(this).val();

    if (val == "1") {
        $("#btnSave").text("Finalizar Venta");
    } else {
        $("#btnSave").text("Guardar Cambios");
    }
});

$('#cboTypeDocumentSale').change(function () {
    let idFormaDePago = $(this).val();

    let formaDePago = formasDePagosListVentaWeb.find(_ => _.idTypeDocumentSale == idFormaDePago);

    if (formaDePago != null) {
        $("#cboFactura").val(formaDePago.tipoFactura);
        $("#cboFactura").trigger('change');
    }
})

$("#btnBuscarCliente").on("click", function () {
    $("#modalDatosFactura").modal("show");
    inicializarClientesFactura();
});

