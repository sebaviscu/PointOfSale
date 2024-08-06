let tableData;
$(document).ready(function () {


    tableData = $("#tbData").DataTable({
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
                "data": "sale.total", render: function (data, type, row) {
                    return `$ ${data}`;
                }
            },
            { "data": "cae" },
            { "data": "caeVencimiento" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
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
                    columns: [1,2]
                }
            }, 'pageLength'
        ]
    });

})