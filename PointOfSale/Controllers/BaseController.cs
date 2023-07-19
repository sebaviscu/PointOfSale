using Microsoft.AspNetCore.Mvc;
using PointOfSale.Business.Utilities;
using System.Security.Claims;
using static PointOfSale.Business.Utilities.Enum;

namespace PointOfSale.Controllers
{
	public class BaseController : Controller
	{
		public (bool Resultado, int UsuarioId) ValidarAutorizacion(Roles[] rolesPermitidos)
		{
			var claimuser = HttpContext.User;

			var userRol = Convert.ToInt32(claimuser.Claims
				.Where(c => c.Type == ClaimTypes.Role)
				.Select(c => c.Value).SingleOrDefault());

			if (!ValidarPermisos.IsValid(userRol, rolesPermitidos))
			{
				//StatusCode(StatusCodes.Status503ServiceUnavailable, "USUARIO CON PERMISOS INSUFICIENTES");
				throw new AccessViolationException("USUARIO CON PERMISOS INSUFICIENTES");
			}
			var userId = Convert.ToInt32(claimuser.Claims
				.Where(c => c.Type == ClaimTypes.NameIdentifier)
				.Select(c => c.Value).SingleOrDefault());

			return (true, userId);
		}
	}
}
