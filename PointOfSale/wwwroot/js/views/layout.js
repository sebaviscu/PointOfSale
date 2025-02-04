let razonesList;

const BASIC_MODEL_MOVIMIENTIO_CAJA = {
    idMovimientoCaja: 0,
    comentario: '',
    registrationDate: null,
    registrationUser: '',
    idRazonMovimientoCaja: 0,
    importe: 0
}
$(document).ready(function () {

    $('#modalDataAbrirTurno').modal({
        backdrop: 'static',
        keyboard: false
    });

    fetch(`/Notification/GetNotificacionesByUser`, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    })
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                openIndividualNotifications(responseJson.object);

            } else {
                swal("Error", responseJson.message, "error");
            }

        }).catch((error) => {
        })


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
            })
    })


    $(".notificacion").on("click", function () {
        //if ($(this).attr('accion') != '') {

            fetch(`/Notification/UpdateNotificacion?idNotificacion=${$(this)[0].id}`, {
                method: "PUT",
                headers: { 'Content-Type': 'application/json;charset=utf-8' }
            })
                .then(response => {
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {
                        $(this).remove();

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }

                }).catch((error) => {
                })
        //}
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
                        $("#cboCambiarTiendas").append(
                            $("<option>").val(item.idTienda).text(item.nombre + ' ' + actual)
                        )
                    });
                    $("#cboCambiarTiendas").val(idActual);
                }
            })
    })

    $("#btnCambiarTienda").on("click", function () {

        $("#modalCambioTienda").LoadingOverlay("show")

        var idTienda = $("#cboCambiarTiendas").val();
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

    $('[data-toggle="tooltip"]').tooltip();

});



$("#btnAbrirMovimientoCaja").on("click", function () {
    cargarRazones();
    disablesMovimientoCajaModal(false);

    $("#modalMovimientoCaja").modal("show");
});

function cargarRazones() {
    showLoading();
    fetch("/MovimientoCaja/GetRazonMovimientoCajaActivas")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                if (responseJson.object.length > 0) {
                    razonesList = responseJson.object;

                    $("#cboTipoRazonMovimiento").trigger("change");
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
            removeLoading();
        });
}

$("#cboTipoRazonMovimiento").on("change", function () {
    filterRazones();
});

let currentNotificationIndex = 0;
let notificationsList = [];

function openIndividualNotifications(list) {
    notificationsList = list;
    currentNotificationIndex = 0;
    $("#btnLeerNotificacion").prop("disabled", true); // Deshabilitar botón inicialmente
    loadNotification();
    $("#modalNotificacionIndividual").modal({
        backdrop: 'static', // Evitar cerrar el modal al hacer clic afuera
        keyboard: false     // Evitar cerrar el modal con Escape
    });
    $("#modalNotificacionIndividual").modal("show")
}

function loadNotification() {
    const currentNotification = notificationsList[currentNotificationIndex];
    $("#txtAutor").val(currentNotification.registrationUser);
    $("#divNotifIndividual").html(currentNotification.descripcion);

    // Actualizar contador 1/n
    $(".notification-counter").text(`${currentNotificationIndex + 1}/${notificationsList.length}`);

    // Mostrar u ocultar los botones de navegación según la posición actual
    if (notificationsList.length === 1) {
        // Si hay solo una notificación, ocultar ambos botones y habilitar "Recibido"
        $("#btnAnterior, #btnSiguiente").prop("disabled", true);
        $("#btnLeerNotificacion").prop("disabled", false);
    } else {
        // Mostrar los botones de navegación
        $("#btnAnterior, #btnSiguiente").prop("disabled", false);

        // Ocultar el botón "<" si estamos en la primera notificación
        if (currentNotificationIndex === 0) {
            $("#btnAnterior").prop("disabled", true);
        } else {
            $("#btnAnterior").prop("disabled", false);
        }

        // Ocultar el botón ">" si estamos en la última notificación
        if (currentNotificationIndex === notificationsList.length - 1) {
            $("#btnSiguiente").prop("disabled", true);
            $("#btnLeerNotificacion").prop("disabled", false); // Habilitar "Recibido" en la última
        } else {
            $("#btnSiguiente").prop("disabled", false);
            $("#btnLeerNotificacion").prop("disabled", true); // Deshabilitar "Recibido" en otras
        }
    }
}


// Navegar a la notificación anterior
$("#btnAnterior").on("click", function () {
    if (currentNotificationIndex > 0) {
        currentNotificationIndex--;
        loadNotification();
    }
});

// Navegar a la siguiente notificación
$("#btnSiguiente").on("click", function () {
    if (currentNotificationIndex < notificationsList.length - 1) {
        currentNotificationIndex++;
        loadNotification();
    }
});


$("#btnLeerNotificacion").on("click", function () {
    $("#modalNotificacionIndividual").LoadingOverlay("show")

    fetch(`/Notification/LimpiarTodoNotificacionIndividuales`, {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    })
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                //removeLoading();
                $("#modalNotificacionIndividual").modal("hide")
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        }).catch((error) => {
        })

});

function filterRazones() {
    let selectedTipo = $("#cboTipoRazonMovimiento").val();
    $("#cboRazonMovimiento").empty();

    razonesList.forEach((item) => {
        if (item.tipo == selectedTipo) {
            $("#cboRazonMovimiento").append(
                $("<option>").val(item.idRazonMovimientoCaja).text(item.descripcion)
            );
        }
    });
}

$("#btnSaveMovimientoCaja").on("click", function () {
    const inputs = $("input.input-validate-movimientoCaja").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    if ($("#txtComentarioMovimientoCaja").val().length < 5) {
        toastr.warning("La descripción debe ser mayor a 5 caracteres", "");
        return
    }

    let importe = parseFloat($("#txtImporteMovimientoCaja").val());
    if ($("#cboTipoRazonMovimiento").val() == "0" && importe > 0) {
        importe = importe * -1;
    }

    const model = structuredClone(BASIC_MODEL_MOVIMIENTIO_CAJA);
    model["idRazonMovimientoCaja"] = parseInt($("#cboRazonMovimiento").val());
    model["importe"] = importe;
    model["comentario"] = $("#txtComentarioMovimientoCaja").val();

    $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("show")

    fetch("/MovimientoCaja/CreateMovimientoCaja", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {

            $("#modalMovimientoCaja").modal("hide");

            $("#txtImporteMovimientoCaja").val('');
            $("#txtComentarioMovimientoCaja").val('');
        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalMovimientoCaja").find("div.modal-content").LoadingOverlay("hide")
    })
})
function disablesMovimientoCajaModal(type) {
    $('#btnSaveMovimientoCaja').css('display', type ? 'none' : '');
    $('#divFechaUsuarioMovimientoCaja').css('display', type ? '' : 'none');

    $('#cboTipoRazonMovimiento').prop('disabled', type);
    $('#cboRazonMovimiento').prop('disabled', type);
    $('#txtImporteMovimientoCaja').prop('disabled', type);
    $('#txtComentarioMovimientoCaja').prop('disabled', type);

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

$("#btnAbrirTurno").on("click", function () {
    let desc = $("#txtObservacionesInicioCajaCierre").val();
    let importeInicioTurno = $("#txtInicioCajaAbrir").val();

    let modelTurno = {
        observacionesApertura: desc,
        TotalInicioCaja: parseFloat(importeInicioTurno != '' ? importeInicioTurno : 0),
    };
    $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("show")
    showLoading();

    fetch("/Turno/AbrirTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(modelTurno)
    }).then(response => {
        $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {
            removeLoading();
            $("#modalDataAbrirTurno").modal("hide");
            location.reload();

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalDataAbrirTurno").find("div.modal-content").LoadingOverlay("hide")
    })
})

function openModalDataAbrirTurno() {
    $("#modalDataAbrirTurno").modal("show");

    if (moment.tz != null) {

        let dateTimeArgentina = moment().tz('America/Argentina/Buenos_Aires');

        $('#modalDataAbrirTurno').on('shown.bs.modal', function () {
            $("#txtInicioTurnoAbrir").val(dateTimeArgentina.format('DD/MM/YYYY'));
            $("#txtHoraInicioTurnoAbrir").val(dateTimeArgentina.format('HH:mm'));
        });
    }
}

$("#btnGuardarBilletes").on("click", function () {
    let totalBilletes = $("#totalSumBilletes").val();

    $("#txtEfectivo").val(parseInt(totalBilletes));

    actualizarTotal();


    $(".txtCantBillete").each(function () {

        const cantidad = $(this).val();
        const valorNominal = $(this).data('valor');

        if (cantidad > 0) {
            billetesArray.push({ valorNominal, cantidad });
        }
    });

    $("#modalBilletes").modal("hide");

})

$(document).on('input', '.txtCantBillete', function () {
    const idParts = $(this).attr('id').split('_');
    const columnPrefix = idParts[1];
    const index = idParts[2];
    const cantidad = $(this).val();
    const valorNominal = $(this).data('valor');
    const total = cantidad * valorNominal;

    $(`#txtSumaBillete_${columnPrefix}_${index}`).val(`$${total}`);
    calcularTotal();
});

function calcularTotal() {
    let totalSum = 0;
    $('.txtCantBillete').each(function () {
        const cantidad = parseInt($(this).val(), 10);
        const valorNominal = parseInt($(this).data('valor'), 10);
        totalSum += cantidad * valorNominal;
    });
    $('#totalSumBilletes').val(`${totalSum}`);
}