using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IPagoEmpresaService : IServiceBase<PagoEmpresa>
    {
        Task<Empresa> GetEmpresa();
        Task<bool> UpdateEmpresa(Empresa empresa);
    }
}
