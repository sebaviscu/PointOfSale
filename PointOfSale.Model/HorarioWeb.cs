using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class HorarioWeb
    {
        public int Id { get; set; }
        public DiasSemana DiaSemana { get; set; }
        public TimeSpan HoraInicio { get; set; }
        public TimeSpan HoraFin { get; set; }

        public int IdAjusteWeb { get; set; }
        public AjustesWeb? AjustesWeb { get; set; }
    }
}
