using AfipServiceReference;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Extensions
{
    public static class FacturaExtension
    {
        public static FECAERequest ToFECAERequest(FacturaAFIP facturaAFIP)
        => new FECAERequest
        {
            FeCabReq = new FECAECabRequest
            {
                CantReg = facturaAFIP.Detalle.Count,
                CbteTipo = facturaAFIP.Cabecera.TipoComprobante.Id,
                PtoVta = facturaAFIP.Cabecera.PuntoVenta,
            },
            FeDetReq = facturaAFIP.Detalle.Select(x => new FECAEDetRequest
            {
                Concepto = x.Concepto,
                DocTipo = x.TipoDocumento.Id,
                DocNro = x.NroDocumento,
                CbteDesde = x.NroComprobanteDesde,
                CbteHasta = x.NroComprobanteHasta,
                CbteFch = x.FechaComprobante?.ToAFIPDateString(),
                ImpTotal = x.ImporteTotal,
                ImpTotConc = x.ImporteTotalConc,
                ImpNeto = x.ImporteNeto,
                ImpOpEx = x.ImporteOpExento,
                ImpIVA = x.ImporteIVA,
                ImpTrib = x.ImporteTributos,
                FchServDesde = x.FechaServicioDesde?.ToAFIPDateString(),
                FchServHasta = x.FechaServicioHasta?.ToAFIPDateString(),
                FchVtoPago = x.FechaVencimientoPago?.ToAFIPDateString(),
                MonId = x.Moneda.Id,
                MonCotiz = x.CotizacionMoneda
            }).ToList(),
        };
        public static FacturaEmitida ToFacturaEmitida(FECAECabResponse fECAECabResponse)
            => new FacturaEmitida
            {

            };
    }


}
