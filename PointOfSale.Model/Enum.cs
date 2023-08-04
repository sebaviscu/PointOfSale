using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PointOfSale.Model
{
    public class Enum
    {
        public enum Roles
        {
            Administrador = 1,
            Empleado = 2,
            Encargado = 3
        }

        public enum TipoVenta
        {
            Kg = 1,
            Unidad = 2
        }

        public enum TipoMovimientoCliente
        {
            Ingreso = 1,
            Egreso = 2
        }

    }
}
