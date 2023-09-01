let tableData;

const BASIC_MODEL = {
    idPromocion: 0,
    nombre: '',
    idProducto: null,
    operador: null,
    cantidadProducto: null,
    idCategory: null,
    dias: null,
    precio: null,
    porcentaje: null,
    isActive: 1,
    modificationDate: null,
    modificationUser: null
}

const dataDays = [
    { id: 1, text: 'Lunes' },
    { id: 2, text: 'Martes' },
    { id: 3, text: 'Miercoles' },
    { id: 4, text: 'Jueves' },
    { id: 5, text: 'Viernes' },
    { id: 6, text: 'Sabado' },
    { id: 7, text: 'Domingo' }
]

$(document).ready(function () {
    $('#cboProducto').select2({
        ajax: {
            url: "/Sales/GetProducts",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idProduct,
                            text: item.description,

                            brand: item.brand,
                            category: item.nameCategory,
                            photoBase64: item.photoBase64,
                            price: parseFloat(item.price),
                            tipoVenta: item.tipoVenta
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 1,
        templateResult: formatResults,
        allowClear: true,
        dropdownParent: $('#modalData .modal-content')
    });

    $('#cboCategoria').select2({
        ajax: {
            url: "/Inventory/GetCategoriesSearch",
            dataType: 'json',
            contentType: "application/json; charset=utf-8",
            delay: 250,
            data: function (params) {
                return {
                    search: params.term
                };
            },
            processResults: function (data) {
                return {
                    results: data.map((item) => (
                        {
                            id: item.idCategory,
                            text: item.description,
                        }
                    ))
                };
            }
        },
        allowClear: true,
        multiple: true,
        placeholder: 'Buscar categorias...',
        debug: true,
        minimumInputLength: 1,
        dropdownParent: $('#modalData .modal-content')
    });

    $('#cboDias').select2({
        allowClear: true,
        debug: true,
        data: dataDays,
        multiple: true,
        placeholder: 'Buscar dias...',
        dropdownParent: $('#modalData .modal-content')
    });

    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Admin/GetPromociones",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idPromocion",
                "visible": false,
                "searchable": false
            },
            { "data": "nombre" },
            { "data": "promocionString" },
            {
                "data": "isActive", render: function (data) {
                    if (data == 1)
                        return '<span class="badge badge-info">Activo</span>';
                    else
                        return '<span class="badge badge-danger">Inactivo</span>';
                }
            },
            {
                //"defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                "defaultContent": '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "110px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Promociones',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });
})

function formatResults(data) {

    if (data.loading)
        return data.text;

    var container = $(
        `<table width="90%">
            <tr>
                <td style="width:60px">
                    <img style="height:60px;width:60px;margin-right:10px" src="data:image/png;base64,${data.photoBase64}"/>
                </td>
                <td>
                    <p style="font-weight: bolder;margin:2px">${data.text}</p>
                    <p>$ ${data.price}</p>
                </td>
            </tr>
         </table>`
    );

    return container;
}


$("#btnNew").on("click", function () {
    openModal()
})

$(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
});


const openModal = (model = BASIC_MODEL) => {
    if (model.idPromocion != 0) {
        //$("#cboProducto").select2().val(model.idProducto).trigger("change");
        //$("#cboCategoria").select2().val(model.idCategory).trigger("change");
        $("#cboDias").select2().val(model.dias).trigger("change");
    }
    else {
        $("#cboDias").val(null).trigger("change");
        $("#cboCategoria").val(null).trigger("change");
        $("#cboProducto").val(null).trigger("change");
    }

    $("#txtId").val(model.idPromocion);
    $("#txtNombre").val(model.nombre);
    $("#cboState").val(model.isActive ? 1 : 0);
    $("#cboOperador").val(model.operador);
    $("#txtCantidad").val(model.cantidadProducto);
    $("#txtPrecio").val(model.precio);
    $("#txtPorcentaje").val(model.porcentaje);

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


$("#btnSave").on("click", function () {
    const inputs = $("input.input-validate").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    //if ($("#cboProducto").val() != null && ($("#cboOperador").val() == '' || $("#txtCantidad").val() == '')) { 
    //    const msg = `Debe completar los campos Operador y Cantidad`;
    //    toastr.warning(msg, "");
    //    return;
    //}


    if ($("#cboProducto").val() == null) {
        $("#cboOperador").val('');
        $("#txtCantidad").val('');
    }

    if ($("#txtPorcentaje").val() != '' && $("#txtPrecio").val() != '') {
        const msg = `Solo es posible completar un campo de PRECIO o PORCENTAJE`;
        toastr.warning(msg, "");
        return;
    }

    if ($("#txtPorcentaje").val() == '' && $("#txtPrecio").val() == '') {
        const msg = `Debe completar al menos un campo de PRECIO o PORCENTAJE`;
        toastr.warning(msg, "");
        return;
    }

    const model = structuredClone(BASIC_MODEL);
    model["idPromocion"] = $("#txtId").val();
    model["nombre"] = $("#txtNombre").val();
    model["isActive"] = $("#cboState").val() === '1' ? true : false;
    model["operador"] = $("#cboOperador").val() != '' ? $("#cboOperador").val() : null;
    model["cantidadProducto"] = $("#txtCantidad").val() != '' ? $("#txtCantidad").val() : null;
    model["idProducto"] = $("#cboProducto").val() != '' ? $("#cboProducto").val() : null;
    model["idCategory"] = $("#cboCategoria").val() != '' ? $("#cboCategoria").val() : null;
    model["dias"] = $("#cboDias").val() != '' ? $("#cboDias").val() : null;
    model["precio"] = $("#txtPrecio").val() != '' ? $("#txtPrecio").val() : null;
    model["porcentaje"] = $("#txtPorcentaje").val() != '' ? $("#txtPorcentaje").val() : null;

    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    if (model.idPromocion == 0) {
        fetch("/Admin/CreatePromociones", {
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
                swal("Exitoso!", "Promociones fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Admin/UpdatePromociones", {
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
                swal("Exitoso!", "Promociones fué modificada", "success");

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
        text: `Eliminar Promociones "${data.nombre}"`,
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

                fetch(`/Admin/DeletePromociones?idPromocion=${data.idPromocion}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.ok ? response.json() : Promise.reject(response);
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableData.row(row).remove().draw();
                        swal("Exitoso!", "Promociones  fué eliminada", "success");

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