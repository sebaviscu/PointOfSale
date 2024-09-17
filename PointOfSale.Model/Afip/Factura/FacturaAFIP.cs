using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointOfSale.Model;
using AFIP.Facturacion.Model;
using PointOfSale.Business.Utilities;

namespace PointOfSale.Model.Afip.Factura
{
    /// <summary>
    /// Información del comprobante o lote de comprobantes de ingreso.Contiene los datos de FeCabReq y FeDetReq
    /// </summary>
    public class FacturaAFIP
    {
        public CabeceraFacturaAFIP Cabecera { get; set; }
        public List<DetalleFacturaAFIP> Detalle { get; set; } = new List<DetalleFacturaAFIP>();
        public ComprobanteAsociado ComprobanteAsociado { get; set; }

        public FacturaAFIP() { }

        /// <summary>
        /// Para Facturar
        /// </summary>
        /// <param name="sale"></param>
        /// <param name="tipoComprobante"></param>
        /// <param name="nroComprobante"></param>
        /// <param name="ptoVenta"></param>
        /// <param name="documento"></param>
        public FacturaAFIP(Sale sale, TipoComprobante tipoComprobante, int nroComprobante, int ptoVenta, long documento)
        {
            Cabecera = CrearCabecera(tipoComprobante, ptoVenta);
            var (importeNeto, importeIVA) = CalcularImportes(sale.Total.Value, tipoComprobante);
            var tipoDocumento = ObtenerTipoDocumento(tipoComprobante, documento);
            AgregarDetalle(sale.RegistrationDate.Value, nroComprobante, documento, tipoDocumento, importeNeto, importeIVA);
        }

        /// <summary>
        /// Para Notas de Credito
        /// </summary>
        /// <param name="tipoComprobante"></param>
        /// <param name="nroComprobante"></param>
        /// <param name="facturaEmitida"></param>
        public FacturaAFIP(TipoComprobante tipoComprobante, int nroComprobante, FacturaEmitida facturaEmitida)
        {
            Cabecera = CrearCabecera(tipoComprobante, facturaEmitida.PuntoVenta);
            ComprobanteAsociado = CrearComprobanteAsociado(tipoComprobante, facturaEmitida);
            var tipoDocumento = ObtenerTipoDocumento(tipoComprobante, facturaEmitida.NroDocumento);
            AgregarDetalle(TimeHelper.GetArgentinaTime(), nroComprobante, facturaEmitida.NroDocumento, tipoDocumento, facturaEmitida.ImporteNeto, facturaEmitida.ImporteIVA);
        }

        private CabeceraFacturaAFIP CrearCabecera(TipoComprobante tipoComprobante, int ptoVenta)
        {
            return new CabeceraFacturaAFIP
            {
                TipoComprobante = tipoComprobante,
                CantidadRegistros = 1,
                PuntoVenta = ptoVenta
            };
        }

        private (decimal, decimal) CalcularImportes(decimal total, TipoComprobante tipoComprobante)
        {
            decimal importeNeto = total;
            decimal importeIVA = 0;

            if (tipoComprobante.Id != TipoComprobante.Factura_C.Id)
            {
                importeNeto = Math.Truncate(total / 1.21m * 100) / 100;
                importeIVA = Math.Truncate((total - importeNeto) * 100) / 100;

                if (importeNeto + importeIVA != total)
                {
                    importeIVA = total - importeNeto;
                }
            }

            return (importeNeto, importeIVA);
        }

        private TipoDocumento ObtenerTipoDocumento(TipoComprobante tipoComprobante, long documento)
        {
            if (tipoComprobante.Id == TipoComprobante.Factura_C.Id || tipoComprobante.Id == TipoComprobante.Factura_B.Id || tipoComprobante.Id == TipoComprobante.NotaCredito_B.Id)
            {
                if(documento == 0)
                {
                    return TipoDocumento.DocOtro;
                }
                else if(documento.ToString().Length == 8)
                {
                    return TipoDocumento.DNI;
                }
                else if(documento.ToString().Length == 11)
                {
                    return TipoDocumento.CUIL;
                }
                return TipoDocumento.DocOtro;
            }
            else if (tipoComprobante.Id == TipoComprobante.Factura_A.Id || tipoComprobante.Id == TipoComprobante.NotaCredito_A.Id)
            {
                return TipoDocumento.CUIT;
            }

            return TipoDocumento.DocOtro;
        }

        private ComprobanteAsociado CrearComprobanteAsociado(TipoComprobante tipoComprobante, FacturaEmitida facturaEmitida)
        {
            var tipoFactura = tipoComprobante.Id == TipoComprobante.NotaCredito_A.Id ? TipoComprobante.Factura_A : TipoComprobante.Factura_B;
            return new ComprobanteAsociado
            {
                NroComprobante = facturaEmitida.NroFactura.Value,
                PuntoVenta = facturaEmitida.PuntoVenta,
                TipoComprobante = tipoFactura.Id
            };
        }

        private void AgregarDetalle(DateTime fechaComprobante, int nroComprobante, long documento, TipoDocumento tipoDocumento, decimal importeNeto, decimal importeIVA)
        {
            var detalleNew = new DetalleFacturaAFIP
            {
                Concepto = Concepto.Producto,
                TipoDocumento = tipoDocumento,
                NroDocumento = documento,
                NroComprobanteDesde = nroComprobante,
                NroComprobanteHasta = nroComprobante,
                FechaComprobante = fechaComprobante,
                ImporteTotalConc = 0,
                ImporteNeto = (double)importeNeto,
                ImporteOpExento = 0,
                ImporteIVA = (double)importeIVA,
                ImporteTributos = 0,
                Moneda = TipoMoneda.PesoArgentino,
                CotizacionMoneda = 1
            };
            Detalle.Add(detalleNew);
        }
    }

}
