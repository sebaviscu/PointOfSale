using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface IProductService
    {
        Task<List<Product>> List();
        Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos);
        Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos);
        Task<bool> Delete(int idProduct);
        Task<bool> EditMassive(string usuario, EditeMassiveProducts data, List<ListaPrecio> listaPrecios);
        Task<Product> Get(int idProducto);
        Task<List<Product>> GetRandomProducts();
        Task<List<Product>> ListActive();
        Task<List<Product>> ListActiveByCategory(int idCategoria);
        Task<List<Product>> ListActiveByDescription(string text);
        Task<List<Product>> GetProductsByIdsActive(List<int> listIds, ListaDePrecio listaPrecios);
        Task<Dictionary<int, string?>> ProductsTopByCategory(string category, string start, string end, int idTienda);

        Task<List<Product>> GetProductsByIds(List<int> listIds);
        Task BuscarVencimientosProductos(int idTienda);
    }
}
