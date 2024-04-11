var proveedoresList = [];

const BASIC_MODEL = {
    idPedido: 0,
    importeEstimado: 0,
    estado: 1,
    comentario: "",
    idProveedor: 0,
    registrationDate: null,
    registrationUser: "",
    fechaCreacion: null,
    cantidadProductos: 0,
    productos: []
}

$(document).ready(function () {
    showLoading();

    fetch("/Admin/GetProveedoresConProductos")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.data.length > 0) {
                proveedoresList = responseJson.data;

                responseJson.data.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });

                removeLoading();
            }
        })

    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Pedido/GetPedidos",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idPedido",
                "visible": false,
                "searchable": false
            },
            { "data": "proveedor.nombre" },
            { "data": "registrationDateString" },
            { "data": "importeEstimadoString" },
            {
                "data": "estado",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-danger">Cancelado</span>';
                    else if (data == 1)
                        return '<span class="badge rounded-pill bg-info text-dark">Iniciado</span>';
                    else if (data == 2)
                        return '<span class="badge rounded-pill bg-primary text-dark">Enviado</span>';
                    else
                        return '<span class="badge rounded-pill bg-success text-dark">Recibido</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
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
                filename: 'Reporte Pedidos',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    new ClipboardJS('#btnCopiar');

    $(document).on('blur', '.editable', function () {
        var input = parseInt($(this).text());
        if (isNaN(input) || input < 1) {
            $(this).text('');
        }
    });

})

$('#btnVolver').on('click', function () {
    $('#panelGenerarMensaje').hide();
    $('#panelProductos').show();
});

$('#tablaProductos tbody').on('blur', '.editable', function () {
    calcularTotalCosto();
});

function calcularTotalCosto() {
    let totalCosto = 0;
    $('#tablaProductos tbody').find('tr').each(function () {
        let cantCelda = $(this).find('.editable').text();
        var cantidad = parseInt(cantCelda != '' ? cantCelda : 0);

        if (isNaN(cantidad) || cantidad < 1) {
            cantidad = 0;
        }
        const costo = parseInt($(this).find('td:eq(2)').text());
        totalCosto += cantidad * costo;

    });
    $('#txtImporteEstimado').val(totalCosto);
}

const openModal = (model = BASIC_MODEL) => {
    $('#btnVolver').click();

    $("#txtIdPedido").val(model.idPedido);

    if (model.idPedido != 0) {
        cargarTabla(model.productos, model.idProveedor, model.estado == 1);

        $('#cboProveedor').val(model.idProveedor);
        $('#cboProveedor').prop('disabled', true);
        $("#cboEstado").val(model.estado);

    }
    else {
        $('#cboProveedor').prop('disabled', false);
    }

    $("#modalData").modal("show")
}

$("#btnNew").on("click", function () {
    openModal()
    $("#cboEstado").val(1);
})


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();


    openModal(data);
    calcularTotalCosto();
})

$("#btnGenerar").on("click", function () {
    var texto = 'Pedido:<br><br>';
    var alMenosUno = false;

    $('#tablaProductos').find('tbody tr').each(function () {
        let cant = $(this).find('td:eq(4)').text();
        var cantidad = parseInt(cant != null ? cant : 0);
        if (cantidad > 0) {
            alMenosUno = true;
            texto += '- ' + $(this).find('td:eq(1)').text() + ' x ' + cant.toLowerCase() + '<br>';
        }
    });

    if (alMenosUno) {
        $('#btnCopiar').attr('data-clipboard-target', '#labelTabla');

        $('#labelTabla').html(texto)
        $('#panelGenerarMensaje').show();
        $('#panelProductos').hide();

        let estado = $("#cboEstado").val();
        if (estado == 1) {
            $("#cboEstado").val(2)
        }
    }
    else {
        $('#panelGenerarMensaje').hide();
        $('#panelProductos').show();
    }

    var celdasEditables = document.querySelectorAll(".editable");
    celdasEditables.forEach(function (celda) {
        celda.addEventListener("click", function () {
            this.focus();
        });

        celda.addEventListener("input", function () {
            var cantidad = parseFloat(this.textContent);
            if (isNaN(cantidad)) {
                cantidad = 0;
            }
            this.textContent = cantidad;
        });
    });
})

$('#cboProveedor').change(function () {

    var idProv = $(this).val();
    var proveedor = proveedoresList.find(_ => _.idProveedor == idProv);

    cargarTabla(proveedor.products, idProv, true);
})

function cargarTabla(productos, idProveedor, nuevo) {
    const tablaBody = document.querySelector('#tablaProductos tbody');
    tablaBody.innerHTML = '';
    var proveedor = proveedoresList.find(_ => _.idProveedor == idProveedor);

    let disable = nuevo ? `class="editable" contenteditable="true"` : "";

    proveedor.products.forEach(producto => {
        const tr = document.createElement('tr');

        let prod = productos.find(_ => _.idProducto == producto.idProduct);

        let cantidadProducto = '';

        let idProd = producto.idProduct;
        let descr = producto.description;
        let costo = producto.costPrice;
        let stock = producto.quantity;
        if (prod != null) {
            cantidadProducto = prod.cantidadProducto;
        }

        tr.innerHTML = `
                              <td hidden>${idProd}</td>
                              <td>${descr}</td>
                              <td>${costo}</td>
                              <td>${stock != null ? stock : 0}</td>
                              <td ${disable} inputmode="numeric" pattern="[0-9]*" min="0" >${cantidadProducto}</td>
                            `;
        tablaBody.appendChild(tr);
    });
}

$('#modalData').on('hidden.bs.modal', function () {
    limpiarTablaYTotal();
});

function limpiarTablaYTotal() {
    $('#tablaProductos tbody').empty();
    $('#txtImporteEstimado').val(0);
    $('#cboProveedor').val('')
    $('#txtComentario').val('')
}

$("#btnSave").on("click", function () {
    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    calcularTotalCosto();
    const model = structuredClone(BASIC_MODEL);
    model["idPedido"] = parseInt($("#txtIdPedido").val());
    model["importeEstimado"] = parseFloat($("#txtImporteEstimado").val());
    model["comentario"] = $("#txtComentario").val();
    model["idProveedor"] = $('#cboProveedor').val();
    model["cantidadProductos"] = $("#txtDescripcionTipoDeGasto").val();
    model["estado"] = parseInt($("#cboEstado").val());

    var productosConCantidad = [];

    $('#tablaProductos tbody tr').each(function () {
        var cantidadPedir = parseInt($(this).find('td:last-child').text());

        if (!isNaN(cantidadPedir) && cantidadPedir > 0) {
            var producto = {
                idProducto: parseInt($(this).find('td:eq(0)').text()),
                cantidadProducto: cantidadPedir
            };
            productosConCantidad.push(producto);
        }
    });
    model["productos"] = productosConCantidad;
    model["cantidadProductos"] = productosConCantidad.length;

    if (model.idPedido == 0) {
        fetch("/Pedido/CreatePedido", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {
                tableData.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "Pedido fue creado", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }
    else {

        fetch("/Pedido/UpdatePedidos", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {
                $("#modalData").modal("hide");
                swal("Exitoso!", "Pedido fue modificado", "success");
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})


$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableData.row(row).data();

    if (data.estado == 1) {

        swal({
            title: "Esta seguro?",
            text: `Eliminar pedido de "${data.proveedor.nombre} por $${data.importeEstimado}"`,
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

                    fetch(`/Pedido/DeletePedido?idPedido=${data.idPedido}`, {
                        method: "DELETE"
                    }).then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                        return response.ok ? response.json() : Promise.reject(response);
                    }).then(responseJson => {
                        if (responseJson.state) {

                            tableData.row(row).remove().draw();
                            swal("Exitoso!", "El pedido  fue eliminada", "success");

                        } else {
                            swal("Lo sentimos", responseJson.message, "error");
                        }
                    })
                        .catch((error) => {
                            $(".showSweetAlert").LoadingOverlay("hide")
                        })
                }
            });
    }
})