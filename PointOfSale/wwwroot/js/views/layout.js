


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

            let list = document.getElementById("contMetodosPago");
            for (i = 0; i < resp.ventasPorTipoVenta.length; ++i) {
                let li = document.createElement('li');
                li.innerText = resp.ventasPorTipoVenta[i].descripcion + ": $" + resp.ventasPorTipoVenta[i].total;
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
            swal("Exitoso!", "Se ha cerrado el turno y automaticamente hemos abierto otro", "success");

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
        swal("Exitoso!", "Datos generados con éxito", "success");

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
            background-color: rgba(80, 80, 80, 0.8);
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