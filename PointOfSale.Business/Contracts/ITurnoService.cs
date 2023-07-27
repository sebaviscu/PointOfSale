using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Contracts
{
    public interface ITurnoService
    {
        Task<Turno> Add(Turno entity);
        Task<Turno> Edit(Turno entity);
        Task CheckTurnosViejos();
        Task<Turno> GetTurno(int idTienda, string usuario);
    }
}
