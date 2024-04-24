using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface IAjusteService
    {
        Task<Ajustes> Get();

        Task<Ajustes> Edit(Ajustes entity);
    }
}
