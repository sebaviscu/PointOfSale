let tableData;
let rowSelected;

const BASIC_MODEL = {
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
    modificationUser: null
}


$(document).ready(function () {

    fetch("/Admin/GetRoles")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboRol").append(
                        $("<option>").val(item.idRol).text(item.description)
                    )
                });
            }
        })

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboTiendas").append(
                $("<option>").val('-1').text(''))

            if (responseJson.data.length > 0) {
                responseJson.data.forEach((item) => {
                    $("#cboTiendas").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })

    tableData = $("#tbData").DataTable({
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

})

const openModal = (model = BASIC_MODEL) => {
    var rol = model.idRol == 0 ? $("#cboRol option:first").val() : model.idRol;
    var tienda = model.idTienda == 0 ? $("#cboTiendas option:first").val() : model.idTienda;
    $("#txtId").val(model.idUsers);
    $("#txtName").val(model.name);
    $("#txtEmail").val(model.email);
    $("#txtPhone").val(model.phone);
    $("#cboRol").val(rol);
    $("#cboState").val(model.isActive);
    $("#txtPassWord").val(model.password);
    $("#cboTiendas").val(tienda);
    //$("#txtPhoto").val("");
    //$("#imgUser").attr("src", `data:image/png;base64,${model.photoBase64}`);

    var rol = $('#cboRol').val();
    $("#cboTiendas").prop("disabled", rol == '1');

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalData").modal("show")

}

$("#btnNewUser").on("click", function () {
    openModal()
})

$('#cboRol').change(function () {
    var rol = $(this).val();

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

    var rol = $("#cboRol").val();
    var tienda = $("#cboTiendas").val();

    if (rol !== '1' && tienda === '-1') {
        toastr.warning("Se debe seleccionar una tienda", "");
        return;
    }
    else
    if (rol === '1' &&  tienda !== '-1') {
        toastr.warning("No se debe seleccionar tienda para un Administrador", "");
        return;
    }

    const model = structuredClone(BASIC_MODEL);
    model["idUsers"] = parseInt($("#txtId").val());
    model["name"] = $("#txtName").val();
    model["email"] = $("#txtEmail").val();
    model["phone"] = $("#txtPhone").val();
    model["idRol"] = rol
    model["password"] = $("#txtPassWord").val();
    model["isActive"] = $("#cboState").val();
    model["idTienda"] = tienda;
    //const inputPhoto = document.getElementById('txtPhoto');

    const formData = new FormData();
    //formData.append('photo', inputPhoto.files[0]);
    formData.append('model', JSON.stringify(model));

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idUsers == 0) {
        fetch("/Admin/CreateUser", {
            method: "POST",
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                tableData.row.add(responseJson.object).draw(false);
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
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelected).data(responseJson.object).draw(false);
                rowSelected = null;
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
        rowSelected = $(this).closest('tr').prev();
    } else {
        rowSelected = $(this).closest('tr');
    }

    const data = tableData.row(rowSelected).data();

    openModal(data);
})



$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableData.row(row).data();

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
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
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

