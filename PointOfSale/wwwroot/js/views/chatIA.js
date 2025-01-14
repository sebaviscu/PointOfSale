

$(document).ready(function () {
    showLoading();

    fetch("/ChatIA/GetChat")
        .then(response => response.json())
        .then(responseJson => {
            removeLoading();
            if (responseJson.object.length > 0) {
                list = responseJson.object;

                list.forEach((item) => {
                    // Formatear fecha/hora
                    let fechaFormateada = moment(item.createdAt).format("DD/MM/YYYY HH:mm");
                    addMessageToChat("Tu", item.question, fechaFormateada);
                    addMessageToChat("Sistema", item.content, '');
                });
            }
        })
});

// Envío de mensaje con Enter
$('#input').on('keypress', function (e) {
    if (e.which === 13 && $(this).val().trim() !== '') {
        var userMessage = $(this).val().trim();
        // Puedes formatear la fecha actual si quieres
        let fechaActual = moment().format("DD/MM/YYYY HH:mm");
        addMessageToChat('Tú', userMessage, fechaActual);

        $(this).val('');

        // Llamada AJAX
        $.ajax({
            url: '/chatIA/ask',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify({ question: userMessage }),
            success: function (response) {

                addMessageToChat("Sistema", response.content, '');

            },
            error: function () {
                addMessageToChat('Asistente', 'Error al procesar la pregunta.');
            }
        });
    }
});



function addMessageToChat(sender, message, date) {

    // Primero agregamos la fecha/hora, centrada (si viene definida).
    // Si no quieres repetir la fecha en cada mensaje, podrías manejar lógica 
    // para solo mostrarla una vez, o cuando cambie la fecha/hora, etc.
    let htmlMessage = "";
    if (date) {
        htmlMessage += `
            <div class="message-date-centered">${date}</div>
        `;
    }

    // Luego, dependiendo del emisor, mostramos la burbuja a la izquierda o derecha
    if (sender === "Sistema") {
        htmlMessage += `
            <div class="message-bubble system">
                <strong>${sender}:</strong> ${message}
            </div>`;
    } else {
        // "Tu", "Tú", "Usuario", etc.
        htmlMessage += `
            <div class="message-bubble user">
                <strong>${sender}:</strong> ${message}
            </div>`;
    }

    // Finalmente, lo insertamos en el chat
    $("#chat").append(htmlMessage);
    $("#chat").scrollTop($("#chat")[0].scrollHeight);
}
