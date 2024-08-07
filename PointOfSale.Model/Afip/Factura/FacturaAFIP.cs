using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PointOfSale.Model;
using AFIP.Facturacion.Model;

namespace PointOfSale.Model.Afip.Factura
{
    /// <summary>
    /// Información del comprobante o lote de comprobantes de ingreso.Contiene los datos de FeCabReq y FeDetReq
    /// </summary>
    public class FacturaAFIP
    {
        public CabeceraFacturaAFIP Cabecera { get; set; }
        public List<DetalleFacturaAFIP> Detalle { get; set; }

        public FacturaAFIP()
        {
        }

        public FacturaAFIP(Sale sale, TipoComprobante tipoComprobante, int nroComprobante, int ptoVenta, int documento)
        {
            Cabecera = new CabeceraFacturaAFIP()
            {
                TipoComprobante = tipoComprobante,
                CantidadRegistros = 1,
                PuntoVenta = ptoVenta
            };

            Detalle = new List<DetalleFacturaAFIP>();

            var tipoDocumento = TipoDocumento.CUIL;
            decimal importeNeto = sale.Total.Value;
            decimal importeIVA = 0;

            if (tipoComprobante.Id != TipoComprobante.Factura_C.Id)
            {
                decimal total = sale.Total.Value;
                importeNeto = Math.Truncate(total / 1.21m * 100) / 100; // Calcular el importe neto sin IVA y truncar a dos decimales
                importeIVA = Math.Truncate((total - importeNeto) * 100) / 100; // Calcular el importe del IVA y truncar a dos decimales

                // Ajustar el importeIVA para asegurar que la suma sea igual al total
                if (importeNeto + importeIVA != total)
                {
                    importeIVA = total - importeNeto;
                }
            }
            if(tipoComprobante.Id == TipoComprobante.Factura_C.Id || tipoComprobante.Id == TipoComprobante.Factura_B.Id)
            {
                // si es B o C, tiene que ser DNI
                // si el importe es menor a mucha plata, poner el dni en 0 y poer otros
                tipoDocumento = documento == 0 ? TipoDocumento.DocOtro : TipoDocumento.DNI;

            } else if (tipoComprobante.Id == TipoComprobante.Factura_A.Id)
            {
                // si es A, tiene que ser CUIT y NroDocumento no puede ser igual al del emisor
                tipoDocumento = TipoDocumento.CUIT;
            }

            var detalleNew = new DetalleFacturaAFIP()
            {
                Concepto = Concepto.Producto,
                TipoDocumento = tipoDocumento,
                NroDocumento = documento, // Número de documento del comprador (0 consumidor final) 
                NroComprobanteDesde = nroComprobante,
                NroComprobanteHasta = nroComprobante,
                FechaComprobante = sale.RegistrationDate,
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
