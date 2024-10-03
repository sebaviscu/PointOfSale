using System.ComponentModel.DataAnnotations.Schema;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Horario : EntityBase
    {
        public string HoraEntrada { get; set; }
        public string HoraSalida { get; set; }
        public DiasSemana DiaSemana { get; set; }
        public int IdUsuario { get; set; }
        public User? User { get; set; }
    }
}
