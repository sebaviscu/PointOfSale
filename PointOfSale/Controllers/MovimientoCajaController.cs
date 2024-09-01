using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    [Authorize]
    public class MovimientoCajaController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ILogger<MovimientoCajaController> _logger;
        private readonly IMovimientoCajaService _movimientoCajaService;
        private readonly INotificationService _notificationService;

        public MovimientoCajaController(IMapper mapper, ILogger<MovimientoCajaController> logger, IMovimientoCajaService movimientoCajaService, INotificationService notificationService)
        {
            _mapper = mapper;
            _logger = logger;
            _movimientoCajaService = movimientoCajaService;
            _notificationService = notificationService;
        }
        public IActionResult Index()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        /// <summary>
        /// Recuperar datos para DataTable
        /// </summary>
        /// <param name="visionGlobal"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetMovimientosCaja()
        {
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);

                var vmList = _mapper.Map<List<VMMovimientoCaja>>(await _movimientoCajaService.List(user.IdTienda));
                return StatusCode(StatusCodes.Status200OK, new { data = vmList });
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de Movimientos de Caja", _logger);
            }
        }

        /// <summary>
        /// Recupera RazonMovimientoCaja para Selects
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> GetRazonMovimientoCaja()
        {
            var gResponse = new GenericResponse<List<VMRazonMovimientoCaja>>();

            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);

                gResponse.State = true;
                gResponse.Object = _mapper.Map<List<VMRazonMovimientoCaja>>(await _movimientoCajaService.GetRazonMovimientoCaja());
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar Razones Movimientos Caja.", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRazonMovimientoCaja([FromBody] VMRazonMovimientoCaja model)
        {

            GenericResponse<VMRazonMovimientoCaja> gResponse = new GenericResponse<VMRazonMovimientoCaja>();
            try
            {
                ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
                var created = await _movimientoCajaService.Add(_mapper.Map<RazonMovimientoCaja>(model));

                model = _mapper.Map<VMRazonMovimientoCaja>(created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Razon Movimiento Caja.", _logger, model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> CambiarEstadoRazonMovimientoCaja(int idRazonMovimientoCaja)
        {

            GenericResponse<VMRazonMovimientoCaja> gResponse = new GenericResponse<VMRazonMovimientoCaja>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                var resp = await _movimientoCajaService.UpdateEstado(idRazonMovimientoCaja);

                gResponse.State = resp;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Razon Movimiento Caja.", _logger, idRazonMovimientoCaja);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateMovimientoCaja([FromBody] VMMovimientoCaja model)
        {

            var gResponse = new GenericResponse<VMMovimientoCaja>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Empleado, Roles.Encargado]);
                if(user.IdTurno == 0)
                {
                    throw new Exception("Debe haber un turno habierto.");
                }

                model.RegistrationUser = user.UserName;
                model.RegistrationDate = TimeHelper.GetArgentinaTime();
                model.IdTienda = user.IdTienda;
                model.IdTurno = user.IdTurno;

                var created = await _movimientoCajaService.Add(_mapper.Map<MovimientoCaja>(model));
                
                var notif = new Notifications(created);
                await _notificationService.Save(notif);

                model = _mapper.Map<VMMovimientoCaja>(created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear Movimiento Caja.", _logger, model);
            }
        }
    }
}
