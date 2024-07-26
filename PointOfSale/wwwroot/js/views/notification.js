$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Notification/GetNotificaciones",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [2],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idNotifications",
                "visible": false,
                "searchable": false
            },
            { "data": "descripcion" },
            { "data": "registrationDate" },
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
        order: [[2, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Notificaciones',
                exportOptions: {
                    columns: [1,2,3,4,5]
                }
            }, 'pageLength'
        ]
    });
})