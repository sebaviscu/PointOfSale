using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class NotificacionesComponent : ViewComponent
    {
        private readonly INotificationService _notificationService;

        public NotificacionesComponent(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var notifications = await _notificationService.GetActive();

            ViewData["emailUser"] = notifications;

            return View();
        }
    }
}
