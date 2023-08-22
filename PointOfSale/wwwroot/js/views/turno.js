let tableData;
let rowSelected;

const BASIC_MODEL = {
    idturno: 0,
    nombre: "",
    modificationDate: null,
    modificationUser: null
}


$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/turno/GetTurnos",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTurno",
                "visible": false,
                "searchable": false
            },
            { "data": "fecha" },
            { "data": "horaInicio" },
            { "data": "horaFin" },
            { "data": "modificationUser" },
            { "data": "descripcion" },
            { "data": "total" },
            {
                "defaultContent": '<button class="btn btn-primary btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte turnos',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });
})