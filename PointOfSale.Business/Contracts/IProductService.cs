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
        Task<string> Add(List<Product> entity);
        Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos, Stock? stock, List<CodigoBarras>? codigoBarras, List<Tag> tags, List<ProductLov> Comodines);
        Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos, Stock? stock, List<CodigoBarras>? codigoBarras, List<Tag> tags, List<ProductLov> Comodines);
        Task<bool> Delete(int idProduct);
        Task<bool> EditMassive(string usuario, EditeMassiveProducts data, List<ListaPrecio> listaPrecios);
        Task<Product> Get(int idProducto);
        Task<List<Product>> ListActive();
        Task<List<Product>> ListByIds(List<int> idsProducts);
        Task<List<Product>> ListActiveByCategoryWeb(int categoryId, int page, int pageSize, string searchText = "");
        Task<List<Product>> ListActiveByDescriptionWeb(string text);
        Task<List<Product>> GetProductsByIdsActive(List<int> listIds, ListaDePrecio listaPrecios);
        Task<Dictionary<int, string?>> ProductsTopByCategory(string category, string start, string end, int idTienda);

        Task<List<Product>> GetProductsByIds(List<int> listIds);
        Task ActivarNotificacionVencimientos(int idTienda);
        Task<List<Vencimiento>> GetProximosVencimientos(int idTienda);
        Task ActualizarStockAndVencimientos(List<PedidoProducto> pedidoProductos, int idTienda, string registrationUser);
        Task<bool> DeleteVencimiento(int idVencimiento);
        Task<bool> EditMassivePorTabla(string user, List<EditeMassiveProductsTable> data);
        Task UpdateStock(int idTienda, Product p, int stockRecibido);
        Task<Stock?> GetStockByIdProductIdTienda(int idProducto, int idTienda);
        Task<List<Stock>> GetStockByProductsByIds(List<int> listIds, int idTienda);
        Task<List<Stock>> ListStock(int idTienda);

        Task<bool> DeleteCodigoBarras(int idCodigoBarras);

        Task<List<Product>> GetProductosDestacadosWeb();

        Task<List<Product>> ProdctuosPreciosByCategory(string category, string? modificationDate, ListaDePrecio listaPrecio);

        Task<Vencimiento> AddVencimiento(Vencimiento entity);
        Task<Vencimiento> EditVencimiento(Vencimiento entity);
    }
}