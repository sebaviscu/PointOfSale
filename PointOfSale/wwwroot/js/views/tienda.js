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
                        tiendaActualBadge = '<span class="mdi mdi-star"  data-bs-toggle="tooltip" data-bs-placement="bottom" title="Actual"></span>';
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

})


const openModalTienda = (model = BASIC_MODEL_TIENDA) => {


    $("#txtId").val(model.idTienda);
    $("#txtNombre").val(model.nombre);
    $("#cboListaPrecios").val(model.idListaPrecio);

    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);

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

    //const inputLogo = document.getElementById('fileLogo');

    const formData = new FormData();
    //formData.append('Logo', inputLogo.files[0]);
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
            $("#modalData").modal("hide");

            if (responseJson.state) {

                $("#modalData").modal("hide");
                swal("Exitoso!", "Punto de venta fué modificado", "success");
                location.reload();
            }
            else {
                rowSelectedTienda = null;
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

    openModalTienda(data);
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
