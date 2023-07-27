using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using System.Security.Claims;

namespace PointOfSale.Controllers
{
    public class AccessController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITurnoService _turnoService;
        public AccessController(IUserService userService, ITurnoService turnoService)
        {
            _userService = userService;
            _turnoService = turnoService;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            if (claimuser.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMUserLogin model)
        {
            User user_found = await _userService.GetByCredentials(model.Email, model.PassWord);

            if (user_found == null)
            {
                ViewData["Message"] = "No matches found";
                return View();
            }

            if (model.TiendaId == null && user_found.IdRol == 1)
            {
                model.IsAdmin = true;
                return View(model);
            }

            var idTienda = user_found.IsAdmin() ? model.TiendaId.Value : user_found.IdTienda;

            await _turnoService.CheckTurnosViejos();
            var turno = await _turnoService.GetTurno(idTienda.Value, user_found.Name);

            List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user_found.Name),
                    new Claim(ClaimTypes.NameIdentifier, user_found.IdUsers.ToString()),
                    new Claim(ClaimTypes.Role,user_found.IdRol.ToString()),
                    new Claim("Email",user_found.Email),
                    new Claim("Tienda",idTienda.ToString()),
                    new Claim("Turno",turno.IdTurno.ToString()),
                };

            ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            AuthenticationProperties properties = new AuthenticationProperties()
            {
                AllowRefresh = true,
                IsPersistent = model.KeepLoggedIn
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

            ViewData["Message"] = null;

            return RedirectToAction("Index", "Home");


        }
    }
}
