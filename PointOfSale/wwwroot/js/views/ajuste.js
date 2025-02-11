const diasSemana = ["Lunes", "Martes", "Miercoles", "Jueves", "Viernes", "Sabado", "Domingo"];

const BASIC_MODEL_AJUSTE = {
    idAjuste: 0,
    modificationDate: null,
    modificationUser: null,
    codigoSeguridad: "",
    imprimirDefault: false,
    controlStock: false,
    encabezado1: "",
    encabezado2: "",
    encabezado3: "",
    pie1: "",
    pie2: "",
    pie3: "",
    nombreImpresora: "",
    minimoIdentificarConsumidor: 0,
    facturaElectronica: false,
    controlEmpleado: false,
    notificarEmailCierreTurno: false,
    controlTotalesCierreTurno: false,
    emailsReceptores: "",
    emailEmisorCierreTurno: "",
    passwordEmailEmisorCierreTurno: "",
}

const BASIC_MODEL_AJUSTE_WEB = {
    idAjusteWeb: 0,
    montoEnvioGratis: 0,
    costoEnvio: 0,
    compraMinima: 0,
    aumentoWeb: 0,
    whatsapp: "",
    facebook: "",
    instagram: "",
    twitter: "",
    tiktok: "",
    youtube: "",
    habilitarWeb: true,
    nombreComodin1: "",
    nombreComodin2: "",
    nombreComodin3: "",
    habilitarComodin1: false,
    habilitarComodin2: false,
    habilitarComodin3: false,
    takeAwayDescuento: 0,
    habilitarTakeAway: false,
    horariosWeb: [],
    direccion: '',
    nombre: '',
    sobreNosotros: '',
    temrinosCondiciones: '',
    politicaPrivacidad: '',
    email: null,
    palabrasClave: "",
    descripcionWeb: "",
    urlSitio: ""
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
    isProdEnvironment: false
}

const toolbarOptions = [
    [{ 'font': [] }],
    [{ 'header': [1, 2, 3, 4, 5, 6, false] }],
    ['bold', 'italic', 'underline', 'strike'],
    [{ 'color': [] }, { 'background': [] }],
    [{ 'script': 'sub' }, { 'script': 'super' }],
    [{ 'list': 'ordered' }, { 'list': 'bullet' }, { 'indent': '-1' }, { 'indent': '+1' }],
    [{ 'align': [] }],
    ['link', 'image', 'video'],
    ['blockquote', 'code-block'],
    ['clean']
];

let quillSobreNosotros = null;
let quillTermino = null;
let quillPrivacidad = null;

let isHealthyAjustes = false;
$(document).ready(function () {
    showLoading();

    quillSobreNosotros = new Quill("#editor-container-sobre-nosotros", {
        theme: "snow",
        modules: {
            toolbar: toolbarOptions
        }
    });
    quillTermino = new Quill("#editor-container-terminos", {
        theme: "snow",
        modules: {
            toolbar: toolbarOptions
        }
    });
    quillPrivacidad = new Quill("#editor-container-privacidad", {
        theme: "snow",
        modules: {
            toolbar: toolbarOptions
        }
    });

    $('#keywords').select2({
        theme: "classic",
        tags: true,
        tokenSeparators: [','],
        maximumSelectionLength: 10,
        createTag: function (params) {
            var term = $.trim(params.term);
            if (term === '') {
                return null;
            }
            return {
                id: term,
                text: term,
                newTag: true // add additional parameters
            };
        }
    });

    fetch("/Ajustes/GetAjuste")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                let model = responseJson.object;

                $("#txtId").val(model.idAjuste);

                $("#txtCodigoSeguridad").val(model.codigoSeguridad);
                $("#txtEncabezado1").val(model.encabezado1);
                $("#txtEncabezado2").val(model.encabezado2);
                $("#txtEncabezado3").val(model.encabezado3);
                $("#txtPie1").val(model.pie1);
                $("#txtPie2").val(model.pie2);
                $("#txtPie3").val(model.pie3);
                $("#txtMinimoIdentificarConsumidor").val(model.minimoIdentificarConsumidor);
                loadTableFromEmailsString(model.emailsReceptoresCierreTurno);
                $("#txtEmailCierreTurno").val(model.emailEmisorCierreTurno);
                $("#txtEmailPassword").val(model.passwordEmailEmisorCierreTurno);
                document.getElementById('switchImprimirDefault').checked = model.imprimirDefault;
                document.getElementById('switchControlStock').checked = model.controlStock;
                document.getElementById('switchFacturaElectronica').checked = model.facturaElectronica;
                document.getElementById('switchControlEmpleado').checked = model.controlEmpleado;
                document.getElementById('switchCierreTurno').checked = model.notificarEmailCierreTurno;
                document.getElementById('switchConmtrolTotalesCierreTurno').checked = model.controlTotalesCierreTurno;


                if (model.nombreImpresora != null && model.nombreImpresora != '' && $("#cboNombreImpresora").val() == null) {
                    $("#cboNombreImpresora").append(
                        $("<option>").val(model.nombreImpresora).text(model.nombreImpresora)
                    );
                }
                else
                    $("#cboNombreImpresora").val(model.nombreImpresora);

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

    fetch("/Ajustes/GetAjusteWeb")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                let model = responseJson.object;
                document.getElementById("imgLogoPreview").src = model.logoImagenNombre;

                $("#txtNombreTienda").val(model.nombre);
                $("#txtDireccion").val(model.direccion);
                $("#txtEnvioGratis").val(model.montoEnvioGratis);
                $("#txtCostoEnvio").val(model.costoEnvio);
                $("#txtCompraMinima").val(model.compraMinima);
                $("#txtAumento").val(model.aumentoWeb);
                $("#txtWhatsApp").val(model.whatsapp);
                $("#txtFacebook").val(model.facebook);
                $("#txtInstagram").val(model.instagram);
                $("#txtTikTok").val(model.tiktok);
                $("#txtTwitter").val(model.twitter);
                $("#txtYouTube").val(model.youtube);
                $("#txtEmail").val(model.email);

                quillSobreNosotros.clipboard.dangerouslyPasteHTML(quillSobreNosotros.getLength(), model.sobreNosotros);
                quillTermino.clipboard.dangerouslyPasteHTML(quillTermino.getLength(), model.temrinosCondiciones);
                quillPrivacidad.clipboard.dangerouslyPasteHTML(quillPrivacidad.getLength(), model.politicaPrivacidad);

                diasSemana.forEach(dia => {
                    const horariosDia = model.horariosWeb.filter(h => h.diaSemana === diasSemana.indexOf(dia) + 1);
                    const diaContainer = $(`[data-dia="${dia}"] .horarios-dia`);

                    horariosDia.forEach(horario => {
                        agregarFilaHorario(diaContainer, horario.id, horario.horaInicio, horario.horaFin);
                    });
                });

                if (model.palabrasClave) {
                    var palabrasClaveArray = model.palabrasClave.split(',');

                    // Agregar opciones al select
                    palabrasClaveArray.forEach(function (palabra) {
                        var newOption = new Option(palabra, palabra, false, false);
                        $('#keywords').append(newOption).trigger('change');
                    });

                    // Seleccionar opciones
                    $('#keywords').val(palabrasClaveArray).trigger('change');
                }

                $("#txtDescripcionWeb").val(model.descripcionWeb);
                $("#txtUrl").val(model.urlSitio);


                document.getElementById('switchTakeAway').checked = model.habilitarTakeAway;
                $("#txtTakeAway").val(model.takeAwayDescuento);

                document.getElementById('switchHabilitarWeb').checked = model.habilitarWeb;

                document.getElementById('switchHabilitarComodin1').checked = model.habilitarComodin1;
                document.getElementById('switchHabilitarComodin2').checked = model.habilitarComodin2;
                document.getElementById('switchHabilitarComodin3').checked = model.habilitarComodin3;
                document.getElementById('switchIvaPrecio').checked = model.ivaEnPrecio;
                $("#txtComodin1").val(model.nombreComodin1);
                $("#txtComodin2").val(model.nombreComodin2);
                $("#txtComodin3").val(model.nombreComodin3);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    fetch(`/Ajustes/GetAjustesFacturacion`)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                let model = responseJson.object;

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

                document.getElementById('switchIsProdEnvironment').checked = model.isProdEnvironment;

                if (model.fechaInicioActividad != null) {
                    let fecha = model.fechaInicioActividad.split('T')[0];
                    $("#txtInicioActividad").val(fecha);
                }

                removeLoading();
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    setupPasswordToggle($('#txtCodigoSeguridad'), $('#togglePassword'));
    setupPasswordToggle($('#txtContraseñaCertificado'), $('#togglePasswordCert'));
    setupPasswordToggle($('#txtEmailPassword'), $('#toggleEmailPassword'));


    healthcheck();

})

$('#keywords').on('change', function () {
    var totalCharacters = $(this).val().reduce((acc, curr) => acc + curr.length, 0);
    if (totalCharacters > 160) {
        alert("El total de caracteres de las palabras clave no debe exceder los 160.");
    }
});

$('#txtDescripcionWeb').on('input', function () {
    var input = $(this).val();
    if (input.length > 160) {
        alert("La descripción web no puede exceder los 160 caracteres.");
    }
});

function agregarFilaHorario(diaContainer, id = "", horaInicio = "", horaFin = "") {

    const filaHtml = `
            <div class="d-flex justify-content-between align-items-center mb-2 horario-row" id="${id}">
                <div class="">
                    <input type="time" class="form-control form-control-sm horario-inicio" value="${horaInicio}">
                </div>
                <div class="">
                    <input type="time" class="form-control form-control-sm horario-fin" value="${horaFin}">
                </div>
            </div>
        `;

    diaContainer.append(filaHtml);
}

$(".btn-agregar-horario").on("click", function () {
    const diaContainer = $(this).closest("[data-dia]").find(".horarios-dia");
    agregarFilaHorario(diaContainer);
});

function obtenerHorarios() {
    let horarios = [];

    // Recorremos cada día de la semana
    $("#horariosContainer > div  > div > div").each(function () {
        let diaSemana = $(this).data("dia"); // Obtenemos el día de la semana
        let filasHorario = $(this).find(".horarios-dia .horario-row");

        filasHorario.each(function () {
            let horarioInicio = $(this).find(".horario-inicio").val();
            let horarioFin = $(this).find(".horario-fin").val();
            let horarioId = $(this).attr("id") || 0; // Si hay un ID existente, lo usamos; si no, es null

            if (horarioInicio && horarioFin) {
                horarios.push({
                    id: horarioId,
                    diaSemana: diaSemana,
                    horaInicio: horarioInicio,
                    horaFin: horarioFin
                });
            }
        });
    });

    return horarios;
}

$("#txtAddEmailsReceptores").on("click", function () {
    let email = $("#txtEmailReceptor").val();

    if (email !== "") {

        let newRow = rowTableFromEmails(email);

        $("#tbEmailsReceptores tbody").append(newRow);

        $("#txtEmailReceptor").val("");
    } else {
        alert("Por favor ingrese un email válido.");
    }
});

$("#tbEmailsReceptores").on("click", ".delete-row", function () {
    $(this).closest("tr").remove();
});

function getEmailsFromTable() {
    let emails = [];

    $("#tbEmailsReceptores tbody tr").each(function () {
        let email = $(this).find("td:first").text();
        emails.push(email);
    });

    return emails.join(";");
}

function loadTableFromEmailsString(emailsString) {
    $("#tbEmailsReceptores tbody").empty();

    if (emailsString != null && emailsString != '') {
        let emailsArray = emailsString.split(";");

        emailsArray.forEach(function (email) {
            let newRow = rowTableFromEmails(email);

            $("#tbEmailsReceptores tbody").append(newRow);
        });
    }

}

function rowTableFromEmails(email) {
    return "<tr class='small-row'>" +
        "<td class='small-cell'>" + email + "</td>" +
        "<td class='delete-button-email'><button class='btn btn-danger btn-sm delete-row'>" +
        "<span class='mdi mdi-delete'></span></button></td></tr>";
}


$("#btnCargarCertificado").on("click", function () {

    let password = $("#txtContraseñaCertificado").val();

    if (password.trim() == "") {
        const msg = `Debe completar la contraseña para poder cargar la informacion del certificado.`;
        toastr.warning(msg, "");
        return;
    }

    const inputCertificado = document.getElementById('fileCertificado');

    if (inputCertificado.files.length == 0) {
        const msg = `Debe buscar un certificado.`;
        toastr.warning(msg, "");
        return;
    }

    const formData = new FormData();
    formData.append('Certificado', inputCertificado.files[0]);
    formData.append('password', password);

    showLoading();

    fetch("/Ajustes/UpdateCertificateInformation", {
        method: "PUT",
        body: formData
    }).then(response => {
        removeLoading();
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {
            let model = responseJson.object;

            $("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.certificadoFechaInicio));
            $("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.certificadoFechaCaducidad));
            $("#txtCuilCertificado").val(model.cuit);
            $("#txtNombreArchivo").val(model.certificadoNombre);

            swal("Exitoso!", "Certificado cargado", "success");
        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
    })

})

$('#imagenLogo').on('change', function (event) {
    const file = event.target.files[0]; // Obtener el archivo seleccionado
    if (file) {
        const reader = new FileReader(); // Crear un lector de archivos
        reader.onload = function (e) {
            // Establecer el src de la imagen en la previsualización
            $('#imgLogoPreview').attr('src', e.target.result);
        };
        reader.readAsDataURL(file); // Leer el archivo como URL base64
    }
});


$("#btnSave").on("click", async function () {


    let checkboxSwitchFacturaElectronica = document.getElementById('switchFacturaElectronica');

    if (checkboxSwitchFacturaElectronica.checked) {

        const inputs = $("input.input-validate-facturacion").serializeArray();
        const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

        let condicionIva = $("#cboCondicionIva").val();

        if (inputs_without_value.length > 0 || condicionIva == null) {
            const msg = `Si selecciona FACTURA ELECTRONICA, debe completar todos los campos de la sección Facturacion.`;
            toastr.warning(msg, "");
            return;
        }
    }

    let checkboxSwitchHabilitarWeb = document.getElementById('switchHabilitarWeb');

    if (checkboxSwitchHabilitarWeb.checked) {

        let nroWA = $("#txtWhatsApp").val();

        if (nroWA == '') {
            const msg = `Si la web esta habilitada, es necesario agregar un numero de WhstaApp para recibir pedidos.`;
            toastr.warning(msg, "");
            return;
        }
    }

    let email = $('#txtEmailCierreTurno').val();

    if (email != '' && !email.endsWith('@gmail.com')) {

        const msg = `El correo del emisor de Cierre de Turno, debe ser de Gmail.`;
        toastr.warning(msg, "");
        return;
    }

    const modelWeb = structuredClone(BASIC_MODEL_AJUSTE_WEB);
    modelWeb["montoEnvioGratis"] = $("#txtEnvioGratis").val();
    modelWeb["compraMinima"] = $("#txtCompraMinima").val();
    modelWeb["costoEnvio"] = $("#txtCostoEnvio").val();
    modelWeb["aumentoWeb"] = $("#txtAumento").val();
    modelWeb["whatsapp"] = $("#txtWhatsApp").val();
    modelWeb["facebook"] = $("#txtFacebook").val();
    modelWeb["instagram"] = $("#txtInstagram").val();
    modelWeb["tiktok"] = $("#txtTikTok").val();
    modelWeb["twitter"] = $("#txtTwitter").val();
    modelWeb["youtube"] = $("#txtYouTube").val();
    modelWeb["nombre"] = $("#txtNombreTienda").val();
    modelWeb["direccion"] = $("#txtDireccion").val();
    modelWeb["email"] = $("#txtEmail").val();
    modelWeb["sobreNosotros"] = FormatearQuill(quillSobreNosotros.root.innerHTML, "Sobre Nosotros");
    modelWeb["temrinosCondiciones"] = FormatearQuill(quillTermino.root.innerHTML, "Terminos y condiciones");
    modelWeb["politicaPrivacidad"] = FormatearQuill(quillPrivacidad.root.innerHTML, "Sobre Nosotros");

    modelWeb["horariosWeb"] = obtenerHorarios();

    let checkboxSwitchTakeAway = document.getElementById('switchTakeAway');
    modelWeb["habilitarTakeAway"] = checkboxSwitchTakeAway.checked;
    modelWeb["takeAwayDescuento"] = $("#txtTakeAway").val();

    modelWeb["habilitarWeb"] = checkboxSwitchHabilitarWeb.checked;

    modelWeb["nombreComodin1"] = $("#txtComodin1").val();
    modelWeb["nombreComodin2"] = $("#txtComodin2").val();
    modelWeb["nombreComodin3"] = $("#txtComodin3").val();

    let habilitarComodin1 = document.getElementById('switchHabilitarComodin1');
    modelWeb["habilitarComodin1"] = habilitarComodin1.checked;
    let habilitarComodin2 = document.getElementById('switchHabilitarComodin2');
    modelWeb["habilitarComodin2"] = habilitarComodin2.checked;
    let habilitarComodin3 = document.getElementById('switchHabilitarComodin3');
    modelWeb["habilitarComodin3"] = habilitarComodin3.checked;
    let habilitarIvaEnPrecio = document.getElementById('switchIvaPrecio');
    modelWeb["IvaEnPrecio"] = habilitarIvaEnPrecio.checked;

    let palabrasClave = $('#keywords').val();
    if (palabrasClave && palabrasClave.length > 0) {

        if (palabrasClave > 160) {
            const msg = `El total de caracteres de las palabras clave no debe exceder los 160.`;
            toastr.warning(msg, "");
            return;
        }

        modelWeb["palabrasClave"] = palabrasClave.join(','); // Combinar las palabras clave en una cadena separada por comas
    }

    let descrWeb = $("#txtDescripcionWeb").val();

    if (descrWeb.length > 160) {
        const msg = `La descripción web no puede exceder los 160 caracteres.`;
        toastr.warning(msg, "");
        return;
    }

    modelWeb["descripcionWeb"] = descrWeb
    modelWeb["urlSitio"] = $("#txtUrl").val();


    const model = structuredClone(BASIC_MODEL_AJUSTE);
    model["idAjuste"] = parseInt($("#txtId").val());

    let checkboxSwitchImprimirDefault = document.getElementById('switchImprimirDefault');
    model["imprimirDefault"] = checkboxSwitchImprimirDefault.checked;

    let checkboxSwitchControlStock = document.getElementById('switchControlStock');
    model["controlStock"] = checkboxSwitchControlStock.checked;

    let checkboxSwitchControlEmpleados = document.getElementById('switchControlEmpleado');
    model["controlEmpleado"] = checkboxSwitchControlEmpleados.checked;

    let checkboxSwitchCierreTurno = document.getElementById('switchCierreTurno');
    model["notificarEmailCierreTurno"] = checkboxSwitchCierreTurno.checked;

    let checkboxswitchConmtrolTotalesCierreTurno = document.getElementById('switchConmtrolTotalesCierreTurno');
    model["controlTotalesCierreTurno"] = checkboxswitchConmtrolTotalesCierreTurno.checked;

    let emailsReceptores = getEmailsFromTable();
    model["emailEmisorCierreTurno"] = $("#txtEmailCierreTurno").val();
    model["passwordEmailEmisorCierreTurno"] = $("#txtEmailPassword").val();
    model["emailsReceptoresCierreTurno"] = emailsReceptores;
    model["codigoSeguridad"] = $("#txtCodigoSeguridad").val();
    model["encabezado1"] = $("#txtEncabezado1").val();
    model["encabezado2"] = $("#txtEncabezado2").val();
    model["encabezado3"] = $("#txtEncabezado3").val();
    model["pie1"] = $("#txtPie1").val();
    model["pie2"] = $("#txtPie2").val();
    model["pie3"] = $("#txtPie3").val();
    model["minimoIdentificarConsumidor"] = $("#txtMinimoIdentificarConsumidor").val();
    model["nombreImpresora"] = $("#cboNombreImpresora").val();

    model["facturaElectronica"] = checkboxSwitchFacturaElectronica.checked;

    const modelFacturacion = structuredClone(BASIC_MODEL_AJUSTES_FACTURACION);
    modelFacturacion["idAjustesFacturacion"] = parseInt($("#txtIdAjustesFacturacion").val());

    modelFacturacion["puntoVenta"] = parseInt($("#txtPuntoVentaCertificado").val());
    modelFacturacion["condicionIva"] = parseInt($("#cboCondicionIva").val());
    modelFacturacion["nombreTitular"] = $("#txtNombreTitular").val();
    modelFacturacion["ingresosBurutosNro"] = $("#txtIIBB").val();
    modelFacturacion["direccionFacturacion"] = $("#txtDireccionFacturacion").val();
    modelFacturacion["fechaInicioActividad"] = $("#txtInicioActividad").val();
    modelFacturacion["certificadoPassword"] = $("#txtContraseñaCertificado").val();

    let checkboxSwitchIsProdEnvironment = document.getElementById('switchIsProdEnvironment');
    modelFacturacion["isProdEnvironment"] = checkboxSwitchIsProdEnvironment.checked;

    const inputCertificado = document.getElementById('fileCertificado');

    const inputLogo = document.getElementById('imagenLogo');

    const formData = new FormData();
    formData.append('modelWeb', JSON.stringify(modelWeb));
    formData.append('modelFacturacion', JSON.stringify(modelFacturacion));
    formData.append('modelAjustes', JSON.stringify(model));

    if (inputCertificado.files.length > 0) {
        formData.append('Certificado', inputCertificado.files[0]);
    }

    if (inputLogo.files && inputLogo.files.length > 0) {
        let compressedImage = await compressImage(inputLogo.files[0], 0.7, 300, 300);
        formData.append('ImagenLogo', compressedImage, inputLogo.files[0].name
        );
    }

    showLoading();

    fetch("/Ajustes/UpdateAjuste", {
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
    isHealthyAjustes = await getHealthcheck();

    if (isHealthyAjustes) {
        getPrintersTienda();
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}

function FormatearQuill(texto, textoAlerta) {
    if (texto.startsWith("<p><br></p>")) {
        texto = texto.substring(11);
    }

    if (texto.length > 50000) {
        const msg = `El texto '${textoAlerta}', no puede ser tan largo.`;
        toastr.warning(msg, "");
        return;

    }
    return texto;
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

$("#btnPruebaTicket").on("click", function () {
    showLoading()
    fetch(`/Ajustes/PrintTicketTests`,)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            removeLoading();
            if (responseJson.state) {
                swal("Exitoso!", "El ticket de prueba se está imprimiendo", "success");

                if (isHealthyAjustes &&
                    responseJson.object.nombreImpresora != null
                    && responseJson.object.nombreImpresora != ''
                    && responseJson.object.ticket != null
                    && responseJson.object.ticket != '') {

                    printTicket(responseJson.object.ticket, responseJson.object.nombreImpresora, responseJson.object.imagesTicket);
                }

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
})