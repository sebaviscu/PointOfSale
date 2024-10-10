let tableDataPagosLicencia;
let rowSelectedPagoLicencia;

const BASIC_MODEL_LICENCIA = {
    id: 0,
    razonSocial: '',
    licencia: 0,
    nombreContacto: null,
    numeroContacto: null,
    proximoPago: null,
    frecuenciaPago: null,
    comentario: null
}

const BASIC_MODEL_PAGO_LICENCIA = {
    id: 0,
    fechaPago: '',
    importe: 0,
    comentario: null,
    estadoPago: null,
    modificationDate: null,
    modificationUser: "",
    tipoFactura: null,
    nroFactura: null,
    iva: null,
    ivaImporte: null,
    importeSinIva: null,
    facturaPendiente: null
}

$(document).ready(function () {

    tableDataPagosLicencia = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Licencia/GetDataTable",
            "type": "GET",
            "datatype": "json"
        },
            "rowCallback": function (row, data) {
                if (data.facturaPendiente == 1) {
                    $('td:eq(3)', row).addClass('factura-pendiente');
                }
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
                "data": "id",
                "visible": false,
                "searchable": false
            },
            { "data": "fechaPago" },
            {
                "data": "importe", render: function (data, type, row) {
                    return `$ ${data}`;
                }
            },
            {
                "data": "estadoPago",
                "className": "text-center", render: function (data) {
                    if (data == 0)
                        return '<span class="badge rounded-pill bg-success">Pagado</span>';
                    else
                        return '<span class="badge rounded-pill bg-warning text-dark">Pendiente</span>';
                }
            },
            {
                "data": "nroFactura", render: function (data, type, row) {
                    return data != '' ? `Factura ${row.tipoFacturaString} ${data} ` : '';
                }
            },
            { "data": "comentario" },
            {
                "defaultContent": '<button class="btn btn-primary btn-edit btn-sm me-2"><i class="mdi mdi-pencil"></i></button>' +
                    '<button class="btn btn-danger btn-delete btn-sm"><i class="mdi mdi-trash-can"></i></button>',
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
                filename: 'Reporte Pagos Licencia',
                exportOptions: {
                    columns: [1, 2, 3, 4]
                }
            }, 'pageLength'
        ]
    });


    fetch("/Licencia/GetLicencia")
        .then(response => {
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                let model = responseJson.object;

                $("#txtRazonSocial").val(model.razonSocial);
                $("#txtNombreContacto").val(model.nombreContacto);
                $("#txtNumeroContacto").val(model.numeroContacto);
                $("#cboFrecuenciaPago").val(model.frecuenciaPago);
                $("#txtComentario").val(model.comentario);

                let licenciasPreseleccionadas = obtenerLicenciasSeleccionadas(model.licencia);
                $('#selectLicencia').val(licenciasPreseleccionadas).trigger('change');

                if (model.proximoPago != null) {
                    let fecha = model.proximoPago.split("T")[0];
                    $('#txtProximoPago').val(fecha);
                }


            }
            else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })


    $('#selectLicencia').select2({
        placeholder: 'Seleccione licencias',
        allowClear: true
    });

})

function setToday() {
    let date = new Date();

    // Convertir la fecha a formato ISO (YYYY-MM-DD) para los inputs de tipo "date"
    let today = date.toISOString().substr(0, 10); // Obtener solo la parte de la fecha (YYYY-MM-DD)

    // Asignar la fecha al input de tipo "date"
    $('#txtFechaPago').val(today);
}


const openModalNuevoPago = (model = BASIC_MODEL_PAGO_LICENCIA) => {

    $("#txtId").val(model.id);

    if (model.fechaPago != '') {
        let fecha = model.fechaPago.split("T")[0];
        $("#txtFechaPago").val(fecha);
    }
    else {
        setToday();
    }

    $("#txtImporte").val(model.importe);
    $("#txtComentarioPago").val(model.comentario);
    $("#cboEstado").val(model.estadoPago);

    document.getElementById("cbxFacturaPendiente").checked = model.facturaPendiente;
    $("#cboTipoFactura").val(model.tipoFactura);
    $("#txtNroFactura").val(model.nroFactura);
    $("#txtIva").val(model.iva);
    $("#txtImporteIva").val(model.ivaImporte);
    $("#txtImporteSinIva").val(model.importeSinIva);


    if (model.modificationDate == null)
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
    openModalNuevoPago()
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

    const model = structuredClone(BASIC_MODEL_PAGO_LICENCIA);
    model["id"] = parseInt($("#txtId").val());
    model["fechaPago"] = $("#txtFechaPago").val();
    model["importe"] = $("#txtImporte").val();
    model["comentario"] = $("#txtComentarioPago").val();
    model["estadoPago"] = $("#cboEstado").val();

    model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;
    model["tipoFactura"] = $("#cboTipoFactura").val();
    model["nroFactura"] = $("#txtNroFactura").val();
    model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
    model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
    model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;


    $("#modalData").find("div.modal-content").LoadingOverlay("show")

    if (model.id == 0) {
        fetch("/Licencia/CreatePagoLicencia", {
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

                tableDataPagosLicencia.row.add(model).draw(false);
                swal("Exitoso!", "El PAgo fue creado con éxito.", "success");
                $('#modalData').modal('hide');

            } else {
                swal("Lo sentimos", responseJson.message, "error");
            }
        }).catch((error) => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
        })
    } else {

        fetch("/Licencia/UpdatePagoLicencia", {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(model)
        }).then(response => {
            $("#modalData").find("div.modal-content").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {
                tableDataPagosLicencia.row(rowSelectedPagoLicencia).data(model).draw(false);
                rowSelectedPagoLicencia = null;
                swal("Exitoso!", "El Pago fue actualizado con éxito.", "success");
                $('#modalData').modal('hide');

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
        rowSelectedPagoLicencia = $(this).closest('tr').prev();
    } else {
        rowSelectedPagoLicencia = $(this).closest('tr');
    }

    const data = tableDataPagosLicencia.row(rowSelectedPagoLicencia).data();

    openModalNuevoPago(data);
})


$("#tbData tbody").on("click", ".btn-delete", function () {

    let row;

    if ($(this).closest('tr').hasClass('child')) {
        row = $(this).closest('tr').prev();
    } else {
        row = $(this).closest('tr');
    }
    const data = tableDataPagosLicencia.row(row).data();

    swal({
        title: "¿Está seguro?",
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

                fetch(`/Licencia/DeletePagoLicencia?idPago=${data.id}`, {
                    method: "DELETE"
                }).then(response => {
                    $(".showSweetAlert").LoadingOverlay("hide")
                    return response.json();
                }).then(responseJson => {
                    if (responseJson.state) {
                        tableDataPagosLicencia.row(row).remove().draw();
                        swal("Exitoso!", "El pago fue eliminada", "success");
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

function calcularIva() {
    let importeText = $('#txtImporte').val();
    let importe = parseFloat(importeText == '' ? 0 : importeText);
    let iva = parseFloat($('#txtIva').val());
    if (!isNaN(importe) && !isNaN(iva)) {
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


$("#btnSaveEmpresa").on("click", function () {
    const inputs = $("input.input-validate-empresa").serializeArray();
    const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

    if (inputs_without_value.length > 0) {
        const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
        toastr.warning(msg, "");
        $(`input[name="${inputs_without_value[0].name}"]`).focus();
        return;
    }

    const model = structuredClone(BASIC_MODEL_LICENCIA);
    model["id"] = 1;
    model["razonSocial"] = $("#txtRazonSocial").val();
    model["nombreContacto"] = $("#txtNombreContacto").val();
    model["numeroContacto"] = $("#txtNumeroContacto").val();
    model["licencia"] = calcularValorLicencias();

    model["frecuenciaPago"] = $("#cboFrecuenciaPago").val();
    model["proximoPago"] = $("#txtProximoPago").val();
    model["comentario"] = $("#txtComentario").val();

    fetch("/Licencia/UpdateEmpresa", {
        method: "PUT",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {

            swal("Exitoso!", "La Licencia fue actualizada con éxito.", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    }).catch((error) => {
    })


})

function calcularValorLicencias() {
    let valorLicencias = 0;
    $('#selectLicencia').val().forEach(function (licencia) {
        valorLicencias += parseInt(licencia); // Sumamos los valores de las licencias seleccionadas
    });
    return valorLicencias;
}

function obtenerLicenciasSeleccionadas(valorLicencias) {
    let licencias = [];
    if (valorLicencias & 1) licencias.push('1'); // Base
    if (valorLicencias & 2) licencias.push('2'); // Facturación
    if (valorLicencias & 4) licencias.push('4'); // Web
    return licencias;
}