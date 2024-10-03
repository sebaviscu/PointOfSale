using PointOfSale.Model;
using static PointOfSale.Model.Enum;

namespace PointOfSale.Models
{
    public class VMHorario
    {
        public int Id { get; set; }
        public DateTime? RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
        public string? HoraEntrada { get; set; }
        public string? HoraSalida { get; set; }
        public DiasSemana? DiaSemana { get; set; }
        public int? IdUsuario { get; set; }
    }
}
