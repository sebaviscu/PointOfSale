
const urlPrintService = 'https://localhost:4568';
async function getHealthcheck() {
    try {
        const response = await fetch(urlPrintService + '/healthcheck');
        if (!response.ok) {
            console.error(`Healthcheck failed: ${response.statusText}`);
            return false;
        }
        const data = await response.json();
        return data.success === true;
    } catch (error) {
        //console.error('Error during healthcheck:', error);
        return false;
    }
}

async function getPrinters() {
    try {
        const response = await fetch(urlPrintService + '/getprinters');
        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
        }
        const data = await response.json();
        if (data.success) {
            return data.printers;
        } else {
            console.error('Error fetching printers:', data.error);
            return [];
        }
    } catch (error) {
        console.error('Error:', error);
        return [];
    }
}

async function printTicket(text, printerName, urlQr) {
    try {
        const response = await fetch(urlPrintService + '/imprimir', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ nombreImpresora: printerName, text: text, qrPath: urlQr })
        });
        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
        }
        const data = await response.json();
        if (data.success) {
            console.log('Documento enviado a la impresora con éxito');
            console.log('Elimina imagen qr: ' + urlQr);
            deleteImgQr(urlQr);
        } else {
            console.error('Error al enviar el documento a la impresora:', data.error);
        }
    } catch (error) {
        alert(`Error al enviar el documento a la impresora: ${error}`);
        console.error('Error:', error);
    }
}

function deleteImgQr(urlQr) {
    fetch(`/Sales/DeleteImgQr?urlQr=${urlQr}`, {
        method: "DELETE"
    }).then(response => {
        $(".showSweetAlert").LoadingOverlay("hide")
        return response.json();
    }).then(responseJson => {
        if (!responseJson.state) {
            swal("Lo sentimos", responseJson.message, "error");
        }
    })
        .catch((error) => {
            $(".showSweetAlert").LoadingOverlay("hide")
        })
}