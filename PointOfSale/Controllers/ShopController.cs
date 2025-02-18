using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Collections.Generic;
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
        private readonly ITagService _tagService;

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
            ISaleService saleService,
            ITagService tagService)
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
            _tagService = tagService;
        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var ajuste = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

            if (ajuste.HabilitarWeb.HasValue && !ajuste.HabilitarWeb.Value)
            {
                return View("Componentes/ErrorWeb");
            }

            var shop = new VMShop(ajuste);
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.GetProductosDestacadosWeb());
            shop.IsLogin = claimuser.Identity.IsAuthenticated;

            return View("Index", shop);
        }

        public async Task<IActionResult> PoliticasPrivacidadCookies()
        {
            var ajuste = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

            if (ajuste.HabilitarWeb.HasValue && !ajuste.HabilitarWeb.Value)
            {
                return View("Componentes/ErrorWeb");
            }

            var shop = new VMShop(ajuste);

            return View("Componentes/politicas-de-privacidad-y-cookies", shop);
        }

        public async Task<IActionResult> TerminosCondiciones()
        {
            var ajuste = _mapper.Map<VMAjustesWeb>(await _ajusteService.GetAjustesWeb());

            if (ajuste.HabilitarWeb.HasValue && !ajuste.HabilitarWeb.Value)
            {
                return View("ErrorWeb");
            }

            var shop = new VMShop(ajuste);

            return View("Componentes/terminos-y-condiciones", shop);
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
                shop.Products = new List<VMProduct>();
                shop.FormasDePago = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
                shop.CategoriaWebs = _mapper.Map<List<VMCategoriaWeb>>(await _categoryService.List());
                shop.CategoriaWebs.AddRange(_mapper.Map<List<VMCategoriaWeb>>(await _tagService.List()));

                var destacados = await _productService.GetProductosDestacadosWeb();
                ViewData["HasDestacados"] = destacados.Any();

                return View("Lista", shop);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar productos paginados para la web.", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> GetMoreProducts([FromBody] Dictionary<int, decimal> productsQuantity, int page, int pageSize, int categoryId = 0, string searchText = "", int tagId = 0)
        {
            var gResponse = new GenericResponse<VMShop>();

            try
            {
                List<VMProduct> products;

                if (categoryId == -2 && tagId != -2)
                {
                    products = _mapper.Map<List<VMProduct>>(
                        await _tagService.ListProductsByTagWeb(tagId, page, pageSize, searchText)
                    );
                }
                else if (categoryId == -3 && tagId == -3)
                {
                    products = _mapper.Map<List<VMProduct>>(
                        await _productService.GetProductosDestacadosWeb()
                    );
                }
                else
                {
                    products = _mapper.Map<List<VMProduct>>(
                        await _productService.ListActiveByCategoryWeb(categoryId, page, pageSize, searchText)
                    );
                }

                if (productsQuantity.Any())
                {
                    products.ForEach(product =>
                    {
                        if (productsQuantity.TryGetValue(product.IdProduct, out var quantity))
                        {
                            product.Quantity = quantity;
                        }
                    });
                }


                var hasMoreProducts = products.Count == pageSize;

                var html = await RenderViewAsync("PVProducts", products);

                return Json(new { hasMoreProducts, html });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar más productos paginados para la web", _logger);
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
            var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByDescriptionWeb(text));
            return PartialView("PVProducts", products);
        }
        public IActionResult VentaWeb()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
        }

        [HttpGet]
        public async Task<IActionResult> GetVentasWeb()
        {
            try
            {
                var user = ValidarAutorizacion();
                var ventasWeb = await _shopService.GetAllByDate(user.IdRol == 1 ? null : TimeHelper.GetArgentinaTime()); // para empelados devuelve solo las del dia
                var vmCategoryList = _mapper.Map<List<VMVentaWeb>>(ventasWeb);
                return StatusCode(StatusCodes.Status200OK, new { data = vmCategoryList });
            }
            catch (Exception e)
            {
                return HandleException(e, "Error al recuperar lista de ventas web", _logger);
            }

        }


        [HttpPut]
        public async Task<IActionResult> UpdateVentaWeb([FromBody] VMVentaWeb model)
        {
            var gResponse = new GenericResponse<VMSaleWebResult>();

            try
            {
                var user = ValidarAutorizacion();
                var result = new VMSaleWebResult();

                model.ModificationUser = user.UserName;
                model.ModificationDate = TimeHelper.GetArgentinaTime();

                var ajustes = await _ajusteService.GetAjustes(model.IdTienda.HasValue ? model.IdTienda.Value : user.IdTienda);
                VentaWeb edited_VemntaWeb = await _shopService.Update(ajustes, _mapper.Map<VentaWeb>(model));

                result.VentaWeb = _mapper.Map<VMVentaWeb>(edited_VemntaWeb);
                result.SaleResult.NombreImpresora = ajustes.NombreImpresora;

                if (model.Estado == EstadoVentaWeb.Finalizada && model.IdTienda.HasValue)
                {
                    var sale = await _saleService.GetSale(edited_VemntaWeb.IdSale.Value);
                    result.SaleResult.IdSale = sale.IdSale;
                    result.SaleResult.SaleNumber = sale.SaleNumber;

                    if (ajustes.FacturaElectronica.HasValue && ajustes.FacturaElectronica.Value)
                    {
                        var facturas = await _afipService.GetFacturaByVentas(new List<Sale> { sale }, ajustes, model.CuilFactura, model.IdClienteFactura);
                        result.SaleResult.FacturasAFIP.AddRange(facturas);
                    }
                }

                gResponse.State = true;
                gResponse.Object = result;
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

    public class VMSaleWebResult
    {

        public VMSaleResult SaleResult { get; set; } = new VMSaleResult();

        public VMVentaWeb VentaWeb { get; set; }

    }
}
