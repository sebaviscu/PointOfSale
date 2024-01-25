using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        private readonly IGenericRepository<ListaPrecio> _repositoryListaPrecios;
        public ProductService(IGenericRepository<Product> repository, IGenericRepository<ListaPrecio> repositoryListaPrecios)
        {
            _repository = repository;
            _repositoryListaPrecios = repositoryListaPrecios;
        }

        public async Task<Product> Get(int idProducto)
        {
            return await _repository.Get(p => p.IdProduct == idProducto);

        }

        public async Task<List<Product>> List()
        {
            IQueryable<Product> query = await _repository.Query();
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).OrderBy(_ => _.Description).ToList();
        }
        public async Task<List<Product>> ListActive()
        {
            IQueryable<Product> query = await _repository.Query(_ => _.IsActive.HasValue ? _.IsActive.Value : false);
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).OrderBy(_ => _.Description).ToList();
        }

        public async Task<List<Product>> ListActiveByCategory(int idCategoria)
        {
            IQueryable<Product> query;
            if (idCategoria == 0)
            {
                query = await _repository.Query(_ => _.IsActive.HasValue ? _.IsActive.Value : false);
            }
            else
            {
                query = await _repository.Query(_ => _.IdCategory == idCategoria && _.IsActive.HasValue ? _.IsActive.Value : false);
            }

            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).OrderBy(_ => _.Description).ToList();
        }
        public async Task<List<Product>> ListActiveByDescription(string text)
        {
            var query = await _repository.Query(_ => _.Description.Contains(text) && _.IsActive.HasValue ? _.IsActive.Value : false);

            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).OrderBy(_ => _.Description).ToList();
        }

        public async Task<Product> Add(Product entity, List<ListaPrecio> listaPrecios)
        {
            Product product_exists = await _repository.Get(p => p.BarCode != string.Empty && p.BarCode == entity.BarCode);

            if (product_exists != null)
                throw new TaskCanceledException("El barcode ya existe");

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

                IQueryable<Product> query = await _repository.Query(p => p.IdProduct == product_created.IdProduct);
                product_created = query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).First();

                return product_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Product> Edit(Product entity, List<ListaPrecio> listaPrecios)
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
                product_edit.Quantity = entity.Quantity < product_edit.Quantity ? product_edit.Quantity : entity.Quantity; // que no sea menor al que ya tiene
                product_edit.Price = entity.Price;
                product_edit.CostPrice = entity.CostPrice;
                product_edit.PriceWeb = entity.PriceWeb;
                product_edit.PorcentajeProfit = entity.PorcentajeProfit;
                if (entity.Photo != null && entity.Photo.Length > 0)
                    product_edit.Photo = entity.Photo;
                product_edit.IsActive = entity.IsActive;
                product_edit.ModificationDate = DateTime.Now;
                product_edit.ModificationUser = entity.ModificationUser;
                product_edit.IdProveedor = entity.IdProveedor;
                product_edit.TipoVenta = entity.TipoVenta;
                product_edit.Comentario = entity.Comentario;
                product_edit.Minimo = entity.Minimo;

                bool response = await _repository.Edit(product_edit);
                if (!response)
                    throw new TaskCanceledException("El producto no pudo modificarse");

                await EditListaPrecios(product_edit.ListaPrecios.ToList(), listaPrecios);

                IQueryable<Product> queryProduct = await _repository.Query(u => u.IdProduct == entity.IdProduct);

                Product product_edited = queryProduct.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).Include(_ => _.ListaPrecios).First();

                return product_edited;
            }
            catch (Exception)
            {
                throw;
            }
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
                        i.RegistrationDate = DateTime.Now;

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


        public async Task<bool> EditMassive(string user, EditeMassiveProducts data, List<ListaPrecio> listaPrecios)
        {
            try
            {
                foreach (var p in data.idProductos)
                {

                    IQueryable<Product> queryProduct1 = await _repository.Query(u => u.IdProduct == p);
                    Product product_edit = queryProduct1.Include(_ => _.ListaPrecios).FirstOrDefault();

                    if (product_edit == null)
                        throw new TaskCanceledException($"El producto con Id {p} no existe");

                    var precioWeb = product_edit.PriceWeb;

                    if (data.PriceWeb != "")
                    {
                        precioWeb = Convert.ToDecimal(data.PriceWeb);
                    }
                    else if (data.PorPorcentaje != "")
                    {
                        precioWeb = Convert.ToDecimal(precioWeb * ((Convert.ToDecimal(data.PorPorcentaje) / 100) + 1));
                    }

                    var precio1 = listaPrecios.First(_ => _.Lista == ListaDePrecio.Lista_1);
                    product_edit.Price = precio1.Precio != 0 ? precio1.Precio : product_edit.Price;
                    product_edit.PriceWeb = precioWeb;
                    product_edit.CostPrice = data.Costo != "" ? Convert.ToDecimal(data.Costo) : product_edit.CostPrice;
                    product_edit.PorcentajeProfit = data.Profit != "" ? Convert.ToInt32(data.Profit) : product_edit.PorcentajeProfit;
                    product_edit.IsActive = data.IsActive;
                    product_edit.Comentario = data.Comentario;
                    product_edit.ModificationUser = user;
                    product_edit.ModificationDate = DateTime.Now;

                    bool response = await _repository.Edit(product_edit);
                    if (!response)
                        throw new TaskCanceledException($"No se ha podido actualizar el producto con Id: {p}");


                    if (data.PorPorcentaje != "")
                    {
                        foreach (var l in listaPrecios)
                        {
                            var nuevoPrecio = product_edit.ListaPrecios.First(_ => _.Lista == l.Lista);
                            l.Precio = Convert.ToDecimal(nuevoPrecio.Precio * ((Convert.ToDecimal(data.PorPorcentaje) / 100) + 1));
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


        public async Task<bool> Delete(int idProduct)
        {
            try
            {
                Product product_found = await _repository.Get(p => p.IdProduct == idProduct);

                if (product_found == null)
                    throw new TaskCanceledException("El producto no existe");

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

            return prods.Take(8).ToList();

        }
    }
}
