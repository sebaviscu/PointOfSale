using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// REFERENCIAS
using PointOfSale.Data.DBContext;
using PointOfSale.Data.Repository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using PointOfSale.Model;
using PointOfSale.Business.Utilities;

namespace PointOfSale.Data.Repository
{
    public class SaleRepository : GenericRepository<Sale>, ISaleRepository
    {
        private readonly POINTOFSALEContext _dbcontext;

        public SaleRepository(POINTOFSALEContext context) : base(context)
        {
            _dbcontext = context;
        }

        public async Task<Sale> Register(Sale entity)
        {
            try
            {
                var productIds = entity.DetailSales.Select(dv => dv.IdProduct).ToList();
                var products = await _dbcontext.Products
                    .Include(p => p.IdCategoryNavigation)
                    .Include(p => p.Proveedor)
                    .Where(p => productIds.Contains(p.IdProduct))
                    .ToListAsync();

                foreach (DetailSale dv in entity.DetailSales)
                {
                    var product_found = products.First(p => p.IdProduct == dv.IdProduct);
                    ControlStock(dv, product_found);

                    dv.TipoVenta = product_found.TipoVenta;
                    dv.CategoryProducty = product_found.IdCategoryNavigation.Description;
                }

                if (string.IsNullOrEmpty(entity.SaleNumber)) // cuando es multiple formas de pago
                {
                    entity.SaleNumber = await GetLastSerialNumberSale();
                }
                entity.RegistrationDate = TimeHelper.GetArgentinaTime();

                await _dbcontext.Sales.AddAsync(entity);
                await _dbcontext.SaveChangesAsync();

                return entity;
            }
            catch (Exception)
            {
                throw;
            }
        }


        private void ControlStock(DetailSale dv, Product p)
        {
            if (p.Minimo != null && p.Minimo > 0)
            {
                p.Quantity -= dv.Quantity;
                if (p.Quantity < 0)
                {
                    p.Quantity = 0;
                }
                else if (p.Quantity <= p.Minimo)
                {
                    var notif = new Notifications(p);
                    _dbcontext.Notificaciones.Add(notif);
                }
                _dbcontext.Products.Update(p);
            }
        }

        public async Task<string> GetLastSerialNumberSale()
        {
            CorrelativeNumber correlative = _dbcontext.CorrelativeNumbers.Where(n => n.Management == "Sale").First();

            correlative.LastNumber = correlative.LastNumber + 1;
            correlative.DateUpdate = TimeHelper.GetArgentinaTime();

            _dbcontext.CorrelativeNumbers.Update(correlative);
            await _dbcontext.SaveChangesAsync();


            string ceros = string.Concat(Enumerable.Repeat("0", correlative.QuantityDigits.Value));
            string saleNumber = ceros + correlative.LastNumber.ToString();
            saleNumber = saleNumber.Substring(saleNumber.Length - correlative.QuantityDigits.Value, correlative.QuantityDigits.Value);
            return saleNumber;
        }

        public async Task<VentaWeb> RegisterWeb(VentaWeb entity)
        {
            var SaleGenerated = new VentaWeb();
            using (var transaction = _dbcontext.Database.BeginTransaction())
            {
                try
                {
                    entity.RegistrationDate = TimeHelper.GetArgentinaTime();
                    foreach (DetailSale dv in entity.DetailSales)
                    {
                        Product product_found = _dbcontext.Products.Include(_ => _.IdCategoryNavigation)
                                                                   .Where(p => p.IdProduct == dv.IdProduct).First();

                        dv.TipoVenta = product_found.TipoVenta;
                        dv.CategoryProducty = product_found.IdCategoryNavigation.Description;
                    }

                    await _dbcontext.VentaWeb.AddAsync(entity);
                    await _dbcontext.SaveChangesAsync();

                    SaleGenerated = entity;

                    transaction.Commit();
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    throw;
                }
            }

            return SaleGenerated;
        }

        public async Task<Sale> CreatSaleFromVentaWeb(VentaWeb entity, Turno turno)
        {
            var sale = new Sale();
            sale.Total = entity.Total;
            sale.RegistrationDate = entity.RegistrationDate;
            sale.IdTienda = entity.IdTienda.Value;
            sale.SaleNumber = await GetLastSerialNumberSale();
            sale.IdTypeDocumentSale = entity.IdFormaDePago;
            sale.IdTurno = turno.IdTurno;
            sale.DetailSales = entity.DetailSales;

            foreach (DetailSale dv in entity.DetailSales)
            {
                Product product_found = _dbcontext.Products.Include(_ => _.Proveedor).Where(p => p.IdProduct == dv.IdProduct).First();

                ControlStock(dv, product_found);
            }

            await _dbcontext.Sales.AddAsync(sale);

            return sale;
        }
    }
}
