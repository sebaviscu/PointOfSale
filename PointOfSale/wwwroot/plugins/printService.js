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
        const response = await fetch("/print/healthcheck", { timeout: 5000 });

        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
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
        const response = await fetch("/print/getprinters", { timeout: 10000 });

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
        if (error.name === 'AbortError') {
            console.error('GetPrinters request timed out');
        } else {
            console.error('Error fetching printers:', error);
        }
        return [];

    }
}

async function printTicket(text, printerName, imagesTicket) {
    try {
        const response = await fetch("/print/imprimir", {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                text: text,
                printerName: printerName,
                imagesTicket: imagesTicket
            }),
            timeout: 15000 // Timeout de 15 segundos
        });

        console.info('paso por imprimir');
        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
        }

        const data = await response.json();
        if (data.success) {
            console.log("Documento enviado a la impresora con éxito");
        } else {
            console.error("Error al enviar el documento a la impresora:", data.error);
        }
    } catch (error) {
        if (error.name === "AbortError") {
            console.error("PrintTicket request timed out");
        } else {
            console.error("Error al enviar el documento a la impresora:", error);
        }
    }
}