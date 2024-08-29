let idSale;
let rowSelectedHistoric;
let tableDataReporteVentas;

let SEARCH_VIEW = {

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

let isHealthySaleHistory = false;

$(document).ready(function () {
    SEARCH_VIEW["searchDate"]();

    $("#txtStartDate").datepicker({ dateFormat: 'dd/mm/yy' });
    $("#txtEndDate").datepicker({ dateFormat: 'dd/mm/yy' });

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            $("#cboTypeDocumentSale").append(
                $("<option>").val('').text('')
            )
            if (responseJson.state > 0) {
                if (responseJson.object.length > 0) {
                    responseJson.object.forEach((item) => {
                        $("#cboTypeDocumentSale").append(
                            $("<option>").val(item.idTypeDocumentSale).text(item.description)
                        )
                    });
                }
            }
            else {
                swal("Lo sentimos", responseJson.message, "error");
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
    isHealthySaleHistory = await getHealthcheck();

    if (isHealthySaleHistory) {
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
    showLoading();

    fetch(`/Sales/History?saleNumber=${saleNumber}&startDate=${startDate}&endDate=${endDate}&presupuestos=${presu}`)
        .then(response => {
            return response.json();
        }).then(responseJson => {

            removeLoading();
            createTable(responseJson);
        })
})

$("#tbsale tbody").on("click", ".btn-pago", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedHistoric = $(this).closest('tr').prev();
    } else {
        rowSelectedHistoric = $(this).closest('tr');
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
    $("#txtRegisterUser").val(d.registrationUser)
    $("#txtDocumentType").val(d.typeDocumentSale)
    $("#txtClientName").val(d.clientName)
    $("#txtTotal").val(d.total)
    $("#txtDescRec").val(d.descuentoRecargo)

    const divObs = document.getElementById("divObs");
    if (d.observaciones) {
        $("#txtObservaciones").val(d.observaciones);
        divObs.style.display = '';
    } else {
        divObs.style.display = 'none';
    }

    document.getElementById("btnAnular").style.display = d.isDelete ? 'none' : '';

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


$("#printTicket").click(function () {
    showLoading();
    fetch(`/Sales/PrintTicket?idSale=${idSale}`, {
    }).then(response => {

        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {
            $("#modalData").modal("hide");

            if (isHealthySaleHistory && responseJson.object.nombreImpresora != '') {

                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);

                swal("Exitoso!", "Ticket impreso!", "success");
                removeLoading();
            }

        } else {
            swal("Lo sentimos", "Error al generar el ticket " + responseJson.message, "error");
        }
    })

})


function createTable(responseJson) {

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
                    $("<td>").append(
                        $("<span>").text(sale.registrationDate),
                        sale.isWeb ? $("<i>")
                            .addClass("mdi mdi-web ms-3")
                            .attr("title", "Venta Web")
                            .tooltip()
                            : 
                            sale.isDelete ? $("<i>")
                            .addClass("mdi mdi-cancel ms-3 text-danger")
                            .attr("title", "Anulada")
                            .tooltip()
                            : null
                    ),
                    $("<td>").text(sale.saleNumber),
                    $("<td>").text(sale.typeDocumentSale)
                        .append(changePresu != "" ?
                            $("<button>").addClass("btn btn-success btn-sm btn-pago").append(
                                $("<i>").addClass("mdi mdi-cash-usd")
                            ).data("sale", sale) : ""),

                    $("<td>").text(sale.cantidadProductos),
                    $("<td>").text("$ " + sale.total),
                    $("<td>").append(
                        $("<button>").addClass("btn btn-info btn-sm").append(
                            $("<i>").addClass("mdi mdi-eye")
                        ).data("sale", sale))
                )
            );


        });
        $("#lblCantidadVentas").html("Cantidad de Ventas: <strong> " + Object.keys(uniqs).length + ".</strong>");
        $("#lbltotal").html("Total: <strong>$ " + total + ".</strong>");
    }

}


function setToday() {
    let date = new Date();

    let split1 = date.toLocaleString("es-AR", { timeZone: "America/Argentina/Buenos_Aires" }).split(',')[0];
    let split2 = split1.split('/');

    let today = new Date(split2[2], split2[1] - 1, split2[0]);

    $('#txtStartDate, #txtEndDate').datepicker('setDate', today);

    $("#btnSearch").click();
}

function turnoActual() {
    showLoading();

    fetch(`/Sales/HistoryTurnoActual`)
        .then(response => {
            $(".card-body").find("div.row").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            removeLoading();
            createTable(responseJson);
        })

}


$("#btnAnular").on("click", function (event) {
    event.preventDefault();
    swal({
        title: "¿Está seguro?",
        text: `Anular la venta`,
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

                fetch(`/Sales/AnularSale?idSale=${idSale}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        swal("Exitoso!", "La venta fué anulada", "success");

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
