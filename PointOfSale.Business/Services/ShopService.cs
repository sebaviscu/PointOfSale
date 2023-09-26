using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class ShopService : IShopService
    {
        private readonly IGenericRepository<VentaWeb> _repository;
        private readonly ITiendaService _tiendaService;
        private readonly IProductService _productService;

        public ShopService(ITiendaService tiendaService, IProductService productService, IGenericRepository<VentaWeb> repository)
        {
            _tiendaService = tiendaService;
            _productService = productService;
            _repository = repository;
        }

        public async Task<List<VentaWeb>> List()
        {
            IQueryable<VentaWeb> query = await _repository.Query();
            return query.Include(_=>_.DetailSales).Include(_=>_.FormaDePago).ToList();
        }


        public async Task<VentaWeb> Edit(VentaWeb entity)
        {
            try
            {
                IQueryable<VentaWeb> query = await _repository.Query(c => c.IdVentaWeb == entity.IdVentaWeb);
                var VentaWeb_found = query.Include(_ => _.DetailSales).First();

                VentaWeb_found.Estado = entity.Estado;
                VentaWeb_found.ModificationDate = DateTime.Now;
                VentaWeb_found.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(VentaWeb_found);

                if (!response)
                    throw new TaskCanceledException("Venta Web no se pudo cambiar.");

                var s = new Sale();

                return VentaWeb_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<VentaWeb> Get(int idVentaWeb)
        {
            IQueryable<VentaWeb> query = await _repository.Query(_=>_.IdVentaWeb == idVentaWeb);
            return query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).First();
        }
    }
}
