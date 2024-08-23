using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class ShopController : BaseController
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IShopService _shopService;
        private readonly ICategoryService _categoryService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly IAjusteService _ajusteService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ILogger<ShopController> _logger;
        private readonly ITicketService _ticketService;
        private readonly IAfipService _afipService;
        private readonly ISaleService _saleService;

        public ShopController(IProductService productService,
            IMapper mapper,
            IShopService shopService,
            ICategoryService categoryService,
            ITypeDocumentSaleService typeDocumentSaleService,
            IAjusteService ajusteService,
            IRazorViewEngine razorViewEngine,
            ILogger<ShopController> logger,
            ITicketService ticketService,
            IAfipService afipService,
            ISaleService saleService)
        {
            _productService = productService;
            _mapper = mapper;
            _shopService = shopService;
            _categoryService = categoryService;
            _typeDocumentSaleService = typeDocumentSaleService;
            _ajusteService = ajusteService;
            _ticketService = ticketService;
            _afipService = afipService;
            _razorViewEngine = razorViewEngine;
            _logger = logger;
            _saleService = saleService;
        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var ajuste = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

            if(ajuste.HabilitarWeb.HasValue && !ajuste.HabilitarWeb.Value)
            {
                return View("ErrorWeb");
            }

            var shop = new VMShop(ajuste);
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.GetRandomProducts());
            shop.IsLogin = claimuser.Identity.IsAuthenticated;

            return View("Index", shop);
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = 6)
        {
            var gResponse = new GenericResponse<VMShop>();

            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;

                var ajuste = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

                if (ajuste.HabilitarWeb.HasValue && !ajuste.HabilitarWeb.Value)
                {
                    return View("ErrorWeb");
                }

                var shop = new VMShop(ajuste);
                shop.IsLogin = claimuser.Identity.IsAuthenticated;
                shop.Products = new List<VMProduct>();//_mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(0, page, pageSize));
                shop.FormasDePago = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
                shop.Categorias = _mapper.Map<List<VMCategory>>(await _categoryService.ListActive());
                return View("Lista", shop);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar productos paginados para la web.", _logger, null);
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetMoreProducts(int page, int pageSize, int categoryId = 0, string searchText = "")
        {
            var gResponse = new GenericResponse<VMShop>();
            try
            {
                var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(categoryId, page, pageSize, searchText));
                var hasMoreProducts = products.Count == pageSize;

                var html = await RenderViewAsync("PVProducts", products);

                return Json(new { hasMoreProducts, html });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar mas productos paginados para la web", _logger, null);
            }

        }

        private async Task<string> RenderViewAsync(string viewName, object model)
        {
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = _razorViewEngine.FindView(ControllerContext, viewName, false);

                if (viewResult.View == null)
                {
                    throw new ArgumentNullException($"View {viewName} was not found.");
                }

                var viewContext = new ViewContext(
                    ControllerContext,
                    viewResult.View,
                    ViewData,
                    TempData,
                    sw,
                    new HtmlHelperOptions()
                );

                await viewResult.View.RenderAsync(viewContext);
                return sw.GetStringBuilder().ToString();
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetProductsByDescription(string text)
        {
            var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByDescription(text));
            return PartialView("PVProducts", products);
        }
        public async Task<IActionResult> VentaWeb()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetVentasWeb()
        {
            List<VMVentaWeb> vmCategoryList = _mapper.Map<List<VMVentaWeb>>(await _shopService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = vmCategoryList });
        }


        [HttpPut]
        public async Task<IActionResult> UpdateVentaWeb([FromBody] VMVentaWeb model)
        {
            GenericResponse<VMVentaWeb> gResponse = new GenericResponse<VMVentaWeb>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                model.ModificationUser = user.UserName;

                VentaWeb edited_VemntaWeb = await _shopService.Update(_mapper.Map<VentaWeb>(model));
                model = _mapper.Map<VMVentaWeb>(edited_VemntaWeb);
                if (model.Estado == EstadoVentaWeb.Finalizada && model.IdTienda.HasValue)
                {
                    var ajustes = await _ajusteService.GetAjustes(model.IdTienda.Value);
                    var sale = await _saleService.GetSale(edited_VemntaWeb.IdSale.Value);

                    var facturaEmitida = await _afipService.FacturarVenta(sale, ajustes, model.CuilFactura, model.IdClienteFactura);

                    if (model.ImprimirTicket)
                    {
                        var ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);
                        model.Ticket = ticket.Ticket;
                        model.ImagesTicket.AddRange(ticket.ImagesTicket);
                        model.NombreImpresora = model.ImprimirTicket && !string.IsNullOrEmpty(ajustes.NombreImpresora) ? ajustes.NombreImpresora : null;
                    }
                }

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar una venta web.", _logger, model);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        public async Task<IActionResult> PrintTicketVentaWeb(int idVentaWeb)
        {
            GenericResponse<VMSaleResult> gResponse = new GenericResponse<VMSaleResult>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var ajustes = await _ajusteService.GetAjustes(user.IdTienda);

                if (string.IsNullOrEmpty(ajustes.NombreImpresora))
                {
                    return HandleException(null, "No existe impresora registrada.", _logger, idVentaWeb);
                }

                var ventaWeb = await _shopService.Get(idVentaWeb);
                if (!ventaWeb.IdTienda.HasValue)
                    ventaWeb.IdTienda = user.IdTienda;

                TicketModel ticket;
                if (ventaWeb.IdSale.HasValue)
                {
                    var sale = await _saleService.GetSale(ventaWeb.IdSale.Value);
                    var facturaEmitida = await _afipService.GetBySaleId(ventaWeb.IdSale.Value);
                    ticket = await _ticketService.TicketSale(sale, ajustes, facturaEmitida);
                }
                else
                {
                    ticket = await _ticketService.TicketSale(ventaWeb, ajustes);
                }


                var model = new VMSaleResult();

                model.NombreImpresora = ajustes.NombreImpresora;
                model.Ticket = ticket.Ticket ?? string.Empty;
                model.ImagesTicket = ticket.ImagesTicket;

                gResponse.State = true;
                gResponse.Object = model;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al imprimir ticket de venta web.", _logger, idVentaWeb);
            }

        }

        [HttpPost]
        public async Task<IActionResult> RegisterSale([FromBody] VMVentaWeb model)
        {
            GenericResponse<VMVentaWeb> gResponse = new GenericResponse<VMVentaWeb>();
            try
            {
                var sale_created = await _shopService.RegisterWeb(_mapper.Map<VentaWeb>(model));

                model = _mapper.Map<VMVentaWeb>(sale_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al registrar una venta web.", _logger, model);
            }

        }

    }
}
