$(document).ready(function () {


    //document.querySelector('input[name="radioButton"]:checked').value

    changeChart(1);


})

function changeChart(typeValues) {
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

                $("#totalSale").text(d.totalSales);
                $("#totalIncome").text(d.totalIncome)
                $("#totalProducts").text(d.totalProducts)
                $("#totalCategories").text(d.totalCategories)

                //let barchart_labels;
                //let barchar_data;
                //if (d.salesLastWeek.length > 0) {
                //    barchart_labels = d.salesLastWeek.map((item) => { return item.date })
                //    barchar_data = d.salesLastWeek.map((item) => { return item.total })
                //} else {
                //    barchart_labels = ["without results"]
                //    barchar_data = [0]
                //}

                let piechart_labels;
                let piechart_data;
                if (d.productsTopLastWeek.length > 0) {
                    piechart_labels = d.productsTopLastWeek.map((item) => { return item.product })
                    piechart_data = d.productsTopLastWeek.map((item) => { return item.quantity })
                } else {
                    piechart_labels = ["without results"];
                    piechart_data = [0];
                }

                


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

                const chartIds = ['chartVentas0', 'chartVentas1', 'chartVentas2'];
                const chartProductsIds = ['charProducts0', 'charProducts1', 'charProducts2'];

                for (let i = 0; i < chartIds.length; i++) {
                    const chartElement = document.getElementById(chartIds[i]);
                    chartElement.hidden = i !== typeValues;

                    const chartProductsElement = document.getElementById(chartProductsIds[i]);
                    chartProductsElement.hidden = i !== typeValues;
                }

                // Graficvo de Lineas
                var chartNew = new ApexCharts(document.querySelector("#chartVentas" + typeValues), options);
                chartNew.render();


                // Grafico de Tortas
                let controlProduct = document.getElementById("charProducts" + typeValues);
                let myPieChart = new Chart(controlProduct, {
                    type: 'doughnut',
                    data: {
                        labels: piechart_labels,
                        datasets: [{
                            data: piechart_data,
                            backgroundColor: ['#4e73df', '#1cc88a', '#36b9cc', "#FF785B"],
                            hoverBackgroundColor: ['#2e59d9', '#17a673', '#2c9faf', "#FF5733"],
                            hoverBorderColor: "rgba(234, 236, 244, 1)",
                        }],
                    },
                    options: {
                        maintainAspectRatio: false,
                        tooltips: {
                            backgroundColor: "rgb(255,255,255)",
                            bodyFontColor: "#858796",
                            borderColor: '#dddfeb',
                            borderWidth: 1,
                            xPadding: 15,
                            yPadding: 15,
                            displayColors: false,
                            caretPadding: 10,
                        },
                        legend: {
                            display: true
                        },
                        cutoutPercentage: 80,
                    },
                });

            }
        })
        .catch((error) => {
            $("div.container-fluid").LoadingOverlay("hide")
        });





}