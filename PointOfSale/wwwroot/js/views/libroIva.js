let tbdataVenta;
let tbdataCompra;
let tbdataServicios;
let tbdataGastos;

$(document).ready(function () {
    showLoading();

    fetch("/Reports/GetFechasReporteIva")
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(responseJson => {
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#txtFecha").append(
                        $("<option>").val(item.dateId).text(item.dateText)
                    );
                });
            }
        })
        .catch(error => {
            console.error('Error fetching fechas:', error);
        });

    removeLoading();
})



$("#btnSearch").click(function () {

    let fecha = $("#txtFecha").val();

    if (fecha.trim() == "") {
        toastr.warning("", "Debes seleccionar una fecha");
        return;
    }
    showLoading();

    $("#txtNetoFinalCredito").attr("importe", parseFloat(0));
    $("#txtImporteIvaFinalCredito").attr("importe", parseFloat(0));
    $("#txtFacturadoFinalCredito").attr("importe", parseFloat(0));
    $("#txtNetoFinalDebito").attr("importe", parseFloat(0));
    $("#txtImporteIvaFinalDebito").attr("importe", parseFloat(0));
    $("#txtFacturadoFinalDebito").attr("importe", parseFloat(0));

    LoadCompra(fecha);
    LoadVenta(fecha);
    LoadServiciosGastos(fecha);

    removeLoading();

})


function setTotalesCredito(totalNetoCredito, totalIvaCredito, totalFacturadoCredito) {

    let txtNetoFinalCreditoImporte = parseFloat($("#txtNetoFinalCredito").attr("importe"));
    let txtImporteIvaFinalCreditoImporte = parseFloat($("#txtImporteIvaFinalCredito").attr("importe"));
    let txtFacturadoFinalCreditoImporte = parseFloat($("#txtFacturadoFinalCredito").attr("importe"));

    $("#txtNetoFinalCredito").attr("importe", parseFloat(txtNetoFinalCreditoImporte + totalNetoCredito).toFixed(2));
    $("#txtImporteIvaFinalCredito").attr("importe", parseFloat(txtImporteIvaFinalCreditoImporte + totalIvaCredito).toFixed(2));
    $("#txtFacturadoFinalCredito").attr("importe", parseFloat(txtFacturadoFinalCreditoImporte + totalFacturadoCredito).toFixed(2));

    $("#txtNetoFinalCredito").val("$ " + parseFloat(txtNetoFinalCreditoImporte + totalNetoCredito).toFixed(2));
    $("#txtImporteIvaFinalCredito").val("$ " + parseFloat(txtImporteIvaFinalCreditoImporte + totalIvaCredito).toFixed(2));
    $("#txtFacturadoFinalCredito").val("$ " + parseFloat(txtFacturadoFinalCreditoImporte + totalFacturadoCredito).toFixed(2));

    let txtImporteCreditoFinal = parseFloat($("#txtImporteIvaFinalCredito").attr("importe"));
    let txtImporteDebitoFinal = parseFloat($("#txtImporteIvaFinalDebito").attr("importe"));
    $("#txtSaldo").val("$ " + parseFloat(txtImporteDebitoFinal - txtImporteCreditoFinal).toFixed(2));
}

function setTotalesDebito(totalNetoDebito, totalIvaDebito, totalFacturadoDebito) {

    let txtNetoFinalDebitoImporte = parseFloat($("#txtNetoFinalDebito").attr("importe"));
    let txtImporteIvaFinalDebitoImporte = parseFloat($("#txtImporteIvaFinalDebito").attr("importe"));
    let txtFacturadoFinalDebitoImporte = parseFloat($("#txtFacturadoFinalDebito").attr("importe"));

    $("#txtNetoFinalDebito").attr("importe", parseFloat(txtNetoFinalDebitoImporte + totalNetoDebito).toFixed(2));
    $("#txtImporteIvaFinalDebito").attr("importe", parseFloat(txtImporteIvaFinalDebitoImporte + totalIvaDebito).toFixed(2));
    $("#txtFacturadoFinalDebito").attr("importe", parseFloat(txtFacturadoFinalDebitoImporte + totalFacturadoDebito).toFixed(2));

    $("#txtNetoFinalDebito").val("$ " + parseFloat(txtNetoFinalDebitoImporte + totalNetoDebito).toFixed(2));
    $("#txtImporteIvaFinalDebito").val("$ " + parseFloat(txtImporteIvaFinalDebitoImporte + totalIvaDebito).toFixed(2));
    $("#txtFacturadoFinalDebito").val("$ " + parseFloat(txtFacturadoFinalDebitoImporte + totalFacturadoDebito).toFixed(2));

    let txtImporteCreditoFinal = parseFloat($("#txtImporteIvaFinalCredito").attr("importe"));
    let txtImporteDebitoFinal = parseFloat($("#txtImporteIvaFinalDebito").attr("importe"));
    $("#txtSaldo").val("$ " + parseFloat(txtImporteDebitoFinal - txtImporteCreditoFinal).toFixed(2));
}

function LoadCompra(fecha) {

    fetch(`/Reports/GetIvaReport?idTipoIva=0&date=${fecha}`, {
        method: "GET"
    })
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                let data = responseJson.object;

                if (data && data.length == 2) {

                    setTotalesCredito(data[0].totalSinIva, data[0].totalIva, data[0].totalFacurado);

                    $("#txtNetoCredito105").val("$ " + data[0].totalSinIva);
                    $("#txtImporteIvaCredito105").val("$ " + data[0].totalIva);
                    $("#txtFacturadoCredito105").val("$ " + data[0].totalFacurado);

                    setTotalesCredito(data[1].totalSinIva, data[1].totalIva, data[1].totalFacurado);

                    $("#txtNetoCredito21").val("$ " + data[1].totalSinIva);
                    $("#txtImporteIvaCredito21").val("$ " + data[1].totalIva);
                    $("#txtFacturadoCredito21").val("$ " + data[1].totalFacurado);

                    if (tbdataCompra != null)
                        tbdataCompra.destroy();

                    let rowsServicios = data[0].ivaRows.concat(data[1].ivaRows);

                    tbdataCompra = $("#tbdataCompra").DataTable({
                        responsive: true,
                        data: rowsServicios,
                        "columns": [

                            { "data": "fechaString" },
                            { "data": "proveedor" },
                            { "data": "tipoFactura" },
                            { "data": "factura" },
                            {
                                "data": "importe", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importe + " </span>";
                                }
                            },
                            {
                                "data": "importeIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeIva + " </span>";
                                }
                            },
                            {
                                "data": "importeSinIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeSinIva + " </span>";
                                }
                            },
                        ],
                        order: [[0, "desc"]],
                        dom: "Bfrtip",
                        buttons: [
                            {
                                text: 'Exportar Excel',
                                extend: 'excelHtml5',
                                title: '',
                                filename: 'Reporte Compras IVA',
                                exportOptions: {
                                    columns: [1, 2, 3, 4, 5, 6, 7]
                                }
                            }, 'pageLength'
                        ]
                    });
                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {

        });

}

function LoadVenta(fecha) {

    fetch(`/Reports/GetIvaReport?idTipoIva=1&date=${fecha}`, {
        method: "GET"
    })
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                let data = responseJson.object;

                if (data && data.length == 4) {

                    setTotalesDebito(data[0].totalSinIva, data[0].totalIva, data[0].totalFacurado);

                    $("#txtNetoADebito").val("$ " + data[0].totalSinIva);
                    $("#txtImporteIvaADebito").val("$ " + data[0].totalIva);
                    $("#txtFacturadoADebito").val("$ " + data[0].totalFacurado);

                    setTotalesDebito(data[1].totalSinIva, data[1].totalIva, data[1].totalFacurado);

                    $("#txtNetoBDebito").val("$ " + data[1].totalSinIva);
                    $("#txtImporteIvaBDebito").val("$ " + data[1].totalIva);
                    $("#txtFacturadoBDebito").val("$ " + data[1].totalFacurado);

                    setTotalesDebito(data[2].totalSinIva, data[2].totalIva, data[2].totalFacurado);

                    $("#txtNetoCDebito").val("$ " + data[2].totalSinIva);
                    $("#txtImporteIvaCDebito").val("$ " + data[2].totalIva);
                    $("#txtFacturadoCDebito").val("$ " + data[2].totalFacurado);

                    setTotalesDebito(data[3].totalSinIva, data[3].totalIva, data[3].totalFacurado);

                    $("#txtNetoXDebito").val("$ " + data[3].totalSinIva);
                    $("#txtImporteIvaXDebito").val("$ " + data[3].totalIva);
                    $("#txtFacturadoXDebito").val("$ " + data[3].totalFacurado);

                    let rowsServicios = data[0].ivaRows.concat(data[1].ivaRows);
                    rowsServicios = rowsServicios.concat(data[2].ivaRows);
                    rowsServicios = rowsServicios.concat(data[3].ivaRows);

                    if (tbdataVenta != null)
                        tbdataVenta.destroy();

                    tbdataVenta = $("#tbdataVenta").DataTable({
                        responsive: true,
                        data: rowsServicios,
                        "columns": [

                            { "data": "fechaString" },
                            { "data": "metodoPago" },
                            {
                                "data": "importe", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importe + " </span>";
                                }
                            },
                            {
                                "data": "importeIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeIva + " </span>";
                                }
                            },
                            {
                                "data": "importeSinIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeSinIva + " </span>";
                                }
                            },
                        ],
                        order: [[0, "desc"]],
                        dom: "Bfrtip",
                        buttons: [
                            {
                                text: 'Exportar Excel',
                                extend: 'excelHtml5',
                                title: '',
                                filename: 'Reporte Compras IVA',
                                exportOptions: {
                                    columns: [1, 2, 3, 4, 5, 6, 7]
                                }
                            }, 'pageLength'
                        ]
                    });
                }
            };
        });
}

function LoadServiciosGastos(fecha) {
    fetch(`/Reports/GetIvaReport?idTipoIva=2&date=${fecha}`, {
        method: "GET"
    })
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                let data = responseJson.object;

                if (data && data.length == 3) {

                    setTotalesCredito(data[0].totalSinIva, data[0].totalIva, data[0].totalFacurado);

                    $("#txtNetoCreditoServ21").val("$ " + data[0].totalSinIva);
                    $("#txtImporteIvaCreditoServ21").val("$ " + data[0].totalIva);
                    $("#txtFacturadoCreditoServ21").val("$ " + data[0].totalFacurado);

                    setTotalesCredito(data[1].totalSinIva, data[1].totalIva, data[1].totalFacurado);

                    $("#txtNetoCreditoServ27").val("$ " + data[1].totalSinIva);
                    $("#txtImporteIvaCreditoServ27").val("$ " + data[1].totalIva);
                    $("#txtFacturadoCreditoServ27").val("$ " + data[1].totalFacurado);

                    setTotalesCredito(data[2].totalSinIva, data[2].totalIva, data[2].totalFacurado);

                    $("#txtNetoCreditoGastos21").val("$ " + data[2].totalSinIva);
                    $("#txtImporteIvaCreditoGastos21").val("$ " + data[2].totalIva);
                    $("#txtFacturadoCreditoGastos21").val("$ " + data[2].totalFacurado);

                    let rowsServicios = data[0].ivaRows.concat(data[1].ivaRows);
                    rowsServicios = rowsServicios.concat(data[2].ivaRows);

                    if (tbdataServicios != null)
                        tbdataServicios.destroy();

                    tbdataServicios = $("#tbdataServicios").DataTable({
                        responsive: true,
                        data: rowsServicios,
                        "columns": [

                            { "data": "fechaString" },
                            { "data": "gastos" },
                            { "data": "tipoFactura" },
                            { "data": "factura" },
                            {
                                "data": "importe", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importe + " </span>";
                                }
                            },
                            {
                                "data": "importeIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeIva + " </span>";
                                }
                            },
                            {
                                "data": "importeSinIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeSinIva + " </span>";
                                }
                            },
                        ],
                        order: [[0, "desc"]],
                        dom: "Bfrtip",
                        buttons: [
                            {
                                text: 'Exportar Excel',
                                extend: 'excelHtml5',
                                title: '',
                                filename: 'Reporte Compras IVA',
                                exportOptions: {
                                    columns: [1, 2, 3, 4, 5, 6, 7]
                                }
                            }, 'pageLength'
                        ]
                    });

                    if (tbdataGastos != null)
                        tbdataGastos.destroy();

                    tbdataGastos = $("#tbdataGastos").DataTable({
                        responsive: true,
                        data: data[2].ivaRows,
                        "columns": [

                            { "data": "fechaString" },
                            { "data": "tipoGastos" },
                            { "data": "gastos" },
                            { "data": "tipoFactura" },
                            { "data": "factura" },
                            {
                                "data": "importe", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importe + " </span>";
                                }
                            },
                            {
                                "data": "importeIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeIva + " </span>";
                                }
                            },
                            {
                                "data": "importeSinIva", "className": "text-end", render: function (data, type, row) {
                                    return "<span>$ " + row.importeSinIva + " </span>";
                                }
                            },
                        ],
                        order: [[0, "desc"]],
                        dom: "Bfrtip",
                        buttons: [
                            {
                                text: 'Exportar Excel',
                                extend: 'excelHtml5',
                                title: '',
                                filename: 'Reporte Compras IVA',
                                exportOptions: {
                                    columns: [1, 2, 3, 4, 5, 6, 7]
                                }
                            }, 'pageLength'
                        ]
                    });
                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {

        });

}