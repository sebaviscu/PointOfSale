
using PointOfSale.Data.DBContext;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Model;
using PointOfSale.Business.Utilities;
using Microsoft.Win32;

namespace PointOfSale.Data.Repository
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        private readonly POINTOFSALEContext _dbcontext;
        private readonly IGenericRepository<Stock> _repositoryStock;
        private readonly IGenericRepository<Notifications> _notificationRepository;

        public SaleRepository(POINTOFSALEContext context, IGenericRepository<Stock> repositoryStock, IGenericRepository<Notifications> notificationRepository) : base(context)
        {
            _dbcontext = context;
            _repositoryStock = repositoryStock;
            _notificationRepository = notificationRepository;
        }

        public async Task<Sale> Register(Sale entity, Ajustes ajustes)
        {

            // Obtener productos solo si se controla el stock
            List<Product> products = null;
            if (ajustes.ControlStock.HasValue && ajustes.ControlStock.Value)
            {
                var productIds = entity.DetailSales.Select(dv => dv.IdProduct).Distinct().ToList();
                products = await _dbcontext.Products
                                           .Where(p => productIds.Contains(p.IdProduct))
                                           .ToListAsync();

                // Parallel execution for stock control
                if (products != null)
                {
                    var stockTasks = entity.DetailSales.Select(dv =>
                    {
                        var product = products.First(p => p.IdProduct == dv.IdProduct);
                        return ControlStock(dv.Quantity.Value, entity.IdTienda, product);
                    }).ToList();

                    // Await all stock control tasks concurrently
                    await Task.WhenAll(stockTasks);
                }
            }

            entity.RegistrationDate = TimeHelper.GetArgentinaTime();

            await _dbcontext.Sales.AddAsync(entity);
            await _dbcontext.SaveChangesAsync();

            return entity;
        }


        private async Task ControlStock(decimal cantidad, int idTienda, Product p)
        {
            var stockExiste = await _repositoryStock.Get(_ => _.IdTienda == idTienda && _.IdProducto == p.IdProduct);

            if (stockExiste != null && stockExiste.StockActual > 0)
            {
                stockExiste.StockActual -= cantidad;

                bool response = await _repositoryStock.Edit(stockExiste);
                if (!response)
                    throw new TaskCanceledException("El stock no se pudo editar");

                if (stockExiste.StockActual <= stockExiste.StockMinimo)
                {
                    var notif = new Notifications(p);
                    _ = _notificationRepository.Add(notif);
                }
            }
        }

        public async Task<string> GetLastSerialNumberSale(int? idTienda, string management)
        {
            var query = _dbcontext.CorrelativeNumbers
                                              .Where(n => n.Management == management);

            if(idTienda != null)
            {
                query = query.Where(n => n.IdTienda == idTienda);
            }

            var correlative = await query.FirstOrDefaultAsync();

            if (correlative == null)
            {
                throw new Exception($"No se encontró el número correlativo para la gestión '{management}'.");
            }

            lock (correlative)
            {
                correlative.LastNumber += 1;
                correlative.DateUpdate = TimeHelper.GetArgentinaTime();
            }

            _dbcontext.CorrelativeNumbers.Update(correlative);

            string ceros = new string('0', correlative.QuantityDigits.Value);
            string saleNumber = (ceros + correlative.LastNumber.ToString())
                                 .Substring(ceros.Length + correlative.LastNumber.ToString().Length - correlative.QuantityDigits.Value);

            return saleNumber;
        }


        public async Task<CorrelativeNumber> CreateSerialNumberSale(int idTienda)
        {
            var c = new CorrelativeNumber()
            {
                LastNumber = 0,
                IdTienda = idTienda,
                QuantityDigits = 6,
                Management = "Sale",
                DateUpdate = TimeHelper.GetArgentinaTime()
            };
            
            await _dbcontext.CorrelativeNumbers.AddAsync(c);
            await _dbcontext.SaveChangesAsync();

          
            return c;
        }

        public async Task<VentaWeb> RegisterWeb(VentaWeb entity)
        {
            try
            {
                entity.RegistrationDate = TimeHelper.GetArgentinaTime();

                var productIds = entity.DetailSales.Select(dv => dv.IdProduct).ToList();
                var products = await _dbcontext.Products
                                               .Include(p => p.IdCategoryNavigation)
                                               .Where(p => productIds.Contains(p.IdProduct))
                                               .ToDictionaryAsync(p => p.IdProduct);

                foreach (var dv in entity.DetailSales)
                {
                    if (products.TryGetValue(dv.IdProduct.Value, out var product))
                    {
                        dv.TipoVenta = product.TipoVenta;
                        dv.CategoryProducty = product.IdCategoryNavigation.Description;
                    }
                    else
                    {
                        throw new Exception($"Producto con Id {dv.IdProduct} no encontrado.");
                    }
                }

                await _dbcontext.VentaWeb.AddAsync(entity);

                await _dbcontext.SaveChangesAsync();

                return entity;
            }
            catch (Exception ex)
            {
                throw new Exception("Ocurrió un error al guardar la venta.", ex);
            }
        }

        public async Task<Sale> CreatSaleFromVentaWeb(VentaWeb entity, Turno turno, Ajustes ajustes)
        {
            var sale = new Sale();
            sale.Total = entity.Total;
            sale.RegistrationDate = entity.RegistrationDate;
            sale.IdTienda = entity.IdTienda.Value;
            sale.SaleNumber = await GetLastSerialNumberSale(entity.IdTienda.Value, "Sale");
            sale.IdTypeDocumentSale = entity.IdFormaDePago;
            sale.IdTurno = turno.IdTurno;
            sale.DetailSales = entity.DetailSales;
            sale.RegistrationUser = entity.ModificationUser;
            sale.DescuentoRecargo = 0;
            sale.IsWeb = true;
            sale.Observaciones = entity.Comentario;

            var saleCreated = await Register(sale, ajustes);

            return saleCreated;
        }
    }
}
