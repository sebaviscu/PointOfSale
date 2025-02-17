using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ShopService : IShopService
    {
        private readonly IGenericRepository<DetailSale> _repositoryDetailsSale;
        private readonly IGenericRepository<VentaWeb> _repository;
        private readonly IProductService _productService;
        private readonly ITurnoService _turnoService;
        private readonly ISaleRepository _saleRepository;
        private readonly INotificationService _notificationService;
        private readonly ITypeDocumentSaleService _typeDocumentSaleService;
        private readonly ICorrelativeNumberService _correlativeNumberService;
        public ShopService(IProductService productService, IGenericRepository<VentaWeb> repository, ITurnoService turnoService, ISaleRepository saleRepository, INotificationService notificationService, IGenericRepository<DetailSale> repositoryDetailsSale, ITypeDocumentSaleService typeDocumentSaleService, ICorrelativeNumberService correlativeNumberService)
        {
            _productService = productService;
            _repository = repository;
            _turnoService = turnoService;
            _saleRepository = saleRepository;
            _notificationService = notificationService;
            _repositoryDetailsSale = repositoryDetailsSale;
            _typeDocumentSaleService = typeDocumentSaleService;
            _correlativeNumberService = correlativeNumberService;
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


        public async Task<VentaWeb> Update(Ajustes? ajustes, VentaWeb entity)
        {
            IQueryable<VentaWeb> query = await _repository.Query(c => c.IdVentaWeb == entity.IdVentaWeb);
            var VentaWeb_found = query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).First();

            if (VentaWeb_found.Estado == EstadoVentaWeb.Finalizada)
            {
                throw new TaskCanceledException("No es posible modificar una Venta Web ya finalizada.");
            }

            if (entity.Estado == EstadoVentaWeb.Finalizada)
            {
                if (!entity.IdTienda.HasValue)
                    throw new TaskCanceledException("No es posible finalizar una Venta Web sin Punto de Venta");

                var turnoAbierto = await _turnoService.CheckTurnoAbierto(entity.IdTienda.Value);
                if (!turnoAbierto)
                    throw new TaskCanceledException("No es posible finalizar una Venta Web sin un Turno abierto.");
            }

            bool hasChanges = HasChanges(VentaWeb_found, entity);

            if (hasChanges)
            {
                if (entity.IdFormaDePago.HasValue)
                {
                    var formaPago = await _typeDocumentSaleService.Get(entity.IdFormaDePago.Value);
                    entity.FormaDePago = formaPago;
                }

                VentaWeb_found.SetEditVentaWeb(entity);
                await UpdateDetailSales(VentaWeb_found, entity.DetailSales.ToList());

                VentaWeb_found.Comentario = entity.Comentario;
                VentaWeb_found.Nombre = entity.Nombre;
                VentaWeb_found.Direccion = entity.Direccion;
                VentaWeb_found.Telefono = entity.Telefono;
                VentaWeb_found.IdFormaDePago = entity.IdFormaDePago;
                VentaWeb_found.Total = entity.Total;
                VentaWeb_found.CostoEnvio = entity.CostoEnvio;
                VentaWeb_found.CruceCallesDireccion = entity.CruceCallesDireccion;
                VentaWeb_found.DescuentoRetiroLocal = entity.DescuentoRetiroLocal;
                VentaWeb_found.ObservacionesUsuario = entity.ObservacionesUsuario;
            }
            else if (HasChangesCheckRecogido(VentaWeb_found, entity))
            {
                await UpdateCheckRecogido(VentaWeb_found, entity.DetailSales.ToList());
            }

            VentaWeb_found.Estado = entity.Estado;
            VentaWeb_found.IdTienda = entity.IdTienda;
            VentaWeb_found.ModificationDate = entity.ModificationDate;
            VentaWeb_found.ModificationUser = entity.ModificationUser;

            if (entity.Estado == EstadoVentaWeb.Finalizada && entity.IdTienda.HasValue)
            {
                var turno = await _turnoService.GetTurnoActual(VentaWeb_found.IdTienda.Value);
                if (turno == null)
                {
                    throw new Exception("No existe un turno abierto para la tienda seleccionada.");
                }

                var sale = await _saleRepository.CreatSaleFromVentaWeb(VentaWeb_found, turno, ajustes);
                VentaWeb_found.IdSale = sale.IdSale;
                VentaWeb_found.TipoFactura = sale.TipoFactura;
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
                                   original.Estado != updated.Estado ||
                                   original.Comentario != updated.Comentario ||
                                   original.Nombre != updated.Nombre ||
                                   original.Direccion != updated.Direccion ||
                                   original.Telefono != updated.Telefono ||
                                   original.IdFormaDePago != updated.IdFormaDePago ||
                                   original.CostoEnvio != updated.CostoEnvio ||
                                   original.DescuentoRetiroLocal != updated.DescuentoRetiroLocal ||
                                   (original.ObservacionesUsuario != updated.ObservacionesUsuario && !string.IsNullOrEmpty(updated.ObservacionesUsuario)) ||
                                   original.CruceCallesDireccion != updated.CruceCallesDireccion;

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

        private bool HasChangesCheckRecogido(VentaWeb original, VentaWeb updated)
        {
            bool detailSalesChanged = original.DetailSales.Count != updated.DetailSales.Count ||
                                      original.DetailSales.Any(originalDetail =>
                                          updated.DetailSales.FirstOrDefault(d => d.IdDetailSale == originalDetail.IdDetailSale) is var updatedDetail &&
                                          (updatedDetail == null || originalDetail.Recogido != updatedDetail.Recogido)
                                      );

            return detailSalesChanged;
        }

        private async Task UpdateDetailSales(VentaWeb ventaWebFound, List<DetailSale> updatedDetailSales)
        {
            // Obtener la lista actual de detalles rastreados en la venta
            var currentDetails = ventaWebFound.DetailSales.ToList();

            // Identificar detalles para eliminar (existen en la base pero no en la nueva lista)
            var detailsToRemove = currentDetails
                .Where(existingDetail => !updatedDetailSales.Any(updated => updated.IdDetailSale == existingDetail.IdDetailSale))
                .ToList();

            ventaWebFound.SetEditProductoVentaWeb("Productos eliminados", detailsToRemove);

            foreach (var detail in detailsToRemove)
            {
                ventaWebFound.DetailSales.Remove(detail);
                await _repositoryDetailsSale.Delete(detail); // Asegura eliminación del repositorio
            }

            var productosAgregados = new List<DetailSale>();
            var productosModificados = new List<DetailSale>();
            // Identificar detalles para agregar o actualizar
            foreach (var updatedDetail in updatedDetailSales)
            {
                var existingDetail = currentDetails.FirstOrDefault(detail => detail.IdDetailSale == updatedDetail.IdDetailSale);

                if (existingDetail == null)
                {
                    // Es un nuevo detalle, agregarlo
                    updatedDetail.IdVentaWeb = ventaWebFound.IdVentaWeb;
                    var prod = await _productService.Get(updatedDetail.IdProduct);
                    updatedDetail.CategoryProducty = prod.IdCategoryNavigation.Description;
                    ventaWebFound.DetailSales.Add(updatedDetail);
                    productosAgregados.Add(updatedDetail);
                }
                else
                {
                    // Actualizar detalles existentes si hay cambios
                    //if (existingDetail.Recogido != updatedDetail.Recogido)
                    //{
                    //    //existingDetail.Price = updatedDetail.Price;
                    //    //existingDetail.Quantity = updatedDetail.Quantity;
                    //    //existingDetail.Total = updatedDetail.Total;
                    //    existingDetail.Recogido = updatedDetail.Recogido;
                    //    productosModificados.Add(updatedDetail);
                    //}
                }
            }
            //ventaWebFound.SetEditProductoVentaWeb("Productos modificados", productosModificados);
            ventaWebFound.SetEditProductoVentaWeb("Productos agregados", productosAgregados);

        }

        private async Task UpdateCheckRecogido(VentaWeb ventaWebFound, List<DetailSale> updatedDetailSales)
        {
            // Obtener la lista actual de detalles rastreados en la venta
            var currentDetails = ventaWebFound.DetailSales.ToList();

            // Identificar detalles para agregar o actualizar
            foreach (var updatedDetail in updatedDetailSales)
            {
                var existingDetail = currentDetails.FirstOrDefault(detail => detail.IdDetailSale == updatedDetail.IdDetailSale);

                existingDetail.Recogido = updatedDetail.Recogido;
            }
        }

        //private bool HasChanges(DetailSale existingDetail, DetailSale updatedDetail)
        //{
        //    return existingDetail.Price != updatedDetail.Price ||
        //           existingDetail.Quantity != updatedDetail.Quantity ||
        //           existingDetail.Total != updatedDetail.Total ||
        //           existingDetail.Recogido != updatedDetail.Recogido;
        //}

        public async Task<VentaWeb> Get(int idVentaWeb)
        {
            IQueryable<VentaWeb> query = await _repository.Query(_ => _.IdVentaWeb == idVentaWeb);
            return query.Include(_ => _.DetailSales).Include(_ => _.FormaDePago).First();
        }

        public async Task<VentaWeb> RegisterWeb(VentaWeb entity)
        {
            entity.IdTienda = null;
            entity.SaleNumber = await _correlativeNumberService.GetSerialNumberAndSave(null, "SaleWeb");
            var sale = await _saleRepository.RegisterWeb(entity);
            _ = _notificationService.Save(new Notifications(sale));
            return sale;
        }
    }
}
    