using AFIP.Facturacion.Configuration;
using AFIP.Facturacion.Services;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using System;
using System.IO;
using System.Net;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AFIP.Facturacion
{
    /// <summary>
    /// Clase para crear objetos Login Tickets
    /// </summary>
    /// <remarks>
    /// Ver documentacion: 
    ///    Especificacion Tecnica del Webservice de Autenticacion y Autorizacion
    ///    Version 1.0
    ///    Departamento de Seguridad Informatica - AFIP
    /// </remarks>
    public class LoginCmsClient
    {
        //public bool IsProdEnvironment { get; set; } = false;
        public string WsaaUrlHomologation { get; set; } = "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";
        public string WsaaUrlProd { get; set; } = "https://wsaa.afip.gov.ar/ws/services/LoginCms";


        public LoginCmsClient(IOptions<AFIPConfigurationOption> configuration)
        {
            _configuration = configuration;
        }


        /// <summary>
        /// The path must end with \\
        /// </summary>
        public string TicketCacheFolderPath { get; set; } = "";

        public uint UniqueId; // Entero de 32 bits sin signo que identifica el requerimiento
        public DateTime GenerationTime; // Momento en que fue generado el requerimiento
        public DateTime ExpirationTime; // Momento en el que expira la solicitud
        public string Service; // Identificacion del WSN para el cual se solicita el TA
        public string Sign; // Firma de seguridad recibida en la respuesta
        public string Token; // Token de seguridad recibido en la respuesta
        public XmlDocument XmlLoginTicketRequest = null;
        public XmlDocument XmlLoginTicketResponse = null;
        public string CertificatePath;
        public string XmlStrLoginTicketRequestTemplate = "<loginTicketRequest><header><uniqueId></uniqueId><generationTime></generationTime><expirationTime></expirationTime></header><service></service></loginTicketRequest>";
        private bool VerboseMode = true;
        private static uint GlobalUniqueID = 0; // OJO! NO ES THREAD-SAFE
        private readonly IOptions<AFIPConfigurationOption> _configuration;

        private WsaaTicket _ticket;


        public async Task<WsaaTicket> GetWsaaTicket(AjustesFacturacion ajustes)
        {
            if (_ticket == null || _ticket.ExpirationTime <= TimeHelper.GetArgentinaTime().AddMinutes(1))
            {
                var pathCertificate = FileStorageService.ObtenerRutaCertificado(ajustes);

                _ticket = await LoginCmsAsync("wsfe", pathCertificate, ajustes.CertificadoPassword, ajustes.IsProdEnvironment, false);
            }

            return _ticket;
        }

        /// <summary>
        /// Construye un Login Ticket obtenido del WSAA
        /// </summary>
        /// <param name="service">Servicio al que se desea acceder</param>
        /// <param name="urlWsaa">URL del WSAA</param>
        /// <param name="x509CertificateFilePath">Ruta del certificado X509 (con clave privada) usado para firmar</param>
        /// <param name="password">Password del certificado X509 (con clave privada) usado para firmar</param>
        /// <param name="verbose">Nivel detallado de descripcion? true/false</param>
        /// <remarks></remarks>
        private async Task<WsaaTicket> LoginCmsAsync(string service,
                                                    string x509CertificateFilePath,
                                                    string password,
                                                    bool isProdEnvironment,
                                                    bool verbose)
        {
            var ticketCacheFile = string.IsNullOrEmpty(TicketCacheFolderPath) ?
                                        service + "ticket.json" :
                                        TicketCacheFolderPath + service + "ticket.json";

            if (File.Exists(ticketCacheFile))
            {
                var ticketJson = File.ReadAllText(ticketCacheFile);
                var ticket = JsonConvert.DeserializeObject<WsaaTicket>(ticketJson);
                if (TimeHelper.GetArgentinaTime() <= ticket.ExpirationTime)
                    return ticket;
            }

            var ID_FNC = $"[ObtenerLoginTicketResponse] Path: {x509CertificateFilePath}. ";
            CertificatePath = x509CertificateFilePath;
            VerboseMode = verbose;
            X509CertificateManager.VerboseMode = verbose;

            // PASO 1: Genero el Login Ticket Request
            try
            {
                GlobalUniqueID += 1;

                XmlLoginTicketRequest = new XmlDocument();
                XmlLoginTicketRequest.LoadXml(XmlStrLoginTicketRequestTemplate);

                var xmlNodoUniqueId = XmlLoginTicketRequest.SelectSingleNode("//uniqueId");
                var xmlNodoGenerationTime = XmlLoginTicketRequest.SelectSingleNode("//generationTime");
                var xmlNodoExpirationTime = XmlLoginTicketRequest.SelectSingleNode("//expirationTime");
                var xmlNodoService = XmlLoginTicketRequest.SelectSingleNode("//service");
                xmlNodoGenerationTime.InnerText = TimeHelper.GetArgentinaTime().AddMinutes(-10).ToString("s");
                xmlNodoExpirationTime.InnerText = TimeHelper.GetArgentinaTime().AddMinutes(+10).ToString("s");
                xmlNodoUniqueId.InnerText = Convert.ToString(GlobalUniqueID);
                xmlNodoService.InnerText = service;
                Service = service;

                if (VerboseMode) Console.WriteLine(XmlLoginTicketRequest.OuterXml);
            }
            catch (Exception ex)
            {
                throw new Exception(ID_FNC + "***Error GENERANDO el LoginTicketRequest : " + ex.ToString() + ex.StackTrace);
            }

            string pasos = string.Empty;
            string base64SignedCms;
            // PASO 2: Firmo el Login Ticket Request
            try
            {
                if (VerboseMode) Console.WriteLine(ID_FNC + "***Leyendo certificado: {0}", CertificatePath);
                pasos += $"*1**Leyendo certificado: {CertificatePath}.\n";

                var securePassword = new NetworkCredential("", password).SecurePassword;
                securePassword.MakeReadOnly();

                pasos += $"*1.5**Leyendo certificado.\n";
                var certFirmante = X509CertificateManager.GetCertificateFromFile(CertificatePath, securePassword);
                pasos += $"*2**GetCertificateFromFile ";

                if (VerboseMode)
                {
                    Console.WriteLine(ID_FNC + "***Firmando: ");
                    Console.WriteLine(XmlLoginTicketRequest.OuterXml);
                }
                pasos += $"*3**Firmandoo: {XmlLoginTicketRequest.OuterXml} ";

                // Convierto el Login Ticket Request a bytes, firmo el msg y lo convierto a Base64
                var msgEncoding = Encoding.UTF8;
                var msgBytes = msgEncoding.GetBytes(XmlLoginTicketRequest.OuterXml);
                pasos += $"*4**intermedio 4. ";

                var encodedSignedCms = X509CertificateManager.SignMessageBytes(msgBytes, certFirmante);
                pasos += $"*5**intermedio 5. ";
                base64SignedCms = Convert.ToBase64String(encodedSignedCms);
            }
            catch (Exception ex)
            {
                throw new Exception(ID_FNC + "\n" + pasos + "\n***Error FIRMANDO el LoginTicketRequest : " + ex.ToString());
            }

            string loginTicketResponse;
            // PASO 3: Invoco al WSAA para obtener el Login Ticket Response
            try
            {
                if (VerboseMode)
                {
                    Console.WriteLine(ID_FNC + "***Llamando al WSAA en URL: {0}", isProdEnvironment ? WsaaUrlProd : WsaaUrlHomologation);
                    Console.WriteLine(ID_FNC + "***Argumento en el request:");
                    Console.WriteLine(base64SignedCms);
                }

                var wsaaService = new AfipLoginCmsServiceReference.LoginCMSClient();
                wsaaService.Endpoint.Address = new EndpointAddress(isProdEnvironment ? WsaaUrlProd : WsaaUrlHomologation);

                var response = await wsaaService.loginCmsAsync(base64SignedCms);
                loginTicketResponse = response.loginCmsReturn;

                if (VerboseMode)
                {
                    Console.WriteLine(ID_FNC + "***LoguinTicketResponse: ");
                    Console.WriteLine(loginTicketResponse);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ID_FNC + "***Error INVOCANDO al servicio WSAA : " + ex.ToString());
            }

            // PASO 4: Analizo el Login Ticket Response recibido del WSAA
            try
            {
                XmlLoginTicketResponse = new XmlDocument();
                XmlLoginTicketResponse.LoadXml(loginTicketResponse);

                UniqueId = uint.Parse(XmlLoginTicketResponse.SelectSingleNode("//uniqueId").InnerText);
                GenerationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//generationTime").InnerText);
                ExpirationTime = DateTime.Parse(XmlLoginTicketResponse.SelectSingleNode("//expirationTime").InnerText);
                Sign = XmlLoginTicketResponse.SelectSingleNode("//sign").InnerText;
                Token = XmlLoginTicketResponse.SelectSingleNode("//token").InnerText;
            }
            catch (Exception ex)
            {
                throw new Exception(ID_FNC + "***Error ANALIZANDO el LoginTicketResponse : " + ex.ToString());
            }

            var ticketResponse = new WsaaTicket { Sign = Sign, Token = Token, ExpirationTime = ExpirationTime };
            File.WriteAllText(ticketCacheFile, JsonConvert.SerializeObject(ticketResponse));

            return ticketResponse;
        }
    }
}
