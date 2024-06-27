let tableData;
let rowSelected;

const BASIC_MODEL = {
    idVentaWeb: 0,
    description: "",
    nombre: "",
    direccion: "",
    telefono: "",
    comentario: "",
    totalString: "",
    formaDePago: null,
    estado: 0,
    detailSales: null,
    idTienda: 0,
    registrationDate: null,
    modificationDate: null,
    modificationUser: null,
    isEdit: null,
    editUser: null,
    editText: null,
    editDate: null
}


//$(document).on('select2:open', () => {
//    document.querySelector('.select2-search__field').focus();
//});

$(document).ready(function () {
    $('[data-toggle="popover"]').popover({
        html: true,
        trigger: 'focus' // Se activará cuando se haga clic y desaparecerá cuando se haga clic fuera
    });


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Shop/GetVentasWeb",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idVentaWeb",
                "visible": false,
                "searchable": false
            },
            { "data": "fecha" },
            { "data": "nombre" },
            { "data": "direccion" },
            { "data": "telefono" },
            { "data": "totalString" },
            {
                "data": "estado", render: function (data) {
                    if (data == 0)
                        return '<span class="badge badge-info">Nueva</span>';
                    else if (data == 1)
                        return '<span class="badge badge-success">Finalizada</span>';
                    else if (data == 2)
                        return '<span class="badge badge-danger">Cerrada</span>';
                }
            },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "60px"
            }
        ],
        order: [[0, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Ventas Web',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboTienda").append(
                        $("<option>").val(item.idTienda).text(item.nombre)
                    )
                });
            }
        })

})


$("#printTicket").click(function () {
    let idVentaWeb = parseInt($("#txtId").val());

    fetch(`/Sales/PrintTicketVentaWeb?idVentaWeb=${idVentaWeb}`)
        .then(response => {

            if (response.state) {
                $("#modalData").modal("hide");

                if (response.object.nombreImpresora != '') {

                    printTicket(response.object.nombreImpresora, responseJson.object.ticket);

                    swal("Exitoso!", "Ticket impreso!", "success");
                }

            } else {
                swal("Lo sentimos", "La venta no fué registrada. Error: " + response.message, "error");
            }
        })

})

const openModal = (model = BASIC_MODEL) => {

    $('#modalData').modal('show');

    
    document.querySelector('#txtNombre').disabled = true;
    document.querySelector('#txtTelefono').disabled = true;
    document.querySelector('#txtDireccion').disabled = true;
    document.querySelector('#txtFormaPago').disabled = true;
    document.querySelector('#txtComentario').disabled = true;

    $("#txtId").val(model.idVentaWeb);
    $("#txtFecha").val(model.fecha);
    $("#txtTotal").val(model.totalString);
    $("#txtNombre").val(model.nombre);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#txtFormaPago").val(model.formaDePago);
    $("#txtComentario").val(model.comentario);
    $("#cboState").val(model.estado);
    $("#cboTienda").val(model.idTienda);

    if (model.isEdit) {

        select2Modal();

        $('#popoverEdit').attr('data-bs-content', model.editText);

        bootstrap.Popover.getInstance(document.getElementById('popoverEdit'));
        popover.setContent({
            '.popover-body': 'Updated content'
        });
    }

    document.querySelector('#popoverEdit').disabled = model.isEdit;

    document.querySelector('#cboState').disabled = model.estado > 0;
    document.querySelector('#cboTienda').disabled = model.estado > 0;

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

    $("#tbProducts tbody").html("")

    model.detailSales.forEach((item) => {
        $("#tbProducts tbody").append(
            $("<tr>").append(
                $("<td>").text(item.descriptionProduct),
                $("<td>").text(`${item.quantity} / ${item.tipoVentaString}`),
                $("<td>").text(`$ ${Number.parseFloat(item.price).toFixed(2)}`),
                $("<td>").text(`$ ${Number.parseFloat(item.total).toFixed(2)}`)
            )
        )
    })

    $("#modalData").modal("show")

}

$("#btnEdit").click(function () {

    document.querySelector('#txtNombre').disabled = false;
    document.querySelector('#txtTelefono').disabled = false;
    document.querySelector('#txtDireccion').disabled = false;
    document.querySelector('#txtFormaPago').disabled = false;
    document.querySelector('#txtComentario').disabled = false;


})


$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL);
    model["idTienda"] = parseInt($("#cboTienda").val());
    model["idVentaWeb"] = parseInt($("#txtId").val());
    model["estado"] = parseInt($("#cboState").val());

    if (model.estado == 1 && isNaN(model.idTienda)) {
        $('#cboTienda').focus();
        toastr.warning(`Debe seleccionar una Tienda para poder finalizar una Venta Web`, "");
        return;
    }

    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    fetch("/Shop/UpdateVentaWeb", {
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
            swal("Exitoso!", "La Venta Web fué modificada", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
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

function select2Modal() {

    // Initialize select2 after showing the modal
    $('#cboSearchProduct').select2({
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
                            category: item.idCategory,
                            photoBase64: item.photoBase64,
                            price: item.price,
                            tipoVenta: item.tipoVenta
                        }
                    ))
                };
            }
        },
        placeholder: 'Buscando producto...',
        minimumInputLength: 2,
        templateResult: formatResults,
        allowClear: true,
        dropdownParent: $('#modalData')
    });

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
                        <p>$ ${parseFloat(data.price).toFixed(2)}</p>
                    </td>
                </tr>
             </table>`
        );

        return container;
    }

    $('#modalData').on('shown.bs.modal', function () {
        $('#cboSearchProduct').select2('open');
    });

    $('#modalData').on('select2:open', () => {
        document.querySelector('.select2-search__field').focus();
    });

}