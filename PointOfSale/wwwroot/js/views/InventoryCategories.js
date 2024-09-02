let tableDataCategory;
let rowSelectedCategory;
let rowSelectedTag;
let tableTags;

const BASIC_MODEL_CATEGORIA = {
    idCategory: 0,
    description: "",
    isActive: 1,
    modificationDate: null,
    modificationUser: null
}


$(document).ready(function () {


    tableDataCategory = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetCategories",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idCategory",
                "visible": false,
                "searchable": false
            },
            { "data": "description" },
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
                filename: 'Reporte categories',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });

    if (!$.fn.DataTable.isDataTable('#tbTags')) {
        tableTags = $("#tbTags").DataTable({
            responsive: true,
            "ajax": {
                "url": "/Inventory/GetTags",
                "type": "GET",
                "datatype": "json"
            },
            "columns": [
                {
                    "data": "idTag",
                    "visible": false,
                    "searchable": false
                },
                { "data": "nombre" },
                {
                    "data": "color",
                    "render": function (data, type, row) {
                        return `<div style="width: 30px; height: 30px; background-color: ${data}; border-radius: 50%;"></div>`;
                    },
                    "orderable": false,
                    "searchable": false,
                    "width": "40px"
                },
                {
                    "defaultContent": '<button class="btn btn-primary btn-edit-tag btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                        '<button class="btn btn-danger btn-delete-tag btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                    "orderable": false,
                    "searchable": false,
                    "width": "80px"
                }
            ],
            order: [[1, "desc"]],
            dom: "lfrtip",  // Remove buttons but keep search, length, etc.
        });
    }
})

const openModalCategory = (model = BASIC_MODEL_CATEGORIA) => {
    $("#txtId").val(model.idCategory);
    $("#txtDescription").val(model.description);
    $("#cboState").val(model.isActive);

    if (model.modificationUser == null)
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
    openModalCategory()
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

    const model = structuredClone(BASIC_MODEL_CATEGORIA);
    model["idCategory"] = parseInt($("#txtId").val());
    model["description"] = $("#txtDescription").val();
    model["isActive"] = $("#cboState").val();


    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idCategory == 0) {
        fetch("/Inventory/CreateCategory", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataCategory.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "La categoria fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Inventory/UpdateCategory", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataCategory.row(rowSelectedCategory).data(responseJson.object).draw(false);
                rowSelectedCategory = null;
                $("#modalData").modal("hide");
                swal("Exitoso!", "La categoria fué modificada", "success");

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
        rowSelectedCategory = $(this).closest('tr').prev();
    } else {
        rowSelectedCategory = $(this).closest('tr');
    }

    const data = tableDataCategory.row(rowSelectedCategory).data();

    openModalCategory(data);
})



$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataCategory.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar categoria "${data.description}"`,
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

                fetch(`/Inventory/DeleteCategory?idCategory=${data.idCategory}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataCategory.row(row).remove().draw();
                        swal("Exitoso!", "Categoria fué eliminada", "success");

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

$('#btnAddNewTag').on('click', function () {
    $('#txtTagName').val('');
    $('#txtTagColor').val('');

    if (tableTags) {
        tableTags.ajax.reload(null, false); // Recarga la tabla sin restablecer la paginación
    }
    $('#modalTagData').modal('show');
});

$('#btnSaveTag').on('click', function () {
    let tagData = {
        idTag: parseInt($('#txtTagId').val()),
        nombre: $('#txtTagName').val(),
        color: $('#txtTagColor').val()
    };


    if (tagData.idTag == 0) {
        fetch("/Inventory/CreateTag", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableTags.row.add(responseJson.object).draw(false);
                    swal("Exitoso!", "El tag fue creado con éxito.", "success");
                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al crear el tag.", "error");
                console.error('Error:', error);
            });
    } else {

        fetch("/Inventory/UpdateTag", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(tagData)
        }).then(response => response.json())
            .then(responseJson => {
                if (responseJson.state) {
                    tableTags.row(rowSelectedTag).data(responseJson.object).draw(false);
                    rowSelectedTag = null;
                    swal("Exitoso!", "El tag fue actualizado con éxito.", "success");
                } else {
                    swal("Lo sentimos", responseJson.message, "error");
                }
            }).catch((error) => {
                swal("Error", "Ocurrió un error al actualizar el tag.", "error");
                console.error('Error:', error);
            });
    }
    $('#txtTagName').val('');
    $('#txtTagColor').val('');
    $('#txtTagId').val(0);
})

$('#tbTags').on('click', '.btn-delete-tag', function () {
    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableTags.row(row).data();

    swal({
        title: "¿Está seguro?",
        text: `Eliminar tag "${data.nombre}"`,
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

                fetch(`/Inventory/DeleteTag?idTag=${data.idTag}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableTags.row(row).remove().draw();
                        swal("Exitoso!", "Tag fué eliminada", "success");

                    } else {
                        swal("Lo sentimos", responseJson.message, "error");
                    }
                })
                    .catch((error) => {
                        $(".showSweetAlert").LoadingOverlay("hide")
                    })
            }
        });
});

$('#tbTags').on('click', '.btn-edit-tag', function () {
    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTag = $(this).closest('tr').prev();
    } else {
        rowSelectedTag = $(this).closest('tr');
    }

    const data = tableTags.row(rowSelectedTag).data();

    $('#txtTagName').val(data.nombre);
    $('#txtTagColor').val(data.color);
    $('#txtTagId').val(data.idTag);
});