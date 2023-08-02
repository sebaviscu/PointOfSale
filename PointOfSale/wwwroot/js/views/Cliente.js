let tableData;
let rowSelected;

const BASIC_MODEL = {
    idCliente: 0,
    nombre: '',
    cuil: null,
    telefono: null,
    direccion: null,
    registrationDate: null,
    modificationDate: null,
    modificationUser: null
}


$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetCliente",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idCliente",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            { "data": "total" },
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
                filename: 'Reporte Clientes',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });
})

const openModal = (model = BASIC_MODEL) => {
    $("#txtId").val(model.idCliente);
    $("#txtNombre").val(model.nombre);
    $("#txtCuil").val(model.cuil);
    $("#txtDireccion").val(model.direccion);
    $("#txtTelefono").val(model.telefono);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $('#tbMovimientos tbody').html("")

    fetch("/Admin/GetMovimientoCliente?idCliente=" + model.idCliente, {
        method: "GET",
        headers: { 'Content-Type': 'application/json;charset=utf-8' }
    }).then(response => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        var totalCli = 0;
        if (responseJson.data.length > 0) {

            responseJson.data.forEach((item) => {

                totalCli = totalCli + parseFloat(item.total);

                $('#tbMovimientos tbody').append(
                    $("<tr>").append(
                        $("<td>").text("$ " + item.total),
                        $("<td>").text(item.registrationDate),
                        $("<td>").text(item.registrationUser),
                        $("<td>").append(
                            $("<button>").addClass("btn btn-danger btn-delete btn-sm").append(
                                $("<i>").addClass("mdi mdi-trash-can")
                            ).data("idSale", item.idSale)
                        )
                    )
                )

            })

        }
    }).catch((error) => {
    })

    //var url = '/Admin/GetMovimientoCliente?idCliente=' + model.idCliente;
    //tableData = $("#tbMovimientos").DataTable({
    //    responsive: true,
    //    "ajax": {
    //        "url": url,
    //        "type": "GET",
    //        "datatype": "json"
    //    },
    //    "columns": [
    //        {
    //            "data": "idCliente",
    //            "visible": false,
    //            "searchable": false
    //        },
    //        { "data": "total" },
    //        { "data": "registrationDate" },
    //        { "data": "registrationUser" },
    //        {
    //            "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>',
    //            "orderable": false,
    //            "searchable": false,
    //            "width": "80px"
    //        }
    //    ],
    //    order: [[0, "desc"]],
    //    dom: "Bfrtip",
    //    buttons: [
    //        {
    //            text: 'Exportar Excel',
    //            extend: 'excelHtml5',
    //            title: '',
    //            filename: 'Reporte Clientes',
    //            exportOptions: {
    //                columns: [1, 2]
    //            }
    //        }, 'pageLength'
    //    ]
    //});

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
    model["nombre"] = $("#txtNombre").val();
    model["direccion"] = $("#txtDireccion").val();
    model["cuil"] = $("#txtCuil").val();
    model["telefono"] = $("#txtTelefono").val();
    model["idCliente"] = $("#txtId").val();

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idCliente == 0) {
        fetch("/Admin/CreateCliente", {
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
                swal("Exitoso!", "Cliente fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdateCliente", {
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
                swal("Exitoso!", "Cliente fué modificada", "success");

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
        text: `Eliminar Cliente "${data.nombre}"`,
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

                fetch(`/Admin/DeleteCliente?idCliente=${data.idCliente}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "Cliente  fué eliminada", "success");

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