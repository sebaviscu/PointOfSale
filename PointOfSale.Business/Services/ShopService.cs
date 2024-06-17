using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class ShopService : IShopService
    {
        public DateTime DateTimeNowArg = TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));
        private readonly IGenericRepository<VentaWeb> _repository;
        private readonly ITiendaService _tiendaService;
        private readonly IProductService _productService;
        private readonly ITurnoService _turnoService;
        private readonly ISaleRepository _saleRepository;
        private readonly INotificationService _notificationService;

        public ShopService(ITiendaService tiendaService, IProductService productService, IGenericRepository<VentaWeb> repository, ITurnoService turnoService, ISaleRepository saleRepository, INotificationService notificationService)
        {
            _tiendaService = tiendaService;
            _productService = productService;
            _repository = repository;
            _turnoService = turnoService;
            _saleRepository = saleRepository;
            _notificationService = notificationService;
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
                VentaWeb_found.ModificationDate = DateTimeNowArg;
                VentaWeb_found.ModificationUser = entity.ModificationUser;

                if(entity.Estado == Model.Enum.EstadoVentaWeb.Finalizada)
                {
                    var turno = await _turnoService.GetTurnoActual(VentaWeb_found.IdTienda.Value);
                    await _saleRepository.CreatSaleFromVentaWeb(VentaWeb_found, turno);
                }

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

        public async Task<VentaWeb> RegisterWeb(VentaWeb entity)
        {
            try
            {
                var tienda = await _tiendaService.List();
                entity.IdTienda = tienda.First().IdTienda;
                var sale = await _saleRepository.RegisterWeb(entity);
                _ = _notificationService.Save(new Notifications(sale));
                return sale;
            }
            catch
            {
                throw;
            }
        }
    }
}
