using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMHorarioWeb
    {
        public int Id { get; set; }
        public DiasSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public override string ToString()
        {
            return $"{DiaSemana.ToString()}: de {HoraInicio.ToString(@"hh\:mm")} a {HoraFin.ToString(@"hh\:mm")}";
        }
    }
}
