using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
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
        private readonly ITiendaService _tiendaService;
        private readonly ICategoryService _categoryService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly IAjusteService _ajusteService;

        private readonly IRazorViewEngine _razorViewEngine;
        private readonly IServiceProvider _serviceProvider;
        private readonly ITempDataProvider _tempDataProvider;
        public ShopController(IProductService productService, IMapper mapper, IShopService shopService, ITiendaService tiendaService, ICategoryService categoryService, ITypeDocumentSaleService typeDocumentSaleService, IAjusteService ajusteService, IRazorViewEngine razorViewEngine, IServiceProvider serviceProvider, ITempDataProvider tempDataProvider)
        {
            _productService = productService;
            _mapper = mapper;
            _shopService = shopService;
            _tiendaService = tiendaService;
            _categoryService = categoryService;
            _typeDocumentSaleService = typeDocumentSaleService;
            _ajusteService = ajusteService;

            _razorViewEngine = razorViewEngine;
            _serviceProvider = serviceProvider;
            _tempDataProvider = tempDataProvider;

        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var ajuste = _mapper.Map<VMAjustes>( await _ajusteService.Get());
            var shop = new VMShop(ajuste);
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.GetRandomProducts());
            shop.IsLogin = claimuser.Identity.IsAuthenticated;

            return View("Index", shop);
        }

        [HttpGet]
        public async Task<IActionResult> Lista(int page = 1, int pageSize = 6)
        {
            ClaimsPrincipal claimuser = HttpContext.User;

            var ajuste = _mapper.Map<VMAjustes>(await _ajusteService.Get());
            var shop = new VMShop(ajuste);
            shop.IsLogin = claimuser.Identity.IsAuthenticated;
            shop.Products = new List<VMProduct>();//_mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(0, page, pageSize));
            shop.FormasDePago = _mapper.Map<List<VMTypeDocumentSale>>(await _typeDocumentSaleService.ListWeb());
            shop.Categorias = _mapper.Map<List<VMCategory>>(await _categoryService.ListActive());
            return View("Lista", shop);
        }

        [HttpGet]
        public async Task<IActionResult> GetMoreProducts(int page, int pageSize, int categoryId = 0, string searchText = "")
        {
            var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(categoryId, page, pageSize, searchText));
            var hasMoreProducts = products.Count == pageSize;

            var html = await RenderViewAsync("PVProducts", products);

            return Json(new { hasMoreProducts, html });
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


        //[HttpGet]
        //public async Task<ActionResult> GetProductsByCategory(int idCategoria)
        //{
        //    var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(idCategoria));
        //    return PartialView("PVProducts", products);
        //}

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
            ClaimsPrincipal claimuser = HttpContext.User;
            var userName = claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.Name)
                    .Select(c => c.Value).SingleOrDefault();

            GenericResponse<VMVentaWeb> gResponse = new GenericResponse<VMVentaWeb>();
            try
            {
                model.ModificationUser = userName;

                VentaWeb edited_Gastos = await _shopService.Edit(_mapper.Map<VentaWeb>(model));

                model = _mapper.Map<VMVentaWeb>(edited_Gastos);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
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
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

    }
}
