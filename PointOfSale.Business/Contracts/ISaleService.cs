using PointOfSale.Model;
using PointOfSale.Model.Input;
using PointOfSale.Model.Output;
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
        //Task<List<Product>> GetProducts(string search);

        Task<List<Cliente>> GetClients(string search);
        Task<RegisterSaleOutput> RegisterSale(Sale model, RegistrationSaleInput saleInput);

        Task<List<Sale>> SaleHistory(string SaleNumber, string StarDate, string EndDate, string presupuestos, int idTienda);
        Task<Sale> Detail(string SaleNumber);

        Task<Sale> Edit(Sale entity);
        Task<bool> GenerarVentas(int idTienda);
        Task<Sale> GetSale(int idSale);

        Task<List<ListaPrecio>> GetProductsSearchAndIdLista(string search, ListaDePrecio listaPrecios);
        Task<Sale> Edit(int idSale, int formaPago);

        Task<List<Cliente>> GetClientsByFactura(string search);
        Task<string> GetLastSerialNumberSale(int idTienda);

        Task<CorrelativeNumber> CreateSerialNumberSale(int idTienda);

        Task<List<Sale>> HistoryTurnoActual(int idTurno);

        Task<Sale> AnularSale(int idSale, string registrationUser);

        Task<List<ListaPrecio>> GetProductsSearchAndIdListaWithTags(string search, ListaDePrecio listaPrecios);

    }
}
