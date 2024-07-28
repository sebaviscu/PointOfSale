let tableData;
let tableDataStock;
let rowSelected;

const BASIC_MODEL = {
    idCategory: 0,
    description: "",
    isActive: 1,
    modificationDate: null,
    modificationUser: null
}


$(document).ready(function () {

    cargarTablaVencimientos();

    $('.filtro-vencimientos').change(function () {
        filtrarTabla();
    });
})


function cargarTablaVencimientos() {

    $("#tbDataVencimientos").DataTable({
        createdRow: function (row, data, dataIndex) {
            if (data.estado == 2) {
                $(row).addClass('vencidoClass');
            } else if (data.estado == 1) {
                $(row).addClass('proximoClass');
            } else {
                $(row).addClass('aptoClass');
            }
        },
        pageLength: 25,
        "ajax": {
            "url": "/Inventory/GetVencimientos",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [2,4],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idVencimiento",
                "visible": false,
                "searchable": false
            },
            {
                "data": "estado",
                "visible": false,
                "searchable": false
            },
            { "data": "fechaVencimiento" },
            { "data": "producto" },
            { "data": "fechaElaboracion" },
            { "data": "lote" }
        ],
        order: [[1, "desc"], [2, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Vencimientos',
                exportOptions: {
                    columns: [2, 3, 4, 5]
                }
            }, 'pageLength'
        ],
        drawCallback: function (settings) {
            filtrarTabla();
        }
    });

    tableDataStock = $("#tbDataStock").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetStocks",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idStock",
                "visible": false,
                "searchable": false
            },
            { "data": "producto.description" },
            { "data": "stockActual" },
            { "data": "stockMinimo" },

        ],
        order: [[2, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte categories',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });

}

function filtrarTabla() {
    var valorSeleccionado = $('.filtro-vencimientos:checked').val();

    // Ocultar todas las filas
    $('#tbDataVencimientos tbody tr').hide();

    // Mostrar las filas correspondientes al filtro seleccionado
    if (valorSeleccionado === '0') {
        $('#tbDataVencimientos tbody tr').show();
    } else if (valorSeleccionado === '1') {
        $('.proximoClass').show();
    } else if (valorSeleccionado === '2') {
        $('.vencidoClass').show();
    }
}