using System;
using System.Collections.Generic;
using System.ComponentModel;
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
            U = 2
        }

        public enum TipoMovimientoCliente
        {
            Ingreso = 1,
            Egreso = 2
        }

        public enum TypeValuesDashboard
        {
            Dia = 0,
            Semana =1,
            Mes= 2
        }

        public enum DiasSemana
        {
            Lunes = 1,
            Martes = 2,
            Miercoles = 3,
            Jueves = 4,
            Viernes = 5,
            Sabado = 6,
            Domingo = 7
        }

        public enum TipoDeGastoEnum
        {
            Sueldos = 0,
            Fijo = 1,
            Variable = 2,
            Servicios = 3
        }
        public enum TipoFactura
        {
            A = 0,
            B = 1,
            C = 2,
            X = 3,
            Presu = 4
        }

        public enum EstadoVentaWeb
        {
            Nueva,
            Finalizada,
            Cerrada
        }

        public enum ListaDePrecio
        {
            Lista_1 = 1,
            Lista_2 = 2, 
            Lista_3 = 3    
        }

        public enum EstadoPago
        {
            Pagado,
            Pendiente
        }

        public enum EstadoVencimiento
        {
            Apto,
            ProximoVencimiento,
            Vencido
        }

        public enum EstadoPedido
        {
            Cancelado,
            Iniciado,
            Enviado,
            Recibido
        }

        public enum  TipoIvaReport
        {
            Compra,
            Venta,
            Servicios,
            Gastos
        }
    }
}
