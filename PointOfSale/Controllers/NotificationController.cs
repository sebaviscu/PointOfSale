using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(INotificationService notificationService, IMapper mapper, ILogger<NotificationController> logger)
        {
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
        }

        public IActionResult Notification()
        {
            ValidarAutorizacion([Roles.Administrador]);
            return ValidateSesionViewOrLogin();
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificaciones()
        {
            List<VMNotifications> vmNotificationsList = _mapper.Map<List<VMNotifications>>(await _notificationService.List());
            return StatusCode(StatusCodes.Status200OK, new { data = vmNotificationsList });
        }

        [HttpGet]
        public async Task<IActionResult> GetNotificacionesActivas()
        {
            List<VMNotifications> vmNotificationsList = _mapper.Map<List<VMNotifications>>(await _notificationService.GetActive());
            return StatusCode(StatusCodes.Status200OK, new { data = vmNotificationsList });
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotifications([FromBody] VMNotifications model)
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                Notifications Notifications_created = await _notificationService.Save(_mapper.Map<Notifications>(model));

                model = _mapper.Map<VMNotifications>(Notifications_created);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al crear notificacion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, model.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotificacion(int idNotificacion)
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                Notifications edited_Notifications = await _notificationService.Edit(idNotificacion, user.UserName);

                var model = _mapper.Map<VMNotifications>(edited_Notifications);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al actualizar notificacion";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage} Request: {RequestModel}.", errorMessage, idNotificacion.ToJson());
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPut]
        public async Task<IActionResult> LimpiarTodoNotificacion()
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador]);
                var result = await _notificationService.LimpiarTodo(user.UserName);

                gResponse.State = true;
                gResponse.State = result;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al limpiar todas las  notificaciones";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.Message}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }

        }
    }
}
