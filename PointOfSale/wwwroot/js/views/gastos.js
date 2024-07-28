let tableData;
let rowSelected;
let tipoGastosList = [];
const monthNames = ["Ene", "Feb", "Mar", "Abr", "May", "Jun",
    "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"
];

const BASIC_MODEL = {
    idGastos: 0,
    idTipoGasto: 0,
    importe: 0,
    comentario: null,
    idUsuario: 0,
    modificationDate: null,
    modificationUser: "",
    estadoPago: 0,
}

const BASIC_MODEL_TIPO_DE_GASTOS = {
    IdTipoGastos: 0,
    gastoParticular: -1,
    descripcion: ""
}

$(document).ready(function () {
    showLoading();

    fetch("/Gastos/GetTipoDeGasto")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                if (responseJson.object.length > 0) {
                    tipoGastosList = responseJson.object;

                    responseJson.object.forEach((item) => {
                        $("#cboTipoDeGastoEnGasto").append(
                            $("<option>").val(item.idTipoGastos).text(item.descripcion)
                        )
                    });
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

    fetch("/Access/GetAllUsers")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                if (responseJson.object.length > 0) {
                    tipoGastosList = responseJson.object;

                    responseJson.object.forEach((item) => {
                        $("#cboUsuario").append(
                            $("<option>").val(item.idUsers).text(item.name)
                        )
                    });
                }
            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }

        })

    tableData = $("#tbData").DataTable({
        responsive: true,
        "rowCallback": function (row, data) {
            if (data.facturaPendiente == 1) {
                $('td:eq(2)', row).addClass('factura-pendiente');
            }
        },
        "ajax": {
            "url": "/Gastos/GetGastos",
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            {
                "data": "idGastos",
                "visible": false,
                "searchable": false
            },
            { "data": "tipoGastoString" },
            { "data": "gastoParticular" },
            { "data": "nroFactura" },
            { "data": "fechaString" },
            { "data": "comentario" },
            {
                "data": "estadoPago",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-success">Pagado</span>';
                    else
                        return '<span class="badge rounded-pill bg-warning text-dark">Pendiente</span>';
                }
            },
            { "data": "importeString" },
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
                filename: 'Reporte Gastos',
                exportOptions: {
                    columns: [1, 2, 3, 4, 5, 6]
                }
            }, 'pageLength'
        ]
    });

    cargarTablaDinamica();
    removeLoading();
})

const openModalTipoDeGasto = (model = BASIC_MODEL_TIPO_DE_GASTOS) => {
    $("#txtIdTipoGastos").val(model.IdTipoGastos);
    $("#cboTipoDeGasto").val(model.gastoParticular);
    $("#txtDescripcionTipoDeGasto").val(model.descripcion);

    $("#modalDataTipoDeGasto").modal("show")
}


$("#btnNewTipoDeGasto").on("click", function () {
    openModalTipoDeGasto()
})


$("#btnSaveTipoDeGastos").on("click", function () {
    const inputs = $("input.input-validate-TipoDeGastos").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_TIPO_DE_GASTOS);
    model["IdTipoGastos"] = parseInt($("#txtIdTipoGastos").val());
    model["gastoParticular"] = $("#cboTipoDeGasto").val();
    model["descripcion"] = $("#txtDescripcionTipoDeGasto").val();
    model["tipoFactura"] = parseInt($("#cboTipoFacturaTipoGasto").val());
    model["iva"] = $("#txtIvaTipoGasto").val() != '' ? parseInt($("#txtIvaTipoGasto").val()) : 0;

    $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("show")


    if (model.IdTipoGastos == 0) {
        fetch("/Gastos/CreateTipoDeGastos", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalDataTipoDeGasto").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    }
})

const openModal = (model = BASIC_MODEL) => {
    $("#txtIdGastos").val(model.idGastos);
    $("#cboTipoDeGastoEnGasto").val(model.idTipoGasto);
    $("#txtImporte").val(model.importe);
    $("#cboUsuario").val(model.idUsuario);
    $("#cboTipoFactura").val(model.tipoFactura);
    $("#txtNroFactura").val(model.nroFactura);
    $("#txtIva").val(model.iva);
    $("#txtImporteIva").val(model.ivaImporte);
    $("#txtImporteSinIva").val(model.importeSinIva);
    $("#txtComentario").val(model.comentario);
    $("#cboEstado").val(model.estadoPago);
    document.getElementById("cbxFacturaPendiente").checked = model.facturaPendiente;

    if (model.modificationDate === null)
        document.getElementById("divModif").style.display = 'none';
    else {
        document.getElementById("divModif").style.display = '';
        let dateTimeModif = new Date(model.modificationDate);

        $("#txtModificado").val(dateTimeModif.toLocaleString());
        $("#txtModificadoUsuario").val(model.modificationUser);
    }

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

    if ($("#cboTipoDeGastoEnGasto").val() == null) {
        const msg = `Debe completar el tipo de gasto`;
        toastr.warning(msg, "");
        return;
    }

    const model = structuredClone(BASIC_MODEL);
    model["idGastos"] = parseInt($("#txtIdGastos").val());
    model["idTipoGasto"] = $("#cboTipoDeGastoEnGasto").val();
    model["importe"] = $("#txtImporte").val();
    model["comentario"] = $("#txtComentario").val();
    model["idUsuario"] = $("#cboUsuario").val() != 0 ? $("#cboUsuario").val() : null;
    model["tipoFactura"] = $("#cboTipoFactura").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
    model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
    model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;
    model["estadoPago"] = parseInt($("#cboEstado").val());
    model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;

    $("#modalData").find("div.modal-content").LoadingOverlay("show")


    if (model.idGastos == 0) {
        fetch("/Gastos/CreateGastos", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                swal("Exitoso!", "Guardado con éxito", "success");
                $("#modalData").modal("hide")

                location.reload()

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Gastos/UpdateGastos", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                swal("Exitoso!", "Modificado con éxito", "success");
                location.reload()

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
        text: `Eliminar gasto "${data.gastoParticular} ${data.tipoGastoString} ${data.comentario}"`,
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

                fetch(`/Gastos/DeleteGastos?idGastos=${data.idGastos}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                        if (responseJson.state) {
                            tableData.row(row).remove().draw();
                            swal("Exitoso!", "El gasto  fue eliminada", "success");
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

$('#cboTipoDeGastoEnGasto').change(function () {
    let idTipoGasro = $(this).val();
    let tipoGasto = tipoGastosList.find(_ => _.idTipoGastos == idTipoGasro);
    if (tipoGasto != null) {
        $("#txtGasto").val(tipoGasto.gastoParticular);
        $("#txtIva").val(tipoGasto.iva ?? '')
        $("#cboTipoFactura").val(tipoGasto.tipoFactura ?? '')
    }
    else {
        $("#txtGasto").val('');
        $("#txtIva").val('')
        $("#cboTipoFactura").val('')
    }
})


function calcularImportes() {
    let importe = $("#txtImporte").val();
    let iva = $("#txtIva").val();
    if(importe !== '' && iva !== '') {
        let importeSinIva = parseFloat(importe) * (1 - (parseFloat(iva) / 100));
        $("#txtImporteSinIva").val(importeSinIva);
        $("#txtImporteIva").val(importe - importeSinIva);

    }
}

function calcularIva() {
    let importeText = $('#txtImporte').val();
    let importe = parseFloat(importeText == '' ? 0 : importeText);
    let iva = parseFloat($('#txtIva').val());
    if(!isNaN(importe) && !isNaN(iva)) {
        let importeSinIva = importe / (1 + (iva / 100));
        let importeIva = importe - importeSinIva;
        $('#txtImporteSinIva').val(importeSinIva.toFixed(2));
        $('#txtImporteIva').val(importeIva.toFixed(2));
    }
}

$('#txtIva').change(function () {
    calcularIva();
});

$('#txtImporte').keyup(function () {
    calcularIva();
});

function cargarTablaDinamica() {
    fetch(`/Gastos/GetGastosTablaDinamica`, {
        method: "GET"
    }).then(response => {
        return response.ok || response.status == 500 ? response.json() : Promise.reject(response);
    }).then(responseJson => {
        if (responseJson.error !== undefined) {
            console.error(responseJson.error);
            return false;
        }

        if (responseJson.state && responseJson.data !== []) {

            let today = new Date();
            let month = "fecha.Month." + monthNames[today.getMonth()];
            let year = "fecha.Year." + today.getFullYear();
            let pivot = new WebDataRocks({
                container: "#wdr-component",
                toolbar: true,
                report: {
                    dataSource: {
                        data: responseJson.object
                    },
                    formats: [
                        {
                            name: "currency",
                            currencySymbol: "$"
                        }
                    ],
                    "slice": {
                        reportFilters: [
                            {
                                uniqueName: "fecha.Day"
                            },
                            {
                                uniqueName: "fecha.Month",
                                "filter": {
                                    "members": [
                                        month
                                    ]
                                }
                            },
                            {
                                uniqueName: "fecha.Year",
                                "filter": {
                                    "members": [
                                        year
                                    ]
                                }
                            }
                        ],
                        "rows": [
                            {
                                "uniqueName": "gasto",
                                sort: "desc"
                            },
                            {
                                "uniqueName": "tipo_Gasto",
                                sort: "desc"
                            },
                            {
                                "uniqueName": "tipo_Factura",
                                sort: "desc"
                            },
                            {
                                "uniqueName": "nro_Factura",
                                sort: "desc"
                            }
                        ],
                        columns: [
                            {
                                uniqueName: "[Measures]"
                            }
                        ],
                        measures: [
                            {
                                uniqueName: "importe",
                                caption: "Importe",
                                aggregation: "sum",
                                format: "currency"
                            },
                            {
                                uniqueName: "importe_Sin_Iva",
                                caption: "Importe sin IVA",
                                aggregation: "sum",
                                format: "currency"
                            },
                            {
                                uniqueName: "iva_Importe",
                                caption: "Importe IVA",
                                aggregation: "sum",
                                format: "currency"
                            }
                        ]
                    }
                },
                global: {
                    localization: "https://cdn.webdatarocks.com/loc/es.json"
                }
            });
        }
        else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
}
