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
            IAFIPFacturacionService afipFacturacionService)
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
        }
        public IActionResult NewSale()
        {
            return View();
        }

        public IActionResult SalesHistory()
        {
            return View();
        }



        [HttpGet]
        public async Task<IActionResult> ListTypeDocumentSale()
        {
            List<VMTypeDocumentSale> vmListTypeDocumentSale = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.GetActive());
            return StatusCode(StatusCodes.Status200OK, vmListTypeDocumentSale);
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(string search)
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var listaPrecioInt = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "ListaPrecios").Select(c => c.Value).SingleOrDefault());
            try
            {
                List<VMProduct> vmListProducts = _mapper.Map<List<VMProduct>>(await _saleService.GetProductsSearchAndIdLista(search.Trim(), (ListaDePrecio)listaPrecioInt));
                return StatusCode(StatusCodes.Status200OK, vmListProducts);

            }
            catch (Exception)
            {

                throw;
            }
            return default;
        }

        [HttpGet]
        public async Task<IActionResult> GetClientes(string search)
        {
            List<VMCliente> vmListClients = _mapper.Map<List<VMCliente>>(await _saleService.GetClients(search));
            return StatusCode(StatusCodes.Status200OK, vmListClients);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMSale model)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });
            var nombreImpresora = string.Empty;
            var ticket = string.Empty;

            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;

                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                string idTurno = claimuser.Claims.Where(c => c.Type == "Turno").Select(c => c.Value).SingleOrDefault();

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
                    var mov = await _clienteService.RegistrarMovimiento(model.ClientId.Value, decimal.Parse(model.Total.Replace('.', ',')), user.UserName, user.IdTienda, sale_created.IdSale, model.TipoMovimiento.Value);

                    sale_created.IdClienteMovimiento = mov.IdClienteMovimiento;
                    sale_created = await _saleService.Edit(sale_created);
                }

                if (model.MultiplesFormaDePago != null && model.MultiplesFormaDePago.Count > 1)
                {
                    int count = 0;

                    foreach (var f in model.MultiplesFormaDePago)
                    {
                        if (count == 0)
                        {
                            count++;
                            continue;
                        }

                        var s = new Sale();
                        s.Total = f.Total;
                        s.RegistrationDate = sale_created.RegistrationDate;
                        s.IdTienda = sale_created.IdTienda;
                        s.SaleNumber = sale_created.SaleNumber;
                        s.IdTypeDocumentSale = f.FormaDePago;
                        s.IdTurno = sale_created.IdTurno;

                        await _saleService.Register(s);

                        var tipoVentaMult = await _typeDocumentSaleService.Get(f.FormaDePago);

                        //if ((int)tipoVentaMult.TipoFactura < 3)
                        //{
                        //    var factura = new FacturaAFIP();
                        //    var facturacionResponse = await _afipFacturacionService.FacturarAsync(factura);
                        //}
                    }
                }

                //var tipoVenta = await _typeDocumentSaleService.Get(sale_created.IdTypeDocumentSale.Value);
                //if ((int)tipoVenta.TipoFactura < 3)
                //{
                //    var factura = new FacturaAFIP();
                //    var facturacionResponse = await _afipFacturacionService.FacturarAsync(factura);
                //}

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
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            GenericResponse<VMSale> gResponse = new GenericResponse<VMSale>();
            try
            {
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
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

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

        public async Task<IActionResult> PrintTicket(int idSale)
        {
            var sale = await _saleService.GetSale(idSale);
            var tienda = await _tiendaService.Get(sale.IdTienda);
            _ticketService.TicketSale(sale, tienda);

            return StatusCode(StatusCodes.Status200OK);
        }

        public async Task<IActionResult> PrintTicketVentaWeb(int idVentaWeb)
        {
            var ventaWeb = await _shopService.Get(idVentaWeb);
            var tienda = await _tiendaService.Get(ventaWeb.IdTienda.Value);
            _ticketService.TicketVentaWeb(ventaWeb, tienda);

            return StatusCode(StatusCodes.Status200OK);
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
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var sale = await _saleService.Edit(idSale, formaPago);

            return StatusCode(StatusCodes.Status200OK);
        }

    }
}
