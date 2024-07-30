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

namespace PointOfSale.Controllers
{
    public class TurnoController : BaseController
    {
        private readonly ITurnoService _turnoService;
        private readonly IMapper _mapper;
        private readonly IDashBoardService _dashBoardService;
        private readonly ILogger<TurnoController> _logger;

        public TurnoController(ITurnoService turnoService, IMapper mapper, IDashBoardService dashBoardService, ILogger<TurnoController> logger)
        {
            _turnoService = turnoService;
            _mapper = mapper;
            _dashBoardService = dashBoardService;
            _logger = logger;
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
        public async Task<IActionResult> GetTurnoActual()
        {
            var gResponse = new GenericResponse<VMTurno>();
            try
            {

                var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

                var vmTurnp = _mapper.Map<VMTurno>(await _turnoService.GetTurnoActualConVentas(tiendaId));

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                var dateActual = TimeHelper.GetArgentinaTime();
                foreach (KeyValuePair<string, decimal> item in await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, tiendaId, dateActual))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }
                vmTurnp.VentasPorTipoVenta = VentasPorTipoVenta;

                gResponse.State = true;
                gResponse.Object = vmTurnp;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar turno actual";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpGet]
        public async Task<IActionResult> GetTurno(int idturno)
        {
            var gResponse = new GenericResponse<VMTurno>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

                var vmTurnp = _mapper.Map<VMTurno>(await _turnoService.GetTurno(idturno));

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                var dateActual = TimeHelper.GetArgentinaTime();
                foreach (KeyValuePair<string, decimal> item in await _dashBoardService.GetSalesByTypoVentaByTurnoByDate(TypeValuesDashboard.Dia, vmTurnp.IdTurno, tiendaId, dateActual))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }
                vmTurnp.VentasPorTipoVenta = VentasPorTipoVenta;

                return StatusCode(StatusCodes.Status200OK, new { data = vmTurnp });
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar turno";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, idturno.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }

        [HttpPost]
        public async Task<IActionResult> CerrarTurno([FromBody] VMSaveTurno modelTurno)
        {
            var gResponse = new GenericResponse<VMTurno>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

                var model = _mapper.Map<VMTurno>(await _turnoService.GetTurnoActual(user.IdTienda));


                model.ModificationUser = user.UserName;
                var tiendaId = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value;
                model.Descripcion = modelTurno.Descripcion;
                var turno_cerrar = await _turnoService.CloseTurno(user.IdTienda, _mapper.Map<Turno>(model));

                var nuevoTurno = await _turnoService.AbrirTurno(Convert.ToInt32(tiendaId), user.UserName);

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMTurno>(nuevoTurno);

                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al cerrar turno";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, modelTurno.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetOneTurno(int idTurno)
        {
            var gResponse = new GenericResponse<VMTurno>();

            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

                var vmTurnp = _mapper.Map<VMTurno>(await _turnoService.GetTurno(idTurno));

                var VentasPorTipoVenta = new List<VMVentasPorTipoDeVenta>();
                foreach (KeyValuePair<string, decimal> item in await _dashBoardService.GetSalesByTypoVentaByTurno(TypeValuesDashboard.Dia, vmTurnp.IdTurno, tiendaId))
                {
                    VentasPorTipoVenta.Add(new VMVentasPorTipoDeVenta()
                    {
                        Descripcion = item.Key,
                        Total = item.Value
                    });
                }
                vmTurnp.VentasPorTipoVenta = VentasPorTipoVenta;

                gResponse.Object = vmTurnp;
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar un turno";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, idTurno.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
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
                    gResponse.Message = ex.Message;
                }

                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar turno actual";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, VMTurno.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }
    }
}
