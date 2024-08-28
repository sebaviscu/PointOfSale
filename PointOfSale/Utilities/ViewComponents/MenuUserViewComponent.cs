using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using System.Security.Claims;

namespace PointOfSale.Utilities.ViewComponents
{
    public class MenuUserViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        private readonly ITiendaService _tiendaService;
        private readonly ITurnoService _turnoService;

        public MenuUserViewComponent(IUserService userService, ITiendaService tiendaService, ITurnoService turnoService)
        {
            _userService = userService;
            _tiendaService = tiendaService;
            _turnoService = turnoService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            ClaimsPrincipal claimuser = HttpContext.User;


            string userName = string.Empty;
            string photoUser = string.Empty;
            string emailUser = string.Empty;
            string rolUser = string.Empty;
            string tiendaName = string.Empty;
            string listaPrecios = string.Empty;
            string turno = string.Empty;
            int cantTiendas = 0;

            try
            {
                if (claimuser.Identity.IsAuthenticated)
                {
                    userName = claimuser.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

                    int IdUser = Convert.ToInt32(claimuser.Claims
                        .Where(c => c.Type == ClaimTypes.NameIdentifier)
                        .Select(c => c.Value).SingleOrDefault());

                    User user_found = await _userService.GetByIdWithRol(IdUser);

                    //var s = claimuser.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();

                    rolUser = user_found.IdRolNavigation.Description;

                    emailUser = ((ClaimsIdentity)claimuser.Identity).FindFirst("Email").Value;
                    listaPrecios = ((ClaimsIdentity)claimuser.Identity).FindFirst("ListaPrecios").Value;

                    var turnoId = ((ClaimsIdentity)claimuser.Identity).FindFirst("Turno");
                    if (turnoId !=  null && !string.IsNullOrEmpty(turnoId.Value))
                    {
                        var turnoObjet = await _turnoService.GetTurno(Convert.ToInt32(turnoId.Value));
                        turno = turnoObjet.FechaInicio.ToString();

                    }

                    var tiendaId = ((ClaimsIdentity)claimuser.Identity).FindFirst("Tienda").Value;

                    var tienda = await _tiendaService.Get(Convert.ToInt32(tiendaId));
                    tiendaName = tienda.Nombre;

                    if (user_found.IsAdmin)
                    {
                        var lTiendas = await _tiendaService.List();
                        cantTiendas = lTiendas.Count();
                    }

                }

                ViewData["userName"] = userName;
                ViewData["photoUser"] = photoUser;
                ViewData["emailUser"] = emailUser;
                ViewData["rolUser"] = rolUser;
                ViewData["tienda"] = tiendaName;
                ViewData["turno"] = turno;
                ViewData["listaPrecios"] = listaPrecios;


                ViewData["cantTiendas"] = cantTiendas;
            }
            catch (NullReferenceException e)
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                return View();
            }


            return View();
        }
    }
}
