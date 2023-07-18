
let TaxValue = 0;
let ProductsForSale = [];
var listProducts;
var original = document.getElementById('nuevaVenta');

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
        })


    newTab();
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


//function showProducts_Prices() {

//    let total = 0;
//    let tax = 0;
//    let subtotal = 0;
//    let percentage = TaxValue / 100;

//    $("#tbProduct tbody").html("")

//    ProductsForSale.forEach((item) => {

//        total = total + parseFloat(item.total);

//        $("#tbProduct tbody").append(
//            $("<tr>").append(
//                $("<td>").append(
//                    $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
//                        $("<i>").addClass("mdi mdi-trash-can")
//                    ).data("idProduct", item.idProduct)
//                ),
//                $("<td>").text(item.descriptionProduct),
//                $("<td>").text(item.quantity),
//                $("<td>").text(item.price),
//                $("<td>").text(item.total)
//            )
//        )

//    })

//    subtotal = total / (1 + percentage);
//    tax = total - subtotal;

//    $("#txtSubTotal").val(subtotal.toFixed(2))
//    $("#txtTotalTaxes").val(tax.toFixed(2))
//    $("#txtTotal").val(total.toFixed(2))

//}

$(document).on("click", "button.btn-delete", function () {
    const _idproduct = $(this).data("idProduct")

    ProductsForSale = ProductsForSale.filter(p => p.idProduct != _idproduct)

    showProducts_Prices()
})

$("#btnFinalizeSale").click(function () {

    if (ProductsForSale.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    const vmDetailSale = ProductsForSale;

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSale").val(),
        //customerDocument: $("#txtDocumentClient").val(),
        //clientName: $("#txtNameClient").val(),
        subtotal: $("#txtSubTotal").val(),
        totalTaxes: $("#txtTotalTaxes").val(),
        total: $("#txtTotal").val(),
        detailSales: vmDetailSale
    }

    $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("show")

    fetch("/Sales/RegisterSale", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(sale)
    }).then(response => {

        $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {

            ProductsForSale = [];
            showProducts_Prices();
            $("#txtDocumentClient").val("");
            $("#txtNameClient").val("");
            $("#cboTypeDocumentSale").val($("#cboTypeDocumentSale option:first").val());

            swal("Registrado!", `Número de venta : ${responseJson.object.saleNumber}`, "success");

        } else {
            swal("Lo sentimos", "La venta no fué registrada", "error");
        }
    }).catch((error) => {
        $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("hide")
    })


})


document.onkeyup = function (e) {
    if (e.altKey && e.which == 78) {
        newTab();
    }
};

var button = '<button class="close" type="button" title="Remove this page">×</button>';
var tabID = 0;
function resetTab() {
    var tabs = $("#tab-list li:not(:first)");
    var len = 1
    $(tabs).each(function (k, v) {
        len++;
        $(this).find('a').html('Tab ' + len + button);
    })
    tabID--;
}

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

    $('#tab-list').append($('<li class="nav-item" role="presentation">    <button class="nav-link" id="profile-tab" data-bs-toggle="tab" data-bs-target="#tab' + tabID + '" type="button" role="tab" aria-controls="tab' + tabID + '" aria-selected="false">     <span>Nueva venta &nbsp;</span> <a class="close" type="button" title="Remove this page">×</a>   </button>   </li>'));
    $('#tab-content').append($('<div class="tab-pane fade" id="tab' + tabID + '">    </div>'));


    var clone = original.cloneNode(true); // "deep" clone
    clone.id = "nuevaVenta" + tabID;
    clone.querySelector("#cboSearchProduct").id = "cboSearchProduct" + tabID;
    clone.querySelector("#tbProduct").id = "tbProduct" + tabID;
    clone.querySelector("#cboTypeDocumentSale").id = "cboTypeDocumentSale" + tabID;
    //clone.querySelector("#txtSubTotal").id = "txtSubTotal" + tabID;
    //clone.querySelector("#txtTotalTaxes").id = "txtTotalTaxes" + tabID;
    clone.querySelector("#txtTotal").id = "txtTotal" + tabID;
    clone.querySelector("#btnFinalizeSale").id = "btnFinalizeSale" + tabID;

    $('#tab' + tabID).append(clone);

    addFunctions(tabID);

    lastTab();
}

function addFunctions(id) {
    $('#cboSearchProduct' + id).select2({
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
        minimumInputLength: 1,
        templateResult: formatResults
    });

    $('#cboSearchProduct' + id).on('select2:select', function (e) {
        var data = e.params.data;

        let product_found = ProductsForSale.filter(prod => prod.idProduct == data.id)
        if (product_found.length > 0) {
            $('#cboSearchProduct' + id).val("").trigger('change');
            toastr.warning("", "The product has already been added");
            return false
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


            let product = {
                idproduct: data.id,
                brandproduct: data.brand,
                descriptionproduct: data.text,
                categoryproducty: data.category,
                quantity: parseInt(value),
                price: data.price.toString(),
                total: (parseFloat(value) * data.price).toString()
            }

            ProductsForSale.push(product)

            let total = 0;

            $('#tbProduct'+ id+' tbody').html("")

            ProductsForSale.forEach((item) => {

                total = total + parseFloat(item.total);

                $('#tbProduct' + id + ' tbody').append(
                    $("<tr>").append(
                        $("<td>").append(
                            $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
                                $("<i>").addClass("mdi mdi-trash-can")
                            ).data("idProduct", item.idProduct)
                        ),
                        $("<td>").text(item.descriptionproduct),
                        $("<td>").text(item.quantity),
                        $("<td>").text(item.price),
                        $("<td>").text(item.total)
                    )
                )

            })

            var texts = $('#tbProduct1 tbody tr td').map(function () {
                return $(this).text();
            }).get();
            console.log(texts);

            $('#txtTotal' + id).val(total.toFixed(2))

            $('#cboSearchProduct' + id).val("").trigger('change');
            swal.close();
        });
    })
    
}

function lastTab() {
    var tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}