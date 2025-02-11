$(document).ready(function () {

    fetch("/Tienda/GetTienda")
        .then(response => {
            if (!response.ok) {
                throw new Error('Network response was not ok.');
            }
            return response.json();
        })
        .then(responseJson => {
            if (responseJson.data.length > 1) {
                $("#cboTiendas").append(
                    $("<option>").val('-1').text('')
                )
            }
            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboTiendas").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })
        .catch(error => {
            console.error('Fetch error:', error);
        });

    $(".btn-submit").on("click", function () {
        $(this).LoadingOverlay('show')

    })
})