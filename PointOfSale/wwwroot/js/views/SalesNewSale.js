
var originalTab = document.getElementById('nuevaVenta');
let AllTabsForSale = [];
var buttonCerrarTab = '<button class="close" type="button" title="Cerrar tab">×</button>';
var tabID = 0;
var promociones = [];

const ProducstTab = {
    idTab: 0,
    products: []
}

$(document).ready(function () {

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboTypeDocumentSale").append(
                $("<option>").val('').text('')
            )
            //borrar los options de cboTipoDocumentoVenta
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTypeDocumentSale").append(
                        $("<option>").val(item.idTypeDocumentSale).text(item.description)
                    )
                });
            }
            newTab();
        })

    fetch("/Admin/GetPromocionesActivas")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            promociones = responseJson.data;
        })
})

function formatResults(data) {

    if (data.loading)
        return data.text;

    var container = $(
        `<table width="90%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <p>$ ${data.price}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}

function formatResultsClients(data) {

    if (data.loading)
        return data.text;

    var container = $(
        `<table width="100%">
            <tr>
                <td class="col-sm-8">
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <em style="font-weight: bolder;margin:2px">${data.cuil}</em>
                </td>
                <td class="col-sm-4">
                    <p style="font-weight: bolder;" class="${data.color}">${data.total}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}

$(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
});


function showProducts_Prices(idTab, currentTab) {
    let total = 0;
    let row = 0;
    $('#tbProduct' + idTab + ' tbody').html("")

    currentTab.products.forEach((item) => {
        item.row = row;
        total = total + parseFloat(item.total);

        $('#tbProduct' + idTab + ' tbody').append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
                        $("<i>").addClass("mdi mdi-trash-can")
                    ).data("idproduct", item.idproduct).data("idTab", idTab).data("row", row)
                ),
                $("<td>").text(item.descriptionproduct),
                $("<td>").text(item.quantity),
                $("<td>").text("$ " + item.price),
                $("<td>").text("$ " + item.total),
                $("<td>").append(item.promocion != null ?
                    $("<i>").addClass("mdi mdi-percent").attr("data-toggle", "tooltip").attr("title", item.promocion) : "")
            )
        )
        row++;
    })

    $('#txtTotal' + idTab).val(total.toFixed(2))
}

$(document).on("click", "button.btn-delete", function () {
    const currentTabId = $(this).data("idTab");
    const row = $(this).data("row");

    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    currentTab.products.splice(row, 1);

    showProducts_Prices(currentTabId, currentTab);
})

$(document).on("click", "button.finalizeSale", function () {
    var currentTabId = $(this).attr("tabId");
    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    if (currentTab.products.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    if (document.getElementById("cboTypeDocumentSale" + currentTabId).value == '') {
        const msg = `Debe completaro el campo Tipo de Venta`;
        toastr.warning(msg, "");
        return;
    }

    const vmDetailSale = currentTab.products;

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSale" + currentTabId).val(),
        clientId: $("#cboCliente" + currentTabId).val() != '' ? $("#cboCliente" + currentTabId).val() : null,
        total: $("#txtTotal" + currentTabId).val(),
        detailSales: vmDetailSale,
        tipoMovimiento: $("#cboCliente" + currentTabId).val() != '' ? 2 : null
    }

    $("#btnFinalizeSale" + currentTabId).closest("div.card-body").LoadingOverlay("show")

    fetch("/Sales/RegisterSale", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(sale)
    }).then(response => {

        $("#btnFinalizeSale" + currentTabId).closest("div.card-body").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {

            $("#cboTypeDocumentSale").val($("#cboTypeDocumentSale option:first").val());

            AllTabsForSale = AllTabsForSale.filter(p => p.idTab != currentTabId)
            document.getElementById('cerrarTab' + currentTabId).click()

            swal("Registrado!", `Número de venta : ${responseJson.object.saleNumber}`, "success");

        } else {
            swal("Lo sentimos", "La venta no fué registrada", "error");
        }
    }).catch((error) => {
        $("#btnFinalizeSale" + currentTabId).closest("div.card-body").LoadingOverlay("hide")
    })


})

document.onkeyup = function (e) {
    if (e.altKey && e.which == 78) {
        newTab();
    }
};

$('#btn-add-tab').click(function () {
    newTab();
});

$('#tab-list').on('click', '.close', function () {
    var tabID = $(this).parents('button').attr('data-bs-target');
    $(this).parents('li').remove();
    $(tabID).remove();

    if ($('.nav-item').length === 1) {
        newTab();
    }
    else {
        lastTab();
    }
});

function newTab() {
    tabID++;

    $('#tab-list').append($('<li class="nav-item" role="presentation">    <button class="nav-link" id="profile-tab' + tabID + '" data-bs-toggle="tab" data-bs-target="#tab' + tabID + '" type="button" role="tab" aria-controls="tab' + tabID + '" aria-selected="false">     <span>Nueva venta &nbsp;</span> <a class="close" type="button" title="Cerrar tab" id="cerrarTab' + tabID + '">×</a>   </button>   </li>'));
    $('#tab-content').append($('<div class="tab-pane fade" id="tab' + tabID + '">    </div>'));


    var clone = originalTab.cloneNode(true);
    clone.id = "nuevaVenta" + tabID;
    clone.querySelector("#cboSearchProduct").id = "cboSearchProduct" + tabID;
    clone.querySelector("#tbProduct").id = "tbProduct" + tabID;
    clone.querySelector("#cboTypeDocumentSale").id = "cboTypeDocumentSale" + tabID;
    clone.querySelector("#txtTotal").id = "txtTotal" + tabID;
    clone.querySelector("#btnFinalizeSale").id = "btnFinalizeSale" + tabID;
    clone.querySelector("#cboCliente").id = "cboCliente" + tabID;

    $('#tab' + tabID).append(clone);

    $("#btnFinalizeSale" + tabID).attr("tabId", tabID);

    var newTab = {
        idTab: tabID,
        products: []
    }
    AllTabsForSale.push(newTab);

    addFunctions(tabID);

    lastTab();
}

function addFunctions(idTab) {
    $('#cboCliente' + idTab).select2({
        ajax: {
            url: "/Sales/GetClientes",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idCliente,
                            text: item.nombre,
                            total: item.total,
                            cuil: item.cuil,
                            color: item.color
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando cliente...',
        minimumInputLength: 3,
        templateResult: formatResultsClients,
        allowClear: true
    });

    $('#cboCliente' + idTab).on('select2:select', function (e) {
        var data = e.params.data;
    })

    $('#cboSearchProduct' + idTab).select2({
        ajax: {
            url: "/Sales/GetProducts",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idProduct,
                            text: item.description,

                            brand: item.brand,
                            category: item.nameCategory,
                            photoBase64: item.photoBase64,
                            price: parseFloat(item.price),
                            tipoVenta: item.tipoVenta
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResults,
        allowClear: true
    });

    $('#cboSearchProduct' + idTab).on('select2:select', function (e) {
        var data = e.params.data;
        var quantity_product_found = 0;
        var currentTab = AllTabsForSale.find(item => item.idTab == idTab);

        if (currentTab.products.length !== 0) {

            let product_found = currentTab.products.filter(prod => prod.idproduct == data.id && prod.promocion == null)
            if (product_found.length == 1) {

                quantity_product_found = product_found[0].quantity;
                currentTab.products.splice(product_found.row, 1);
            }
        }
        if (data.tipoVenta == 2) {
            setNewProduct(1, quantity_product_found, data, currentTab, idTab);
            return;
        }
        swal({
            title: data.text,
            text: data.brand,
            type: "input",
            showcancelbutton: true,
            closeonconfirm: false,
            inputplaceholder: "ingrese cantidad"
        }, function (value) {

            if (value === false) return false;

            if (value === "") {
                toastr.warning("", "debes ingresar el monto");
                return;
            }

            if (isNaN(parseInt(value))) {
                toastr.warning("", "debes ingresar un valor numérico");
                return false
            }

            setNewProduct(value, quantity_product_found, data, currentTab, idTab);

            $('#cboSearchProduct' + idTab).val("").trigger('change');
            $('#cboSearchProduct' + idTab).select2('open');
            swal.close();
        });
    })

}

function setNewProduct(cant, quantity_product_found, data, currentTab, idTab) {

    let totalQuantity = parseFloat(cant) + parseFloat(quantity_product_found);
    data.total = totalQuantity * data.price;
    data.quantity = totalQuantity;

    data = applayPromociones(totalQuantity, data, currentTab);

    let product = {
        idproduct: data.id,
        brandproduct: data.brand,
        descriptionproduct: data.text,
        categoryproducty: data.category,
        quantity: data.quantity,
        price: data.price.toString(),
        total: (data.total).toString(),
        promocion: data.promocion
    };

    currentTab.products.push(product);

    showProducts_Prices(idTab, currentTab);
    $('#cboSearchProduct' + idTab).val("").trigger('change');
    $('#cboSearchProduct' + idTab).select2('open');
}

function applayPromociones(totalQuantity, data, currentTab) {
    let promPorProducto = promociones.find(item => item.idProducto == data.id);

    if (promPorProducto != null) {
        if (promPorProducto.idProducto != null && promPorProducto.idCategory.length == 0 && promPorProducto.dias.length == 0) { // producto
            data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);

        } else if (promPorProducto.idProducto != null && promPorProducto.idCategory.length != 0 && promPorProducto.dias.length == 0) { //producto categoria
            if (containsCategoria(idCategory, data.idCategory)) {
                data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);
            }

        } else if (promPorProducto.idProducto != null && promPorProducto.idCategory.length == 0 && promPorProducto.dias.length != 0) { // producto dia
            if (containsDias(promPorProducto.dias)) {
                data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);
            }

        } else if (promPorProducto.idProducto != null && promPorProducto.idCategory.length != 0 && promPorProducto.dias.length != 0) { // producto categoria dia
            if (containsDias(promPorProducto.dias) && containsCategoria(idCategory, data.idCategory)) {
                data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);
            }
        }
    }

    let promPorCat = promociones.find(item => item.idCategory.includes(data.idCategory));

    if (promPorCat != null) {
        if (promPorCat.idProducto == null && promPorCat.idCategory.length != 0 && promPorCat.dias.length == 0) { // categoria
            if (containsCategoria(idCategory, data.idCategory)) {
                data = calcularPrecioPorcentaje(data, promPorCat, totalQuantity);
                data.promocion = promPorCat.nombre;
            }

        }
        if (promPorCat.idProducto == null && promPorCat.idCategory.length != 0 && promPorCat.dias.length != 0) { // categoria dia
            if (containsDias(promPorCat.dias) && containsCategoria(idCategory, data.idCategory)) {
                data = calcularPrecioPorcentaje(data, promPorCat, totalQuantity);
                data.promocion = promPorCat.nombre;
            }
        }
    }

    let currentdate = new Date();
    let today = currentdate.getDay().toString();

    let promPorDia = promociones.find(item => item.dias.includes(today));

    if (promPorDia != null) {
        if (promPorDia.idProducto == null && promPorDia.idCategory.length == 0 && promPorDia.dias.length != 0) { // dia
            data = calcularPrecioPorcentaje(data, promPorDia, totalQuantity);
            data.promocion = promPorDia.nombre;
        }
    }

    return data;
}

function containsCategoria(cat, catProducto) {
    if (cat.includes(catProducto))
        true;
    false
}

function containsDias(dias) {

    let currentdate = new Date();
    let today = currentdate.getDay().toString();

    if (dias.includes(today))
        true;
    false
}

function applyForProduct(prom, totalQuantity, data, currentTab) {
    let applay = false;

    switch (prom.operador) {
        case 0:
            if (totalQuantity < prom.cantidadProducto)
                return data;

            let diffDividido = totalQuantity % prom.cantidadProducto;

            if (diffDividido == 0) {
                data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                applay = true;

            } else if (diffDividido > 0) {
                let newProd = {
                    idproduct: data.id,
                    brandproduct: data.brand,
                    descriptionproduct: data.text,
                    categoryproducty: data.category,
                    quantity: diffDividido,
                    price: data.price.toString(),
                    total: data.price * diffDividido
                };
                currentTab.products.push(newProd);

                let difCant = totalQuantity - diffDividido;
                data = calcularPrecioPorcentaje(data, prom, difCant);

                applay = true;
            }

            break;

        case 1:
            if (totalQuantity > prom.cantidadProducto) {
                data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                applay = true;
            }
            break;
    }


    if (applay)
        data.promocion = prom.nombre;

    return data;
}

function calcularPrecioPorcentaje(data, prom, totalQuantity) {
    let precio = 0;

    if (prom.precio != null) {
        precio = prom.precio;
    }
    else if (prom.porcentaje != null) {
        precio = data.price * (1 - (prom.porcentaje / 100));
    }

    data.total = precio * totalQuantity;
    data.price = precio;
    data.quantity = totalQuantity;

    return data;
}

function lastTab() {
    var tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}

function hiddenCliente() {
    var value = document.getElementById("card-cliente").hidden;
    if (value) {
        $("#card-cliente").attr("hidden", false);
    }
    else {
        $("#card-cliente").attr("hidden", true);
    }
}