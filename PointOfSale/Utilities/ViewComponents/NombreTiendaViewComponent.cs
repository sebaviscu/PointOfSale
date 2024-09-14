using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class NombreTiendaViewComponent : ViewComponent
    {
        private readonly ITiendaService _tiendaService;

        public NombreTiendaViewComponent(ITiendaService tiendaService)
        {
            _tiendaService = tiendaService;
        }
        public async Task<IViewComponentResult> InvokeAsync()
        {

            ClaimsPrincipal claimuser = HttpContext.User;
            var nombreTienda = string.Empty;
            var color = "#4c84ff";

            if (claimuser.Identity.IsAuthenticated)
            {
                var tiendaId = ((ClaimsIdentity)claimuser.Identity).FindFirst("Tienda").Value;

                var tienda = await _tiendaService.Get(Convert.ToInt32(tiendaId));
                nombreTienda = tienda.Nombre;
                color = tienda.Color;
            }

            ViewData["NombreTienda"] = nombreTienda;
            ViewData["Color"] = color;

            return View();
        }
    }
}
