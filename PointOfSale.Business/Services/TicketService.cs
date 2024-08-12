using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {
        private readonly IAfipService _afipService;

        public TicketService(IAfipService afipService)
        {
            _afipService = afipService;
        }

        public async Task<TicketModel> TicketSale(Sale sale, Ajustes ajustes, FacturaEmitida? facturaEmitida)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.DescuentoRecargo, facturaEmitida);
        }

        public async Task<TicketModel> TicketVentaWeb(VentaWeb sale, Ajustes ajustes, FacturaEmitida? facturaEmitida)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, null, facturaEmitida);
        }
        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }

        private async Task<TicketModel> CreateTicket(Ajustes ajustes, DateTime registrationDate, decimal total, ICollection<DetailSale> detailSales, decimal? descuentoRecargo, FacturaEmitida? facturaEmitida)
        {

            if (string.IsNullOrEmpty(ajustes.NombreImpresora))
            {
                return null;
            }
            var isFactura = facturaEmitida != null && string.IsNullOrEmpty(facturaEmitida.Errores);

            var Ticket1 = new TicketModel();

            Ticket1.TextoIzquierda("");

            Ticket1.TextoCentro(ajustes.NombreTiendaTicket.ToUpper());

            Ticket1.LineasGuion();

            if (isFactura)
            {
                Ticket1.TextoIzquierda($"{facturaEmitida.TipoFactura}");
                Ticket1.TextoIzquierda($"No Fac: {facturaEmitida.NumeroFacturaString}");
            }

            Ticket1.TextoIzquierda("Fecha: " + registrationDate.ToShortDateString() + "  Hora: " + registrationDate.ToShortTimeString());
            Ticket1.LineasGuion();
            Ticket1.TextoIzquierda("");

            foreach (var d in detailSales)
            {
                Ticket1.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price.Value,
                   d.Quantity.Value,
                   d.Total.Value);
            }

            Ticket1.TextoIzquierda(" ");

            if (descuentoRecargo != null && descuentoRecargo != 0)
            {
                if (descuentoRecargo > 0)
                {
                    Ticket1.TextoIzquierda(" Regargo: $" + descuentoRecargo);
                }
                else
                {
                    Ticket1.TextoIzquierda(" Descuento: $" + (descuentoRecargo * -1));
                }
                Ticket1.TextoIzquierda(" ");
            }

            Ticket1.LineasTotal();
            Ticket1.AgregaTotales("Total", double.Parse(total.ToString()));
            Ticket1.LineasTotal();

            Ticket1.TextoIzquierda(" ");
            Ticket1.TextoAgradecimiento("¡Gracias por su compra!");
            //Ticket1.TextoIzquierda(" ");


            if (isFactura)
            {
                // Generar y agregar el QR
                Ticket1.AgregarCAEInfo(facturaEmitida.CAE, facturaEmitida.CAEVencimiento.Value.ToShortDateString());
                var linkAfip = await _afipService.GenerateLinkAfipFactura(facturaEmitida);
                Ticket1.urlQr = QrHelper.GenerarQR(linkAfip, facturaEmitida.IdSale.ToString());
            }

            return Ticket1;
        }
    }
}
