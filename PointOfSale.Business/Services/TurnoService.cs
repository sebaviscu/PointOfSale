using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class TurnoService : ITurnoService
    {
        private readonly IGenericRepository<Turno> _repository;
        private readonly IEmailService _emailService;
        private readonly IAjusteService _ajusteService;
        private readonly IDashBoardService _dashBoardService;

        public TurnoService(IGenericRepository<Turno> repository, IEmailService emailService, IAjusteService ajusteService, IDashBoardService dashBoardService)
        {
            _repository = repository;
            _emailService = emailService;
            _ajusteService = ajusteService;
            _dashBoardService = dashBoardService;
        }

        public async Task<List<Turno>> List(int idtienda)
        {
            var result = await _repository.Query(_ => _.IdTienda == idtienda);
            IQueryable<Turno> list = QueryTurnoisConVentasSinAnular(result);

            return list.ToList();
        }

        private static IQueryable<Turno> QueryTurnoisConVentasSinAnular(IQueryable<Turno> result)
        {
            var query = result.Include(_ => _.Sales).OrderByDescending(_ => _.IdTurno);
            var list = query
                         .Select(t => new Turno
                         {
                             IdTurno = t.IdTurno,
                             FechaFin = t.FechaFin,
                             FechaInicio = t.FechaInicio,
                             IdTienda = t.IdTienda,
                             ModificationUser = t.ModificationUser,
                             ObservacionesApertura = t.ObservacionesApertura,
                             ObservacionesCierre = t.ObservacionesCierre,
                             RegistrationDate = t.RegistrationDate,
                             RegistrationUser = t.RegistrationUser,
                             TotalInicioCaja = t.TotalInicioCaja,
                             Sales = t.Sales.Where(s => !s.IsDelete).ToList()
                         });
            return list;
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
            Turno Turno_found = await _repository.Get(c => c.IdTurno == entity.IdTurno);

            Turno_found.ModificationUser = entity.ModificationUser;
            Turno_found.ObservacionesCierre = entity.ObservacionesCierre;
            Turno_found.FechaFin = entity.FechaFin;

            bool response = await _repository.Edit(Turno_found);

            if (!response)
                throw new TaskCanceledException("Turno no se pudo cambiar.");

            return Turno_found;
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
            var query2 = QueryTurnoisConVentasSinAnular(query);

            var turno = query2.FirstOrDefault(_ => _.FechaFin == null
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

        public async Task<string> CerrarTurno(Turno entity, List<VentasPorTipoDeVenta> ventasPorTipoDeVentas)
        {

            var Turno_found = await _repository.Get(c => c.IdTurno == entity.IdTurno);

            var respError = string.Empty;

            var ventasRegistradas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, entity.IdTurno, entity.IdTienda, TimeHelper.GetArgentinaTime(), false);
            foreach (KeyValuePair<string, decimal> item in ventasRegistradas)
            {

                var venta = ventasPorTipoDeVentas.FirstOrDefault(_ => _.Descripcion == item.Key);

                if (venta != null)
                {
                    var coraInferior = item.Value * 0.95m;
                    var coraSuperior = item.Value * 1.05m;
                    if (coraInferior > venta.Total || venta.Total > coraSuperior)
                    {
                        respError += $"Existen diferencias en el tipo de ventas  '{venta.Descripcion}'. \n";
                    }
                }
            }

            if (respError == string.Empty)
            {
                var turnoCerrado = await Edit(entity);
                var ajustes = await _ajusteService.GetAjustes(entity.IdTienda);

                if (ajustes.NotificarEmailCierreTurno.HasValue && ajustes.NotificarEmailCierreTurno.Value
                            && !string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno) && !string.IsNullOrEmpty(ajustes.EmailsReceptoresCierreTurno))
                {

                    await _emailService.NotificarCierreCaja(turnoCerrado, ventasRegistradas, ajustes);
                }
            }

            return respError;
        }
    }
}
