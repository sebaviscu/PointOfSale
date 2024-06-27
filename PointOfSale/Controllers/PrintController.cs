using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace PointOfSale.Controllers
{
    public class PrintController : BaseController
    {
        private readonly HttpClient _httpClient;

        public PrintController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrinters()
        {
            var response = await _httpClient.GetAsync("http://localhost:4567/getprinters");
            var content = await response.Content.ReadAsStringAsync();
            return Content(content, "application/json");
        }

        [HttpPost]
        public async Task<IActionResult> Imprimir([FromBody] PrintRequest printRequest)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(printRequest), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("http://localhost:4567/imprimir", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync();
                return Ok(JsonConvert.DeserializeObject(result));
            }

            return StatusCode((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }
    }

    public class PrintRequest
    {
        public string Text { get; set; }
        public string NombreImpresora { get; set; }
    }
}