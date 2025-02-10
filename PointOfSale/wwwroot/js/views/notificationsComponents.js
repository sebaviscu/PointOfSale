$(document).ready(function () {
    $('#listaNotificaciones').addClass('show');
    $('#listaNotificaciones .dropdown-menu').addClass('show');
});


$("#limpiarNotificaciones").on("click", function () {
    $(this).LoadingOverlay("show")

    fetch(`/Notification/LimpiarTodoNotificacion`, {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    })
        .then(response => {
            return response.json();
        }).then(responseJson => {
            $('#limpiarNotificaciones').LoadingOverlay("hide")

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
})