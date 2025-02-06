let tableDataPromociones;
let rowSelectedPromocion;
let categorySelector;
let productSelector;
let daysSelector;

const BASIC_MODEL_PROMOCION = {
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
    { value: 1, label: 'Lunes' },
    { value: 2, label: 'Martes' },
    { value: 3, label: 'Miercoles' },
    { value: 4, label: 'Jueves' },
    { value: 5, label: 'Viernes' },
    { value: 6, label: 'Sabado' },
    { value: 7, label: 'Domingo' }
]

$(document).ready(function () {


    tableDataPromociones = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Inventory/GetPromociones",
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
                        return '<span class="badge badge-info btn btn-cambiar-estado">Activo</span>';
                    else
                        return '<span class="badge badge-danger btn btn-cambiar-estado">Inactivo</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
                "orderable": false,
                "searchable": false
            }
        ],
        order: [[0, "desc"]],
        dom: "frtip"
    });


    cargarSelect2();
})


$("#btnNew").on("click", function () {
    openModalPromocion()
})

$(document).on('select2:open', () => {
    document.querySelector('.select2-search__field').focus();
});


const openModalPromocion = (model = BASIC_MODEL_PROMOCION) => {
    if (model.idPromocion != 0) {
        productSelector.setChoiceByValue(parseInt(model.idProducto));
        model.idCategory.forEach(cat => {
            categorySelector.setChoiceByValue(parseInt(cat));
        });
        model.dias.forEach(dia => {
            daysSelector.setChoiceByValue(dia.toString());
        });
    } else {
        productSelector.removeActiveItems();
        categorySelector.removeActiveItems();
        daysSelector.removeActiveItems();
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
        let dateTimeModif = new Date(model.modificationDate);

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
    let productSelected = productSelector.getValue(true);

    if (productSelected != null && ($("#cboOperador").val() == '' || $("#txtCantidad").val() == '')) { 
        const msg = `Debe completar los campos Operador y Cantidad`;
        toastr.warning(msg, "");
        return;
    }


    if (productSelected == null) {
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

    const model = structuredClone(BASIC_MODEL_PROMOCION);
    model["idPromocion"] = $("#txtId").val();
    model["nombre"] = $("#txtNombre").val();
    model["isActive"] = $("#cboState").val() === '1' ? true : false;
    model["operador"] = $("#cboOperador").val() != '' ? $("#cboOperador").val() : null;
    model["cantidadProducto"] = $("#txtCantidad").val() != '' ? $("#txtCantidad").val() : null;
    model["idProducto"] = productSelected;
    model["idCategory"] = $("#cboCategoria").val() != '' ? $("#cboCategoria").val() : null;
    model["dias"] = $("#cboDias").val() != '' ? $("#cboDias").val() : null;
    model["precio"] = $("#txtPrecio").val() != '' ? $("#txtPrecio").val() : null;
    model["porcentaje"] = $("#txtPorcentaje").val() != '' ? $("#txtPorcentaje").val() : null;

    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    if (model.idPromocion == 0) {
        fetch("/Inventory/CreatePromociones", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                tableDataPromociones.row.add(responseJson.object).draw(false);
                $("#modalData").modal("hide");
                swal("Exitoso!", "Promociones fué creada", "success");

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Inventory/UpdatePromociones", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                tableDataPromociones.row(rowSelectedPromocion).data(responseJson.object).draw(false);
                rowSelectedPromocion = null;
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

$("#tbData tbody").on("click", ".btn-cambiar-estado", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    let data = tableDataPromociones.row(row).data();

    if (data == undefined) {
        data = tableDataPromociones.row(row).data();
    }

    swal({
        title: "¿Desea cambiar el estado de la promocion? ",
        text: ` \n Nombre: ${data.nombre} \n\n ${data.promocionString}  \n  \n `,
        type: "warning",
        showCancelButton: true,
        confirmButtonClass: "btn-danger",
        confirmButtonText: "Si, cambiar estado",
        cancelButtonText: "No, cancelar",
        closeOnConfirm: false,
        closeOnCancel: true
    },
        function (respuesta) {

            if (respuesta) {

                $(".showSweetAlert").LoadingOverlay("show")

                fetch(`/Inventory/CambiarEstadoPromocion?idPromocion=${data.idPromocion}`, {
                    method: "PUT"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        location.reload()

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

$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedPromocion = $(this).closest('tr').prev();
    } else {
        rowSelectedPromocion = $(this).closest('tr');
    }

    const data = tableDataPromociones.row(rowSelectedPromocion).data();

    openModalPromocion(data);
})

$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataPromociones.row(row).data();

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

                fetch(`/Inventory/DeletePromociones?idPromocion=${data.idPromocion}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {

                        tableDataPromociones.row(row).remove().draw();
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


function cargarSelect2() {

    daysSelector = new Choices('#cboDias', {
        removeItemButton: true,
        maxItemCount: 7,
        searchResultLimit: 10,
        shouldSort: false,
        duplicateItemsAllowed: false
    });

    daysSelector.setChoices(dataDays, 'value', 'label', false);


    categorySelector = new Choices('#cboCategoria', {
        removeItemButton: true,
        maxItemCount: 3,
        searchResultLimit: 10,
        shouldSort: false,
        duplicateItemsAllowed: false
    });

    fetch('/Inventory/GetCategoriesActive')
        .then(response => response.json())
        .then(data => {
            const cats = data.data.map(cat => ({
                value: cat.idCategory,
                label: cat.description
            }));
            categorySelector.setChoices(cats, 'value', 'label', false);
        });

    productSelector = new Choices('#cboProducto', {
        removeItemButton: true,
        maxItemCount: 1,
        searchResultLimit: 3,
        shouldSort: false,
        duplicateItemsAllowed: false,
        allowHTML: true
    });

    showLoading();

    fetch('/Inventory/GetProductsActive')
        .then(response => response.json())
        .then(data => {
            removeLoading();

            const prods = data.data.map(prod => ({
                value: prod.idProduct,
                label: `${prod.description}&nbsp;&nbsp;&nbsp;&nbsp;$${prod.price}`
            }));
            productSelector.setChoices(prods, 'value', 'label', false);
        });
}