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

        public MenuUserViewComponent(IUserService userService, ITiendaService tiendaService)
        {
            _userService = userService;
            _tiendaService = tiendaService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            ClaimsPrincipal claimuser = HttpContext.User;


            string userName = string.Empty;
            string photoUser = string.Empty;
            string emailUser = string.Empty;
            string rolUser = string.Empty;
            string tiendaName = string.Empty;

            if (claimuser.Identity.IsAuthenticated)
            {
                userName = claimuser.Claims.Where(c => c.Type == ClaimTypes.Name).Select(c => c.Value).SingleOrDefault();

                int IdUser = Convert.ToInt32(claimuser.Claims
                    .Where(c => c.Type == ClaimTypes.NameIdentifier)
                    .Select(c => c.Value).SingleOrDefault());

                User user_found = await _userService.GetByIdWithRol(IdUser);

                var s = claimuser.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();

				rolUser = user_found.IdRolNavigation.Description;

                //if (user_found.Photo != null)
                //    photoUser = Convert.ToBase64String(user_found.Photo);

                emailUser = ((ClaimsIdentity)claimuser.Identity).FindFirst("Email").Value;

                var tiendaId = ((ClaimsIdentity)claimuser.Identity).FindFirst("Tienda").Value;

                var tienda = await _tiendaService.Get(Convert.ToInt32(tiendaId));
                tiendaName = tienda.Nombre;
            }

            ViewData["userName"] = userName;
            ViewData["photoUser"] = photoUser;
            ViewData["emailUser"] = emailUser;
            ViewData["rolUser"] = rolUser;
            ViewData["tienda"] = tiendaName;

            return View();
        }
    }
}
