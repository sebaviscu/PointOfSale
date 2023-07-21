
var originalTab = document.getElementById('nuevaVenta');
let AllTabsForSale = [];
var buttonCerrarTab = '<button class="close" type="button" title="Cerrar tab">×</button>';
var tabID = 0;

const ProducstTab = {
    idTab: 0,
    products: []
}

$(document).ready(function () {

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

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
})

function formatResults(data) {

    if (data.loading)
        return data.text;

    var container = $(
        `<table width="100%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <p style="margin:2px">${data.brand}</p>
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

    $('#tbProduct' + idTab + ' tbody').html("")

    currentTab.products.forEach((item) => {

        total = total + parseFloat(item.total);

        $('#tbProduct' + idTab + ' tbody').append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
                        $("<i>").addClass("mdi mdi-trash-can")
                    ).data("idproduct", item.idproduct).data("idTab", idTab)
                ),
                $("<td>").text(item.descriptionproduct),
                $("<td>").text(item.quantity),
                $("<td>").text("$ " + item.price),
                $("<td>").text("$ " + item.total)
            )
        )

    })

    $('#txtTotal' + idTab).val(total.toFixed(2))
}

$(document).on("click", "button.btn-delete", function () {
    const _idproduct = $(this).data("idproduct");
    const currentTabId = $(this).data("idTab");

    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);
    currentTab.products = currentTab.products.filter(p => p.idproduct != _idproduct)

    showProducts_Prices(currentTabId, currentTab);
})

$(document).on("click", "button.finalizeSale", function () {
    var currentTabId = $(this).attr("tabId");
    var currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    if (currentTab.products.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    const vmDetailSale = currentTab.products;

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSale" + currentTabId).val(),
        //customerDocument: $("#txtDocumentClient").val(),
        //clientName: $("#txtNameClient").val(),
        subtotal: $("#txtSubTotal" + currentTabId).val(),
        totalTaxes: $("#txtTotalTaxes" + currentTabId).val(),
        total: $("#txtTotal" + currentTabId).val(),
        detailSales: vmDetailSale
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

            //showProducts_Prices();
            //$("#txtDocumentClient").val("");
            //$("#txtNameClient").val("");
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


    var clone = originalTab.cloneNode(true); // "deep" clone
    clone.id = "nuevaVenta" + tabID;
    clone.querySelector("#cboSearchProduct").id = "cboSearchProduct" + tabID;
    clone.querySelector("#tbProduct").id = "tbProduct" + tabID;
    clone.querySelector("#cboTypeDocumentSale").id = "cboTypeDocumentSale" + tabID;
    clone.querySelector("#txtTotal").id = "txtTotal" + tabID;
    clone.querySelector("#btnFinalizeSale").id = "btnFinalizeSale" + tabID;

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
                            price: parseFloat(item.price)
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResults
    });

    $('#cboSearchProduct' + idTab).on('select2:select', function (e) {
        var data = e.params.data;
        var quantity_product_found = 0;
        var currentTab = AllTabsForSale.find(item => item.idTab == idTab);

        if (currentTab.products.length !== 0) {

            let product_found = currentTab.products.filter(prod => prod.idproduct == data.id)
            if (product_found.length > 0) {

                quantity_product_found = product_found[0].quantity;
            }
            currentTab.products = currentTab.products.filter(p => p.idproduct != data.id)
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
                return false
            }

            if (isNaN(parseInt(value))) {
                toastr.warning("", "debes ingresar un valor numérico");
                return false
            }

            let totalQuantity = parseFloat(value) + parseFloat(quantity_product_found);

            let product = {
                idproduct: data.id,
                brandproduct: data.brand,
                descriptionproduct: data.text,
                categoryproducty: data.category,
                quantity: totalQuantity,
                price: data.price.toString(),
                total: (totalQuantity * data.price).toString()
            }

            currentTab.products.push(product);

            showProducts_Prices(idTab, currentTab);

            $('#cboSearchProduct' + idTab).val("").trigger('change');
            $('#cboSearchProduct' + idTab).select2('open');
            swal.close();
        });
    })

}

function lastTab() {
    var tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}