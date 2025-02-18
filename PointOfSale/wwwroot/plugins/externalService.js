const urlPrintService = 'https://localhost:4568';

async function fetchWithTimeout(resource, options = {}) {
    const { timeout = 10000 } = options; // 10 segundos de timeout por defecto

    const controller = new AbortController();
    const id = setTimeout(() => controller.abort(), timeout);
    const response = await fetch(resource, {
        ...options,
        signal: controller.signal
    });
    clearTimeout(id);
    return response;
}

async function getHealthcheck() {
    try {
        const response = await fetchWithTimeout(urlPrintService + '/healthcheck', { timeout: 5000 });
        if (!response.ok) {
            console.error(`Healthcheck failed: ${response.statusText}`);
            return false;
        }
        const data = await response.json();
        return data.success === true;
    } catch (error) {
        if (error.name === 'AbortError') {
            console.error('Healthcheck request timed out');
        } else {
            console.error('Error during healthcheck:', error);
        }
        return false;
    }
}

async function getPrinters() {
    try {
        const response = await fetchWithTimeout(urlPrintService + '/getprinters', { timeout: 10000 });
        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
        }
        const data = await response.json();
        if (data.success) {
            return data.printers;
        } else {
            swal("", "Error al recuperar impresoras instaladas: " + data.error, "warning");
            return [];
        }
    } catch (error) {
        if (error.name === 'AbortError') {
            alert('Error: La solicitud de impresión ha excedido el tiempo de espera.');
            console.error('Print request timed out');
        } else {
            console.error('Error al recuperar impresoras:', error);
        }
        return [];
    }
}

async function printTicket(text, printerName, imagesTicket) {
    try {
        const response = await fetchWithTimeout(urlPrintService + '/imprimir', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ nombreImpresora: printerName, text: text, images: imagesTicket }),
            timeout: 15000 // Timeout de 15 segundos para la impresión
        });

        const data = await response.json();
        if (data.success) {
            toastr.success("", "Imprimiendo ticket");

            //swal("", "Imprimiendo ticket", "success");
        } else {
            swal("", "Error al imprimir el ticket: " + data.error, "warning");
        }
    } catch (error) {
        if (error.name === 'AbortError') {
            console.error('Print request timed out');
        } else {
            console.error('Error al querer imprimir:', error);
        }
    }
}

async function getLastAuthorizedReceipt(ptoVenta, idTipoComprobante) {
    const requestBody = {
        ptoVenta: ptoVenta,
        idTipoComprobante: idTipoComprobante
    };

    try {
        const response = await fetchWithTimeout(urlPrintService + '/getLastAuthorizedReceipt', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody),
            timeout: 30000 // Timeout de 30 segundos para la solicitud
        });

        const data = await response.json();

        if (!data.success) {
            throw new Error(data.error);
        }

        return data.numeroComprobante;
    } catch (error) {
        if (error.name === 'AbortError') {
            console.error('get Last Authorized Receipt timed out');
        } else {
            toastr.warning(error.message, "Error al recuperar ultimo comrpobante");
            console.error('Error querer recuperar ultimo comrpobante:', error);
        }
        throw error;
    }
}

async function getInvoicing(factura) {

    try {
        const response = await fetchWithTimeout(urlPrintService + '/invoice', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(factura),
            timeout: 120000 // Timeout de 120 segundos para la solicitud
        });

        const data = await response.json();

        if (!data.success) {
            throw new Error(data.error);
        }

        return data.invoice;
    } catch (error) {
        if (error.name === 'AbortError') {
            console.error('Error: La solicitud de facturar ha excedido el tiempo de espera.');
        } else {
            console.error('Error a la solicitud de facturar:', error.message);
        }
        throw error; // Vuelve a lanzar el error para manejarlo en otro lugar si es necesario
    }
}



function printTicketSale(imprimirTicket, saleResult, facturaEmitida) {

    if (isHealthySale && imprimirTicket && saleResult.nombreImpresora != null && saleResult.nombreImpresora != '') {

        let printTicketRequest = {
            idSale: parseInt(saleResult.idSale),
            facturaEmitida: facturaEmitida
        };

        fetch("/Print/Imprimir", {
            method: "POST",
            headers: { 'Content-Type': 'application/json;charset=utf-8' },
            body: JSON.stringify(printTicketRequest)
        }).then(response => {
            return response.json();
        }).then(responseTicket => {

            printTicket(responseTicket.object.ticket, saleResult.nombreImpresora, responseTicket.object.imagesTicket);

        })
            .catch(error => {
                swal("Error al imprimir", error + "\n", "warning");
            });
    }
}


async function InvoiceSale(imprimirTicket, saleResult) {
    showLoading();

    if (!saleResult.facturasAFIP || !Array.isArray(saleResult.facturasAFIP)) {
        console.error("Error: saleResult.facturasAFIP no está definido o no es un array", saleResult.facturasAFIP);
        return;
    }

    try {
        for (let f of saleResult.facturasAFIP) {
            try {
                let nroComprobante = await getLastAuthorizedReceipt(f.cabecera.puntoVenta, f.cabecera.tipoComprobante.id);

                for (let d of f.detalle) {
                    nroComprobante++;
                    d.nroComprobanteDesde = nroComprobante;
                    d.nroComprobanteHasta = nroComprobante;

                    await new Promise(resolve => setTimeout(resolve, 500)); // Espera 0.5 segundos

                    try {
                        let i = await getInvoicing(f);

                        let saveInvoice = {
                            facturacion: i,
                            idFacturaEmitida: parseInt(f.idFacturaEmitida)
                        };

                        let response = await fetch("/Facturacion/SaveInvoice", {
                            method: "POST",
                            headers: { 'Content-Type': 'application/json;charset=utf-8' },
                            body: JSON.stringify(saveInvoice)
                        });

                        let responseInvoice = await response.json();

                        removeLoading();
                        printTicketSale(imprimirTicket, saleResult, responseInvoice.object);
                    } catch (error) {
                        toastr.warning("La venta fue registrada correctamente, pero no se ha podido facturar.", "Error al Facturar");
                        saveNotificationInvoiceError(saleResult.saleNumber, error, parseInt(f.idFacturaEmitida));
                    }
                }
            } catch (error) {
                toastr.warning("La venta fue registrada correctamente, pero no se ha podido facturar.", "Error al Facturar");
                saveNotificationInvoiceError(saleResult.saleNumber, error, null);
            }

            await new Promise(resolve => setTimeout(resolve, 500)); // Espera 0.5 segundos
        }
    } catch (error) {
        console.error('Error en external service: ', error);
    }
}


function saveNotificationInvoiceError(saleNumber, error, idFacturaEmitida) {
    removeLoading();

    if (idFacturaEmitida != null) {

        fetch(`/Facturacion/ErrorInvoice?idFacturaEmitida=${idFacturaEmitida}&error=${error}`, {
            method: "PUT",
            headers: { 'Content-Type': 'application/json;charset=utf-8' }
        });
    }


    let failInvoice = {
        saleNumnber: saleNumber,
        error: error.message
    };

    fetch("/Notification/CreateNotificationsFailInvoice", {
        method: "POST",
        headers: { 'Content-Type': 'application/json;charset=utf-8' },
        body: JSON.stringify(failInvoice)
    });
}


function NotaCredito(idFact) {
    showLoading();


    fetch(`/Facturacion/NotaCredito?idFacturaEmitida=${idFact}`, {
        method: "DELETE"
    }).then(response => {
        $(".showSweetAlert").LoadingOverlay("hide");
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {


            if (responseJson.state) {
                let factura = responseJson.object;

                getLastAuthorizedReceipt(factura.cabecera.puntoVenta, factura.cabecera.tipoComprobante.id)
                    .then(nroComprobante => {
                        factura.detalle.forEach(d => {
                            nroComprobante++;
                            d.nroComprobanteDesde = nroComprobante;
                            d.nroComprobanteHasta = nroComprobante;

                            getInvoicing(factura)
                                .then(i => {

                                    let saveInvoice = {
                                        facturacion: i,
                                        idFacturaEmitida: parseInt(factura.idFacturaEmitida)
                                    };

                                    fetch("/Facturacion/SaveInvoice", {
                                        method: "POST",
                                        headers: { 'Content-Type': 'application/json;charset=utf-8' },
                                        body: JSON.stringify(saveInvoice)
                                    }).then(response => {
                                        return response.json();
                                    }).then(responseInvoice => {
                                        removeLoading();
                                        swal("Exitoso!", "La factura fué anulada", "success");

                                        location.reload();

                                    });
                                })
                                .catch(error => {
                                    saveNotificationInvoiceError(saleResult.saleNumber, error)
                                });
                        });
                    })
                    .catch(error => {
                        saveNotificationInvoiceError(saleResult.saleNumber, error)
                    });


            } else {
                swal("Lo sentimos", "No se ha podido realizar la nota de credito. Error: " + responseJson.message, "error");
            }


        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
        .catch((error) => {
            $(".showSweetAlert").LoadingOverlay("hide")
        })


}


function Refacturar(idFact) {
    showLoading();

    fetch(`/Facturacion/Refacturar?idFacturaEmitida=${idFact}`, {
        method: "POST"
    }).then(response => {
        $(".showSweetAlert").LoadingOverlay("hide");
        return response.json();
    }).then(responseJson => {
        if (responseJson.state) {


            if (responseJson.state) {
                let factura = responseJson.object;

                getLastAuthorizedReceipt(factura.cabecera.puntoVenta, factura.cabecera.tipoComprobante.id)
                    .then(nroComprobante => {
                        factura.detalle.forEach(d => {
                            nroComprobante++;
                            d.nroComprobanteDesde = nroComprobante;
                            d.nroComprobanteHasta = nroComprobante;

                            getInvoicing(factura)
                                .then(i => {

                                    let saveInvoice = {
                                        facturacion: i,
                                        idFacturaEmitida: parseInt(factura.idFacturaEmitida)
                                    };

                                    fetch("/Facturacion/SaveInvoice", {
                                        method: "POST",
                                        headers: { 'Content-Type': 'application/json;charset=utf-8' },
                                        body: JSON.stringify(saveInvoice)
                                    }).then(response => {
                                        return response.json();
                                    }).then(responseInvoice => {
                                        removeLoading();
                                        swal("Exitoso!", "La factura fué refacturada", "success");

                                        location.reload();
                                    });
                                })
                                .catch(error => {
                                    saveNotificationInvoiceError("Refacturada", error)
                                });
                        });
                    })
                    .catch(error => {
                        saveNotificationInvoiceError("Refacturada", error)
                    });


            } else {
                swal("Lo sentimos", "No se ha podido realizar la nota de credito. Error: " + responseJson.message, "error");
            }


        } else {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
        .catch((error) => {
            $(".showSweetAlert").LoadingOverlay("hide")
        })


}