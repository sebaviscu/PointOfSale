using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class ListaPrecio
    {
        public DateTime DateTimeNowArg => TimeZoneInfo.ConvertTime(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time"));

        public ListaPrecio()
        {

        }

        public ListaPrecio(ListaDePrecio listaDePrecio)
        {
            Lista = listaDePrecio;
            RegistrationDate = DateTimeNowArg;
        }

        public ListaPrecio(int idProd, ListaDePrecio listaDePrecio, decimal precio, int porcentajeProfit)
        {
            IdProducto = idProd;
            Lista = listaDePrecio;
            Precio = precio;
            RegistrationDate = DateTimeNowArg;
            PorcentajeProfit = porcentajeProfit;
        }

        public int IdListaPrecios { get; set; }
        public ListaDePrecio Lista { get; set; }
        public int IdProducto { get; set; }
        public Product? Producto { get; set; }
        public decimal Precio { get; set; }
        public int PorcentajeProfit { get; set; }
        public DateTime? RegistrationDate { get; set; }
    }
}
