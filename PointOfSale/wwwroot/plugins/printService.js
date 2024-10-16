﻿
async function getHealthcheck() {
    try {
        const response = await fetch('https://localhost:4568/healthcheck');
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
        const response = await fetch(`https://localhost:4568/getprinters`);
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

async function printTicket(text, printerName) {
    try {
        const response = await fetch(`https://localhost:4568/imprimir`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ nombreImpresora: printerName, text: text })
        });
        if (!response.ok) {
            throw new Error(`Network response was not ok: ${response.statusText}`);
        }
        const data = await response.json();
        if (data.success) {
            console.log('Documento enviado a la impresora con éxito');
        } else {
            console.error('Error al enviar el documento a la impresora:', data.error);
        }
    } catch (error) {
        alert(`Error al enviar el documento a la impresora: ${error}`);
        console.error('Error:', error);
    }
}