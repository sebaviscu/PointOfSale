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
    nombreTiendaTicket: "",
    nombreImpresora: "",
    minimoIdentificarConsumidor: 0
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

    let $passwordInput = $('#txtCodigoSeguridad');
    let $togglePasswordButton = $('#togglePassword');

    $togglePasswordButton.on('mousedown', function () {
        $passwordInput.attr('type', 'text');
    });

    $togglePasswordButton.on('mouseup mouseleave', function () {
        $passwordInput.attr('type', 'password');
    });

    // Evitar que el botón reciba el foco
    $togglePasswordButton.on('click', function (e) {
        e.preventDefault();
    });

    healthcheck();

})

$("#btnSave").on("click", function () {
    showLoading();

    const model = structuredClone(BASIC_MODEL_AJUSTE);

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

    const checkboxSwitchImprimirDefault = document.getElementById('switchImprimirDefault');
    model["imprimirDefault"] = checkboxSwitchImprimirDefault.checked;
    model["codigoSeguridad"] = $("#txtCodigoSeguridad").val();
    model["nombreTiendaTicket"] = $("#txtNombreTiendaTicket").val();
    model["minimoIdentificarConsumidor"] = $("#txtMinimoIdentificarConsumidor").val();
    model["nombreImpresora"] = $("#cboNombreImpresora").val();

    fetch("/Admin/UpdateAjuste", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
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