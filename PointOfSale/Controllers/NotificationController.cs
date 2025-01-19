using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Business.Utilities;
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
        private readonly IUserService _userService;

        public NotificationController(INotificationService notificationService, IMapper mapper, ILogger<NotificationController> logger, IUserService userService)
        {
            _notificationService = notificationService;
            _mapper = mapper;
            _logger = logger;
            _userService = userService;
        }

        public IActionResult Notification()
        {
            return ValidateSesionViewOrLogin([Roles.Administrador]);
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

        [HttpGet]
        public async Task<IActionResult> GetNotificacionesByUser()
        {
            var gResponse = new GenericResponse<List<VMNotifications>>();
            var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);

            try
            {
                var vmNotificationsList = _mapper.Map<List<VMNotifications>>(await _notificationService.GetByUserByActive(user.IdUsuario));

                gResponse.State = true;
                gResponse.Object = vmNotificationsList;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al recuperar lista de notificaciones por usuario.", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateNotifications([FromBody] VMNotifications model)
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

            try
            {
                if (model.IdRol == -2 || model.IdUser == -2)
                {
                    List<User> users = null;
                    if (model.IdRol == -2)
                    {
                        users = await _userService.GetAllUsersByTienda(user.IdTienda);
                    }
                    else if (model.IdUser == -2)
                    {
                        users = await _userService.GetUsersByRolByTiendaForNotifications(model.IdRol.Value, user.IdTienda);
                    }

                    List<Notifications> notifications = new List<Notifications>();
                    foreach (var u in users)
                    {
                        var n = new Notifications(user.UserName, u.IdUsers, u.Name, model.Descripcion);
                        notifications.Add(n);
                    }

                    var notifList = await _notificationService.SaveRange(notifications);

                    gResponse.ListObject = _mapper.Map<List<VMNotifications>>(notifList);
                }
                else
                {
                    var norificacion = new Notifications(user.UserName, model.IdUser.Value, model.UserNameString, model.Descripcion);

                    var Notifications_created = await _notificationService.Save(norificacion);

                    model = _mapper.Map<VMNotifications>(Notifications_created);
                    gResponse.Object = model;

                }

                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al crear notificacion.", _logger, model);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotificacion(int idNotificacion)
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado]);

                Notifications edited_Notifications = await _notificationService.RecibirNotificacion(idNotificacion, user.UserName);

                var model = _mapper.Map<VMNotifications>(edited_Notifications);

                gResponse.State = true;
                gResponse.Object = model;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar notificacion.", _logger, idNotificacion);
            }
        }

        [HttpPut]
        public async Task<IActionResult> EditNotificacion([FromBody] VMNotifications vmNotif)
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                if (string.IsNullOrEmpty(vmNotif.Descripcion))
                {
                    throw new Exception("El contenido de la notificacion, no puede estar vacio.");
                }

                var user = ValidarAutorizacion([Roles.Administrador]);

                var edited_Notifications = await _notificationService.Edit(_mapper.Map<Notifications>(vmNotif));

                vmNotif = _mapper.Map<VMNotifications>(edited_Notifications);

                gResponse.State = true;
                gResponse.Object = vmNotif;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al actualizar notificacion.", _logger, vmNotif);
            }
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(int idNotificacion)
        {

            GenericResponse<string> gResponse = new GenericResponse<string>();
            try
            {
                ValidarAutorizacion([Roles.Administrador]);
                gResponse.State = await _notificationService.Delete(idNotificacion);
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al eliminar notificacion.", _logger, idNotificacion);
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
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al  limpiar todas las  notificaciones.", _logger, null);
            }

        }

        [HttpPut]
        public async Task<IActionResult> LimpiarTodoNotificacionIndividuales()
        {
            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();

            try
            {
                var user = ValidarAutorizacion([Roles.Administrador, Roles.Encargado, Roles.Empleado]);
                var result = await _notificationService.LimpiarIndividuales(user.UserName, user.IdUsuario);

                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                return HandleException(ex, "Error al  limpiar todas las  notificaciones.", _logger, null);
            }

        }
    }
}
