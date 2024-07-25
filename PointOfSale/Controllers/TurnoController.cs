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

namespace PointOfSale.Controllers
{
    public class TurnoController : BaseController
    {
        private readonly ITurnoService _turnoService;
        private readonly IMapper _mapper;
        private readonly IDashBoardService _dashBoardService;
        public TurnoController(ITurnoService turnoService, IMapper mapper, IDashBoardService dashBoardService)
        {
            _turnoService = turnoService;
            _mapper = mapper;
            _dashBoardService = dashBoardService;
        }

        public IActionResult Turno()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Empleado, Roles.Encargado });
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnos()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });
            List<VMTurno> VMTurnoList = _mapper.Map<List<VMTurno>>(await _turnoService.List(user.IdTienda));
            return StatusCode(StatusCodes.Status200OK, new { data = VMTurnoList });
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnoActual()
        {
            //ValidarAutorizacion(new Roles[] { Roles.Administrador });
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

            return StatusCode(StatusCodes.Status200OK, new { data = vmTurnp });
        }

        [HttpGet]
        public async Task<IActionResult> GetTurno(int idturno)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
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

        [HttpPost]
        public async Task<IActionResult> CerrarTurno([FromBody] VMSaveTurno modelTurno)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            var model = _mapper.Map<VMTurno>(await _turnoService.GetTurnoActual(user.IdTienda));

            GenericResponse<VMTurno> gResponse = new GenericResponse<VMTurno>();
            try
            {
                model.ModificationUser = user.UserName;
                var tiendaId = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value;
                model.Descripcion = modelTurno.Descripcion;
                var turno_cerrar = await _turnoService.CloseTurno(user.IdTienda, _mapper.Map<Turno>(model));

                var nuevoTurno = await _turnoService.AbrirTurno(Convert.ToInt32(tiendaId), user.UserName);

                gResponse.State = true;
                gResponse.Object = _mapper.Map<VMTurno>(nuevoTurno);
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }


        [HttpGet]
        public async Task<IActionResult> GetOneTurno(int idTurno)
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
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

            return StatusCode(StatusCodes.Status200OK, new { data = vmTurnp });
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] VMTurno VMTurno)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMTurno> gResponse = new GenericResponse<VMTurno>();
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
    }
}
