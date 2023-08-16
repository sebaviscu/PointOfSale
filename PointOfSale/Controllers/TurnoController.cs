using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

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

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetTurnoActual()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
            var tiendaId = Convert.ToInt32(((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value);

            var vmTurnp = _mapper.Map<VMTurno>(await _turnoService.GetTurnoActualConVentas(tiendaId));

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

        [HttpPost]
        public async Task<IActionResult> CerrarTurno()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });

            var model = _mapper.Map<VMTurno>(await _turnoService.GetTurnoActual(user.IdTienda));

            GenericResponse<VMTurno> gResponse = new GenericResponse<VMTurno>();
            try
            {
                model.ModificationUser = user.UserName;
                var tiendaId = ((ClaimsIdentity)HttpContext.User.Identity).FindFirst("Tienda").Value;

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

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }
    }
}
