using System.Text;
using System.Text.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Externos.PrintServices.ResponseModel;
using Microsoft.Extensions.Configuration;
using AfipServiceReference;
using AFIP.Facturacion.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Externos.PrintServices
{
    public class PrintService : IPrintService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private const string UrlPrintService = "https://localhost:4568";
        private readonly string BearerToken;

        public PrintService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            BearerToken = configuration["PrintToken"] ?? throw new Exception("El PrintToken no está configurado");
        }

        private async Task<HttpResponseMessage> FetchWithTimeoutAsync(string resource, HttpContent content, HttpMethod method, int timeout = 50000)
        {
            using var httpClient = _httpClientFactory.CreateClient();
            using var cts = new CancellationTokenSource(timeout);

            var request = new HttpRequestMessage(method, $"{UrlPrintService}{resource}")
            {
                Content = method == HttpMethod.Post ? content : null
            };

            // Agregar el encabezado de autorización con el token Bearer
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", BearerToken);

            try
            {
                var response = await httpClient.SendAsync(request, cts.Token);
                return response;
            }
            catch (TaskCanceledException) when (!cts.Token.IsCancellationRequested)
            {
                throw new TimeoutException("La solicitud ha excedido el tiempo de espera.");
            }
        }

        public async Task<bool> GetHealthcheckAsync()
        {
            try
            {
                // Crear la solicitud GET con un timeout de 5000 ms
                var response = await FetchWithTimeoutAsync("/healthcheck", null, HttpMethod.Get, timeout: 5000);

                if (!response.IsSuccessStatusCode)
                {
                    Console.Error.WriteLine($"Healthcheck fallido: {response.ReasonPhrase}");
                    return false;
                }

                // Leer la respuesta y deserializarla
                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<HealthcheckResponse>(responseData);

                return data != null && data.Success;
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("La solicitud de Healthcheck ha excedido el tiempo de espera.");
                return false;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error durante el Healthcheck: {ex.Message}");
                return false;
            }
        }

        public async Task PrintTicketAsync(string text, string printerName, string[] imagesTicket)
        {
            var requestBody = new
            {
                nombreImpresora = printerName,
                text = text,
                images = imagesTicket
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await FetchWithTimeoutAsync("/imprimir", content, HttpMethod.Post, timeout: 15000);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Network response was not ok: {response.ReasonPhrase}");
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<ResponseModel>(responseData);

                if (data?.Success == true)
                {
                    Console.WriteLine("Documento enviado a la impresora con éxito");
                }
                else
                {
                    Console.Error.WriteLine($"Error al enviar el documento a la impresora: {data?.Error}");
                }
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Error: La solicitud de impresión ha excedido el tiempo de espera.");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al enviar el documento a la impresora: {ex.Message}");
            }
        }

        public async Task<List<string>> GetPrintersAsync()
        {
            try
            {
                var response = await FetchWithTimeoutAsync("/getprinters", null, HttpMethod.Get, timeout: 10000);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Network response was not ok: {response.ReasonPhrase}");
                }

                // Leer la respuesta y deserializarla
                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<PrintersResponse>(responseData);

                if (data != null && data.Success)
                {
                    return data.Printers;
                }
                else
                {
                    Console.Error.WriteLine($"Error fetching printers: {data?.Error}");
                    return new List<string>();
                }
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("GetPrinters request timed out.");
                return new List<string>();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error fetching printers: {ex.Message}");
                return new List<string>();
            }
        }


        public async Task<int> GetLastAuthorizedReceiptAsync(int ptoVenta, int idTipoComprobante)
        {
            var requestBody = new
            {
                ptoVenta,
                idTipoComprobante
            };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            try
            {
                var response = await FetchWithTimeoutAsync("/getLastAuthorizedReceipt", content, HttpMethod.Post, timeout: 60000);

                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<LastAuthorizedReceiptResponse>(responseData);

                if (!data.Success)
                {
                    throw new Exception(data.Error);
                }

                return data.NumeroComprobante;
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Error: La solicitud de ultimo comprobante ha excedido el tiempo de espera.");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error a al solicitud de ultimo comprobante: {ex.Message}");
                throw;
            }
        }

        public async Task<FacturacionResponse> FacturarAsync(FacturaAFIP factura)
        {

            var content = new StringContent(JsonSerializer.Serialize(factura), Encoding.UTF8, "application/json");

            try
            {
                var response = await FetchWithTimeoutAsync("/invoice", content, HttpMethod.Post, timeout: 120000);

                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<InvoiceResponse>(responseData);

                if (!data.Success) 
                {
                    throw new Exception(data.Error);
                }

                return data.Invoice;
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Error: La solicitud de facturar ha excedido el tiempo de espera.");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error a la solicitud de facturar: {ex.Message}");
                throw;
            }
        }

        private class ResponseModel
        {
            public bool Success { get; set; }
            public string Error { get; set; }
        }
    }
}
