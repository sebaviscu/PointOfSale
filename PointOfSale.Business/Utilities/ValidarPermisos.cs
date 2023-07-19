using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
	public class ValidarPermisos
	{
		public static bool IsValid(int rol, Enum.Roles[] rolesPermitidos)
		{
			return rolesPermitidos.Contains((Enum.Roles)rol);
		}
	}
}
