let tableDataBackup;
let rowSelectedBackup;
let tableDataTablaProductoBackup;

$(document).ready(function () {


    tableDataBackup = $("#tbDataBackup").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetBackups",
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
            {
                "data": "id",
                "visible": false,
                "searchable": false
            },
            {
                "data": "correlativeNumberMasivo", render: function (data) {
                    return data != null ? 'MASIVO (' + data + ')' : '';
                }
            },
            { "data": "description" },
            { "data": "registrationDate" },
            { "data": "registrationUser" },
            {
                "defaultContent": '<button class="btn btn-info btn-view btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "100px"
            }
        ],
        order: [[3, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Backup',
                exportOptions: {
                    columns: [2, 3, 4]
                }
            }, 'pageLength'
        ]
    });

})


$("#tbDataBackup tbody").on("click", ".btn-view", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedBackup = $(this).closest('tr').prev();
    } else {
        rowSelectedBackup = $(this).closest('tr');
    }

    const data = tableDataBackup.row(rowSelectedBackup).data();

    openModalBackup(data);
})

function openModalBackup(model) {
    var dateTimeModif = new Date(model.registrationDate);

    $("#txtModificado").val(dateTimeModif.toLocaleString());
    $("#txtModificadoUsuario").val(model.registrationUser);
    $("#txtId").val(model.id);
    $("#txtCorrelativeNumberMasivo").val(model.correlativeNumberMasivo);

    $("#modalDataBackup").modal("show")

    let dd = [];
    dd.push(model);

    if (tableDataTablaProductoBackup != null)
        tableDataTablaProductoBackup.destroy();

    tableDataTablaProductoBackup = $("#tbDataProductosBackup").DataTable({
        responsive: true,
        data: model.products, // Aquí pasamos el array directamente
        columns: [
            { data: "description", title: "Producto" },
            { data: "tipoVenta", title: "TipoVenta", render: data => data == 1 ? "Kg" : "U" },
            { data: "iva", title: "Iva", render: data => data + " %" },
            { data: "costPrice", title: "Costos", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "precio1", title: "Precio (1)", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "porcentajeProfit1", title: "Profit(1)", render: data => data + " %" },
            { data: "precio2", title: "Precio (2)", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "porcentajeProfit2", title: "Profit(2)", render: data => data + " %" },
            { data: "precio3", title: "Precio (3)", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "porcentajeProfit3", title: "Profit(3)", render: data => data + " %" },
            { data: "priceWeb", title: "Precio Web", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "formatoWeb", title: "FormatoWeb" },
            { data: "precioFormatoWeb", title: "Precio Formato Web", render: $.fn.dataTable.render.number(',', '.', 0, '$ ') },
            { data: "category", title: "Categoria" },
            { data: "proveedor", title: "Proveedor" },
            { data: "isActive", title: "Estado", render: data => data ? "Activo" : "Inactivo" },
            { data: "destacado", title: "Destacado", render: data => data ? "Sí" : "No" },
            { data: "productoWeb", title: "ProductoWeb", render: data => data ? "Sí" : "No" },
            { data: "modificarPrecio", title: "Modificar Precio", render: data => data ? "Sí" : "No" },
            { data: "precioAlMomento", title: "Precio Al Momento", render: data => data ? "Sí" : "No" },
            { data: "excluirPromociones", title: "Excluir Promociones", render: data => data ? "Sí" : "No" }
        ],
        scrollX: true, // Habilita scroll horizontal
        scrollY: "400px", // Habilita scroll vertical con una altura máxima
        paging: false, // Desactiva el paginado
        dom: "t" // Solo muestra la tabla (sin botones ni paginación)
    });
}


$("#btnRestoreBackup").on("click", function () {

    let idBackup = $("#txtId").val();
    let correlativeNumberMasivo = $("#txtCorrelativeNumberMasivo").val();
    $("#btnRestoreBackup").LoadingOverlay("show")


    fetch(`/Inventory/RestoreBackup?idBackup=${idBackup}&correlativeNumber=${correlativeNumberMasivo}`, {
        method: "POST"
    }).then(response => {
        $(".showSweetAlert").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {

        $("#btnRestoreBackup").LoadingOverlay("hide")
        if (responseJson.state) {

            $("#modalDataBackup").modal("hide")
            swal("Completado", "El backup se ha restaurado de forma correcta", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
        .catch((error) => {
            $(".showSweetAlert").LoadingOverlay("hide")
        })

})

