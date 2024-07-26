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

let isHealthy = false;

$(document).ready(function () {
    SEARCH_VIEW["searchDate"]();

    $("#txtStartDate").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#txtEndDate").datepicker({ dateFormat: 'dd/mm/yy' });

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboTypeDocumentSale").append(
                $("<option>").val('').text('')
            )
            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboTypeDocumentSale").append(
                        $("<option>").val(item.idTypeDocumentSale).text(item.description)
                    )
                });
            }
        });

    var saleNumberByClient = $('#txtSaleNumberByClient').text().trim();

    if (saleNumberByClient != '') {
        $('#cboSearchBy').val('number');
        SEARCH_VIEW["searchSale"]();
        $('#txtSaleNumberSearch').val(saleNumberByClient);
        $("#btnSearch").click();
    }

    healthcheck();
})

async function healthcheck() {
    isHealthy = await getHealthcheck();

    if (isHealthy) {
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}

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
    //let presupuestos = document.getElementById("switchPresupuestos").checked
    let presu = $('.filtro-presupuesto:checked').attr('id');


    $(".card-body").find("div.row").LoadingOverlay("show")

    fetch(`/Sales/History?saleNumber=${saleNumber}&startDate=${startDate}&endDate=${endDate}&presupuestos=${presu}`)
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

                    var changePresu = false;
                    if (sale.typeDocumentSale == "Presupuesto") {
                        changePresu = true;
                        sale.typeDocumentSale = sale.typeDocumentSale + " ";
                    }

                    $("#tbsale tbody").append(
                        $("<tr>").append(
                            $("<td>").text(sale.registrationDate),
                            $("<td>").text(sale.saleNumber),
                            $("<td>").text(sale.typeDocumentSale)
                                .append(changePresu != "" ?
                                    $("<button>").addClass("btn btn-success btn-sm btn-pago").append(
                                        $("<i>").addClass("mdi mdi-cash-usd")
                                    ).data("sale", sale) : "")
                            ,
                            $("<td>").text(sale.cantidadProductos),
                            $("<td>").text("$ " + sale.total),
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

$("#tbsale tbody").on("click", ".btn-pago", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }
    var d = $(this).data("sale")

    $("#modalPago").modal("show");
    $("#txtTotalParcial").val(d.total);


    $("#btnFinalizar").click(function () {
        let formaOPago = $("#cboTypeDocumentSale").val();

        if (formaOPago == "") {
            const msg = `Debe completar la forma de pago`;
            toastr.warning(msg, "");
            return false;
        }

        fetch(`/Sales/UpstatSale?idSale=${d.idSale}&formaPago=${formaOPago}`)
            .then(response => {
                $("#modalPago").modal("hide");
                swal("Exitoso!", "Se ha modificado la venta!", "success");
                $("#btnSearch").click();
            })
    })
})

$("#tbsale tbody").on("click", ".btn-info", function () {

    let d = $(this).data("sale")
    $("#txtRegistrationDate").val(d.registrationDate)
    $("#txtSaleNumber").val(d.saleNumber)
    $("#txtRegisterUser").val(d.users)
    $("#txtDocumentType").val(d.typeDocumentSale)
    $("#txtClientName").val(d.clientName)
    $("#txtTotal").val("$ " + d.total)
    $("#txtDescRec").val(d.descuentoRecargo)
    idSale = d.idSale;

    $("#tbProducts tbody").html("")

    d.detailSales.forEach((item) => {
        $("#tbProducts tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descriptionProduct),
                $("<td>").text(item.quantity),
                $("<td>").text("$ " + item.price + " ").append(item.promocion != null ?
                    $("<i>").addClass("mdi mdi-percent").attr("data-toggle", "tooltip").attr("title", item.promocion) : ""),
                $("<td>").text("$ " + item.total)
            )
        )
    })

    $("#linkPrint").attr("href", `/Sales/ShowPDFSale?idSale=${d.saleNumber}`);

    $("#modalData").modal("show");
})

let idSale;

$("#printTicket").click(function () {

    fetch(`/Sales/PrintTicket?idSale=${idSale}`, {
    }).then(response => {

        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {
            $("#modalData").modal("hide");

            if (isHealthy && responseJson.object.nombreImpresora != '') {

                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora);

                swal("Exitoso!", "Ticket impreso!", "success");
            }

        } else {
            swal("Lo sentimos", "La venta no fué registrada. Error: " + responseJson.message, "error");
        }
    })

})


function setToday() {
    let date = new Date();

    let split1 = date.toLocaleString("es-AR", { timeZone: "America/Argentina/Buenos_Aires" }).split(',')[0];
    let split2 = split1.split('/');

    let today = new Date(split2[2], split2[1] - 1, split2[0]);

    $('#txtStartDate, #txtEndDate').datepicker('setDate', today);

    $("#btnSearch").click();
}