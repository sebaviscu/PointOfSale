let tableData;
let rowSelected;

const BASIC_MODEL = {
    idturno: 0,
    descripcion: "",
    fecha: "",
    horaInicio: "",
    horaFin: ""
}


$(document).ready(function () {


    tableData = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/turno/GetTurnos",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idTurno",
                "visible": false,
                "searchable": false
            },
            { "data": "fecha" },
            { "data": "horaInicio" },
            { "data": "horaFin" },
            { "data": "modificationUser" },
            { "data": "descripcion" },
            { "data": "total" },
            {
                "defaultContent": '<button class="btn btn-primary btn-sm me-2 btn-edit"><i class="mdi mdi-eye"></i></button>',
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
                filename: 'Reporte turnos',
                exportOptions: {
                    columns: [1, 2]
                }
            }, 'pageLength'
        ]
    });
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


const openModal = (model = BASIC_MODEL) => {
    $("#txtId").val(model.idTurno);
    $("#txtDescripcion").val(model.descripcion);
    $("#txtFecha").val(model.fecha);
    $("#txtHoraInicio").val(model.horaInicio);
    $("#txtHoraFin").val(model.horaFin);
    document.getElementById('txtTotal').innerHTML = "Total: " + model.total;

    fetch(`/Turno/GetOneTurno?idTurno=` + model.idTurno, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#contMetodosPago").empty();

            var resp = responseJson.data;
            let list = document.getElementById("contMetodosPago");
            for (i = 0; i < resp.ventasPorTipoVenta.length; ++i) {
                let li = document.createElement('li');
                li.innerText = resp.ventasPorTipoVenta[i].descripcion + ": $" + resp.ventasPorTipoVenta[i].total;
                list.appendChild(li);
            }

        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });


    $("#modalData").modal("show")
}


$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL);
    model["idTurno"] = parseInt($("#txtId").val());
    model["descripcion"] = $("#txtDescripcion").val();

    fetch("/Turno/Update", {
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
            swal("Exitoso!", "Turno fué modificada", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
})