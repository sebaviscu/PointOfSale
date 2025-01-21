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
                var todavy = TimeHelper.GetArgentinaTime();

                int day = (int)todavy.DayOfWeek;

                if (ajustes.HorariosWeb != null)
                {
                    var horariosDiaActual = ajustes.HorariosWeb.Where(_ => (int)_.DiaSemana == day).ToList();
                    foreach (var item in horariosDiaActual)
                    {
                        if (!string.IsNullOrEmpty(HorariosToday))
                        {
                            HorariosToday += $" - {item.HoraInicio.ToString(@"hh\:mm")} {item.HoraFin.ToString(@"hh\:mm")}";
                        }
                        else
                        {
                            HorariosToday = item.ToString();
                        }
                    }
                    Open = horariosDiaActual.Any(_ => _.HoraInicio < todavy.TimeOfDay && _.HoraFin > todavy.TimeOfDay);
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
        public bool Open { get; set; } = false;
    }
}
