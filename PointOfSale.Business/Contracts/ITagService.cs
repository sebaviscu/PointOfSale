using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface ITagService
    {
        Task<List<Tag>> List();

        Task<Tag> Add(Tag entity);
        Task<Tag> Edit(Tag entity);

        Task<bool> Delete(int idTag);

        Task AddTagToProduct(int productId, int tagId);

        Task<List<Product>> ListProductsByTag(int tagId, int page, int pageSize, string searchText = "");
    }
}
