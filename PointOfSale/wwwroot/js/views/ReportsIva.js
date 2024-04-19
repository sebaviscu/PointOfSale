let tbdataComprador;
let tbdataVendedor;
let tbdataServicios;

$(document).ready(function () {

    fetch("/Reports/GetFechasReporteIva")
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok');
            }
            return response.json();
        })
        .then(responseJson => {
            console.log('Fechas recibidas:', responseJson);
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
})


$("#btnSearch").click(function () {

    let fecha = $("#txtFecha").val();

    if (fecha.trim() == "") {
        toastr.warning("", "Debes ingresar una fecha");
        return;
    }

    let idTipoIva = $('#cboTipoIva').val();

    switch (idTipoIva) {
        case "0":
            IvaCompra(fecha);
            $("#div-ivaComprador").show();
            $("#div-ivaVendedor, #div-ivaServicios").hide();
            break;
        case "1":
            IvaVenta(fecha);
            $("#div-ivaVendedor").show();
            $("#div-ivaComprador, #div-ivaServicios").hide();
            break;
        case "2":
            IvaServicios(fecha);
            $("#div-ivaServicios").show();
            $("#div-ivaComprador, #div-ivaVendedor").hide();
            break;
        default:
            break;
    }

    $("#lblTotal").html("Ventas Total: <strong> $ .</strong>");
    $("#lblTotalSinIva").html("Total IVA: <strong>$ .</strong>");
    $("#lblTotalIva").html("Total sin IVA: <strong>$ .</strong>");
})

function IvaCompra(fecha) {

    if (typeof tbdataComprador !== 'undefined' && tbdataComprador !== null) {
        tbdataComprador.destroy();
    }

    var options = {
        "processing": true,
        "ajax": {
            "url": `/Reports/GetIvaReport?idTipoIva=0&date=${fecha}`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "productName" },
            { "data": "categoria" },
        ],
        order: [[8, "desc"]],
        "scrollX": true,
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Iva Compra',
            }, 'pageLength'
        ]
    };

    tbdataComprador = $('#tbdataComprador').DataTable(options);
}

function IvaVenta(fecha) {

    (tbdataVendedor != undefined)
    tbdataVendedor.destroy();

    var options = {
        "processing": true,
        "ajax": {
            "url": `/Reports/GetIvaReport?idTipoIva=1&date=${fecha}`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "productName" },
            { "data": "categoria" },
            { "data": "proveedor" },
            {
                "data": "precio1", render: function (data, type, row) {
                    return "<span>" + row.precio1 + " / " + row.tipoVenta + " </span>";
                }
            },
            {
                "data": "precio2", render: function (data, type, row) {
                    return "<span>" + row.precio2 + " / " + row.tipoVenta + " </span>";
                }
            },
            {
                "data": "precio3", render: function (data, type, row) {
                    return "<span>" + row.precio3 + " / " + row.tipoVenta + " </span>";
                }
            },
            { "data": "costo" },
            { "data": "stock" },
            { "data": "cantidad" }
        ],
        order: [[8, "desc"]],
        "scrollX": true,
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Iva Venta',
            }, 'pageLength'
        ]
    };


    tbdataVendedor = $('#tbdataVendedor').DataTable(options);
}


function IvaServicios(fecha) {

    (tbdataServicios != undefined)
    tbdataServicios.destroy();

    var options = {
        "processing": true,
        "ajax": {
            "url": `/Reports/GetIvaReport?idTipoIva=2&date=${fecha}`,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "productName" },
            { "data": "categoria" },
            { "data": "proveedor" },
            {
                "data": "precio1", render: function (data, type, row) {
                    return "<span>" + row.precio1 + " / " + row.tipoVenta + " </span>";
                }
            },
            {
                "data": "precio2", render: function (data, type, row) {
                    return "<span>" + row.precio2 + " / " + row.tipoVenta + " </span>";
                }
            },
            {
                "data": "precio3", render: function (data, type, row) {
                    return "<span>" + row.precio3 + " / " + row.tipoVenta + " </span>";
                }
            },
            { "data": "costo" },
            { "data": "stock" },
            { "data": "cantidad" }
        ],
        order: [[8, "desc"]],
        "scrollX": true,
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Iva Servicios',
            }, 'pageLength'
        ]
    };


    tbdataServicios = $('#tbdataServicios').DataTable(options);
}
