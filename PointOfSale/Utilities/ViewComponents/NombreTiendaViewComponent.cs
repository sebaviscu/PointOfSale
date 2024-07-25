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

            if (claimuser.Identity.IsAuthenticated)
            {
                var tiendaId = ((ClaimsIdentity)claimuser.Identity).FindFirst("Tienda").Value;

                try
                {

                var tienda = await _tiendaService.Get(Convert.ToInt32(tiendaId));
                nombreTienda = tienda.Nombre;
                }
                catch (Exception e)
                {

                    throw;
                }
            }

            ViewData["NombreTienda"] = nombreTienda;

            return View();
        }
    }
}
