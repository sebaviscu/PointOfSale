using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Controllers;

namespace PointOfSale.Web.Controllers
{
    public class PrintController : BaseController
    {
        private readonly IPrintService _printService;
        public PrintController(IPrintService printService)
        {
            _printService = printService;
        }
        
        [HttpGet]
        public async Task<IActionResult> Healthcheck()
        {
            var result = await _printService.GetHealthcheckAsync();
            if (result)
            {
                return Ok(new { success = true });
            }
            else
            {
                return StatusCode(500, new { success = false });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Imprimir([FromBody] PrintTicketRequest request)
        {
            await _printService.PrintTicketAsync(request.Text, request.PrinterName, request.ImagesTicket);
            return Ok(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetPrinters()
        {
            var printers = await _printService.GetPrintersAsync();
            return Ok(new { success = true, printers });
        }
    }

    public class PrintTicketRequest
    {
        public string Text { get; set; }
        public string PrinterName { get; set; }
        public string[] ImagesTicket { get; set; }
    }
}
