using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;
using PointOfSale.Model.Auditoria;

namespace PointOfSale.Business.Utilities
{
    public class EmailService : IEmailService
    {
        private readonly IAjusteService _ajusteService;
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly bool enableSsl;
        private readonly IMovimientoCajaService _movimientoCajaService;

        public EmailService(IAjusteService ajusteService, IMovimientoCajaService movimientoCajaService)
        {
            smtpServer = "smtp.gmail.com"; // Servidor SMTP de Gmail
            smtpPort = 587; // Puerto para TLS
            enableSsl = true; // Usar SSL
            _ajusteService = ajusteService;
            _movimientoCajaService = movimientoCajaService;
        }

        private void SendEmail(string senderEmail, string senderPassword, List<string> recipientEmails, string subject, string body)
        {
            try
            {
                // Configuración del cliente SMTP
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = enableSsl;

                    // Configuración del mensaje de correo
                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(senderEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true; // Cambia esto a false si no estás enviando HTML

                        // Agregar los destinatarios
                        foreach (string recipient in recipientEmails)
                        {
                            if (!string.IsNullOrEmpty(recipient))
                            {
                                mailMessage.To.Add(recipient);
                            }
                        }

                        // Enviar el correo electrónico
                        smtpClient.Send(mailMessage);
                    }
                }

            }
            catch (Exception ex)
            {
                new Exception($"Error al enviar el correo: {ex.Message}");
            }
        }

        private void SendEmailTicket(string senderEmail, string senderPassword, List<string> recipientEmails, string subject, string body, byte[] attachment)
        {
            try
            {
                using (SmtpClient smtpClient = new SmtpClient(smtpServer, smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(senderEmail, senderPassword);
                    smtpClient.EnableSsl = enableSsl;

                    using (MailMessage mailMessage = new MailMessage())
                    {
                        mailMessage.From = new MailAddress(senderEmail);
                        mailMessage.Subject = subject;
                        mailMessage.Body = body;
                        mailMessage.IsBodyHtml = true;

                        foreach (string recipient in recipientEmails)
                        {
                            if (!string.IsNullOrEmpty(recipient))
                            {
                                mailMessage.To.Add(recipient);
                            }
                        }

                        // Adjuntar el ticket en PDF
                        if (attachment != null)
                        {
                            mailMessage.Attachments.Add(new Attachment(new MemoryStream(attachment), "Ticket.pdf", "application/pdf"));
                        }

                        smtpClient.Send(mailMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al enviar el correo: {ex.Message}");
            }
        }

        public async Task NotificarCierreCaja(Turno turno, Dictionary<string, decimal> datosVentas, Ajustes ajustes)
        {
            var emailsReceptores = ajustes.EmailsReceptoresCierreTurno.Split(';').ToList();

            var fecha = TimeHelper.GetArgentinaTime();
            string subject = $"{ajustes.NombreTiendaTicket} [Cierre de Caja] {fecha.ToString()}";

            var movimientos = await _movimientoCajaService.GetMovimientoCajaByTurno(turno.IdTurno);

            decimal totalMovimientoEgreso = 0;
            decimal totalMovimientoIngreso = 0;
            foreach (var m in movimientos)
            {
                if (m.RazonMovimientoCaja.Tipo == TipoMovimientoCaja.Egreso)
                    totalMovimientoEgreso -= m.Importe;
                else
                    totalMovimientoIngreso += m.Importe;
            }

            string body =
                $"<h1>{ajustes.NombreTiendaTicket}</h1>" +
                $"<h3>Cierre de Caja {fecha.Date.ToString("dd/MM/yyyy")}</h3>" +
                $"<p><strong>Fecha de Inicio del Turno:</strong> {turno.FechaInicio}</p>" +
                $"<p><strong>Fecha de Fin del Turno:</strong> {turno.FechaFin}</p>" +
                $"<p><strong>Empleado del cierre de caja:</strong> {turno.ModificationUser}</p>";

            if (turno.TotalInicioCaja > 0)
            {
                body += $"<p><strong>Inicio de Caja:</strong> $ {turno.TotalInicioCaja}</p>";
            }

            if (totalMovimientoEgreso != 0)
            {
                body += $"<p><strong>Egresos de Caja:</strong> $ {totalMovimientoEgreso}</p>";
            }

            if (totalMovimientoIngreso != 0)
            {
                body += $"<p><strong>Ingresos de Caja:</strong> $ {totalMovimientoIngreso}</p>";
            }

            body +=
                $"<h4>Resumen:</h4>" +
                $"<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 25%;'>" +
                $"<thead>" +
                $"<tr><th>Metodo de pago</th><th>Importe</th></tr>" +
                $"</thead>" +
                $"<tbody>";

            var totalCaja = 0m;


            foreach (KeyValuePair<string, decimal> item in datosVentas)
            {
                var descripcion = item.Key;
                var total = item.Value;
                totalCaja += total;

                body += $"<tr><td>{descripcion}</td><td style=\"text-align: right;\">${total}</td></tr>";
            }

            body += $"<p style='font-size: 22px;'><strong>TOTAL CAJA:</strong> $ {totalCaja}</p>";
            body += "<br>";

            if (!string.IsNullOrEmpty(turno.ObservacionesApertura))
            {
                body +=
                    $"</tbody>" +
                    $"</table>" +
                    $"<br><p><strong>Observaciones del Apertura:</strong> {turno.ObservacionesApertura}</p>";
            }

            if (!string.IsNullOrEmpty(turno.ObservacionesCierre))
            {
                body +=
                    $"</tbody>" +
                    $"</table>" +
                    $"<br><p><strong>Observaciones del Cierre:</strong> {turno.ObservacionesCierre}</p>";
            }

            SendEmail(ajustes.EmailEmisorCierreTurno, ajustes.PasswordEmailEmisorCierreTurno, emailsReceptores, subject, body);
        }


        public async Task EnviarTicketEmail(int idTienda, string emailReceptor, byte[] attachment)
        {
            var ajustes = await _ajusteService.GetAjustes(idTienda);

            if (!string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno))
            {
                SendEmailTicket(ajustes.EmailEmisorCierreTurno, ajustes.PasswordEmailEmisorCierreTurno, new List<string> { emailReceptor }, "Ticket de Venta", "Adjunto se encuentra su ticket de venta.", attachment);
            }

        }
    }
}
