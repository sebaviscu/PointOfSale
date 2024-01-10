using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IProductService
    {
        Task<List<Product>> List();
        Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios);
        Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios);
        Task<bool> Delete(int idProduct);
        Task<bool> EditMassive(string usuario, EditeMassiveProducts data);
        Task<Product> Get(int idProducto);
        Task<List<Product>> GetRandomProducts();
        Task<List<Product>> ListActive();
        Task<List<Product>> ListActiveByCategory(int idCategoria);
        Task<List<Product>> ListActiveByDescription(string text);
    }
}
