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
using PointOfSale.Model.Auditoria;
using static iTextSharp.text.pdf.events.IndexEvents;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Notifications> _notificationRepository;
        private readonly IGenericRepository<Product> _repository;
        private readonly IGenericRepository<ListaPrecio> _repositoryListaPrecios;
        private readonly IGenericRepository<DetailSale> _repositoryDetailSale;
        private readonly IGenericRepository<Vencimiento> _repositoryVencimientos;
        private readonly IGenericRepository<CodigoBarras> _repositoryCodigosBarras;
        private readonly INotificationService _notificationService;
        private readonly IGenericRepository<Stock> _repositoryStock;

        public ProductService(IGenericRepository<Product> repository,
            IGenericRepository<ListaPrecio> repositoryListaPrecios,
            IGenericRepository<DetailSale> repositoryDetailSale,
            IGenericRepository<Vencimiento> repositoryVencimientos,
            INotificationService notificationService,
            IGenericRepository<Stock> repositoryStock,
            IGenericRepository<Notifications> notificationRepository,
            IGenericRepository<CodigoBarras> repositoryCodigosBarras)
        {
            _repository = repository;
            _repositoryListaPrecios = repositoryListaPrecios;
            _repositoryDetailSale = repositoryDetailSale;
            _repositoryVencimientos = repositoryVencimientos;
            _notificationService = notificationService;
            _repositoryStock = repositoryStock;
            _notificationRepository = notificationRepository;
            _repositoryCodigosBarras = repositoryCodigosBarras;
        }

        public async Task<Product> Get(int idProducto)
        {
            IQueryable<Product> queryProduct = await _repository.Query(u => u.IdProduct == idProducto);
            return queryProduct.Include(_ => _.Proveedor)
                                .Include(_ => _.ListaPrecios)
                                .Include(_ => _.Vencimientos)
                                .Include(_ => _.CodigoBarras)
                                .Include(p => p.ProductTags)
                                    .ThenInclude(pt => pt.Tag)
                                .First();
        }

        public async Task<List<Product>> List()
        {
            IQueryable<Product> query = await _repository.Query();
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).Include(_ => _.CodigoBarras).OrderBy(_ => _.Description).ToList();
            //return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).Include(_ => _.CodigoBarras).OrderBy(_ => _.Description).Take(3).ToList();
        }

        public async Task<List<Product>> ListActive()
        {
            IQueryable<Product> query = await _repository.Query(_ => _.IsActive);
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).Include(_ => _.Vencimientos).OrderBy(_ => _.Description).ToList();
        }

        public async Task<List<Stock>> ListStock(int idTienda)
        {
            IQueryable<Stock> query = await _repositoryStock.Query();
            return await query.Include(_ => _.Producto).Where(_ => _.IdTienda == idTienda).ToListAsync();
        }

        public async Task<Stock?> GetStockByIdProductIdTienda(int idProducto, int idTienda)
        {
            IQueryable<Stock> queryProduct = await _repositoryStock.Query(u => u.IdProducto == idProducto && u.IdTienda == idTienda);
            return queryProduct.FirstOrDefault();
        }

        public async Task<List<Product>> ListActiveByCategory(int categoryId, int page, int pageSize, string searchText = "")
        {
            IQueryable<Product> query;

            if (categoryId != 0)
            {
                query = await _repository.Query(p => p.IsActive && p.IdCategory.Value == categoryId);
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
                .Include(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
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


        public async Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos, Stock? stock, List<CodigoBarras>? codigoBarras, List<Tag> tags)
        {
            if (codigoBarras != null)
            {
                var product_exists = await _repository.QuerySimple()
                    .Include(p => p.CodigoBarras)
                    .Where(p => p.CodigoBarras != null)
                    .ToListAsync();

                if (product_exists.Any(p => p.CodigoBarras.Any(pb => codigoBarras.Any(eb => eb.Codigo == pb.Codigo))))
                {
                    throw new TaskCanceledException("El codigo de barras ya existe");
                }
            }

            if (entity.Photo.Length == 0)
            {
                entity.Photo = GetDefaultImage();
            }

            await UpdateProductTags(entity, tags.Select(_ => _.IdTag).ToList());

            Product product_created = await _repository.Add(entity);

            if (product_created.IdProduct == 0)
                throw new TaskCanceledException("Error al crear el producto");

            if (stock != null)
            {
                stock.IdProducto = product_created.IdProduct;
                _ = await _repositoryStock.Add(stock);
            }

            listaPrecios[0].IdProducto = product_created.IdProduct;
            listaPrecios[1].IdProducto = product_created.IdProduct;
            listaPrecios[2].IdProducto = product_created.IdProduct;

            _ = await _repositoryListaPrecios.Add(listaPrecios[0]);
            _ = await _repositoryListaPrecios.Add(listaPrecios[1]);
            _ = await _repositoryListaPrecios.Add(listaPrecios[2]);

            await EditOrCreateVencimientos(vencimientos, product_created.IdProduct);
            await EditOrCreateCodBarras(codigoBarras, product_created.IdProduct);

            IQueryable<Product> query = await _repository.Query(p => p.IdProduct == product_created.IdProduct);
            product_created = await query.Include(p => p.IdCategoryNavigation)
                                                    .Include(p => p.Proveedor)
                                                    .Include(p => p.ListaPrecios)
                                                    .Include(p => p.Vencimientos)
                                                    .Include(p => p.CodigoBarras)
                                                    .Include(p => p.ProductTags)
                                                    .ThenInclude(pt => pt.Tag)
                                                   .FirstOrDefaultAsync();

            return product_created;
        }

        /// <summary>
        /// Importar Porductos.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        /// <exception cref="TaskCanceledException"></exception>
        public async Task<Product> Add(Product entity)
        {
            var product_exists = await _repository.QuerySimple()
                .Include(p => p.CodigoBarras)
                .Where(p => p.CodigoBarras != null)
                .ToListAsync();

            if (product_exists.Any(p => p.CodigoBarras.Any(pb => entity.CodigoBarras.Any(eb => eb.Codigo == pb.Codigo))))
            {
                throw new TaskCanceledException("El codigo de barras ya existe");
            }

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

        public async Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios, List<Vencimiento> vencimientos, Stock? stock, List<CodigoBarras>? codigoBarras, List<Tag> tags)
        {
            try
            {
                var prodQuery = await _repository.Query(p => p.IdProduct == entity.IdProduct);

                var product = await prodQuery.Include(p => p.ListaPrecios)
                                    .Include(p => p.ProductTags)
                                       .ThenInclude(pt => pt.Tag)
                                       .FirstOrDefaultAsync();

                if (product == null)
                    throw new Exception("Producto no encontrado.");

                // Actualizar campos del producto
                product.Description = entity.Description;
                product.IdCategory = entity.IdCategory;
                product.Price = entity.Price;
                product.CostPrice = entity.CostPrice;
                product.PriceWeb = entity.PriceWeb;
                product.PorcentajeProfit = entity.PorcentajeProfit;
                product.IsActive = entity.IsActive;
                product.ModificationDate = TimeHelper.GetArgentinaTime();
                product.ModificationUser = entity.ModificationUser;
                product.IdProveedor = entity.IdProveedor;
                product.TipoVenta = entity.TipoVenta;
                product.Comentario = entity.Comentario;
                product.Iva = entity.Iva;
                product.FormatoWeb = entity.FormatoWeb;
                product.PrecioFormatoWeb = entity.PrecioFormatoWeb;

                if (entity.Photo != null && entity.Photo.Length > 0)
                    product.Photo = entity.Photo;
                else if ((entity.Photo == null || entity.Photo.Length == 0) && (product.Photo == null || product.Photo.Length == 0))
                {
                    product.Photo = GetDefaultImage();
                }

                await UpdateProductTags(product, tags.Select(_=>_.IdTag).ToList());


                bool response = await _repository.Edit(product);
                if (!response)
                    throw new TaskCanceledException("El producto no pudo modificarse.");

                await EditListaPrecios(product.ListaPrecios.ToList(), listaPrecios);
                await EditOrCreateVencimientos(vencimientos, product.IdProduct);
                await EditOrCreateCodBarras(codigoBarras, product.IdProduct);

                if (stock != null)
                    await EditStock(stock);

                var updatedProduct = await prodQuery.Include(p => p.IdCategoryNavigation)
                                                    .Include(p => p.Proveedor)
                                                    .Include(p => p.ListaPrecios)
                                                    .Include(p => p.Vencimientos)
                                                    .Include(p => p.CodigoBarras)
                                                    .Include(p => p.ProductTags)
                                                    .ThenInclude(pt => pt.Tag)
                                                   .FirstOrDefaultAsync();

                return updatedProduct ?? throw new Exception("Error al cargar el producto actualizado.");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al editar el producto.", ex);
            }
        }


        public byte[] GetDefaultImage()
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/product_default.jpg");
            return System.IO.File.ReadAllBytes(imagePath);
        }


        public async Task EditStock(Stock stock)
        {
            var stockExiste = await _repositoryStock.Get(_ => _.IdProducto == stock.IdProducto && _.IdTienda == stock.IdTienda);

            if (stockExiste == null)
            {
                await _repositoryStock.Add(stock);
            }
            else
            {
                stockExiste.StockMinimo = stock.StockMinimo;
                stockExiste.StockActual = stock.StockActual;

                bool response = await _repositoryStock.Edit(stockExiste);
                if (!response)
                    throw new TaskCanceledException("El stock no se pudo editar");
            }
        }

        private async Task UpdateProductTags(Product product, List<int> tagIds)
        {
            var tagsToRemove = product.ProductTags.Where(pt => !tagIds.Contains(pt.TagId)).ToList();

            foreach (var tagToRemove in tagsToRemove)
            {
                product.ProductTags.Remove(tagToRemove);
            }

            foreach (var tagId in tagIds)
            {
                if (!product.ProductTags.Any(pt => pt.TagId == tagId))
                {
                    product.ProductTags.Add(new ProductTag { ProductId = product.IdProduct, TagId = tagId });
                }
            }

            //await _repository.Edit(product);
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

        public async Task EditOrCreateCodBarras(List<CodigoBarras> codigoBarras, int idProducto)
        {
            foreach (var v in codigoBarras)
            {
                if (v.IdCodigoBarras == 0)
                {
                    v.IdProducto = idProducto;
                    await _repositoryCodigosBarras.Add(v);
                }
                else
                {
                    var cod_exist = await _repositoryCodigosBarras.Get(_ => _.Codigo == v.Codigo && _.IdProducto != idProducto);
                    if (cod_exist != null)
                    {
                        var prod = await _repository.Get(_ => _.IdProduct == cod_exist.IdProducto);
                        throw new Exception($"El codigo de barras ingresado ya existe en otro producto {prod.Description}.");
                    }

                    var cb = await _repositoryCodigosBarras.Get(_ => _.IdCodigoBarras == v.IdCodigoBarras);
                    cb.Codigo = v.Codigo;
                    cb.Descripcion = v.Descripcion;
                    await _repositoryCodigosBarras.Edit(cb);

                }
            }
        }

        public async Task EditOrCreateVencimientos(List<Vencimiento> listaVencimientos, int idProducto)
        {
            foreach (var v in listaVencimientos)
            {
                if (v.IdVencimiento == 0)
                {
                    v.IdProducto = idProducto;
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

            return queryProducts.Include(c => c.Producto).Include(c => c.Producto.CodigoBarras).OrderBy(_ => _.Producto.Description).ToList().Select(_ => _.Producto).ToList();
        }


        public async Task<List<Product>> GetProductsByIds(List<int> listIds)
        {
            var queryProducts = await _repository.Query(p =>
               listIds.Contains(p.IdProduct));

            return queryProducts.Include(c => c.Proveedor).Include(c => c.IdCategoryNavigation).Include(c => c.ListaPrecios).ToList();
        }
        public async Task<List<Stock>> GetStockByProductsByIds(List<int> listIds, int idTienda)
        {
            var queryProducts = await _repositoryStock.Query(p =>
                p.IdTienda == idTienda && listIds.Contains(p.IdProducto));

            return queryProducts.ToList();
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

            foreach (var p in queryProducts.ToList())
            {
                var pedProd = pedidoProductos.First(_ => _.IdProducto == p.IdProduct);

                if (pedProd.CantidadProductoRecibida.HasValue)
                {
                    await UpdateStock(idTienda, p, pedProd.CantidadProductoRecibida.Value);
                }

                if (pedProd.Vencimiento.HasValue)
                {
                    var v = new Vencimiento()
                    {
                        IdTienda = idTienda,
                        IdProducto = p.IdProduct,
                        Notificar = true,
                        FechaVencimiento = pedProd.Vencimiento.Value,
                        Lote = pedProd.Lote,
                        RegistrationDate = TimeHelper.GetArgentinaTime(),
                        RegistrationUser = registrationUser
                    };

                    await _repositoryVencimientos.Add(v);
                }

            }
        }

        public async Task UpdateStock(int idTienda, Product p, int stockRecibido)
        {
            var stockExiste = await _repositoryStock.Get(_ => _.IdTienda == idTienda && _.IdProducto == p.IdProduct);

            if (stockExiste == null)
            {
                var stock = new Stock(stockRecibido, 0, p.IdProduct, idTienda);

                await _repositoryStock.Add(stock);
            }
            else
            {
                stockExiste.StockActual += stockRecibido;

                bool response = await _repositoryStock.Edit(stockExiste);
                if (!response)
                    throw new TaskCanceledException("El stock no se pudo editar");
            }
        }

        public async Task<bool> DeleteVencimiento(int idVencimiento)
        {
            IQueryable<Vencimiento> query = await _repositoryVencimientos.Query(p => p.IdVencimiento == idVencimiento);
            var vencimiento_found = query.FirstOrDefault();

            if (vencimiento_found == null)
                throw new TaskCanceledException("El Vencimiento no existe");

            bool response = await _repositoryVencimientos.Delete(vencimiento_found);

            return response;
        }
        public async Task<bool> DeleteCodigoBarras(int idCodigoBarras)
        {
            IQueryable<CodigoBarras> query = await _repositoryCodigosBarras.Query(p => p.IdCodigoBarras == idCodigoBarras);
            var codBarras_found = query.FirstOrDefault();

            if (codBarras_found == null)
                throw new TaskCanceledException("El Vencimiento no existe");

            bool response = await _repositoryCodigosBarras.Delete(codBarras_found);

            return response;
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
