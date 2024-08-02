using AFIP.Facturacion.Model;
using AfipServiceReference;
using PointOfSale.Model.Afip.Factura;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Services
{
    public interface IAFIPFacturacionService
    {
        Task<FacturacionResponse> FacturarAsync(FacturaAFIP factura);
        Task<FERecuperaLastCbteResponse> GetUltimoComprobanteAutorizadoAsync(int ptoVenta, TipoComprobante tipoComprobante);
        Task<MonedaResponse> GetTiposMonedasAsync();
        Task<DocTipoResponse> GetTiposDocAsync();
        Task<FEPtoVentaResponse> GetPtosVentaAsync();
    }
}
