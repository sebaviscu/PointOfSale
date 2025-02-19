﻿using AFIP.Facturacion.Model;
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
        public List<ImporteIva> ImportesIva { get; set; } = new List<ImporteIva>();
        public int IdFacturaEmitida { get; set; }
        public FacturaAFIP() { }

        /// <summary>
        /// Para Facturar
        /// </summary>
        /// <param name="sale"></param>
        /// <param name="tipoComprobante"></param>
        /// <param name="nroComprobante"></param>
        /// <param name="ptoVenta"></param>
        /// <param name="documento"></param>
        public FacturaAFIP(List<DetailSale> detailSale, decimal importeVenta, DateTime fechaComprobante, TipoComprobante tipoComprobante, int? nroComprobante, int ptoVenta, long documento)
        {
            Cabecera = CrearCabecera(tipoComprobante, ptoVenta);
            var tipoDocumento = ObtenerTipoDocumento(tipoComprobante, documento);
            AgregarDetalle(detailSale, importeVenta, fechaComprobante, nroComprobante, documento, tipoDocumento, tipoComprobante);
        }

        /// <summary>
        /// Para Notas de Credito
        /// </summary>
        /// <param name="tipoComprobante"></param>
        /// <param name="nroComprobante"></param>
        /// <param name="facturaEmitida"></param>
        public FacturaAFIP(List<DetailSale> detailSale, decimal importeVenta, TipoComprobante tipoComprobante, int? nroComprobante, FacturaEmitida facturaEmitida, long documento, bool isNotaCredito)
        {
            IdFacturaEmitida = facturaEmitida.IdFacturaEmitida;

            Cabecera = CrearCabecera(tipoComprobante, facturaEmitida.PuntoVenta);
            if (isNotaCredito)
                ComprobanteAsociado = CrearComprobanteAsociado(tipoComprobante, facturaEmitida);

            var tipoDocumento = ObtenerTipoDocumento(tipoComprobante, documento);
            AgregarDetalle(detailSale, importeVenta, TimeHelper.GetArgentinaTime(), nroComprobante, documento, tipoDocumento, tipoComprobante);
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

        private ImporteIva CalcularImportes(IGrouping<decimal?, DetailSale> detalle, decimal importeVenta, TipoComprobante tipoComprobante)
        {
            var importeIva = new ImporteIva();
            if (tipoComprobante.Id != TipoComprobante.Factura_C.Id)
            {
                var tipoIva = IVA_Afip.IVA_21;
                switch (detalle.Key.Value)
                {
                    case 0M:
                        tipoIva = IVA_Afip.IVA_0;
                        break;
                    case 10.5M:
                        tipoIva = IVA_Afip.IVA_105;
                        break;
                    case 21M:
                        tipoIva = IVA_Afip.IVA_21;
                        break;
                    case 27M:
                        tipoIva = IVA_Afip.IVA_27;
                        break;
                }


                //if (detalle.Sum(_ => _.Total) == importeVenta) // si se divide pago
                //{
                var iva = detalle.Key.Value;
                var totalIva = Math.Truncate((importeVenta - (importeVenta / (1 + (iva / 100)))) * 100) / 100;

                double importeIVA = (double)totalIva;
                double importeNeto = (double)(importeVenta - totalIva);
                double importeTotal = (double)importeVenta;
                //}
                //else
                //{
                //    importeIVA = (double)detalle.Sum(_ => _.ImporteIva);
                //    importeNeto = (double)detalle.Sum(_ => _.ImporteNeto);
                //    importeTotal = (double)detalle.Sum(_ => _.Total);
                //}

                importeIva.TipoIva = tipoIva;
                importeIva.ImporteIVA = importeIVA;
                importeIva.ImporteNeto = importeNeto;

                ImportesIva.Add(importeIva);
            }
            else
            {
                importeIva.TipoIva = IVA_Afip.IVA_0;
                importeIva.ImporteIVA = 0;
                importeIva.ImporteNeto = (double)detalle.Sum(_ => _.Total);
            }

            return importeIva;
        }

        private TipoDocumento ObtenerTipoDocumento(TipoComprobante tipoComprobante, long documento)
        {
            if (tipoComprobante.Id == TipoComprobante.Factura_C.Id || tipoComprobante.Id == TipoComprobante.Factura_B.Id || tipoComprobante.Id == TipoComprobante.NotaCredito_B.Id)
            {
                if (documento == 0)
                {
                    return TipoDocumento.DocOtro;
                }
                else if (documento.ToString().Length == 8)
                {
                    return TipoDocumento.DNI;
                }
                else if (documento.ToString().Length == 11)
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

        private void AgregarDetalle(List<DetailSale> detailSale, decimal importeVenta, DateTime fechaComprobante, int? nroComp, long documento, TipoDocumento tipoDocumento, TipoComprobante tipoComprobante)
        {
            var listaDetalles = new List<DetalleFacturaAFIP>();

            var imporetsIva = detailSale.GroupBy(_ => _.Iva).ToList();
            int i = 0;
            // ver numero de comrpobante que onda
            var nroComprobante = nroComp != null ? (long)(nroComp + i) : (long)0;
            foreach (var tiposIVA in imporetsIva)
            {
                var importeTotalIVA = tiposIVA.Sum(_ => _.Total);
                var totalDetalle = detailSale.Sum(_ => _.Total);

                // Calcular la proporción del importe total de IVA en la venta parcial
                if (importeVenta != totalDetalle)
                {
                    // Calcular la proporción del importeTotalIVA en el importeVenta
                    var proporcion = importeVenta / totalDetalle;
                    importeTotalIVA = importeTotalIVA * proporcion;
                }

                var importeIva = CalcularImportes(tiposIVA, importeTotalIVA, tipoComprobante);

                var detalleNew = new DetalleFacturaAFIP
                {
                    Concepto = Concepto.Producto,
                    TipoDocumento = tipoDocumento,
                    NroDocumento = documento,
                    NroComprobanteDesde = nroComprobante,
                    NroComprobanteHasta = nroComprobante,
                    FechaComprobante = fechaComprobante,
                    ImporteTotalConc = 0,
                    ImporteNeto = importeIva.ImporteNeto,
                    ImporteOpExento = 0,
                    ImporteIVA = importeIva.ImporteIVA,
                    ImporteTributos = 0,
                    Moneda = TipoMoneda.PesoArgentino,
                    CotizacionMoneda = 1,
                    CondicionIVAReceptorId = 15, // 1 =>IVA Responsable Inscripto, 6 => Responsable Monotributo, 15 => IVA No Alcanzado
                    ImporteIvaObj = importeIva
                };
                listaDetalles.Add(detalleNew);
                i++;
            }

            Detalle = listaDetalles;
        }
    }

}
