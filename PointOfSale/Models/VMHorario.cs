using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMHorario : EntityBaseDto
    {
        public string? HoraEntrada { get; set; }
        public string? HoraSalida { get; set; }
        public DiasSemana? DiaSemana { get; set; }
        public int? IdUsuario { get; set; }
    }
}
