using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Models;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class ReportsController : BaseController
    {
        private readonly ISaleService _saleService;
        private readonly IMapper _mapper;
        private readonly IProductService _productService;

        public ReportsController(ISaleService saleService, IMapper mapper, IProductService productService)
        {
            _saleService = saleService;
            _mapper = mapper;
            _productService = productService;
        }
        public IActionResult ProductsReport()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProductsReport(string idCategoria, string startDate, string endDate)
        {

            if (idCategoria == null && startDate == null && endDate == null)
                return View();

            ClaimsPrincipal claimuser = HttpContext.User;

            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var prodsDictionary = await _productService.ProductsTopByCategory(idCategoria, startDate, endDate, user.IdTienda);
            var products = await _productService.GetProductsByIds(prodsDictionary.Select(_ => _.Key).ToList(), user.IdListaPrecios);
            var listVMSalesReport = new List<VMProductReport>();

            foreach (var p in prodsDictionary)
            {
                var vmSR = new VMProductReport();
                var product = products.First(_ => _.IdProduct == p.Key);

                var totalPrecioPorCantidad = Convert.ToDecimal(p.Value) * product.Price;
                var costoTotal = Convert.ToDecimal(p.Value) * product.CostPrice;

                vmSR.ProductName = product.Description;
                vmSR.Precio = $"$ {product.Price.ToString()} / {product.TipoVenta.ToString()}";
                vmSR.Cantidad = p.Value;
                vmSR.TotalPrecioPorCantidad = "$ " + (totalPrecioPorCantidad).ToString();
                vmSR.Costo = "$ " + product.CostPrice.ToString();
                vmSR.TotalProfit = "$ " + (totalPrecioPorCantidad - costoTotal).ToString();
                vmSR.Proveedor = product.Proveedor.Nombre;

                listVMSalesReport.Add(vmSR);
            }


            //List<VMSalesReport> vmList = _mapper.Map<List<VMSalesReport>>(await _saleService.Report(startDate, endDate));
            return StatusCode(StatusCodes.Status200OK, new { data = listVMSalesReport });
        }
    }
}
