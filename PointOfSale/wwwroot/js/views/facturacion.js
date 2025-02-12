let tableDataFactura;
let rowSelectedFactura;
let url_qr = null;
let tableDataDetallesIVA;

const BASIC_MODEL_FACTURACION = {
    idFacturaEmitida: 0,
    cae: "",
    caeVencimiento: "",
    fechaEmicion: "",
    nroDocumento: "",
    tipoDocumento: "",
    resultado: "",
    observaciones: "",
    registrationDate: "",
    registrationUser: "",
    nroFacturaString: "",
    puntoVentaString: "",
    tipoFactura: "",
    importeTotal: 0,
    importeNeto: 0,
    importeIVA: 0,
    qr: '',
    sale: null,
    cliente: null,
    facturaAnulada: null,
    idFacturaAnulada: null,
    DetalleFacturaIvas: []
}


$(document).ready(function () {

    let textoBusqueda = $('#txtIdSaleViewData').text().trim();

    tableDataFactura = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Facturacion/GetFacturas",
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
            },
            {
                "targets": [8],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data && data != '0001-01-01T00:00:00' ? moment(data).format('DD/MM/YYYY') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idFacturaEmitida",
                "visible": false,
                "searchable": false
            },
            {
                "data": "idSale",
                "visible": false,
                "searchable": true
            },
            { "data": "registrationDate" },
            {
                "data": "tipoFactura",
                render: function (data, type, row) {
                    let badge = '';
                    if (row.observaciones != null) {
                        badge = row.resultado != 'A' ?
                            ' <span class="badge rounded-pill bg-danger"> ERROR </span>' :
                            ' <span class="badge rounded-pill bg-info"> Obs </span>';
                    }
                    return data + ' ' + badge;
                }
            },
            {
                "data": "nroFacturaString", render: function (data, type, row) {
                    return data != '' ? `${row.puntoVentaString}-${data} ` : '';
                }
            },
            {
                "data": "sale.typeDocumentSale",
                "render": function (data, type, row) {
                    return data ? data : '';
                }
            },
            {
                "data": "importeTotal", render: function (data, type, row) {
                    return `$ ${data}`;
                }
            },
            { "data": "cae" },
            { "data": "caeVencimiento" },
            {
                "defaultContent": '<button class="btn btn-primary btn-ver btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[2, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Facturas',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6, 7, 8]
                }
            }, 'pageLength'
        ],
        "initComplete": function () {
            if (textoBusqueda !== '') {
                this.api().column(1).search(textoBusqueda).draw();
            }
        }
    });
})

const openModalFactura = (model = BASIC_MODEL_FACTURACION) => {

    url_qr = null;

    $("#txtIdFacturacion").val(model.idFacturaEmitida);
    $("#txtFechaEmision").val(moment(model.registrationDate).format('DD/MM/YYYY HH:mm'));
    $("#txtTipoFactura").val(model.tipoFactura);
    $("#txtNumeroFactura").val(model.nroFacturaString);
    $("#txtFormaPago").val(model.sale != null ? model.sale.typeDocumentSale : '');
    $("#txtImporteTotal").val(model.importeTotal);
    $("#txtImporteNeto").val(model.importeNeto);
    $("#txtImporteIva").val(model.importeIVA);
    $("#txtNumeroVenta").val(model.sale != null ? model.sale.saleNumber : '');
    $("#txtCae").val(model.cae);
    $("#txtVencimientoCae").val(model.caeVencimiento != null ? moment(model.caeVencimiento).format('DD/MM/YYYY') : '');
    $("#txtRegistrationUser").val(model.registrationUser);
    $("#txtPuntoVenta").val(model.puntoVentaString);
    $("#txtCuil").val(`${model.tipoDocumento}: ${model.nroDocumento}`);
    $('#txtCuil').attr('cuil', model.nroDocumento);
    $("#btnVerVenta").attr("sale-number", model.sale != null ? model.sale.saleNumber : '');

    const isAnulada = model.resultado !== "A";

    $("#bntAfip").toggle(!isAnulada);
    $("#divFormaPago").toggle(!isAnulada);
    $("#btnAnularFactura").toggle(!isAnulada);

    $("#btnShowRefacturar").toggle(isAnulada);
    $("#divFacuraAnulada").toggle(isAnulada);

    if (isAnulada) {
        $("#txtFacuraAnulada").val(model.facturaAnulada);
    }

    document.getElementById("divClienteSeleccionado").style.setProperty("display", "none", "important");


    if (model.observaciones != null) {
        const divError = document.getElementById("divError");
        divError.style.display = '';

        if (model.resultado === "A") {
            divError.className = "alert alert-warning d-flex align-items-center";
            document.getElementById("alertTitle").textContent = "ALERTA: ";
            $("#btnAnularFactura").show();
        } else {
            $("#btnAnularFactura").hide();
            divError.className = "alert alert-danger d-flex align-items-center";
            document.getElementById("alertTitle").textContent = "ERROR: ";
        }

        $("#txtError").text(model.observaciones);
        divError.classList.add('d-flex');
    } else {
        $("#txtError").text('');
        document.getElementById("alertTitle").textContent = "";

        const divError = document.getElementById("divError");
        divError.classList.remove('d-flex');
        divError.style.display = 'none';

        if (model.qr != '') {
            url_qr = model.qr;
        }
    }

    cargarTablaDetallesIva(model.detalleFacturaIvas);

    $("#modalData").modal("show")
}

$("#tbData tbody").on("click", ".btn-ver", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedFactura = $(this).closest('tr').prev();
    } else {
        rowSelectedFactura = $(this).closest('tr');
    }
    const data = tableDataFactura.row(rowSelectedFactura).data();
    showLoading();

    fetch(`/Facturacion/GetFactura?idFacturaEmitida=${data.idFacturaEmitida}`,)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            removeLoading();
            if (responseJson.state) {

                openModalFactura(responseJson.object);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
})

$("#btnVerVenta").on("click", function () {
    let saleNumber = $(this).attr("sale-number");
    let urlString = '/Reports/ReportSale?saleNumber=' + encodeURIComponent(saleNumber);

    window.open(urlString, '_blank');
})

$("#bntAfip").on("click", function () {
    if (url_qr != null) {
        window.open(url_qr, '_blank');
    }
})

$("#btnShowRefacturar").on("click", function (event) {

    $("#divClienteSeleccionado").show();
    let cuilText = $("#txtCuil").val();
    let cuil = $('#txtCuil').attr('cuil');

    $('#txtClienteParaFactura').val(cuilText);
    $('#txtClienteParaFactura').attr('cuil', cuil);
})

$("#btnRefacturar").on("click", function (event) {
    event.preventDefault();

    swal({
        title: "",
        text: "¿Está seguro que desea refacturar?",
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        cancelButtonClass: "btn-secondary",
        confirmButtonText: "Si, refacturar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                let idFact = $("#txtIdFacturacion").val();
                let cuil = $('#txtClienteParaFactura').attr('cuil');

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Facturacion/Refacturar?idFacturaEmitida=${idFact}&cuil=${cuil}`, {
                    method: "POST"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        swal("Exitoso!", "La factura fué refacturada", "success");
                        $("#modalData").modal("hide")

                        location.reload();

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

$("#btnBuscarCliente").on("click", function () {
    $("#modalDatosFactura").modal("show");
    inicializarClientesFactura();
});

$("#btnAnularFactura").on("click", function (event) {
    event.preventDefault();
    swal({
        title: "¿Está seguro?",
        text: `Anular la factura`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, anular",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                let idFact = $("#txtIdFacturacion").val();

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Facturacion/NotaCredito?idFacturaEmitida=${idFact}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        swal("Exitoso!", "La factura fué anulada", "success");

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

function cargarTablaDetallesIva(data) {
    if (data.length == 0)
        return;

    if (tableDataDetallesIVA != null)
        tableDataDetallesIVA.destroy();

    tableDataDetallesIVA = $("#tbDetallesIva").DataTable({
        responsive: true,
        paging: false,  // Desactiva la paginación
        searching: false,  // Opcional: Desactiva la barra de búsqueda
        info: false,  // Oculta el texto de "Mostrando X de Y registros"
        //ordering: false,
        data: data,
        "columns": [
            {
                "data": "id",
                "visible": false,
                "searchable": false
            },
            {
                data: null,
                render: function (data, type, row) {
                    if (data.tipoIva == 3)
                        return '0 %'
                    else if (data.tipoIva == 4)
                        return '10.5 %'
                    else if (data.tipoIva == 5)
                        return '21 %'
                    else if (data.tipoIva == 6)
                        return '27 %'
                }
            },
            {
                data: "importeNeto",
                render: function (data, type, row) {
                    return '$' + data.toFixed(2);
                }
            },
            {
                data: "importeIVA",
                render: function (data, type, row) {
                    return '$' + data.toFixed(2);
                }
            },
            {
                data: "importeTotal",
                render: function (data, type, row) {
                    return '$' + data.toFixed(2);
                }
            }
        ],
        order: [[1, "desc"]],
        dom: "t"  // Solo muestra la tabla sin controles
    });

}