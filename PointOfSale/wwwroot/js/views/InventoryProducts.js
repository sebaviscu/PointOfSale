﻿let tableData;
let rowSelected;
let edicionMasiva = false;
var aProductos = [];
var tienda;
var aumentoWeb = 0;

const BASIC_MODEL = {
    idProduct: 0,
    barCode: "",
    description: "",
    idCategory: 0,
    quantity: 0,
    price: 0,
    isActive: 1,
    photo: "",
    modificationDate: null,
    modificationUser: null,
    priceWeb: "",
    porcentajeProfit: 0,
    costPrice: "",
    tipoVenta: "",
    idProveedor: "",
    comentario: "",
    minimo: 0,
    precio2: "",
    precio3: "",
    porcentajeProfit2: 0,
    porcentajeProfit3: 0,
    vencimientos: [],
    iva: 0
}

const BASIC_MASSIVE_EDIT = {
    precio: 0,
    idProductos: [],
    isActive: null
}

const BASIC_IMPRIMIR_PRECIOS = {
    idProductos: [],
    listaPrecio: 0,
    fechaModificacion: false,
    codigoBarras: false,
    path: ""
}

const BASIC_vencimientos = {
    idVencimiento: 0,
    lote: "",
    fechaVencimiento: null,
    fechaElaboracion: null,
    notificar: 0,
    registrationDate: false,
    registrationUser: false
}

$(document).ready(function () {
    showLoading();

    tableData = $("#tbData").DataTable({
        pageLength: 50,
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetProducts",
            "type": "GET",
            "datatype": "json"
        },
        rowId: 'idProduct',
        "columns": [
            {
                "data": "idProduct",
                "visible": false,
                "searchable": false
            },
            {
                "defaultContent": `<input type="checkbox" class="chkProducto">`,
                "orderable": false,
                "searchable": false,
                "width": "40px",
                "className": "text-center"
            },
            //{
            //    "data": "photoBase64", render: function (data) {
            //        return `<img style="height:40px;" src="data:image/png;base64,${data}" class="rounded mx-auto d-block" />`;
            //    },
            //    "className": "text-center"
            //},
            { "data": "description" },
            { "data": "nameCategory" },
            { "data": "nameProveedor" },
            { "data": "priceString" },
            {
                "data": "precio2", render: function (data) {
                    return `$ ${data}`;
                }
            },
            {
                "data": "priceWeb", render: function (data) {
                    return `$ ${data}`;
                }
            },
            { "data": "modificationDateString" },
            {
                "data": "isActive",
                "className": "text-center", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "130px",
                "className": "text-center"
            }
        ],
        order: [[2, "asc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Report Productos',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6, 7, 8]
                }
            }, 'pageLength'
        ]
    });


    fetch("/Inventory/GetCategories")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboCategory").append(
                        $("<option>").val(item.idCategory).text(item.description)
                    )
                });
            }
        })

    fetch("/Admin/GetProveedores")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data.length > 0) {
                $("#cboProveedor").append(
                    $("<option>").val('').text('')
                )
                responseJson.data.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });
            }
        })

    fetch("/Admin/GetAjustesProductos")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data != null) {
                $("#txtAumento").val(responseJson.data.aumentoWeb + ' %');
                $("#txtAumentoMasivo").val(responseJson.data.aumentoWeb + ' %');
                aumentoWeb = responseJson.data.aumentoWeb;
            }
        })

    $('.filtro-vencimientos').change(function () {
        filtrarTabla();
    });

    $("#txtfVencimiento").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#txtfElaborado").datepicker({ dateFormat: 'dd/mm/yy' });
    cargarTablaVencimientos();
    removeLoading();
})

$("#chkSelectAll").on("click", function () {
    var estado = document.querySelector('#chkSelectAll').checked;

    var checkboxes = document.querySelectorAll('input[type="checkbox"].chkProducto');

    for (var i = 0; i < checkboxes.length; i++) {
        checkboxes[i].checked = estado;
    }
})

function editAll() {
    if (document.querySelectorAll('input[type=checkbox]:checked').length == 0)
        return;

    aProductos = [];

    document.querySelectorAll('#tbData tr').forEach((row, i) => {
        if (row.querySelector('input[type=checkbox]') != null && row.querySelector('input[type=checkbox]').checked && row.id !== '') {
            aProductos.push([row.id, row.childNodes[1].textContent, row.childNodes[4].textContent]);
        }
    })

    $("#txtPriceMasivo").val('');
    $("#txtPriceWebMasivo").val('');
    $("#txtProfitMasivo").val('');
    $("#txtCostoMasivo").val('');
    $("#txtPrice2Masivo").val(parseInt(''));
    $("#txtProfit2Masivo").val('');
    $("#txtPrice3Masivo").val('');
    $("#txtProfit3Masivo").val('');

    // Lista tipo de ventas
    var cont = document.getElementById('listProductosEditar');
    cont.innerHTML = "";
    var ul = document.createElement('ul');
    ul.setAttribute('style', 'padding: 0; margin: 0;');
    ul.setAttribute('id', 'theList');
    //ul.classList.add("list-group");

    for (i = 0; i <= aProductos.length - 1; i++) {
        var li = document.createElement('li');
        li.innerHTML = aProductos[i][1] + ": " + aProductos[i][2];
        li.setAttribute('style', 'display: block;');
        //li.classList.add("list-group-item");
        ul.appendChild(li);
    }
    cont.appendChild(ul);

    let productos = tableData.ajax.json().data;

    let idsProductos = aProductos.map(item => parseInt(item[0]));

    let productosFiltrados = productos.filter(producto => idsProductos.includes(producto.idProduct));

    cargarTabla(productosFiltrados);

    $("#modalDataMasivo").modal("show")
}

const openModal = (model = BASIC_MODEL) => {

    $("#txtId").val(model.idProduct);
    $("#txtBarCode").val(model.barCode);
    $("#txtDescription").val(model.description);
    $("#cboCategory").val(model.idCategory == 0 ? $("#cboCategory option:first").val() : model.idCategory);
    $("#txtQuantity").val(model.quantity);
    $("#txtMinimo").val(model.minimo);
    $("#txtPrice").val(model.price != 0 ? model.price.replace(/,/g, '.') : '');
    $("#txtPriceWeb").val(model.priceWeb);
    $("#txtProfit").val(model.porcentajeProfit);
    $("#txtCosto").val(model.costPrice);
    $("#cboTipoVenta").val(model.tipoVenta);
    $("#cboState").val(model.isActive);
    $("#txtPhoto").val("");
    $("#cboProveedor").val(model.idProveedor == 0 ? $("#cboProveedor option:first").val() : model.idProveedor);
    $("#txtComentario").val(model.comentario);
    $("#txtIva").val(model.iva);
    $("#txtPrice2").val(model.precio2.replace(/,/g, '.'));
    $("#txtProfit2").val(model.porcentajeProfit2);
    $("#txtPrice3").val(model.precio3.replace(/,/g, '.'));
    $("#txtProfit3").val(model.porcentajeProfit3);

    if (model.photoBase64 != null) {
        $("#imgProduct").attr("src", `data:image/png;base64,${model.photoBase64}`);
    }

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }


    $("#tbVencimientos tbody").html("");
    if (model.vencimientos && model.vencimientos.length > 0) {

        model.vencimientos.sort((a, b) => b.idVencimiento - a.idVencimiento);

        const top3Vencimientos = model.vencimientos.slice(0, 3);

        top3Vencimientos.forEach((v) => {
            addVencimientoTable(v);
        });

    }

    $("#txtfVencimiento").val('');
    $("#txtfElaborado").val('');
    $("#txtLote").val('');

    $("#modalData").modal("show")
}

function addVencimientoTable(data) {
    var fechaElaboracion = "";
    var fechaVencimiento = "";
    var fechaCompleta = "";

    if (data.fechaVencimiento != null) {

        fechaCompleta = data.fechaVencimiento;
        fechaVencimiento = fechaCompleta.split('T')[0].replace(/-/g, '/');
    }

    if (data.fechaElaboracion != null) {

        fechaCompleta = data.fechaElaboracion;
        fechaElaboracion = fechaCompleta.split('T')[0].replace(/-/g, '/');
    }

    $("#tbVencimientos tbody").append(
        $("<tr>").append(
            $("<td>").text(fechaVencimiento),
            $("<td>").text(fechaElaboracion),
            $("<td>").text(data.lote),
            $("<td>")
                .append(
                    $("<input>").attr("type", "checkbox").prop("checked", data.notificar)
                        .data("estado-inicial", data.notificar)
                ),

            $("<td>").append(
                $("<button>").addClass("btn btn-danger btn-sm btn-delete-vencimiento").append(
                    $("<i>").addClass("mdi mdi-trash-can")
                ).data("vencimiento", data))
        )
    )
}


$("#tbVencimientos tbody").on("click", ".btn-delete-vencimiento", function (event) {
    event.preventDefault();

    var v = $(this).data("vencimiento")
    var btn = $(this);

    swal({
        title: "¿Desea eliminar el vencimiento? ",
        text: "",
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar vencimiento",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {


            if (respuesta) {

                if (v.idVencimiento == 0) {
                    btn.closest("tr").remove();
                }
                else {
                    $(".showSweetAlert").LoadingOverlay("show")

                    fetch(`/Inventory/DeleteVencimiento?idVencimiento=${v.idVencimiento}`, {
                        method: "DELETE"
                    }).then(response => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                        return response.ok ? response.json() : Promise.reject(response);
                    }).then(responseJson => {
                        if (responseJson.state) {

                            btn.closest("tr").remove();
                            swal("Eliminado", "El vencimiento ha sido eliminado", "success");

                        } else {
                            swal("Lo sentimos", responseJson.message, "error");
                        }
                    })
                        .catch((error) => {
                            $(".showSweetAlert").LoadingOverlay("hide")
                        })
                }

            }
        });

})

function obtenerDatosTabla() {
    var datos = [];
    $("#tbVencimientos tbody tr").each(function () {
        var fila = {};
        fila.fechaVencimiento = $(this).find("td:eq(0)").text();
        fila.fechaElaboracion = $(this).find("td:eq(1)").text();
        fila.lote = $(this).find("td:eq(2)").text();
        fila.notificar = $(this).find("td:eq(3) input[type=checkbox]").prop("checked");
        fila.idVencimiento = $(this).find("button").data("vencimiento").idVencimiento;
        //fila.cambio = false; // Inicialmente establecido como false

        // Verificar si el valor del checkbox ha cambiado
        var estadoInicial = $(this).find("td:eq(3) input[type=checkbox]").data("estado-inicial");
        if ((estadoInicial !== undefined && fila.notificar !== estadoInicial) || fila.idVencimiento === 0) {
            //fila.cambio = true; // Si ha cambiado, establecer cambio a true
            datos.push(fila);
        }

        // Verificar si data.idVencimiento es igual a 0
        //if (fila.idVencimiento === 0 || fila.cambio) {
        //    datos.push(fila);
        //}
    });
    return datos;
}

$("#btnAddVencimiento").on("click", function () {
    const model = structuredClone(BASIC_vencimientos);
    model["fechaVencimiento"] = $("#txtfVencimiento").val();
    model["fechaElaboracion"] = $("#txtfElaborado").val();
    model["lote"] = $("#txtLote").val();
    model["notificar"] = true;

    addVencimientoTable(model);

    $("#txtfVencimiento").val('');
    $("#txtfElaborado").val('');
    $("#txtLote").val('');

    return false;
})


$("#tbVencimientos tbody").on("click", ".btn-danger", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }
    var v = $(this).data("vencimiento")

    if (v.idVencimiento == 0) {
        rowSelected.remove();
    }
})

$("#bntModalImportar").on("click", function () {
    $("#modalImpprtar").modal("show")
})

$("#btnImportar").on("click", function () {
    swal({
        title: "¿Está seguro?",
        text: `Se importaran ${cantProductosImportar} Productos`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, IMPORTAR",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {
                let ruta = $("#txtRutaImportar").val();

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Inventory/ImportarProductos?path=${ruta}`, {
                    method: "GET"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {
                        swal("Exitoso!", "Se han importado " + cantProductosImportar + " filas", "success");

                        location.reload();
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

var cantProductosImportar = 0;

$("#btnCargarImportar").on("click", function () {
    $("#btnCargarImportar").LoadingOverlay("show")

    let ruta = $("#txtRutaImportar").val();

    fetch(`/Inventory/CargarProductos?path=${ruta}`, {
        method: "GET"
    }).then(response => {
        $("#btnCargarImportar").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.state) {
            cantProductosImportar = responseJson.object.length;
            //swal("Exitoso!", "Se cargaron " + cantProductosImportar + " filas", "success");
            $("#lblCantidadProds").html("Cantidad de Productos: <strong> " + cantProductosImportar + ".</strong>");

            let i = 0;
            responseJson.object.forEach((product) => {
                $("#tableImportarProductos tbody").append(
                    $("<tr>").append(
                        $("<td>").text(++i),
                        $("<td>").text(product.description),
                        $("<td>").text(product.barCode),
                        $("<td>").text(product.tipoVenta == '1' ? 'Kg' : 'U'),
                        $("<td>").text(product.costPrice),
                        $("<td>").text(product.porcentajeProfit),
                        $("<td>").text(product.price),
                        $("<td>").text(product.porcentajeProfit2),
                        $("<td>").text(product.precio2),
                        $("<td>").text(product.porcentajeProfit3),
                        $("<td>").text(product.precio3),
                        $("<td>").text(product.priceWeb),
                        $("<td>").text(product.nameProveedor),
                        $("<td>").text(product.nameCategory)
                    )
                )
            });

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
        .catch((error) => {
            $("#btnCargarImportar").LoadingOverlay("hide")
        })

})

$("#btnNewProduct").on("click", function () {
    openModal()
})

$("#btnSaveMasivo").on("click", function () {

    if ($("#txtPriceMasivo").val() == '' && $("#txtPrice2Masivo").val() == '' && $("#txtPrice3Masivo").val() == '' && $("#txtPorPorcentajeMasivo").val() == '' && $("#txtIvaMasivo").val() == '' && $("#cboStateMasivo").val() == '') {
        const msg = `Debe completar alguno de los campos de Precio o Por %`;
        toastr.warning(msg, "");
        return;
    }

    if (($("#txtPriceMasivo").val() != '' || $("#txtPrice2Masivo").val() != '' || $("#txtPrice3Masivo").val() != '') && $("#txtPorPorcentajeMasivo").val() != '' && $("#txtIvaMasivo").val() == '' && $("#cboStateMasivo").val() == '') {
        const msg = `Puede completar solo uno de los campos Precio o Por %`;
        toastr.warning(msg, "");
        return;
    }
    showLoading();

    const model = structuredClone(BASIC_MASSIVE_EDIT);
    model["idProductos"] = aProductos.map(d => d[0]);
    model["precio"] = $("#txtPriceMasivo").val() != '' ? $("#txtPriceMasivo").val().replace('.', ',') : '0';
    model["priceWeb"] = $("#txtPriceWebMasivo").val() != '' ? $("#txtPriceWebMasivo").val().replace('.', ',') : '0';
    model["profit"] = $("#txtProfitMasivo").val() != '' ? $("#txtProfitMasivo").val().replace('.', ',') : '0';
    model["costo"] = $("#txtCostoMasivo").val();
    model["comentario"] = $("#txtComentarioMasivo").val();
    if ($("#cboStateMasivo").val() != '-1')
    {
        model["isActive"] = $("#cboStateMasivo").val() == '1' ? true : false;
    }
    model["porPorcentaje"] = $("#txtPorPorcentajeMasivo").val();
    model["iva"] = $("#txtIvaMasivo").val();
    model["redondeo"] = $("#rounding").val();

    model["precio2"] = $("#txtPrice2Masivo").val() != '' ? $("#txtPrice2Masivo").val().replace('.', ',') : '0';
    model["porcentajeProfit2"] = $("#txtProfit2Masivo").val() != '' ? $("#txtProfit2Masivo").val().replace('.', ',') : 0;
    model["precio3"] = $("#txtPrice3Masivo").val() != '' ? $("#txtPrice3Masivo").val().replace('.', ',') : '0';
    model["porcentajeProfit3"] = $("#txtProfit3Masivo").val() != '' ? $("#txtProfit3Masivo").val().replace('.', ',') : 0;
    $("#modalDataMasivo").find("div.modal-content").LoadingOverlay("show")

    fetch("/Inventory/EditMassiveProducts", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalDataMasivo").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        removeLoading();

        if (responseJson.state) {
            location.reload();
            $("#modalDataMasivo").modal("hide");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
})

$("#btnSave").on("click", function () {
    const inputs = $(".input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "" || item.value.trim() == null)

    if (inputs_without_value.length > 0) {
        const msg = `Debe completaro el campo : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    //if (document.getElementById("cboProveedor").value == '') {
    //    const msg = `Debe completaro el campo Proveedor"`;
    //    toastr.warning(msg, "");
    //    return;
    //}

    if (document.getElementById("cboTipoVenta").value == '') {
        const msg = `Debe completaro el campo Tipo Venta"`;
        toastr.warning(msg, "");
        return;
    }

    const model = structuredClone(BASIC_MODEL);
    model["idProduct"] = parseInt($("#txtId").val());
    model["barCode"] = $("#txtBarCode").val();
    model["description"] = $("#txtDescription").val();
    model["idCategory"] = $("#cboCategory").val();
    model["quantity"] = $("#txtQuantity").val();
    model["minimo"] = $("#txtMinimo").val();
    model["price"] = $("#txtPrice").val().replace(/\./g, ',');
    model["priceWeb"] = $("#txtPriceWeb").val();
    model["porcentajeProfit"] = $("#txtProfit").val();
    model["costPrice"] = $("#txtCosto").val().replace(/\./g, ',');
    model["tipoVenta"] = $("#cboTipoVenta").val();
    model["isActive"] = $("#cboState").val();
    model["idProveedor"] = $("#cboProveedor").val();
    model["comentario"] = $("#txtComentario").val();
    model["iva"] = $("#txtIva").val();

    model["priceWeb"] = $("#txtPriceWeb").val() != '' && $("#txtPriceWeb").val() != undefined ? $("#txtPriceWeb").val().replace(/\./g, ',') : $("#txtPrice").val().replace(/\./g, ',')

    model["precio2"] = $("#txtPrice2").val() != '' ? $("#txtPrice2").val().replace(/\./g, ',') : $("#txtPrice").val().replace(/\./g, ',');
    model["porcentajeProfit2"] = $("#txtProfit2").val() != '' ? $("#txtProfit2").val().replace(/\./g, ',') : $("#txtProfit").val().replace(/\./g, ',');

    model["precio3"] = $("#txtPrice3").val() != '' ? $("#txtPrice3").val().replace(/\./g, ',') : $("#txtPrice").val().replace(/\./g, ',');
    model["porcentajeProfit3"] = $("#txtProfit3").val() != '' ? $("#txtProfit3").val().replace(/\./g, ',') : $("#txtProfit").val().replace(/\./g, ',');

    let vencimientos = obtenerDatosTabla();

    const inputPhoto = document.getElementById('txtPhoto');

    const formData = new FormData();
    formData.append('photo', inputPhoto.files[0]);
    formData.append('model', JSON.stringify(model));
    formData.append('vencimientos', JSON.stringify(vencimientos));

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idProduct == 0) {
        fetch("/Inventory/CreateProduct", {
            method: "POST",
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                tableData.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "El producto fué creado", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Inventory/EditProduct", {
            method: "PUT",
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelected).data(responseJson.object).draw(false);
                rowSelected = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "El producto fué modificado", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }
})


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();

    fetch(`/Inventory/GetProduct?IdProduct=${data.idProduct}`,)
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data != null) {
                openModal(responseJson.data);
            }
        })
})



$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableData.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar el producto "${data.description}"`,
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

                fetch(`/Inventory/DeleteProduct?IdProduct=${data.idProduct}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "El producto fué eliminado", "success");

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

function calcularPrecio() {

    let ids = ["", "2", "3"];

    let iva = $("#txtIva").val();
    let costo = $("#txtCosto").val();

    iva = iva === '' ? 0 : parseFloat(iva);

    if (costo !== '') {
        costo = parseFloat(costo) * (1 + (iva / 100));

        ids.forEach(id => {
            let profit = $("#txtProfit" + id).val();

            if (profit !== '') {
                let precio = costo * (1 + (parseFloat(profit) / 100));
                $("#txtPrice" + id).val(precio.toFixed(2));
            }
        });
    }

    var aumento = $("#txtAumento").val();
    var precio = $("#txtPrice").val();

    if (aumento !== '' && precio !== '') {
        var precioFinal = parseFloat(precio) * (1 + (parseFloat(aumento) / 100));
        $("#txtPriceWeb").val(precioFinal);
    }
}

function calcularPrecioMasivo() {

    let ids = ["Masivo", "2Masivo", "3Masivo"];

    let iva = $("#txtIvaMasivo").val();
    let costo = $("#txtCostoMasivo").val();

    iva = iva === '' ? 0 : parseFloat(iva);

    let roundingDigits = parseInt($("#rounding").val());

    if (isNaN(roundingDigits) || roundingDigits < 0) {
        alert('Por favor, ingrese un valor de redondeo válido.');
        return;
    }

    if (costo !== '') {
        costo = parseFloat(costo) * (1 + (iva / 100));

        ids.forEach(id => {
            let profit = $("#txtProfit" + id).val();

            if (profit !== '') {
                let precio = costo * (1 + (parseFloat(profit) / 100));
                let roundedPrice = roundToDigits(precio, roundingDigits);

                $("#txtPrice" + id).val(roundedPrice.replace(',', '.'));
            }
        });
    }

    var aumento = $("#txtAumento").val();
    var precio = $("#txtPriceMasivo").val();

    if (aumento !== '' && precio !== '') {
        var precioFinal = parseFloat(precio) * (1 + (parseFloat(aumento) / 100));
        let roundedPrice = roundToDigits(precioFinal, roundingDigits);

        $("#txtPriceWebMasivo").val(roundedPrice.replace(',', '.'));
    }
}


function imprimirPrecios() {
    if (document.querySelectorAll('input[type=checkbox]:checked').length == 0)
        return;

    aProductos = [];

    document.querySelectorAll('#tbData tr').forEach((row, i) => {
        if (row.querySelector('input[type=checkbox]') != null && row.querySelector('input[type=checkbox]').checked && row.id !== '') {
            aProductos.push([row.id, row.childNodes[1].textContent, row.childNodes[4].textContent]);
        }
    })

    // Lista tipo de ventas
    var cont = document.getElementById('listProdImprimir');
    cont.innerHTML = "";
    var ul = document.createElement('ul');
    ul.setAttribute('style', 'padding: 0; margin: 0;text-align: left;');
    ul.setAttribute('id', 'theList');
    //ul.classList.add("list-group");

    for (i = 0; i <= aProductos.length - 1; i++) {
        var li = document.createElement('li');
        li.innerHTML = "· " + aProductos[i][1];
        li.setAttribute('style', 'display: block;');
        //li.classList.add("list-group-item");
        ul.appendChild(li);
    }
    cont.appendChild(ul);

    $("#modalDataImprimirPrecios").modal("show");
}

$("#btnImprimir").on("click", function () {
    const model = structuredClone(BASIC_IMPRIMIR_PRECIOS);
    model["idProductos"] = aProductos.map(d => parseInt(d[0]));
    model["listaPrecio"] = parseInt($("#cboListaPrecio").val());
    model["fechaModificacion"] = document.getElementById("switchUltimaModificacion").checked
    model["codigoBarras"] = document.getElementById("switchCodigoBarras").checked

    showLoading();
    $("#modalDataImprimirPrecios").modal("hide");
    fetch("/Inventory/ImprimirListaPrecios", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {

            var byteCharacters = atob(responseJson.data);
            var byteNumbers = new Array(byteCharacters.length);
            for (var i = 0; i < byteCharacters.length; i++) {
                byteNumbers[i] = byteCharacters.charCodeAt(i);
            }
            var byteArray = new Uint8Array(byteNumbers);
            var file = new Blob([byteArray], { type: 'application/pdf;base64' });
            var fileURL = URL.createObjectURL(file);
            window.open(fileURL);

            removeLoading()

        } else {
            swal("Lo sentimos", responseJson.error, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })

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

function cargarTabla(productosFiltrados) {
    const tablaBody = $('#tablaProductosEditar tbody');
    tablaBody.empty();

    productosFiltrados.forEach(producto => {
        let idProd = producto.idProduct;
        let descr = producto.description;
        let costo = producto.costPrice == null ? 0 : producto.costPrice;
        let precio1 = producto.price;
        let precio2 = producto.precio2;
        let precio3 = producto.precio3;
        let priceWeb = producto.priceWeb;

        let $tr = $('<tr>');
        $tr.append($('<td hidden>').text(idProd));
        $tr.append($('<td>').text(descr));
        $tr.append($('<td>').text(`$ ${costo}`).addClass('editable costo').attr({
            'contenteditable': true,
            'inputmode': 'numeric',
            'pattern': '[0-9]*',
            'min': 0
        }));
        $tr.append($('<td>').text(`$ ${precio1}`).addClass('editable').attr({
            'data-profit': producto.porcentajeProfit,
            'data-web': 0,
            'data-iva': producto.iva,
            'contenteditable': true,
            'inputmode': 'numeric',
            'pattern': '[0-9]*',
            'min': 0
        }));
        $tr.append($('<td>').text(`$ ${precio2}`).addClass('editable').attr({
            'data-profit': producto.porcentajeProfit2,
            'data-web': 0,
            'data-iva': producto.iva,
            'contenteditable': true,
            'inputmode': 'numeric',
            'pattern': '[0-9]*',
            'min': 0
        }));
        $tr.append($('<td>').text(`$ ${precio3}`).addClass('editable').attr({
            'data-profit': producto.porcentajeProfit3,
            'data-web': 0,
            'data-iva': producto.iva,
            'contenteditable': true,
            'inputmode': 'numeric',
            'pattern': '[0-9]*',
            'min': 0
        }));
        $tr.append($('<td>').text(`$ ${priceWeb}`).addClass('editable').attr({
            'data-profit': producto.porcentajeProfit,
            'data-web': aumentoWeb,
            'data-iva': producto.iva,
            'contenteditable': true,
            'inputmode': 'numeric',
            'pattern': '[0-9]*',
            'min': 0
        }));

        tablaBody.append($tr);
    });

    $('#rounding').change(function () {
        let tab = $('#myTab .nav-link.active').attr('id');
        if (tab == 'edit-tabla-tab') {
            recalculatePrices();
        }
        else {
            calcularPrecioMasivo();
        }
    });

    $('.editable.costo').on('blur', function () {
        recalculatePrices();
    });

    var celdasEditables = document.querySelectorAll(".editable");
    celdasEditables.forEach(function (celda) {
        var celdasEditables = document.querySelectorAll(".editable");
        celdasEditables.forEach(function (celda) {
            var contenidoOriginal = celda.textContent.trim();
            celda.addEventListener("click", function () {
                if (this.textContent.trim() === contenidoOriginal) {
                    this.textContent = '';
                }
                this.focus();
            });

            celda.addEventListener("input", function () {
                var contenido = this.textContent.trim();
                if (/^\$?\s?\d*\.?\d*$/.test(contenido)) {
                    var cantidad = parseFloat(contenido.replace(/\$|\s/g, '').replace('.', ','));
                    if (!isNaN(cantidad)) {
                        this.textContent = cantidad;
                    }
                } else {
                    this.textContent = '';
                }
            });

            celda.addEventListener("blur", function () {
                var contenido = this.textContent.trim();
                if (contenido === '') {
                    this.textContent = contenidoOriginal;
                } else if (/^\d*\.?\d*$/.test(contenido)) {
                    var cantidad = parseFloat(contenido.replace('.', ','));
                    if (!isNaN(cantidad)) {
                        this.textContent = `$ ${cantidad.toFixed(2).replace('.', ',')}`;
                    }
                }
            });
        });

    });
}

function roundToDigits(number, digits) {
    if (digits === 0) {
        return number.toFixed(2).replace('.', ',');
    }

    let factor = Math.pow(10, digits);
    return (Math.round(number / factor) * factor).toFixed(2);
}

function recalculatePrices() {
    let roundingDigits = parseInt($("#rounding").val());

    if (isNaN(roundingDigits) || roundingDigits < 0) {
        alert('Por favor, ingrese un valor de redondeo válido.');
        return;
    }

    $('#tablaProductosEditar tbody tr').each(function () {
        let $fila = $(this);
        let costoText = $fila.find('.editable.costo').text().replace('$ ', '').replace(',', '.');

        $fila.find('td:nth-child(n+4)').each(function () {
            let costo = parseFloat(costoText) || 0;
            let profit = parseFloat($(this).attr('data-profit'));
            let iva = parseFloat($(this).attr('data-iva'));
            let web = parseFloat($(this).attr('data-web'));
            iva = iva == '' || isNaN(iva) ? 0 : iva;
            web = web == '' ? 0 : web;

            if (profit != 0 && costo != 0) {
                costo = costo * (1 + (iva / 100));
                var nuevoPrecio = costo * (1 + profit / 100);
                nuevoPrecio = nuevoPrecio * (1 + (web / 100));

                let roundedPrice = roundToDigits(nuevoPrecio, roundingDigits);

                $(this).text(`$ ${roundedPrice}`);
            }
        });
    });
}

$('#btnSaveMasivoTabla').click(function () {
    var model = [];
    showLoading();

    $('#tablaProductosEditar tbody tr').each(function () {
        var rowData = {};
        $(this).find('td').each(function (index) {
            var fieldName = $('#tablaProductosEditar th').eq(index).attr('title');

            if (fieldName != null) {
                var fieldValue = $(this).text().trim().replace('$ ', '').replace(',', '.');
                rowData[fieldName.trim()] = fieldValue;
            }
        });
        model.push(rowData);
    });
    $("#modalDataMasivo").find("div.modal-content").LoadingOverlay("show")

    fetch("/Inventory/EditMassiveProductsForTable", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalDataMasivo").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        removeLoading();

        if (responseJson.state) {
            location.reload();
            $("#modalDataMasivo").modal("hide");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataMasivo").find("div.modal-content").LoadingOverlay("hide")
    })

});