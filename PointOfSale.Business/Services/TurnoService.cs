using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static iTextSharp.text.pdf.events.IndexEvents;

namespace PointOfSale.Business.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly IGenericRepository<Turno> _repository;
        private readonly IEmailService _emailService;
        private readonly IAjusteService _ajusteService;
        public TurnoService(IGenericRepository<Turno> repository, IEmailService emailService, IAjusteService ajusteService)
        {
            _repository = repository;
            _emailService = emailService;
            _ajusteService = ajusteService;
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

                Turno_found.ModificationUser = entity.ModificationUser;
                Turno_found.ObservacionesCierre = entity.ObservacionesCierre;
                Turno_found.FechaFin = entity.FechaFin;

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

        private async Task<Turno> CloseTurno(Turno turno)
        {

            if (turno.Sales.Any())
            {
                turno.FechaFin = turno.FechaInicio.Date.AddDays(1).AddMinutes(-1);
                turno.ModificationUser = "Automatico";


                bool response = await _repository.Edit(turno);

                if (!response)
                    throw new TaskCanceledException("Turno no se pudo actualizar.");
            }
            else
            {
                bool response = await _repository.Delete(turno);
                if (!response)
                    throw new TaskCanceledException("Turno no se pudo eliminar.");
            }

            return turno;

        }

        public async Task<Turno?> GetTurnoActualConVentas(int idtienda)
        {
            var query = await _repository.Query();
            var turno = query.Include(_ => _.Sales).FirstOrDefault(_ => _.FechaFin == null
                                            && _.FechaInicio.Day == TimeHelper.GetArgentinaTime().Day
                                            && _.FechaInicio.Month == TimeHelper.GetArgentinaTime().Month
                                            && _.FechaInicio.Year == TimeHelper.GetArgentinaTime().Year
                                            && _.IdTienda == idtienda);

            return turno;
        }

        public async Task<Turno?> GetTurnoActual(int idtienda)
        {
            var query = await _repository.Query();
            var turno = query.SingleOrDefault(_ => _.FechaFin == null
                                            && _.FechaInicio.Day == TimeHelper.GetArgentinaTime().Day
                                            && _.FechaInicio.Month == TimeHelper.GetArgentinaTime().Month
                                            && _.FechaInicio.Year == TimeHelper.GetArgentinaTime().Year
                                            && _.IdTienda == idtienda);
            return turno;
        }

        public async Task CheckTurnosViejos(int idtienda)
        {
            var query = await _repository.Query();
            var turnos = query.Include(_ => _.Sales).Where(_ => _.FechaFin == null && _.FechaInicio.Date <= TimeHelper.GetArgentinaTime().AddDays(-1).Date && _.IdTienda == idtienda).ToList();

            foreach (var t in turnos)
            {
                await CloseTurno(t);
            }
        }

        public async Task<Turno> GetTurno(int idTurno)
        {
            var query = await _repository.Query();
            return query.SingleOrDefault(_ => _.IdTurno == idTurno);
        }

        public async Task CerrarTurno(Turno turno)
        {
            var turnoCerrado = await Edit(turno);
            await _emailService.NotificarCierreCaja(turno.IdTurno);
        }
    }
}
