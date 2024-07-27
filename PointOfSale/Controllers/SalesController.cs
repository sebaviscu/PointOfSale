using AFIP.Facturacion.Model.Factura;
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
using Neodynamic.SDK.Web;

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
        private readonly IAFIPFacturacionService _afipFacturacionService;

        public SalesController(
            ITypeDocumentSaleService typeDocumentSaleService,
            ISaleService saleService,
            IMapper mapper,
            IConverter converter,
            IClienteService clienteService,
            ITicketService ticketService,
            ITiendaService tiendaService,
            IShopService shopService,
            IAFIPFacturacionService afipFacturacionService,
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
            try
            {
                List<VMTypeDocumentSale> vmListTypeDocumentSale = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.GetActive());
                return StatusCode(StatusCodes.Status200OK, vmListTypeDocumentSale);
            }
            catch (Exception e)
            {

                throw;
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string search)
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
                List<VMProduct> vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception e)
            {

                throw;
            }
            return default;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes(string search)
        {
            try
            {
                List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClients(search));
                return StatusCode(StatusCodes.Status200OK, vmListClients);
            }
            catch (Exception e)
            {

                throw;
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

                ClaimsPrincipal claimuser = HttpContext.User;

                string idUsuario = claimuser.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
                string idTurno = claimuser.Claims.FirstOrDefault(c => c.Type == "Turno")?.Value;

                model.IdUsers = int.Parse(idUsuario);
                model.IdTurno = int.Parse(idTurno);
                model.IdTienda = user.IdTienda;

                Sale sale_created = await _saleService.Register(_mapper.Map<Sale>(model));

                if (model.ImprimirTicket)
                {
                    var tienda = await _tiendaService.Get(user.IdTienda);
                    ticket = _ticketService.TicketSale(sale_created, tienda);
                    nombreImpresora = tienda.NombreImpresora;
                }

                if (model.ClientId.HasValue)
                {
                    var mov = await _clienteService.RegistrarMovimiento(model.ClientId.Value, model.Total.Value, user.UserName, user.IdTienda, sale_created.IdSale, model.TipoMovimiento.Value);

                    sale_created.IdClienteMovimiento = mov.IdClienteMovimiento;
                    sale_created = await _saleService.Edit(sale_created);
                }

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

                        var tipoVentaMult = await _typeDocumentSaleService.Get(f.FormaDePago);

                        // Si es necesario facturar, se debe hacer aquí
                        // if ((int)tipoVentaMult.TipoFactura < 3)
                        // {
                        //     var factura = new FacturaAFIP();
                        //     var facturacionResponse = await _afipFacturacionService.FacturarAsync(factura);
                        // }
                    }
                }

                // Si es necesario facturar, se debe hacer aquí
                // var tipoVenta = await _typeDocumentSaleService.Get(sale_created.IdTypeDocumentSale.Value);
                // if ((int)tipoVenta.TipoFactura < 3)
                // {
                //     var factura = new FacturaAFIP();
                //     var facturacionResponse = await _afipFacturacionService.FacturarAsync(factura);
                // }

                model = _mapper.Map<VMSale>(sale_created);
                model.NombreImpresora = nombreImpresora;
                model.Ticket = ticket;

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
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
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
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
            catch (Exception e)
            {

                throw;
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
            catch (Exception e)
            {

                throw;
            }

        }

        public async Task<IActionResult> PrintTicketVentaWeb(int idVentaWeb)
        {
            try
            {
                var ventaWeb = await _shopService.Get(idVentaWeb);
                var tienda = await _tiendaService.Get(ventaWeb.IdTienda.Value);
                var ticket = _ticketService.TicketVentaWeb(ventaWeb, tienda);

                GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();

                var model = new VMSale();

                model.NombreImpresora = tienda.NombreImpresora;
                model.Ticket = ticket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception e)
            {

                throw;
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
