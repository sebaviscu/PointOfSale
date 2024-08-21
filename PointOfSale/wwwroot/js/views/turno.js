let tableData;
let rowSelectedTurno;

const BASIC_MODEL_TURNO = {
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
        "columnDefs": [
            {
                "targets": [1],
                "render": function (data, type, row) {
                    if (type === 'display' || type === 'filter') {
                        return data ? moment(data).format('DD/MM/YYYY') : '';
                    }
                    return data;
                }
            }
        ],
        "columns": [
            {
                "data": "idTurno",
                "visible": false,
                "searchable": false
            },
            { "data": "fechaInicio" },
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
        order: [[1, "desc"]],
        dom: "Bfrtip",
        buttons: [
            {
                text: 'Exportar Excel',
                extend: 'excelHtml5',
                title: '',
                filename: 'Reporte Turnos',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });
})


$("#tbData tbody").on("click", ".btn-edit", function () {

    if ($(this).closest('tr').hasClass('child')) {
        rowSelectedTurno = $(this).closest('tr').prev();
    } else {
        rowSelectedTurno = $(this).closest('tr');
    }

    const data = tableData.row(rowSelectedTurno).data();

    openModal(data);
})


const openModal = (model = BASIC_MODEL_TURNO) => {
    $("#txtId").val(model.idTurno);
    $("#txtDescripcion").val(model.descripcion);
    $("#txtFecha").val(model.fecha);
    $("#txtHoraInicio").val(model.horaInicio);
    $("#txtHoraFin").val(model.horaFin);
    $("#txtTotal").val(model.total);

    fetch(`/Turno/GetOneTurno?idTurno=` + model.idTurno, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                $("#contMetodosPago").empty();

                let resp = responseJson.object;
                let list = document.getElementById("contMetodosPago");
                for (i = 0; i < resp.ventasPorTipoVenta.length; ++i) {
                    let li = document.createElement('li');
                    li.innerText = resp.ventasPorTipoVenta[i].descripcion + ": $" + resp.ventasPorTipoVenta[i].total;
                    list.appendChild(li);
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });


    $("#modalData").modal("show")
}


$("#btnSave").on("click", function () {

    const model = structuredClone(BASIC_MODEL_TURNO);
    model["idTurno"] = parseInt($("#txtId").val());
    model["descripcion"] = $("#txtDescripcion").val();

    fetch("/Turno/UpdateTurno", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            tableData.row(rowSelectedTurno).data(responseJson.object).draw(false);
            rowSelectedTurno = null;
            $("#modalData").modal("hide");
            swal("Exitoso!", "Turno fué modificada", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
        $("#modalData").find("div.modal-content").LoadingOverlay("hide")
    })
})