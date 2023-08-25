using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PointOfSale.Model;

namespace PointOfSale.Business.Contracts
{
    public interface ITypeDocumentSaleService
    {
        Task<List<TypeDocumentSale>> List();
        Task<TypeDocumentSale> Add(TypeDocumentSale entity);
        Task<TypeDocumentSale> Edit(TypeDocumentSale entity);
        Task<bool> Delete(int idUser);
        Task<List<TypeDocumentSale>> GetActive();
        Task<TypeDocumentSale> Get(int idTipoVenta);
    }
}
