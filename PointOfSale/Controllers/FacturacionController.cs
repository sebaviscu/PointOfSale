using AFIP.Facturacion.Extensions;
using AFIP.Facturacion.Model;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class FacturacionController : BaseController
    {
        private readonly ILogger<FacturacionController> _logger;
        private readonly IMapper _mapper;
        private readonly IAfipService _afipService;

        public FacturacionController(ILogger<FacturacionController> logger, IMapper mapper, IAfipService afipService)
        {
            _afipService = afipService;
            _logger = logger;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        [HttpGet]
        public async Task<IActionResult> FacturacionById(int idFacturaEmitida)
        {
            ValidarAutorizacion([Roles.Administrador]);

            if (!HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Access");

            var factura = _mapper.Map<VMFacturaEmitida>(await _afipService.GetById(idFacturaEmitida));
            ViewData["IdSale"] = factura.IdSale;

            return View("Facturacion");
        }

        /// <summary>
        /// Retorna las facturas para DataTable
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetFacturas()
        {
            var gResponse = new GenericResponse<List<VMFacturaEmitida>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var facturas = await _afipService.GetAllTakeLimit(user.IdTienda);
                var vmFactura = _mapper.Map<List<VMFacturaEmitida>>(facturas);

                return StatusCode(StatusCodes.Status200OK, new { data = vmFactura });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar facturas", _logger);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetFactura(int idFacturaEmitida)
        {
            var gResponse = new GenericResponse<VMFacturaEmitida>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var factura = await _afipService.GetById(idFacturaEmitida);

                var vmFactura = _mapper.Map<VMFacturaEmitida>(factura);

                if (factura.Observaciones == null && factura.NroDocumento.HasValue)
                {
                    vmFactura.QR = await _afipService.GenerateLinkAfipFactura(factura);
                }

                gResponse.State = true;
                gResponse.Object = vmFactura;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar facturas", _logger, idFacturaEmitida);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> NotaCredito(int idFacturaEmitida)
        {
            var gResponse = new GenericResponse<FacturaAFIP>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var credito = await _afipService.NotaCredito(idFacturaEmitida, user.UserName);

                gResponse.State = true;
                gResponse.Object = credito;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar facturas", _logger, idFacturaEmitida);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Refacturar(int idFacturaEmitida, string? cuil = null)
        {
            var gResponse = new GenericResponse<FacturaAFIP>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var factura = await _afipService.Refacturar(idFacturaEmitida, cuil, user.UserName);

                //if (factura.Observaciones == null)
                //{
                //    vmFactura.QR = await _afipService.GenerateLinkAfipFactura(factura);
                //}

                gResponse.State = true;
                gResponse.Object = factura;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar facturas", _logger, idFacturaEmitida);
            }
        }
        
        [HttpPost]
        public async Task<IActionResult> SaveInvoice([FromBody] SaveInvoiceRequest request)
        {
            var gResponse = new GenericResponse<VMFacturaEmitida>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var factura = await _afipService.SaveFacturaEmitida(request.Facturacion, request.IdFacturaEmitida);

                var vmFactura = _mapper.Map<VMFacturaEmitida>(factura);

                if (factura.Observaciones == null)
                {
                    vmFactura.QR = await _afipService.GenerateLinkAfipFactura(factura);
                }

                gResponse.State = true;
                gResponse.Object = vmFactura;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al guardar factura", _logger, request.Facturacion);
            }
        }

        [HttpPut]
        public async Task<IActionResult> ErrorInvoice(int IdFacturaEmitida, string error)
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var factura = await _afipService.EditeFacturaError(IdFacturaEmitida, error);

                var vmFactura = _mapper.Map<VMFacturaEmitida>(factura);

                return StatusCode(StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear error en invoice", _logger, IdFacturaEmitida);
            }
        }

    }

    public class SaveInvoiceRequest
    {
        public FacturacionResponse Facturacion { get; set; }
        public int IdFacturaEmitida { get; set; }
    }

}
