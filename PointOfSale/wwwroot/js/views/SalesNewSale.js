
let TaxValue = 18;
let ProductsForSale = [];

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

    //fetch("/Negocio/Obtener")
    //    .then(response => {
    //        return response.ok ? response.json() : Promise.reject(response);
    //    }).then(responseJson => {
    //        if (responseJson.estado) {

    //            const d = responseJson.objeto;

    //            $("#inputGroupSubTotal").text(`Sub Total - ${d.simboloMoneda}`)
    //            $("#inputGroupIGV").text(`IGV(${d.porcentajeImpuesto}%) - ${d.simboloMoneda}`)
    //            $("#inputGroupTotal").text(`Total - ${d.simboloMoneda}`)

    //            TaxValue = parseFloat(d.porcentajeImpuesto)

    //        } else {
    //            $("#inputGroupIGV").text(`IGV(0}%)`)
    //            TaxValue = 0
    //        }
    //    })

    $("#cboSearchProduct").select2({
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
        placeholder: 'Search product...',
        minimumInputLength: 1,
        templateResult: formatResults
    });


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
                    <p style="font-weight: bolder;margin:2px">${data.brand}</p>
                    <p style="margin:2px">${data.text}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}


$(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
});

$('#cboSearchProduct').on('select2:select', function (e) {
    var data = e.params.data;

    let product_found = ProductsForSale.filter(prod => prod.idProduct == data.id)
    if (product_found.length > 0) {
        $("#cboSearchProduct").val("").trigger('change');
        toastr.warning("", "The product has already been added");
        return false
    }

    swal({
        title: data.brand,
        text: data.text,
        type: "input",
        showCancelButton: true,
        closeOnConfirm: false,
        inputPlaceholder: "Enter quantity"
    }, function (value) {

        if (value === false) return false;

        if (value === "") {
            toastr.warning("", "You need to enter the amount");
            return false
        }

        if (isNaN(parseInt(value))) {
            toastr.warning("", "You must enter a numeric value");
            return false
        }


        let product = {
            idProduct: data.id,
            brandProduct: data.brand,
            descriptionProduct: data.text,
            categoryProducty: data.category,
            quantity: parseInt(value),
            price: data.price.toString(),
            total: (parseFloat(value) * data.price).toString()
        }

        ProductsForSale.push(product)
        showProducts_Prices();

        $("#cboSearchProduct").val("").trigger('change');
        swal.close();

    });

});

function showProducts_Prices() {

    let total = 0;
    let tax = 0;
    let subtotal = 0;
    let percentage = TaxValue / 100;

    $("#tbProduct tbody").html("")

    ProductsForSale.forEach((item) => {

        total = total + parseFloat(item.total);

        $("#tbProduct tbody").append(
            $("<tr>").append(
                $("<td>").append(
                    $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
                        $("<i>").addClass("mdi mdi-trash-can")
                    ).data("idProduct", item.idProduct)
                ),
                $("<td>").text(item.descriptionProduct),
                $("<td>").text(item.quantity),
                $("<td>").text(item.price),
                $("<td>").text(item.total)
            )
        )

    })

    subtotal = total / (1 + percentage);
    tax = total - subtotal;

    $("#txtSubTotal").val(subtotal.toFixed(2))
    $("#txtTotalTaxes").val(tax.toFixed(2))
    $("#txtTotal").val(total.toFixed(2))

}

$(document).on("click", "button.btn-delete", function () {
    const _idproduct = $(this).data("idProduct")

    ProductsForSale = ProductsForSale.filter(p => p.idProduct != _idproduct)

    showProducts_Prices()
})

$("#btnFinalizeSale").click(function () {

    if (ProductsForSale.length < 1) {
        toastr.warning("", "You must enter products");
        return;
    }

    const vmDetailSale = ProductsForSale;

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSale").val(),
        customerDocument: $("#txtDocumentClient").val(),
        clientName: $("#txtNameClient").val(),
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

            swal("Registered!", `Sale Number : ${responseJson.object.saleNumber}`, "success");

        } else {
            swal("We're sorry", "The sale could not be registered", "error");
        }
    }).catch((error) => {
        $("#btnFinalizeSale").closest("div.card-body").LoadingOverlay("hide")
    })


})