let tableDataNotificaciones;
let rowSelectedNotificaciones;
let tipoNotificacionesList = [];
let allUsers = [];

const BASIC_MODEL_NOTIFICACION = {
    idNotifications: 0,
    descripcion: '',
    isActive: 1,
    registrationUser: '',
    registrationDate: null,
    idUser: null,
    userNameString: ''
}


$(document).ready(function () {
    fetch("/Admin/GetUsersByTienda")
        .then(response => response.json())
        .then(responseJson => {
            if (responseJson.data.length > 0) {
                allUsers = responseJson.data; // Guardar todos los usuarios en la variable global

                // Llenar el combo con todos los usuarios inicialmente
                allUsers.forEach((user) => {
                    $("#cboPorUsuario").append(
                        $("<option>").val(user.idUsers).text(user.name)
                    );
                });
                $("#cboPorUsuario").append(
                    $("<option>").val(-2).text(" - TODOS - ")
                );
            }
        });

    tableDataNotificaciones = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Notification/GetNotificaciones",
            "type": "GET",
            "datatype": "json"
        },
        "columnDefs": [
            {
                "targets": [2],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY HH:mm') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idNotifications",
                "visible": false,
                "searchable": false
            },
            { "data": "descripcion" },
            { "data": "registrationDate" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            { "data": "modificationUser" },
            { "data": "modificationDateString" },
            {
                "data": "idUser", render: function (data, type, row) {
                    if (data != null && row.isActive)
                        return '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                            '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>';
                    else
                        return '';
                },
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[2, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Notificaciones',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5]
                }
            }, 'pageLength'
        ]
    });
})


$('#cboPorRol').change(function () {
    let idRol = $(this).val();

    $("#cboPorUsuario").empty(); // Vaciar el combo antes de agregar los nuevos valores

    if (idRol != -2) {
        // Filtrar los usuarios según el rol
        let filteredUsers = allUsers.filter(user => user.idRol == idRol);

        if (filteredUsers.length > 0) {
            filteredUsers.forEach((user) => {
                $("#cboPorUsuario").append(
                    $("<option>").val(user.idUsers).text(user.name)
                );
            });
        }

        $("#cboPorUsuario").append(
            $("<option>").val(-2).text(" - TODOS - ")
        );
        $("#cboPorUsuario").prop("disabled", false);
    } else {
        // Deshabilitar si se selecciona "TODOS"
        $("#cboPorUsuario").prop("disabled", true);
    }
});

const openModalNuevoNotificacion = (model = BASIC_MODEL_NOTIFICACION) => {

    let user = allUsers.filter(user => user.idUsers == model.idUser);

    if (user.length != 0) {
        $("#cboPorRol").val(user[0].idRol);
    }

    $("#txtIdNotificacion").val(model.idNotifications);
    $("#cboEstadoNotificacion").val(model.isActive ? "1" : "0");
    $("#cboPorUsuario").val(model.idUser);

    let splitString = model.descripcion.split("</strong>");

    $("#txtDescripcionNotificacion").val(splitString.length > 1 ? splitString[1] : '');

    $("#cboPorRol").prop("disabled", model.idUser != null);
    $("#cboPorUsuario").prop("disabled", model.idUser != null);
    $("#cboEstadoNotificacion").prop("disabled", model.idUser != null);


    $("#modalNotificacion").modal("show");
}

$("#btnNew").on("click", function () {
    openModalNuevoNotificacion()
})

$("#btnSaveNotificacion").on("click", function () {
    let descripction = $("#txtDescripcionNotificacion").val();

    if (descripction == null || descripction == '') {
        const msg = `La notificacion no puede estar vacia.`;
        toastr.warning(msg, "");
        return;
    }

    const model = structuredClone(BASIC_MODEL_NOTIFICACION);
    model["idNotifications"] = parseInt($("#txtIdNotificacion").val());
    model["descripcion"] = descripction;
    model["isActive"] = parseInt($("#cboEstadoNotificacion").val());
    model["idUser"] = parseInt($("#cboPorUsuario").val());
    model["userNameString"] = $("#cboPorUsuario option:selected").text();
    model["idRol"] = parseInt($("#cboPorRol").val());

    $("#modalNotificacion").find("div.modal-content").LoadingOverlay("show")
    showLoading();

    if (model.idNotifications == 0) {
        fetch("/Notification/CreateNotifications", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalNotificacion").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            removeLoading();

            if (responseJson.state) {
                swal("Exitoso!", "Notificación fué guardada con éxito", "success");
                $("#modalNotificacion").modal("hide")

                if (responseJson.object != null)
                    tableDataNotificaciones.row.add(responseJson.object).draw(false);
                else
                    location.reload();


            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalNotificacion").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Notification/EditNotificacion", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalNotificacion").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                tableDataNotificaciones.row(rowSelectedNotificaciones).data(responseJson.object).draw(false);
                rowSelectedNotificaciones = null;
                swal("Exitoso!", "El Notificaciones fue actualizado con éxito.", "success");
                $('#modalNotificacion').modal('hide');

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalNotificacion").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedNotificaciones = $(this).closest('tr').prev();
    } else {
        rowSelectedNotificaciones = $(this).closest('tr');
    }

    const data = tableDataNotificaciones.row(rowSelectedNotificaciones).data();

    openModalNuevoNotificacion(data);
})

$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataNotificaciones.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar notificacion`,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, eliminar",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Notification/Delete?idNotificacion=${data.idNotifications}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {
                        tableDataNotificaciones.row(row).remove().draw();
                        swal("Exitoso!", "La notificacion fue eliminada", "success");
                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                })
                    .catch((error) => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                    })
            }
        });
})