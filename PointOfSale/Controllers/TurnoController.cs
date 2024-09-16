using AutoMapper;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Scaffolding;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;
using PointOfSale.Business.Utilities;
using NuGet.Protocol;
using PointOfSale.Model.Auditoria;

namespace PointOfSale.Controllers
{
    public class TurnoController : BaseController
    {
        private readonly ITurnoService _turnoService;
        private readonly IMapper _mapper;
        private readonly IDashBoardService _dashBoardService;
        private readonly IMovimientoCajaService _movimientoCajaService;
        private readonly ILogger<TurnoController> _logger;

        public TurnoController(ITurnoService turnoService, IMapper mapper, IDashBoardService dashBoardService, ILogger<TurnoController> logger, IMovimientoCajaService movimientoCajaService)
        {
            _turnoService = turnoService;
            _mapper = mapper;
            _dashBoardService = dashBoardService;
            _logger = logger;
            _movimientoCajaService = movimientoCajaService;
        }

        public IActionResult Turno()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Empleado, Roles.Encargado });
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnos()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);
            List<VMTurno> VMTurnoList = _mapper.Map<List<VMTurno>>(await _turnoService.List(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = VMTurnoList });
        }


        [HttpGet]
        public async Task<IActionResult> CheckTurnoAbierto()
        {
            var gResponse = new GenericResponse<bool>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var turno = await _turnoService.GetTurnoActual(user.IdTienda);

                gResponse.State = true;
                gResponse.Object = turno != null;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al chequear turno abierto.", _logger, null);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTurnoActual()
        {
            var gResponse = new GenericResponse<VMTurnoOutput>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var turno = await _turnoService.GetTurnoActualConVentas(user.IdTienda);
                var outout = new VMTurnoOutput();

                if (turno != null)
                {
                    VMTurno vmTurnp = null;
                    vmTurnp = _mapper.Map<VMTurno>(turno);
                    var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                    var dateActual = TimeHelper.GetArgentinaTime();

                    var ventas = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, user.IdTienda, dateActual, false);
                    foreach (KeyValuePair<string, decimal> item in ventas.OrderBy(_ => _.Key))
                    {
                        VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                        {
                            Descripcion = item.Key,
                            Total = item.Value
                        });
                    }
                    vmTurnp.VentasPorTipoVenta = VentasPorTipoVenta;

                    var movimientos = await _movimientoCajaService.GetMovimientoCajaByTurno(user.IdTurno);

                    decimal totalMovimiento = 0;
                    foreach (var m in movimientos)
                    {
                        if (m.RazonMovimientoCaja.Tipo == TipoMovimientoCaja.Egreso)
                            totalMovimiento -= m.Importe;
                        else
                            totalMovimiento += m.Importe;
                    }

                    outout.Turno = vmTurnp;
                    outout.TotalMovimientosCaja = totalMovimiento;

                    gResponse.Object = outout;
                }
                else
                    gResponse.Object = outout;

                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar turno actual.", _logger, null);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTurno(int idturno)
        {
            var gResponse = new GenericResponse<VMTurnoOutput>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Empleado]);

                var outout = new VMTurnoOutput();
                var vmTurnp = _mapper.Map<VMTurno>(await _turnoService.GetTurno(idturno));

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                var datos = await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, user.IdTienda, vmTurnp.FechaInicio.Value, false);
                foreach (KeyValuePair<string, decimal> item in datos)
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }

                vmTurnp.VentasPorTipoVenta = VentasPorTipoVenta;

                var movimientos = await _movimientoCajaService.GetMovimientoCajaByTurno(idturno);

                decimal totalMovimiento = 0;
                foreach (var m in movimientos)
                {
                    if (m.RazonMovimientoCaja.Tipo == TipoMovimientoCaja.Egreso)
                        totalMovimiento -= m.Importe;
                    else
                        totalMovimiento += m.Importe;
                }

                outout.Turno = vmTurnp;
                outout.TotalMovimientosCaja = totalMovimiento;
                gResponse.Object = outout;
                gResponse.State = true;

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar turno.", _logger, idturno);
            }

        }

        [HttpPost]
        public async Task<IActionResult> AbrirTurno([FromBody] VMSaveTurno modelTurno)
        {
            var gResponse = new GenericResponse<VMTurno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
                var model = new VMTurno()
                {
                    RegistrationUser = user.UserName,
                    RegistrationDate = TimeHelper.GetArgentinaTime(),
                    ObservacionesApertura = modelTurno.ObservacionesApertura,
                    TotalInicioCaja = modelTurno.TotalInicioCaja,
                    FechaInicio = TimeHelper.GetArgentinaTime(),
                    IdTienda = user.IdTienda
                };


                var nuevoTurno = await _turnoService.Add(_mapper.Map<Turno>(model));

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMTurno>(nuevoTurno);

                UpdateClaimAsync("Turno", nuevoTurno.IdTurno.ToString());

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cerrar turno.", _logger, modelTurno);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CerrarTurno([FromBody] VMTurno modelTurno)
        {
            var gResponse = new GenericResponse<string>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                modelTurno.ObservacionesCierre = modelTurno.ObservacionesCierre;
                modelTurno.ModificationUser = user.UserName;
                modelTurno.IdTienda = user.IdTienda;
                modelTurno.FechaFin = TimeHelper.GetArgentinaTime();

                var errores = await _turnoService.CerrarTurno(_mapper.Map<Turno>(modelTurno), _mapper.Map<List<VentasPorTipoDeVenta>>(modelTurno.VentasPorTipoVenta));

                if (errores == string.Empty)
                {
                    UpdateClaimAsync("Turno", null);
                }

                gResponse.State = true;
                gResponse.Object = errores;


                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al cerrar turno.", _logger, modelTurno);
            }
        }


        [HttpPut]
        public async Task<IActionResult> UpdateTurno([FromBody] VMTurno VMTurno)
        {
            GenericResponse<VMTurno> gResponse = new GenericResponse<VMTurno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                try
                {
                    VMTurno.ModificationUser = user.UserName;
                    Turno edited_Tienda = await _turnoService.Edit(_mapper.Map<Turno>(VMTurno));

                    VMTurno = _mapper.Map<VMTurno>(edited_Tienda);

                    gResponse.State = true;
                    gResponse.Object = VMTurno;
                }
                catch (Exception ex)
                {
                    gResponse.State = false;
                    gResponse.Message = ex.ToString();
                }

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar turno actual.", _logger, VMTurno);
            }

        }
    }
}
