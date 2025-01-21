using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMHorarioWeb
    {
        public int Id { get; set; }
        public DiasSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }
    }
}
