using System.Text;
using System.Text.Json;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Externos.PrintServices.ResponseModel;
using Microsoft.Extensions.Configuration;

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

        private async Task<HttpResponseMessage> FetchWithTimeoutAsync(string resource, HttpContent content, HttpMethod method, int timeout = 10000)
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
                var response = await FetchWithTimeoutAsync("/getLastAuthorizedReceipt", content, HttpMethod.Post, timeout: 150000);

                var responseData = await response.Content.ReadAsStringAsync();
                var data = JsonSerializer.Deserialize<LastAuthorizedReceiptResponse>(responseData);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Network response was not ok: {response.ReasonPhrase}");
                }

                return data.NumeroComprobante;
            }
            catch (TimeoutException)
            {
                Console.Error.WriteLine("Error: La solicitud de impresión ha excedido el tiempo de espera.");
                throw;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error al enviar el documento a la impresora: {ex.Message}");
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
