using static PointOfSale.Model.Enum;

namespace PointOfSale.Model
{
    public class Empresa : EntityBase
    {
        public string RazonSocial { get; set; }
        public string? NombreContacto { get; set; }
        public string? NumeroContacto { get; set; }
        public Licencias Licencia { get; set; }
        public DateTime? ProximoPago { get; set; }
        public FrecuenciasPago? FrecuenciaPago { get; set; }
        public string? Comentario { get; set; }
        public virtual List<PagoEmpresa>? Pagos {  get; set; }

        public void AddLicencia(Licencias licencia)
        {
            Licencia |= licencia;
        }

        public void RemoveLicencia(Licencias licencia)
        {
            Licencia &= ~licencia;
        }

        public bool CheckLicencia(Licencias licencia)
        {
            return (Licencia & licencia) == licencia;
        }

        public override string ToString()
        {
            return Licencia.ToString();
        }
    }
}
