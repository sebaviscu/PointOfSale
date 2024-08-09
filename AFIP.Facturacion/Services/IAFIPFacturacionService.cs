using AFIP.Facturacion.Model;
using AfipServiceReference;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Services
{
    public interface IAFIPFacturacionService
    {
        Task<FacturacionResponse> FacturarAsync(AjustesFacturacion ajustes, FacturaAFIP factura);
        Task<FERecuperaLastCbteResponse> GetUltimoComprobanteAutorizadoAsync(AjustesFacturacion ajustes, int ptoVenta, TipoComprobante tipoComprobante);
    }
}
