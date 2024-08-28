let tableDataTurno;
let rowSelectedTurno;

const BASIC_MODEL_TURNO = {
    idturno: 0,
    descripcion: "",
    fecha: "",
    horaInicio: "",
    horaFin: ""
}


$(document).ready(function () {


    tableDataTurno = $("#tbData").DataTable({
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
            { "data": "total" },
            {
                "defaultContent": '<button class="btn btn-primary btn-sm me-2 btn-edit"><i class="mdi mdi-eye"></i></button>',
                "orderable": false,
                "searchable": false,
                "width": "80px"
            }
        ],
        order: [[1, 2, 3, 4, 5, "desc"]],
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

    const data = tableDataTurno.row(rowSelectedTurno).data();

    openModalTurno(data);
})


const openModalTurno = (model = BASIC_MODEL_TURNO) => {
    abrirTurnoDesdeViewTurnos(model.idTurno);
    
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

            tableDataTurno.row(rowSelectedTurno).data(responseJson.object).draw(false);
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