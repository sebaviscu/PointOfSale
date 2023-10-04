using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
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
        private readonly ISaleService _saleService;
        private readonly ICategoryService _categoryService;

        public ShopController(IProductService productService, IMapper mapper, IShopService shopService, ITiendaService tiendaService, ISaleService saleService, ICategoryService categoryService)
        {
            _productService = productService;
            _mapper = mapper;
            _shopService = shopService;
            _tiendaService = tiendaService;
            _saleService = saleService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            ClaimsPrincipal claimuser = HttpContext.User;

            var tienda = _mapper.Map<VMTienda>(await _tiendaService.GetTiendaPrincipal());
            var shop = new VMShop(tienda);
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.GetRandomProducts());
            shop.IsLogin = claimuser.Identity.IsAuthenticated;

            return View("Index", shop);
        }

        public async Task<IActionResult> Lista()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            var tienda = _mapper.Map<VMTienda>(await _tiendaService.GetTiendaPrincipal());

            var shop = new VMShop(tienda);
            shop.IsLogin = claimuser.Identity.IsAuthenticated;
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(0));
            var list = _mapper.Map<List<VMCategory>>(await _categoryService.ListActive());
            shop.Categorias.AddRange(list);
            return View("Lista", shop);
        }

        [HttpGet]
        public async Task<ActionResult> GetProductsByCategory(int idCategoria)
        {
            var products = _mapper.Map<List<VMProduct>>(await _productService.ListActiveByCategory(idCategoria));
            return PartialView("PVProducts", products);
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
            //var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            GenericResponse<VMVentaWeb> gResponse = new GenericResponse<VMVentaWeb>();
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;

                string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();
                var idTienda = Convert.ToInt32(claimuser.Claims.Where(c => c.Type == "Tienda").Select(c => c.Value).SingleOrDefault());

                model.IdUsers = int.Parse(idUsuario);
                model.IdTienda = idTienda;

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
