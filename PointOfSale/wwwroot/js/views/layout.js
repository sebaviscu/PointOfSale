
$(document).ready(function () {
    $("#limpiarNotificaciones").on("click", function () {


        fetch("/Notification/LimpiarTodoNotificacion", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        }).then(response => {
            $(".dropdown-menu .dropdown-header").remove();
            $("#listaNotificaciones").remove();
        }).catch((error) => {
        })
    })


    $(".notificacion").on("click", function () {

        fetch(`/Notification/UpdateNotificacion?idNotificacion=${$(this)[0].id}`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        })
            .then(response => {
                return response.ok ? response.json() : Promise.reject(response);
            }).then(responseJson => {

                window.location.href = responseJson.object.accion;

            }).catch((error) => {
                $("#modalDataTurno").find("div.modal-content").LoadingOverlay("hide")
            })
    })
});

function cerrarTurno() {
    fetch(`/Turno/GetTurnoActual`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            var resp = responseJson.data;

            var dateTimeModif = new Date(resp.fechaInicio);
            $("#txtInicioTurno").val(dateTimeModif.toLocaleString());
            $("#contMetodosPagoLayout").empty();

            let list = document.getElementById("contMetodosPagoLayout");
            for (i = 0; i < resp.ventasPorTipoVenta.length; ++i) {
                let li = document.createElement('li');
                li.innerText = resp.ventasPorTipoVenta[i].descripcion + ": $" + resp.ventasPorTipoVenta[i].total;
                console.log(resp.ventasPorTipoVenta[i].descripcion + ": $" + resp.ventasPorTipoVenta[i].total);
                list.appendChild(li);
            }

            $("#modalDataTurno").modal("show")
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
        return response.ok ? response.json() : Promise.reject(response);
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


function printTicket(text, printerName) {
    fetch('https://localhost:4567/print', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ printerName: printerName, text: text })
    })
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                console.log('Documento enviado a la impresora con éxito');
            } else {
                console.error('Error al enviar el documento a la impresora:', data.error);
            }
        })
        .catch(error => {
            alert('Error al enviar el documento a la impresora: ' + error);
            console.error('Error:', error);
        });
}