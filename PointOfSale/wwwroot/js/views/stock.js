let tableData;
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
            { "data": "fechaVencimientoString" },
            { "data": "producto" },
            { "data": "fechaElaboracionString" },
            { "data": "lote" }
        ],
        order: [[1, "desc"]],
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