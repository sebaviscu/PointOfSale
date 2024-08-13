using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Services;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities;
using PointOfSale.Utilities.Response;
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
        private readonly IProductService _productService;
        private readonly ILogger<AccessController> _logger;
        private readonly IAfipService _afipService;

        public AccessController(IUserService userService, ITurnoService turnoService, ITiendaService tiendaService, ISaleService saleService, IProductService productService, ILogger<AccessController> logger, IAfipService afipService)
        {
            _userService = userService;
            _turnoService = turnoService;
            _tiendaService = tiendaService;
            _saleService = saleService;
            _productService = productService;
            _logger = logger;
            _afipService = afipService;
        }

        public IActionResult Login()
        {
            try
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
                return View(new VMUserLogin());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hacer login");
                ViewData["Message"] = $"Error: {ex.ToString()}.";
                return View();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMUserLogin model)
        {
            try
            {
                var user_found = await _userService.GetByCredentials(model.Email, model.PassWord);

                if (user_found == null)
                {
                    ViewData["Message"] = "Usuario no encontrado";
                    return View();
                }

                int idTienda;
                var tiendas = await _tiendaService.List();
                int listaPrecio = 1;

                if (tiendas.Count == 1)
                {
                    var tienda = tiendas.First();
                    idTienda = tienda.IdTienda;
                }
                else
                {
                    if (model.TiendaId == null && user_found.IdRol == 1)
                    {
                        model.IsAdmin = true;
                        model.PassWord = model.PassWord;
                        model.Email = model.Email;
                        return View(model);
                    }

                    idTienda = (int)(user_found.IsAdmin ? model.TiendaId.Value : user_found.IdTienda);
                }

                listaPrecio = (int)tiendas.FirstOrDefault(_ => _.IdTienda == idTienda).IdListaPrecio.Value;
                await _turnoService.CheckTurnosViejos(idTienda);
                var turno = await _turnoService.GetTurno(idTienda, user_found.Name);

                await _productService.ActivarNotificacionVencimientos(idTienda);

                await _afipService.CheckVencimientoCertificado(idTienda);

                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user_found.Name),
                    new Claim(ClaimTypes.NameIdentifier, user_found.IdUsers.ToString()),
                    new Claim(ClaimTypes.Role,user_found.IdRol.ToString()),
                    new Claim("Email",user_found.Email),
                    new Claim("Tienda",idTienda.ToString()),
                    new Claim("Turno",turno.IdTurno.ToString()),
                    new Claim("ListaPrecios", listaPrecio.ToString())
                };

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = model.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                ViewData["Message"] = string.Empty;

                if (user_found.IdRol == 1)
                    return RedirectToAction("DashBoard", "Admin");
                else
                    return RedirectToAction("NewSale", "Sales");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al ingresar al login");
                ViewData["Message"] = $"Error: {ex}.";
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var gResponse = new GenericResponse<List<User>>();
            try
            {
                gResponse.Object = await _userService.GetAllUsers();
                gResponse.State = true;
                return StatusCode(StatusCodes.Status200OK, gResponse);
            }
            catch (Exception ex)
            {
                var errorMessage = "Error al recuperar todos los usuarios";
                gResponse.State = false;
                gResponse.Message = $"{errorMessage}\n {ex.ToString()}";
                _logger.LogError(ex, "{ErrorMessage}.", errorMessage);
                return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
            }
        }

        [HttpPost]
        public async Task<IActionResult?> GenerarDatos()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);
            ClaimsPrincipal claimuser = HttpContext.User;

            string idUsuario = claimuser.Claims.Where(c => c.Type == ClaimTypes.NameIdentifier).Select(c => c.Value).SingleOrDefault();

            var result = await _saleService.GenerarVentas(user.IdTienda, int.Parse(idUsuario));

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();

            return StatusCode(result ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError, gResponse);
        }
    }
}
