using AutoMapper;
using DinkToPdf;
using DinkToPdf.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

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
        private readonly IAjusteService _ajustesService;

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
            ILogger<SalesController> logger,
            IAjusteService ajustesService)
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
            _ajustesService = ajustesService;
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
                return HandleException(ex, "Error al recuperar formas de ventas", _logger);
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
                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
                var vmListProducts = _mapper.Map<List<VmProductsSelect2>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de productos", _logger, search.ToJson());
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
                return HandleException(ex, "Error al recuperar lista de productos", _logger, search.ToJson());
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
                return HandleException(ex, "Error al recuperar lista de clientes", _logger, search.ToJson());
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
                return HandleException(ex, "Error al recuperar lista de clientes para facturar", _logger, search.ToJson());
            }
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMSale model)
         {

            var gResponse = new GenericResponse<VMSaleResult>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                model.IdUsers = user.IdUsuario;
                model.IdTurno = user.IdTurno;
                model.IdTienda = user.IdTienda;
                model.RegistrationUser = user.UserName;

                var ajustesTask = _ajustesService.GetAjustes(user.IdTienda);
                var lastNumberTask = _saleService.GetLastSerialNumberSale();

                await Task.WhenAll(ajustesTask, lastNumberTask);

                var ajustes = await ajustesTask;
                var lastNumber = await lastNumberTask;

                var paso = false;

                var modelResponde = new VMSaleResult()
                {
                    SaleNumber = lastNumber,
                    NombreImpresora = model.ImprimirTicket && !string.IsNullOrEmpty(ajustes.NombreImpresora) ? ajustes.NombreImpresora : null
                };

                foreach (var m in model.MultiplesFormaDePago)
                {
                    var newVMSale = new VMSale
                    {
                        Total = m.Total,
                        IdTypeDocumentSale = m.FormaDePago,
                        IdUsers = user.IdUsuario,
                        IdTurno = user.IdTurno,
                        IdTienda = user.IdTienda,
                        RegistrationUser = user.UserName,
                        SaleNumber = lastNumber
                    };

                    if (!paso)
                    {
                        newVMSale.DetailSales = model.DetailSales;
                        paso = true;
                    }

                    Sale sale_created = await _saleService.Register(_mapper.Map<Sale>(newVMSale), ajustes);

                    await RegistrarionClient(newVMSale, user.UserName, user.IdTienda, sale_created);

                    var facturaEmitida = await RegistrationFacturar(model, sale_created, ajustes);

                    // si es multiple, no se podra reimprimir el ticket ya que se necesita el idSale y en este caso, hay muchos, ver que podemos hacer
                    //var modelResponde = _mapper.Map<VMSale>(sale_created);
                    modelResponde.IdSale = sale_created.IdSale;
                    if (model.ImprimirTicket && !string.IsNullOrEmpty(ajustes.NombreImpresora))
                    {
                        var ticket = await RegistrationTicketPrinting(ajustes, sale_created, facturaEmitida);
                        modelResponde.Ticket += ticket.Ticket;
                        modelResponde.ImagesTicket.AddRange(ticket.ImagesTicket);
                    }
                }

                gResponse.State = true;
                gResponse.Object = modelResponde;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al registrar la venta", _logger, model.ToJson());
            }
        }

        private async Task<TicketModel> RegistrationTicketPrinting(Ajustes ajustes, Sale saleCreated, FacturaEmitida? facturaEmitida)
        {
            var ticket = await _ticketService.TicketSale(saleCreated, ajustes, facturaEmitida);
            return ticket;
        }

        private async Task<FacturaEmitida?> RegistrationFacturar(VMSale model, Sale sale_created, Ajustes ajustes)
        {
            if (!ajustes.FacturaElectronica.HasValue || (ajustes.FacturaElectronica.HasValue && !ajustes.FacturaElectronica.Value))
            {
                return null;
            }

            var tipoVenta = await _typeDocumentSaleService.Get(sale_created.IdTypeDocumentSale.Value);
            FacturaEmitida facturaEmitida = null;

            if ((int)tipoVenta.TipoFactura < 3)
            {
                sale_created.TypeDocumentSaleNavigation = tipoVenta;

                facturaEmitida = await _afipFacturacionService.Facturar(sale_created, model.CuilFactura, model.IdClienteFactura, sale_created.RegistrationUser);
                sale_created.IdFacturaEmitida = facturaEmitida.IdFacturaEmitida;
                _ = await _saleService.Edit(sale_created);
            }

            return facturaEmitida;
        }

        private async Task RegistrationMultiplesPagos(VMSale model, Sale sale_created, Ajustes ajustes)
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

                    await _saleService.Register(s, ajustes);
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
                return HandleException(ex, "Error al registrar No Cierre de venta", _logger, model.ToJson());
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
                return HandleException(ex, "Error al recuperar reporte de ventas", _logger, null,
                    ("SaleNumber", saleNumber.ToJson()), ("StartDate", startDate.ToJson()), ("EndDate", endDate.ToJson()), ("Presupuestos", presupuestos.ToJson()));
            }
        }

        public async Task<IActionResult> PrintTicket(int idSale)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();

            try
            {
                if(idSale == 0)
                {
                    gResponse.State = false;
                    gResponse.Message = "Es una venta con multiples formas de pago, no se pued eimprimir ticket.";
                    StatusCode(StatusCodes.Status500InternalServerError, gResponse);
                }
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var sale = await _saleService.GetSale(idSale);
                var facturaEmitida = await _afipFacturacionService.GetBySaleId(idSale);
                var ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);

                var model = new VMSale();


                model.NombreImpresora = ajustes.NombreImpresora;
                model.Ticket = ticket.Ticket ?? string.Empty;
                model.ImagesTicket = ticket.ImagesTicket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir ticket", _logger, idSale.ToJson());
            }

        }

        public async Task<IActionResult> PrintTicketVentaWeb(int idVentaWeb)
        {
            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var ajustes = await _ajustesService.GetAjustes(user.IdTienda);
                var ventaWeb = await _shopService.Get(idVentaWeb);
                //var facturaEmitida = await _afipFacturacionService.GetBySaleId(idSale);
                var ticket = await _ticketService.TicketVentaWeb(ventaWeb, ajustes, null);


                var model = new VMSale();

                model.NombreImpresora = ajustes.NombreImpresora;
                model.Ticket = ticket.Ticket ?? string.Empty;
                model.ImagesTicket = ticket.ImagesTicket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir ticket de venta web", _logger, idVentaWeb.ToJson());
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
