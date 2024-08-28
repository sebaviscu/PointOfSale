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
        Task<List<Turno>> List(int idtienda);
        Task<Turno> Add(Turno entity);
        Task<Turno> Edit(Turno entity);
        Task CheckTurnosViejos(int idtienda);
        Task<Turno> GetTurno_OrCreate(int idTienda, string usuario);
        Task<Turno?> GetTurnoActual(int idtienda);
        Task<Turno> CloseTurno(int idtienda, Turno entity);
        Task<Turno?> GetTurnoActualConVentas(int idtienda);

        Task<Turno> AbrirTurno(int idTienda, string usuario);
        Task<Turno> GetTurno(int idTurno);


    }
}
