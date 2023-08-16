var typeValuesGlobal = 0;

$(document).ready(function () {


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

    changeChart(1);
})

$('#cboCategory').change(function () {
    SetTopSeler(typeValuesGlobal, $(this).val());
})

function changeChart(typeValues) {
    typeValuesGlobal = typeValues;

    $("div.container-fluid").LoadingOverlay("show")

    fetch(`/Admin/GetSummary?typeValues=${typeValues}`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            if (responseJson.state) {

                let d = responseJson.object;

                $("#txtTotalSale").text(d.totalSales);
                $("#txtTotalGastos").text(d.gastos)
                $("#txtCantidadClientes").text(d.cantidadClientes)
                $("#txtGanancia").text(d.ganancia)

                var cont = document.getElementById('containerMetodosPago');
                cont.innerHTML = "";
                // Lista tipo de ventas
                var ul = document.createElement('ul');
                ul.setAttribute('style', 'padding: 0; margin: 0;');
                ul.setAttribute('id', 'theList');

                for (i = 0; i <= d.ventasPorTipoVenta.length - 1; i++) {
                    var li = document.createElement('li');     
                    li.innerHTML = d.ventasPorTipoVenta[i].descripcion + ": $" + d.ventasPorTipoVenta[i].total;      
                    li.setAttribute('style', 'display: block;');    // remove the bullets.
                    ul.appendChild(li);     // append li to ul.
                }
                cont.appendChild(ul);       // add list to the container.


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

                SetTopSeler(typeValuesGlobal, $('#cboCategory').val());
            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });
}

function SetTopSeler(typeValues, idCategory) {

    fetch(`/Admin/GetSalesByTypoVenta?typeValues=${typeValues}&idCategoria=${idCategory}`, {
        method: "GET"
    })
        .then(response => {
            $("div.container-fluid").LoadingOverlay("hide")
            return response.ok ? response.json() : Promise.reject(response);
        }).then(responseJson => {

            var options = {
                series: responseJson.map((item) => { return item.quantity }),
                chart: {
                    type: 'donut',
                },
                labels: responseJson.map((item) => { return item.product }),
                responsive: [{
                    breakpoint: 480,
                    options: {
                        chart: {
                            width: 200
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }]
            };

            var chartNew = new ApexCharts(document.querySelector("#charProducts"), options);
            chartNew.render();

            chartNew.updateOptions({
                series: responseJson.map((item) => { return item.quantity }),
                chart: {
                    type: 'donut',
                },
                labels: responseJson.map((item) => { return item.product }),
                responsive: [{
                    breakpoint: 480,
                    options: {
                        chart: {
                            width: 200
                        },
                        legend: {
                            position: 'bottom'
                        }
                    }
                }]
            })


        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });

}