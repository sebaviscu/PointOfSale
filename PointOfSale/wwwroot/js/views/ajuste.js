const BASIC_MODEL = {
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
    youtube: ""
}


$(document).ready(function () {
    showLoading();

    fetch("/Admin/GetAjuste")
        .then(response => {
            removeLoading();
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data != null) {

                var model = responseJson.data;

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

                if (model.modificationUser === null)
                    document.getElementById("divModif").style.display = 'none';
                else {
                    document.getElementById("divModif").style.display = '';
                    var dateTimeModif = new Date(model.modificationDate);

                    $("#txtModificado").val(dateTimeModif.toLocaleString());
                    $("#txtModificadoUsuario").val(model.modificationUser);
                }


            }
        })

})

$("#btnSave").on("click", function () {
    showLoading();

    const model = structuredClone(BASIC_MODEL);

    model["nombre"] = $("#txtNombreTienda").val();
    model["direccion"] = $("#txtDireccion").val();
    model["idAjuste"] = parseInt($("#txtId").val());
    model["nombreImpresora"] = $("#txtImpresora").val();
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

    fetch("/Admin/UpdateAjuste", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        removeLoading();
        return response.ok ? response.json() : Promise.reject(response);
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