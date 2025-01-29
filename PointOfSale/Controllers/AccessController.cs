using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Models;
using PointOfSale.Utilities.Response;
using System.Security.Claims;
using static PointOfSale.Model.Enum;
using UAParser;
using NuGet.Protocol;
using PointOfSale.Business.Services;

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
        private readonly IAjusteService _ajusteService;

        public AccessController(IUserService userService, ITurnoService turnoService, ITiendaService tiendaService, ISaleService saleService, IProductService productService, ILogger<AccessController> logger, IAfipService afipService, IAjusteService ajusteService)
        {
            _userService = userService;
            _turnoService = turnoService;
            _tiendaService = tiendaService;
            _saleService = saleService;
            _productService = productService;
            _logger = logger;
            _afipService = afipService;
            _ajusteService = ajusteService;
        }

        public async Task<IActionResult> Login()
        {
            try
            {
                ClaimsPrincipal claimuser = HttpContext.User;
                if (claimuser.Identity.IsAuthenticated)
                {
                    var idTienda = claimuser.Claims.Where(c => c.Type == "Tienda").Select(c => c.Value).SingleOrDefault();
                    var turnoLogin = claimuser.Claims.Where(c => c.Type == "Turno").Select(c => c.Value).SingleOrDefault();
                    var turnoActual = await _turnoService.GetTurnoActual(Convert.ToInt32(idTienda));

                    if (turnoActual.IdTurno != Convert.ToInt32(turnoLogin))
                    {
                        await UpdateClaimAsync("Turno", turnoActual.IdTurno.ToString());
                    }

                    var rol = claimuser.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value).SingleOrDefault();
                    if (rol != "1")
                        return RedirectToAction("NewSale", "Sales");
                    else
                        return RedirectToAction("DashBoard", "Admin");

                }
                return View(new VMUserLogin());

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error hacer login");
                ViewData["Message"] = $"Error: {ex.ToString()}.";
                return View(new VMUserLogin());
            }
        }

        [HttpPost]
        public async Task<IActionResult> Login(VMUserLogin model)
        {
            try
            {
                var result = await _userService.CheckFirstLogin(model.Email, model.PassWord);
                if (result)
                {
                    return View(new VMUserLogin() { FirstLogin = true });
                }

                var resultado = await _userService.GetByCredentials(model.Email, model.PassWord);
                var user_found = resultado.Usuario;

                if (user_found == null)
                {
                    ViewData["Message"] = resultado.Mensaje;
                    return View(new VMUserLogin());
                }

                int idTienda;
                var tiendas = await _tiendaService.List();
                int listaPrecio = 1;

                if (!user_found.IdTienda.HasValue)
                {
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
                }
                else
                {
                    idTienda = user_found.IdTienda.Value;
                }

                listaPrecio = (int)tiendas.FirstOrDefault(_ => _.IdTienda == idTienda).IdListaPrecio.Value;

                await _turnoService.CheckTurnosViejos(idTienda);
                var turnoActual = await _turnoService.GetTurnoActual(idTienda);

                await _productService.ActivarNotificacionVencimientos(idTienda);

                await _afipService.CheckVencimientoCertificado(idTienda);

                var ajustes = await _ajusteService.GetAjustes(idTienda);

                List<Claim> claims = new List<Claim>()
                {
                    new Claim(ClaimTypes.Name, user_found.Name),
                    new Claim(ClaimTypes.NameIdentifier, user_found.IdUsers.ToString()),
                    new Claim(ClaimTypes.Role,user_found.IdRol.ToString()),
                    new Claim("Email",user_found.Email),
                    new Claim("Tienda",idTienda.ToString()),
                    new Claim("ListaPrecios", listaPrecio.ToString()),
                    new Claim("ControlCierreCaja", ajustes.ControlTotalesCierreTurno.HasValue ? ajustes.ControlTotalesCierreTurno.Value.ToString() : "false")
                };

                if (turnoActual != null)
                {
                    claims.Add(new Claim("Turno", turnoActual.IdTurno.ToString()));
                }

                var now = TimeHelper.GetArgentinaTime();
                var diaSemanaActual = (DiasSemana)((int)now.DayOfWeek == 0 ? 7 : (int)now.DayOfWeek);
                var horario = user_found.Horarios.FirstOrDefault(_ => _.DiaSemana == diaSemanaActual);

                if (horario != null && user_found.SinHorario.HasValue && !user_found.SinHorario.Value)
                {
                    claims.Add(new Claim("HoraEntrada", horario.HoraEntrada));
                    claims.Add(new Claim("HoraSalida", horario.HoraSalida));
                }

                ClaimsIdentity claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                AuthenticationProperties properties = new AuthenticationProperties()
                {
                    AllowRefresh = true,
                    IsPersistent = true // model.KeepLoggedIn
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);

                var userAgent = HttpContext.Request.Headers["User-Agent"].ToString();
                var parser = Parser.GetDefault();
                var clientInfo = parser.Parse(userAgent);

                var hl = new HistorialLogin()
                {
                    Fecha = TimeHelper.GetArgentinaTime(),
                    IdUser = user_found.IdUsers,
                    UserName = user_found.Name,
                    Informacion = clientInfo.ToString()
                };

                await _userService.SaveHistorialLogin(hl);

                ViewData["Message"] = string.Empty;

                if (user_found.IdRol == 1)
                    return RedirectToAction("DashBoard", "Admin");
                else
                    return RedirectToAction("NewSale", "Sales");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al ingresar al login: {model.ToJson()}");
                ViewData["Message"] = $"Error: {ex.Message}";
                return View(new VMUserLogin());
            }
        }

        [HttpPost]
        public async Task<IActionResult> ChangeFirsUser(VMUserLogin model)
        {

            if (model.Email.ToLower() == "admin")
            {
                ViewData["Message"] = "Usuario no puede ser 'admin'";
                return View(new VMUserLogin());
            }

            var user_list = await _userService.List();
            var user_found = user_list.Single();

            user_found.Email = model.Email;
            user_found.Name = model.Name;
            user_found.Password = EncryptionHelper.EncryptString(model.PassWord);

            var user_edit = await _userService.Edit(user_found);

            model.IsAdmin = true;
            model.PassWord = model.PassWord;
            model.Email = model.Email;
            return View(model);
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
                return HandleException(ex, "Error al recuperar todos los usuarios.", _logger);
            }
        }

        [HttpPost]
        public async Task<IActionResult?> GenerarDatos()
        {
            var user = ValidarAutorizacion([Roles.Administrador]);
            ClaimsPrincipal claimuser = HttpContext.User;

            var result = await _saleService.GenerarVentas(user.IdTienda);

            GenericResponse<VMUser> gResponse = new GenericResponse<VMUser>();

            return StatusCode(result ? StatusCodes.Status200OK : StatusCodes.Status500InternalServerError, gResponse);
        }
    }
}
