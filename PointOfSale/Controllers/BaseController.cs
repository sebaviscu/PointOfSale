using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Utilities;
using System;
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
                return View();
        }

        public UserAuth ValidarAutorizacion(Roles[] rolesPermitidos)
        {
            var claimUser = HttpContext.User;

            var userAuth = new UserAuth
            {
                IdRol = GetClaimValue<int>(claimUser, ClaimTypes.Role),
                UserName = GetClaimValue<string>(claimUser, ClaimTypes.Name),
                IdTienda = GetClaimValue<int>(claimUser, "Tienda"),
                IdListaPrecios = GetClaimValue<int>(claimUser, "ListaPrecios"),
                IdUsuario = GetClaimValue<int>(claimUser, ClaimTypes.NameIdentifier),
                IdTurno = GetClaimValue<int>(claimUser, "Turno")
            };

            userAuth.Result = ValidarPermisos.IsValid(userAuth.IdRol, rolesPermitidos);

            if (!userAuth.Result)
            {
                throw new AccessViolationException("USUARIO CON PERMISOS INSUFICIENTES");
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

                claims.Add(new Claim(claimType, newValue));

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = (HttpContext.Request.Cookies[".AspNetCore.Cookies"] != null)
                };

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            }
        }

        protected IActionResult HandleException(Exception ex, string errorMessage, ILogger<object> _logger, object model = null)
        {
            var gResponse = new GenericResponse<object>
            {
                State = false,
                Message = $"{errorMessage}\n {ex.InnerException?.ToString() ?? ex.Message}"
            };

            _logger.LogError(ex, "{ErrorMessage}. Request: {ModelRequest}", errorMessage, model?.ToJson());

            return StatusCode(StatusCodes.Status500InternalServerError, gResponse);
        }
    }
}
