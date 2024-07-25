using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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

        public NotificationController(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public IActionResult Notification()
        {
            ValidarAutorizacion(new Roles[] { Roles.Administrador });
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
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }

            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateNotificacion(int idNotificacion)
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                Notifications edited_Notifications = await _notificationService.Edit(idNotificacion, user.UserName);

                var model = _mapper.Map<VMNotifications>(edited_Notifications);

                gResponse.State = true;
                gResponse.Object = model;
            }
            catch (Exception ex)
            {
                gResponse.State = false;
                gResponse.Message = ex.Message;
            }
            return StatusCode(StatusCodes.Status200OK, gResponse);
        }

        [HttpPut]
        public async Task<IActionResult> LimpiarTodoNotificacion()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador });

            GenericResponse<VMNotifications> gResponse = new GenericResponse<VMNotifications>();
            try
            {
                var result = await _notificationService.LimpiarTodo(user.UserName);


                gResponse.State = result;
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
