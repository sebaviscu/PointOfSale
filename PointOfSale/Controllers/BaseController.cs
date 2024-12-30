using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Utilities;
using System.Security.Claims;
using static PointOfSale.Model.Enum;
using PointOfSale.Model;
using NuGet.Protocol;
using PointOfSale.Utilities.Response;

namespace PointOfSale.Controllers
{
    public class BaseController : Controller
    {
        public IActionResult ValidateSesionViewOrLogin()
        {
            if (!HttpContext.User.Identity.IsAuthenticated)
                return RedirectToAction("Login", "Access");
            else
            {
                ValidarHorario();
                return View();
            }
        }

        public UserAuth ValidarAutorizacion(Roles[] rolesPermitidos)
        {
            var claimUser = HttpContext.User;

            var idListaPrecios = GetClaimValue<int>(claimUser, "ListaPrecios");

            ListaDePrecio listaPrecios;
            switch (idListaPrecios)
            {
                case 1:
                    listaPrecios = ListaDePrecio.Lista_1;
                    break;
                case 2:
                    listaPrecios = ListaDePrecio.Lista_2;
                    break;
                case 3:
                    listaPrecios = ListaDePrecio.Lista_3;
                    break;
                default:
                    throw new ArgumentException("Invalid ListaDePrecio value");
            }

            var userAuth = new UserAuth
            {
                IdRol = GetClaimValue<int>(claimUser, ClaimTypes.Role),
                UserName = GetClaimValue<string>(claimUser, ClaimTypes.Name),
                IdTienda = GetClaimValue<int>(claimUser, "Tienda"),
                IdListaPrecios = idListaPrecios,
                IdUsuario = GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                IdTurno = GetClaimValue<int>(claimUser, "Turno"),
                ListaPrecios = listaPrecios
            };

            userAuth.Result = rolesPermitidos.Contains((Model.Enum.Roles)userAuth.IdRol);

            if (!userAuth.Result)
            {
                StatusCode(StatusCodes.Status404NotFound);
                //throw new AccessViolationException("USUARIO CON PERMISOS INSUFICIENTES");
            }

            return userAuth;
        }

        private T GetClaimValue<T>(ClaimsPrincipal user, string claimType)
        {
            var claim = user.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
            if (string.IsNullOrEmpty(claim))
            {
                return default;
            }
            return (T)Convert.ChangeType(claim, typeof(T));
        }

        public async Task UpdateClaimAsync(string claimType, string newValue)
        {
            var user = HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                var claims = user.Claims.ToList();

                var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
                if (claimToRemove != null)
                {
                    claims.Remove(claimToRemove);
                }

                if (newValue != null)
                {
                    claims.Add(new Claim(claimType, newValue));
                }

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

                // Actualiza la cookie
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = (HttpContext.Request.Cookies[".AspNetCore.Cookies"] != null)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, properties);

                // Actualiza el HttpContext.User
                HttpContext.User = claimsPrincipal;
            }
        }


        protected IActionResult HandleException(Exception? ex, string errorMessage, ILogger<object> _logger, object model = null, params (string Key, object Value)[] additionalData)
        {
            var gResponse = new GenericResponse<object>
            {
                State = false,
                Message = ex == null ? errorMessage : $"{errorMessage}\n {ex.InnerException?.Message ?? ex.Message}"
            };

            // Preparar los parámetros adicionales para el log
            var logParams = new List<object> { errorMessage };

            if (model != null)
            {
                logParams.Add("ModelRequest");
                logParams.Add(model.ToJson());
            }

            if (additionalData != null)
            {
                foreach (var data in additionalData)
                {
                    logParams.Add(data.Key);
                    logParams.Add(data.Value?.ToJson() ?? "null");
                }
            }

            // Registrar el log con los parámetros adicionales
            var logTemplate = "{ErrorMessage}. " + string.Join(". ", additionalData.Select(d => $"{d.Key}: {{{d.Key}}}"));
            _logger.LogError(ex, logTemplate, logParams.ToArray());

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }



        public void ValidarHorario()
        {
            var claimUser = HttpContext.User;

            var horaEntrada = GetClaimValue<string>(claimUser, "HoraEntrada");
            var horaSalida = GetClaimValue<string>(claimUser, "HoraSalida");

            if (horaEntrada == null || horaSalida == null)
                return;
            var now = TimeHelper.GetArgentinaTime();

            var vaido = TimeSpan.Parse(horaEntrada) <= now.TimeOfDay &&
                        TimeSpan.Parse(horaSalida) >= now.TimeOfDay;

            if (!vaido)
            {
                throw new AccessViolationException("FUERA DEL HORARIO LABORAL");
            }

        }
    }
}
