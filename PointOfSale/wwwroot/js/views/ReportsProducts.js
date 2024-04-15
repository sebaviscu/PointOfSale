let tableData;

$(document).ready(function () {

    $("#txtStartDate").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#txtEndDate").datepicker({ dateFormat: 'dd/mm/yy' });

    fetch("/Inventory/GetCategories")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboCategory").append(
                $("<option>").val('Todo').text('Todo')
            )
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboCategory").append(
                        $("<option>").val(item.description).text(item.description)
                    )
                });

            }
        })
})



$("#btnSearch").click(function () {

    let startDate = $("#txtStartDate").val();

    if (startDate.trim() == "") {
        toastr.warning("", "Debes ingresar al menos la fecha de inicio");
        return;
    }

    if ($("#txtEndDate").val().trim() == "") {
        $('#txtEndDate').datepicker('setDate', startDate);
    }

    let endDate = $("#txtEndDate").val().trim();
    let idCategory = $('#cboCategory').val();

    if (tableData != undefined)
        tableData.destroy();

    var options = {
        "processing": true,
        "ajax": {
            "url": `/Reports/ProductsReport?idCategoria=${idCategory}&startDate=${startDate}&endDate=${endDate}`,
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
                filename: 'Reporte Productos Vendidos',
            }, 'pageLength'
        ]
    };


    tableData = $('#tbdata').DataTable(options);
})

function setToday() {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    $('#txtStartDate, #txtEndDate').datepicker('setDate', today);

    $("#btnSearch").click();
}