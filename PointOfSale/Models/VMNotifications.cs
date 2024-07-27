namespace PointOfSale.Models
{
    public class VMNotifications
    {
        public int IdNotifications { get; set; }
        public string Descripcion { get; set; }
        public bool IsActive { get; set; }

        public DateTime RegistrationDate { get; set; }
        public string? RegistrationDateString { get; set; }

        public DateTime? ModificationDate { get; set; }
        public string? ModificationDateString { get; set; }

        public string? ModificationUser { get; set; }
        public string? Accion { get; set; }
        public string Rols { get; set; }
    }
}
