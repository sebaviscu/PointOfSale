using PointOfSale.Business.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class ShopService : IShopService
    {
        private readonly ITiendaService _tiendaService;
        private readonly IProductService _productService;

        public ShopService(ITiendaService tiendaService, IProductService productService)
        {
            _tiendaService = tiendaService;
            _productService = productService;
        }
    }
}
