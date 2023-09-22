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
    registrationDate: null,
    modificationDate: null,
    modificationUser: null
}


$(document).ready(function () {


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
                filename: 'Reporte categories',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });
})

const openModal = (model = BASIC_MODEL) => {
    $("#txtId").val(model.idVentaWeb);
    $("#txtFecha").val(model.fecha);
    $("#txtTotal").val(model.totalString);
    $("#txtNombre").val(model.nombre);
    $("#txtTelefono").val(model.telefono);
    $("#txtDireccion").val(model.direccion);
    $("#txtFormaPago").val(model.formaDePago);
    $("#txtComentario").val(model.comentario);
    $("#cboState").val(model.estado);

    if (model.modificationUser === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        var dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }


    var tableData = model.detailSales.map(value => {
        return (
            `<tr>
                       <td class="table-products" style="border-right-color: #ffffff00;"><span class="text-muted">$ ${Number.parseFloat(value.price).toFixed(2)} x ${value.quantity} ${value.tipoVenta}</span>. - ${value.descriptionProduct}</td>
                       <td class="table-products" style="font-size: 12px; text-align: right;"><strong>$ ${Number.parseFloat(value.total).toFixed(2)}</strong></td>
                    </tr>`
        );
    }).join('');

    const tableBody = document.querySelector("#tableProductos");
    tableBody.innerHTML = tableData;



    $("#modalData").modal("show")

}

$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL);
    model["idVentaWeb"] = parseInt($("#txtId").val());
    model["estado"] = $("#cboState").val();


    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    fetch(`/Shop/UpdateVentaWeb?idVentaWeb=${model.idVentaWeb}?estado=${model.estado}`, {
        method: "PUT"
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