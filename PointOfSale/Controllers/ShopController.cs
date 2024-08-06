using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol;
using Org.BouncyCastle.Pkcs;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IShopService _shopService;
        private readonly ICategoryService _categoryService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly IAjusteService _ajusteService;
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ILogger<ShopController> _logger;
        public ShopController(IProductService productService, IMapper mapper, IShopService shopService, ICategoryService categoryService, ITypeDocumentSaleService typeDocumentSaleService, IAjusteService ajusteService, IRazorViewEngine razorViewEngine, ILogger<ShopController> logger)
        {
            _productService = productService;
            _mapper = mapper;
            _shopService = shopService;
            _categoryService = categoryService;
            _typeDocumentSaleService = typeDocumentSaleService;
            _ajusteService = ajusteService;

            _razorViewEngine = razorViewEngine;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var ajuste = _mapper.Map<VMAjustes>(await _ajusteService.GetAjustesWeb());
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

                var ajuste = _mapper.Map<VMAjustes>(await _ajusteService.GetAjustesWeb());
                var shop = new VMShop(ajuste);
                shop.IsLogin = claimuser.Identity.IsAuthenticated;
                shop.Products = new List<VMProduct>();//_mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(0, page, pageSize));
                shop.FormasDePago = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
                shop.Categorias = _mapper.Map<List<VMCategory>>(await _categoryService.ListActive());
                return View("Lista", shop);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.ToString();
                _logger.LogError(ex, "Error al recuperar productos paginados para la web");
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                var errorMessage = "Error al recuperar mas productos paginados para la web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                ClaimsPrincipal claimuser = HttpContext.User;
                var userName = claimuser.Claims
                        .Where(c => c.Type == ClaimTypes.Name)
                        .Select(c => c.Value).SingleOrDefault();

                model.ModificationUser = userName;

                VentaWeb edited_VemntaWeb = await _shopService.Update(_mapper.Map<VentaWeb>(model));

                model = _mapper.Map<VMVentaWeb>(edited_VemntaWeb);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar una venta web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
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
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al registrar una venta web";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

    }
}
