let tableDataReportPrecios;

$(document).ready(function () {

    $("#txtModificadoDate").datepicker({ dateFormat: 'dd/mm/yy' });

    fetch("/Inventory/GetCategories")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            $("#cboCategory").append(
                $("<option>").val('Todo').text('Todo')
            )
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboCategory").append(
                        $("<option>").val(item.idCategory).text(item.description)
                    )
                });

            }
        })
})

$("#btnSearch").click(function () {

    let modificationDate = $("#txtModificadoDate").val();

    let idCategory = $('#cboCategory').val();

    if (tableDataReportPrecios != undefined)
        tableDataReportPrecios.destroy();

    var options = {
        "processing": true,
        pageLength: 25,
        "ajax": {
            "url": `/Reports/GetPreciosReport?categoria=${idCategory}&modificationDate=${modificationDate}`,
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [3],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            { "data": "productName" },
            { "data": "categoria" },
            {
                "data": "Price", render: function (data, type, row) {
                    return "<span> $ " + row.precio1 + " / " + row.tipoVenta + " </span>";
                }
            },
            { "data": "modificationDate" },
            { "data": "modificationUser" }
        ],
        order: [[3, "desc"]],
        "scrollX": true,
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Precios',
            }, 'pageLength'
        ]
    };


    tableDataReportPrecios = $('#tbdata').DataTable(options);
})

function setToday() {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    $('#txtModificadoDate').datepicker('setDate', today);

    $("#btnSearch").click();
}