using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using iTextSharp.text;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using static iTextSharp.text.pdf.events.IndexEvents;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IGenericRepository<ListaPrecio> _repositoryListaPrecios;
        private readonly IGenericRepository<DetailSale> _repositoryDetailSale;
        private readonly IGenericRepository<Vencimiento> _repositoryVencimientos;
        private readonly INotificationService _notificationService;

        public ProductService(IGenericRepository<Product> repository,
            IGenericRepository<ListaPrecio> repositoryListaPrecios,
            IGenericRepository<DetailSale> repositoryDetailSale,
            IGenericRepository<Vencimiento> repositoryVencimientos,
            INotificationService notificationService)
        {
            _repository = repository;
            _repositoryListaPrecios = repositoryListaPrecios;
            _repositoryDetailSale = repositoryDetailSale;
            _repositoryVencimientos = repositoryVencimientos;
            _notificationService = notificationService;
        }

        public async Task<Product> Get(int idProducto)
        {
            IQueryable<Product> queryProduct = await _repository.Query(u => u.IdProduct == idProducto);
            return queryProduct.Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).First();
        }

        public async Task<List<Product>> List()
        {
            IQueryable<Product> query = await _repository.Query();
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).OrderBy(_ => _.Description).ToList();
            //return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).OrderBy(_ => _.Description).Take(3).ToList();
        }

        public async Task<List<Product>> ListActive()
        {
            IQueryable<Product> query = await _repository.Query(_ => _.IsActive);
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).OrderBy(_ => _.Description).ToList();
        }

        public async Task<List<Product>> ListActiveByCategory(int categoryId, int page, int pageSize, string searchText = "")
        {
            IQueryable<Product> query;

            if (categoryId != 0)
            {
                query = await _repository.Query(p => p.IsActive && p.IdCategory.Value  == categoryId);
            }
            else
            {
                query = await _repository.Query(p => p.IsActive);
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p => p.Description.Contains(searchText));
            }

            return await query
                .OrderBy(p => p.Description)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Product>> ListActiveByDescription(string text)
        {
            var query = await _repository.Query(_ => _.Description.Contains(text) && _.IsActive);

            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).OrderBy(_ => _.Description).ToList();
        }


        public async Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos)
        {
            Product product_exists = await _repository.Get(p => p.BarCode != string.Empty && p.BarCode == entity.BarCode);

            if (product_exists != null)
                throw new TaskCanceledException("El barcode ya existe");

            if (entity.Photo.Length == 0)
            {
                entity.Photo = GetDefaultImage();
            }
            try
            {
                Product product_created = await _repository.Add(entity);

                if (product_created.IdProduct == 0)
                    throw new TaskCanceledException("Error al crear producto");

                listaPrecios[0].IdProducto = product_created.IdProduct;
                listaPrecios[1].IdProducto = product_created.IdProduct;
                listaPrecios[2].IdProducto = product_created.IdProduct;

                _ = await _repositoryListaPrecios.Add(listaPrecios[0]);
                _ = await _repositoryListaPrecios.Add(listaPrecios[1]);
                _ = await _repositoryListaPrecios.Add(listaPrecios[2]);

                foreach (var v in vencimientos)
                {
                    v.IdProducto = product_created.IdProduct;
                }

                await EditOrCreateVencimientos(vencimientos);

                IQueryable<Product> query = await _repository.Query(p => p.IdProduct == product_created.IdProduct);
                product_created = query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).First();

                return product_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Product> Add(Product entity)
        {
            Product product_exists = await _repository.Get(p => p.BarCode != string.Empty && p.BarCode == entity.BarCode);

            if (product_exists != null)
                throw new TaskCanceledException("El barcode ya existe");

            if (entity.Photo.Length == 0)
            {
                entity.Photo = GetDefaultImage();
            }

            try
            {
                Product product_created = await _repository.Add(entity);

                if (product_created.IdProduct == 0)
                    throw new TaskCanceledException("Error al crear producto");

                return product_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos)
        {
            Product product_exists = await _repository.Get(p => (p.BarCode != string.Empty && p.BarCode == entity.BarCode) && p.IdProduct != entity.IdProduct);

            if (product_exists != null)
                throw new TaskCanceledException("El barcode ya existe");

            try
            {
                IQueryable<Product> queryProduct1 = await _repository.Query(u => u.IdProduct == entity.IdProduct);
                Product product_edit = queryProduct1.Include(_ => _.ListaPrecios).First();

                product_edit.BarCode = entity.BarCode;
                product_edit.Brand = entity.Brand;
                product_edit.Description = entity.Description;
                product_edit.IdCategory = entity.IdCategory;
                product_edit.Quantity = entity.Quantity < product_edit.Quantity ? product_edit.Quantity : entity.Quantity;
                product_edit.Price = entity.Price;
                product_edit.CostPrice = entity.CostPrice;
                product_edit.PriceWeb = entity.PriceWeb;
                product_edit.PorcentajeProfit = entity.PorcentajeProfit;

                if (entity.Photo != null && entity.Photo.Length > 0)
                    product_edit.Photo = entity.Photo;
                else if((entity.Photo == null || entity.Photo.Length == 0) && (product_edit.Photo == null || product_edit.Photo.Length == 0))
                {
                    product_edit.Photo = GetDefaultImage();
                }

                product_edit.IsActive = entity.IsActive;
                product_edit.ModificationDate = TimeHelper.GetArgentinaTime();
                product_edit.ModificationUser = entity.ModificationUser;
                product_edit.IdProveedor = entity.IdProveedor;
                product_edit.TipoVenta = entity.TipoVenta;
                product_edit.Comentario = entity.Comentario;
                product_edit.Minimo = entity.Minimo;
                product_edit.Iva = entity.Iva;

                bool response = await _repository.Edit(product_edit);
                if (!response)
                    throw new TaskCanceledException("El producto no pudo modificarse");

                foreach (var v in vencimientos.Where(_ => _.IdVencimiento == 0))
                {
                    v.IdProducto = product_edit.IdProduct;
                }

                await EditListaPrecios(product_edit.ListaPrecios.ToList(), listaPrecios);
                await EditOrCreateVencimientos(vencimientos);

                IQueryable<Product> queryProduct = await _repository.Query(u => u.IdProduct == entity.IdProduct);

                Product product_edited = queryProduct.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).First();

                return product_edited;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public byte[] GetDefaultImage()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product_default.jpg");
            return System.IO.File.ReadAllBytes(imagePath);
        }

        public async Task EditListaPrecios(List<ListaPrecio> listaPreciosActual, List<ListaPrecio> listaPreciosNueva)
        {
            try
            {
                foreach (var i in listaPreciosActual)
                {
                    var nuevoPrecio = listaPreciosNueva.First(_ => _.Lista == i.Lista);
                    if (nuevoPrecio.Precio != 0)
                    {
                        i.Precio = nuevoPrecio.Precio;
                        i.PorcentajeProfit = nuevoPrecio.PorcentajeProfit != 0 ? nuevoPrecio.PorcentajeProfit : i.PorcentajeProfit;
                        i.RegistrationDate = TimeHelper.GetArgentinaTime();

                        bool response = await _repositoryListaPrecios.Edit(i);
                        if (!response)
                            throw new TaskCanceledException("La lista de precios no se pudo editar");

                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task EditOrCreateVencimientos(List<Vencimiento> listaVencimientos)
        {
            try
            {
                foreach (var v in listaVencimientos)
                {
                    if (v.IdVencimiento == 0)
                    {
                        await _repositoryVencimientos.Add(v);
                    }
                    else
                    {
                        var oVen = await _repositoryVencimientos.Get(_ => _.IdVencimiento == v.IdVencimiento);
                        oVen.FechaVencimiento = v.FechaVencimiento;
                        oVen.FechaElaboracion = v.FechaElaboracion;
                        oVen.Lote = v.Lote;
                        oVen.Notificar = v.Notificar;
                        await _repositoryVencimientos.Edit(oVen);

                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        public async Task<bool> EditMassive(string user, EditeMassiveProducts data, List<ListaPrecio> listaPrecios)
        {
            try
            {
                foreach (var p in data.idProductos)
                {

                    var product_edit = await GetProductById(p);

                    var precioWeb = product_edit.PriceWeb;
                    var precio1 = listaPrecios.First(_ => _.Lista == ListaDePrecio.Lista_1);

                    if (data.PriceWeb != "" && data.PriceWeb != "0")
                    {
                        precioWeb = Convert.ToDecimal(data.PriceWeb);
                        product_edit.Price = precio1.Precio != 0 ? precio1.Precio : product_edit.Price;
                    }
                    else if (data.PorPorcentaje != "")
                    {
                        var precioWebReal = Convert.ToDecimal(precioWeb * ((Convert.ToDecimal(data.PorPorcentaje) / 100) + 1));
                        precioWeb = RoundToDigits(precioWebReal, data.Redondeo);
                        var priceReal = Convert.ToDecimal(product_edit.Price * ((Convert.ToDecimal(data.PorPorcentaje) / 100) + 1));
                        product_edit.Price = RoundToDigits(priceReal, data.Redondeo);
                    }

                    product_edit.PriceWeb = precioWeb;
                    product_edit.CostPrice = data.Costo != "" ? Convert.ToDecimal(data.Costo) : product_edit.CostPrice;
                    product_edit.PorcentajeProfit = data.Profit != "" ? Convert.ToInt32(data.Profit) : product_edit.PorcentajeProfit;
                    product_edit.IsActive = (bool)(data.IsActive.HasValue ? data.IsActive : product_edit.IsActive);
                    product_edit.Comentario = data.Comentario;
                    product_edit.Iva = data.Iva;
                    product_edit.ModificationUser = user;
                    product_edit.ModificationDate = TimeHelper.GetArgentinaTime();

                    bool response = await _repository.Edit(product_edit);
                    if (!response)
                        throw new TaskCanceledException($"No se ha podido actualizar el producto con Id: {p}");


                    if (data.PorPorcentaje != "")
                    {
                        foreach (var l in listaPrecios)
                        {
                            var nuevoPrecio = product_edit.ListaPrecios.First(_ => _.Lista == l.Lista);
                            var precioReal = Convert.ToDecimal(nuevoPrecio.Precio * ((Convert.ToDecimal(data.PorPorcentaje) / 100) + 1));
                            l.Precio = RoundToDigits(precioReal, data.Redondeo);
                        }
                    }
                    await EditListaPrecios(product_edit.ListaPrecios.ToList(), listaPrecios);

                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> EditMassivePorTabla(string user, List<EditeMassiveProductsTable> data)
        {
            try
            {
                foreach (var p in data)
                {
                    var product_edit = await GetProductById(p.Id);

                    product_edit.Price = p.Precio1;
                    product_edit.PriceWeb = p.PrecioWeb;
                    product_edit.CostPrice = p.Costo;
                    product_edit.ModificationUser = user;
                    product_edit.ModificationDate = TimeHelper.GetArgentinaTime();


                    var listaPrecios = new List<ListaPrecio>();

                    var listasDePrecio = new[]
                    {
                        (ListaDePrecio.Lista_1, p.Precio1),
                        (ListaDePrecio.Lista_2, p.Precio2),
                        (ListaDePrecio.Lista_3, p.Precio3)
                    };

                    foreach (var (lista, precio) in listasDePrecio)
                    {
                        var nuevoPrecio = product_edit.ListaPrecios.FirstOrDefault(_ => _.Lista == lista);
                        if (nuevoPrecio != null)
                        {
                            nuevoPrecio.Precio = precio;
                        }
                    }


                    bool response = await _repository.Edit(product_edit);
                    if (!response)
                        throw new TaskCanceledException($"No se ha podido actualizar el producto con Id: {p}");

                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<Product> GetProductById(int id)
        {
            IQueryable<Product> queryProduct = await _repository.Query(u => u.IdProduct == id);
            var producto = queryProduct.Include(_ => _.ListaPrecios).FirstOrDefault();
            if (producto == null)
                throw new TaskCanceledException($"El producto con Id {id} no existe");

            return producto;
        }

        public async Task<bool> Delete(int idProduct)
        {
            try
            {
                IQueryable<Product> query = await _repository.Query(p => p.IdProduct == idProduct);
                var product_found = query.Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).FirstOrDefault();

                if (product_found == null)
                    throw new TaskCanceledException("El producto no existe");

                if (product_found.ListaPrecios != null)
                {
                    _ = await _repositoryListaPrecios.Delete(product_found.ListaPrecios);
                }

                if (product_found.Vencimientos != null)
                {
                    _ = await _repositoryVencimientos.Delete(product_found.Vencimientos);
                }

                bool response = await _repository.Delete(product_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<List<Product>> GetRandomProducts()
        {
            var prods = await ListActive();
            Random random = new Random();
            var randomProds = prods.OrderBy(x => random.Next()).Take(8).ToList();
            return randomProds;
        }

        public async Task<List<Product>> GetProductsByIdsActive(List<int> listIds, ListaDePrecio listaPrecios)
        {
            var queryProducts = await _repositoryListaPrecios.Query(p =>
               p.Lista == listaPrecios &&
               p.Producto.IsActive == true &&
               listIds.Contains(p.IdProducto));

            return queryProducts.Include(c => c.Producto).OrderBy(_ => _.Producto.Description).ToList().Select(_ => _.Producto).ToList();
        }


        public async Task<List<Product>> GetProductsByIds(List<int> listIds)
        {
            var queryProducts = await _repository.Query(p =>
               listIds.Contains(p.IdProduct));

            return queryProducts.Include(c => c.Proveedor).Include(c => c.IdCategoryNavigation).Include(c => c.ListaPrecios).ToList();
        }

        public async Task<Dictionary<int, string?>> ProductsTopByCategory(string category, string starDate, string endDate, int idTienda)
        {
            try
            {
                starDate = starDate is null ? "" : starDate;
                endDate = endDate is null ? "" : endDate;


                DateTime start_date = DateTime.ParseExact(starDate, "dd/MM/yyyy", new CultureInfo("es-PE"));
                DateTime end_date = DateTime.ParseExact(endDate, "dd/MM/yyyy", new CultureInfo("es-PE"));

                IQueryable<DetailSale> query = await _repositoryDetailSale.Query();

                if (category == "Todo")
                {
                    query = query
                            .Include(v => v.IdSaleNavigation)
                            .Include(v => v.Producto)
                            .Where(dv =>
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start_date.Date
                                    && dv.IdSaleNavigation.RegistrationDate.Value.Date <= end_date.Date
                                    && dv.IdSaleNavigation.IdTienda == idTienda);
                }
                else
                {
                    query = query
                            .Include(v => v.IdSaleNavigation)
                            .Include(v => v.Producto)
                            .Where(dv =>
                                    dv.IdSaleNavigation.RegistrationDate.Value.Date >= start_date.Date
                                    && dv.IdSaleNavigation.RegistrationDate.Value.Date <= end_date.Date
                                    && dv.IdSaleNavigation.IdTienda == idTienda
                                    && dv.CategoryProducty == category);
                }

                Dictionary<int, string?> resultado = query
                .GroupBy(dv => dv.IdProduct).OrderByDescending(g => g.Sum(_ => _.Quantity))
                .Select(dv => new { product = dv.Key, total = dv.Sum(_ => _.Quantity) })
                .ToDictionary(keySelector: r => r.product.Value, elementSelector: r => Math.Truncate(r.total.Value).ToString());

                return resultado;
            }
            catch
            {
                throw;
            }
        }

        public async Task ActivarNotificacionVencimientos(int idTienda)
        {
            var queryProducts = await _repositoryVencimientos.Query(p => p.FechaVencimiento.Date == TimeHelper.GetArgentinaTime().Date && p.Notificar && p.IdTienda == idTienda);
            var vencimientos = queryProducts.Include(c => c.Producto).ToList();

            foreach (var v in vencimientos)
            {
                v.Notificar = false;
                await _repositoryVencimientos.Edit(v);
                var n = new Notifications(v);
                await _notificationService.Save(n);
            }
        }

        public async Task<List<Vencimiento>> GetProximosVencimientos(int idTienda)
        {
            var queryProducts = await _repositoryVencimientos.Query(p => p.IdTienda == idTienda);
            return queryProducts.Include(c => c.Producto).ToList();
        }


        public async Task ActualizarStockAndVencimientos(List<PedidoProducto> pedidoProductos, int idTienda, string registrationUser)
        {
            var idsProds = pedidoProductos.Select(_ => _.IdProducto).ToList();
            var queryProducts = await _repository.Query(p => idsProds.Contains(p.IdProduct));
            //var listaProductos = queryProducts.ToList();

            foreach (var p in queryProducts.ToList())
            {
                var pedProd = pedidoProductos.First(_ => _.IdProducto == p.IdProduct);
                p.Quantity += pedProd.CantidadProductoRecibida;
                await _repository.Edit(p);

                if (pedProd.Vencimiento.HasValue)
                {
                    var v = new Vencimiento();
                    v.IdTienda = idTienda;
                    v.IdProducto = p.IdProduct;
                    v.Notificar = true;
                    v.FechaVencimiento = pedProd.Vencimiento.Value;
                    v.Lote = pedProd.Lote;
                    v.RegistrationDate = TimeHelper.GetArgentinaTime();
                    v.RegistrationUser = registrationUser;

                    await _repositoryVencimientos.Add(v);
                }

            }
        }

        public async Task<bool> DeleteVencimiento(int idVencimiento)
        {
            try
            {
                IQueryable<Vencimiento> query = await _repositoryVencimientos.Query(p => p.IdVencimiento == idVencimiento);
                var vencimiento_found = query.FirstOrDefault();

                if (vencimiento_found == null)
                    throw new TaskCanceledException("El Vencimiento no existe");

                bool response = await _repositoryVencimientos.Delete(vencimiento_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public static decimal RoundToDigits(decimal number, int digits)
        {
            if (digits == 0)
            {
                return number;
            }

            decimal factor = (decimal)Math.Pow(10, digits);
            decimal roundedNumber = Math.Round(number / factor) * factor;

            return roundedNumber;
        }
    }
}
