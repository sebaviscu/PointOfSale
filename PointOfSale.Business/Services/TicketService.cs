using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {
        private readonly IAfipService _afipService;
        private readonly IAjusteService _ajusteService;

        public TicketService(IAfipService afipService, IAjusteService ajusteService)
        {
            _afipService = afipService;
            _ajusteService = ajusteService;
        }

        public async Task<TicketModel> TicketSale(Sale sale, Ajustes ajustes, FacturaEmitida? facturaEmitida)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.IdTienda, sale.DescuentoRecargo, facturaEmitida);
        }

        public async Task<TicketModel> TicketVentaWeb(VentaWeb sale, Ajustes ajustes, FacturaEmitida? facturaEmitida)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.IdTienda.Value, null, facturaEmitida);
        }
        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }

        private async Task<TicketModel> CreateTicket(Ajustes ajustes, DateTime registrationDate, decimal total, ICollection<DetailSale> detailSales, int idTienda, decimal? descuentoRecargo, FacturaEmitida? facturaEmitida)
        {

            if (string.IsNullOrEmpty(ajustes.NombreImpresora))
            {
                return null;
            }
            var isFactura = facturaEmitida != null && string.IsNullOrEmpty(facturaEmitida.Observaciones);

            var Ticket1 = new TicketModel();

            Ticket1.TextoIzquierda("");

            Ticket1.TextoCentro(ajustes.NombreTiendaTicket.ToUpper());

            Ticket1.LineasGuion();

            if (isFactura)
            {
                await DatosFactura(idTienda, facturaEmitida, Ticket1);
            }

            Ticket1.TextoIzquierda($"Fecha: {registrationDate.ToShortDateString()}");
            Ticket1.TextoIzquierda($"Hora: {registrationDate.ToShortTimeString()}");
            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("");

            IsMultiplesFormasPago(detailSales, total);

            foreach (var d in detailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price.Value,
                   d.Quantity.Value,
                   d.Total.Value,
                   21m);
            }

            Ticket1.TextoIzquierda(" ");

            if (descuentoRecargo != null && descuentoRecargo != 0)
            {
                string label = descuentoRecargo > 0 ? "Recargo" : "Descuento";
                decimal amount = Math.Abs(descuentoRecargo.Value);

                Ticket1.TextoIzquierda($" {label}: ${amount}");
                Ticket1.TextoIzquierda(" ");
            }

            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(total.ToString()));
            Ticket1.LineasTotal();

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoAgradecimiento("¡Gracias por su compra!");
            Ticket1.TextoIzquierda(" ");


            if (isFactura)
            {
                Ticket1.TextoIzquierda($"CAE: {facturaEmitida.CAE}");
                Ticket1.TextoIzquierda($"Vto: {facturaEmitida.CAEVencimiento.Value.ToShortDateString()}");

                // Generar y agregar el QR
                var linkAfip = await _afipService.GenerateLinkAfipFactura(facturaEmitida);
                var qrBase64 = QrHelper.GenerarQR(linkAfip, facturaEmitida.IdSale.ToString());
                Ticket1.InsertarImagen($"F_{facturaEmitida.CAE}", qrBase64);
            }

            return Ticket1;
        }

        private async Task DatosFactura(int idTienda, FacturaEmitida? facturaEmitida, TicketModel Ticket1)
        {
            var ajustesFactura = await _ajusteService.GetAjustesFacturacion(idTienda);
            Ticket1.TextoIzquierda($"{ajustesFactura.NombreTitular}");
            Ticket1.TextoIzquierda($"{ajustesFactura.CuitString}");

            if (facturaEmitida.TipoFactura == "Factura A")
            {
                Ticket1.TextoIzquierda($"IIBB: {ajustesFactura.IngresosBurutosNro}");
                Ticket1.TextoIzquierda($"Inicio actividad: {ajustesFactura.FechaInicioActividad}");
                Ticket1.TextoIzquierda($"{ajustesFactura.DireccionFacturacion}");
            }

            Ticket1.TextoIzquierda($"{ajustesFactura.CondicionIva.ToString()}");

            Ticket1.LineasGuion();

            Ticket1.TextoIzquierda($"{facturaEmitida.TipoFactura}");
            Ticket1.TextoIzquierda($"No Fac: {facturaEmitida.NumeroFacturaString}");
        }

        private void IsMultiplesFormasPago(ICollection<DetailSale> detailSales, decimal total)
        {
            if (detailSales.Count == 0)
            {
                var d = new DetailSale()
                {
                    DescriptionProduct = "Productos",
                    Price = total,
                    Quantity = 1,
                    Total = total
                };
                detailSales.Add(d);
            }

            var totDetailsSale = detailSales.Sum(_ => _.Total);

            if (totDetailsSale != total)
            {
                var d = new DetailSale()
                {
                    DescriptionProduct = "Otros",
                    Price = total - totDetailsSale,
                    Quantity = 1,
                    Total = total - totDetailsSale
                };
                detailSales.Add(d);
            }
        }
    }
}
