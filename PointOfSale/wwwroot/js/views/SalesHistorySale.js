const SEARCH_VIEW = {

    searchDate: () => {

        $("#txtStartDate").val("");
        $("#txtEndDate").val("");
        $("#txtSaleNumber").val("");

        $(".search-date").show()
        $(".search-sale").hide()
    },
    searchSale: () => {

        $("#txtStartDate").val("");
        $("#txtEndDate").val("");
        $("#txtSaleNumber").val("");

        $(".search-sale").show()
        $(".search-date").hide()
    }

}

$(document).ready(function () {
    SEARCH_VIEW["searchDate"]();

    $("#txtStartDate").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#txtEndDate").datepicker({ dateFormat: 'dd/mm/yy' });
})

$("#cboSearchBy").change(function () {

    if ($("#cboSearchBy").val() == "date") {
        SEARCH_VIEW["searchDate"]();
    } else {
        SEARCH_VIEW["searchSale"]();
    }
});

$("#btnSearch").click(function () {

    if ($("#cboSearchBy").val() == "date") {

        if ($("#txtStartDate").val().trim() == "") {
            toastr.warning("", "Debes ingresar al menos la fecha de inicio.");
            return;
        }
    } else {
        if ($("#txtSaleNumberSearch").val().trim() == "") {
            toastr.warning("", "Debes ingresar el número de venta");
            return;
        }
    }

    let saleNumber = $("#txtSaleNumberSearch").val();
    let startDate = $("#txtStartDate").val();

    if ($("#txtEndDate").val().trim() == "") {
        $('#txtEndDate').datepicker('setDate', startDate);
    }

    let endDate = $("#txtEndDate").val().trim();

    $(".card-body").find("div.row").LoadingOverlay("show")

    fetch(`/Sales/History?saleNumber=${saleNumber}&startDate=${startDate}&endDate=${endDate}`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#tbsale tbody").html("");
            if (responseJson.length > 0) {

                let total = 0;

                var uniqs = responseJson.reduce((acc, val) => {
                    acc[val.saleNumber] = acc[val.saleNumber] === undefined ? 1 : acc[val.saleNumber] += 1;
                    return acc;
                }, {});

                responseJson.forEach((sale) => {

                    total = total + parseFloat(sale.totalDecimal);

                    $("#tbsale tbody").append(
                        $("<tr>").append(
                            $("<td>").text(sale.registrationDate),
                            $("<td>").text(sale.saleNumber),
                            $("<td>").text(sale.typeDocumentSale),
                            $("<td>").text(sale.cantidadProductos),
                            $("<td>").text(sale.total),
                            $("<td>").append(
                                $("<button>").addClass("btn btn-info btn-sm").append(
                                    $("<i>").addClass("mdi mdi-eye")
                                ).data("sale", sale))
                        )
                    )
                });
                $("#lblCantidadVentas").html("Cantidad de Ventas: <strong> " + Object.keys(uniqs).length + ".</strong>");
                $("#lbltotal").html("Total: <strong>$ " + total + ".</strong>");
            }
        })
})

$("#tbsale tbody").on("click", ".btn-info", function () {

    let d = $(this).data("sale")
    $("#txtRegistrationDate").val(d.registrationDate)
    $("#txtSaleNumber").val(d.saleNumber)
    $("#txtRegisterUser").val(d.users)
    $("#txtDocumentType").val(d.typeDocumentSale)
    $("#txtClientName").val(d.clientName)
    $("#txtTotal").val(d.total)
    $("#txtDescRec").val(d.descuentoRecargo)
    idSale = d.idSale;

    $("#tbProducts tbody").html("")

    d.detailSales.forEach((item) => {
        $("#tbProducts tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descriptionProduct),
                $("<td>").text(item.quantity),
                $("<td>").text(item.price + " ").append(item.promocion != null ?
                    $("<i>").addClass("mdi mdi-percent").attr("data-toggle", "tooltip").attr("title", item.promocion) : ""),
                $("<td>").text(item.total)
            )
        )
    })

    $("#linkPrint").attr("href", `/Sales/ShowPDFSale?idSale=${d.saleNumber}`);


    $("#modalData").modal("show");
})

let idSale;

$("#printTicket").click(function () {

    fetch(`/Sales/PrintTicket?idSale=${idSale}`)
        .then(response => {
            $("#modalData").modal("hide");
            swal("Exitoso!", "Ticket impreso!", "success");
        })
})

function setToday() {
    var date = new Date();
    var today = new Date(date.getFullYear(), date.getMonth(), date.getDate());

    $('#txtStartDate, #txtEndDate').datepicker('setDate', today);

    $("#btnSearch").click();
}