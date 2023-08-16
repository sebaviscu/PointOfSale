


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

    fetch("/Turno/CerrarTurno", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
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