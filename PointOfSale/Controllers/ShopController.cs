using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;
        private readonly IShopService _shopService;
        private readonly ITiendaService _tiendaService;
        public ShopController(IProductService productService, IMapper mapper, IShopService shopService, ITiendaService tiendaService)
        {
            _productService = productService;
            _mapper = mapper;
            _shopService = shopService;
            _tiendaService = tiendaService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            ClaimsPrincipal claimuser = HttpContext.User;

            var shop = new VMShop();
            shop.Tienda = _mapper.Map<VMTienda>(await _tiendaService.GetTiendaPrincipal());
            shop.Products = _mapper.Map<List<VMProduct>>(await _productService.GetRandomProducts());
            shop.IsLogin = claimuser.Identity.IsAuthenticated;

            return View("Index", shop);
        }

        public async Task<IActionResult> Lista()
        {
            var vmProductList = _mapper.Map<List<VMProduct>>(await _productService.List());
            return View("Lista", vmProductList);
        }
    }
}
