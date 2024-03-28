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
            var notifications = _mapper.Map<List<VMNotifications>>(await _notificationService.GetActive());

            //ViewData["emailUser"] = notifications;

            return View(notifications);
        }
    }
}
