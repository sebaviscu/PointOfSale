using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Model;
using PointOfSale.Models;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class AccessController : BaseController
    {
        private readonly IUserService _userService;
        private readonly ITurnoService _turnoService;
        private readonly ITiendaService _tiendaService;
        private readonly ISaleService _saleService;

        public AccessController(IUserService userService, ITurnoService turnoService, ITiendaService tiendaService, ISaleService saleService)
        {
            _userService = userService;
            _turnoService = turnoService;
            _tiendaService = tiendaService;
            _saleService = saleService;
        }

        public IActionResult Login()
        {
            ClaimsPrincipal claimuser = HttpContext.User;
            if (claimuser.Identity.IsAuthenticated)
            {
                var rol = claimuser.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();
                if (rol == "1")
                    return RedirectToAction("DashBoard", "Admin");
                else
                    return RedirectToAction("NewSale", "Sales");

            }
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
            int idTienda;
            var tiendas = await _tiendaService.List();

            if (tiendas.Count == 1)
            {
                idTienda = tiendas.First().IdTienda;
            }
            else
            {
                if (model.TiendaId == null && user_found.IdRol == 1)
                {
                    model.IsAdmin = true;
                    return View(model);
                }

                idTienda = (int)(user_found.IsAdmin ? model.TiendaId.Value : user_found.IdTienda);
            }

            await _turnoService.CheckTurnosViejos(idTienda);
            var turno = await _turnoService.GetTurno(idTienda, user_found.Name);

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

            if (user_found.IdRol == 1)
                return RedirectToAction("DashBoard", "Admin");
            else
                return RedirectToAction("NewSale", "Sales");

        }

        [HttpGet]
        public async Task<List<User>> GetAllUsers()
        {
            var users = await _userService.GetAllUsers();
            return users;
        }

        [HttpPost]
        public async Task<IActionResult?> GenerarDatos()
        {
            var user = ValidarAutorizacion(new Roles[] { Roles.Administrador, Roles.Encargado, Roles.Empleado });
            ClaimsPrincipal claimuser = HttpContext.User;

            string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            await _saleService.GenerarVentas(user.IdTienda, int.Parse(idUsuario));
            return default;
        }
    }
}
