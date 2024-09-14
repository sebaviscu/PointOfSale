using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;

namespace PointOfSale.Models
{
    public class VMShop
    {

        public VMShop(VMAjustesWeb? ajustes)
        {
            if (ajustes != null)
            {
                Ajustes = ajustes;
                Horarios = $"· Lunes: {ajustes.Lunes} \n· Martes: {ajustes.Martes} \n· Miercoles: {ajustes.Miercoles} \n· Jueves: {ajustes.Jueves}\n· Viernes: {ajustes.Viernes}\n· Sabado: {ajustes.Sabado}\n· Domingo: {ajustes.Domingo}\n· Feriados: {ajustes.Feriado}";

                int day = (int)TimeHelper.GetArgentinaTime().DayOfWeek;
                switch (day)
                {
                    case 1:
                        HorariosToday = "Lunes: " + ajustes.Lunes;
                        break;
                    case 2:
                        HorariosToday = "Martes: " + ajustes.Martes;
                        break;
                    case 3:
                        HorariosToday = "Miercoles: " + ajustes.Miercoles;
                        break;
                    case 4:
                        HorariosToday = "Jueves: " + ajustes.Jueves;
                        break;
                    case 5:
                        HorariosToday = "Viernes: " + ajustes.Viernes;
                        break;
                    case 6:
                        HorariosToday = "Sabado: " + ajustes.Sabado;
                        break;
                    case 0:
                        HorariosToday = "Domingo: " + ajustes.Domingo;
                        break;
                }

            }
        }

        public List<VMProduct> Products { get; set; }

        public List<VMCategory> Categorias { get; set; }
        public List<VMTag> Tags { get; set; }
        public List<VMCategoriaWeb> CategoriaWebs { get; set; }

        public bool IsLogin { get; set; }

        public string Horarios { get; set; }

        public string HorariosToday { get; set; }

        public List<VMTypeDocumentSale> FormasDePago { get; set; }

        public VMAjustesWeb Ajustes { get; set; }
    }
}
