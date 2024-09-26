using AFIP.Facturacion.Extensions;
using AFIP.Facturacion.Model;
using AfipServiceReference;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Services
{
    public class AFIPFacturacionService : IAFIPFacturacionService
    {
        private readonly WsfeClient _wsfeClient;

        public AFIPFacturacionService(WsfeClient wsfeClient)
        {
            _wsfeClient = wsfeClient;
        }

        public async Task<FacturacionResponse> FacturarAsync(AjustesFacturacion ajustes, FacturaAFIP factura)
        {
            var comprobante = await _wsfeClient.FECAESolicitarAsync(ajustes, FacturaExtension.ToFECAERequest(factura));

            comprobante.Body.FECAESolicitarResult.Errors.EnsureSucceededResponse();

            return new FacturacionResponse
            {
                FECAECabResponse = comprobante.Body.FECAESolicitarResult.FeCabResp,
                FECAEDetResponse = comprobante.Body.FECAESolicitarResult.FeDetResp,
            };


            //var comprobante = await _wsfeClient.FECAEASolicitar_CAEA_Async(ajustes, FacturaExtension.ToFECAERequest_CAEA(factura));
            //var errors = comprobante.Body.FECAEARegInformativoResult.Errors;
            //var cabecera = comprobante.Body.FECAEARegInformativoResult.FeCabResp;
            //var detalles = comprobante.Body.FECAEARegInformativoResult.FeDetResp;

            //return default;
        }

        public async Task<FERecuperaLastCbteResponse> GetUltimoComprobanteAutorizadoAsync(AjustesFacturacion ajustes, int ptoVenta, TipoComprobante tipoComprobante)
        {
            var result = await _wsfeClient.FECompUltimoAutorizadoAsync(ajustes, ptoVenta, tipoComprobante.Id);
            result.Body.FECompUltimoAutorizadoResult.Errors.EnsureSucceededResponse();

            return result.Body.FECompUltimoAutorizadoResult;
        }
    }
}
