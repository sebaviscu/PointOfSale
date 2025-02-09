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
        //public static FECAERequest ToFECAERequest(FacturaAFIP facturaAFIP)
        //{

        //    var detalle = facturaAFIP.Detalle.First();
        //    var facturaFinal = new FECAERequest
        //    {
        //        FeCabReq = new FECAECabRequest
        //        {
        //            CantReg = facturaAFIP.Detalle.Count,
        //            CbteTipo = facturaAFIP.Cabecera.TipoComprobante.Id,
        //            PtoVta = facturaAFIP.Cabecera.PuntoVenta,
        //        },
        //        FeDetReq = new List<FECAEDetRequest>()
        //        {
        //            new FECAEDetRequest
        //            {
        //                Concepto = detalle.Concepto,
        //                DocTipo = detalle.TipoDocumento.Id,
        //                DocNro = detalle.NroDocumento,
        //                CbteDesde = detalle.NroComprobanteDesde,
        //                CbteHasta = detalle.NroComprobanteHasta,
        //                CbteFch = detalle.FechaComprobante?.ToAFIPDateString(),
        //                ImpTotal = detalle.ImporteTotal,
        //                ImpTotConc = detalle.ImporteTotalConc,
        //                ImpNeto = detalle.ImporteNeto,
        //                ImpOpEx = detalle.ImporteOpExento,
        //                ImpIVA = detalle.ImporteIVA,
        //                ImpTrib = detalle.ImporteTributos,
        //                FchServDesde = detalle.FechaServicioDesde?.ToAFIPDateString(),
        //                FchServHasta = detalle.FechaServicioHasta?.ToAFIPDateString(),
        //                FchVtoPago = detalle.FechaVencimientoPago?.ToAFIPDateString(),
        //                MonId = detalle.Moneda.Id,
        //                MonCotiz = detalle.CotizacionMoneda,
        //                CondicionIVAReceptorId = detalle.CondicionIVAReceptorId
        //            }
        //        }
        //    };

        //    if (facturaAFIP.ComprobanteAsociado != null)
        //    {
        //        facturaFinal.FeDetReq.First().CbtesAsoc = new List<CbteAsoc>()
        //        {
        //            new CbteAsoc()
        //            {
        //                Tipo = facturaAFIP.ComprobanteAsociado.TipoComprobante,
        //                Nro = facturaAFIP.ComprobanteAsociado.NroComprobante,
        //                PtoVta = facturaAFIP.ComprobanteAsociado.PuntoVenta,
        //                CbteFch = facturaAFIP.ComprobanteAsociado.FechaComprobante?.ToAFIPDateString(),
        //                Cuit = facturaAFIP.ComprobanteAsociado.Cuil?.ToString()
        //            }
        //        };
        //    }

        //    if (detalle.ImporteIVA > 0)
        //    {
        //        facturaFinal.FeDetReq.First().Iva = new List<AlicIva>
        //                {
        //                    new AlicIva
        //                    {
        //                        Id = 5,
        //                        BaseImp = detalle.ImporteNeto,
        //                        Importe = detalle.ImporteIVA
        //                    }
        //                };
        //    }

        //    return facturaFinal;
        //}

        public static FECAERequest ToFECAERequest(FacturaAFIP facturaAFIP)
        {
            var facturaFinal = new FECAERequest
            {
                FeCabReq = new FECAECabRequest
                {
                    CantReg = facturaAFIP.Detalle.Count,
                    CbteTipo = facturaAFIP.Cabecera.TipoComprobante.Id,
                    PtoVta = facturaAFIP.Cabecera.PuntoVenta,
                }
            };

            var listFeDetReq = new List<FECAEDetRequest>();
            foreach (var detalle in facturaAFIP.Detalle)
            {
                var iva = new AlicIva
                {
                    Id = (int)detalle.ImporteIva.TipoIva,
                    BaseImp = detalle.ImporteIva.ImporteNeto,
                    Importe = detalle.ImporteIva.ImporteIVA
                };

                var detalleProduct = new FECAEDetRequest
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
                    MonCotiz = detalle.CotizacionMoneda,
                    CondicionIVAReceptorId = detalle.CondicionIVAReceptorId,
                    Iva = new List<AlicIva>() { iva }
                };
                listFeDetReq.Add(detalleProduct);
            }
            facturaFinal.FeDetReq = listFeDetReq;


            if (facturaAFIP.ComprobanteAsociado != null)
            {
                facturaFinal.FeDetReq.First().CbtesAsoc = new List<CbteAsoc>()
                {
                    new CbteAsoc()
                    {
                        Tipo = facturaAFIP.ComprobanteAsociado.TipoComprobante,
                        Nro = facturaAFIP.ComprobanteAsociado.NroComprobante,
                        PtoVta = facturaAFIP.ComprobanteAsociado.PuntoVenta,
                        CbteFch = facturaAFIP.ComprobanteAsociado.FechaComprobante?.ToAFIPDateString(),
                        Cuit = facturaAFIP.ComprobanteAsociado.Cuil?.ToString()
                    }
                };
            }

            //if (facturaAFIP.ImportesIva.Any())
            //{
            //    var listaIva = new List<AlicIva>();
            //    foreach (var i in facturaAFIP.ImportesIva)
            //    {
            //        var ai = new AlicIva
            //        {
            //            Id = (int)i.TipoIva,
            //            BaseImp = i.ImporteNeto,
            //            Importe = i.ImporteIVA
            //        };
            //        listaIva.Add(ai);
            //    }
            //    facturaFinal.FeDetReq.First().Iva = listaIva;
            //}

            return facturaFinal;
        }

        public static FECAEARequest ToFECAERequest_CAEA(FacturaAFIP facturaAFIP)
        {

            var detalle = facturaAFIP.Detalle.First();
            var facturaFinal = new FECAEARequest
            {
                FeCabReq = new FECAEACabRequest
                {
                    CantReg = facturaAFIP.Detalle.Count,
                    CbteTipo = facturaAFIP.Cabecera.TipoComprobante.Id,
                    PtoVta = facturaAFIP.Cabecera.PuntoVenta,
                },
                FeDetReq = new List<FECAEADetRequest>()
                {
                    new FECAEADetRequest
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
                        MonCotiz = detalle.CotizacionMoneda,
                        CondicionIVAReceptorId = detalle.CondicionIVAReceptorId
                    }
                }
            };

            if (facturaAFIP.ComprobanteAsociado != null)
            {
                facturaFinal.FeDetReq.First().CbtesAsoc = new List<CbteAsoc>()
                {
                    new CbteAsoc()
                    {
                        Tipo = facturaAFIP.ComprobanteAsociado.TipoComprobante,
                        Nro = facturaAFIP.ComprobanteAsociado.NroComprobante,
                        PtoVta = facturaAFIP.ComprobanteAsociado.PuntoVenta,
                        CbteFch = facturaAFIP.ComprobanteAsociado.FechaComprobante?.ToAFIPDateString(),
                        Cuit = facturaAFIP.ComprobanteAsociado.Cuil?.ToString()
                    }
                };
            }

            //if (facturaAFIP.ImportesIva.Any())
            //{
            //    foreach (var i in facturaAFIP.ImportesIva)
            //    {
            //        var ai = new AlicIva
            //        {
            //            Id = (int)i.TipoIva,
            //            BaseImp = i.ImporteNeto,
            //            Importe = i.ImporteIVA
            //        };

            //        facturaFinal.FeDetReq.First().Iva.Add(ai);
            //    }
            //}

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
                NroDocumento = fECAECabResponse.DocNro,
                TipoDocumentoId = fECAECabResponse.DocTipo,
                TipoDocumento = TipoDocumento.GetById(fECAECabResponse.DocTipo).Description,
                Resultado = fECAECabResponse.Resultado,
                RegistrationDate = TimeHelper.GetArgentinaTime()
            };

            if (fECAECabResponse.Observaciones != null && fECAECabResponse.Observaciones.Any())
            {
                facturaRecibida.Observaciones = string.Join(Environment.NewLine, fECAECabResponse.Observaciones.Select(_ => _.Msg));
            }

            return facturaRecibida;
        }


    }
}
