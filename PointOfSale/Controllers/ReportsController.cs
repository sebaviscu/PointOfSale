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

            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado });

            var prodsDictionary = await _productService.ProductsTopByCategory(idCategoria, startDate, endDate, user.IdTienda);
            var products = await _productService.GetProductsByIds(prodsDictionary.Select(_ => _.Key).ToList());
            var listVMSalesReport = new List<VMProductReport>();

            foreach (var p in prodsDictionary)
            {
                var vmSR = new VMProductReport();
                var product = products.First(_ => _.IdProduct == p.Key);
                var listaPrecio = product.ListaPrecios;

                vmSR.ProductName = product.Description;
                vmSR.Precio1 = $"$ {listaPrecio[0].Precio.ToString()}";
                vmSR.Precio2 = $"$ {listaPrecio[1].Precio.ToString()}";
                vmSR.Precio3 = $"$ {listaPrecio[2].Precio.ToString()}";
                vmSR.Cantidad = p.Value;
                vmSR.TipoVenta = product.TipoVenta.ToString();
                vmSR.Costo = "$ " + product.CostPrice.ToString();
                vmSR.Proveedor = product.Proveedor.Nombre;
                vmSR.Stock = product.Quantity.ToString();

                listVMSalesReport.Add(vmSR);
            }


            //List<VMSalesReport> vmList = _mapper.Map<List<VMSalesReport>>(await _saleService.Report(startDate, endDate));
            return StatusCode(StatusCodes.Status200OK, new { data = listVMSalesReport.OrderBy(_ => _.ProductName) });
        }
    }
}
