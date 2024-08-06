let tableData;
let rowSelectedTienda;

const BASIC_MODEL_TIENDA = {
    idTienda: 0,
    nombre: "",
    idListaPrecio: 1,
    modificationDate: null,
    modificationUser: null,
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
            {
                "data": "nombre", render: function (data, type, row) {
                    let tiendaActualBadge = '';
                    if (row.tiendaActual == 1) {
                        tiendaActualBadge = '<span class="badge badge-info">Actual</span>';
                    }
                    return `${data} ${tiendaActualBadge}`;
                }
            },
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

    $("#cboNombreImpresora").append(
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

    //healthcheck();
})

//async function healthcheck() {
//    isHealthy = await getHealthcheck();

//    if (isHealthy) {
//        document.getElementById("lblErrorPrintService").style.display = 'none';
//    } else {
//        document.getElementById("lblErrorPrintService").style.display = '';
//    }
//}

//async function getPrintersTienda() {
//    try {
//        let printers = await getPrinters();

//        printers.forEach(printer => {
//            $("#cboNombreImpresora").append(
//                $("<option>").val(printer).text(printer)
//            );
//        });
//    } catch (error) {
//        console.error('Error fetching printers:', error);
//    }
//}

const openModalTienda = (model = BASIC_MODEL_TIENDA) => {


    $("#txtId").val(model.idTienda);
    $("#txtNombre").val(model.nombre);
    $("#cboListaPrecios").val(model.idListaPrecio);
    $("#cboNombreImpresora").val(model.nombreImpresora);

    $("#txtEmail").val(model.email);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);

    if (model.vMX509Certificate2 != null) {
        $("#txtFechaIniCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notBefore));
        $("#txtFechaCadCert").val(formatDateToDDMMYYYY(model.vMX509Certificate2.notAfter));
        $("#txtSubjectCert").val(model.vMX509Certificate2.cuil);
    }

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

function formatDateToDDMMYYYY(isoDate) {
    const date = new Date(isoDate);

    // Obtener los componentes de la fecha
    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

    // Formatear la fecha como dd/mm/yyyy
    return `${day}/${month}/${year}`;
}
$("#btnNew").on("click", function () {
    openModalTienda()
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

    const model = structuredClone(BASIC_MODEL_TIENDA);
    model["idTienda"] = parseInt($("#txtId").val());
    model["nombre"] = $("#txtNombre").val();
    model["idListaPrecio"] = parseInt($("#cboListaPrecios").val());

    model["telefono"] = $("#txtTelefono").val();
    model["email"] = $("#txtEmail").val();
    model["direccion"] = $("#txtDireccion").val();
    model["nombreImpresora"] = $("#cboNombreImpresora").val();
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

                if (responseJson.state) {
                    
                    $("#modalData").modal("hide");
                    swal("Exitoso!", "Punto de venta fué modificado", "success");
                    location.reload();
                }
                else {
                    rowSelectedTienda = null;
                    swal("Exitoso!", "El Punto de venta fué modificado", "success");
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

    showLoading();

    fetch(`/Tienda/GetTiendoWithCertificado?idTienda=${data.idTienda}`)
        .then(response => {
            return response.json();
        }).then(responseJson => {
            removeLoading();
            if (responseJson.state) {

                openModalTienda(responseJson.object);

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
                        swal("Exitoso!", "Punto de venta fué eliminado", "success");

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
