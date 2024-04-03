$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Notification/GetNotificaciones",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idNotifications",
                "visible": false,
                "searchable": false
            },
            { "data": "descripcion" },
            { "data": "registrationDateString" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            { "data": "modificationUser" },
            { "data": "modificationDateString" }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Notificaciones',
                exportOptions: {
                    columns: [1,2]
                }
            }, 'pageLength'
        ]
    });
})