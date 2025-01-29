using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;
using PointOfSale.Model.Input;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class SalesController : BaseController
    {
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IClienteService _clienteService;
        private readonly ITicketService _ticketService;
        private readonly ILogger<SalesController> _logger;
        private readonly IAfipService _afipFacturacionService;
        private readonly IAjusteService _ajustesService;
        private readonly IEmailService _emailService;

        public SalesController(
            ITypeDocumentSaleService typeDocumentSaleService,
            ISaleService saleService,
            IMapper mapper,
            IClienteService clienteService,
            ITicketService ticketService,
            IAfipService afipFacturacionService,
            ILogger<SalesController> logger,
            IAjusteService ajustesService,
            IEmailService emailService)
        {
            _typeDocumentSaleService = typeDocumentSaleService;
            _saleService = saleService;
            _mapper = mapper;
            _clienteService = clienteService;
            _ticketService = ticketService;
            _afipFacturacionService = afipFacturacionService;
            _logger = logger;
            _ajustesService = ajustesService;
            _emailService = emailService;
        }

        public IActionResult NewSale()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
        }

        [HttpGet]
        public async Task<IActionResult> ListTypeDocumentSale()
        {
            var gResponse = new GenericResponse<List<VMTypeDocumentSale>>();
            try
            {
                List<VMTypeDocumentSale> vmListTypeDocumentSale = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.GetActive());
                gResponse.State = true;
                gResponse.Object = vmListTypeDocumentSale;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar formas de ventas", _logger);
            }

        }

        /// <summary>
        /// Devuelve productos para select2
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts(string search, int? listaPrecios)
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = listaPrecios == null
                    ? Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault())
                    : listaPrecios;

                var vmListProducts = _mapper.Map<List<VmProductsSelect2>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de productos", _logger, search);
            }
        }

        /// <summary>
        /// Devuelve productos para select2 para ver precios
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProductsVerPrecios(string search)
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
                var products = await _saleService.GetProductsSearchAndIdListaWithTags(search.Trim(), (ListaDePrecio)listaPrecioInt);
                var list = _mapper.Map<List<VMProduct>>(products.Select(_ => _.Producto).ToList());
                return StatusCode(StatusCodes.Status200OK, list);

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de productos", _logger, search);
            }
        }

        /// <summary>
        /// Devuelve clientes para select2
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetClientes(string search)
        {
            var gResponse = new GenericResponse<List<VMCliente>>();
            try
            {
                var user = ValidarAutorizacion();

                List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClients(search, user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, vmListClients);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de clientes", _logger, search);
            }

        }

        /// <summary>
        /// Devuelve clientes para select2
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetClientesByFacturar(string search)
        {
            var gResponse = new GenericResponse<List<VMCliente>>();
            try
            {
                List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClientsByFactura(search));
                vmListClients.ForEach(_ => _.CondicionIva = CondicionIva.RespInscripto);
                return StatusCode(StatusCodes.Status200OK, vmListClients);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de clientes para facturar", _logger, search);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMSale model)
        {

            var gResponse = new GenericResponse<VMSaleResult>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                var inouinput = new RegistrationSaleInput()
                {
                    MultiplesFormaDePago = _mapper.Map<List<MultiplesFormaPago>>(model.MultiplesFormaDePago),
                    ClientId = model.ClientId,
                    CuilFactura = model.CuilFactura,
                    IdClienteFactura = model.IdClienteFactura,
                    ImprimirTicket = model.ImprimirTicket,
                    TipoMovimiento = model.TipoMovimiento
                };

                model.IdTurno = user.IdTurno;
                model.IdTienda = user.IdTienda;
                model.RegistrationUser = user.UserName;

                var result = await _saleService.RegisterSale(_mapper.Map<Sale>(model), inouinput);

                var errores = $"{result.ErrorFacturacion} {result.Errores}";
                gResponse.State = string.IsNullOrEmpty(result.Errores);
                gResponse.Message = errores.Trim();
                gResponse.Object = _mapper.Map<VMSaleResult>(result);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al registrar la venta", _logger, model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> History(string saleNumber, string startDate, string endDate, string presupuestos)
        {
            var gResponse = new GenericResponse<List<VMSale>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                List<VMSale> vmHistorySale = _mapper.Map<List<VMSale>>(await _saleService.SaleHistory(saleNumber, startDate, endDate, presupuestos, user.IdTienda));

                if (vmHistorySale.Any(_ => _.IdClienteMovimiento != null))
                {
                    var movslist = vmHistorySale.Where(_ => _.IdClienteMovimiento != null).ToList();
                    var idMov = movslist.Select(_ => _.IdClienteMovimiento.Value).ToList();
                    var movs = await _clienteService.GetClienteByMovimientos(idMov, user.IdTienda);

                    foreach (var m in movslist)
                    {
                        var cliente = movs.FirstOrDefault(_ => _.IdClienteMovimiento == m.IdClienteMovimiento).Cliente;
                        m.ClientName = cliente.Nombre;
                    }
                }

                return StatusCode(StatusCodes.Status200OK, vmHistorySale);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar reporte de ventas", _logger, null,
                    ("SaleNumber", saleNumber), ("StartDate", startDate), ("EndDate", endDate), ("Presupuestos", presupuestos));
            }
        }

        [HttpGet]
        public async Task<IActionResult> HistoryTurnoActual()
        {
            var gResponse = new GenericResponse<List<VMSale>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                List<VMSale> vmHistorySale = _mapper.Map<List<VMSale>>(await _saleService.HistoryTurnoActual(user.IdTurno));

                if (vmHistorySale.Any(_ => _.IdClienteMovimiento != null))
                {
                    var movslist = vmHistorySale.Where(_ => _.IdClienteMovimiento != null).ToList();
                    var idMov = movslist.Select(_ => _.IdClienteMovimiento.Value).ToList();
                    var movs = await _clienteService.GetClienteByMovimientos(idMov, user.IdTienda);

                    foreach (var m in movslist)
                    {
                        var cliente = movs.FirstOrDefault(_ => _.IdClienteMovimiento == m.IdClienteMovimiento).Cliente;
                        m.ClientName = cliente.Nombre;
                    }
                }

                return StatusCode(StatusCodes.Status200OK, vmHistorySale);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar reporte de ventas por turno", _logger);
            }
        }

        public async Task<IActionResult> PrintTicket(int idSale)
        {
            return await ImprimirTickets(new List<int> { idSale });
        }

        public async Task<IActionResult> PrintMultiplesTickets([FromQuery] List<int> idSales)
        {
            return await ImprimirTickets(idSales);
        }

        private async Task<IActionResult> ImprimirTickets(List<int> idSales)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);
                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var model = new VMSale
                {
                    NombreImpresora = ajustes.NombreImpresora,
                    ImagesTicket = new List<Images>()
                };

                foreach (var item in idSales)
                {
                    var sale = await _saleService.GetSale(item);
                    var facturaEmitida = await _afipFacturacionService.GetBySaleId(item);
                    if (!string.IsNullOrEmpty(ajustes.NombreImpresora))
                    {
                        var ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);

                        model.Ticket += ticket.Ticket ?? string.Empty;
                        model.ImagesTicket.AddRange(ticket.ImagesTicket);
                        gResponse.State = true;
                    }
                    else
                    {
                        gResponse.State = false;
                        gResponse.Message = "No hay una Impresora cargada";
                    }

                }

                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir ticket(s)", _logger, idSales);
            }
        }


        public async Task<IActionResult> PdfTicket(int idSale)
        {
            return await GenerarPdfTicket(new List<int> { idSale });
        }

        public async Task<IActionResult> PdfMultiplesTickets([FromQuery] List<int> idSales)
        {
            return await GenerarPdfTicket(idSales);
        }

        private async Task<IActionResult> GenerarPdfTicket(List<int> idSales)
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);
                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var ticketCompleto = string.Empty;
                var imagesTicket = new List<Images>();

                foreach (var idSale in idSales)
                {
                    var sale = await _saleService.GetSale(idSale);
                    var facturaEmitida = await _afipFacturacionService.GetBySaleId(idSale);
                    var ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);

                    if (!string.IsNullOrEmpty(ticket.Ticket))
                    {
                        ticketCompleto += ticket.TicketSinFuentes();
                        imagesTicket.AddRange(ticket.ImagesTicket);
                    }
                }

                if (!string.IsNullOrEmpty(ticketCompleto))
                {
                    var pdfBytes = _ticketService.PdfTicket(ticketCompleto, imagesTicket);
                    return File(pdfBytes, "application/pdf", $"Ticket_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.pdf");
                }

                return StatusCode(StatusCodes.Status204NoContent, "No se generó ningún ticket.");
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al generar PDF del ticket", _logger, idSales);
            }
        }

        [HttpPost]
        public async Task<IActionResult> EnviarTicketEmail([FromBody] VMEmailTicketRequest request)
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                // Obtener los ajustes y el ticket
                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var sale = await _saleService.GetSale(request.IdSale);
                var facturaEmitida = await _afipFacturacionService.GetBySaleId(request.IdSale);
                var ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);

                if (!string.IsNullOrEmpty(ticket.Ticket))
                {
                    var pdfBytes = _ticketService.PdfTicket(ticket.TicketSinFuentes(), ticket.ImagesTicket);

                    // Enviar el ticket por email
                    await _emailService.EnviarTicketEmail(user.IdTienda, request.Email, pdfBytes);

                    return Ok(new { message = "Correo enviado con éxito." });
                }

                return BadRequest(new { message = "No se pudo generar el ticket." });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, $"Error al enviar el correo: {ex.Message}");
            }
        }


        public async Task<IActionResult> UpstatSale(int idSale, int formaPago)
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

            var sale = await _saleService.Edit(idSale, formaPago);

            return StatusCode(StatusCodes.Status200OK);
        }

        public async Task<IActionResult> AnularSale(int idSale)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                _ = await _saleService.AnularSale(idSale, user.UserName);

                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al querer borrar la venta", _logger, idSale);
            }
        }
    }
}
