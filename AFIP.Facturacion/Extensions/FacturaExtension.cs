using AFIP.Facturacion.Model;
using AfipServiceReference;
using PointOfSale.Business.Utilities;
using PointOfSale.Model.Afip.Factura;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AFIP.Facturacion.Extensions
{
    public static class FacturaExtension
    {
        public static FECAERequest ToFECAERequest(FacturaAFIP facturaAFIP)
        {

            var detalle = facturaAFIP.Detalle.First();
            var facturaFinal = new FECAERequest
            {
                FeCabReq = new FECAECabRequest
                {
                    CantReg = facturaAFIP.Detalle.Count,
                    CbteTipo = facturaAFIP.Cabecera.TipoComprobante.Id,
                    PtoVta = facturaAFIP.Cabecera.PuntoVenta,
                },
                FeDetReq = new List<FECAEDetRequest>()
                {
                    new FECAEDetRequest
                    {
                        Concepto = detalle.Concepto,
                        DocTipo = detalle.TipoDocumento.Id,
                        DocNro = detalle.NroDocumento,
                        CbteDesde = detalle.NroComprobanteDesde,
                        CbteHasta = detalle.NroComprobanteHasta,
                        CbteFch = detalle.FechaComprobante?.ToAFIPDateString(),
                        ImpTotal = detalle.ImporteTotal,
                        ImpTotConc = detalle.ImporteTotalConc,
                        ImpNeto = detalle.ImporteNeto,
                        ImpOpEx = detalle.ImporteOpExento,
                        ImpIVA = detalle.ImporteIVA,
                        ImpTrib = detalle.ImporteTributos,
                        FchServDesde = detalle.FechaServicioDesde?.ToAFIPDateString(),
                        FchServHasta = detalle.FechaServicioHasta?.ToAFIPDateString(),
                        FchVtoPago = detalle.FechaVencimientoPago?.ToAFIPDateString(),
                        MonId = detalle.Moneda.Id,
                        MonCotiz = detalle.CotizacionMoneda
                    }
                }
            };

            if (detalle.ImporteIVA > 0)
            {
                facturaFinal.FeDetReq.First().Iva = new List<AlicIva>
                        {
                            new AlicIva
                            {
                                Id = 5,
                                BaseImp = detalle.ImporteNeto,
                                Importe = detalle.ImporteIVA
                            }
                        };
            }

            return facturaFinal;
        }

        public static FacturaEmitida ToFacturaEmitida(FECAEDetResponse fECAECabResponse)
        {
            if (fECAECabResponse == null)
                return null;

            var facturaRecibida = new FacturaEmitida()
            {
                CAE = fECAECabResponse.CAE,
                CAEVencimiento = fECAECabResponse.CAEFchVto != string.Empty ? DateTime.ParseExact(fECAECabResponse.CAEFchVto, "yyyyMMdd", CultureInfo.InvariantCulture) : null,
                FechaEmicion = DateTime.ParseExact(fECAECabResponse.CbteFch, "yyyyMMdd", CultureInfo.InvariantCulture),
                NroDocumento = (int)fECAECabResponse.DocNro,
                TipoDocumentoId = fECAECabResponse.DocTipo,
                TipoDocumento = TipoDocumento.GetById(fECAECabResponse.DocTipo).Description,
                Resultado = fECAECabResponse.Resultado,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            if (fECAECabResponse.Observaciones != null && fECAECabResponse.Observaciones.Any())
            {
                facturaRecibida.Errores = string.Join(Environment.NewLine, fECAECabResponse.Observaciones.Select(_=>_.Msg));
            }
            return facturaRecibida;
        }


    }
}
