let originalTab = document.getElementById('nuevaVenta');
let AllTabsForSale = [];
let buttonCerrarTab = '<button class="close" type="button" title="Cerrar tab">×</button>';
let tabID = 0;
let formaDePagoID = 0;
let promociones = [];
let productSelected = null;
let formasDePagosList = [];
let isHealthy = false;
let ajustes = null;

const BASIC_MODEL_CLIENTE_SALE = {
    idCliente: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    condicionIva: null,
    isActive: true
}

const ProducstTab = {
    idTab: 0,
    products: []
}

$(document).ready(function () {

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

    fetch("/Admin/GetPromocionesActivas")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                promociones = responseJson.object;

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        });

    fetch("/Admin/GetAjustesVentas")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                ajustes = responseJson.object;

                document.getElementById('cboImprimirTicket').checked = ajustes.imprimirDefault;

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    newTab();
    healthcheck();
    inicializarClientesFactura();
    inicializarConsultarPrecios();

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
function inicializarClientesFactura() {
    let isNuevoCliente = $('#switchNuevoCliente').is(':checked');
    toggleFields(isNuevoCliente);
    $('#switchNuevoCliente').change(function () {
        let isNuevoCliente = $(this).is(':checked');
        toggleFields(isNuevoCliente);
        resetModalClientesFactura();
    });

    $('[data-bs-toggle="popover"]').popover({
        html: true
    });

    $('#modalDatosFactura').on('shown.bs.modal', function () {
        if (!$('#cboClienteFactura').data('select2')) {
            funClientesFactura();
        }
        setTimeout(function () {
            $('#cboClienteFactura').select2('open');
        }, 100);
    });

    $('#modalDatosFactura').on('hidden.bs.modal', function () {
        resetModalClientesFactura();
    });
}
function funClientesFactura() {
    $('#cboClienteFactura').select2({
        ajax: {
            url: "/Sales/GetClientesByFacturar",
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
                            cuil: item.cuil,
                            telefono: item.telefono,
                            direccion: item.direccion,
                            condicionIva: item.condicionIva,
                            comentario: item.comentario,
                            color: '',
                            total: ''
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando cliente...',
        minimumInputLength: 3,
        templateResult: formatResultsClients,
        allowClear: true,
        dropdownParent: $('#modalDatosFactura .modal-content')
    });

    $('#cboClienteFactura').on('select2:select', function (e) {
        let data = e.params.data;
        productSelected = data;
        $('#txtNombre').val(data.text);
        $('#txtCuil').val(data.cuil);
        $('#txtIdCuilFactura').val(data.cuil);
        $('#txtTelefono').val(data.telefono);
        $('#txtDireccion').val(data.direccion);
        $('#cboCondicionIva').val(data.condicionIva);
        $('#txtComentario').val(data.comentario);
        $('#txtIdClienteFactura').val(data.id);
    });

    $('#txtCuil').on('input', function () {
        if ($('#cboCondicionIva').val() == '4') { // Resp. Inscripto
            let formattedValue = formatCuil($(this).val());
            $(this).val(formattedValue);
        }
    });

    $('#txtCuil').on('keypress', function (e) {
        let charCode = (e.which) ? e.which : e.keyCode;
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
        }
    });
}

$("#cboCondicionIva").on("change", function () {
    $('#txtCuil').val('');
});
function formatCuil(value) {
    value = value.replace(/\D/g, '');
    let formattedValue = '';

    if (value.length > 0) formattedValue += value.substring(0, 2);
    if (value.length > 2) formattedValue += '-' + value.substring(2, 10);
    if (value.length > 10) formattedValue += '-' + value.substring(10, 11);

    return formattedValue;
}

function removeHyphens(value) {
    return value.replace(/-/g, '');
}

function validateCuilCuit() {
    let cuilValue = $('#txtCuil').val().replace(/\D/g, '');
    return cuilValue.length == 11;
}

function validateDni() {
    let dniValue = $('#txtCuil').val().replace(/\D/g, '');
    return dniValue.length == 8;
}
function resetModalClientesFactura() {
    $('#cboClienteFactura').val(null).trigger('change');
    $('#txtNombre').val('');
    $('#txtCuil').val('');
    $('#txtTelefono').val('');
    $('#txtDireccion').val('');
    $('#cboCondicionIva').val('');
    $('#txtComentario').val('');
    $('#txtIdClienteFactura').val(0);
}
function toggleFields(isNuevoCliente) {
    $('#txtNombre, #txtCuil, #cboCondicionIva, #txtTelefono, #txtDireccion, #txtComentario').prop('disabled', !isNuevoCliente);
    $('#cboClienteFactura').prop('disabled', isNuevoCliente);

    if (isNuevoCliente) {
        $('#switchGuardarNuevoClienteDiv').css('display', 'block');
    } else {
        $('#switchGuardarNuevoClienteDiv').css('display', 'none');
        $('#switchGuardarNuevoCliente').prop('checked', false);
    }
}

$("#btnBuscarCliente").on("click", function () {
    $("#modalDatosFactura").modal("show");
});
$("#btnSeleccionarClienteFactura").on("click", function () {

    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    let isValid = true;
    let cuilInput = $('#txtCuil').val();

    if ($('#cboCondicionIva').val() == '4') { // Resp. Inscripto
        isValid = validateCuilCuit();
        cuilInput = removeHyphens(cuilInput);
    } else if ($('#cboCondicionIva').val() == '1') { // Consumidor Final
        isValid = validateDni();
    }

    if (!isValid) {
        const msg = `El campo CUIL / CUIT / DNI debe ser completado de forma corecta, dependiendo de la condicion del IVA`;
        toastr.warning(msg, "");
        return;
    }

    let nuevoCliente = document.getElementById("switchNuevoCliente").checked
    let guardarNuevouevoCliente = document.getElementById("switchGuardarNuevoCliente").checked

    const model = structuredClone(BASIC_MODEL_CLIENTE_SALE);
    model["nombre"] = $("#txtNombre").val();
    model["direccion"] = $("#txtDireccion").val();
    model["cuil"] = $("#txtCuil").val();
    model["telefono"] = $("#txtTelefono").val();
    model["condicionIva"] = $("#cboCondicionIva").val();
    model["idCliente"] = $("#txtIdClienteFactura").val();

    if (nuevoCliente) {

        if (guardarNuevouevoCliente) {

            fetch("/Admin/CreateCliente", {
                method: "POST",
                headers: { 'Content-Type': 'application/json;charset=utf-8' },
                body: JSON.stringify(model)
            }).then(response => {
                return response.json();
            }).then(responseJson => {

                if (responseJson.state) {

                    model["idCliente"] = responseJson.object.idCliente;

                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {

            })
        }
    }

    $('#txtClienteParaFactura').val(`${model.nombre}  (CUIT: ${model.cuil})`);
    $('#txtClienteParaFactura').attr('cuil', model.cuil);
    $('#txtClienteParaFactura').attr('idCliente', model.idCliente);

    $('#btnFinalizarVentaParcial').prop('disabled', false);
    $("#txtMinimoIdentificarConsumidor").toggle(false);

    $("#modalDatosFactura").modal("hide")
});
async function healthcheck() {
    isHealthy = await getHealthcheck();

    if (isHealthy) {
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
            updateUIFormaDePago(false, false, false);
        }
    } else {
        // Otros casos
        updateUIFormaDePago(false, false, false);
    }

    $('#btnFinalizarVentaParcial').prop('disabled', $('#cboTypeDocumentSaleParcial' + idLineaFormaPago).val() == null ? true : false);
}
function updateUIFormaDePago(disableButton, showMinimo, showCliente) {
    $('#btnFinalizarVentaParcial').prop('disabled', disableButton);
    $("#txtMinimoIdentificarConsumidor").toggle(showMinimo);
    document.getElementById("divClienteSeleccionado").style.display = showCliente ? '' : 'none';
}

function formatResults(data) {

    if (data.loading)
        return data.text;

    let container = $(
        `<table width="90%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <p>$ ${formatNumber(data.price)}</p>
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

    let container = $(
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
    $('#txtSubTotal' + idTab).html('$ ' + formatNumber(subTotal));
    $("#txtSubTotal" + idTab).attr("subTotalReal", parseFloat(total).toFixed(2));

    $("#lblCantidadProductos" + idTab).html("Cantidad de Articulos: <strong> " + currentTab.products.length + "</strong>");
}

$(document).on("click", "button.btn-delete", async function () {

    if (!(await validateCode())) { return false; }

    const currentTabId = $(this).data("idTab");
        const row = $(this).data("row");

        let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

        currentTab.products.splice(row, 1);

        showProducts_Prices(currentTabId, currentTab);

})

function cleanSaleParcial() {
    $('#cboTypeDocumentSaleParcial').val('');
    $('#cboFactura').val('');

    $('#txtClienteParaFactura').attr('cuil', '');
    $('#txtClienteParaFactura').attr('idCliente', '');
    document.getElementById("divClienteSeleccionado").style.display = 'none';


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
            title: `¿Desea agregar la venta a la cuenta corriente de ${nombreCliente}?`,
            text: "",
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

    $("#txtTotalParcial" + formaDePagoID).on("change", function (e) {
        calcularSuma();
    });

    $('#cboFactura' + formaDePagoID).change(function () {

        validateTipoFacturaAndMonto('');
    })

    $('#txtTotalParcial' + formaDePagoID).change(function () {

        validateTipoFacturaAndMonto('');
    })

    $('#cboTypeDocumentSaleParcial' + formaDePagoID).change(function () {
        let idFormaDePago = $(this).val();
        changeCboTypeDocumentSaleParcial(formaDePagoID, idFormaDePago);
    })

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

    return parseFloat(totFijo) - parseFloat(subTotal);
}

function getVentaForRegister() {

    let currentTabId = getTabActiveId();

    let currentTab = AllTabsForSale.find(item => item.idTab == currentTabId);

    const vmDetailSale = currentTab.products;


    let descRec = $("#txtDescRec" + currentTabId).attr('totalDescRec');
    let total = $("#txtTotalParcial").val();

    let formasDePago = [];

    if ($("#cboCliente" + currentTabId).val() == '') {

        $(".cboFormaDePago").each(function (index) {
            let subTotal = {
                total: parseFloat(index === 0 ? $("#txtTotalParcial").val() : $("#txtTotalParcial" + index).val()).toFixed(2),
                formaDePago: index === 0 ? $("#cboTypeDocumentSaleParcial").val() : $("#cboTypeDocumentSaleParcial" + index).val()
            };
            formasDePago.push(subTotal);
        });
    }
    else {
        total = $("#txtSubTotal" + currentTabId).attr("subTotalReal");
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
        descuentorecargo: descRec != undefined ? descRec.replace('.', ',') : null,
        idClienteFactura: idClienteParaFactura != '' ? parseInt(idClienteParaFactura) : null,
        cuilFactura: cuilParaFactura != '' ? cuilParaFactura : null
    }

    return sale;
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

        if (responseJson.state) {

            let nuevaVentaSpan = document.getElementById('profile-tab' + currentTabId).querySelector('span');
            if (nuevaVentaSpan != null) {
                nuevaVentaSpan.textContent = responseJson.object.saleNumber;
            }

            $("#btnImprimirTicket" + currentTabId).attr("idSale", responseJson.object.idSale);


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

            if (isHealthy && sale.imprimirTicket && responseJson.object.nombreImpresora != null && responseJson.object.nombreImpresora != '' && responseJson.object.ticket != null && responseJson.object.ticket != '') {
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
    $('#cboDescRec' + tabID).prop('disabled', true);
    $('#txtPeso' + tabID).prop('disabled', true);
    $('#cboCliente' + tabID).prop('disabled', true);
    $('.delete-item-' + tabID).prop('disabled', true)
    $('#btnFinalizeSaleParcial' + tabID).prop('disabled', true);
    $('#btnFinalizeSaleParcial' + tabID).hide()
    $('#btnImprimirTicket' + tabID).prop('hidden', false);
}

document.onkeyup = async function (e) {
    if (e.altKey && e.which == 78) { // alt + N
        newTab();
        return false;

    } else if (e.which == 113) { // F2
        let id = getTabActiveId();
        $('#cboSearchProduct' + id).select2('close');
        $('#btnFinalizeSaleParcial' + id).click();
        return false;
    } else if (e.which == 120) { // F9
        $("#modalConsultarPrecio").modal("show")
        return false;
    }
};

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
                        tipoVenta: item.tipoVenta
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
        $('#txtPrecioConsultarPrecio').val(data.price);
        $('#lblProductName').text(data.text);
        $('#imgProductConsultarPrecio').attr('src', `data:image/png;base64,${data.photoBase64}`);
    });
}


$('#btn-add-tab').click(function () {
    newTab();
});

$('#tab-list').on('click', '.close',async function () {
    if (!(await validateCode())) { return false; }

    let tabID = $(this).parents('button').attr('data-bs-target');
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


    let clone = originalTab.cloneNode(true);
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
    clone.querySelector("#btnImprimirTicket").id = "btnImprimirTicket" + tabID;

    $('#tab' + tabID).append(clone);

    $("#btnFinalizeSaleParcial" + tabID).attr("tabId", tabID);
    $("#cboDescRec" + tabID).attr("tabId", tabID);

    let newTab = {
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

    $('#btnImprimirTicket' + idTab).on("click", function () {
        let idSale = $("#btnImprimirTicket" + idTab).attr("idsale");

        fetch(`/Sales/PrintTicket?idSale=${idSale}`
        ).then(response => {

            return response.json();
        }).then(response => {
            $("#modalData").modal("hide");
            printTicket(response.object.ticket, response.object.nombreImpresora);

            swal("Exitoso!", "Ticket impreso!", "success");

        });
    });

    $('#tbProduct' + idTab + ' tbody').on('dblclick', 'tr', async function () {
        let rowIndex = $(this).index();

        let currentTab = AllTabsForSale.find(item => item.idTab == idTab);
        let productRow = currentTab.products.filter(prod => prod.row == rowIndex);

        swal({
            title: 'Cambio de precio',
            text: productRow[0].descriptionproduct + ' ($' + productRow[0].price + ')',
            type: "input",
            showCancelButton: true,
            closeOnConfirm: false,
            inputPlaceholder: "ingrese el nuevo precio"
        },async function (value) {

            if (!(await validateCode())) { return false; }
            
            if (value === false) return false;

            if (value === "") {
                toastr.warning("", "debes ingresar el monto");
                return false;
            }

            let nuevoPrecio = parseFloat(value).toFixed(2);

            if (isNaN(nuevoPrecio)) {
                toastr.warning("", "debes ingresar un valor numérico");
                return false
            }

            productRow[0].price = nuevoPrecio;
            productRow[0].total = (nuevoPrecio * parseFloat(productRow[0].quantity)).toFixed(2);

            showProducts_Prices(idTab, currentTab);

            swal.close();

        });
    });

    $('#txtPeso' + idTab).on('keypress', function (e) {
        if ($('#txtPeso' + idTab).val() != '' && e.which === 13) {

            $(this).attr("disabled", "disabled");

            agregarProductoEvento(idTab);
            $('#txtPeso' + idTab).val('');

            $(this).removeAttr("disabled");
        }
    });

    $('#cboDescRec' + idTab).change(function () {
        let currentTabId = $(this).attr("tabId");

        let total = $("#txtSubTotal" + currentTabId).attr("subTotalReal");

        if (total != '') {
            let descRecAplicar = $("#cboDescRec" + currentTabId).val()
            let desc = parseFloat(total * (descRecAplicar / 100));
            $('#txtDescRec' + idTab).html('$ ' + desc.toFixed(2));
            $("#txtDescRec" + idTab).attr('totalDescRec', desc.toFixed(2));

            $("#txtTotal" + currentTabId).val('$ ' + parseFloat(parseFloat(total) + desc).toFixed(2));

            $("#txtTotal" + currentTabId).attr("totalReal", parseFloat(parseFloat(total) + desc).toFixed(2));

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
        let data = e.params.data;
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
                            category: item.idCategory,
                            photoBase64: item.photoBase64,
                            price: item.price,
                            tipoVenta: item.tipoVenta,
                            categoryProducty: item.categoryProducty
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 3,
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
            let peso = $("#txtPeso" + idTab).val();

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

    })

    $("#btnAgregarProducto" + idTab).on("click", function () {
        agregarProductoEvento(idTab);
    })

}

function agregarProductoEvento(idTab) {
    let peso = parseFloat($("#txtPeso" + idTab).val());

    if (peso === false || peso === "" || isNaN(peso)) return false;

    //let data = $("#cboSearchProduct" + idTab).select2('data')[0];

    if (productSelected === null) {
        return false;
    }

    let quantity_product_found = 0;
    let currentTab = AllTabsForSale.find(item => item.idTab == idTab);

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
    data.total = totalQuantity * parseFloat(data.price);
    data.quantity = Math.trunc(totalQuantity * 10000) / 10000;

    data = applyPromociones(totalQuantity, data, currentTab);

    let product = new Producto();
    product.idproduct = data.id;
    product.descriptionproduct = data.text;
    product.quantity = data.quantity;
    product.price = formatNumber(data.price);
    product.total = formatNumber(data.total);
    product.tipoVenta = data.tipoVenta;
    product.categoryProducty = data.categoryProducty;

    if (data.promocion) {
        product.promocion = data.promocion;
        product.diferenciapromocion = data.diferenciapromocion.toFixed(2).toString();
    }

    currentTab.products.push(product);

    showProducts_Prices(idTab, currentTab);
    $('#cboSearchProduct' + idTab).val("").trigger('change');
    $('#cboSearchProduct' + idTab).select2('open');
    $('#txtPeso' + idTab).val('');
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

function applyPromociones(totalQuantity, data, currentTab) {
    let currentdate = new Date();
    let today = currentdate.getDay().toString();

    let promPorDia = promociones.find(item => item.dias.includes(today) && !item.idProducto && item.idCategory.length === 0);
    let promPorCat = promociones.find(item => item.idCategory.includes(data.category) && !item.idProducto && (!item.dias.length || item.dias.includes(today)));
    let promPorProducto = promociones.find(item => parseInt(item.idProducto) === data.id);

    if (promPorProducto) {
        data = applyForProduct(promPorProducto, totalQuantity, data, currentTab);
    } else if (promPorCat) {
        data = calcularPrecioPorcentaje(data, promPorCat, totalQuantity);
        data.promocion = promPorCat.nombre;
    } else if (promPorDia) {
        data = calcularPrecioPorcentaje(data, promPorDia, totalQuantity);
        data.promocion = promPorDia.nombre;
    }

    return data;
}

function containsCategoria(cat, catProducto) {
    return cat.includes(catProducto);
}

function containsDias(dias) {
    let today = new Date().getDay().toString();
    return dias.includes(today);
}

function applyForProduct(prom, totalQuantity, data, currentTab) {
    let apply = false;

    if (prom.operador === null && prom.precio !== null) {
        data = calcularPrecioPorcentaje(data, prom, totalQuantity);
        apply = true;
    } else if (prom.operador !== null) {
        switch (prom.operador) {
            case 0:
                if (totalQuantity < prom.cantidadProducto) return data;

                let diffDividido = totalQuantity % prom.cantidadProducto;

                if (diffDividido === 0) {
                    data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                    apply = true;
                } else if (diffDividido > 0) {
                    let precio = parseFloat(data.price);
                    if (isNaN(precio)) return data;

                    let newProd = new Producto();
                    newProd.idproduct = data.id;
                    newProd.descriptionproduct = data.text;
                    newProd.categoryProducty = data.categoryProducty;
                    newProd.quantity = diffDividido;
                    newProd.price = precio.toFixed(2).toString();
                    newProd.total = (precio * diffDividido).toFixed(2).toString();

                    currentTab.products.push(newProd);

                    let difCant = totalQuantity - diffDividido;
                    data = calcularPrecioPorcentaje(data, prom, difCant);
                    apply = true;
                }
                break;

            case 1:
                if (totalQuantity > prom.cantidadProducto) {
                    data = calcularPrecioPorcentaje(data, prom, totalQuantity);
                    apply = true;
                }
                break;
        }
    }

    if (apply) data.promocion = prom.nombre;

    return data;
}

function calcularPrecioPorcentaje(data, prom, totalQuantity) {
    let precio = prom.precio !== null ? prom.precio : parseFloat(data.price) * (1 - (prom.porcentaje / 100));

    data.diferenciapromocion = (parseFloat(data.price) * totalQuantity) - (precio * totalQuantity);
    data.total = precio * totalQuantity;
    data.price = precio;
    data.quantity = totalQuantity;

    return data;
}

function lastTab() {
    let tabFirst = $('#tab-list button:last');
    tabFirst.tab('show');
}

//$(document).on('click', 'a[href]:not([target="_blank"])', function (event) {
//    if (AllTabsForSale[0].products.length > 0) {
//        event.preventDefault();
//        registerNoCierreSale();
//    }
//});

//window.addEventListener('beforeunload', function (event) {
//    if (AllTabsForSale[0].products.length > 0) {
//        event.preventDefault();
//        registerNoCierreSale();
//    }
//});

//function registerNoCierreSale() {

//    //let sale = getVentaForRegister();
//    //sale.idTypeDocumentSale = null
//    //sale.multiplesFormaDePago = null

//    swal({
//        title: 'No es posible cerrar la pantalla con una venta abierta \n\n',
//        icon: 'question',
//        showCancelButton: true,
//        confirmButtonText: 'Aceptar'
//    }, function (value) {


//    });
//}


function getTabActiveId() {
    let idTab = document.getElementsByClassName(" nav-link tab-venta active")[0].id;

    return idTab[idTab.length - 1];
}

async function validateCode() {
    if (!ajustes.needControl) return true;

    return new Promise((resolve, reject) => {
        swal({
            title: 'Codigo de Seguridad',
            type: "input",
            showCancelButton: true,
            closeOnConfirm: true,
            inputPlaceholder: "password..."
        }, function (value) {
            if (value === false) {
                resolve(false);
                return;
            }

            fetch(`/Admin/ValidateSecurityCode?encryptedCode=${value}`, {
                method: 'POST'
            })
                .then(response => response.json())
                .then(data => {
                    if (data.state) {
                        resolve(data.object);
                    } else {
                        resolve(false);
                    }
                    swal.close();
                })
                .catch(error => {
                    reject(false);
                });
        });
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
}