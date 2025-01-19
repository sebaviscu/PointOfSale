namespace PointOfSale.Models
{
    public class VMNotifications
    {
        public int IdNotifications { get; set; }
        public string Descripcion { get; set; }
        public bool IsActive { get; set; }
        public string? RegistrationUser { get; set; }

        public DateTime? RegistrationDate { get; set; }
        public string? RegistrationDateString { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationDateString { get; set; }

        public string? ModificationUser { get; set; }
        public string? Accion { get; set; }
        public string? Rols { get; set; }
        public int? IdUser { get; set; }
        public string? UserNameString { get; set; }
        public int? IdRol { get; set; } // solo en VM
    }
}
