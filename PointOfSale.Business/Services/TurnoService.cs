using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly IGenericRepository<Turno> _repository;
        public TurnoService(IGenericRepository<Turno> repository)
        {
            _repository = repository;
        }

        public async Task<List<Turno>> List(int idtienda)
        {
            var result = await _repository.Query();
            var s = result.Include(_ => _.Sales).OrderByDescending(_ => _.IdTurno).ToList();
            return s;
        }
        public async Task<Turno> Add(Turno entity)
        {
            Turno Turno_created = await _repository.Add(entity);
            if (Turno_created.IdTurno == 0)
                throw new TaskCanceledException("Turno no se pudo crear.");

            return Turno_created;
        }

        public async Task<Turno> Edit(Turno entity)
        {
            try
            {
                Turno Turno_found = await _repository.Get(c => c.IdTurno == entity.IdTurno);

                Turno_found.Descripcion = entity.Descripcion;
                Turno_found.ModificationUser = entity.ModificationUser;

                bool response = await _repository.Edit(Turno_found);

                if (!response)
                    throw new TaskCanceledException("Turno no se pudo cambiar.");

                return Turno_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Turno> CloseTurno(int idtienda, Turno entity)
        {
            try
            {
                var query = await _repository.Query();
                var Turno_found = query.Include(_ => _.Sales).FirstOrDefault(c => c.IdTurno == entity.IdTurno && c.IdTienda == idtienda);

                if(Turno_found == null)
                    throw new TaskCanceledException("Turno no encontrado.");

                if (Turno_found.Sales.Any())
                {
                    if (entity.ModificationUser == null)
                    {
                        Turno_found.FechaFin = Turno_found.FechaInicio.Date.AddDays(1).AddMinutes(-1);
                        Turno_found.ModificationUser = "Automatico";
                    }
                    else
                    {
                        Turno_found.FechaFin = TimeHelper.GetArgentinaTime();
                        Turno_found.ModificationUser = entity.ModificationUser;
                    }
                    Turno_found.Descripcion = entity.Descripcion;

                    bool response = await _repository.Edit(Turno_found);

                    if (!response)
                        throw new TaskCanceledException("Turno no se pudo actualizar.");
                }
                else
                {
                    bool response = await _repository.Delete(Turno_found);
                    if (!response)
                        throw new TaskCanceledException("Turno no se pudo eliminar.");
                }

                return Turno_found;
            }
            catch
            {
                throw;
            }
        }

        public async Task<Turno> GetTurnoActualConVentas(int idtienda)
        {
            var query = await _repository.Query();
            var turno = query.Include(_ => _.Sales).Single(_ => _.FechaFin == null
                                            && _.FechaInicio.Day == TimeHelper.GetArgentinaTime().Day
                                            && _.FechaInicio.Month == TimeHelper.GetArgentinaTime().Month
                                            && _.FechaInicio.Year == TimeHelper.GetArgentinaTime().Year
                                            && _.IdTienda == idtienda);
            return turno;
        }

        public async Task<Turno> GetTurnoActual(int idtienda)
        {
            var query = await _repository.Query();
            var turno = query.Single(_ => _.FechaFin == null
                                            && _.FechaInicio.Day == TimeHelper.GetArgentinaTime().Day
                                            && _.FechaInicio.Month == TimeHelper.GetArgentinaTime().Month
                                            && _.FechaInicio.Year == TimeHelper.GetArgentinaTime().Year
                                            && _.IdTienda == idtienda);
            return turno;
        }

        public async Task CheckTurnosViejos(int idtienda)
        {
            var query = await _repository.Query();
            var turnos = query.Where(_ => _.FechaFin == null && _.FechaInicio.Date <= TimeHelper.GetArgentinaTime().AddDays(-1).Date && _.IdTienda == idtienda).ToList();

            foreach (var t in turnos)
            {
                await CloseTurno(idtienda, t);
            }
        }

        public async Task<Turno> GetTurno(int idTienda, string usuario)
        {
            var query = await _repository.Query();
            var turno = query.SingleOrDefault(_ => _.IdTienda == idTienda
                                            && _.FechaFin == null
                                            && _.FechaInicio.Date == TimeHelper.GetArgentinaTime().Date);

            if (turno == null)
            {
                var t = new Turno(idTienda, usuario);
                turno = await Add(t);
            }
            return turno;
        }

        public async Task<Turno> GetTurno(int idTurno)
        {
            var query = await _repository.Query();
            return query.SingleOrDefault(_ => _.IdTurno == idTurno);
        }

        public async Task<Turno> AbrirTurno(int idTienda, string usuario)
        {
            var turno = await Add(new Turno(idTienda, usuario));

            return turno;
        }
    }
}
