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
    idTienda:0,
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
                filename: 'Reporte Ventas Web',
                exportOptions: {
                    columns: [1, 2,3,4,5,6]
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
            $("#modalData").modal("hide");
            swal("Exitoso!", "Ticket impreso!", "success");
        })

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
    $("#cboTienda").val(model.idTienda);

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

$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL);
    model["idTienda"] = parseInt($("#cboTienda").val());
    model["idVentaWeb"] = parseInt($("#txtId").val());
    model["estado"] = parseInt($("#cboState").val());


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