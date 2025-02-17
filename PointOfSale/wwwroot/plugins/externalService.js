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
            swal("", "Imprimiendo ticket", "success");
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

function InvoiceSale(imprimirTicket, saleResult) {
    showLoading();

    for (let f of saleResult.facturasAFIP) {
        getLastAuthorizedReceipt(f.cabecera.puntoVenta, f.cabecera.tipoComprobante.id)
            .then(nroComprobante => {
                f.detalle.forEach(d => {
                    nroComprobante++;
                    d.nroComprobanteDesde = nroComprobante;
                    d.nroComprobanteHasta = nroComprobante;

                    getInvoicing(f)
                        .then(i => {

                            let saveInvoice = {
                                facturacion: i,
                                idSale: parseInt(saleResult.idSale)
                            };

                            fetch("/Facturacion/SaveInvoice", {
                                method: "POST",
                                headers: { 'Content-Type': 'application/json;charset=utf-8' },
                                body: JSON.stringify(saveInvoice)
                            }).then(response => {
                                return response.json();
                            }).then(responseInvoice => {
                                removeLoading();
                                printTicketSale(imprimirTicket, saleResult, responseInvoice.object);

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
    }
}

function saveNotificationInvoiceError(saleNumber, error) {
    removeLoading();
    swal("Error al Facturar", "La venta fué registrada correctamente, pero no se ha podido facturar.\n", "warning");

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