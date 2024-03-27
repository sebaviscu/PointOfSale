using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface ISaleService
    {
        Task<List<Product>> GetProducts(string search);

        Task<List<Cliente>> GetClients(string search);
        Task<Sale> Register(Sale entity);

        Task<List<Sale>> SaleHistory(string SaleNumber, string StarDate, string EndDate, bool incluirPresupuestos);
        Task<Sale> Detail(string SaleNumber);

        Task<Sale> Edit(Sale entity);
        Task<bool> GenerarVentas(int idTienda, int idUser);
        Task<Sale> GetSale(int idSale);

        Task<List<Product>> GetProductsSearchAndIdLista(string search, ListaDePrecio listaPrecios);
        Task<Sale> Edit(int idSale, int formaPago);
    }
}
