using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Business.Utilities
{
	public class ValidarPermisos
	{
		public static bool IsValid(int rol, Model.Enum.Roles[] rolesPermitidos)
		{
			return rolesPermitidos.Contains((Model.Enum.Roles)rol);
		}
	}
}
