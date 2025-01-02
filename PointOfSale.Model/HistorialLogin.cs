namespace PointOfSale.Model
{
    public class HistorialLogin
    {
        public int Id { get; set; }
        public string Informacion { get; set; }
        public DateTime Fecha { get; set; }

        public int IdUser { get; set; }
        public string UserName { get; set; }
        public User? User { get; set; }
    }
}
