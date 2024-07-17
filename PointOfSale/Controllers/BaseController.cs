using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Utilities;
using System;
using System.Security.Claims;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Controllers
{
    public class BaseController : Controller
    {
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
    }
}
