let tableMovimientoCaja;

$(document).ready(function () {
    
    let url = `/MovimientoCaja/GetMovimientosCaja`;

    tableMovimientoCaja = $("#tbData").DataTable({
        responsive: true,
        "columnDefs": [
            {
                "targets": [1],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "ajax": {
            "url": url,
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idMovimientoCaja",
                "visible": false,
                "searchable": false
            },
            { "data": "registrationDate" },
            { "data": "registrationUser" },
            { "data": "razonMovimientoCaja.tipoString" },
            {
                "data": "importe",
                "render": function (data, type, row) {
                    return '$' + parseFloat(data).toFixed(0);
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-view btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "50px"
            }
        ],
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Movimientos de Caja',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });
})
