using AFIP.Facturacion.Services;
using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;
using NuGet.Protocol;
using AFIP.Facturacion.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class SalesController : BaseController
    {
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IConverter _converter;
        private readonly IClienteService _clienteService;
        private readonly ITicketService _ticketService;
        private readonly ITiendaService _tiendaService;
        private readonly IShopService _shopService;
        private readonly ILogger<SalesController> _logger;
        private readonly IAfipService _afipFacturacionService;

        public SalesController(
            ITypeDocumentSaleService typeDocumentSaleService,
            ISaleService saleService,
            IMapper mapper,
            IConverter converter,
            IClienteService clienteService,
            ITicketService ticketService,
            ITiendaService tiendaService,
            IShopService shopService,
            IAfipService afipFacturacionService,
            ILogger<SalesController> logger)
        {
            _typeDocumentSaleService = typeDocumentSaleService;
            _saleService = saleService;
            _mapper = mapper;
            _converter = converter;
            _clienteService = clienteService;
            _ticketService = ticketService;
            _tiendaService = tiendaService;
            _shopService = shopService;
            _afipFacturacionService = afipFacturacionService;
            _logger = logger;
        }
        public IActionResult NewSale()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
            return ValidateSesionViewOrLogin();
        }

        public IActionResult SalesHistory()
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
            return ValidateSesionViewOrLogin();
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
                var errorMessage = "Error al recuperar formas de ventas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        /// <summary>
        /// Devuelve productos para select2
        /// </summary>
        /// <param name="search"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetProducts(string search)
        {
            var gResponse = new GenericResponse<List<VmProductsSelect2>>();
            try
            {
                //var pp = await _afipFacturacionService.GetPtosVentaAsync();
                //var mm = await _afipFacturacionService.GetTiposMonedasAsync();
                //var dd = await _afipFacturacionService.GetTiposDocAsync();
                //var aa = await _afipFacturacionService.GetUltimoComprobanteAutorizadoAsync(1, TipoComprobante.Factura_A);

                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
                var vmListProducts = _mapper.Map<List<VmProductsSelect2>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar lista de productos";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {Search}", errorMessage, search);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
            var gResponse = new GenericResponse<List<VMProduct>>();
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
                var vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar lista de productos";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {Search}", errorMessage, search);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClients(search));
                return StatusCode(StatusCodes.Status200OK, vmListClients);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar lista de clientes";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {Search}", errorMessage, search);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al recuperar lista de clientes para facturar";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {Search}", errorMessage, search);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMSale model)
        {

            var gResponse = new GenericResponse<VMSale>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
                var nombreImpresora = string.Empty;
                var ticket = string.Empty;

                RegistrationSetClaimProperties(model, user);

                Sale sale_created = await _saleService.Register(_mapper.Map<Sale>(model));

                await RegistrarionClient(model, user.UserName, user.IdTienda, sale_created);

                await RegistrationMultiplesPagos(model, sale_created);

                await RegistrationFacturar(model, user.UserName, sale_created);

                (nombreImpresora, ticket) = await RegistrationTicketPrinting(model, user.IdTienda, sale_created);

                var modelResponde = _mapper.Map<VMSale>(sale_created);
                modelResponde.NombreImpresora = nombreImpresora;
                modelResponde.Ticket = ticket;

                gResponse.State = true;
                gResponse.Object = modelResponde;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al registrar venta.";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {ModelRequest}", errorMessage, model);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        private async Task<(string nombreImpresora, string ticket)> RegistrationTicketPrinting(VMSale model, int idTienda, Sale saleCreated)
        {
            if (!model.ImprimirTicket)
            {
                return (string.Empty, string.Empty);
            }

            var tienda = await _tiendaService.Get(idTienda);
            var ticket = _ticketService.TicketSale(saleCreated, tienda);
            return (tienda.NombreImpresora, ticket);
        }

        private void RegistrationSetClaimProperties(VMSale model, (bool Resultado, string UserName, int IdTienda, ListaDePrecio IdListaPrecios) user)
        {
            ClaimsPrincipal claimuser = HttpContext.User;

            string idUsuario = claimuser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            string idTurno = claimuser.Claims.FirstOrDefault(c => c.Type == "Turno")?.Value;

            model.IdUsers = int.Parse(idUsuario);
            model.IdTurno = int.Parse(idTurno);
            model.IdTienda = user.IdTienda;
        }

        private async Task RegistrationFacturar(VMSale model, string registrationUser, Sale sale_created)
        {
            var tipoVenta = await _typeDocumentSaleService.Get(sale_created.IdTypeDocumentSale.Value);

            if ((int)tipoVenta.TipoFactura < 3)
            {
                sale_created.TypeDocumentSaleNavigation = tipoVenta;

                var facturaEmitidaResponse = await _afipFacturacionService.Facturar(sale_created, model.IdCuilFactura, model.IdClienteFactura, registrationUser);
                sale_created.IdFacturaEmitida = facturaEmitidaResponse.IdFacturaEmitida;
                _ = await _saleService.Edit(sale_created);
            }
        }

        private async Task RegistrationMultiplesPagos(VMSale model, Sale sale_created)
        {
            if (model.MultiplesFormaDePago != null && model.MultiplesFormaDePago.Count > 1)
            {
                foreach (var f in model.MultiplesFormaDePago.Skip(1))
                {
                    var s = new Sale
                    {
                        Total = f.Total,
                        RegistrationDate = sale_created.RegistrationDate,
                        IdTienda = sale_created.IdTienda,
                        SaleNumber = sale_created.SaleNumber,
                        IdTypeDocumentSale = f.FormaDePago,
                        IdTurno = sale_created.IdTurno
                    };

                    await _saleService.Register(s);
                }
            }
        }

        private async Task RegistrarionClient(VMSale model, string registrationUser, int IdTienda, Sale sale_created)
        {
            if (model.ClientId.HasValue)
            {
                var mov = await _clienteService.RegistrarMovimiento(model.ClientId.Value, model.Total.Value, registrationUser, IdTienda, sale_created.IdSale, model.TipoMovimiento.Value);

                sale_created.IdClienteMovimiento = mov.IdClienteMovimiento;
                _ = await _saleService.Edit(sale_created);
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterNoCierreSale([FromBody] VMSale model)
        {

            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                ClaimsPrincipal claimuser = HttpContext.User;

                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                string idTurno = claimuser.Claims.Where(c => c.Type == "Turno").Select(c => c.Value).SingleOrDefault();

                model.IdUsers = int.Parse(idUsuario);
                model.IdTurno = int.Parse(idTurno);
                model.IdTienda = user.IdTienda;


                //model = _mapper.Map<VMSale>(sale_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al registrar No Cierre de venta";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {ModelRequest}", errorMessage, model);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpGet]
        public IActionResult ReportSale(int saleNumber)
        {
            ViewData["saleNumber"] = saleNumber;

            return View("SalesHistory");
        }

        [HttpGet]
        public async Task<IActionResult> History(string saleNumber, string startDate, string endDate, string presupuestos)
        {
            var gResponse = new GenericResponse<List<VMSale>>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                List<VMSale> vmHistorySale = _mapper.Map<List<VMSale>>(await _saleService.SaleHistory(saleNumber, startDate, endDate, presupuestos));

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
                var errorMessage = "Error al recuperar reporte de ventas";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {ModelRequest} | SaleNumber: {SaleNumber} | StartDate: {StartDate} | EndDate: {EndDate} | Presupuestos: {Presupuestos}",
                        errorMessage, saleNumber.ToJson(), startDate.ToJson(), endDate.ToJson(), presupuestos.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        public async Task<IActionResult> PrintTicket(int idSale)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();

            try
            {
                var sale = await _saleService.GetSale(idSale);
                var tienda = await _tiendaService.Get(sale.IdTienda);
                var ticket = _ticketService.TicketSale(sale, tienda);

                var model = new VMSale();


                model.NombreImpresora = tienda.NombreImpresora;
                model.Ticket = ticket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al imprimir ticket";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {ModelRequest}", errorMessage, idSale.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public async Task<IActionResult> PrintTicketVentaWeb(int idVentaWeb)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
                var ventaWeb = await _shopService.Get(idVentaWeb);
                var tienda = await _tiendaService.Get(ventaWeb.IdTienda.Value);
                var ticket = _ticketService.TicketVentaWeb(ventaWeb, tienda);


                var model = new VMSale();

                model.NombreImpresora = tienda.NombreImpresora;
                model.Ticket = ticket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al imprimir ticket de venta web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage} Request: {ModelRequest}", errorMessage, idVentaWeb);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        public async Task<IActionResult> ShowPDFSaleAsync(string saleNumber)
        {
            string urlTemplateView = $"{this.Request.Scheme}://{this.Request.Host}/Template/PDFSale?saleNumber={saleNumber}";

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings()
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait
                },
                Objects = {
                    new ObjectSettings(){
                        Page = urlTemplateView
                    }
                }
            };
            var archivoPDF = _converter.Convert(pdf);
            return File(archivoPDF, "application/pdf");
        }

        public async Task<IActionResult> UpstatSale(int idSale, int formaPago)
        {
            ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

            var sale = await _saleService.Edit(idSale, formaPago);

            return StatusCode(StatusCodes.Status200OK);
        }

    }
}
