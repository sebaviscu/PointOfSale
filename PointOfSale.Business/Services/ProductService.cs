using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;

namespace PointOfSale.Business.Services
{
    public class ProductService : IProductService
    {
        private readonly IGenericRepository<Product> _repository;
        public ProductService(IGenericRepository<Product> repository)
        {
            _repository = repository;
        }

        public async Task<Product> Get(int idProducto)
        {
            return await _repository.Get(p => p.IdProduct == idProducto);

        }

        public async Task<List<Product>> List()
        {
            IQueryable<Product> query = await _repository.Query();
            return query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).OrderBy(_ => _.Description).ToList();
        }
        public async Task<Product> Add(Product entity)
        {
            Product product_exists = await _repository.Get(p =>p.BarCode != string.Empty && p.BarCode == entity.BarCode);

            if (product_exists != null)
                throw new TaskCanceledException("The barcode already exists");

            try
            {
                Product product_created = await _repository.Add(entity);

                if (product_created.IdProduct == 0)
                    throw new TaskCanceledException("Error al crear product");

                IQueryable<Product> query = await _repository.Query(p => p.IdProduct == product_created.IdProduct);
                product_created = query.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).First();

                return product_created;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<Product> Edit(Product entity)
        {
            Product product_exists = await _repository.Get(p => (p.BarCode != string.Empty && p.BarCode == entity.BarCode) && p.IdProduct != entity.IdProduct);

            if (product_exists != null)
                throw new TaskCanceledException("The barcode already exists");

            try
            {
                Product product_edit = await _repository.First(u => u.IdProduct == entity.IdProduct);

                product_edit.BarCode = entity.BarCode;
                product_edit.Brand = entity.Brand;
                product_edit.Description = entity.Description;
                product_edit.IdCategory = entity.IdCategory;
                product_edit.Quantity = entity.Quantity;
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

                bool response = await _repository.Edit(product_edit);
                if (!response)
                    throw new TaskCanceledException("The product could not be modified");

                Product product_edited = queryProduct.Include(c => c.IdCategoryNavigation).Include(_ => _.Proveedor).First();

                return product_edited;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> EditMassive(string user, EditeMassiveProducts data)
        {
            try
            {
                foreach (var p in data.idProductos)
                {

                    Product product_edit = await _repository.Get(_ => _.IdProduct == p);

                    if (product_edit == null)
                        throw new TaskCanceledException($"El producto con Id {p} no existe");


                    product_edit.Price = data.Precio != "" ? Convert.ToDecimal(data.Precio) : product_edit.Price;
                    product_edit.PriceWeb = data.PriceWeb != "" ? Convert.ToDecimal(data.PriceWeb) : product_edit.PriceWeb;
                    product_edit.CostPrice = data.Costo != "" ? Convert.ToDecimal(data.Costo) : product_edit.CostPrice;
                    product_edit.PorcentajeProfit = data.Profit != "" ? Convert.ToInt32(data.Profit) : product_edit.PorcentajeProfit;
                    product_edit.IsActive = data.IsActive;
                    product_edit.ModificationUser = user;
                    product_edit.ModificationDate = DateTime.Now;

                    bool response = await _repository.Edit(product_edit);
                    if (!response)
                        throw new TaskCanceledException("The product could not be modified");
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
                    throw new TaskCanceledException("The product no existe");

                bool response = await _repository.Delete(product_found);

                return response;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
