using Microsoft.EntityFrameworkCore;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Data.Repository;
using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Business.Services
{
    public class DashBoardService : IDashBoardService
    {
        private readonly ISaleRepository _repositorySale;
        private readonly IGenericRepository<DetailSale> _repositoryDetailSale;
        private readonly IGenericRepository<ProveedorMovimiento> _proveedorMovimiento;
        private readonly IGenericRepository<Gastos> _gastosRepository;

        public DashBoardService(
            ISaleRepository repositorySale,
            IGenericRepository<DetailSale> repositoryDetailSale,
            IGenericRepository<ProveedorMovimiento> proveedorMovimiento,
            IGenericRepository<Gastos> gastosRepository
            )
        {

            _repositorySale = repositorySale;
            _repositoryDetailSale = repositoryDetailSale;
            _proveedorMovimiento = proveedorMovimiento;
            _gastosRepository = gastosRepository;
        }

        public async Task<GraficoVentasConComparacion> GetSales(TypeValuesDashboard typeValues, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            var resultados = new GraficoVentasConComparacion();
            var dateCompare = TimeHelper.GetArgentinaTime();

            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    dateCompare = new DateTime(dateStart.Year, dateStart.Month, dateStart.Day, 0, 0, 0);
                    var resp = await GetSalesHour(dateStart, dateCompare, idTienda, visionGlobal);
                    resultados.VentasActualesHour = resp.Ventas;
                    resultados.CantidadClientes = resp.CantidadVentas;
                    resultados.VentasComparacionHour = await GetComparationHour(dateStart, dateCompare.AddDays(-1), idTienda, visionGlobal);
                    break;

                case TypeValuesDashboard.Semana:
                    var numberWeek = (int)dateStart.Date.DayOfWeek == 0 ? 7 : (int)dateStart.Date.DayOfWeek;
                    dateStart = dateStart.AddDays(-numberWeek + 1);
                    dateCompare = dateStart.AddDays(-7);
                    var resp2 = await GetSalesActuales(dateStart, dateStart.AddDays(7), idTienda, visionGlobal);
                    resultados.VentasActuales = resp2.Ventas;
                    resultados.CantidadClientes = resp2.CantidadVentas;
                    resultados.VentasComparacion = await GetComparation(dateStart, dateCompare, idTienda, visionGlobal);

                    break;

                case TypeValuesDashboard.Mes:
                    dateStart = dateStart.AddDays((-dateStart.Date.Day) + 1);
                    dateCompare = dateStart.AddMonths(-1);
                    var resp3 = await GetSalesActuales(dateStart, dateStart.AddMonths(1), idTienda, visionGlobal);
                    resultados.VentasActuales = resp3.Ventas;
                    resultados.CantidadClientes = resp3.CantidadVentas;
                    resultados.VentasComparacion = await GetComparation(dateStart, dateCompare, idTienda, visionGlobal);

                    break;
            }


            return resultados;
        }

        public async Task<Dictionary<string, decimal>> GetMovimientosProveedores(TypeValuesDashboard typeValues, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _proveedorMovimiento.Query();

            query = query.Include(v => v.Proveedor)
                    .Where(_ => _.RegistrationDate.Date >= start.Date
                        && _.RegistrationDate.Date < end.Date
                        && _.EstadoPago.Value == EstadoPago.Pagado);

            if (!visionGlobal)
            {
                query = query.Where(v => v.idTienda == idTienda);
            }

            Dictionary<string, decimal> resultado = query

                        .GroupBy(v => v.Proveedor.Nombre).OrderByDescending(g => g.Sum(_ => _.Importe))
                        .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                        .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVenta(TypeValuesDashboard typeValues, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _repositorySale.Query();

            query = query.Include(v => v.TypeDocumentSaleNavigation)
                .Where(
                vd => vd.RegistrationDate.Value.Date >= start.Date && vd.RegistrationDate.Value.Date < end.Date
                && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                && !vd.IsDelete
                && vd.IdClienteMovimiento == null);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            Dictionary<string, decimal> resultado = query
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }


        private async Task<(Dictionary<DateTime, decimal> Ventas, int CantidadVentas)> GetSalesActuales(DateTime start, DateTime end, int idTienda, bool visionGlobal)
        {
            var query = await _repositorySale.Query();

            var queryVentasActuales = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date >= start.Date
                            && v.RegistrationDate.Value.Date < end.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && !v.IsDelete
                            && v.IdClienteMovimiento == null);

            if (!visionGlobal)
            {
                queryVentasActuales = queryVentasActuales.Where(v => v.IdTienda == idTienda);
            }

            var cantVentas = queryVentasActuales.Count();

            var resp = queryVentasActuales
                .GroupBy(v => v.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);

            return (resp, cantVentas);
        }

        private async Task<(Dictionary<int, decimal> Ventas, int CantidadVentas)> GetSalesHour(DateTime end, DateTime start, int idTienda, bool visionGlobal)
        {
            var resultados = new GraficoVentasConComparacion();

            var query = await _repositorySale.Query();

            var queryVentasActuales = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date >= start && v.RegistrationDate.Value.Date <= end
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && !v.IsDelete
                            && v.IdClienteMovimiento == null);

            if (!visionGlobal)
            {
                queryVentasActuales = queryVentasActuales.Where(v => v.IdTienda == idTienda);
            }

            var cantVentas = queryVentasActuales.Count();

            var resp = queryVentasActuales.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);

            return (resp, cantVentas);
        }

        private async Task<Dictionary<DateTime, decimal>> GetComparation(DateTime start, DateTime dateCompare, int idTienda, bool visionGlobal)
        {
            var query = await _repositorySale.Query();

            query = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date < start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.RegistrationDate.Value.Date >= dateCompare.Date
                            && !v.IsDelete
                            && v.IdClienteMovimiento == null);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            return query
                .GroupBy(_ => _.RegistrationDate.Value.Date).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        private async Task<Dictionary<int, decimal>> GetComparationHour(DateTime start, DateTime dateCompare, int idTienda, bool visionGlobal)
        {
            var query = await _repositorySale.Query();

            query = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(v =>
                            v.RegistrationDate.Value.Date < start.Date
                            && v.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                            && v.RegistrationDate.Value.Date >= dateCompare.Date
                            && !v.IsDelete
                            && v.IdClienteMovimiento == null);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            return query.ToList()
                .GroupBy(_ => _.RegistrationDate.Value.Hour).OrderByDescending(g => g.Key)
                .Select(dv => new { date = dv.Key, total = dv.Sum(v => v.Total.Value) })
                .OrderBy(_ => _.date)
                .ToDictionary(keySelector: r => r.date, elementSelector: r => r.total);
        }

        public async Task<Dictionary<string, string?>> ProductsTopByCategory(TypeValuesDashboard typeValues, string category, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            IQueryable<DetailSale> query = await _repositoryDetailSale.Query();

            if (category == "Todo")
            {
                query = query
                        .Include(v => v.IdSaleNavigation)
                        .Include(v => v.IdSaleNavigation.TypeDocumentSaleNavigation)
                        .Include(v => v.Producto)
                        .Where(dv =>
                                dv.IdSaleNavigation.RegistrationDate.Value.Date >= start.Date && dv.IdSaleNavigation.RegistrationDate.Value.Date < end.Date
                                && dv.IdSaleNavigation.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu);
            }
            else
            {
                query = query
                        .Include(v => v.IdSaleNavigation)
                        .Include(v => v.IdSaleNavigation.TypeDocumentSaleNavigation)
                        .Include(v => v.Producto)
                        .Where(dv =>
                                dv.IdSaleNavigation.RegistrationDate.Value.Date >= start.Date && dv.IdSaleNavigation.RegistrationDate.Value.Date < end.Date
                                && dv.IdSaleNavigation.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                                && dv.CategoryProducty == category);
            }

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdSaleNavigation.IdTienda == idTienda);
            }

            Dictionary<string, string?> resultado = query
                 .GroupBy(dv => dv.DescriptionProduct)
                 .OrderByDescending(g => g.Sum(_ => _.Quantity))
                 .Select(dv => new { product = dv.Key, total = dv.Sum(_ => _.Quantity) })
                 .Take(10)
                 .ToDictionary(
                     keySelector: r => r.product,
                     elementSelector: r => r.total % 1 == 0 ? Math.Truncate(r.total.Value).ToString() : Math.Round(r.total.Value, 1).ToString()
                 );

            return resultado;

        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard typeValues, int turno, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _repositorySale.Query();

            query = query.Include(v => v.TypeDocumentSaleNavigation)
                .Where(vd => vd.RegistrationDate.Value.Date >= start.Date && vd.RegistrationDate.Value.Date < end.Date
                        && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                        && vd.IdClienteMovimiento == null
                        && !vd.IsDelete
                        && vd.IdTurno == turno);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            Dictionary<string, decimal> resultado = query
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTurno(TypeValuesDashboard typeValues, int turno, int idTienda, bool visionGlobal)
        {
            IQueryable<Sale> query = await _repositorySale.Query();

            Dictionary<string, decimal> resultado = query
                .Include(v => v.TypeDocumentSaleNavigation)
                .Where(vd => vd.IdClienteMovimiento == null
                        && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                        && vd.IdTurno == turno
                        && !vd.IsDelete
                        && vd.IdTienda == idTienda)
                .GroupBy(v => v.TypeDocumentSaleNavigation.Description).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetGastos(TypeValuesDashboard typeValues, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _gastosRepository.Query();

            query = query.Include(v => v.TipoDeGasto)
                .Where(vd => vd.RegistrationDate.Date >= start.Date && vd.RegistrationDate.Date < end.Date
                        && vd.TipoDeGasto.GastoParticular != TipoDeGastoEnum.Sueldos
                        && vd.EstadoPago == EstadoPago.Pagado);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            Dictionary<string, decimal> resultado = query

                .GroupBy(v => v.TipoDeGasto.Descripcion).OrderByDescending(g => g.Sum(_ => _.Importe))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetGastosSueldos(TypeValuesDashboard typeValues, int idTienda, DateTime dateStart, bool visionGlobal)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _gastosRepository.Query();

            query = query.Include(v => v.TipoDeGasto)
                .Where(vd => vd.RegistrationDate.Date >= start.Date && vd.RegistrationDate.Date < end.Date
                        && vd.TipoDeGasto.GastoParticular == TipoDeGastoEnum.Sueldos
                        && vd.EstadoPago == EstadoPago.Pagado);

            if (!visionGlobal)
            {
                query = query.Where(v => v.IdTienda == idTienda);
            }

            Dictionary<string, decimal> resultado = query
                .GroupBy(v => v.TipoDeGasto.Descripcion).OrderByDescending(g => g.Sum(_ => _.Importe))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        private static void FechasParaQuery(TypeValuesDashboard typeValues, DateTime dateStart, out DateTime dateCompare, out DateTime start)
        {

            switch (typeValues)
            {
                case TypeValuesDashboard.Dia:
                    start = dateStart.Date;
                    dateCompare = dateStart.AddDays(1).Date;
                    break;

                case TypeValuesDashboard.Semana:
                    var numberWeek = (int)dateStart.Date.DayOfWeek == 0 ? 7 : (int)dateStart.Date.DayOfWeek;
                    start = dateStart.AddDays(-numberWeek + 1);
                    dateCompare = start.AddDays(7);

                    break;

                case TypeValuesDashboard.Mes:
                    start = dateStart.AddDays((-dateStart.Day) + 1);
                    dateCompare = start.AddMonths(1);

                    break;

                default:
                    dateCompare = TimeHelper.GetArgentinaTime();
                    start = TimeHelper.GetArgentinaTime();
                    break;
            }
        }

        public async Task<Dictionary<string, decimal>> GetSalesByTypoVentaByTienda(TypeValuesDashboard typeValues, DateTime dateStart)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _repositorySale.Query();

            query = query.Include(v => v.TypeDocumentSaleNavigation).Include(v => v.Tienda)
                .Where(
                vd => vd.RegistrationDate.Value.Date >= start.Date && vd.RegistrationDate.Value.Date < end.Date
                && vd.TypeDocumentSaleNavigation.TipoFactura != TipoFactura.Presu
                && !vd.IsDelete
                && vd.IdClienteMovimiento == null);

            Dictionary<string, decimal> resultado = query
                .GroupBy(v => v.Tienda.Nombre).OrderByDescending(g => g.Sum(_ => _.Total))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Total.Value) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

        public async Task<Dictionary<string, decimal>> GetMovimientosProveedoresByTienda(TypeValuesDashboard typeValues, DateTime dateStart)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _proveedorMovimiento.Query();

            query = query.Include(v => v.Proveedor).Include(v => v.Tienda)
                    .Where(_ => _.RegistrationDate.Date >= start.Date
                        && _.RegistrationDate.Date < end.Date
                        && _.EstadoPago.Value == EstadoPago.Pagado);


            Dictionary<string, decimal> resultado = query

                        .GroupBy(v => v.Tienda.Nombre).OrderByDescending(g => g.Sum(_ => _.Importe))
                        .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                        .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }


        public async Task<Dictionary<string, decimal>> GetGastosByTienda(TypeValuesDashboard typeValues, DateTime dateStart)
        {
            FechasParaQuery(typeValues, dateStart, out DateTime end, out DateTime start);

            var query = await _gastosRepository.Query();

            query = query.Include(v => v.TipoDeGasto).Include(v => v.Tienda)
                .Where(vd => vd.RegistrationDate.Date >= start.Date && vd.RegistrationDate.Date < end.Date
                        && vd.TipoDeGasto.GastoParticular != TipoDeGastoEnum.Sueldos
                        && vd.EstadoPago == EstadoPago.Pagado);

            Dictionary<string, decimal> resultado = query

                .GroupBy(v => v.Tienda.Nombre).OrderByDescending(g => g.Sum(_ => _.Importe))
                .Select(dv => new { descripcion = dv.Key, total = dv.Sum(_ => _.Importe) })
                .ToDictionary(keySelector: r => r.descripcion, elementSelector: r => r.total);

            return resultado;
        }

    }

    public class GraficoVentasConComparacion
    {
        public Dictionary<DateTime, decimal> VentasActuales { get; set; }
        public Dictionary<DateTime, decimal> VentasComparacion { get; set; }
        public Dictionary<int, decimal> VentasActualesHour { get; set; }
        public Dictionary<int, decimal> VentasComparacionHour { get; set; }
        public Dictionary<string, decimal> VentasPorTipoVenta { get; set; }

        public int CantidadClientes { get; set; }
    }
}
