let tableDataFactura;
let rowSelectedFactura;

const BASIC_MODEL_FACTURACION = {
    idFacturaEmitida: 0,
    cae: "",
    caeVencimiento: "",
    fechaEmicion: "",
    nroDocumento: "",
    tipoDocumento: "",
    resultado: "",
    errores: "",
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

const BASIC_MODEL_AJUSTES_FACTURACION = {
    idAjustesFacturacion: 0,
    logo: "",
    cuit: 0,
    condicionIva: null,
    puntoVenta: 0,
    certificadoFechaInicio: null,
    certificadoFechaCaducidad: null,
    certificadoPassword: "",
    certificadoNombre: ""
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
                    } BASIC_MODEL_AJUSTES_FACTURACION
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
                "data": "errores", render: function (data, type, row) {
                    return data != null ? '<span class="badge rounded-pill bg-danger">  <i class="mdi mdi-close"></i>&nbsp; ERROR </span>' : '';
                }
            },
            {
                "defaultContent": '<button class="btn btn-info btn-ver btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
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

    let $passwordInput = $('#txtContraseñaCertificado');
    let $togglePasswordButton = $('#togglePassword');

    $togglePasswordButton.on('mousedown', function () {
        $passwordInput.attr('type', 'text');
    });

    $togglePasswordButton.on('mouseup mouseleave', function () {
        $passwordInput.attr('type', 'password');
    });

    // Evitar que el botón reciba el foco
    $togglePasswordButton.on('click', function (e) {
        e.preventDefault();
    });
})

const openModalFactura = (model = BASIC_MODEL_FACTURACION) => {

    url_qr = null;

    $("#txtId").val(model.idFacturaEmitida);
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

    if (model.errores != null) {
        document.getElementById("divError").style.display = '';
        $("#txtError").text(model.errores);
    }
    else {
        document.getElementById("divError").style.display = 'none';
        if (model.qr != '') {
            url_qr = model.qr;
        }
    }

    $("#modalData").modal("show")
}
let url_qr = null;

$("#tbData tbody").on("click", ".btn-info", function () {

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

const openModalAjustesFacturacion = (model = BASIC_MODEL_AJUSTES_FACTURACION) => {


    $("#txtIdAjustesFacturacion").val(model.idAjustesFacturacion);

    $("#cboCondicionIva").val(model.condicionIva);
    $("#txtPuntoVentaCertificado").val(model.puntoVenta);
    $("#txtContraseñaCertificado").val(model.certificadoPassword);

    $("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.certificadoFechaInicio));
    $("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.certificadoFechaCaducidad));
    $("#txtCuilCertificado").val(model.cuit);
    $("#txtNombreArchivo").val(model.certificadoNombre);

    //if (model.vMX509Certificate2 != null) {
        //$("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notBefore));
        //$("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notAfter));
        //$("#txtCuil").val(model.vMX509Certificate2.cuil);
    //}

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalDataAjustesFscturacion").modal("show")
}


$("#btnSaveAjustesFacturacion").on("click", function () {
    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_AJUSTES_FACTURACION);
    model["idAjustesFacturacion"] = parseInt($("#txtIdAjustesFacturacion").val());

    model["puntoVenta"] = parseInt($("#txtPuntoVentaCertificado").val());
    model["condicionIva"] = parseInt($("#cboCondicionIva").val());
    model["certificadoPassword"] = $("#txtContraseñaCertificado").val();

    const inputCertificado = document.getElementById('fileCertificado');

    const formData = new FormData();
    formData.append('Certificado', inputCertificado.files[0]);
    formData.append('model', JSON.stringify(model));

    $("#modalDataAjustesFscturacion").find("div.modal-content").LoadingOverlay("show")

    fetch("/Admin/UpdateAjustesFacturacion", {
        method: "PUT",
        body: formData
    }).then(response => {
        $("#modalDataAjustesFscturacion").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        $("#modalDataAjustesFscturacion").modal("hide");

        if (responseJson.state) {

            $("#modalDataAjustesFscturacion").modal("hide");
            swal("Exitoso!", "Los Ajustes de Facturacion fueron modificado", "success");
        }
        else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataAjustesFscturacion").find("div.modal-content").LoadingOverlay("hide")
    })

})

function formatDateToDDMMYYYY(isoDate) {
    const date = new Date(isoDate);

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}