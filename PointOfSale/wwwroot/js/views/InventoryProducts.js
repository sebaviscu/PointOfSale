let tableData;
let rowSelected;
let edicionMasiva = false;
var aProductos = [];
var tienda;

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
    porcentajeProfit: "",
    costPrice: "",
    tipoVenta: "",
    idProveedor: ""
}

const BASIC_MASSIVE_EDIT = {
    precio: 0,
    idProductos: [],
    isActive: 1
}

$(document).ready(function () {

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

    fetch("/Tienda/GetOneTienda")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data != null) {
                tienda = responseJson.data;
                $("#txtAumento").val(tienda.aumentoWeb);
            }
        })

    tableData = $("#tbData").DataTable({
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
            {
                "data": "photoBase64", render: function (data) {
                    return `<img style="height:40px;" src="data:image/png;base64,${data}" class="rounded mx-auto d-block" />`;
                },
                "className": "text-center"
            },
            { "data": "nameProveedor" },
            { "data": "description" },
            { "data": "nameCategory" },
            { "data": "price" },
            {
                "data": "isActive", render: function (data) {
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
                "width": "100px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Report Products',
                exportOptions: {
                    columns: [2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });
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
            aProductos.push([row.id, row.childNodes[4].textContent, row.childNodes[5].textContent]);
        }
    })

    $(".hideAllEdit").hide();
    $("#listProd").show();

    document.getElementById("modalGridTitle").innerHTML = "Edicion Masiva";

    // Lista tipo de ventas
    var cont = document.getElementById('listProductosEditar');
    cont.innerHTML = "";
    var ul = document.createElement('ul');
    ul.setAttribute('style', 'padding: 0; margin: 0;');
    ul.setAttribute('id', 'theList');
    //ul.classList.add("list-group");

    for (i = 0; i <= aProductos.length - 1; i++) {
        var li = document.createElement('li');
        li.innerHTML = aProductos[i][1] + ": $" + aProductos[i][2];
        li.setAttribute('style', 'display: block;');
        //li.classList.add("list-group-item");
        ul.appendChild(li);
    }
    cont.appendChild(ul);

    edicionMasiva = true;

    $("#modalData").modal("show")
}

const openModal = (model = BASIC_MODEL) => {
    $(".hideAllEdit").show();
    $("#listProd").hide();

    document.getElementById("modalGridTitle").innerHTML = "Detalle de productos"

    $("#txtId").val(model.idProduct);
    $("#txtBarCode").val(model.barCode);
    $("#txtDescription").val(model.description);
    $("#cboCategory").val(model.idCategory == 0 ? $("#cboCategory option:first").val() : model.idCategory);
    $("#txtQuantity").val(model.quantity);
    $("#txtPrice").val(model.price);
    $("#txtPriceWeb").val(model.priceWeb);
    $("#txtProfit").val(model.porcentajeProfit);
    $("#txtCosto").val(model.costPrice);
    $("#cboTipoVenta").val(model.tipoVenta);
    $("#cboState").val(model.isActive);
    $("#txtPhoto").val("");
    $("#cboProveedor").val(model.idProveedor == 0 ? $("#cboProveedor option:first").val() : model.idProveedor);

    $("#imgProduct").attr("src", `data:image/png;base64,${model.photoBase64}`);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    edicionMasiva = false;

    $("#modalData").modal("show")

}

$("#btnNewProduct").on("click", function () {
    openModal()
})

$("#btnSave").on("click", function () {

    if (edicionMasiva) {
        saveMassiveProducts();

    } else {
        saveOneProduct();

    }

})

function saveMassiveProducts() {

    const model = structuredClone(BASIC_MASSIVE_EDIT);
    model["idProductos"] = aProductos.map(d => d[0]);
    model["precio"] = $("#txtPrice").val();
    model["priceWeb"] = $("#txtPriceWeb").val();
    model["profit"] = $("#txtProfit").val();
    model["costo"] = $("#txtCosto").val();
    model["isActive"] = $("#cboState").val() == '1' ? true : false;

    fetch("/Inventory/EditMassiveProducts", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.state) {
            $("#modalData").modal("hide");
            //swal("Exitoso!", "Los productos fueron modificados", "success");
            location.reload();

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
}

function saveOneProduct() {
    const inputs = $(".input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "" || item.value.trim() == null)

    if (inputs_without_value.length > 0) {
        const msg = `Debe completaro el campo : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if (document.getElementById("cboProveedor").value == '') {
        const msg = `Debe completaro el campo Proveedor"`;
        toastr.warning(msg, "");
        return;
    }

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
    model["price"] = $("#txtPrice").val();
    model["priceWeb"] = $("#txtPriceWeb").val();
    model["porcentajeProfit"] = $("#txtProfit").val();
    model["costPrice"] = $("#txtCosto").val();
    model["tipoVenta"] = $("#cboTipoVenta").val();
    model["isActive"] = $("#cboState").val();
    model["idProveedor"] = $("#cboProveedor").val();

    const inputPhoto = document.getElementById('txtPhoto');

    const formData = new FormData();
    formData.append('photo', inputPhoto.files[0]);
    formData.append('model', JSON.stringify(model));

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
}


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();

    openModal(data);
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
    var costo = $("#txtCosto").val();
    var profit = $("#txtProfit").val();

    if (costo !== '' && profit !== '') {

        var precio = parseFloat(costo) * (1 + (parseFloat(profit) / 100));
        $("#txtPrice").val(precio);
    }
}


function calcularPrecioWeb() {
    var aumento = $("#txtAumento").val();
    var precio = $("#txtPrice").val();

    if (aumento !== '' && precio !== '') {

        var precio = parseFloat(precio) * (1 + (parseFloat(aumento) / 100));
        $("#txtPriceWeb").val(precio);
    }
}