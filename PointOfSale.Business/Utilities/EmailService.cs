using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;

namespace PointOfSale.Business.Utilities
{
    public class EmailService : IEmailService
    {
        private readonly IAjusteService _ajusteService;
        private readonly string smtpServer;
        private readonly int smtpPort;
        private readonly bool enableSsl;

        public EmailService(IAjusteService ajusteService)
        {
            smtpServer = "smtp.gmail.com"; // Servidor SMTP de Gmail
            smtpPort = 587; // Puerto para TLS
            enableSsl = true; // Usar SSL
            _ajusteService = ajusteService;
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
                throw new Exception($"Error al enviar el correo: {ex.Message}");
            }
        }

        public async Task NotificarCierreCaja(int idTienda)
        {
            var ajustes = await _ajusteService.GetAjustes(idTienda);

            if (ajustes.NotificarEmailCierreTurno.HasValue && ajustes.NotificarEmailCierreTurno.Value 
                && !string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.EmailsReceptoresCierreTurno))
            {
                var emailsReceptores = ajustes.EmailsReceptoresCierreTurno.Split(';').ToList();

                string subject = $"{ajustes.NombreTiendaTicket} [Cierre de Caja] {TimeHelper.GetArgentinaTime().ToString()}";
                string body = 
                    $"<h1>{ajustes.NombreTiendaTicket}</h1>" +
                    $"<h3>Cierre de caja</h3>" +
                    $"<p>Total: $123</p>";

                SendEmail(ajustes.EmailEmisorCierreTurno, ajustes.PasswordEmailEmisorCierreTurno, emailsReceptores, subject, body);
            }

        }
    }
}
