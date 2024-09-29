
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

    $("#cboCondicionIvaCliente").on("change", function () {
        $('#txtCuilCliente').val('');
    });

    seleccionaCliente();
}

function funClientesFactura() {
    showLoading();

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
        $('#txtNombreCliente').val(data.text);
        $('#txtCuilCliente').val(data.cuil);
        //$('#txtIdCuilFacturaCliente').val(data.cuil);
        $('#txtTelefonoCliente').val(data.telefono);
        $('#txtDireccionCliente').val(data.direccion);
        $('#cboCondicionIvaCliente').val(data.condicionIva);
        $('#txtComentarioCliente').val(data.comentario);
        $('#txtIdClienteFactura').val(data.id);
    });

    $('#txtCuilCliente').on('input', function () {
        if ($('#cboCondicionIvaCliente').val() == '4') { // Resp. Inscripto
            let formattedValue = formatCuil($(this).val());
            $(this).val(formattedValue);
        }
    });

    $('#txtCuilCliente').on('keypress', function (e) {
        let charCode = (e.which) ? e.which : e.keyCode;
        if (charCode < 48 || charCode > 57) {
            e.preventDefault();
        }
    });
    removeLoading();
}


function removeHyphens(value) {
    return value.replace(/-/g, '');
}

function resetModalClientesFactura() {
    $('#cboClienteFactura').val(null).trigger('change');
    $('#txtNombreCliente').val('');
    $('#txtCuilCliente').val('');
    $('#txtTelefonoCliente').val('');
    $('#txtDireccionCliente').val('');
    $('#cboCondicionIvaCliente').val('');
    $('#txtComentarioCliente').val('');
    $('#txtIdClienteFacturaCliente').val(0);
}
function toggleFields(isNuevoCliente) {
    $('#txtNombreCliente, #txtCuilCliente, #cboCondicionIvaCliente, #txtTelefonoCliente, #txtDireccionCliente, #txtComentarioCliente').prop('disabled', !isNuevoCliente);
    $('#cboClienteFactura').prop('disabled', isNuevoCliente);

    if (isNuevoCliente) {
        $('#switchGuardarNuevoClienteDiv').css('display', 'block');
    } else {
        $('#switchGuardarNuevoClienteDiv').css('display', 'none');
        $('#switchGuardarNuevoCliente').prop('checked', false);
    }
}

async function seleccionaCliente() {

    $("#btnSeleccionarClienteFactura").on("click", function () {

        const inputs = $("input.input-validate-cliente").serializeArray();
        const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

        if (inputs_without_value.length > 0) {
            const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
            toastr.warning(msg, "");
            $(`input[name="${inputs_without_value[0].name}"]`).focus();
            return;
        }

        let isValid = true;
        let cuilInput = $('#txtCuilCliente').val();

        if ($('#cboCondicionIvaCliente').val() == '4') { // Resp. Inscripto
            isValid = validateCuilCuit();
            cuilInput = removeHyphens(cuilInput);
        } else if ($('#cboCondicionIvaCliente').val() == '1') { // Consumidor Final
            isValid = validateDni();
        }

        if (!isValid) {
            const msg = `El campo CUIL / CUIT / DNI debe ser completado de forma corecta, dependiendo de la condicion del IVA`;
            toastr.warning(msg, "");
            return;
        }

        let nuevoCliente = document.getElementById("switchNuevoCliente").checked
        let guardarNuevouevoCliente = document.getElementById("switchGuardarNuevoCliente").checked

        let BASIC_MODEL_CLIENTE_SALE = {
            idCliente: 0,
            nombre: '',
            cuil: null,
            telefono: null,
            direccion: null,
            condicionIva: null,
            isActive: true
        }

        const model = structuredClone(BASIC_MODEL_CLIENTE_SALE);
        model["nombre"] = $("#txtNombreCliente").val();
        model["direccion"] = $("#txtDireccionCliente").val();
        model["cuil"] = $("#txtCuilCliente").val();
        model["telefono"] = $("#txtTelefonoCliente").val();
        model["condicionIva"] = $("#cboCondicionIvaCliente").val();
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
        let textSinGuiones = model.cuil.replace(/-/g, '');

        $('#txtClienteParaFactura').attr('cuil', textSinGuiones);
        $('#txtClienteParaFactura').attr('idcliente', model.idCliente);

        $('#btnFinalizarVentaParcial').prop('disabled', false);
        $("#txtMinimoIdentificarConsumidor").toggle(false);

        $("#modalDatosFactura").modal("hide")
    });
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
            </tr>
         </table>`
    );

    return container;
}