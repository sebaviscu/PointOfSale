let tableData;
let rowSelected;

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
    montoEnvioGratis: 0,
    aumentoWeb: 0,
    whatsapp: "",
    lunes: "",
    martes: "",
    miercoles: "",
    jueves: "",
    viernes: "",
    sabado: "",
    domingo: "",
    feriado: "",
    facebook: "",
    instagram: "",
    twitter: "",
    tiktok: "",
    youtube: "",
    principal: 0
}


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
                "data": "principal", render: function (data) {
                    if (data == 1)
                        return '<input type="checkbox" checked disabled>';
                    else
                        return '<input type="checkbox" disabled>';
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
                    columns: [1, 2,4,5]
                }
            }, 'pageLength'
        ]
    });
})

const openModal = (model = BASIC_MODEL) => {
    $("#txtId").val(model.idTienda);
    $("#txtNombre").val(model.nombre);
    $("#cboListaPrecios").val(model.idListaPrecio);

    $("#txtEmail").val(model.email);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#txtImpresora").val(model.nombreImpresora);
    $("#txtEnvioGratis").val(model.montoEnvioGratis);
    $("#txtAumento").val(model.aumentoWeb);
    $("#txtWhatsApp").val(model.whatsapp);
    $("#txtLunes").val(model.lunes);
    $("#txtMartes").val(model.martes);
    $("#txtMiercoles").val(model.miercoles);
    $("#txtJueves").val(model.jueves);
    $("#txtViernes").val(model.viernes);
    $("#txtSabado").val(model.sabado);
    $("#txtDomingo").val(model.domingo);
    $("#txtFeriados").val(model.feriado);
    $("#txtFacebook").val(model.facebook);
    $("#txtInstagram").val(model.instagram);
    $("#txtTikTok").val(model.tiktok);
    $("#txtTwitter").val(model.twitter);
    $("#txtYouTube").val(model.youtube);
    $("#imgTienda").attr("src", `data:image/png;base64,${model.photoBase64}`);
    document.getElementById("cboPrincipal").checked = model.principal;

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

$("#btnNew").on("click", function () {
    openModal()
})
$("#bntVerImpresoras").on("click", function () {

    fetch(`/Tienda/GetImpresoras`, {
        method: "GET"
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.state) {

            alert(responseJson.object);
            console.log(responseJson.object);
        }
    })
        .catch((error) => {
        })
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
    model["nombreImpresora"] = $("#txtImpresora").val();
    model["montoEnvioGratis"] = $("#txtEnvioGratis").val();
    model["aumentoWeb"] = $("#txtAumento").val();
    model["whatsapp"] = $("#txtWhatsApp").val();
    model["lunes"] = $("#txtLunes").val();
    model["martes"] = $("#txtMartes").val();
    model["miercoles"] = $("#txtMiercoles").val();
    model["jueves"] = $("#txtJueves").val();
    model["viernes"] = $("#txtViernes").val();
    model["sabado"] = $("#txtSabado").val();
    model["domingo"] = $("#txtDomingo").val();
    model["feriado"] = $("#txtFeriados").val();
    model["facebook"] = $("#txtFacebook").val();
    model["instagram"] = $("#txtInstagram").val();
    model["tiktok"] = $("#txtTikTok").val();
    model["twitter"] = $("#txtTwitter").val();
    model["youtube"] = $("#txtYouTube").val();
    model["principal"] = document.querySelector('#cboPrincipal').checked;

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
            return response.ok ? response.json() : Promise.reject(response);
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
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.state) {

                tableData.row(rowSelected).data(responseJson.object).draw(false);
                rowSelected = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "La tienda fué modificada", "success");

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
                    return response.ok ? response.json() : Promise.reject(response);
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