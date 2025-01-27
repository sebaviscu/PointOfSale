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

                Nombre = ajustes.Nombre;
                Direccion = ajustes.Direccion;
                Email = ajustes.Email;
                Whatsapp = ajustes.Whatsapp;
                SobreNosotros = ajustes.SobreNosotros;
                MontoEnvioGratis = ajustes.MontoEnvioGratis;
                Twitter = ajustes.Twitter;
                Facebook = ajustes.Facebook;
                Youtube = ajustes.Youtube;
                Tiktok = ajustes.Tiktok;
                Instagram = ajustes.Instagram;
                HabilitarTakeAway = ajustes.HabilitarTakeAway;
                TakeAwayDescuento = ajustes.TakeAwayDescuento;
                TemrinosCondiciones = ajustes.TemrinosCondiciones;
                PoliticaPrivacidad = ajustes.PoliticaPrivacidad;
                
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

        //public VMAjustesWeb Ajustes { get; set; }
        public bool Open { get; set; } = false;

        public string? SobreNosotros { get; set; }
        public string? TemrinosCondiciones { get; set; }
        public string? PoliticaPrivacidad { get; set; }
        public string? Nombre { get; set; }
        public string? Whatsapp { get; set; }
        public string? Direccion { get; set; }
        public string? Email { get; set; }
        public decimal? MontoEnvioGratis { get; set; }
        public string? Facebook { get; set; }
        public string? Instagram { get; set; }
        public string? Tiktok { get; set; }
        public string? Twitter { get; set; }
        public string? Youtube { get; set; }
        public bool? HabilitarTakeAway { get; set; }
        public decimal? TakeAwayDescuento { get; set; }
    }
}
