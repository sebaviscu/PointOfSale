using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Utilities;
using System;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

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

        public (bool Resultado, string UserName, int IdTienda, ListaDePrecio IdListaPrecios) ValidarAutorizacion(Roles[] rolesPermitidos)
        {
            var claimuser = HttpContext.User;

            var userRol = Convert.ToInt32(claimuser.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value).SingleOrDefault());

            if (!ValidarPermisos.IsValid(userRol, rolesPermitidos))
            {
                throw new AccessViolationException("USUARIO CON PERMISOS INSUFICIENTES");
            }
            var userName = claimuser.Claims
                .Where(c => c.Type == ClaimTypes.Name)
                .Select(c => c.Value).SingleOrDefault();

            var idTienda = Convert.ToInt32(claimuser.Claims
                .Where(c => c.Type == "Tienda")
                .Select(c => c.Value).SingleOrDefault());

            var idListaPrecio = Convert.ToInt32(claimuser.Claims.
                                Where(c => c.Type == "ListaPrecios")
                                .Select(c => c.Value).SingleOrDefault());


            return (true, userName, idTienda, (ListaDePrecio)idListaPrecio);
        }

        public async Task UpdateClaimAsync(string claimType, string newValue)
        {
            // Obtener el usuario autenticado
            var user = HttpContext.User;

            if (user.Identity.IsAuthenticated)
            {
                // Obtener la lista actual de claims
                var claims = user.Claims.ToList();

                // Eliminar el claim actual que deseas modificar
                var claimToRemove = claims.FirstOrDefault(c => c.Type == claimType);
                if (claimToRemove != null)
                {
                    claims.Remove(claimToRemove);
                }

                // Agregar el nuevo claim con el valor actualizado
                claims.Add(new Claim(claimType, newValue));

                // Crear la nueva identidad con los claims actualizados
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // Crear las propiedades de autenticación
                var properties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = (HttpContext.Request.Cookies[".AspNetCore.Cookies"] != null)
                };

                // Actualizar la autenticación del usuario
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), properties);
            }
        }
    }
}
