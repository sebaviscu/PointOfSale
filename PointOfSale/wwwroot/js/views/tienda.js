let tableData;
let rowSelectedTienda;

const BASIC_MODEL = {
    idTienda: 0,
    nombre: "",
    idListaPrecio: 1,
    modificationDate: null,
    modificationUser: null,
    nombre: "",
    email: "",
    telefono: "",
    direccion: "",
    nombreImpresora: "",
    logo: "",
    cuit: ""
}

let isHealthy = false;

$(document).ready(function () {
    $("#general").show();
    $("#facturacion").hide();
    $("#carroCompras").hide();

    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Tienda/GetTienda",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTienda",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            { "data": "telefono" },
            { "data": "direccion" },
            {
                "data": "idListaPrecio", render: function (data) {
                    return 'Lista ' + data;
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Tiendas',
                exportOptions: {
                    columns: [1, 2, 4, 5]
                }
            }, 'pageLength'
        ]
    });

    $("#cboNombreTienda").append(
        $("<option>").val('').text('')
    )


    var $passwordInput = $('#txtContraseñaCertificado');
    var $togglePasswordButton = $('#togglePassword');

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

    healthcheck();

    $('#submitFile').on('click', function () {
        var fileInput = $('#txtRutaCertificado')[0];
        if (fileInput.files.length > 0) {
            var file = fileInput.files[0];
            console.log('Archivo seleccionado:', file.name);

            // Si necesitas enviar el archivo al servidor:
            var formData = new FormData();
            formData.append('file', file);



        } else {
            alert('Por favor, selecciona un archivo.');
        }
    });
})


async function healthcheck() {
    isHealthy = await getHealthcheck();

    if (isHealthy) {
        document.getElementById("lblErrorPrintService").style.display = 'none';
    } else {
        document.getElementById("lblErrorPrintService").style.display = '';
    }
}

async function getPrintersTienda() {
    try {
        let printers = await getPrinters();

        printers.forEach(printer => {
            $("#cboNombreTienda").append(
                $("<option>").val(printer).text(printer)
            );
        });
    } catch (error) {
        console.error('Error fetching printers:', error);
    }
}

const openModal = (model = BASIC_MODEL) => {


    $("#txtId").val(model.idTienda);
    $("#txtNombre").val(model.nombre);
    $("#cboListaPrecios").val(model.idListaPrecio);
    $("#cboNombreTienda").val(model.nombreImpresora);

    $("#txtEmail").val(model.email);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#txtCuit").val(model.cuit);

    $("#imgTienda").attr("src", `data:image/png;base64,${model.photoBase64}`);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#modalData").modal("show")

}

$("#btnNew").on("click", function () {
    openModal()
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

    const model = structuredClone(BASIC_MODEL);
    model["idTienda"] = parseInt($("#txtId").val());
    model["nombre"] = $("#txtNombre").val();
    model["idListaPrecio"] = parseInt($("#cboListaPrecios").val());

    model["telefono"] = $("#txtTelefono").val();
    model["email"] = $("#txtEmail").val();
    model["direccion"] = $("#txtDireccion").val();
    model["nombreImpresora"] = $("#cboNombreTienda").val();
    model["cuit"] = $("#txtCuit").val();

    //const inputPhoto = document.getElementById('txtLogo');
    //model["photo"] = inputPhoto.files[0];

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    const formData = new FormData();
    //formData.append('photo', inputPhoto.files[0]);
    formData.append('model', JSON.stringify(model));

    if (model.idTienda == 0) {
        fetch("/Tienda/CreateTienda", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableData.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "La tienda fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Tienda/UpdateTienda", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                $("#modalData").modal("hide");

                if (responseJson.message != '') {
                    swal({
                        title: 'La tienda fué modificada',
                        text: responseJson.message,
                        showCancelButton: false,
                        closeOnConfirm: false
                    }, function (value) {

                        document.location.href = "/";

                    });

                }
                else {
                    tableData.row(rowSelectedTienda).data(responseJson.object).draw(false);
                    rowSelectedTienda = null;
                    swal("Exitoso!", "La tienda fué modificada", "success");
                }


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
        rowSelectedTienda = $(this).closest('tr').prev();
    } else {
        rowSelectedTienda = $(this).closest('tr');
    }

    const data = tableData.row(rowSelectedTienda).data();

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
        title: "¿Está seguro?",
        text: `Eliminar tienda "${data.nombre}"`,
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

                fetch(`/Tienda/DeleteTienda?idTienda=${data.idTienda}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "Tienda fué eliminada", "success");

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

$("#clickGeneral").on("click", function () {
    $("#general").show();
    $("#facturacion").hide();
    $("#carroCompras").hide();
})

$("#clickFacturacion").on("click", function () {
    $("#general").hide();
    $("#facturacion").show();
    $("#carroCompras").hide();
})

$("#clickWeb").on("click", function () {
    $("#general").hide();
    $("#facturacion").hide();
    $("#carroCompras").show();
})
