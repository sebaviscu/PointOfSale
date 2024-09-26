using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class TagService : ITagService
    {

        private readonly IGenericRepository<Tag> _repositoryTag;
        private readonly IGenericRepository<Product> _repositoryProduct;
        private readonly IGenericRepository<ProductTag> _repositoryProductTag;

        public TagService(IGenericRepository<Tag> repositoryTag, IGenericRepository<Product> repositoryProduct, IGenericRepository<ProductTag> repositoryProductTag)
        {
            _repositoryTag = repositoryTag;
            _repositoryProduct = repositoryProduct;
            _repositoryProductTag = repositoryProductTag;
        }

        public async Task<List<Tag>> List()
        {
            var query = await _repositoryTag.Query();
            return query.OrderBy(_ => _.Nombre).ToList();
        }

        public async Task<Tag> Add(Tag entity)
        {
            var tag_created = await _repositoryTag.Add(entity);
            if (tag_created.IdTag == 0)
                throw new TaskCanceledException("Category no se pudo crear.");

            return tag_created;
        }

        public async Task<Tag> Edit(Tag entity)
        {
            var tag_edit = await _repositoryTag.First(_ => _.IdTag == entity.IdTag);

            tag_edit.Nombre = entity.Nombre;
            tag_edit.Color = entity.Color;

            await _repositoryTag.EditAsync(tag_edit);
            await _repositoryProductTag.SaveChangesAsync();

            return tag_edit;
        }

        public async Task<bool> Delete(int idTag)
        {
            var category_found = await _repositoryTag.Get(c => c.IdTag == idTag);

            if (category_found == null)
                throw new TaskCanceledException("El tag no existe");


            return await _repositoryTag.Delete(category_found);
        }

        public async Task AddTagToProduct(int productId, int tagId)
        {
            var product = await _repositoryProduct.QuerySimple().Include(p => p.ProductTags)
                                                 .FirstOrDefaultAsync(p => p.IdProduct == productId);

            if (product.ProductTags.Count >= 3)
            {
                throw new InvalidOperationException("Un producto no puede tener más de 3 tags.");
            }

            var productTag = new ProductTag
            {
                ProductId = productId,
                TagId = tagId
            };

            await _repositoryProductTag.AddAsync(productTag);
            await _repositoryProductTag.SaveChangesAsync();
        }

        public async Task<List<Product>> ListProductsByTagWeb(int tagId, int page, int pageSize, string searchText = "")
        {
            var query = tagId != 0
                ?await _repositoryProductTag.Query(p => p.TagId == tagId)
                : await _repositoryProductTag.Query();

            var queryProduct = query
                .Include(pt => pt.Product)
                .ThenInclude(p => p.ProductTags)
                .ThenInclude(pt => pt.Tag)
                .Select(pt => pt.Product);

            if (!string.IsNullOrEmpty(searchText))
            {
                queryProduct = queryProduct.Where(p => p.Description.Contains(searchText));
            }

            queryProduct = queryProduct.Where(p => p.IsActive && p.ProductoWeb);

            return await queryProduct
                .OrderBy(p => p.Description)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

    }
}
