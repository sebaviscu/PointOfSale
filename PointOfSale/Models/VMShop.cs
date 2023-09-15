using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMShop
    {
        public VMShop(VMTienda tienda)
        {
            Tienda= tienda;

            Horarios = $"· Lunes: {tienda.Lunes} \n· Martes: {tienda.Martes} \n· Miercoles: {tienda.Miercoles} \n· Jueves: {tienda.Jueves}\n· Viernes: {tienda.Viernes}\n· Sabado: {tienda.Sabado}\n· Domingo: {tienda.Domingo}\n· Feriados: {tienda.Feriado}";

            int day = (int)DateTime.Now.DayOfWeek;
            switch (day)
            {
                case 1:
                    HorariosToday = "Lunes: " + tienda.Lunes;
                    break;
                case 2:
                    HorariosToday = "Martes: " + tienda.Martes;
                    break;
                case 3:
                    HorariosToday = "Miercoles: " + tienda.Miercoles;
                    break;
                case 4:
                    HorariosToday = "Jueves: " + tienda.Jueves;
                    break;
                case 5:
                    HorariosToday = "Viernes: " + tienda.Viernes;
                    break;
                case 6:
                    HorariosToday = "Sabado: " + tienda.Sabado;
                    break;
                case 7:
                    HorariosToday = "Domingo: " + tienda.Domingo;
                    break;
            }
        }

        public List<VMProduct> Products { get; set; }

        public VMTienda Tienda { get; set; }

        public bool IsLogin { get; set; }

        public string Horarios { get; set; }

        public string HorariosToday { get; set; }
    }
}
