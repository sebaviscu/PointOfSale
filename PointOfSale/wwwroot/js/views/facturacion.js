let tableDataFactura;
let rowSelectedFactura;
let url_qr = null;

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
    idFacturaAnulada: null
}


$(document).ready(function () {

    tableDataFactura = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetFacturas",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [1],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            },
            {
                "targets": [7],
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
            { "data": "registrationDate" },
            {
                "data": "tipoFactura",
                render: function (data, type, row) {
                    // Asegurando el orden correcto de las operaciones y evitando posibles errores
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
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Formas de Pago',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
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
    $("#txtNumeroVenta").val(model.sale != null? model.sale.saleNumber : '');
    $("#txtCae").val(model.cae);
    $("#txtVencimientoCae").val(moment(model.caeVencimiento).format('DD/MM/YYYY'));
    $("#txtRegistrationUser").val(model.registrationUser);
    $("#txtPuntoVenta").val(model.puntoVentaString);
    $("#txtCuil").val(`${model.tipoDocumento}: ${model.nroDocumento}`);
    $("#btnVerVenta").attr("sale-number", model.sale != null ? model.sale.saleNumber : '');

    if (model.facturaAnulada != null) {

        $("#divFormaPago").hide();
        $("#divFacuraAnulada").show();
        $("#btnAnularFactura").hide();
        $("#txtFacuraAnulada").val(model.facturaAnulada);
    }
    else {
        $("#divFormaPago").show();
        $("#btnAnularFactura").show();
        $("#divFacuraAnulada").hide();
    }

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

    fetch(`/Admin/GetFactura?idFacturaEmitida=${data.idFacturaEmitida}`,)
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
    let urlString = '/Sales/ReportSale?saleNumber=' + encodeURIComponent(saleNumber);

    window.open(urlString, '_blank');
})

$("#bntAfip").on("click", function () {
    if (url_qr != null) {
        window.open(url_qr, '_blank');
    }
})

$("#btnConfiguracion").on("click", function () {

    showLoading();

    fetch(`/Admin/GetAjustesFacturacion`)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            removeLoading();
            if (responseJson.state) {

                openModalAjustesFacturacion(responseJson.object);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
})


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

                fetch(`/Admin/NotaCredito?idFacturaEmitida=${idFact}`, {
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
