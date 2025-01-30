using AngleSharp.Dom;
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
        private readonly IGenericRepository<VentasPorTipoDeVentaTurno> _repositoryVentasPorTipoDeVentaTurno;
        private readonly IEmailService _emailService;
        private readonly IAjusteService _ajusteService;
        private readonly IDashBoardService _dashBoardService;

        public TurnoService(IGenericRepository<Turno> repository, IEmailService emailService, IAjusteService ajusteService, IDashBoardService dashBoardService, IGenericRepository<VentasPorTipoDeVentaTurno> repositoryVentasPorTipoDeVentaTurno)
        {
            _repository = repository;
            _emailService = emailService;
            _ajusteService = ajusteService;
            _dashBoardService = dashBoardService;
            _repositoryVentasPorTipoDeVentaTurno = repositoryVentasPorTipoDeVentaTurno;
        }

        public async Task<List<Turno>> List(int idtienda)
        {
            var result = await _repository.Query(_ => _.IdTienda == idtienda);
            IQueryable<Turno> list = QueryTurnoisConVentasSinAnular(result);

            return list.ToList();
        }

        private static IQueryable<Turno> QueryTurnoisConVentasSinAnular(IQueryable<Turno> result)
        {
            var query = result.Include(_ => _.Sales).Include(_ => _.MovimientosCaja).OrderByDescending(_ => _.IdTurno);
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
                             Sales = t.Sales.Where(s => !s.IsDelete).ToList(),
                             MovimientosCaja = t.MovimientosCaja
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
            return query.Include(_ => _.VentasPorTipoDeVenta)
                        .Include(_ => _.MovimientosCaja)
                            .ThenInclude(_=>_.RazonMovimientoCaja)
                        .SingleOrDefault(_ => _.IdTurno == idTurno);
        }

        public async Task<(Turno TurnoCerrado, Dictionary<string, decimal> VentasRegistradas)> CerrarTurno(Turno entity)
        {
            var turno = await _repository.Get(_ => _.IdTurno == entity.IdTurno);
            var ventasRegistradas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, turno.IdTurno, turno.IdTienda, TimeHelper.GetArgentinaTime());

            turno.ModificationUser = entity.ModificationUser;
            turno.ObservacionesCierre = entity.ObservacionesCierre;
            turno.FechaFin = TimeHelper.GetArgentinaTime();
            turno.BilletesEfectivo = entity.BilletesEfectivo;

            var turnoCerrado = await Edit(turno);

            await NotificarTurnoCerrado(turnoCerrado, ventasRegistradas);

            return (turnoCerrado, ventasRegistradas);
        }

        public async Task<(Turno TurnoCerrado, Dictionary<string, decimal> VentasRegistradas)> CerrarTurnoSimple(Turno entity, List<VentasPorTipoDeVentaTurno> ventasPorTipoDeVentas)
        {
            var turno = await _repository.First(_ => _.IdTurno == entity.IdTurno);

            turno.ModificationUser = entity.ModificationUser;
            turno.ObservacionesCierre = entity.ObservacionesCierre;
            turno.FechaFin = TimeHelper.GetArgentinaTime();
            turno.VentasPorTipoDeVenta = ventasPorTipoDeVentas;
            turno.ValidacionRealizada = true;
            turno.TotalCierreCajaSistema = entity.TotalCierreCajaSistema;
            turno.ErroresCierreCaja = $"Diferencia de caja de <strong>'${entity.TotalCierreCajaReal - entity.TotalCierreCajaSistema}'</strong>. <br>";
            bool response = await _repository.Edit(turno);

            var diccionarioVentas = ventasPorTipoDeVentas.ToDictionary(_ => _.Descripcion, _ => _.TotalUsuario.Value);

            await NotificarTurnoCerrado(turno, diccionarioVentas);

            return (turno, diccionarioVentas);
        }

        private async Task NotificarTurnoCerrado(Turno turnoCerrado, Dictionary<string, decimal> ventasRegistradas)
        {
            var ajustes = await _ajusteService.GetAjustes(turnoCerrado.IdTienda);

            if (ajustes.NotificarEmailCierreTurno.HasValue && ajustes.NotificarEmailCierreTurno.Value
                        && !string.IsNullOrEmpty(ajustes.EmailEmisorCierreTurno)
                        && !string.IsNullOrEmpty(ajustes.PasswordEmailEmisorCierreTurno)
                        && !string.IsNullOrEmpty(ajustes.EmailsReceptoresCierreTurno))
            {
                await _emailService.NotificarCierreCaja(turnoCerrado, ventasRegistradas, ajustes);
            }
        }

        public async Task<Turno> ValidarCierreTurno(Turno entity, List<VentasPorTipoDeVentaTurno> ventasPorTipoDeVentasReales)
        {
            var listaVentas = new List<VentasPorTipoDeVentaTurno>();
            var totalVentaUsuario = 0m;
            var totalsistema = 0m;
            var diferenciaTotales = 0m;
            var respError = string.Empty;

            var ventasRegistradasSistema = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, entity.IdTurno, entity.IdTienda, TimeHelper.GetArgentinaTime());

            var turno = await GetTurno(entity.IdTurno);

            var totalMovimiento = turno.MovimientosCaja != null && turno.MovimientosCaja.Any() ? turno.MovimientosCaja.Sum(_ => _.Importe) : 0m;
            if (!ventasRegistradasSistema.Any(_ => _.Key == "Efectivo") && (totalMovimiento > 0 || turno.TotalInicioCaja > 0))
            {
                ventasRegistradasSistema.Add("Efectivo", 0);
            }

            foreach (KeyValuePair<string, decimal> itemSistema in ventasRegistradasSistema.OrderBy(_ => _.Key))
            {
                var ventaUsuario = ventasPorTipoDeVentasReales.FirstOrDefault(_ => _.Descripcion == itemSistema.Key);

                var totalSistema = (int)itemSistema.Value;
                if (itemSistema.Key == "Efectivo" && (totalMovimiento != 0 || turno.TotalInicioCaja > 0))
                {
                    totalSistema += (int)totalMovimiento;
                    totalSistema += (int)turno.TotalInicioCaja;
                }

                if (ventaUsuario != null)
                {
                    var diferencia = (int)ventaUsuario.TotalUsuario - totalSistema;
                    totalVentaUsuario += ventaUsuario.TotalUsuario.Value;
                    totalsistema += totalSistema;
                    if (diferencia != 0)
                    {
                        diferenciaTotales += diferencia;
                        respError += $"- Existe diferencia en <strong>'{ventaUsuario.Descripcion.ToUpper()}'</strong> de <strong>$ {(int)diferencia}</strong>. <br>";
                    }

                    ventaUsuario.Error = respError;
                    ventaUsuario.IdTurno = entity.IdTurno;
                    ventaUsuario.TotalSistema = totalSistema;
                    listaVentas.Add(ventaUsuario);
                }
            }

            if (diferenciaTotales != 0)
            {
                respError += $"<br> La diferencia total es de $ {diferenciaTotales}. <br>";
            }

            if (!turno.ValidacionRealizada.HasValue || (turno.ValidacionRealizada.HasValue && !turno.ValidacionRealizada.Value))
            {
                if (listaVentas.Any())
                    await _repositoryVentasPorTipoDeVentaTurno.AddRange(listaVentas);

                turno.ErroresCierreCaja = respError;
                turno.TotalCierreCajaReal = totalVentaUsuario;
                turno.TotalCierreCajaSistema = totalsistema;
                turno.ValidacionRealizada = true;
                turno.BilletesEfectivo = entity.BilletesEfectivo ?? string.Empty;
                await _repository.Edit(turno);
            }

            return turno;
        }


        public async Task<Turno?> GetTurnoConVentasPorTipoDeVentaTurno(int idTurno)
        {
            var query = await _repository.Query(_ => _.IdTurno == idTurno);

            return query.Include(_ => _.VentasPorTipoDeVenta).Include(_ => _.MovimientosCaja).FirstOrDefault();
        }

    }
}
