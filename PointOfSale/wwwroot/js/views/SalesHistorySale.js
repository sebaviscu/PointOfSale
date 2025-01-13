let idSale;
let rowSelectedHistoric;
let tableReportSale;
let isAdmin;
let isHealthySaleHistory = false;

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


$(document).ready(function () {

    isAdmin = $('#txtIsAdmin').text().trim() == 'True';

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

    let saleNumberByClient = $('#txtSaleNumberByClient').text().trim();

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
        $('#printTicket').prop('disabled', false);
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        $('#printTicket').prop('disabled', true);
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


    let d = tableReportSale.row($(this).closest('tr')).data();

    let formattedDate = moment(d.registrationDate).format("DD/MM/YYYY HH:mm");

    $("#txtRegistrationDate").val(formattedDate)
    $("#txtSaleNumber").val(d.saleNumber)
    $("#txtRegisterUser").val(d.registrationUser)
    $("#txtDocumentType").val(d.typeDocumentSale)
    $("#txtClientName").val(d.clientName)
    $("#txtTotal").val(d.total)
    $("#txtDescRec").val(d.descuentoRecargo)

    const divObs = document.getElementById("divObs");
    if (d.observaciones) {
        $("#txtObservaciones").text(d.observaciones);
        divObs.style.display = '';
    } else {
        divObs.style.display = 'none';
    }

    document.getElementById("btnAnular").style.display = d.isDelete ? 'none' : '';

    document.getElementById("btnVerFactura").style.display = d.resultadoFacturacion != null ? '' : 'none';

    if (d.resultadoFacturacion != null) {
        const $btnVerFactura = $("#btnVerFactura");

        if (d.resultadoFacturacion == true) {
            $btnVerFactura.removeClass("text-organe").addClass("btn-success");
            $btnVerFactura.html('<i class="mdi mdi-checkbox-marked-circle"></i> Ver Factura');
        } else {
            $btnVerFactura.removeClass("btn-success").addClass("text-organe");
            $btnVerFactura.html('<i class="mdi mdi-minus-circle-outline"></i> Facturas');
        }
        if (isAdmin) {
            $btnVerFactura.off("click").on("click", function () {
                if (d.idFacturaEmitida != null) {
                    window.location.href = `/Admin/FacturacionById?idFacturaEmitida=${d.idFacturaEmitida}`;
                }
            });
        }
    }


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

    if ($.fn.DataTable.isDataTable("#tbsale")) {
        $('#tbsale').DataTable().destroy();
    }

    $("#tbsale tbody").html("");

    tableReportSale = $("#tbsale").DataTable({
        responsive: true,
        data: responseJson,
        pageLength: 100,
        columns: [
            {
                data: "registrationDate",
                render: function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data, 'YYYY-MM-DDTHH:mm:ss.SS').format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            },
            { data: "registrationUser" },
            {
                data: null,
                render: function (data, type, row) {
                    let content = `${row.saleNumber}`;
                    if (row.isWeb) {
                        content += `<i class="mdi mdi-web ms-3" title="Venta Web"></i>`;
                    }
                    if (row.isDelete) {
                        content += `<i class="mdi mdi-cancel ms-3 text-danger" title="Anulada"></i>`;
                    }

                    if (row.resultadoFacturacion != null && !row.resultadoFacturacion) {
                        content += `<i class="mdi mdi-minus-circle-outline ms-3" style="color:orange;" title="Error al facturar"></i>`;
                    }
                    return content;
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    let button = row.typeDocumentSale === "Presupuesto" ?
                        `<button class="btn btn-success btn-sm btn-pago"><i class="mdi mdi-cash-usd"></i></button>` : "";
                    return `${row.typeDocumentSale} ${button}`;
                }
            },
            { data: "cantidadProductos" },
            {
                data: "total",
                render: $.fn.dataTable.render.number(',', '.', 2, '$ ')
            },
            {
                "defaultContent": '<button class="btn btn-info btn-sm"><i class="mdi mdi-eye"></i></button>',
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
                filename: 'Reporte Ventas',
                exportOptions: {
                    columns: [0, 1, 2, 3, 4]
                }
            },
            'pageLength'
        ]
    });

    let total = responseJson.reduce((acc, sale) => acc + parseFloat(sale.totalDecimal), 0);
    let uniqs = responseJson.reduce((acc, val) => {
        acc[val.saleNumber] = acc[val.saleNumber] === undefined ? 1 : acc[val.saleNumber] += 1;
        return acc;
    }, {});

    if (isAdmin) {
        $("#lblCantidadVentas").html("Cantidad de Ventas: <strong> " + Object.keys(uniqs).length + ".</strong>");

        const groupedSales = responseJson.reduce((acc, sale) => {
            if (!acc[sale.tipoFactura]) {
                acc[sale.tipoFactura] = { tipoFactura: sale.tipoFactura, total: 0 };
            }
            acc[sale.tipoFactura].total += sale.total;
            return acc;
        }, {});

        // Convertir el objeto agrupado en un array
        const groupedData = Object.values(groupedSales);

        // Generar el contenido HTML
        let htmlContent = `Total: <strong>$ ${total.toFixed(0)}</strong>`;
        groupedData.forEach(item => {
            htmlContent += ` | Factura ${item.tipoFactura}: <strong>$ ${item.total.toFixed(0)}</strong>`;
        });

        // Asignar el contenido al elemento
        $("#lbltotal").html(htmlContent);

        //$("#lbltotal").html("Total: <strong>$ " + total.toFixed(2) + ".</strong>");
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
