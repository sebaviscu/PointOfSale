namespace PointOfSale.Models
{
    public class VMHistorialLogin
    {
        public int Id { get; set; }
        public string Informacion { get; set; }
        public DateTime Fecha { get; set; }

        public int IdUser { get; set; }
        public string UserName { get; set; }
    }
}
