using AFIP.Facturacion.Extensions;
using AFIP.Facturacion.Model;
using AfipServiceReference;
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

        public async Task<FacturacionResponse> FacturarAsync(FacturaAFIP factura)
        {
            var comprobante = await _wsfeClient.FECAESolicitarAsync(FacturaExtension.ToFECAERequest(factura));

            comprobante.Body.FECAESolicitarResult.Errors.EnsureSucceededResponse();

            return new FacturacionResponse
            {
                FECAECabResponse = comprobante.Body.FECAESolicitarResult.FeCabResp, 
                FECAEDetResponse = comprobante.Body.FECAESolicitarResult.FeDetResp,
            };
        }

        public async Task<FERecuperaLastCbteResponse> GetUltimoComprobanteAutorizadoAsync(int ptoVenta, TipoComprobante tipoComprobante)
        {
            var result = await _wsfeClient.FECompUltimoAutorizadoAsync(ptoVenta, tipoComprobante.Id);
            result.Body.FECompUltimoAutorizadoResult.Errors.EnsureSucceededResponse();

            return result.Body.FECompUltimoAutorizadoResult;
        }

        public async Task<MonedaResponse> GetTiposMonedasAsync()
        {
            var result = await _wsfeClient.FEParamGetTiposMonedasAsync();
            result.Body.FEParamGetTiposMonedasResult.Errors.EnsureSucceededResponse();

            return result.Body.FEParamGetTiposMonedasResult;
        }

        public async Task<DocTipoResponse> GetTiposDocAsync()
        {
            var result = await _wsfeClient.FEParamGetTiposDocAsync();
            result.Body.FEParamGetTiposDocResult.Errors.EnsureSucceededResponse();

            return result.Body.FEParamGetTiposDocResult;
        }

        public async Task<FEPtoVentaResponse> GetPtosVentaAsync()
        {
            var result = await _wsfeClient.FEParamGetPtosVentaAsync();
            result.Body.FEParamGetPtosVentaResult.Errors.EnsureSucceededResponse();

            return result.Body.FEParamGetPtosVentaResult;
        }
    }


}
