namespace PointOfSale.Models
{
    public class VMNotifications
    {
        public int IdNotifications { get; set; }
        public string Descripcion { get; set; }
        public bool Estado  { get; set; }
        public DateTime RegistrationDate { get; set; }
        public DateTime? ModificationDate { get; set; }
        public string? ModificationUser { get; set; }
    }
}
