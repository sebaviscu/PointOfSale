
var originalTab = document.getElementById('nuevaVenta');
var AllTabsForSale = [];
var buttonCerrarTab = '<button class="close" type="button" title="Cerrar tab">×</button>';
var tabID = 0;
var formaDePagoID = 0;
var promociones = [];
var productSelected = null;

const ProducstTab = {
    idTab: 0,
    products: []
}

$(document).ready(function () {

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboTypeDocumentSaleParcial").append(
                $("<option>").val('').text('')
            )
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTypeDocumentSaleParcial").append(
                        $("<option>").val(item.idTypeDocumentSale).text(item.description)
                    )
                });
            }
        });

    fetch("/Admin/GetPromocionesActivas")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            promociones = responseJson.data;
        });

    newTab();
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
                    <p>$ ${parseInt(data.price)}</p>
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
    let subTotal = 0;
    let row = 0;
    let totalPromocion = 0;
    $('#tbProduct' + idTab + ' tbody').html("")

    currentTab.products.forEach((item) => {
        item.row = row;
        total = total + parseFloat(item.total);
        subTotal = subTotal + parseFloat(item.total);
        $('#tbProduct' + idTab + ' tbody').append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-delete btn-sm delete-item-" + idTab).append(
                        $("<i>").addClass("mdi mdi-trash-can")
                    ).data("idproduct", item.idproduct).data("idTab", idTab).data("row", row)
                ),
                $("<td>").text(item.descriptionproduct),
                $("<td>").text(item.quantity),
                $("<td>").text("$ " + parseInt(item.price)),
                $("<td>").text("$ " + parseInt(item.total)),
                $("<td>").append(item.promocion != null ?
                    $("<i>").addClass("mdi mdi-percent").attr("data-toggle", "tooltip").attr("title", item.promocion) : "")
            )
        )
        if (item.diferenciaPromocion != null) {
            totalPromocion = totalPromocion + parseFloat(item.diferenciaPromocion) * -1;
            subTotal = subTotal + item.diferenciaPromocion;
        }
        row++;
    })

    $('#txtTotal' + idTab).val(parseInt(total));
    $('#txtPromociones' + idTab).html('$ ' + parseInt(totalPromocion));
    $('#txtSubTotal' + idTab).html('$ ' + parseInt(subTotal));
    $("#txtSubTotal" + idTab).attr("totalReal", parseInt(total));

    $("#lblCantidadProductos" + idTab).html("Cantidad de Articulos: <strong> " + currentTab.products.length + "</strong>");
}

$(document).on("click", "button.btn-delete", function () {
    const currentTabId = $(this).data("idTab");
    const row = $(this).data("row");

    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    currentTab.products.splice(row, 1);

    showProducts_Prices(currentTabId, currentTab);
})

//$(document).on("click", "button.finalizeSale", function () { // DEPRECADO
//    var currentTabId = $(this).attr("tabId");
//    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

//    if (currentTab.products.length < 1) {
//        toastr.warning("", "Debe ingresar productos");
//        return;
//    }

//    if (document.getElementById("cboTypeDocumentSale" + currentTabId).value == '') {
//        const msg = `Debe completaro el campo Tipo de Venta`;
//        toastr.warning(msg, "");
//        return;
//    }

//    const vmDetailSale = currentTab.products;

//    const sale = {
//        idTypeDocumentSale: $("#cboTypeDocumentSale" + currentTabId).val(),
//        clientId: $("#cboCliente" + currentTabId).val() != '' ? $("#cboCliente" + currentTabId).val() : null,
//        total: $("#txtTotal" + currentTabId).val(),
//        detailSales: vmDetailSale,
//        tipoMovimiento: $("#cboCliente" + currentTabId).val() != '' ? 2 : null,
//        imprimirTicket: document.querySelector('#cboImprimirTicket').checked
//    }

//    fetch("/Sales/RegisterSale", {
//        method: "POST",
//        headers: { 'Content-Type': 'application/json;charset=utf-8' },
//        body: JSON.stringify(sale)
//    }).then(response => {

//        return response.ok ? response.json() : Promise.reject(response);
//    }).then(responseJson => {

//        if (responseJson.state) {

//            $("#cboTypeDocumentSale").val($("#cboTypeDocumentSale option:first").val());

//            AllTabsForSale = AllTabsForSale.filter(p => p.idTab != currentTabId)
//            document.getElementById('cerrarTab' + currentTabId).click()

//            swal("Registrado!", `Número de venta : ${responseJson.object.saleNumber}`, "success");

//        } else {
//            swal("Lo sentimos", "La venta no fué registrada", "error");
//        }
//    })


//})

function cleanSaleParcial() {
    for (var i = 1; i < formaDePagoID + 1; i++) {
        const element = document.getElementById("nuevaFormaDePago" + i);
        if (element != null)
            element.remove();
    }

    formaDePagoID = 0;
}

$(document).on("click", "button.finalizarSaleParcial", function () {
    cleanSaleParcial();

    var currentTabId = $(this).attr("tabId");
    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    if (currentTab.products.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    let total = $("#txtTotal" + currentTabId).val();

    $("#txtTotalView").val(total);
    $("#txtTotalParcial").val(total);
    $("#txtSumaSubtotales").val(0);
    $("#btnCalcSubTotales").attr("idFormaDePago", 0);

    $("#txtTotalParcial").on("change", function () {
        calcularSuma();
    });

    $("#btnCalcSubTotales").on("click", function () {
        let subTotal = getSumaSubTotales();

        let valorParcial = $("#txtTotalParcial").val();
        $("#txtTotalParcial").val(parseFloat(subTotal) + parseFloat(valorParcial));
        calcularSuma();
    });

    $("#modalDividirPago").removeAttr("idTab");
    $("#modalDividirPago").attr("idTab", currentTabId);

    $("#modalDividirPago").modal("show")
})


$(document).on("click", "button.btnAddFormaDePago", function () {
    formaDePagoID++;

    $('#formaDePagoPanel').append($('<div class="form-row" id="formaPago' + formaDePagoID + '"></div>'));

    let originalFormaPago = document.getElementById('formaDePagoPanel');
    let cloneFP = originalFormaPago.cloneNode(true);
    cloneFP.id = "nuevaFormaDePago" + formaDePagoID;
    cloneFP.querySelector("#txtTotalParcial").id = "txtTotalParcial" + formaDePagoID;
    cloneFP.querySelector("#cboTypeDocumentSaleParcial").id = "cboTypeDocumentSaleParcial" + formaDePagoID;
    cloneFP.querySelector("#btnAddNuevaFormaDePago").id = "btnAddNuevaFormaDePago" + formaDePagoID;
    cloneFP.querySelector("#btnCalcSubTotales").id = "btnCalcSubTotales" + formaDePagoID;

    $('#rowDividirPago').append(cloneFP);
    $("#btnCalcSubTotales" + formaDePagoID).attr("idformadepago", formaDePagoID);
    $("#txtTotalParcial" + formaDePagoID).val(0);

    $("#txtTotalParcial" + formaDePagoID).on("change", function (e) {
        let result = calcularSuma();

        if (result < 0) {
            e.currentTarget.classList.add("invalid")
        } else {
            e.currentTarget.classList.remove("invalid")
        }
    });
    return false;

})


$(document).on("click", "a.calcSubTotales", function (e) {
    var idFormaDePago = e.currentTarget.attributes.idformadepago.value;
    let subTotal = getSumaSubTotales();

    let valorParcial = $("#txtTotalParcial" + idFormaDePago).val();
    $("#txtTotalParcial" + idFormaDePago).val(parseFloat(subTotal) + parseFloat(valorParcial));
    calcularSuma();
})

function calcularSuma() {
    let subTotal = getSumaSubTotales();
    if (subTotal !== 0) {
        if (subTotal > 0)
            $("#txtSumaSubtotales").html("FALTA <strong>$ " + subTotal + "</strong>");
        else
            $("#txtSumaSubtotales").html("SOBRA <strong>$ " + subTotal + "</strong>");

        $("#txtSumaSubtotales").show();
    }
    else {
        $("#txtSumaSubtotales").hide();
    }

    return subTotal;
}

function getSumaSubTotales() {
    const inputElements = document.querySelectorAll('.inputSubtotal');
    let subTotal = 0;

    inputElements.forEach(function (input) {
        const value = parseFloat(input.value);

        if (!isNaN(value) && value > 0) {
            subTotal += value;
        }
    });

    var totFijo = $("#txtTotalView").val();

    return totFijo - subTotal;
}


$("#btnFinalizarVentaParcial").on("click", function () {
    $("#btnFinalizarVentaParcial").closest("div.card-body").LoadingOverlay("show")

    if ($(".cboFormaDePago").filter(function () { return $(this).val() === ""; }).length !== 0) {
        const msg = `Debe completaro el campo Forma de Pago`;
        toastr.warning(msg, "");
        return;
    }

    let suma = getSumaSubTotales();
    if (suma > 0) {
        const msg = `La suma de los sub totales no es igual al total.`;
        toastr.warning(msg, "");
        return;
    }

    $("#modalDividirPago").modal("hide")
    showLoading();

    var currentTabId = $("#modalDividirPago").attr("idtab");
    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    const vmDetailSale = currentTab.products;

    var formasDePago = [];

    $(".cboFormaDePago").each(function (index) {
        let subTotal = {
            total: index === 0 ? $("#txtTotalParcial").val() : $("#txtTotalParcial" + index).val(),
            formaDePago: index === 0 ? $("#cboTypeDocumentSaleParcial").val() : $("#cboTypeDocumentSaleParcial" + index).val()
        };
        formasDePago.push(subTotal);
    });

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSaleParcial").val(),
        clientId: $("#cboCliente" + currentTabId).val() != '' ? $("#cboCliente" + currentTabId).val() : null,
        total: $("#txtTotalParcial").val(),
        detailSales: vmDetailSale,
        tipoMovimiento: $("#cboCliente" + currentTabId).val() != '' ? 2 : null,
        imprimirTicket: document.querySelector('#cboImprimirTicket').checked,
        multiplesFormaDePago: formasDePago
    }

    fetch("/Sales/RegisterSale", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(sale)
    }).then(response => {

        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        removeLoading();

        if (responseJson.state) {
            //swal("Registrado!", `Número de venta : ${responseJson.object.saleNumber}`, "success");

            var nuevaVentaSpan = document.getElementById('profile-tab' + currentTabId).querySelector('span');
            nuevaVentaSpan.textContent = responseJson.object.saleNumber;

            AllTabsForSale = AllTabsForSale.filter(p => p.idTab != currentTabId)
            cleanSaleParcial();

            $("#btnReimprimirTicket" + tabID).attr("idSale", responseJson.object.idSale);

            if ($(".tab-venta").length > 2) {

                // para cerrar la ultima venta de 3
                var firstTabID = document.getElementsByClassName("tab-venta")[0].getAttribute("data-bs-target")
                $(firstTabID).remove();
                document.getElementsByClassName("li-tab")[0].remove()
            }
            disableAfterVenta(currentTabId);

            newTab();

        } else {
            swal("Lo sentimos", "La venta no fué registrada", "error");
        }
    })
});

function disableAfterVenta(tabID) {
    $('#btnAgregarProducto' + tabID).prop('disabled', true);
    $('#cboSearchProduct' + tabID).prop('disabled', true);
    $('#cboDescRec' + tabID).prop('disabled', true);
    $('#txtPeso' + tabID).prop('disabled', true);
    $('#cboCliente' + tabID).prop('disabled', true);
    $('.delete-item-' + tabID).prop('disabled', true)
    $('#btnFinalizeSaleParcial' + tabID).prop('disabled', true);
    $('#btnFinalizeSaleParcial' + tabID).hide()
    $('#btnReimprimirTicket' + tabID).show()
}

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

    $('#tab-list').append($('<li class="nav-item li-tab" role="presentation">    <button class="nav-link tab-venta" id="profile-tab' + tabID + '" data-bs-toggle="tab" data-bs-target="#tab' + tabID + '" type="button" role="tab" aria-controls="tab' + tabID + '" aria-selected="false">     <span>Nueva venta</span> <a class="close" type="button" title="Cerrar tab" id="cerrarTab' + tabID + '">&nbsp; ×</a>   </button>   </li>'));
    $('#tab-content').append($('<div class="tab-pane fade" id="tab' + tabID + '">    </div>'));


    var clone = originalTab.cloneNode(true);
    clone.id = "nuevaVenta" + tabID;
    clone.querySelector("#cboSearchProduct").id = "cboSearchProduct" + tabID;
    clone.querySelector("#tbProduct").id = "tbProduct" + tabID;
    clone.querySelector("#txtTotal").id = "txtTotal" + tabID;
    clone.querySelector("#btnFinalizeSaleParcial").id = "btnFinalizeSaleParcial" + tabID;
    clone.querySelector("#cboCliente").id = "cboCliente" + tabID;
    clone.querySelector("#txtPeso").id = "txtPeso" + tabID;
    clone.querySelector("#btnAgregarProducto").id = "btnAgregarProducto" + tabID;
    clone.querySelector("#lblCantidadProductos").id = "lblCantidadProductos" + tabID;
    clone.querySelector("#cboDescRec").id = "cboDescRec" + tabID;
    clone.querySelector("#txtSubTotal").id = "txtSubTotal" + tabID;
    clone.querySelector("#txtPromociones").id = "txtPromociones" + tabID;
    clone.querySelector("#txtDescRec").id = "txtDescRec" + tabID;
    clone.querySelector("#btnReimprimirTicket").id = "btnReimprimirTicket" + tabID;

    $('#tab' + tabID).append(clone);

    $("#btnFinalizeSaleParcial" + tabID).attr("tabId", tabID);
    $("#cboDescRec" + tabID).attr("tabId", tabID);

    var newTab = {
        idTab: tabID,
        products: []
    }
    AllTabsForSale.push(newTab);

    addFunctions(tabID);
    lastTab();

    $('#cboSearchProduct' + tabID).val("").trigger('change');
    $('#cboSearchProduct' + tabID).select2('open');
}

function addFunctions(idTab) {

    $("#btnReimprimirTicket" + idTab).click(function () {
        let idSale = $(this).attr("idSale");

        fetch(`/Sales/PrintTicket?idSale=${idSale}`)
            .then(response => {
                $("#modalData").modal("hide");
                swal("Exitoso!", "Ticket impreso!", "success");
            })
    })

    $('#txtPeso' + idTab).on('keypress', function (e) {
        if ($('#txtPeso' + idTab).val() != '' && e.which === 13) {

            //Disable textbox to prevent multiple submit
            $(this).attr("disabled", "disabled");

            agregarProductoEvento(idTab);
            $('#txtPeso' + idTab).val('');

            //Enable the textbox again if needed.
            $(this).removeAttr("disabled");
        }
    });

    $('#cboDescRec' + idTab).change(function () {
        var currentTabId = $(this).attr("tabId");

        let total = $("#txtSubTotal" + currentTabId).attr("totalReal");

        if (total != '') {
            var descRecAplicar = $("#cboDescRec" + currentTabId).val()
            let desc = total * (descRecAplicar / 100);
            $('#txtDescRec' + idTab).html('$ ' + parseInt(desc));

            $("#txtTotal" + currentTabId).val(parseInt(parseInt(total) + desc));
        }
    })

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
                            price: item.price,
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
        let data = e.params.data;
        productSelected = data;
        let quantity_product_found = 0;
        let currentTab = AllTabsForSale.find(item => item.idTab == idTab);

        if (currentTab.products.length !== 0) {

            let product_found = currentTab.products.filter(prod => prod.idproduct == data.id &&
                prod.promocion == null &&
                data.tipoVenta == 2);

            if (product_found.length == 1) {

                quantity_product_found = product_found[0].quantity;
                currentTab.products.splice(product_found[0].row, 1);
            }
        }
        if (data.tipoVenta == 2) {
            var peso = $("#txtPeso" + idTab).val();

            if (peso != "") {
                if (peso === false || isNaN(peso)) return false;
            }

            peso = peso == "" ? 1 : parseFloat(peso);

            setNewProduct(peso, quantity_product_found, data, currentTab, idTab);
            $('#txtPeso' + idTab).val('');

            return;
        }

        const element = document.getElementById("txtPeso" + idTab);
        window.setTimeout(() => element.focus(), 0);

        //swal({
        //    title: data.text,
        //    text: data.brand,
        //    type: "input",
        //    showcancelbutton: true,
        //    closeonconfirm: false,
        //    inputplaceholder: "ingrese cantidad"
        //}, function (value) {

        //    if (value === false) return false;

        //    if (value === "") {
        //        toastr.warning("", "debes ingresar el monto");
        //        return;
        //    }

        //    if (isNaN(parseInt(value))) {
        //        toastr.warning("", "debes ingresar un valor numérico");
        //        return false
        //    }

        //    setNewProduct(value, quantity_product_found, data, currentTab, idTab);

        //    $('#cboSearchProduct' + idTab).val("").trigger('change');
        //    $('#cboSearchProduct' + idTab).select2('open');

        //    swal.close();

        //});
    })

    $("#btnAgregarProducto" + idTab).on("click", function () {
        agregarProductoEvento(idTab);
    })

}

function agregarProductoEvento(idTab) {
    var peso = parseFloat($("#txtPeso" + idTab).val());

    if (peso === false || peso === "" || isNaN(peso)) return false;

    //let data = $("#cboSearchProduct" + idTab).select2('data')[0];

    if (productSelected === null) {
        return false;
    }

    var quantity_product_found = 0;
    var currentTab = AllTabsForSale.find(item => item.idTab == idTab);

    if (currentTab.products.length !== 0) {

        let product_found = currentTab.products.filter(prod => prod.idproduct == productSelected.id && prod.promocion == null)
        if (product_found.length == 1) {

            quantity_product_found = product_found[0].quantity;
            currentTab.products.splice(product_found[0].row, 1);
        }
    }

    setNewProduct(peso, quantity_product_found, productSelected, currentTab, idTab);
}

function setNewProduct(cant, quantity_product_found, data, currentTab, idTab) {
    let totalQuantity = parseFloat(cant) + parseFloat(quantity_product_found);
    data.total = totalQuantity * parseInt(data.price);
    data.quantity = totalQuantity;

    data = applayPromociones(totalQuantity, data, currentTab);

    let product = new Producto();
    product.idproduct = data.id;
    product.brandproduct = data.brand;
    product.descriptionproduct = data.text;
    product.categoryproducty = data.category;
    product.quantity = data.quantity;
    product.price = parseInt(data.price).toString();
    product.total = data.total.toString();
    product.promocion = data.promocion;
    product.diferenciaPromocion = data.diferenciaPromocion;

    currentTab.products.push(product);

    showProducts_Prices(idTab, currentTab);
    $('#cboSearchProduct' + idTab).val("").trigger('change');
    $('#cboSearchProduct' + idTab).select2('open');
    $('#txtPeso' + idTab).val('')
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
    let today = currentdate.getDay();

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
                var precio = parseFloat(data.price);

                if (precio === false || precio === "" || isNaN(precio)) return false;

                let newProd = new Producto();
                newProd.idproduct = data.id;
                newProd.brandproduct = data.brand;
                newProd.descriptionproduct = data.text;
                newProd.categoryproducty = data.category;
                newProd.quantity = diffDividido;
                newProd.price = precio.toString();
                newProd.total = precio * diffDividido;

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
        precio = parseInt(data.price) * (1 - (prom.porcentaje / 100));
    }

    data.diferenciaPromocion = (parseInt(data.price) * totalQuantity) - (precio * totalQuantity);
    data.total = precio * totalQuantity;
    data.price = precio;
    data.quantity = totalQuantity;

    return data;
}

function lastTab() {
    var tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}

var areYouReallySure = false;
function areYouSure() {
    if (allowPrompt) {
        if (!areYouReallySure && true) {
            areYouReallySure = true;
            var confMessage = "";
            return confMessage;
        }
    } else {
        allowPrompt = true;
    }
}

var allowPrompt = true;
//window.onbeforeunload = areYouSure;

class Producto {
    idproduct = 0;
    brandproduct = "";
    descriptionproduct = "";
    categoryproducty = "";
    quantity = 0;
    price = 0;
    total = 0;
    promocion = null;
    diferenciaPromocion = null;
}