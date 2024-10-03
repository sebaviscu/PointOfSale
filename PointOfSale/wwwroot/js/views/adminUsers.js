let tableDataUsers;
let rowSelectedUser;

const BASIC_MODEL_USER = {
    idUsers: 0,
    name: "",
    email: "",
    phone: "",
    idRol: 0,
    password: "",
    isActive: 1,
    photo: "",
    idTienda: 0,
    tiendaName: "",
    modificationDate: "",
    modificationUser: null,
    horarios: []
}

const BASIC_MODEL_HORARIOS = {
    id: 0,
    HoraEntrada: "",
    HoraSalida: "",
    DiaSemana: null,
    ModificationUser: null,
    modificationDate: null,
    RegistrationDate: null
}


$(document).ready(function () {

    fetch("/Admin/GetRoles")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                if (responseJson.object.length > 0) {
                    responseJson.object.forEach((item) => {
                        $("#cboRol").append(
                            $("<option>").val(item.idRol).text(item.description)
                        )
                    });
                }
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        })

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            $("#cboTiendas").append(
                $("<option>").val('-1').text(' '))

            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboTiendas").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })

    tableDataUsers = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetUsers",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idUsers",
                "visible": false,
                "searchable": false
            },
            { "data": "name" },
            { "data": "email" },
            { "data": "phone" },
            { "data": "nameRol" },
            { "data": "tiendaName" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "100px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Report Users',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    let $passwordInput = $('#txtPassWord');
    let $togglePasswordButton = $('#togglePassword');

    $togglePasswordButton.on('mousedown', function () {
        $passwordInput.attr('type', 'text');
    });

    $togglePasswordButton.on('mouseup mouseleave', function () {
        $passwordInput.attr('type', 'password');
    });

    // Evitar que el botón reciba el foco
    $togglePasswordButton.on('click', function (e) {
        e.preventDefault();
    });

})

const openModalUser = (model = BASIC_MODEL_USER) => {
    let rol = model.idRol == 0 ? $("#cboRol option:first").val() : model.idRol;
    let tienda = model.idTienda == 0 ? null : model.idTienda;
    $("#txtId").val(model.idUsers);
    $("#txtName").val(model.name);
    $("#txtEmail").val(model.email);
    $("#txtPhone").val(model.phone);
    $("#cboRol").val(rol);
    $("#cboState").val(model.isActive);
    $("#txtPassWord").val(model.password);

    $("#cboTiendas").val(tienda);


    document.getElementById('switchSinHorario').checked = model.sinHorario;

    if (model.horarios != null && model.horarios.length == 7) {

        setearHorario("timeRangeLunes", model.horarios[0].horaEntrada, model.horarios[0].horaSalida);
        setearHorario("timeRangeMartes", model.horarios[1].horaEntrada, model.horarios[1].horaSalida);
        setearHorario("timeRangeMiercoles", model.horarios[2].horaEntrada, model.horarios[2].horaSalida);
        setearHorario("timeRangeJueves", model.horarios[3].horaEntrada, model.horarios[3].horaSalida);
        setearHorario("timeRangeViernes", model.horarios[4].horaEntrada, model.horarios[4].horaSalida);
        setearHorario("timeRangeSabado", model.horarios[5].horaEntrada, model.horarios[5].horaSalida);
        setearHorario("timeRangeDomingo", model.horarios[6].horaEntrada, model.horarios[6].horaSalida);
    }
    else {
        setearHorario("timeRangeLunes", "06:00", "16:00");      // Lunes (6:00 AM - 4:00 PM)
        setearHorario("timeRangeMartes", "09:00", "17:00");     // Martes (9:00 AM - 5:00 PM)
        setearHorario("timeRangeMiercoles", "08:00", "18:00");  // Miércoles (8:00 AM - 6:00 PM)
        setearHorario("timeRangeJueves", "08:00", "18:00");  // Miércoles (8:00 AM - 6:00 PM)
        setearHorario("timeRangeViernes", "08:00", "18:00");  // Miércoles (8:00 AM - 6:00 PM)
        setearHorario("timeRangeSabado", "08:00", "18:00");  // Miércoles (8:00 AM - 6:00 PM)
        setearHorario("timeRangeDomingo", "08:00", "18:00");  // Miércoles (8:00 AM - 6:00 PM)
    }


    //let rol = $('#cboRol').val();
    //$("#cboTiendas").prop("disabled", rol == '1');

    if (model.modificationUser == null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalData").modal("show")

}

$("#btnNewUser").on("click", function () {
    openModalUser()
})

$('#cboRol').change(function () {
    let rol = $(this).val();

    $("#cboTiendas").val(rol == '1' ? '' : null);
    $("#cboTiendas").prop("disabled", rol == '1');

})

$("#btnSave").on("click", function () {
    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    let rol = $("#cboRol").val();
    let tienda = $("#cboTiendas").val();

    if (rol != '1' && (tienda == '-1' || tienda == null)) {
        toastr.warning("Se debe seleccionar una tienda", "");
        return;
    }

    const model = structuredClone(BASIC_MODEL_USER);
    model["idUsers"] = parseInt($("#txtId").val());
    model["name"] = $("#txtName").val();
    model["email"] = $("#txtEmail").val();
    model["phone"] = $("#txtPhone").val();
    model["idRol"] = rol
    model["isActive"] = $("#cboState").val();
    model["password"] = $("#txtPassWord").val();

    model["idTienda"] = tienda != '' && tienda != '-1' ? tienda : null;


    let checkboxSwitchSinHorario = document.getElementById('switchSinHorario');
    model["sinHorario"] = checkboxSwitchSinHorario.checked;

    let horarios = obtenerHorariosSemana();

    const formData = new FormData();
    formData.append('model', JSON.stringify(model));
    formData.append('horarios', JSON.stringify(horarios));

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idUsers == 0) {
        fetch("/Admin/CreateUser", {
            method: "POST",
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataUsers.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "El usuario fué creado", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateUser", {
            method: "PUT",
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataUsers.row(rowSelectedUser).data(responseJson.object).draw(false);
                rowSelectedUser = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "El usuario fué modificado", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }

})

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedUser = $(this).closest('tr').prev();
    } else {
        rowSelectedUser = $(this).closest('tr');
    }

    const data = tableDataUsers.row(rowSelectedUser).data();

    showLoading();
    fetch(`/Admin/GetUser?idUser=${data.idUsers}`,)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            removeLoading();
            if (responseJson.state) {

                openModalUser(responseJson.object);

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
})



$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataUsers.row(row).data();

    swal({
        title: "¿Estas seguro?",
        text: `Que desea eliminar el usuario "${data.name}"`,
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

                fetch(`/Admin/DeleteUser?IdUser=${data.idUsers}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataUsers.row(row).remove().draw();
                        swal("Exitoso!", "El usuario fué eliminado", "success");

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
// Función para reinicializar o actualizar el slider
function setearHorario(idSelector, horaEntrada, horaSalida) {
    // Función para convertir el formato HH:MM a minutos
    function convertirHorasAMinutos(hora) {
        var partes = hora.split(':');
        var horas = parseInt(partes[0], 10);
        var minutos = parseInt(partes[1], 10);
        return horas * 60 + minutos;
    }

    // Convertimos las horas a minutos
    var fromValue = convertirHorasAMinutos(horaEntrada);
    var toValue = convertirHorasAMinutos(horaSalida);

    // Verificar si el slider ya está inicializado
    var slider = $("#" + idSelector).data("ionRangeSlider");

    if (slider) {
        // Si ya está inicializado, lo actualizamos con los nuevos valores
        slider.update({
            from: fromValue,
            to: toValue
        });
    } else {
        // Inicializamos el slider si no está inicializado
        $("#" + idSelector).ionRangeSlider({
            type: "double",
            min: 0,
            max: 1440,           // 1440 minutos = 24 horas
            from: fromValue,      // Valor convertido para la hora de inicio
            to: toValue,          // Valor convertido para la hora de fin
            step: 30,             // Intervalo de 30 minutos
            grid: true,
            prettify: function (num) {
                var hours = Math.floor(num / 60);
                var minutes = num % 60;
                if (minutes < 10) minutes = '0' + minutes;
                return hours.toString().padStart(2, '0') + ':' + minutes.toString().padStart(2, '0');
            }
        });
    }
}



function obtenerHorariosSemana() {
    var horariosSemana = [];

    // Define los IDs de los sliders para cada día de la semana
    var dias = [
        { id: "timeRangeLunes", diaSemana: 1 },
        { id: "timeRangeMartes", diaSemana: 2 },
        { id: "timeRangeMiercoles", diaSemana: 3 },
        { id: "timeRangeJueves", diaSemana: 4 },
        { id: "timeRangeViernes", diaSemana: 5 },
        { id: "timeRangeSabado", diaSemana: 6 },
        { id: "timeRangeDomingo", diaSemana: 7 }
    ];

    // Itera sobre los días de la semana
    dias.forEach(function (dia) {
        var slider = $("#" + dia.id).data("ionRangeSlider");

        // Convertir el valor de minutos a formato HH:MM
        function convertirMinutosAHoras(minutos) {
            var hours = Math.floor(minutos / 60);
            var minutes = minutos % 60;
            if (minutes < 10) minutes = '0' + minutes;
            return hours.toString().padStart(2, '0') + ':' + minutes.toString().padStart(2, '0');
        }

        // Agregar el horario del día al array
        horariosSemana.push({
            diaSemana: dia.diaSemana,               // Día de la semana (1 = Lunes, 2 = Martes, etc.)
            horaEntrada: convertirMinutosAHoras(slider.result.from), // Hora de entrada en formato HH:MM
            horaSalida: convertirMinutosAHoras(slider.result.to)    // Hora de salida en formato HH:MM
        });
    });

    return horariosSemana;
}
