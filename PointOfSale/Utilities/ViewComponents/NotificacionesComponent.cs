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
                .Where(x => !x.Rols.Contains(userRol.ToString()))
                .ToList();

            foreach (var n in notificationsByRol)
            {
                n.Accion = string.Empty;
            }

            ViewData["userRol"] = userRol;

            return View(notifications);
        }
    }
}
