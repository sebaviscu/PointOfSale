let tableDataPagosLicencia;
$(document).ready(function () {

    $('#selectLicencia').select2({
        placeholder: 'Seleccione licencias',
        allowClear: true
    });

    tableDataPagosLicencia = $("#tbData").DataTable({
        responsive: true,
        "ajax": {
            "url": "/Licencia/GetDataTable",
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
            { "data": "comentario" }

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

                $("#txtRazonSocial").val(responseJson.object.razonSocial);
                $("#txtNombreContacto").val(responseJson.object.nombreContacto);
                $("#txtNumeroContacto").val(responseJson.object.numeroContacto);
                $("#cboFrecuenciaPago").val(responseJson.object.frecuenciaPago);
                $("#txtComentario").val(responseJson.object.comentario);

                let licenciasPreseleccionadas = obtenerLicenciasSeleccionadas(responseJson.object.licencia);
                $('#selectLicencia').val(licenciasPreseleccionadas).trigger('change');

                if (responseJson.object.proximoPago != null) {
                    var fecha = responseJson.object.proximoPago.split("T")[0];
                    $('#txtProximoPago').val(fecha);
                }

            }
            else
            {
                swal("Lo sentimos", responseJson.message, "error");
            }
        })

})

document.onkeyup = async function (e) {

    if (e.altKey && e.which == 78) { // alt + N
        redirectUpdate()
    }
};

function redirectUpdate() {
    window.open('/Licencia/UpdateLicencia', '_blank');

}

function obtenerLicenciasSeleccionadas(valorLicencias) {
    let licencias = [];
    if (valorLicencias & 1) licencias.push('1'); // Base
    if (valorLicencias & 2) licencias.push('2'); // Facturación
    if (valorLicencias & 4) licencias.push('4'); // Web
    return licencias;
}