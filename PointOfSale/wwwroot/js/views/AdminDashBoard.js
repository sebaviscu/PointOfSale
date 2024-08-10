let typeValuesGlobal = 0;
let proveedoresListDashboard = [];
let tipoGastosListDashboard = [];
let tipoDeGasto = "1";
let chartVentas;
let charGastosSueldos;
let charGastosProveedor;
let charGastos;
let charTipoVentas;

let chartVentasGlobal;
let charGastosProveedorGlobal;
let charGastosGlobal;
let visionGlobal = false;

let gastoTotal = 0;
let proveedorTotal = 0;
let sueldoTotal = 0;
let saleTotal = 0;
let prueba_ver_si_lo_agarra = 0;


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
    facturaPendiente: 0
}

$(document).ready(function () {

    fetch("/Tienda/GetTienda")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            document.getElementById("divSwitchVisionGlobal").style.display = responseJson.data != null && responseJson.data.length > 1 ? '' : 'none';

        })

    fetch("/Gastos/GetTipoDeGasto")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                if (responseJson.object.length > 0) {
                    tipoGastosListDashboard = responseJson.object;

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
                    tipoGastosListDashboard = responseJson.object;

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

    fetch("/Admin/GetProveedores")
        .then(response => {
            return response.json();
        }).then(responseJson => {

            if (responseJson.data.length > 0) {
                proveedoresListDashboard = responseJson.data;

                responseJson.data.forEach((item) => {
                    $("#cboProveedor").append(
                        $("<option>").val(item.idProveedor).text(item.nombre)
                    )
                });
            }
        })

    fetch("/Inventory/GetCategories")
        .then(response => {
            return response.json();
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

$('#switchVisionGlobal').change(function () {
    visionGlobal = $(this).is(':checked')
    changeChart(typeValuesGlobal);
});

$("#btnSearch").click(function () {

    if ($("#txtDay").val().trim() == "") {
        toastr.warning("", "Debes ingresar una fecha.");
        return;
    }

    changeChart(typeValuesGlobal);
})

$('#cboCategory').change(function () {
    SetTopSeler(typeValuesGlobal, $(this).val());
})

function changeChart(typeValues) {

    dateFilter = $("#txtDay").val();

    typeValuesGlobal = typeValues;
    LoadingText("show");

    fetch(`/Admin/GetSummary?typeValues=${typeValues}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            LoadingText("hide");
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                let d = responseJson.object;

                $("#txtTotalSale").text("$ " + d.totalSales);
                saleTotal = d.totalSales;
                setGastosGanancia();

                $("#txtCantidadClientes").text(d.cantidadClientes)
                $("#idTextFilter").text(d.textoFiltroDiaSemanaMes)

                SetGraficoVentas(d);

            }
        });

    SetTopSeler(typeValuesGlobal, $('#cboCategory').val());
    SetTipoVentas(typeValues, dateFilter);
    SetGraficoGastos(typeValues, dateFilter);
    SetGraficoGastosProveedor(typeValues, dateFilter);
    SetGraficoGastosSueldos(typeValues, dateFilter);

    document.getElementById("divGraficosGlobales").style.display = visionGlobal ? '' : 'none';

    if (visionGlobal) {
        SetVentasByTiendas(typeValues, dateFilter)
        SetProveedoresByTiendas(typeValues, dateFilter);
        SetGastosByTiendas(typeValues, dateFilter);
    }
}

function LoadingText(text) {
    $("#btnSearch").LoadingOverlay(text)
    $("#chartVentas").LoadingOverlay(text)
    $("#txtTotalSale").LoadingOverlay(text)
    $("#txtTotalGastos").LoadingOverlay(text)
    $("#txtCantidadClientes").LoadingOverlay(text)
    $("#txtGanancia").LoadingOverlay(text)
    $("#idTextFilter").LoadingOverlay(text)
}


function SetGraficoVentas(d) {

    let options = {
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
                colors: ['#f3f3f3', 'transparent'],
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

    // Si el gráfico no está inicializado, créalo
    if (!chartVentas) {
        chartVentas = new ApexCharts(document.querySelector("#chartVentas"), options);
        chartVentas.render();
    } else {
        // Actualiza los datos del gráfico existente
        chartVentas.updateOptions({
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
            xaxis: {
                categories: d.ejeX,
                title: {
                    text: d.ejeXLeyenda
                }
            },
        });
    }
}

function SetTopSeler(typeValues, idCategory) {

    if (idCategory == null) {
        idCategory = "Todo";
    }

    const tableBody = document.querySelector("#tableTopSeller");
    tableBody.innerHTML = ''
    $("#chartTopSeller").LoadingOverlay("show")
    let dateFilter = $("#txtDay").val();

    fetch(`/Admin/GetSalesByTypoVenta?typeValues=${typeValues}&idCategoria=${idCategory}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            $("#chartTopSeller").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {
            if (responseJson.state) {

                const tableDataSales = responseJson.object.map(value => {
                    return (
                        `<tr>
                       <td><h3 style="color: darkgray;">${value.product}&nbsp;</h3></td>
                       <td style="text-align: right;"><h4>&nbsp;${value.quantity}</h4></td>
                    </tr>`
                    );
                }).join('');

                tableBody.innerHTML = tableDataSales;
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        })
        .catch((error) => {
            $("#chartTopSeller").LoadingOverlay("hide")
        });
}

function SetGraficoGastosSueldos(typeValues, dateFilter) {
    $("#charGastosSueldos").LoadingOverlay("show")
    $("#gastosSueldosTexto").LoadingOverlay("show")

    fetch(`/Admin/GetGastosSueldos?typeValues=${typeValues}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            $("#charGastosSueldos").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                let respuesta = responseJson.object;

                let gastosPorTipoSueldos = respuesta.gastosPorTipoSueldos;

                $("#gastosSueldosTexto").LoadingOverlay("hide")
                $("#gastosSueldosTexto").text("$ " + respuesta.gastosSueldosTexto)
                sueldoTotal = respuesta.gastosSueldosTexto;
                setGastosGanancia();
                //let options = {
                //    series: gastosPorTipoSueldos.map((item) => { return item.total }),
                //    chart: {
                //        type: 'pie',
                //    },
                //    labels: gastosPorTipoSueldos.map((item) => { return item.descripcion }),
                //    responsive: [{
                //        breakpoint: 480,
                //        options: {
                //            chart: {
                //                width: '100%'
                //            }
                //        }
                //    }],
                //    theme: {
                //        palette: 'palette5'
                //    },
                //    title: {
                //        text: "Sueldos",
                //        align: 'left',
                //        margin: 10,
                //        offsetX: 0,
                //        offsetY: 0,
                //        floating: false,
                //        style: {
                //            fontSize: '14px',
                //            fontWeight: 'bold',
                //            fontFamily: undefined,
                //            color: '#263238'
                //        },
                //    }
                //};

                //if (!charGastosSueldos) {
                //    charGastosSueldos = new ApexCharts(document.querySelector("#charGastosSueldos"), options);
                //    charGastosSueldos.render();
                //} else {
                //    charGastosSueldos.updateOptions({
                //        series: gastosPorTipoSueldos.map((item) => { return item.total }),
                //        labels: gastosPorTipoSueldos.map((item) => { return item.descripcion })

                //    });
                //}
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}

function SetGraficoGastosProveedor(typeValues, dateFilter) {
    $("#charGastosProveedor").LoadingOverlay("show")
    $("#gastosProvvedoresTexto").LoadingOverlay("show")

    fetch(`/Admin/GetMovimientosProveedores?typeValues=${typeValues}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            $("#charGastosProveedor").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                let respuesta = responseJson.object;

                let gastoProveedores = respuesta.gastosPorTipoProveedor;

                $("#gastosProvvedoresTexto").LoadingOverlay("hide")
                $("#gastosProvvedoresTexto").text("$ " + respuesta.gastosProvvedoresTexto)
                proveedorTotal = respuesta.gastosProvvedoresTexto;
                setGastosGanancia();

                let options = {
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


                if (!charGastosProveedor) {
                    charGastosProveedor = new ApexCharts(document.querySelector("#charGastosProveedor"), options);
                    charGastosProveedor.render();
                } else {
                    charGastosProveedor.updateOptions({
                        series: gastoProveedores.map((item) => { return item.total }),
                        labels: gastoProveedores.map((item) => { return item.descripcion })
                    });
                }
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });

}

function SetGraficoGastos(typeValues, dateFilter) {
    $("#charGastos").LoadingOverlay("show")
    $("#gastoTexto").LoadingOverlay("show")

    fetch(`/Admin/GetGastos?typeValues=${typeValues}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            $("#charGastos").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                let respuesta = responseJson.object;

                let gastosPorTipo = respuesta.gastosPorTipo;
                $("#gastoTexto").LoadingOverlay("hide")
                $("#gastoTexto").text("$ " + respuesta.gastosTexto);

                gastoTotal = respuesta.gastosTexto;
                setGastosGanancia();

                let options = {
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


                if (!charGastos) {
                    charGastos = new ApexCharts(document.querySelector("#charGastos"), options);
                    charGastos.render();
                } else {
                    charGastos.updateOptions({
                        series: gastosPorTipo.map((item) => { return item.total }),
                        labels: gastosPorTipo.map((item) => { return item.descripcion })
                    });
                }
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}


function SetTipoVentas(typeValues, dateFilter) {

    $("#charTipoVentas").LoadingOverlay("show")

    fetch(`/Admin/GetSalesByTypoVentaByGrafico?typeValues=${typeValues}&dateFilter=${dateFilter}&visionGlobal=${visionGlobal}`, {
        method: "GET"
    })
        .then(response => {
            $("#charTipoVentas").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                let respuesta = responseJson.object;

                let tipoVentas = respuesta.ventasPorTipoVenta;

                let options = {
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

                if (!charTipoVentas) {
                    charTipoVentas = new ApexCharts(document.querySelector("#charTipoVentas"), options);
                    charTipoVentas.render();
                } else {
                    charTipoVentas.updateOptions({
                        series: tipoVentas.map((item) => { return item.total }),
                        labels: tipoVentas.map((item) => { return item.descripcion })
                    });
                }
            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}

function SetVentasByTiendas(typeValues, dateFilter) {

    $("#charTipoVentasGlobal").LoadingOverlay("show")

    fetch(`/Admin/GetSalesByTypoVentaByTienda?typeValues=${typeValues}&dateFilter=${dateFilter}`, {
        method: "GET"
    })
        .then(response => {
            $("#charTipoVentasGlobal").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {

                let respuesta = responseJson.object;

                let tipoVentas = respuesta.gastosPorTipo;

                let options = {
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
                    theme: {
                        palette: 'palette7'
                    },
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

                if (!chartVentasGlobal) {
                    chartVentasGlobal = new ApexCharts(document.querySelector("#charTipoVentasGlobal"), options);
                    chartVentasGlobal.render();
                } else {
                    chartVentasGlobal.updateOptions({
                        series: tipoVentas.map((item) => { return item.total }),
                        labels: tipoVentas.map((item) => { return item.descripcion })
                    });
                }

            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}

function SetProveedoresByTiendas(typeValues, dateFilter) {

    $("#charGastosProveedorGlobal").LoadingOverlay("show")

    fetch(`/Admin/GetMovimientosProveedoresByTienda?typeValues=${typeValues}&dateFilter=${dateFilter}`, {
        method: "GET"
    })
        .then(response => {
            $("#charGastosProveedorGlobal").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                let respuesta = responseJson.object;

                let tipoVentas = respuesta.gastosPorTipo;

                let options = {
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
                    theme: {
                        palette: 'palette7'
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

                if (!charGastosProveedorGlobal) {
                    charGastosProveedorGlobal = new ApexCharts(document.querySelector("#charGastosProveedorGlobal"), options);
                    charGastosProveedorGlobal.render();
                } else {
                    charGastosProveedorGlobal.updateOptions({
                        series: tipoVentas.map((item) => { return item.total }),
                        labels: tipoVentas.map((item) => { return item.descripcion })
                    });
                }

            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}


function SetGastosByTiendas(typeValues, dateFilter) {

    $("#charGastosGlobal").LoadingOverlay("show")

    fetch(`/Admin/GetGastosByTienda?typeValues=${typeValues}&dateFilter=${dateFilter}`, {
        method: "GET"
    })
        .then(response => {
            $("#charGastosGlobal").LoadingOverlay("hide")
            return response.json();
        }).then(responseJson => {

            if (responseJson.state) {
                let respuesta = responseJson.object;

                let tipoVentas = respuesta.gastosPorTipo;

                let options = {
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
                    theme: {
                        palette: 'palette7'
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

                if (!charGastosGlobal) {
                    charGastosGlobal = new ApexCharts(document.querySelector("#charGastosGlobal"), options);
                    charGastosGlobal.render();
                } else {
                    charGastosGlobal.updateOptions({
                        series: tipoVentas.map((item) => { return item.total }),
                        labels: tipoVentas.map((item) => { return item.descripcion })
                    });
                }

            }
            else {
                swal("Lo sentimos", "Se ha producido un error: " + responseJson.message, "error");
            }
        });
}


$('#cboTipoDeGastoEnGasto').change(function () {
    let idTipoGasro = $(this).val();
    let tipoGasto = tipoGastosListDashboard.find(_ => _.idTipoGastos == idTipoGasro);

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

$('#cboTipoDePago').change(function () {
    tipoDeGasto = $(this).val();
    $('#txtImporte').val('');
    $("#txtIva").val('');
    $('#txtImporteSinIva').val('');
    $('#txtImporteIva').val('');

    if (tipoDeGasto == "1") { // gasto
        $(".pago-gasto").show();
        $(".pago-proveedor").hide();
    }
    else { // proveedor
        $(".pago-gasto").hide();
        $(".pago-proveedor").show();
    }
})

$('#cboProveedor').change(function () {
    let idProv = $(this).val();
    let proveedor = proveedoresListDashboard.find(_ => _.idProveedor == idProv);
    $('#txtImporte').val('');
    $("#txtIva").val('');
    $('#txtImporteSinIva').val('');
    $('#txtImporteIva').val('');

    if (proveedor != null) {
        $("#txtCuilPago").val(proveedor.cuil);
        $("#txtDireccionPago").val(proveedor.direccion);
        $("#txtIva").val(proveedor.iva != null ? proveedor.iva : '');
        $("#cboTipoFactura").val(proveedor.tipoFactura ?? '');
    }
    else {
        $("#txtCuilPago").val('');
        $("#txtDireccionPago").val('');
        $("#txtIva").val('');
        $("#cboTipoFactura").val('');
    }
    calcularIva();
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
    $("#txtCuilPago").val('');
    $("#txtDireccionPago").val('');
})

$("#btnSavePagoProveedor").on("click", function () {
    let url;
    let model;

    showLoading();
    if (tipoDeGasto == "1") { // gasto
        url = "/Gastos/CreateGastos";

        let validacion = true;
        $(".input-validate-gasto").each(function () {
            validacion = ($(this).val().trim() == "") ? false : validacion;
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
        model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;
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
        model["facturaPendiente"] = document.querySelector('#cbxFacturaPendiente').checked;
    }


    $("#modalNuevoGasto").modal("hide");
    fetch(url, {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(model)
    }).then(response => {
        removeLoading();
        return response.json();
    }).then(responseJson => {

        if (responseJson.state) {
            swal("Exitoso!", "Se registró con éxito", "success");

            SetGraficoGastosProveedor(typeValuesGlobal, '');
            SetGraficoGastos(typeValuesGlobal, '');
            SetGraficoGastosSueldos(typeValuesGlobal, '');
        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
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


function setGastosGanancia() {

    let gastos = parseFloat(parseFloat(gastoTotal) + parseFloat(proveedorTotal) + parseFloat(sueldoTotal));

    $("#txtTotalGastos").text("$ " + gastos.toFixed(0))
    $("#txtGanancia").text("$ " + (parseFloat(saleTotal) - gastos).toFixed(0));

}
