const BASIC_MODEL_AJUSTE = {
    idAjuste: 0,
    modificationDate: null,
    modificationUser: null,
    montoEnvioGratis: 0,
    aumentoWeb: 0,
    whatsapp: "",
    lunes: "",
    martes: "",
    miercoles: "",
    jueves: "",
    viernes: "",
    sabado: "",
    domingo: "",
    feriado: "",
    facebook: "",
    instagram: "",
    twitter: "",
    tiktok: "",
    youtube: "",
    codigoSeguridad: "",
    imprimirDefault: false,
    controlStock: false,
    nombreTiendaTicket: "",
    nombreImpresora: "",
    minimoIdentificarConsumidor: 0,
    facturaElectronica: false
}

const BASIC_MODEL_AJUSTES_FACTURACION = {
    idAjustesFacturacion: 0,
    logo: "",
    cuit: 0,
    condicionIva: null,
    puntoVenta: 0,
    certificadoFechaInicio: null,
    certificadoFechaCaducidad: null,
    certificadoPassword: "",
    certificadoNombre: "",
    nombreTitular: "",
    ingresosBurutosNro: "",
    direccionFacturacion: "",
    nombreTitular: null
}


let isHealthy = false;
$(document).ready(function () {
    showLoading();

    fetch("/Admin/GetAjuste")
        .then(response => {
            removeLoading();
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                let model = responseJson.object;

                $("#txtId").val(model.idAjuste);
                $("#txtNombreTienda").val(model.nombre);
                $("#txtDireccion").val(model.direccion);

                $("#txtEnvioGratis").val(model.montoEnvioGratis);
                $("#txtAumento").val(model.aumentoWeb);
                $("#txtWhatsApp").val(model.whatsapp);
                $("#txtLunes").val(model.lunes);
                $("#txtMartes").val(model.martes);
                $("#txtMiercoles").val(model.miercoles);
                $("#txtJueves").val(model.jueves);
                $("#txtViernes").val(model.viernes);
                $("#txtSabado").val(model.sabado);
                $("#txtDomingo").val(model.domingo);
                $("#txtFeriados").val(model.feriado);
                $("#txtFacebook").val(model.facebook);
                $("#txtInstagram").val(model.instagram);
                $("#txtTikTok").val(model.tiktok);
                $("#txtTwitter").val(model.twitter);
                $("#txtYouTube").val(model.youtube);


                $("#txtCodigoSeguridad").val(model.codigoSeguridad);
                $("#txtNombreTiendaTicket").val(model.nombreTiendaTicket);
                $("#txtMinimoIdentificarConsumidor").val(model.minimoIdentificarConsumidor);
                $("#cboNombreImpresora").val(model.nombreImpresora);
                document.getElementById('switchImprimirDefault').checked = model.imprimirDefault;
                document.getElementById('switchControlStock').checked = model.controlStock;
                document.getElementById('switchFacturaElectronica').checked = model.facturaElectronica;

                if (model.modificationUser == null)
                    document.getElementById("divModif").style.display = 'none';
                else {
                    document.getElementById("divModif").style.display = '';
                    let dateTimeModif = new Date(model.modificationDate);

                    $("#txtModificado").val(dateTimeModif.toLocaleString());
                    $("#txtModificadoUsuario").val(model.modificationUser);
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    fetch(`/Admin/GetAjustesFacturacion`)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                openModalAjustesFacturacion(responseJson.object);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    setupPasswordToggle($('#txtCodigoSeguridad'), $('#togglePassword'));
    setupPasswordToggle($('#txtContraseñaCertificado'), $('#togglePasswordCert'));

    healthcheck();

})


const openModalAjustesFacturacion = (model = BASIC_MODEL_AJUSTES_FACTURACION) => {


    $("#txtIdAjustesFacturacion").val(model.idAjustesFacturacion);

    $("#cboCondicionIva").val(model.condicionIva);
    $("#txtPuntoVentaCertificado").val(model.puntoVenta);
    $("#txtContraseñaCertificado").val(model.certificadoPassword);

    $("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.certificadoFechaInicio));
    $("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.certificadoFechaCaducidad));
    $("#txtCuilCertificado").val(model.cuit);
    $("#txtNombreArchivo").val(model.certificadoNombre);

    $("#txtNombreTitular").val(model.nombreTitular);
    $("#txtIIBB").val(model.ingresosBurutosNro);
    $("#txtDireccionFacturacion").val(model.direccionFacturacion);

    var fecha = model.fechaInicioActividad.split('T')[0];
    $("#txtInicioActividad").val(fecha);

    // un boton para cragar datos del certificado
    //if (model.vMX509Certificate2 != null) {
    //$("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notBefore));
    //$("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notAfter));
    //$("#txtCuil").val(model.vMX509Certificate2.cuil);
    //}

    $("#modalDataAjustesFscturacion").modal("show")
}

$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL_AJUSTE);

    let checkboxSwitchFacturaElectronica = document.getElementById('switchFacturaElectronica');
    model["facturaElectronica"] = checkboxSwitchFacturaElectronica.checked;

    if (model.facturaElectronica) {

        const inputs = $("input.input-validate-facturacion").serializeArray();
        const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

        if (inputs_without_value.length > 0) {
            const msg = `Si selecciona FACTURA ELECTRONICA, debe completar todos los campos de la sección Facturacion.`;
            toastr.warning(msg, "");
            return;
        }
    }

    model["nombre"] = $("#txtNombreTienda").val();
    model["direccion"] = $("#txtDireccion").val();
    model["idAjuste"] = parseInt($("#txtId").val());
    model["montoEnvioGratis"] = $("#txtEnvioGratis").val();
    model["aumentoWeb"] = $("#txtAumento").val();
    model["whatsapp"] = $("#txtWhatsApp").val();
    model["lunes"] = $("#txtLunes").val();
    model["martes"] = $("#txtMartes").val();
    model["miercoles"] = $("#txtMiercoles").val();
    model["jueves"] = $("#txtJueves").val();
    model["viernes"] = $("#txtViernes").val();
    model["sabado"] = $("#txtSabado").val();
    model["domingo"] = $("#txtDomingo").val();
    model["feriado"] = $("#txtFeriados").val();
    model["facebook"] = $("#txtFacebook").val();
    model["instagram"] = $("#txtInstagram").val();
    model["tiktok"] = $("#txtTikTok").val();
    model["twitter"] = $("#txtTwitter").val();
    model["youtube"] = $("#txtYouTube").val();

    let checkboxSwitchImprimirDefault = document.getElementById('switchImprimirDefault');
    model["imprimirDefault"] = checkboxSwitchImprimirDefault.checked;

    let checkboxSwitchControlStock = document.getElementById('switchControlStock');
    model["controlStock"] = checkboxSwitchControlStock.checked;


    model["codigoSeguridad"] = $("#txtCodigoSeguridad").val();
    model["nombreTiendaTicket"] = $("#txtNombreTiendaTicket").val();
    model["minimoIdentificarConsumidor"] = $("#txtMinimoIdentificarConsumidor").val();
    model["nombreImpresora"] = $("#cboNombreImpresora").val();

    const modelFacturacion = structuredClone(BASIC_MODEL_AJUSTES_FACTURACION);
    modelFacturacion["idAjustesFacturacion"] = parseInt($("#txtIdAjustesFacturacion").val());

    modelFacturacion["puntoVenta"] = parseInt($("#txtPuntoVentaCertificado").val());
    modelFacturacion["condicionIva"] = parseInt($("#cboCondicionIva").val());
    modelFacturacion["nombreTitular"] = $("#txtNombreTitular").val();
    modelFacturacion["ingresosBurutosNro"] = $("#txtIIBB").val();
    modelFacturacion["direccionFacturacion"] = $("#txtDireccionFacturacion").val();
    modelFacturacion["fechaInicioActividad"] = $("#txtInicioActividad").val();
    modelFacturacion["certificadoPassword"] = $("#txtContraseñaCertificado").val();

    const inputCertificado = document.getElementById('fileCertificado');

    const formData = new FormData();
    formData.append('modelFacturacion', JSON.stringify(modelFacturacion));
    formData.append('modelAjustes', JSON.stringify(model));
    formData.append('Certificado', inputCertificado.files[0]);

    showLoading();

    fetch("/Admin/UpdateAjuste", {
        method: "PUT",
        body: formData
    }).then(response => {
        removeLoading();
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            swal("Exitoso!", "Ajustes fué modificado", "success");
            location.reload()

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
    })


})
async function healthcheck() {
    isHealthy = await getHealthcheck();

    if (isHealthy) {
        getPrintersTienda();
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}

async function getPrintersTienda() {
    try {
        let printers = await getPrinters();

        printers.forEach(printer => {
            $("#cboNombreImpresora").append(
                $("<option>").val(printer).text(printer)
            );
        });

    } catch (error) {
        console.error('Error fetching printers:', error);
    }
}

function formatDateToDDMMYYYY(isoDate) {
    if (isoDate == null) {
        return "";
    }

    const date = new Date(isoDate);

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    return `${day}/${month}/${year}`;
}