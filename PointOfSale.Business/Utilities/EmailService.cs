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

        private void SendEmail(string senderEmail, string senderPassword, List<string> recipientEmails, string subject, string body, byte[]? attachment = null)
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

        public async Task NotificarCierreCaja(Turno turno, Ajustes ajustes)
        {
            var emailsReceptores = ajustes.EmailsReceptoresCierreTurno.Split(';').ToList();

            var fecha = TimeHelper.GetArgentinaTime();
            string subject = $"[Cierre de Caja] {fecha.ToString()}";

            decimal totalMovimientoEgreso = 0;
            decimal totalMovimientoIngreso = 0;
            foreach (var m in turno.MovimientosCaja)
            {
                if (m.RazonMovimientoCaja.Tipo == TipoMovimientoCaja.Egreso)
                    totalMovimientoEgreso -= m.Importe;
                else
                    totalMovimientoIngreso += m.Importe;
            }

            string body =
                $"<h1>{ajustes.Encabezado1}</h1>" +
                $"<h3>Cierre de Caja {fecha.Date.ToString("dd/MM/yyyy")}</h3>" +
                $"<p><strong>Inicio:</strong> {turno.FechaInicio.ToShortTimeString()}</p>" +
                $"<p><strong>Fin:</strong> {turno.FechaFin.Value.ToShortTimeString()}</p>" +
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

            body += "<hr>";


            body += $"<p><strong>TOTAL Sistema:</strong> $ {turno.TotalCierreCajaSistema.Value.ToString("0")}</p>";

            body +=
                $"<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 50%;'>" +
                $"<thead>" +
                $"<tr><th>Metodo de pago</th><th>Importe</th></tr>" +
                $"</thead>" +
                $"<tbody>";

            foreach (var item in turno.VentasPorTipoDeVenta)
            {
                body += $"<tr><td>{item.Descripcion}</td><td style=\"text-align: right;\">${item.TotalSistema.Value.ToString("0")}</td></tr>";
            }

            body += "</tbody></table><br>";

            if (ajustes.ControlTotalesCierreTurno.HasValue && ajustes.ControlTotalesCierreTurno.Value)
            {
                body += $"<p><strong>TOTAL Usuario:</strong> $ {turno.TotalCierreCajaReal.Value.ToString("0")}</p>";

                body +=
                    $"<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 50%;'>" +
                    $"<thead>" +
                    $"<tr><th>Metodo de pago</th><th>Importe</th></tr>" +
                    $"</thead>" +
                    $"<tbody>";

                foreach (var item in turno.VentasPorTipoDeVenta)
                {
                    body += $"<tr><td>{item.Descripcion}</td><td style=\"text-align: right;\">${item.TotalUsuario.Value.ToString("0")}</td></tr>";
                }
                body += "</tbody></table><br>";
            }

            // Diferencias de caja
            if (!string.IsNullOrEmpty(turno.ErroresCierreCaja))
            {
                var diferenciasArray = turno.ErroresCierreCaja.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                var difCaja = string.Empty;
                var montoDiferencia = System.Text.RegularExpressions.Regex.Match(diferenciasArray[diferenciasArray.Length - 1], @"\$ -?(\d+)");
                if (montoDiferencia.Success)
                {
                    difCaja = montoDiferencia.Groups[0].Value.Replace(" ", "");
                }
                body += $"<p><strong>DIFERENCIA de Caja:</strong> {difCaja}</p>";


                // Iniciar la tabla de diferencias
                body += "<table border='1' cellpadding='5' cellspacing='0' style='border-collapse: collapse; width: 50%;'>" +
                         "<thead><tr><th>Metodo de pago</th><th>Importe</th></tr></thead><tbody>";

                foreach (var diferencia in diferenciasArray)
                {
                    var descripcionMatch = System.Text.RegularExpressions.Regex.Match(diferencia, @"'(.+?)'");
                    var montoMatch = System.Text.RegularExpressions.Regex.Match(diferencia, @"\$ -?(\d+)");

                    if (descripcionMatch.Success && montoMatch.Success)
                    {
                        body += $"<tr><td>{descripcionMatch.Groups[1].Value}</td><td style=\"text-align: right;\">{montoMatch.Groups[0].Value.Replace(" ", "")}</td></tr>";
                    }
                }

                body += "</tbody></table><br>";
            }

            // Observaciones del cierre
            if (!string.IsNullOrEmpty(turno.ObservacionesApertura))
            {
                body += $"<p><strong>Observaciones del Apertura:</strong> {turno.ObservacionesApertura}</p>";
            }

            if (!string.IsNullOrEmpty(turno.ObservacionesCierre))
            {
                body += $"<p><strong>Observaciones del Cierre:</strong> {turno.ObservacionesCierre}</p>";
            }

            // Enviar el correo
            SendEmail(ajustes.EmailEmisorCierreTurno, ajustes.PasswordEmailEmisorCierreTurno, emailsReceptores, subject, body);
        }



        public async Task EnviarTicketEmail(int idTienda, string emailReceptor, byte[] attachment)
        {
            var ajustes = await _ajusteService.GetAjustes(idTienda);

            if (!string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno))
            {
                SendEmail(ajustes.EmailEmisorCierreTurno, ajustes.PasswordEmailEmisorCierreTurno, new List<string> { emailReceptor }, "Ticket de Venta", "Adjunto se encuentra su ticket de venta.", attachment);
            }

        }
    }
}
