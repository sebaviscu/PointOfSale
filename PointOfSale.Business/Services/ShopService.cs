using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using PointOfSale.Model.Auditoria;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ShopService : IShopService
    {
        private readonly IGenericRepository<DetailSale> _repositoryDetailsSale;
        private readonly IGenericRepository<VentaWeb> _repository;
        private readonly ITiendaService _tiendaService;
        private readonly IProductService _productService;
        private readonly ITurnoService _turnoService;
        private readonly ISaleRepository _saleRepository;
        private readonly INotificationService _notificationService;
        private readonly IAjusteService _ajusteService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly IAfipService _afipService;

        public ShopService(ITiendaService tiendaService, IProductService productService, IGenericRepository<VentaWeb> repository, ITurnoService turnoService, ISaleRepository saleRepository, INotificationService notificationService, IGenericRepository<DetailSale> repositoryDetailsSale, IAjusteService ajusteService, ITypeDocumentSaleService typeDocumentSaleService, IAfipService afipService)
        {
            _tiendaService = tiendaService;
            _productService = productService;
            _repository = repository;
            _turnoService = turnoService;
            _saleRepository = saleRepository;
            _notificationService = notificationService;
            _repositoryDetailsSale = repositoryDetailsSale;
            _ajusteService = ajusteService;
            _typeDocumentSaleService = typeDocumentSaleService;
            _afipService = afipService;
        }

        public async Task<List<VentaWeb>> List()
        {
            IQueryable<VentaWeb> query = await _repository.Query();
            return query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).ToList();
        }

        public async Task<List<VentaWeb>> GetAllByDate(DateTime? registrationDate)
        {
            if (registrationDate.HasValue)
            {
                IQueryable<VentaWeb> query = await _repository.Query(_ => _.RegistrationDate.Value.Date == registrationDate.Value.Date);
                return query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).ToList();
            }
            else
                return await List();
        }


        public async Task<VentaWeb> Update(VentaWeb entity)
        {
            IQueryable<VentaWeb> query = await _repository.Query(c => c.IdVentaWeb == entity.IdVentaWeb);
            var VentaWeb_found = query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).First();

            if (VentaWeb_found.Estado == Model.Enum.EstadoVentaWeb.Finalizada)
            {
                throw new TaskCanceledException("No es posible modificar una Venta Web finalizada.");
            }

            bool hasChanges = HasChanges(VentaWeb_found, entity);


            VentaWeb_found.IsEdit = hasChanges;
            if (hasChanges)
            {
                VentaWeb_found.SetEditVentaWeb(entity.ModificationUser, TimeHelper.GetArgentinaTime());

                VentaWeb_found.Comentario = entity.Comentario;
                VentaWeb_found.Nombre = entity.Nombre;
                VentaWeb_found.Direccion = entity.Direccion;
                VentaWeb_found.Telefono = entity.Telefono;
                VentaWeb_found.IdFormaDePago = entity.IdFormaDePago;
                VentaWeb_found.Total = entity.Total;
                UpdateDetailSales(VentaWeb_found, entity.DetailSales.ToList());
            }

            VentaWeb_found.Estado = entity.Estado;
            VentaWeb_found.IdTienda = entity.IdTienda;
            VentaWeb_found.ModificationDate = TimeHelper.GetArgentinaTime();
            VentaWeb_found.ModificationUser = entity.ModificationUser;

            if (entity.Estado == EstadoVentaWeb.Finalizada && entity.IdTienda.HasValue)
            {
                var ajustes = await _ajusteService.GetAjustes(VentaWeb_found.IdTienda.Value);

                var turno = await _turnoService.GetTurnoActual(VentaWeb_found.IdTienda.Value);
                if(turno == null)
                {
                    throw new Exception("No existe un turno abierto.");
                }
                var sale = await _saleRepository.CreatSaleFromVentaWeb(VentaWeb_found, turno, ajustes);
                VentaWeb_found.IdSale = sale.IdSale;
            }

            bool response = await _repository.Edit(VentaWeb_found);

            if (!response)
                throw new TaskCanceledException("Venta Web no se pudo cambiar.");

            return VentaWeb_found;

        }

        private bool HasChanges(VentaWeb original, VentaWeb updated)
        {
            // Verificar cambios en los campos de VentaWeb
            bool ventaWebChanged =
                                   original.Comentario != updated.Comentario ||
                                   original.Nombre != updated.Nombre ||
                                   original.Direccion != updated.Direccion ||
                                   original.Telefono != updated.Telefono ||
                                   original.IdFormaDePago != updated.IdFormaDePago;

            // Verificar cambios en los DetailSales
            bool detailSalesChanged = original.DetailSales.Count != updated.DetailSales.Count ||
                                      original.DetailSales.Any(originalDetail =>
                                          updated.DetailSales.FirstOrDefault(d => d.IdDetailSale == originalDetail.IdDetailSale) is var updatedDetail &&
                                          (updatedDetail == null ||
                                          originalDetail.DescriptionProduct != updatedDetail.DescriptionProduct ||
                                          originalDetail.Quantity != updatedDetail.Quantity ||
                                          originalDetail.Price != updatedDetail.Price ||
                                          originalDetail.TipoVentaString != updatedDetail.TipoVentaString ||
                                          originalDetail.Total != updatedDetail.Total)
                                      );

            return ventaWebChanged || detailSalesChanged;
        }


        private void UpdateDetailSales(VentaWeb ventaWebFound, List<DetailSale> updatedDetailSales)
        {
            foreach (var existingDetailSale in ventaWebFound.DetailSales.ToList())
            {
                if (!updatedDetailSales.Any(ds => ds.IdDetailSale == existingDetailSale.IdDetailSale))
                {
                    _repositoryDetailsSale.Delete(existingDetailSale);
                }
            }

            foreach (var updatedDetailSale in updatedDetailSales)
            {
                var existingDetailSale = ventaWebFound.DetailSales
                    .FirstOrDefault(ds => ds.IdDetailSale == updatedDetailSale.IdDetailSale);

                if (existingDetailSale == null)
                {
                    updatedDetailSale.IdVentaWeb = ventaWebFound.IdVentaWeb;

                    ventaWebFound.DetailSales.Add(updatedDetailSale);
                }
            }
        }


        public async Task<VentaWeb> Get(int idVentaWeb)
        {
            IQueryable<VentaWeb> query = await _repository.Query(_ => _.IdVentaWeb == idVentaWeb);
            return query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).First();
        }

        public async Task<VentaWeb> RegisterWeb(VentaWeb entity)
        {
            entity.IdTienda = null;
            entity.SaleNumber = await _saleRepository.GetLastSerialNumberSale(null, "SaleWeb");
            var sale = await _saleRepository.RegisterWeb(entity);
            _ = _notificationService.Save(new Notifications(sale));
            return sale;
        }
    }
}
