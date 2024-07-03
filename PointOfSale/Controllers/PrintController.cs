using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Net;
using System.Text;
using PointOfSale.Business.Contracts;

namespace PointOfSale.Controllers
{
    public class PrintController : BaseController
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PrintController> _logger;
        private readonly IAuditoriaService _auditoriaService;

        public PrintController(IHttpClientFactory httpClient, ILogger<PrintController> logger, IAuditoriaService auditoriaService)
        {
            _httpClientFactory = httpClient;
            _logger = logger;
            _auditoriaService = auditoriaService;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrinters()
        {
            _logger.LogInformation("GetPrinters called");
            _auditoriaService.SaveAuditoria("PrintController", "GetPrinters called");
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync("http://localhost:4567/getprinters");

                _auditoriaService.SaveAuditoria("PrintController", "Received response from getprinters");
                _logger.LogInformation("Received response from getprinters");

                if (!response.IsSuccessStatusCode)
                {
                    _auditoriaService.SaveAuditoria("PrintController", $"Error response from getprinters: {response.StatusCode} {response.Content} {response.ToString()}");
                    _logger.LogError($"Error response from getprinters: {response.StatusCode}");
                    return StatusCode((int)response.StatusCode, "Error fetching printers");
                }

                var content = await response.Content.ReadAsStringAsync();
                _auditoriaService.SaveAuditoria("PrintController", $"Response content: {content}");
                return Content(content, "application/json");
            }
            catch (Exception ex)
            {
                _auditoriaService.SaveAuditoria("PrintController", $"Internal server error: {ex.ToString()}");
                _logger.LogError(ex, "Exception in GetPrinters");
                return StatusCode(500, $"Internal server error: {ex.ToString()}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Imprimir([FromBody] PrintRequest printRequest)
        {
            _logger.LogInformation("Imprimir called");

            try
            {
                var client = _httpClientFactory.CreateClient();
                var jsonContent = new StringContent(JsonConvert.SerializeObject(printRequest), Encoding.UTF8, "application/json");
                var response = await client.PostAsync("http://localhost:4567/imprimir", jsonContent);

                _logger.LogInformation("Received response from imprimir");

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Error response from imprimir: {response.StatusCode}");
                    return StatusCode((int)response.StatusCode, "Error printing");
                }

                var result = await response.Content.ReadAsStringAsync();
                return Ok(JsonConvert.DeserializeObject(result));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception in Imprimir");
                return StatusCode(500, $"Internal server error: {ex.ToString()}");
            }
        }
    }

    public class PrintRequest
    {
        public string Text { get; set; }
        public string NombreImpresora { get; set; }
    }
}