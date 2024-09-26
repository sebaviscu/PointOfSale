using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using PointOfSale.Model.Auditoria;
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
                             ErroresCierreCaja = t.ErroresCierreCaja,
                             TotalCierreCajaReal = t.TotalCierreCajaReal,
                             TotalCierreCajaSistema = t.TotalCierreCajaSistema,
                             ValidacionRealizada = t.ValidacionRealizada,
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

        public async Task<(Turno TurnoCerrado, Dictionary<string, decimal> VentasRegistradas)> CerrarTurno(Turno entity, List<VentasPorTipoDeVenta> ventasPorTipoDeVentas)
        {
            var turno = await _repository.Get(_ => _.IdTurno == entity.IdTurno);
            var ventasRegistradas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, turno.IdTurno, turno.IdTienda, TimeHelper.GetArgentinaTime(), false);

            turno.ModificationUser = entity.ModificationUser;
            turno.ObservacionesCierre = entity.ObservacionesCierre;
            turno.FechaFin = TimeHelper.GetArgentinaTime();

            var turnoCerrado = await Edit(turno);
            var ajustes = await _ajusteService.GetAjustes(turno.IdTienda);

            if (ajustes.NotificarEmailCierreTurno.HasValue && ajustes.NotificarEmailCierreTurno.Value
                        && !string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno)
                        && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno)
                        && !string.IsNullOrEmpty(ajustes.EmailsReceptoresCierreTurno))
            {
                await _emailService.NotificarCierreCaja(turnoCerrado, ventasRegistradas, ajustes);
            }

            return (turnoCerrado, ventasRegistradas);
        }


        public async Task<Turno> ValidarCierreTurno(Turno entity, List<VentasPorTipoDeVenta> ventasPorTipoDeVentasReales)
        {
            var ventasRegistradasSistema = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, entity.IdTurno, entity.IdTienda, TimeHelper.GetArgentinaTime(), false);
            return await ValidarVentas(ventasPorTipoDeVentasReales, ventasRegistradasSistema, entity.IdTurno);
        }

        private async Task<Turno> ValidarVentas(List<VentasPorTipoDeVenta> ventasPorTipoDeVentasReales, Dictionary<string, decimal> ventasRegistradasSistema, int idTurno)
        {

            var respError = string.Empty;
            foreach (KeyValuePair<string, decimal> item in ventasRegistradasSistema)
            {
                var venta = ventasPorTipoDeVentasReales.FirstOrDefault(_ => _.Descripcion == item.Key);

                if (venta != null)
                { 
                    var diferencia = (int)item.Value - (int)venta.Total;

                    if (diferencia != 0)
                    {
                        respError += $"- Existe diferencia en <strong>'{venta.Descripcion.ToUpper()}'</strong> de <strong>$ {(int)Math.Abs(diferencia)}</strong>. <br>";
                    }
                }
            }

            var turno = await _repository.Get(_ => _.IdTurno == idTurno);
            if (!turno.ValidacionRealizada.HasValue || (turno.ValidacionRealizada.HasValue && !turno.ValidacionRealizada.Value))
            {
                turno.ErroresCierreCaja = respError;
                turno.TotalCierreCajaReal = ventasPorTipoDeVentasReales.Sum(_ => _.Total);
                turno.TotalCierreCajaSistema = ventasRegistradasSistema.Sum(_ => _.Value);
                turno.ValidacionRealizada = true;

                await _repository.Edit(turno);
            }

            return turno;
        }
    }
}
