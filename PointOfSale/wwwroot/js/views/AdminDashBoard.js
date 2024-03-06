var typeValuesGlobal = 0;
var proveedoresList = [];
var tipoGastosList = [];
var tipoDeGasto = "1";

const BASIC_MODEL_GASTO = {
    idGastos: 0,
    idTipoGasto: 0,
    importe: 0,
    comentario: null,
    idUsuario: 0,
    modificationDate: null,
    modificationUser: "",
    estadoPago: 0,
}

const BASIC_MODEL_PAGO_PROVEEDOR = {
    idProveedor: 0,
    tipoFactura: null,
    nroFactura: null,
    iva: 0,
    ivaImporte: 0,
    importe: 0,
    importeSinIva: 0,
    comentario: null,
    estadoPago: 0,
}

$(document).ready(function () {

    fetch("/Gastos/GetTipoDeGasto")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.data.length > 0) {
                tipoGastosList = responseJson.data;
                responseJson.data.forEach((item) => {
                    $("#cboTipoDeGastoEnGasto").append(
                        $("<option>").val(item.idTipoGastos).text(item.descripcion)
                    )
                });
            }
        })

    fetch("/Access/GetAllUsers")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.length > 0) {
                responseJson.forEach((item) => {
                    $("#cboUsuario").append(
                        $("<option>").val(item.idUsers).text(item.name)
                    )
                });
            }
        })

    fetch("/Admin/GetProveedores")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.data.length > 0) {
                proveedoresList = responseJson.data;

                responseJson.data.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });
            }
        })

    fetch("/Inventory/GetCategories")
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {
            $("#cboCategory").append(
                $("<option>").val('Todo').text('Todo')
            )
            if (responseJson.data.length > 0) {

                responseJson.data.forEach((item) => {
                    $("#cboCategory").append(
                        $("<option>").val(item.description).text(item.description)
                    )
                });

            }
        })

    $("#txtDay").datepicker();
    changeChart(1);
})


$("#btnSearch").click(function () {

    if ($("#txtDay").val().trim() == "") {
        toastr.warning("", "Debes ingresar una fecha.");
        return;
    }

    let dayDate = $("#txtDay").val();

    changeChart(typeValuesGlobal, dayDate);
})

$('#cboCategory').change(function () {
    SetTopSeler(typeValuesGlobal, $(this).val());
})

function changeChart(typeValues, dateFilter) {
    showLoading();

    if (dateFilter == undefined) dateFilter = '';

    typeValuesGlobal = typeValues;

    fetch(`/Admin/GetSummary?typeValues=${typeValues}&dateFilter=${dateFilter}`, {
        method: "GET"
    })
        .then(response => {
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                let d = responseJson.object;

                $("#txtTotalSale").text(d.totalSales);
                $("#txtTotalGastos").text(d.gastosTotales)
                $("#txtCantidadClientes").text(d.cantidadClientes)
                $("#txtGanancia").text(d.ganancia)
                $("#gastosProvvedoresTexto").text(d.gastosProvvedoresTexto)
                $("#gastosSueldosTexto").text(d.gastosSueldosTexto)
                $("#gastoTexto").text(d.gastosTexto)
                $("#idTextFilter").text(d.textoFiltroDiaSemanaMes)

                SetGraficoVentas(d);
                SetTopSeler(typeValuesGlobal, $('#cboCategory').val());
                SetGraficoGastos(d.gastosPorTipo);
                SetTipoVentas(d.ventasPorTipoVenta);
                SetGraficoGastosProveedor(d.gastosPorTipoProveedor);
                //SetGraficoGastosSueldos(d.gastosPorTipoSueldos);
                removeLoading();
            }
        });
}

function SetGraficoVentas(d) {
    var options = {
        series: [
            {
                name: d.actual,
                data: d.salesList
            },
            {
                name: d.anterior,
                data: d.salesListComparacion
            }
        ],
        chart: {
            height: 350,
            type: 'line',
            dropShadow: {
                enabled: true,
                color: '#000',
                top: 18,
                left: 7,
                blur: 10,
                opacity: 0.2
            },
            toolbar: {
                show: false
            }
        },
        colors: ['#2275ba', '#c4c4c4'],
        dataLabels: {
            enabled: true,
        },
        stroke: {
            curve: 'smooth'
        },
        title: {
            text: 'Ventas',
            align: 'left'
        },
        grid: {
            borderColor: '#e7e7e7',
            row: {
                colors: ['#f3f3f3', 'transparent'], // takes an array which will be repeated on columns
                opacity: 0.5
            },
        },
        markers: {
            size: 1
        },
        xaxis: {
            categories: d.ejeX,
            title: {
                text: d.ejeXLeyenda
            }
        },
        yaxis: {
            title: {
                text: '$'
            }
        },
        legend: {
            position: 'top',
            horizontalAlign: 'right',
            floating: true,
            offsetY: -25,
            offsetX: -5
        }
    };

    // Grafico de Lineas
    var chartNew = new ApexCharts(document.querySelector("#chartVentas"), options);
    chartNew.render();

    // Actualiza Grafico
    chartNew.updateOptions({
        series: [
            {
                name: d.actual,
                data: d.salesList
            },
            {
                name: d.anterior,
                data: d.salesListComparacion
            }
        ]

    })
}

function SetTopSeler(typeValues, idCategory) {

    fetch(`/Admin/GetSalesByTypoVenta?typeValues=${typeValues}&idCategoria=${idCategory}`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            let d = responseJson;

            //var topSeller = document.getElementById('containerTopSeller');
            //topSeller.innerHTML = "";
            //var ul = document.createElement('ol');
            //ul.setAttribute('style', 'padding: 0; margin: 0;');
            //ul.setAttribute('id', 'theList');

            //for (i = 0; i <= d.length - 1; i++) {
            //    var li = document.createElement('li');
            //    li.innerHTML = d[i].product + ":&nbsp " + d[i].quantity + "";
            //    li.setAttribute('class', 'h3');    
            //    ul.appendChild(li);
            //}
            //topSeller.appendChild(ul);


            // Lista tipo de gastos

            const tableData = d.map(value => {
                return (
                    `<tr>
                       <td><h3 style="color: darkgray;">${value.product}&nbsp;</h3></td>
                       <td style="text-align: right;"><h4>&nbsp;${value.quantity}</h4></td>
                    </tr>`
                );
            }).join('');

            const tableBody = document.querySelector("#tableTopSeller");
            tableBody.innerHTML = tableData;

        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
}

function SetGraficoGastosSueldos(gastosPorTipo) {

    var options = {
        series: gastosPorTipo.map((item) => { return item.total }),
        chart: {
            type: 'pie',
        },
        labels: gastosPorTipo.map((item) => { return item.descripcion }),
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '100%'
                }
            }
        }],
        theme: {
            palette: 'palette5'
        },
        title: {
            text: "Sueldos",
            align: 'left',
            margin: 10,
            offsetX: 0,
            offsetY: 0,
            floating: false,
            style: {
                fontSize: '14px',
                fontWeight: 'bold',
                fontFamily: undefined,
                color: '#263238'
            },
        }
    };

    var chartNew = new ApexCharts(document.querySelector("#charGastosSueldos"), options);
    chartNew.render();

    chartNew.updateOptions({
        series: gastosPorTipo.map((item) => { return item.total }),
        labels: gastosPorTipo.map((item) => { return item.descripcion })
    })
}
function SetGraficoGastosProveedor(gastoProveedores) {

    var options = {
        series: gastoProveedores.map((item) => { return item.total }),
        chart: {
            type: 'pie',
        },
        labels: gastoProveedores.map((item) => { return item.descripcion }),
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '100%'
                }
            }
        }],
        theme: {
            palette: 'palette2'
        },
        title: {
            text: "Proveedores",
            align: 'left',
            margin: 10,
            offsetX: 0,
            offsetY: 0,
            floating: false,
            style: {
                fontSize: '14px',
                fontWeight: 'bold',
                fontFamily: undefined,
                color: '#263238'
            },
        }
    };

    var chartNew = new ApexCharts(document.querySelector("#charGastosProveedor"), options);
    chartNew.render();

    chartNew.updateOptions({
        series: gastoProveedores.map((item) => { return item.total }),
        labels: gastoProveedores.map((item) => { return item.descripcion })
    })
}

function SetGraficoGastos(gastosPorTipo) {

    var options = {
        series: gastosPorTipo.map((item) => { return item.total }),
        chart: {
            type: 'pie',
        },
        labels: gastosPorTipo.map((item) => { return item.descripcion }),
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '100%'
                }
            }
        }],
        theme: {
            palette: 'palette10'
        },
        title: {
            text: "Gastos Particulares",
            align: 'left',
            margin: 10,
            offsetX: 0,
            offsetY: 0,
            floating: false,
            style: {
                fontSize: '14px',
                fontWeight: 'bold',
                fontFamily: undefined,
                color: '#263238'
            },
        }
    };

    var chartNew = new ApexCharts(document.querySelector("#charGastos"), options);
    chartNew.render();

    chartNew.updateOptions({
        series: gastosPorTipo.map((item) => { return item.total }),
        labels: gastosPorTipo.map((item) => { return item.descripcion })
    })
}

function SetTipoVentas(tipoVentas) {

    var options = {
        series: tipoVentas.map((item) => { return item.total }),
        chart: {
            type: 'pie',
        },
        labels: tipoVentas.map((item) => { return item.descripcion }),
        responsive: [{
            breakpoint: 480,
            options: {
                chart: {
                    width: '100%'
                }
            }
        }],
        title: {
            text: "Ventas",
            align: 'left',
            margin: 10,
            offsetX: 0,
            offsetY: 0,
            floating: false,
            style: {
                fontSize: '14px',
                fontWeight: 'bold',
                fontFamily: undefined,
                color: '#263238'
            },
        }
    };

    var chartNew = new ApexCharts(document.querySelector("#charTipoVentas"), options);
    chartNew.render();

    chartNew.updateOptions({
        series: tipoVentas.map((item) => { return item.total }),
        labels: tipoVentas.map((item) => { return item.descripcion })
    })
}

$('#cboTipoDeGastoEnGasto').change(function () {
    var idTipoGasro = $(this).val();
    var tipoGasto = tipoGastosList.find(_ => _.idTipoGastos == idTipoGasro);

    if (tipoGasto != null) {
        $("#txtGasto").val(tipoGasto.gastoParticular);
    }
    else {
        $("#txtGasto").val('');
    }
})

$('#cboTipoDePago').change(function () {
    tipoDeGasto = $(this).val();

    if (tipoDeGasto === "1") { // gasto
        $(".pago-gasto").show();
        $(".pago-proveedor").hide();
    }
    else { // proveedor
        $(".pago-gasto").hide();
        $(".pago-proveedor").show();
    }
})

$('#cboProveedor').change(function () {
    var idProv = $(this).val();
    var proveedor = proveedoresList.find(_ => _.idProveedor == idProv);

    if (proveedor != null) {
        $("#txtCuilPago").val(proveedor.cuil);
        $("#txtDireccionPago").val(proveedor.direccion);
    }
    else {
        $("#txtCuilPago").val('');
        $("#txtDireccionPago").val('');
    }
})


$("#btnNuevoGasto").on("click", function () {
    $("#modalNuevoGasto").modal("show")

    $("#cboTipoDeGastoEnGasto").val('');
    $("#txtImporte").val('');
    $("#txtComentario").val('');
    $("#cboUsuario").val('');
    $("#cboTipoFactura").val('');
    $("#txtNroFactura").val('');
    $("#txtIva").val('');
    $("#txtImporteIva").val('');
    $("#txtImporteSinIva").val('');
    $("#cboEstado").val('');
    $("#cboProveedor").val('');
    $("#txtCuilPago").val('');
    $("#txtDireccionPago").val('');
})

$("#btnSavePagoProveedor").on("click", function () {
    let url;
    let model;

    if (tipoDeGasto === "1") { // gasto
        url = "/Gastos/CreateGastos";

        let validacion = true;
        $(".input-validate-gasto").each(function () {
            validacion = ($(this).val().trim() === "") ? false : validacion;
        });

        if ($("#txtImporte").val().trim() == "") {
            validacion = false;
        }

        if (!validacion) {
            toastr.warning(`Debe completar todos los campos obligatorios`, "");
            return;
        }

        model = structuredClone(BASIC_MODEL_GASTO);
        model["idGastos"] = 0;
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
    }
    else { // proveedor
        url = "/Admin/RegistrarPagoProveedor";
        const inputs = $(".input-validate-proveedor").serializeArray();
        const inputs_without_value = inputs.filter((item) => item.value.trim() == "")

        if (inputs_without_value.length > 0) {
            const msg = `Debe completar los campos : "${inputs_without_value[0].name}"`;
            toastr.warning(msg, "");
            $(`input[name="${inputs_without_value[0].name}"]`).focus();
            return;
        }

        if ($("#cboProveedor").val() == '') {
            const msg = `Debe completar el campo del Proveedor`;
            toastr.warning(msg, "");
            return;
        }

        model = structuredClone(BASIC_MODEL_PAGO_PROVEEDOR);
        model["idProveedor"] = parseInt($("#cboProveedor").val());
        model["tipoFactura"] = $("#cboTipoFactura").val();
        model["nroFactura"] = $("#txtNroFactura").val();
        model["iva"] = $("#txtIva").val() != '' ? $("#txtIva").val() : 0;
        model["ivaImporte"] = $("#txtImporteIva").val() != '' ? $("#txtImporteIva").val() : 0;
        model["importe"] = $("#txtImporte").val();
        model["importeSinIva"] = $("#txtImporteSinIva").val() != '' ? $("#txtImporteSinIva").val() : 0;
        model["comentario"] = $("#txtComentario").val();
        model["estadoPago"] = parseInt($("#cboEstado").val());
    }


    fetch(url, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        return response.ok ? response.json() : Promise.reject(response);
    }).then(responseJson => {

        if (responseJson.state) {
            $("#modalNuevoGasto").modal("hide");
            swal("Exitoso!", "Se registró con éxito", "success");

        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
})

function calcularImportesGasto() {
    var importe = $("#txtImporteGasto").val();
    var iva = $("#txtIvaGasto").val();

    if (importe !== '' && iva !== '') {
        let importeFloat = parseFloat(importe)
        var importeSinIva = parseFloat(importeFloat) * (1 - (parseFloat(iva) / 100));

        $("#txtImporteSinIvaGasto").val(importeSinIva.toFixed(2));
        $("#txtImporteIvaGasto").val((importeFloat - importeSinIva).toFixed(2));

    }
}

function calcularImportesProv() {
    var importe = $("#txtImporte").val();
    var iva = $("#txtIva").val();

    if (importe !== '' && iva !== '') {
        let importeFloat = parseFloat(importe)
        var importeSinIva = importeFloat * (1 - (parseFloat(iva) / 100));

        $("#txtImporteSinIva").val(importeSinIva.toFixed(2));
        $("#txtImporteIva").val((importeFloat - importeSinIva).toFixed(2));
    }
}