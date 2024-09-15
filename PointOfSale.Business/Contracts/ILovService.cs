using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface ILovService
    {
        Task<List<Lov>> GetLovByType(LovType lovType);

        Task<Lov> Add(Lov entity);

        Task<Lov> Edit(Lov entity);
        Task<bool> Delete(int idLov);
    }
}
