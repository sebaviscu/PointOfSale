let tableDataTienda;
let rowSelectedTienda;

const BASIC_MODEL_TIENDA = {
    idTienda: 0,
    nombre: "",
    idListaPrecio: 1,
    modificationDate: null,
    modificationUser: null,
    telefono: "",
    direccion: "",
    logo: "",
    cuit: 0,
    condicionIva: null,
    puntoVenta: null,
    certificadoPassword: ""
}

let isHealthy = false;

$(document).ready(function () {
    $("#general").show();
    $("#facturacion").hide();
    $("#carroCompras").hide();

    tableDataTienda = $("#tbData").DataTable({
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
                        tiendaActualBadge = '<span class="mdi mdi-star"></span>';
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
    let $passwordInput = $('#txtContraseñaCertificado');
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


const openModalTienda = (model = BASIC_MODEL_TIENDA) => {


    $("#txtId").val(model.idTienda);
    $("#txtNombre").val(model.nombre);
    $("#cboListaPrecios").val(model.idListaPrecio);

    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#cboCondicionIva").val(model.condicionIva);
    $("#txtPuntoVenta").val(model.puntoVenta);
    $("#txtContraseñaCertificado").val(model.certificadoPassword);

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

    const day = date.getDate().toString().padStart(2, '0');
    const month = (date.getMonth() + 1).toString().padStart(2, '0');
    const year = date.getFullYear();

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
    model["direccion"] = $("#txtDireccion").val();
    model["puntoVenta"] = parseInt($("#txtPuntoVenta").val());
    model["condicionIva"] = parseInt($("#cboCondicionIva").val());
    model["cuit"] = parseInt($("#txtSubjectCert").val()); // viene del certificado
    model["certificadoPassword"] = $("#txtContraseñaCertificado").val();
    
    const inputCertificado = document.getElementById('fileCertificado');
    const inputLogo = document.getElementById('fileLogo');

    const formData = new FormData();
    formData.append('Certificado', inputCertificado.files[0]);
    formData.append('Logo', inputLogo.files[0]);
    formData.append('model', JSON.stringify(model));

    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    const url = model.idTienda == 0 ? "/Tienda/CreateTienda" : "/Tienda/UpdateTienda";
    const method = model.idTienda == 0 ? "POST" : "PUT";

    if (model.idTienda == 0) {
        fetch(url, {
            method: method,
            body: formData
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataTienda.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "La tienda fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch(url, {
            method: method,
            body: formData
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

    const data = tableDataTienda.row(rowSelectedTienda).data();

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
    const data = tableDataTienda.row(row).data();

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

                        tableDataTienda.row(row).remove().draw();
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
