const BASIC_MODEL_CLIENTE_SALE = {
    idCliente: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    condicionIva: null,
    isActive: true
}

$(document).ready(function () {

    $("#limpiarNotificaciones").on("click", function () {




        fetch(`/Notification/LimpiarTodoNotificacion`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        })
            .then(response => {
                return response.json();
            }).then(responseJson => {
                if (responseJson.state) {
                    $(".dropdown-menu .dropdown-header").remove();
                    $("#listaNotificaciones").remove();

                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }

            }).catch((error) => {
                $("#modalDataTurno").find("div.modal-content").LoadingOverlay("hide")
            })
    })


    $(".notificacion").on("click", function () {
        if ($(this).attr('accion') != '') {

            fetch(`/Notification/UpdateNotificacion?idNotificacion=${$(this)[0].id}`, {
                method: "PUT",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            })
                .then(response => {
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {
                        window.location.href = responseJson.object.accion;

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }

                }).catch((error) => {
                    $("#modalDataTurno").find("div.modal-content").LoadingOverlay("hide")
                })
        }
    })

    $("#btnChangePV").on("click", function () {

        fetch("/Tienda/GetTienda")
            .then(response => {
                return response.json();
            }).then(responseJson => {

                if (responseJson.data.length > 0) {
                    $("#modalCambioTienda").modal("show");
                    let idActual = 0;
                    responseJson.data.forEach((item) => {
                        let actual = '';
                        if (item.tiendaActual) {
                            idActual = item.idTienda;
                            actual = ' (Actual)';
                        }
                        $("#cboTiendas").append(
                            $("<option>").val(item.idTienda).text(item.nombre + ' ' + actual)
                        )
                    });
                    $("#cboTiendas").val(idActual);
                }
            })
    })

    $("#btnCambiarTienda").on("click", function () {

        //$("#modalCambioTienda").modal("hide");
        $("#modalCambioTienda").LoadingOverlay("show")

        var idTienda = $("#cboTiendas").val();
        fetch(`/Tienda/ChangeTienda?idTienda=${idTienda}`, {
            method: "DELETE"
        }).then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
            .catch((error) => {
            })
    })
});

function cerrarTurno() {
    fetch(`/Turno/GetTurnoActual`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                var resp = responseJson.object;

                var dateTimeModif = new Date(resp.fechaInicio);
                $("#txtInicioTurno").val(dateTimeModif.toLocaleString());
                $("#contMetodosPagoLayout").empty();

                let list = document.getElementById("contMetodosPagoLayout");
                for (i = 0; i < resp.ventasPorTipoVenta.length; ++i) {
                    let li = document.createElement('li');
                    li.innerText = resp.ventasPorTipoVenta[i].descripcion + ": $ " + resp.ventasPorTipoVenta[i].total;
                    console.log(resp.ventasPorTipoVenta[i].descripcion + ": $ " + resp.ventasPorTipoVenta[i].total);
                    list.appendChild(li);
                }

                $("#modalDataTurno").modal("show")
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
}


$("#btnSaveTurno").on("click", function () {
    let desc = $("#txtDescripcion").val();

    var modelTurno = {
        descripcion: desc
    };

    fetch("/Turno/CerrarTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(modelTurno)
    }).then(response => {
        $("#modalDataTurno").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            $("#modalDataTurno").modal("hide");
            //swal("Exitoso!", "Se ha cerrado el turno y automaticamente hemos abierto otro", "success");
            swal({
                title: 'Se ha cerrado el turno.',
                text: 'Se debe iniciar sesion nuevamente.',
                showCancelButton: false,
                closeOnConfirm: false
            }, function (value) {

                document.location.href = "/";

            });

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataTurno").find("div.modal-content").LoadingOverlay("hide")
    })
})


function generarDatos() {
    showLoading();

    fetch(`/Access/GenerarDatos`, {
        method: "POST"

    }).then(responseJson => {

        removeLoading();
        if (responseJson.status == 200) {
            swal("Exitoso!", "Datos cerados", "success");

        } else {
            swal("Lo sentimos", "", "error");
        }
    })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });

}

function showLoading() {
    if (document.getElementById("divLoadingFrame") != null) {
        return;
    }
    var style = document.createElement("style");
    style.id = "styleLoadingWindow";
    style.innerHTML = `
        .loading-frame {
            position: fixed;
            background-color: rgba(80, 80, 80, 0.3);
            left: 0;
            top: 0;
            right: 0;
            bottom: 0;
            z-index: 4;
        }

        .loading-track {
            height: 50px;
            display: inline-block;
            position: absolute;
            top: calc(50% - 50px);
            left: 50%;
        }

        .loading-dot {
            height: 5px;
            width: 5px;
            background-color: white;
            border-radius: 100%;
            opacity: 0;
        }

        .loading-dot-animated {
            animation-name: loading-dot-animated;
            animation-direction: alternate;
            animation-duration: .75s;
            animation-iteration-count: infinite;
            animation-timing-function: ease-in-out;
        }

        @keyframes loading-dot-animated {
            from {
                opacity: 0;
            }

            to {
                opacity: 1;
            }
        }
    `
    document.body.appendChild(style);
    var frame = document.createElement("div");
    frame.id = "divLoadingFrame";
    frame.classList.add("loading-frame");
    for (var i = 0; i < 10; i++) {
        var track = document.createElement("div");
        track.classList.add("loading-track");
        var dot = document.createElement("div");
        dot.classList.add("loading-dot");
        track.style.transform = "rotate(" + String(i * 36) + "deg)";
        track.appendChild(dot);
        frame.appendChild(track);
    }
    document.body.appendChild(frame);
    var wait = 0;
    var dots = document.getElementsByClassName("loading-dot");
    for (var i = 0; i < dots.length; i++) {
        window.setTimeout(function (dot) {
            dot.classList.add("loading-dot-animated");
        }, wait, dots[i]);
        wait += 150;
    }
};

function removeLoading() {
    document.body.removeChild(document.getElementById("divLoadingFrame"));
    document.body.removeChild(document.getElementById("styleLoadingWindow"));
};

function setupPasswordToggle($passwordInput, $toggleButton) {
    $toggleButton.on('mousedown', function () {
        $passwordInput.attr('type', 'text');
    });

    $toggleButton.on('mouseup mouseleave', function () {
        $passwordInput.attr('type', 'password');
    });

    // Evitar que el botón reciba el foco
    $toggleButton.on('click', function (e) {
        e.preventDefault();
    });
}

function formatCuil(value) {
    value = value.replace(/\D/g, '');
    let formattedValue = '';

    if (value.length > 0) formattedValue += value.substring(0, 2);
    if (value.length > 2) formattedValue += '-' + value.substring(2, 10);
    if (value.length > 10) formattedValue += '-' + value.substring(10, 11);

    return formattedValue;
}

function validateCuilCuit() {
    let cuilValue = $('#txtCuilCliente').val().replace(/\D/g, '');
    return cuilValue.length == 11;
}

function validateDni() {
    let dniValue = $('#txtCuilCliente').val().replace(/\D/g, '');
    return dniValue.length == 8;
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
        $('#txtIdCuilFacturaCliente').val(data.cuil);
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
        $('#txtClienteParaFactura').attr('cuil', model.cuil);
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