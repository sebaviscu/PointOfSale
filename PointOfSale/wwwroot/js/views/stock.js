let tableDataStock;
let rowSelectedStock;
let tableDataVencimiento;
let rowSelectedVencimiento;
let productSelector;

const BASIC_MODEL_VENCIMIENTO = {
    idVencimiento: 0,
    lote: null,
    fechaVencimiento: null,
    fechaElaboracion: null,
    notificar: null,
    idProducto: 0,
    idTienda: 0
}

$(document).ready(function () {

    cargarTablaVencimientos();

    cargarSelect2();
    $('.filtro-vencimientos').change(function () {
        filtrarTabla();
    });
})

function cargarTablaVencimientos() {

    tableDataVencimiento = $("#tbDataVencimientos").DataTable({
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
                "targets": [2, 4],
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
            { "data": "lote" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false
            }
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

    //$("#txtfVencimiento").datepicker({ dateFormat: 'dd/mm/yy' });
    //$("#txtfElaborado").datepicker({ dateFormat: 'dd/mm/yy' });
}

function filtrarTabla() {
    let valorSeleccionado = $('.filtro-vencimientos:checked').val();

    $('#tbDataVencimientos tbody tr').hide();

    if (valorSeleccionado === '0') {
        $('#tbDataVencimientos tbody tr').show();
    } else if (valorSeleccionado === '1') {
        $('.proximoClass').show();
    } else if (valorSeleccionado === '2') {
        $('.vencidoClass').show();
    }
}

function cargarSelect2() {

    productSelector = new Choices('#cboProducto', {
        removeItemButton: true,
        maxItemCount: 1,
        searchResultLimit: 3,
        shouldSort: false,
        duplicateItemsAllowed: false,
        allowHTML: true
    });

    fetch('/Inventory/GetProductsActive')
        .then(response => response.json())
        .then(data => {
            const prods = data.data.map(prod => ({
                value: prod.idProduct,
                label: `${prod.description}`
            }));
            productSelector.setChoices(prods, 'value', 'label', false);
        });
}

$("#btnNewVencimiento").on("click", function () {
    openModalVencimiento()
})

const openModalVencimiento = (model = BASIC_MODEL_VENCIMIENTO) => {

    if (model.idVencimiento != 0) {
        productSelector.setChoiceByValue(parseInt(model.idProducto));

    } else {
        productSelector.removeActiveItems();
    }


    let fechaFormateadaVencimiento = model.fechaVencimiento != null ? model.fechaVencimiento.split('T')[0] : '';
    let fechaFormateadaElaborado = model.fechaElaboracion != null ? model.fechaElaboracion.split('T')[0] : '';

    $("#txtId").val(model.idVencimiento);
    $("#txtLote").val(model.lote);
    $("#txtfVencimiento").val(fechaFormateadaVencimiento);
    $("#txtfElaborado").val(fechaFormateadaElaborado);
    document.getElementById('switchNotiicar').checked = model.notificar;


    $("#modalDataVencimiento").modal("show")
}

$("#btnSaveVencimiento").on("click", function () {

    const model = structuredClone(BASIC_MODEL_VENCIMIENTO);
    model["idVencimiento"] = $("#txtId").val();
    model["idProducto"] = productSelector.getValue(true);
    model["lote"] = $("#txtLote").val();
    model["lote"] = $("#txtLote").val();
    model["fechaVencimiento"] = $("#txtfVencimiento").val();
    model["fechaElaboracion"] = $("#txtfElaborado").val();
    model["notificar"] = document.getElementById('switchNotiicar').checked;

    $("#modalDataVencimiento").find("div.modal-content").LoadingOverlay("show")

    if (model.idVencimiento == 0) {
        fetch("/Inventory/CreateVencimiento", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataVencimiento").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataVencimiento.row.add(responseJson.object).draw(false);
                $("#modalDataVencimiento").modal("hide");
                swal("Exitoso!", "El Vencimeinto fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataVencimiento").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Inventory/UpdateVencimientoes", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataVencimiento").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataVencimiento.row(rowSelectedVencimiento).data(responseJson.object).draw(false);
                rowSelectedVencimiento = null;
                $("#modalDataVencimiento").modal("hide");
                swal("Exitoso!", "El Vencimiento fué modificada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataVencimiento").find("div.modal-content").LoadingOverlay("hide")
        })
    }
})

$("#tbDataVencimientos tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedVencimiento = $(this).closest('tr').prev();
    } else {
        rowSelectedVencimiento = $(this).closest('tr');
    }

    const data = tableDataVencimiento.row(rowSelectedVencimiento).data();

    openModalVencimiento(data);
})

$("#tbDataVencimientos tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataVencimiento.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar Vencimiento "${data.fechaVencimiento}"`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Inventory/DeleteVencimiento?idVencimiento=${data.idVencimiento}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataVencimiento.row(row).remove().draw();
                        swal("Exitoso!", "Promociones  fué eliminada", "success");

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                })
                    .catch((error) => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                    })
            }
        });
})
