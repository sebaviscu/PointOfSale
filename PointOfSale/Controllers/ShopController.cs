using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Models;

namespace PointOfSale.Controllers
{
    public class ShopController : Controller
    {
        private readonly IProductService _productService;
        private readonly IMapper _mapper;

        public ShopController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> Lista()
        {
            var vmProductList = _mapper.Map<List<VMProduct>>(await _productService.List());
            return View("Lista", vmProductList);
        }
    }
}
