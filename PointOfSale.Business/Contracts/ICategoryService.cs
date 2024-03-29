﻿using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface ICategoryService
    {
        Task<List<Category>> List();
        Task<Category> Add(Category entity);
        Task<Category> Edit(Category entity);
        Task<bool> Delete(int idCategory);
        Task<List<Category>> GetCategoriesSearch(string search);
        Task<Category> Get(int idProducto);
        Task<List<Category>> GetMultiple(int[] idCategorys);
        Task<List<Category>> ListActive();
    }
}
