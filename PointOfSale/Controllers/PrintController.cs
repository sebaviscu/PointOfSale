using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Controllers;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Model;
using PointOfSale.Models;
using AutoMapper;
using PointOfSale.Utilities.Response;
using ZXing;
using PointOfSale.Business.Utilities;

namespace PointOfSale.Web.Controllers
{
    public class PrintController : BaseController
    {
        private readonly ITicketService _ticketService;
        private readonly IAjusteService _ajusteService;
        private readonly ISaleService _saleService;
        private readonly ILogger<PrintController> _logger;
        private readonly IMapper _mapper;

        public PrintController(ITicketService ticketService, IAjusteService ajusteService, ISaleService saleService, ILogger<PrintController> logger, IMapper mapper)
        {
            _ticketService = ticketService;
            _ajusteService = ajusteService;
            _saleService = saleService;
            _logger = logger;
            _mapper = mapper;
        }


        [HttpPost]
        public async Task<IActionResult> Imprimir([FromBody] PrintTicketRequest printTicketRequest)
        {
            var gResponse = new GenericResponse<TicketModel>();

            var user = ValidarAutorizacion();
            try
            {
                var ajustes = await _ajusteService.GetAjustes(user.IdTienda);
                var saleCreated = await _saleService.GetSale(printTicketRequest.IdSale);

                FacturaEmitida facturaEmitida = null;

                if (printTicketRequest.FacturaEmitida != null)
                {
                    facturaEmitida = _mapper.Map<FacturaEmitida>(printTicketRequest.FacturaEmitida);
                }

                var ticket = await _ticketService.TicketSale(saleCreated, ajustes, facturaEmitida);

                gResponse.State = true;
                gResponse.Object = ticket;
                return StatusCode(StatusCodes.Status200OK, gResponse);

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear ticket para imprimir.", _logger, printTicketRequest);
            }
        }

    }

    public class PrintTicketRequest
    {
        public int IdSale { get; set; }
        public VMFacturaEmitida? FacturaEmitida { get; set; }
    }
}
