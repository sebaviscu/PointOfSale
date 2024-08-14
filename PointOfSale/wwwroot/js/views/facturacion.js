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
    cliente: null
}

//const BASIC_MODEL_AJUSTES_FACTURACION = {
//    idAjustesFacturacion: 0,
//    logo: "",
//    cuit: 0,
//    condicionIva: null,
//    puntoVenta: 0,
//    certificadoFechaInicio: null,
//    certificadoFechaCaducidad: null,
//    certificadoPassword: "",
//    certificadoNombre: ""
//}

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
            { "data": "tipoFactura" },
            {
                "data": "nroFacturaString", render: function (data, type, row) {
                    return data != '' ? `${row.puntoVentaString}-${data} ` : '';
                }
            },
            { "data": "sale.typeDocumentSale" },
            {
                "data": "importeTotal", render: function (data, type, row) {
                    return `$ ${data}`;
                }
            },
            { "data": "cae" },
            { "data": "caeVencimiento" },
            {
                "data": "observaciones", render: function (data, type, row) {
                    return data != null ?
                        row.resultado != 'A' ?
                            '<span class="badge rounded-pill bg-danger"> ERROR </span>' :
                            '<span class="badge rounded-pill bg-info"> Obs </span>' :
                        '';
                }
            },
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
    $("#txtFormaPago").val(model.sale.typeDocumentSale);
    $("#txtImporteTotal").val(model.importeTotal);
    $("#txtImporteNeto").val(model.importeNeto);
    $("#txtImporteIva").val(model.importeIVA);
    $("#txtCae").val(model.cae);
    $("#txtVencimientoCae").val(moment(model.caeVencimiento).format('DD/MM/YYYY'));
    $("#txtRegistrationUser").val(model.registrationUser);
    $("#txtPuntoVenta").val(model.puntoVentaString);
    $("#txtCuil").val(`${model.tipoDocumento}: ${model.nroDocumento}`);
    $("#btnVerVenta").attr("sale-number", model.sale.saleNumber);

    if (model.observaciones != null) {
        const divError = document.getElementById("divError");
        divError.style.display = '';

        if (model.resultado === "A") {
            divError.className = "alert alert-warning d-flex align-items-center";
            document.getElementById("alertTitle").textContent = "ALERTA: ";
        } else {
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