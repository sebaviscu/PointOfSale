using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Contracts
{
    public interface ILovService : IServiceBase<Lov>
    {
        Task<List<Lov>> GetLovByType(LovType lovType);
        Task<List<Lov>> GetLovActiveByType(LovType lovType);
    }
}
