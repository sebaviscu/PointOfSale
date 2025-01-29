let originalTab = document.getElementById('nuevaVenta');
let dbInstance = null;
var AllTabsForSale = [];
let buttonCerrarTab = '<button class="close" type="button" title="Cerrar tab">×</button>';
let tabID = 0;
let formaDePagoID = 0;
let promociones = [];
let productSelected = null;
let formasDePagosList = [];
let isHealthySale = false;
let ajustes = null;
let selectedRowtbProduct = null;
let lastSearchTerm = '';
let searchCache = {};


$(document).ready(function () {
    showLoading();
    $.fn.select2.defaults.set("selectOnClose", true);

    fetch("/Sales/ListTypeDocumentSale")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                formasDePagosList = responseJson.object;

                if (responseJson.object.length > 0) {
                    responseJson.object.forEach((item) => {
                        $("#cboTypeDocumentSaleParcial").append(
                            $("<option>").val(item.idTypeDocumentSale).text(item.description)
                        )
                    });
                }
            }
            else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        });

    fetch("/Inventory/GetPromocionesActivas")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                promociones = responseJson.object;

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        });

    fetch("/Ajustes/GetAjustesVentas")
        .then(response => {
            removeLoading();
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                ajustes = responseJson.object;

                $('#cboListaPrecios1').val(ajustes.listaPrecios);
                document.getElementById('cboImprimirTicket').checked = ajustes.imprimirDefault;

                if (!ajustes.existeTurno) {
                    $('#cboSearchProduct1').select2('close');

                    $('#bloqueo').show();
                }
                else {
                    $('#bloqueo').hide();


                    newTab();
                    healthcheck();
                    inicializarConsultarPrecios();

                    initializeDatabase()
                        .then((db) => {
                            getAllVentasFromIndexedDB((ventas) => {
                                if (ventas.length > 0) {
                                    AllTabsForSale = ventas;

                                    ventas.forEach((currentTab, index) => {
                                        if (index > 0) {
                                            newTab(); // Ejecutar solo a partir de la segunda venta
                                        }
                                        showProducts_Prices(currentTab.idTab, currentTab);
                                        $("#lblFechaUsuario" + currentTab.idTab).html(currentTab.date + " &nbsp;&nbsp; " + currentTab.user);
                                    });
                                }
                            });
                        })
                        .catch((error) => {
                            console.error('Error al inicializar la base de datos:', error);
                        });

                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })



})

function inicializarConsultarPrecios() {
    $('#modalConsultarPrecio').on('shown.bs.modal', function () {
        if (!$('#cboSearchProductConsultarPrecio').data('select2')) {
            funConsultarPrecio();
        }
        setTimeout(function () {
            $('#cboSearchProductConsultarPrecio').select2('open');
        }, 100);
    });

    $('#modalConsultarPrecio').on('hidden.bs.modal', function () {
        resetModaConsultarPreciol();
    });
}
$("#btnBuscarCliente").on("click", function () {
    $("#modalDatosFactura").modal("show");
    inicializarClientesFactura();
});

async function healthcheck() {
    isHealthySale = await getHealthcheck();

    if (isHealthySale) {
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}

$('#cboTypeDocumentSaleParcial').change(function () {
    let idFormaDePago = $(this).val();
    changeCboTypeDocumentSaleParcial('', idFormaDePago);
})

function changeCboTypeDocumentSaleParcial(idLineaFormaPago, idFormaDePago) {
    let formaDePago = formasDePagosList.find(_ => _.idTypeDocumentSale == idFormaDePago);

    if (formaDePago != null) {
        $("#cboFactura" + idLineaFormaPago).val(formaDePago.tipoFactura);
        $("#cboFactura" + idLineaFormaPago).trigger('change');
    }

    let hayEfectivo = $('.cboFormaDePago').filter(function () {
        return $(this).val() == '1';
    }).length > 0;

    if (hayEfectivo) {
        $("#divVueltoEfectivo").show();
    } else {
        $("#divVueltoEfectivo").hide();
    }

    let formaPagoWithDisc = formaDePago != null && formaDePago.descuentoRecargo != null && formaDePago.descuentoRecargo != 0 && idLineaFormaPago == '';
    if (formaPagoWithDisc) {
        let descripcionDescRec = formaDePago.descuentoRecargo < 0 ? 'Descuento' : 'Recargo';
        toastr.success(formaDePago.descuentoRecargo + " %", "Forma de pago con " + descripcionDescRec);

        let currentTabid = getTabActiveId();
        let total = $("#txtTotal" + currentTabid).attr("totalReal");
        let descuento = parseFloat(total) * (parseInt(formaDePago.descuentoRecargo) / 100);
        let totalWithDisc = parseFloat(total) + descuento;
        $("#txtTotalParcial").val(totalWithDisc);
        $("#txtTotalParcial").attr("descuentoFormaPago", descuento);
    }

    $('#btnAddNuevaFormaDePago' + idLineaFormaPago).prop('hidden', formaPagoWithDisc);
}


$('#cboFactura').change(function () {

    validateTipoFacturaAndMonto('');
})

$('#txtTotalParcial').change(function () {

    validateTipoFacturaAndMonto('');
})

function validateTipoFacturaAndMonto(idLineaFormaPago) {

    let formaPagoId = $('#cboFactura' + idLineaFormaPago).val();
    let monto = $('#txtTotalParcial' + idLineaFormaPago).val();

    if (formaPagoId == 0) {
        // Caso A
        updateUIFormaDePago(true, false, true);
    } else if (formaPagoId == 1 || formaPagoId == 2) {
        // Casos B y C
        if (parseFloat(monto) > parseFloat(ajustes.minimoIdentificarConsumidor)) {
            updateUIFormaDePago(true, true, true);
        } else {
            updateUIFormaDePago(false, false, true);
        }
    } else {
        // Otros casos
        updateUIFormaDePago(false, false, false);
    }

}

function updateUIFormaDePago(disableButton, showMinimo, showCliente) {
    $('#btnFinalizarVentaParcial').prop('disabled', disableButton);
    $("#txtMinimoIdentificarConsumidor").toggle(showMinimo);
    document.getElementById("divClienteSeleccionado").style.display = showCliente ? '' : 'none';
}

function calcularVueltoTotal() {
    let totalEfectivo = 0;
    let pagaCon = parseFloat($('#txtPagaCon').val()) || 0;

    // Iterar sobre todos los elementos .form-row dentro de #formaDePagoPanel
    $('#formaDePagoPanel .form-row').each(function () {
        let formaDePago = $(this).find('.cboFormaDePago').val(); // Obtener el valor del select de forma de pago

        if (formaDePago == 1) {  // Verifica si es "Efectivo"
            let subtotal = parseFloat($(this).find('.inputSubtotal').val()) || 0; // Obtener el subtotal para efectivo
            totalEfectivo += subtotal;  // Sumar al total de efectivo
        }
    });

    // Calcular el vuelto
    let vuelto = pagaCon - totalEfectivo;

    // Asegurarse de que el vuelto no sea negativo
    vuelto = vuelto >= 0 ? vuelto.toFixed(2) : 0;

    // Colocar el resultado en txtVuelto
    $('#txtVuelto').val(vuelto);
}

$('#txtPagaCon').on('input', function () {
    calcularVueltoTotal();

});

function formatResults(data) {

    if (data.loading)
        return data.text;

    let tipoVentaString = data.tipoVenta == 1 ? "Kg" : "U.";

    let container = $(
        `<table width="90%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <p>$ ${formatNumber(data.price)} / ${tipoVentaString}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}
function formatResultsBuscarPrecio(data) {
    if (data.loading)
        return data.text;

    let container = $(
        `<table width="90%">
            <tr>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}

function formatResultsClients(data) {

    if (data.loading)
        return data.text;

    let cuil = data.cuil != null ? data.cuil : '';

    let container = $(
        `<table width="100%">
            <tr>
                <td class="col-sm-8">
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <em style="font-weight: bolder;margin:2px">${cuil}</em>
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
                $("<td>").text(item.quantity).addClass("cantidad"),
                $("<td>").text("$ " + item.price),
                $("<td>").text("$ " + item.total),
                $("<td>").append(item.promocion != null ?
                    $("<i>").addClass("mdi mdi-percent").attr("data-toggle", "tooltip").attr("title", item.promocion) : "")
            )
        )
        if (item.diferenciapromocion != null) {
            totalPromocion = totalPromocion + parseFloat(item.diferenciapromocion) * -1;
            subTotal = subTotal + parseFloat(item.diferenciapromocion);
        }

        row++;
    })

    $('#txtTotal' + idTab).val('$ ' + formatNumber(total));
    $("#txtTotal" + idTab).attr("totalReal", parseFloat(total).toFixed(2));
    $('#txtPromociones' + idTab).html('$ ' + formatNumber(totalPromocion));
    $('#txtPromociones' + idTab).val(totalPromocion);
    $('#txtSubTotal' + idTab).html('$ ' + formatNumber(subTotal));
    $("#txtSubTotal" + idTab).attr("subTotalReal", parseFloat(total).toFixed(2));

    var totalProductos = currentTab.products.reduce(function (total, producto) {
        if (producto.tipoVenta === 2) {
            return total + producto.quantity; // Sumar la cantidad
        } else if (producto.tipoVenta === 1) {
            return total + 1; // Sumar 1 sin importar la cantidad
        }
        return total;
    }, 0); // Inicializar total en 0

    // Actualiza el HTML con el total calculado
    $("#lblCantidadProductos" + idTab).html("Cantidad de Articulos: <strong> " + totalProductos + "</strong>");

    //$("#lblCantidadProductos" + idTab).html("Cantidad de Articulos: <strong> " + currentTab.products.length + "</strong>");
    $('#cboListaPrecios' + idTab).prop('disabled', currentTab.products.length > 0);
}

$(document).on("click", "button.btn-delete", async function () {

    const isValidCode = await validateCode("borrar producto de una venta");
    if (!isValidCode) {
        return false;
    }
    const currentTabId = $(this).data("idTab");
    const row = $(this).data("row");

    let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    currentTab.products.splice(row, 1);
    //localStorage.setItem('ventaActual', JSON.stringify(currentTab));
    deleteVentaFromIndexedDB(currentTab.idTab);

    showProducts_Prices(currentTabId, currentTab);

})

function cleanSaleParcial() {
    $('#cboTypeDocumentSaleParcial').val('');
    $('#cboFactura').val('');
    $('#txtPagaCon').val('');
    $('#txtVuelto').val('');

    $("#txtClienteParaFactura").val('')
    $('#txtClienteParaFactura').attr('cuil', '');
    $('#txtClienteParaFactura').attr('idCliente', '');
    document.getElementById("divClienteSeleccionado").style.display = 'none';
    $("#divVueltoEfectivo").hide();


    for (let i = 1; i < formaDePagoID + 1; i++) {
        const element = document.getElementById("nuevaFormaDePago" + i);
        if (element != null)
            element.remove();
    }

    formaDePagoID = 0;
}

$(document).on("click", "button.finalizarSaleParcial", function () {
    cleanSaleParcial();

    let currentTabId = $(this).attr("tabId");
    let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    if (currentTab.products.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    let clientId = $("#cboCliente" + currentTabId).val() != '' ? $("#cboCliente" + currentTabId).val() : null;

    if (clientId != null) {
        let nombreCliente = $("#cboCliente" + currentTabId)[0].innerText;

        swal({
            title: `Cuenta Corriente`,
            text: `\n ¿Desea agregar la venta a la cuenta corriente de ${nombreCliente}? \n`,
            type: "warning",
            showCancelButton: true,
            confirmButtonClass: "btn-info",
            confirmButtonText: "Si, Agregar",
            cancelButtonText: "No, Cancelar",
            closeOnConfirm: true,
            closeOnCancel: true
        },
            function (respuesta) {

                if (respuesta) {

                    registrationSale(currentTabId);
                }
            });

    }
    else {

        let total = $("#txtTotal" + currentTabId).attr("totalReal");
        calcularMinimoIdentificarConsumidor(total);

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
            $("#txtTotalParcial").val(parseFloat(subTotal).toFixed(2) + parseFloat(valorParcial).toFixed(2));
            calcularSuma();
        });

        $("#modalDividirPago").removeAttr("idTab");
        $("#modalDividirPago").attr("idTab", currentTabId);

        $("#modalDividirPago").modal("show")
    }
})

$('#modalDividirPago').on('shown.bs.modal', function () {
    $('#cboTypeDocumentSaleParcial').focus().click();

    // Remover cualquier evento previo y añadir el evento 'keydown' para #cboTypeDocumentSaleParcial
    $('#cboTypeDocumentSaleParcial').off('keydown').on('keydown', function (e) {
        if (e.key === 'Tab') {
            e.preventDefault();
            $('#btnFinalizarVentaParcial').focus();
        } else if (e.key === 'Enter') {
            e.preventDefault();
            $('#btnFinalizarVentaParcial').click();
        }
    });

    // Remover cualquier evento previo y añadir el evento 'keydown' para #btnFinalizarVentaParcial
    $('#btnFinalizarVentaParcial').off('keydown').on('keydown', function (e) {
        if (e.key === 'Enter') {
            e.preventDefault(); // Prevenir comportamiento por defecto
            $(this).click(); // Ejecutar el click del botón
        }
    });
});

$('#modalDividirPago').on('hidden.bs.modal', function (e) {

    $("#txtTotalParcial").attr("descuentoFormaPago", 0);

});

$(document).on("click", "button.btnAddFormaDePago", function () {
    formaDePagoID++;

    $('#formaDePagoPanel').append($('<div class="form-row" id="formaPago' + formaDePagoID + '"></div>'));

    let originalFormaPago = document.getElementById('formaDePagoPanel');
    let cloneFP = originalFormaPago.cloneNode(true);
    cloneFP.id = "nuevaFormaDePago" + formaDePagoID;
    cloneFP.querySelector("#txtTotalParcial").id = "txtTotalParcial" + formaDePagoID;
    cloneFP.querySelector("#cboTypeDocumentSaleParcial").id = "cboTypeDocumentSaleParcial" + formaDePagoID;
    cloneFP.querySelector("#btnAddNuevaFormaDePago").id = "btnAddNuevaFormaDePago" + formaDePagoID;
    cloneFP.querySelector("#cboFactura").id = "cboFactura" + formaDePagoID;

    $('#rowDividirPago').append(cloneFP);
    $("#txtTotalParcial" + formaDePagoID).val(0);
    $('#cboTypeDocumentSaleParcial' + formaDePagoID).val('');
    $('#cboFactura' + formaDePagoID).val('');

    $("#txtTotalParcial" + formaDePagoID).on("change", function (e) {
        calcularSuma();
    });

    $('#cboFactura' + formaDePagoID).change(function () {

        validateTipoFacturaAndMonto(formaDePagoID);
    })

    $('#txtTotalParcial' + formaDePagoID).change(function () {

        validateTipoFacturaAndMonto(formaDePagoID);
    })

    $('#cboTypeDocumentSaleParcial' + formaDePagoID).change(function () {
        let idFormaDePago = $(this).val();
        changeCboTypeDocumentSaleParcial(formaDePagoID, idFormaDePago);
    })

    $("#cboTypeDocumentSaleParcial" + formaDePagoID).trigger('change');

    return false;

})

function calcularSuma() {
    let subTotal = getSumaSubTotales();

    if (subTotal !== 0) {
        const texto = subTotal > 0 ? "FALTA" : "SOBRA";
        let subTotalText = Math.abs(subTotal);

        $("#txtSumaSubtotales").html(`${texto} <strong>$ ${subTotalText}</strong>`).show();
    } else {
        $("#txtSumaSubtotales").hide();
    }

    return subTotal;
}


function calcularMinimoIdentificarConsumidor(totFijo) {

    if (ajustes.minimoIdentificarConsumidor != '' && parseFloat(totFijo) > parseFloat(ajustes.minimoIdentificarConsumidor)) {
        $("#txtMinimoIdentificarConsumidor").html("El total supera el minimo para identificar al consumidor. Debe ingresar el CUIL / CUIT / DNI si va a facturarlo.");

        $("#txtMinimoIdentificarConsumidor").show();
    }
    else {
        $("#txtMinimoIdentificarConsumidor").hide();
    }
}

function getSumaSubTotales() {
    const inputElements = document.querySelectorAll('.inputSubtotal');
    let subTotal = 0;

    inputElements.forEach(function (input) {
        const value = parseFloat(input.value).toFixed(2);

        if (!isNaN(value) && value > 0) {
            subTotal += parseFloat(value);
        }
    });

    let totFijo = $("#txtTotalView").val();
    let descFormaPago = $("#txtTotalParcial").attr("descuentoFormaPago");

    return parseFloat(totFijo) - parseFloat(subTotal + Math.abs(descFormaPago));
}

function getVentaForRegister() {

    let currentTabId = getTabActiveId();

    let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    const vmDetailSale = currentTab.products;

    let total = $("#txtTotalParcial").val();

    //let descString = $("#txtDescRec" + currentTabId).attr('totalDescRec');
    //let descRec = parseFloat(descString != undefined && descString != null  ? descString : 0);

    let descProm = parseFloat($("#txtPromociones" + currentTabId).val() != null ? $("#txtPromociones" + currentTabId).val() : 0);
    let descFormaPago = parseFloat($("#txtTotalParcial").attr("descuentoFormaPago") != null ? $("#txtTotalParcial").attr("descuentoFormaPago") : 0);

    let sumDescuentos = (descFormaPago + descProm).toFixed(2);

    let formasDePago = [];

    if ($("#cboCliente" + currentTabId).val() == '') {

        $(".cboFormaDePago").each(function (index) {
            let subTotal = {
                total: parseFloat(index === 0 ? $("#txtTotalParcial").val() : $("#txtTotalParcial" + index).val()).toFixed(2),
                formaDePago: index === 0 ? $("#cboTypeDocumentSaleParcial").val() : $("#cboTypeDocumentSaleParcial" + index).val(),
                tipoFactura: index === 0 ? $("#cboFactura").val() : $("#cboFactura" + index).val()

            };
            formasDePago.push(subTotal);
        });
    }
    else {
        total = $("#txtSubTotal" + currentTabId).attr("subTotalReal");

        let subTotal = {
            total: parseFloat(total).toFixed(2),
            formaDePago: null
        };

        formasDePago.push(subTotal);
    }

    let cuilParaFactura = $('#txtClienteParaFactura').attr('cuil');
    let idClienteParaFactura = $('#txtClienteParaFactura').attr('idCliente');

    const sale = {
        idTypeDocumentSale: $("#cboTypeDocumentSaleParcial").val(),
        clientId: $("#cboCliente" + currentTabId).val() != '' ? $("#cboCliente" + currentTabId).val() : null,
        total: parseFloat(total).toFixed(2),
        detailSales: vmDetailSale,
        tipoMovimiento: $("#cboCliente" + currentTabId).val() != '' ? 2 : null,
        imprimirTicket: document.querySelector('#cboImprimirTicket').checked,
        multiplesFormaDePago: formasDePago != [] ? formasDePago : null,
        descuentorecargo: sumDescuentos != undefined && sumDescuentos != null ? sumDescuentos.replace('.', ',') : null,
        idClienteFactura: idClienteParaFactura != '' ? parseInt(idClienteParaFactura) : null,
        cuilFactura: cuilParaFactura != '' ? cuilParaFactura : null,
        observaciones: $("#txtObservaciones" + currentTabId).val()
    }

    return sale;
}

$("#btnFinalizarVentaParcial").off("click").on("click", function () {
    $("#btnFinalizarVentaParcial").closest("div.card-body").LoadingOverlay("show")

    if ($(".cboFormaDePago").filter(function () { return $(this).val() === "" || $(this).val() == null; }).length !== 0) {
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
    document.getElementById('cboImprimirTicket').checked = ajustes.imprimirDefault;

    let currentTabId = $("#modalDividirPago").attr("idtab");

    registrationSale(currentTabId);
});

function registrationSale(currentTabId) {
    showLoading();

    let sale = getVentaForRegister();

    fetch("/Sales/RegisterSale", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(sale)
    }).then(response => {

        return response.json();
    }).then(responseJson => {
        removeLoading();
        //localStorage.removeItem('ventaActual');
        deleteVentaFromIndexedDB(parseInt(currentTabId));

        if (responseJson.state) {

            if (responseJson.message != null && responseJson.message != '') {
                swal("Error al Facturar", "La venta fué registrada correctamente, pero no se ha podido facturar.\n", "warning");
                console.error("ERROR AL FACTURAR");
                console.error(responseJson.message);

            } else if (responseJson.object.errores != null && responseJson.object.errores != '') {
                swal("Error", responseJson.object.errores, "warning");
            }

            let nuevaVentaSpan = document.getElementById('profile-tab' + currentTabId).querySelector('span');
            if (nuevaVentaSpan != null) {
                nuevaVentaSpan.textContent = responseJson.object.tipoVenta + '-' + responseJson.object.saleNumber;
            }

            if (responseJson.object.idSale != null) {
                $("#btnImprimirTicket" + currentTabId).attr("idSale", responseJson.object.idSale);
            }
            else {
                let newIdSaleMultiple = responseJson.object.idSaleMultiple.slice(0, -1);  // Elimina el último carácter
                $("#btnImprimirTicket" + currentTabId).attr("idSalesMultiple", newIdSaleMultiple);
            }


            AllTabsForSale = AllTabsForSale.filter(p => p.idTab != currentTabId);
            cleanSaleParcial();

            if ($(".tab-venta").length > 2) {

                // para cerrar la ultima venta de 3
                let firstTabID = document.getElementsByClassName("tab-venta")[0].getAttribute("data-bs-target");

                if ($('#btnAgregarProducto' + firstTabID[firstTabID.length - 1]).is(':disabled')) { // si no esta cerrada la venta, no se cierra
                    $(firstTabID).remove();
                    document.getElementsByClassName("li-tab")[0].remove();
                }
            }
            disableAfterVenta(currentTabId);

            if (isHealthySale && sale.imprimirTicket && responseJson.object.nombreImpresora != null && responseJson.object.nombreImpresora != '' && responseJson.object.ticket != null && responseJson.object.ticket != '') {
                printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);
            }

            newTab();

        } else {
            swal("Lo sentimos", "La venta no fué registrada. Error: " + responseJson.message, "error");
        }
    });
}

function disableAfterVenta(tabID) {
    $('#btnAgregarProducto' + tabID).prop('disabled', true);
    $('#cboSearchProduct' + tabID).prop('disabled', true);
    //$('#cboDescRec' + tabID).prop('disabled', true);
    $('#txtPeso' + tabID).prop('disabled', true);
    $('#cboCliente' + tabID).prop('disabled', true);
    $('.delete-item-' + tabID).prop('disabled', true)
    $('#txtObservaciones' + tabID).prop('disabled', true);
    $('#btnFinalizeSaleParcial' + tabID).prop('disabled', true);
    $('#btnFinalizeSaleParcial' + tabID).hide()

    $('#btnImprimirTicket' + tabID).prop('hidden', false);
    $('#btnTicketEmail' + tabID).prop('hidden', false);
    $('#btnTicketPdf' + tabID).prop('hidden', false);
}

document.onkeyup = async function (e) {
    let currentTabId = getTabActiveId();

    if (e.altKey && e.which == 78) { // alt + N
        newTab();

    } else if (e.which == 113) { // F2
        $('#cboSearchProduct' + currentTabId).select2('close');
        $('#btnFinalizeSaleParcial' + currentTabId).click();

    } else if (e.which == 114) { // alt + F3
        registrarVentaEfectivo(currentTabId);

    } else if (e.which == 120) { // F9
        $('#cboSearchProduct' + currentTabId).select2('close');
        $("#modalConsultarPrecio").modal("show")

    } else if (e.altKey && e.which == 66) { // alt + B

        $('#cboSearchProduct' + currentTabId).val("").trigger('change');
        $('#cboSearchProduct' + currentTabId).select2('open');

    }
    return false;
};

$('#modalConsultarPrecio').on('hidden.bs.modal', function () {
    let currentTabId = getTabActiveId();
    $('#cboSearchProduct' + currentTabId).select2('open');
});

function registrarVentaEfectivo(currentTabId) {
    let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    if (currentTab.products.length < 1) {
        toastr.warning("", "Debe ingresar productos");
        return;
    }

    $('#cboSearchProduct' + currentTabId).select2('close');
    $('#cboTypeDocumentSaleParcial').val(1);
    $("#cboTypeDocumentSaleParcial").trigger('change');

    let total = $("#txtTotal" + currentTabId).attr("totalReal");
    $("#txtTotalParcial").val(total);

    registrationSale(currentTabId)
}

function resetModaConsultarPreciol() {
    $('#cboSearchProductConsultarPrecio').val(null).trigger('change');
    $('#txtPrecioConsultarPrecio').val('');
    $('#imgProductConsultarPrecio').attr('src', '');
    $('#lblProductName').text('');
}

function funConsultarPrecio() {

    $('#cboSearchProductConsultarPrecio').select2({
        ajax: {
            url: "/Sales/GetProductsVerPrecios",
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
                    results: data.map((item) => ({
                        id: item.idProduct,
                        text: item.description,
                        category: item.idCategory,
                        photoBase64: item.photoBase64,
                        price: item.price,
                        tipoVenta: item.tipoVenta,
                        comentario: item.comentario,
                        tags: item.tags
                    }))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResultsBuscarPrecio,
        allowClear: true,
        dropdownParent: $('#modalConsultarPrecio .modal-content')
    });

    $('#cboSearchProductConsultarPrecio').on('select2:select', function (e) {
        let data = e.params.data;
        productSelected = data;
        let tipoVentaString = data.tipoVenta == 1 ? "Kg" : "U.";

        // Asignar los valores de los campos de texto
        $('#txtComentariosConsultaProducto').val(data.comentario);
        $('#txtPrecioConsultarPrecio').val(`${data.price} / ${tipoVentaString}`);
        $('#lblProductName').text(data.text);
        $('#imgProductConsultarPrecio').attr('src', `data:image/png;base64,${data.photoBase64}`);

        // Si hay tags, agregar los tags como <span> en la tags-container
        if (data.tags && data.tags.length > 0) {
            let tagsContainer = $('#tags-container');  // Asegúrate de tener un contenedor con id="tags-container" o similar
            tagsContainer.empty();  // Limpiar cualquier tag previo

            // Agregar visualmente cada tag con su color y nombre
            data.tags.forEach(tag => {
                let tagElement = `<span class="status text-white" style="background-color: ${tag.color}; border: 1px solid ${tag.color}; padding: 3px 8px; border-radius: 5px; margin-right: 5px;">${tag.nombre}</span>`;
                tagsContainer.append(tagElement);
            });
        } else {
            $('#tags-container').empty();  // Limpiar si no hay tags
        }
    });

}


$('#btn-add-tab').click(function () {
    newTab();
});

$('#tab-list').on('click', '.close', async function () {
    const isValidCode = await validateCode("cerrar venta sin finalizar");
    if (!isValidCode) {
        return false;
    }

    let tabId = getTabActiveId();

    let tabIndex = $(this).parents('button').attr('data-bs-target');
    $(this).parents('li').remove();
    $(tabIndex).remove();

    //localStorage.removeItem('ventaActual');
    deleteVentaFromIndexedDB(parseInt(tabId));

    AllTabsForSale = AllTabsForSale.filter(p => p.idTab != tabId);

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


    let clone = originalTab.cloneNode(true);
    clone.id = "nuevaVenta" + tabID;
    clone.querySelector("#cboSearchProduct").id = "cboSearchProduct" + tabID;
    clone.querySelector("#tbProduct").id = "tbProduct" + tabID;
    clone.querySelector("#txtTotal").id = "txtTotal" + tabID;
    clone.querySelector("#btnFinalizeSaleParcial").id = "btnFinalizeSaleParcial" + tabID;
    clone.querySelector("#txtObservaciones").id = "txtObservaciones" + tabID;
    clone.querySelector("#cboCliente").id = "cboCliente" + tabID;
    clone.querySelector("#txtPeso").id = "txtPeso" + tabID;
    clone.querySelector("#btnAgregarProducto").id = "btnAgregarProducto" + tabID;
    clone.querySelector("#lblCantidadProductos").id = "lblCantidadProductos" + tabID;
    clone.querySelector("#lblFechaUsuario").id = "lblFechaUsuario" + tabID;
    clone.querySelector("#cboListaPrecios").id = "cboListaPrecios" + tabID;
    clone.querySelector("#btnMasCantidad").id = "btnMasCantidad" + tabID;
    clone.querySelector("#btnMenosCantidad").id = "btnMenosCantidad" + tabID;

    //clone.querySelector("#cboDescRec").id = "cboDescRec" + tabID;
    clone.querySelector("#txtSubTotal").id = "txtSubTotal" + tabID;
    clone.querySelector("#txtPromociones").id = "txtPromociones" + tabID;
    //clone.querySelector("#txtDescRec").id = "txtDescRec" + tabID;
    clone.querySelector("#btnImprimirTicket").id = "btnImprimirTicket" + tabID;
    clone.querySelector("#btnTicketEmail").id = "btnTicketEmail" + tabID;
    clone.querySelector("#btnTicketPdf").id = "btnTicketPdf" + tabID;

    $('#tab' + tabID).append(clone);

    if (ajustes != null)
        $('#cboListaPrecios' + tabID).val(ajustes.listaPrecios);

    $("#btnFinalizeSaleParcial" + tabID).attr("tabId", tabID);
    //$("#cboDescRec" + tabID).attr("tabId", tabID);

    let fecha = moment().tz('America/Argentina/Buenos_Aires').format('DD/MM/YYYY hh:mm');
    let userName = ajustes != null ? ajustes.user : '';

    let newTab = {
        idTab: tabID,
        user: userName,
        date: fecha,
        products: []
    }


    $("#lblFechaUsuario" + tabID).html(fecha + " &nbsp;&nbsp; " + userName);

    AllTabsForSale.push(newTab);

    addFunctions(tabID);
    lastTab();

    $('#cboSearchProduct' + tabID).val("").trigger('change');
    $('#cboSearchProduct' + tabID).select2('open');
}


function addFunctions(idTab) {

    $('#tbProduct' + idTab + ' tbody').on('click', 'tr', function () {

        if (selectedRowtbProduct) {
            selectedRowtbProduct.removeClass('selectedRow');
        }
        selectedRowtbProduct = $(this);
        selectedRowtbProduct.addClass('selectedRow');
    });

    $('#btnMasCantidad' + idTab).on('click', function () {
        adjustQuantity(idTab, 1);
    });

    $('#btnMenosCantidad' + idTab).on('click', function () {
        adjustQuantity(idTab, -1);
    });

    $('#btnImprimirTicket' + idTab).on("click", function () {
        let idSale = $("#btnImprimirTicket" + idTab).attr("idsale");

        if (idSale != null) {
            imprimirTicketFunction(`/Sales/PrintTicket?idSale=${idSale}`, false);
        } else {
            let idSalesMultiples = $("#btnImprimirTicket" + idTab).attr("idSalesMultiple");
            let saleNumbersArray = idSalesMultiples.split(",").map(Number);
            let queryString = saleNumbersArray.map(id => `idSales=${id}`).join("&");

            imprimirTicketFunction(`/Sales/PrintMultiplesTickets?${queryString}`, true);
        }
    });

    $('#btnTicketPdf' + idTab).on("click", function () {
        let idSale = $("#btnImprimirTicket" + idTab).attr("idsale");

        if (idSale != null) {
            generarPdfTicket(`/Sales/PdfTicket?idSale=${idSale}`);
        } else {
            let idSalesMultiples = $("#btnImprimirTicket" + idTab).attr("idSalesMultiple");
            let saleNumbersArray = idSalesMultiples.split(",").map(Number);
            let queryString = saleNumbersArray.map(id => `idSales=${id}`).join("&");

            generarPdfTicket(`/Sales/PdfMultiplesTickets?${queryString}`);
        }
    });

    $('#btnTicketEmail' + idTab).on("click", function () {
        $('#emailInput').val('');
        $('#emailModal').modal('show');
    });

    $('#tbProduct' + idTab + ' tbody').on('dblclick', 'tr', async function () {

        let rowIndex = $(this).index();
        let currentTab = AllTabsForSale.find(item => item.idTab == idTab);
        let productRow = currentTab.products.filter(prod => prod.row == rowIndex);

        if (!productRow[0].modificarPrecio)
            return;

        // Validar el código antes de continuar
        const isValidCode = await validateCode("cambio de precio");
        if (!isValidCode) {
            return false;
        }

        // Agregar un pequeño retraso para que el segundo swal tenga tiempo de abrirse correctamente
        setTimeout(async () => {
            // Mostrar el swal para el cambio de precio
            swal({
                title: 'Cambio de precio',
                text: productRow[0].descriptionproduct + ' ($' + productRow[0].price + ')',
                type: "input",
                showCancelButton: true,
                closeOnConfirm: false,
                inputPlaceholder: "ingrese el nuevo precio"
            }, async function (value) {


                if (value === false) return false;

                if (value === "") {
                    toastr.warning("", "debes ingresar el monto");
                    return false;
                }

                let nuevoPrecio = parseFloat(value).toFixed(0);

                if (isNaN(nuevoPrecio)) {
                    toastr.warning("", "debes ingresar un valor numérico");
                    return false
                }

                productRow[0].price = nuevoPrecio;
                productRow[0].total = (nuevoPrecio * parseFloat(productRow[0].quantity)).toFixed(2);

                showProducts_Prices(idTab, currentTab);

                swal.close();

            });
        }, 500);
    });


    $('#txtPeso' + idTab).on('keypress', function (e) {
        if ($('#txtPeso' + idTab).val() != '' && e.which === 13) { // 13 => enter

            $(this).attr("disabled", "disabled");

            agregarProductoEvento(idTab);
            //$('#txtPeso' + idTab).val('');

            $(this).removeAttr("disabled");
        }
    });

    //$('#cboDescRec' + idTab).change(function () {
    //    let currentTabId = $(this).attr("tabId");

    //    let total = $("#txtSubTotal" + currentTabId).attr("subTotalReal");

    //    if (total != '') {
    //        let descRecAplicar = $("#cboDescRec" + currentTabId).val()
    //        let desc = parseFloat(total * (descRecAplicar / 100));
    //        $('#txtDescRec' + idTab).html('$ ' + desc.toFixed(2));
    //        $("#txtDescRec" + idTab).attr('totalDescRec', desc.toFixed(2));

    //        $("#txtTotal" + currentTabId).val('$ ' + parseFloat(parseFloat(total) + desc).toFixed(2));

    //        $("#txtTotal" + currentTabId).attr("totalReal", parseFloat(parseFloat(total) + desc).toFixed(2));

    //    }
    //})

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
        let data = e.params.data;
    })


    let productAddedByBarcode = false;  // Flag para evitar la duplicación

    $('#cboSearchProduct' + idTab).off('select2:select');

    $('#cboSearchProduct' + idTab).select2({
        ajax: {
            url: "/Sales/GetProducts",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                lastSearchTerm = params.term;

                return {
                    search: params.term,
                    listaPrecios: $('#cboListaPrecios' + idTab).val()
                };
            },
            transport: function (params, success, failure) {
                // Verifica si el término de búsqueda ya está en la caché
                if (searchCache[params.data.search]) {
                    success(searchCache[params.data.search]); // Si está en la caché, usa los datos en caché
                } else {
                    // Si no está en la caché, realiza la solicitud AJAX
                    var $request = $.ajax(params);

                    $request
                        .done(function (data) {
                            searchCache[params.data.search] = data; // Almacena en la caché la respuesta
                            success(data); // Devuelve los resultados
                        })
                        .fail(failure);

                    return $request; // Retorna la solicitud AJAX
                }
            },
            processResults: function (data) {
                const results = data.map((item) => ({
                    id: item.idProduct,
                    text: item.description,
                    category: item.idCategory,
                    photoBase64: item.photoBase64,
                    price: item.price,
                    tipoVenta: item.tipoVenta,
                    iva: item.iva,
                    categoryProducty: item.categoryProducty,
                    modificarPrecio: item.modificarPrecio,
                    precioAlMomento: item.precioAlMomento,
                    excluirPromociones: item.excluirPromociones
                }));

                // Si es un código de barras, selecciona automáticamente el primer producto
                if (isBarcode(lastSearchTerm) && results.length > 0) {
                    const firstProduct = results[0];

                    // Marcar que se ha añadido un producto por código de barras
                    productAddedByBarcode = true;

                    // Seleccionar el primer producto
                    $('#cboSearchProduct' + idTab).val(firstProduct.id).trigger('change');

                    // Llamar manualmente al evento select2:select
                    $('#cboSearchProduct' + idTab).trigger({
                        type: 'select2:select',
                        params: {
                            data: firstProduct
                        }
                    });

                    // Limpiar el select2 después de agregar el producto
                    setTimeout(function () {
                        $('#cboSearchProduct' + idTab).val(null).trigger('change');
                    }, 100);
                }
                else {
                    productAddedByBarcode = false;
                }

                return {
                    results: results
                };
            },
            cache: true // La opción cache sigue habilitada, aunque usamos la nuestra
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResults
    });

    let se_agrego_antes = false;

    // Evento para manejar la selección del producto
    $('#cboSearchProduct' + idTab).on('select2:select', function (e) {

        let currentTab = AllTabsForSale.find(item => item.idTab == idTab);

        let searchTerm = lastSearchTerm;
        let data = e.params.data;
        productSelected = data;

        if (productSelected.precioAlMomento) {

            swal({
                title: 'Ingrese el precio',
                text: '',
                type: "input",
                showCancelButton: true,
                closeOnConfirm: true,
                inputPlaceholder: "$"
            }, async function (value) {

                if (value === "") {
                    toastr.warning("", "debes ingresar el monto");
                    return false;
                }

                let nuevoPrecio = parseFloat(value).toFixed(0);

                if (isNaN(nuevoPrecio)) {
                    toastr.warning("", "debes ingresar un valor numérico");
                    return false
                }

                data.price = nuevoPrecio;
                data.total = nuevoPrecio;
                setNewProduct(1, 0, data, currentTab, idTab);

                swal.close();

            });
            return;
        }


        if (productAddedByBarcode) {
            let quantity_product_found_codBar = 0;
            let product_found = currentTab.products.filter(prod => prod.idproduct == data.id && prod.promocion == null);

            if (product_found.length == 1) {
                quantity_product_found_codBar = product_found[0].quantity;
                currentTab.products.splice(product_found[0].row, 1);
            }

            let peso = $("#txtPeso" + idTab).val();
            peso = peso == "" ? 1 : parseFloat(peso);
            peso += quantity_product_found_codBar;
            $("#txtPeso" + idTab).val(peso);

            agregarProductoEvento(idTab);

            productAddedByBarcode = false; // Resetear el flag
            se_agrego_antes = true;

            // Limpiar y reabrir el select2
            setTimeout(() => {
                $('#cboSearchProduct' + idTab).select2('close');
                $('#cboSearchProduct' + idTab).val("").trigger('change');
                $('#cboSearchProduct' + idTab).select2('open');
            }, 0);

            return;
        }
        else if (se_agrego_antes) {
            se_agrego_antes = false;
            return;
        }


        let quantity_product_found = 0;

        if (currentTab.products.length !== 0) {
            let product_found = currentTab.products.filter(prod => prod.idproduct == data.id &&
                prod.promocion == null &&
                data.tipoVenta == 2 &&
                isBarcode(searchTerm));

            if (product_found.length == 1) {
                quantity_product_found = product_found[0].quantity;
                currentTab.products.splice(product_found[0].row, 1);
            }
        }

        if (data.tipoVenta == 1) {
            $('#txtPeso' + idTab).val('');
        } else {
            $('#txtPeso' + idTab).val('1');
        }

        const element = document.getElementById("txtPeso" + idTab);
        window.setTimeout(() => {
            element.focus();
            element.select();
        }, 0);
    });

    function isBarcode(input) {
        return input.length >= 8 && !isNaN(input);
    }

    $("#btnAgregarProducto" + idTab).on("click", function () {
        agregarProductoEvento(idTab);
    })

}

function adjustQuantity(idTab, increment) {
    if (selectedRowtbProduct) {
        let cantidadCell = selectedRowtbProduct.find('.cantidad');
        let cantidad = parseFloat(cantidadCell.text());
        let newCantidad = cantidad + increment;

        if (newCantidad > 0) {
            cantidadCell.text(newCantidad);

            let row = selectedRowtbProduct.find('.btn-delete').data('row');

            let currentTab = AllTabsForSale.find(item => item.idTab == idTab);
            let productRow = currentTab.products.filter(prod => prod.row == row);

            productRow[0].quantity = newCantidad;
            productRow[0].total = (parseFloat(productRow[0].price) * newCantidad).toFixed(2);

            showProducts_Prices(idTab, currentTab);
        }
    }
}

function agregarProductoEvento(idTab) {
    let peso = parseFloat($("#txtPeso" + idTab).val());

    if (peso === false || peso === "" || isNaN(peso)) return false;

    if (productSelected === null) {
        return false;
    }

    if (productSelected.tipoVenta == 2 && !Number.isInteger(peso)) {
        toastr.warning('No se puede agregar DECIMALES a este producto', "");
        $("#txtPeso" + idTab).val('')
        return;
    }

    let quantity_product_found = 0;
    let currentTab = AllTabsForSale.find(item => item.idTab == idTab);

    if (currentTab.products.length !== 0) {

        let product_found = currentTab.products.filter(prod => prod.idproduct == productSelected.id)
        if (product_found.length == 1) {

            quantity_product_found = product_found[0].quantity;
            currentTab.products.splice(product_found[0].row, 1);
        }
    }

    setNewProduct(peso, quantity_product_found, productSelected, currentTab, idTab);
}

function setNewProduct(cant, quantity_product_found, data, currentTab, idTab) {
    let totalQuantity = parseFloat(cant) + parseFloat(quantity_product_found);
    data.total = totalQuantity * parseFloat(data.price);
    data.quantity = Math.trunc(totalQuantity * 10000) / 10000;

    //let currentTab = AllTabsForSale.find(item => item.idTab == idTab);
    if (!data.excluirPromociones) {
        data = applyPromociones(totalQuantity, data, currentTab);
    }

    let product = new Producto();
    product.idproduct = data.id;
    product.descriptionproduct = data.text;
    product.quantity = data.quantity;
    product.price = formatNumber(data.price);
    product.total = formatNumber(data.total);
    product.tipoVenta = data.tipoVenta;
    product.iva = data.iva;
    product.categoryProducty = data.categoryProducty;
    product.modificarPrecio = data.modificarPrecio;
    product.precioAlMomento = data.precioAlMomento;
    product.excluirPromociones = data.excluirPromociones;

    if (data.promocion) {
        product.promocion = data.promocion;
        product.diferenciapromocion = data.diferenciapromocion.toFixed(2).toString();
    }

    currentTab.products.push(product);

    showProducts_Prices(idTab, currentTab);
    $('#cboSearchProduct' + idTab).val("").trigger('change');
    $('#cboSearchProduct' + idTab).select2('open');
    $('#txtPeso' + idTab).val('1');


    //localStorage.setItem('ventaActual', JSON.stringify(currentTab));

    saveVentaToIndexedDB(currentTab);
}

function formatNumber(num) {
    // Si el número es una cadena con coma como separador decimal, reemplazarla por punto
    if (typeof num === 'string') {
        num = parseFloat(num.replace(',', '.'));
    }

    // Verificar si el número es un entero
    if (Number.isInteger(num)) {
        return num.toString();
    }
    // Si no es un entero, truncar a 2 decimales
    return num.toFixed(2);
}

function lastTab() {
    let tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}

function ventaAbierta() {
    return AllTabsForSale.some(tab => tab.products.length > 0);
}

function getTabActiveId() {
    let idTab = document.getElementsByClassName(" nav-link tab-venta active")[0].id;

    return idTab[idTab.length - 1];
}

async function validateCode(detalle) {
    if (!ajustes.needControl) return true;

    return new Promise((resolve, reject) => {
        swal({
            title: 'Codigo de Seguridad',
            type: "input",
            showCancelButton: true,
            closeOnConfirm: false, // Evitar que se cierre automáticamente
            inputPlaceholder: "password..."
        }, async function (value) {
            if (value === false) {
                resolve(false);
                return;
            }

            try {
                const response = await fetch(`/Ajustes/ValidateSecurityCode?encryptedCode=${value}&detalle=${detalle}`, {
                    method: 'POST'
                });
                const data = await response.json();

                if (data.state) {
                    resolve(data.object);
                } else {
                    resolve(false);
                }
            } catch (error) {
                reject(false);
            } finally {
                // Cerrar swal manualmente después de la validación, pase lo que pase
                swal.close();
            }
        });
    });
}

function imprimirTicketFunction(url, isMultiple) {
    showLoading();

    fetch(url)
        .then(response => response.json())
        .then(response => {
            removeLoading();
            if (isHealthySale &&
                response.object.nombreImpresora != null &&
                response.object.nombreImpresora != '' &&
                response.object.ticket != null &&
                response.object.ticket != '') {
                printTicket(response.object.ticket, response.object.nombreImpresora);
            }

            //swal("Exitoso!", "Ticket impreso!", "success");
        });
}

function generarPdfTicket(url) {
    showLoading();

    fetch(url, {
        method: 'GET'
    }).then(response => {
        if (response.ok) {
            return response.blob();
        }
        removeLoading();
        throw new Error('Error al generar el PDF.');
    }).then(blob => {

        removeLoading();
        const url = window.URL.createObjectURL(blob);

        window.open(url, '_blank');
    }).catch(error => {
        removeLoading();
        swal("Error", error.message, "error");
    });
}

$('#sendEmailBtn').on("click", function () {
    let idTab = getTabActiveId();
    let email = $('#emailInput').val();
    let idSale = $("#btnImprimirTicket" + idTab).attr("idsale");

    if (email && idSale) {
        showLoading();

        fetch(`/Sales/EnviarTicketEmail`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                email: email,
                idSale: idSale
            })
        }).then(response => {
            removeLoading();
            if (response.ok) {
                swal("Exitoso!", "Ticket enviado por correo!", "success");
            } else {
                throw new Error('Error al enviar el correo.');
            }
        }).catch(error => {
            swal("Error", error.message, "error");
        });

        $('#emailModal').modal('hide');
    } else {
        swal("Error", "Debe ingresar un email válido", "error");
    }
});

function initializeDatabase(version = 1) {
    return new Promise((resolve, reject) => {
        if (dbInstance) {
            resolve(dbInstance);
            return;
        }

        const request = indexedDB.open('VentasDB', version);

        request.onupgradeneeded = function (event) {
            const db = event.target.result;
            if (!db.objectStoreNames.contains('ventas')) {
                db.createObjectStore('ventas', { keyPath: 'idTab' });
            }
        };

        request.onsuccess = function (event) {
            dbInstance = event.target.result;
            resolve(dbInstance);
        };

        request.onerror = function (event) {
            reject(event.target.error);
        };
    });
}


function saveVentaToIndexedDB(venta) {
    initializeDatabase()
        .then((db) => {
            const transaction = db.transaction('ventas', 'readwrite');
            const store = transaction.objectStore('ventas');
            store.put(venta);

            transaction.oncomplete = function () {
            };

            transaction.onerror = function (event) {
            };
        })
        .catch((error) => {
            console.error('Error al inicializar la base de datos para guardar:', error);
        });
}

function getAllVentasFromIndexedDB(callback) {
    initializeDatabase()
        .then((db) => {
            const transaction = db.transaction('ventas', 'readonly');
            const store = transaction.objectStore('ventas');
            const request = store.getAll(); // Recupera todas las ventas

            request.onsuccess = function () {
                if (request.result && request.result.length > 0) {
                    callback(request.result); // Devuelve todas las ventas
                } else {
                    callback([]); // Devuelve un array vacío si no hay ventas
                }
            };

            request.onerror = function (event) {
                console.error('Error al recuperar las ventas:', event.target.error);
            };
        })
        .catch((error) => {
            console.error('Error al inicializar la base de datos para recuperar:', error);
        });
}


function deleteVentaFromIndexedDB(idTab) {
    if (idTab === undefined || idTab === null || idTab === '') {
        console.error('Error: idTab no es válido. Debe ser un valor definido.');
        return;
    }

    initializeDatabase()
        .then((db) => {
            const transaction = db.transaction('ventas', 'readwrite');
            const store = transaction.objectStore('ventas');

            const request = store.delete(idTab);

            request.onsuccess = function () {
            };

            request.onerror = function (event) {
                console.error('Error al eliminar la venta:', event.target.error);
            };
        })
        .catch((error) => {
            console.error('Error al inicializar la base de datos para eliminar:', error);
        });
}


class Producto {
    idproduct = 0;
    descriptionproduct = "";
    quantity = 0;
    price = 0;
    total = 0;
    promocion = null;
    diferenciapromocion = null;
    tipoVenta = 0;
    categoryProducty = "";
    iva = 21;
    modificarPrecio = 1;
    precioAlMomento = 0;
    excluirPromociones = 0;
}