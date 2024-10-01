using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
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
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
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
                var facturas = await _afipService.GetAll(user.IdTienda);
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
                return HandleException(ex, "Error al recuperar facturas", _logger, idFacturaEmitida);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> NotaCredito(int idFacturaEmitida)
        {
            var gResponse = new GenericResponse<VMFacturaEmitida>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var credito = await _afipService.NotaCredito(idFacturaEmitida, user.UserName);

                var vmFactura = _mapper.Map<VMFacturaEmitida>(credito);

                if (credito.Observaciones == null)
                {
                    vmFactura.QR = await _afipService.GenerateLinkAfipFactura(credito);
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

        [HttpPost]
        public async Task<IActionResult> Refacturar(int idFacturaEmitida, string cuil)
        {
            var gResponse = new GenericResponse<VMFacturaEmitida>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var factura = await _afipService.Refacturar(idFacturaEmitida, cuil, user.UserName);

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
                return HandleException(ex, "Error al recuperar facturas", _logger, idFacturaEmitida);
            }
        }

    }
}
