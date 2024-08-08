using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class UserAuth
    {
        public int IdUsuario { get; set; }
        public int IdTurno { get; set; }
        public int IdRol { get; set; }
        public int IdTienda { get; set; }
        public string UserName { get; set; }
        public int IdListaPrecios { get; set; }
        public bool Result { get; set; }
    }
}
