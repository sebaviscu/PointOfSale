using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class NotificacionesComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;
        private readonly IMapper _mapper;
        public NotificacionesComponent(INotificationService notificationService, IMapper mapper)
        {
            _notificationService = notificationService;
            _mapper = mapper;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimuser = HttpContext.User;

            var userRol = Convert.ToInt32(claimuser.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value).SingleOrDefault());

            var notifications = _mapper.Map<List<VMNotifications>>(await _notificationService.GetActive());

            var notificationsByRol = notifications
                .Where(x => x.Rols!= null && x.Rols.Contains(userRol.ToString()))
                .ToList();

            var idUser = Convert.ToInt32(claimuser.Claims
                .Where(c => c.Type == ClaimTypes.NameIdentifier)
                .Select(c => c.Value).SingleOrDefault());

            var notificationsUser = _mapper.Map<List<VMNotifications>>(await _notificationService.GetByUserByActive(idUser));

            notificationsByRol.AddRange(notificationsUser);

            ViewData["userRol"] = userRol;

            return View(notificationsByRol);
        }
    }
}
