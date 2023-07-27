$(document).ready(function () {
    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.data.length > 1) {
                $("#cboTiendas").append(
                    $("<option>").val('-1').text(''))
            }
            //borrar los options de cboTipoDocumentoVenta
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboTiendas").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })
})