let proveedoresListPedido = [];
let tableDataPedido;
let rowSelectedPedido;

const BASIC_MODEL_PEDIDO_WEB = {
    idPedido: 0,
    importeEstimado: 0,
    estado: 1,
    comentario: "",
    idProveedor: 0,
    registrationDate: null,
    registrationUser: "",
    cantidadProductos: 0,
    productos: []
}

const BASIC_MODEL_RECIBIR = {
    idPedido: 0,
    importeEstimado: 0,
    estado: 1,
    comentario: "",
    idProveedor: 0,
    registrationDate: null,
    registrationUser: "",
    productos: [],
    tipoFactura: null,
    nroFactura: null,
    iva: null,
    ivaImporte: null,
    importeSinIva: null,
    estadoPago: 0,
    facturaPendiente: 0
}

$(document).ready(function () {
    showLoading();

    fetch("/Proveedores/GetProveedoresConProductos")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                proveedoresListPedido = responseJson.object;

                proveedoresListPedido.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });

                removeLoading();
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        })

    tableDataPedido = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Pedido/GetPedidos",
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
            },
            {
                "targets": [5],
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
                "data": "idPedido",
                "visible": false,
                "searchable": false
            },
            {
                "data": "orden",
                "visible": false,
                "searchable": false
            },
            { "data": "proveedor.nombre" },
            { "data": "registrationDate" },
            { "data": "importeEstimadoString" },
            { "data": "fechaCerrado" },
            {
                "data": "estado",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-danger">Cancelado</span>';
                    else if (data == 1)
                        return '<span class="badge rounded-pill bg-info">Iniciado</span>';
                    else if (data == 2)
                        return '<span class="badge rounded-pill bg-primary">Enviado</span>';
                    else
                        return '<span class="badge rounded-pill bg-success">Recibido</span>';
                }
            },
            {
                "data": "estado",
                "className": "text-center", render: function (data) {
                    if (data == 2)
                        return '<button class="btn btn-secondary btn-sm btn-recibir">Recibir</button>';
                    else
                        return '';
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
        order: [[1, "asc"]],
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
        let input = parseInt($(this).text());
        if (isNaN(input) || input < 0) {
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
        let cantidad = parseInt(cantCelda != '' ? cantCelda : 0);

        if (isNaN(cantidad) || cantidad < 1) {
            cantidad = 0;
        }
        let str = $(this).find('td:eq(2)').text();
        let costoString = str.slice(1).trim();

        const costo = parseFloat(costoString);
        totalCosto += cantidad * costo;

    });
    $('#txtImporteEstimado').val(totalCosto);
}

$('#tablaProductosRecibidos tbody').on('blur', '.editable', function () {
    calcularTotalProductosRecibidos();
});

function calcularTotalProductosRecibidos() {
    let totalCosto = 0;
    $('#tablaProductosRecibidos tbody').find('tr').each(function () {
        let cantCelda = $(this).find('.editable').text();
        let cantidad = parseInt(cantCelda != '' ? cantCelda : 0);

        if (isNaN(cantidad) || cantidad < 1) {
            cantidad = 0;
        }
        let str = $(this).find('td:eq(3)').text();
        let costoString = str.slice(1).trim();

        const costo = parseFloat(costoString);
        totalCosto += cantidad * costo;

    });
    $('#txtImporteRecibido').val(totalCosto);
    calcularIva();
}

const openModalPedido = (model = BASIC_MODEL_PEDIDO_WEB) => {
    $('#btnVolver').click();

    $("#txtIdPedido").val(model.idPedido);

    if (model.idPedido != 0) {
        cargarTabla(model.productos, model.idProveedor, model.estado == 1);

        $('#cboProveedor').val(model.idProveedor);
        $('#cboProveedor').prop('disabled', true);
        $("#cboEstado").val(model.estado);
        setTimeout(function () {
            $('#cboEstado').prop('disabled', true);
            $('#txtImporteEstimado').val(model.importeFinal ?? model.importeEstimado);
        }, 500);

        let deshabilitado = model.estado === 3;
        $('#txtComentario, #btnGenerar, #cboEstado, #btnSave').prop('disabled', deshabilitado);

    }
    else {
        $('#cboProveedor').prop('disabled', false);
    }

    if (model.fechaCerradoString == null || model.fechaCerradoString == '')
        document.getElementById("divFechaCerrado").style.display = 'none';
    else {
        document.getElementById("divFechaCerrado").style.display = '';

        $("#txtCerrado").val(model.fechaCerradoString);
        $("#txtCerradoUsuario").val(model.usuarioFechaCerrado);
    }

    switch (model.estado) {
        case 1:
            setTimeout(function () {
                $('#cboEstado').prop('disabled', false);
            }, 500);
            break;
        case 2:
            $('#optionRecibido').hide();
            $("#lblCantPedir").text('Cantidad Pedida');
            break;
        case 3:
            $('#optionRecibido').show();
            $("#lblCantPedir").text('Cantidad Recibida');
            $('label[for="txtImporteEstimado"]').text('Importe');
            break;
        default:
            $('#optionRecibido').hide();
            $("#lblCantPedir").text('Cantidad a Pedir');
            break;
    }

    $("#modalData").modal("show")
}

$("#btnNew").on("click", function () {
    openModalPedido()
    $("#cboEstado").val(1);
})


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedPedido = $(this).closest('tr').prev();
    } else {
        rowSelectedPedido = $(this).closest('tr');
    }

    const data = tableDataPedido.row(rowSelectedPedido).data();


    openModalPedido(data);
    calcularTotalCosto();
})

$("#btnGenerar").on("click", function () {
    let texto = 'Pedido:<br><br>';
    let alMenosUno = false;

    $('#tablaProductos').find('tbody tr').each(function () {
        let cant = $(this).find('td:eq(4)').text();
        let cantidad = parseInt(cant != null ? cant : 0);
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

    let celdasEditables = document.querySelectorAll(".editable");
    celdasEditables.forEach(function (celda) {
        celda.addEventListener("click", function () {
            this.focus();
        });

        celda.addEventListener("input", function () {
            let cantidad = parseFloat(this.textContent);
            if (isNaN(cantidad)) {
                cantidad = 0;
            }
            this.textContent = cantidad;
        });
    });
})

$('#cboProveedor').change(function () {

    $('#txtImporteEstimado').val(0);
    let idProv = $(this).val();
    let proveedor = proveedoresListPedido.find(_ => _.idProveedor == idProv);

    cargarTabla(proveedor.products, idProv, true);
})

function cargarTabla(productos, idProveedor, nuevo) {
    const tablaBody = document.querySelector('#tablaProductos tbody');
    tablaBody.innerHTML = '';
    let proveedor = proveedoresListPedido.find(_ => _.idProveedor == idProveedor);

    let disable = nuevo ? `class="editable" contenteditable="true"` : "";

    proveedor.products.forEach(producto => {
        const tr = document.createElement('tr');

        let prod = productos.find(_ => _.idProducto == producto.idProducto);

        let cantidadProducto = '';

        let idProd = producto.idProducto;
        let descr = producto.description;
        let costo = producto.costPrice;
        let stock = parseInt(producto.stocks != null && producto.stocks.length > 0 ? producto.stocks[0].stockActual : 0);
        if (prod != null) {
            if (prod.cantidadProductoRecibida != null) {
                cantidadProducto = prod.cantidadProductoRecibida;
            }
            else {
                if (prod.cantidadProducto == null)
                    prod.cantidadProducto = '';

                cantidadProducto = prod.cantidadProducto;
            }
        }

        tr.innerHTML = `
                              <td hidden>${idProd}</td>
                              <td>${descr}</td>
                              <td>$ ${costo}</td>
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
    const model = structuredClone(BASIC_MODEL_PEDIDO_WEB);
    model["idPedido"] = parseInt($("#txtIdPedido").val());
    model["importeEstimado"] = parseFloat($("#txtImporteEstimado").val());
    model["comentario"] = $("#txtComentario").val();
    model["idProveedor"] = $('#cboProveedor').val();
    model["cantidadProductos"] = $("#txtDescripcionTipoDeGasto").val();
    model["estado"] = parseInt($("#cboEstado").val());

    let productosConCantidad = [];

    $('#tablaProductos tbody tr').each(function () {
        let cantidadPedir = parseInt($(this).find('td:last-child').text());

        if (!isNaN(cantidadPedir) && cantidadPedir > 0) {
            let producto = {
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
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                tableDataPedido.row.add(responseJson.object).draw(false);
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
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                tableDataPedido.row(rowSelectedPedido).data(responseJson.object).draw(false);
                rowSelectedPedido = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "Pedido fue modificado", "success");

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
    const data = tableDataPedido.row(row).data();

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
                        return response.json();
                    }).then(responseJson => {
                        if (responseJson.state) {

                            tableDataPedido.row(row).remove().draw();
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


$("#tbData tbody").on("click", ".btn-recibir", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedPedido = $(this).closest('tr').prev();
    } else {
        rowSelectedPedido = $(this).closest('tr');
    }

    const data = tableDataPedido.row(rowSelectedPedido).data();


    openModalRecibido(data);
})

const openModalRecibido = (model = BASIC_MODEL_PEDIDO_WEB) => {

    $("#txtIdPedidoRecibido").val(model.idPedido);
    //$("#txtImporteRecibido").val(model.importeEstimado);
    //$("#txtImporteRecibido").attr("placeholder", model.importeEstimado);
    $("#txtComentarioRecibido").val(model.comentario);
    $('#txtProveedorRecibido').val(model.proveedor.nombre);
    $("#txtFechaCreacion").val(model.registrationDateString);
    $("#txtCreationUser").val(model.registrationUser);
    $('#txtIdProveedor').val(model.proveedor.idProveedor);

    $("#txtIva").val(model.proveedor.iva ?? '');
    $("#cboTipoFactura").val(model.proveedor.tipoFactura ?? '');

    let sumaCantidadProductos = 0;
    $.each(model.productos, function (index, producto) {
        sumaCantidadProductos += producto.cantidadProducto;
    });

    $("#txtCantidadProductos").val(sumaCantidadProductos);

    cargarTablaRecibidos(model.productos, model.idProveedor);
    $("#modalDataRecibido").modal("show")
}


function cargarTablaRecibidos(productos, idProveedor) {
    const tablaBody = document.querySelector('#tablaProductosRecibidos tbody');
    tablaBody.innerHTML = '';

    let proveedor = proveedoresListPedido.find(_ => _.idProveedor == idProveedor);

    productos.forEach(producto => {
        const tr = document.createElement('tr');

        let prod = proveedor.products.find(_ => _.idProducto == producto.idProducto);

        let idPedidoProducto = producto.idPedidoProducto;
        let idProd = producto.idProducto;
        let descr = prod.description;
        let costo = prod.costPrice;
        let cantidadPedida = producto.cantidadProducto;


        tr.innerHTML = `
                              <td hidden>${idPedidoProducto}</td>
                              <td hidden>${idProd}</td>
                              <td>${descr}</td>
                              <td>$ ${costo}</td>
                              <td>${cantidadPedida}</td>
                              <td class="editable" contenteditable="true" inputmode="numeric" pattern="[0-9]*" min="0" ></td>
                              <td class="editableText" contenteditable="true" inputmode="text"></td>
                              <td class="editableText" contenteditable="true" inputmode="text"></td>
                            `;
        tablaBody.appendChild(tr);
    });
}

$("#btnCerrarPedido").on("click", function () {
    calcularTotalProductosRecibidos();

    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    $("#modalDataRecibido").find("div.modal-content").LoadingOverlay("show")

    const model = structuredClone(BASIC_MODEL_RECIBIR);
    let idPedido = parseInt($("#txtIdPedidoRecibido").val());
    model["idPedido"] = idPedido;
    model["importeEstimado"] = parseFloat($("#txtImporteRecibido").val());
    model["comentario"] = $("#txtComentarioRecibido").val();
    model["idProveedor"] = parseInt($('#txtIdProveedor').val());

    model["tipoFactura"] = $("#cboTipoFactura").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val();
    model["ivaImporte"] = $("#txtImporteIva").val();
    model["importeSinIva"] = $("#txtImporteSinIva").val();
    model["estadoPago"] = parseInt($("#cboEstadoFactura").val());
    model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;


    let productosConCantidad = [];
    let fechaValida = true;
    let cantidadesVacias = false;
    $('#tablaProductosRecibidos tbody tr').each(function () {
        let idPedidoProducto = parseInt($(this).find('td:eq(0)').text());
        let idProducto = parseInt($(this).find('td:eq(1)').text());
        let cantidadPedida = parseInt($(this).find('td:eq(4)').text());
        let cantidadRecibida = parseInt($(this).find('td:eq(5)').text());
        let vencimiento = $(this).find('td:eq(6)').text();
        let lote = $(this).find('td:eq(7)').text();

        cantidadesVacias = $(this).find('td:eq(5)').text() == '';

        if (vencimiento != '') {
            fechaValida = validarFecha(vencimiento);
        } else {
            vencimiento = null;
        }

        if (!isNaN(cantidadRecibida)) {
            let producto = {
                idPedidoProducto: idPedidoProducto,
                idPedido: idPedido,
                idProducto: idProducto,
                cantidadProducto: cantidadPedida,
                vencimiento: vencimiento,
                lote: lote,
                cantidadProductoRecibida: cantidadRecibida

            };
            productosConCantidad.push(producto);
        }
    });
    model["productos"] = productosConCantidad;


    if (cantidadesVacias) {
        mostrarAdvertencia("Debe ingresar la cantidad recibida de todos los productos");
        return;
    }

    if (!fechaValida) {
        mostrarAdvertencia("Debe ingresar fechas de vencimiento validas (dd/mm/aaaa)");
        return;
    }

    if (model.idPedido != 0) {
        fetch("/Pedido/CerrarPedidos", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataRecibido").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                $("#modalDataRecibido").modal("hide");
                swal("Exitoso!", "Pedido fue recibido", "success");
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalDataRecibido").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

function mostrarAdvertencia(msg) {
    toastr.warning(msg, "");
    $("#modalDataRecibido").find("div.modal-content").LoadingOverlay("hide");
}

function validarFecha(fechaString) {
    let regex = /^(\d{2})\/(\d{2})\/(\d{4})$/;

    if (!regex.test(fechaString)) {
    }

    let partes = fechaString.split('/');
    let dia = parseInt(partes[0], 10);
    let mes = parseInt(partes[1], 10);
    let anio = parseInt(partes[2], 10);

    if (isNaN(dia) || isNaN(mes) || isNaN(anio)) {
        return false;
    }

    if (dia < 1 || dia > 31 || mes < 1 || mes > 12) {
        return false; // 
    }

    if (anio < 1000 || anio > 9999) {
        return false;
    }

    return true;
}

function calcularIva() {
    let importeText = $('#txtImporteRecibido').val();
    let importe = parseFloat(importeText == '' ? 0 : importeText);
    let iva = parseFloat($('#txtIva').val());

    if (!isNaN(importe) && !isNaN(iva)) {
        let importeSinIva = importe / (1 + (iva / 100));
        let importeIva = importe - importeSinIva;

        $('#txtImporteSinIva').val(importeSinIva.toFixed(2));
        $('#txtImporteIva').val(importeIva.toFixed(2));
    }
}

$('#txtIva').change(function () {
    calcularIva();
});

$('#txtImporteRecibido').keyup(function () {
    calcularIva();
});
