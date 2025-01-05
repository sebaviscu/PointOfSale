using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.IO.Image;
using System.IO;
using PointOfSale.Business.Contracts;
using PointOfSale.Business.Utilities;
using PointOfSale.Model;
using PointOfSale.Model.Afip.Factura;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using static PointOfSale.Model.Enum;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.Drawing;

namespace PointOfSale.Business.Services
{
    public class TicketService : ITicketService
    {
        private readonly IAfipService _afipService;
        private readonly IAjusteService _ajusteService;
        private readonly IMovimientoCajaService _movimientoCajaService;

        public TicketService(IAfipService afipService, IAjusteService ajusteService, IMovimientoCajaService movimientoCajaService)
        {
            _afipService = afipService;
            _ajusteService = ajusteService;
            _movimientoCajaService = movimientoCajaService;
        }

        public async Task<TicketModel> TicketSale(Sale sale, Ajustes ajustes, FacturaEmitida? facturaEmitida)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.IdTienda, sale.DescuentoRecargo, facturaEmitida);
        }

        public async Task<TicketModel> TicketSale(VentaWeb sale, Ajustes ajustes)
        {
            return await CreateTicket(ajustes, sale.RegistrationDate.Value, sale.Total.Value, sale.DetailSales, sale.IdTienda.Value, null, null);
        }

        public async Task<TicketModel> TicketTest(List<DetailSale> detailSales, Ajustes ajustes)
        {
            var total = detailSales.Any() ? detailSales.Sum(_ => _.Total) : 0m;
            return await CreateTicket(ajustes, TimeHelper.GetArgentinaTime(), total, detailSales, ajustes.IdTienda, null, null);
        }

        public void ImprimirTiket(string impresora, string line)
        {
            PrinterModel.SendStringToPrinter(impresora, line);
        }

        private async Task<TicketModel> CreateTicket(Ajustes ajustes, DateTime registrationDate, decimal total, ICollection<DetailSale> detailSales, int idTienda, decimal? descuentoRecargo, FacturaEmitida? facturaEmitida)
        {
            var isFactura = facturaEmitida != null && string.IsNullOrEmpty(facturaEmitida.Observaciones);

            var Ticket = new TicketModel();

            Ticket.TextoIzquierda("");
            Ticket.ChangeFont(10, FontStyle.Bold);
            if (!string.IsNullOrEmpty(ajustes.Encabezado1)) Ticket.TextoCentro(ajustes.Encabezado1);
            Ticket.ChangeFont(8, FontStyle.Bold);
            if (!string.IsNullOrEmpty(ajustes.Encabezado2)) Ticket.TextoCentro(ajustes.Encabezado2);
            if (!string.IsNullOrEmpty(ajustes.Encabezado3)) Ticket.TextoCentro(ajustes.Encabezado3);
            Ticket.TextoIzquierda("");


            Ticket.ChangeFont(7, FontStyle.Bold);
            Ticket.LineasGuion();

            if (isFactura)
            {
                await DatosFactura(idTienda, facturaEmitida, Ticket);
            }

            Ticket.TextoIzquierda($"Fecha: {registrationDate.ToShortDateString()}");
            Ticket.TextoIzquierda($"Hora: {registrationDate.ToShortTimeString()}");
            Ticket.LineasGuion();
            Ticket.TextoIzquierda("");

            IsMultiplesFormasPago(detailSales, total);

            var ivaAcumulado = 0m;

            foreach (var d in detailSales)
            {
                Ticket.AgregaArticulo(d.DescriptionProduct.ToUpper(),
                   d.Price,
                   d.Quantity,
                   d.Total,
                   d.Iva.Value == 0 ? 21.00m : d.Iva.Value);

                ivaAcumulado += d.ImporteIva;
            }

            Ticket.TextoIzquierda(" ");

            if (descuentoRecargo != null && descuentoRecargo != 0)
            {
                string label = descuentoRecargo > 0 ? "Recargo" : "Descuento";
                decimal amount = Math.Abs(descuentoRecargo.Value);

                Ticket.TextoIzquierda($" {label}: ${amount}");
                Ticket.TextoIzquierda(" ");
            }
            Ticket.LineasTotal();
            Ticket.AgregaTotales("Total", double.Parse(total.ToString()));
            Ticket.LineasTotal();

            Ticket.TextoBetween($"IVA Contenido", "$" + ivaAcumulado.ToString("N2"));
            Ticket.LineasGuion();


            Ticket.TextoIzquierda(" ");
            Ticket.TextoIzquierda(" ");
            if (!string.IsNullOrEmpty(ajustes.Pie1)) Ticket.TextoCentro(ajustes.Pie1);
            if (!string.IsNullOrEmpty(ajustes.Pie2)) Ticket.TextoCentro(ajustes.Pie2);
            if (!string.IsNullOrEmpty(ajustes.Pie3)) Ticket.TextoCentro(ajustes.Pie3);
            Ticket.TextoIzquierda(" ");

            if (isFactura)
            {
                Ticket.TextoIzquierda($"CAE: {facturaEmitida.CAE}");
                Ticket.TextoIzquierda($"Vto: {facturaEmitida.CAEVencimiento.Value.ToShortDateString()}");

                // Generar y agregar el QR
                var linkAfip = await _afipService.GenerateLinkAfipFactura(facturaEmitida);
                var qrBase64 = QrHelper.GenerarQR(linkAfip, facturaEmitida.IdSale.ToString());
                Ticket.InsertarImagen($"F_{facturaEmitida.CAE}", qrBase64);
                Ticket.TextoIzquierda(" ");
            }

            Ticket.TextoIzquierda(" ");

            return Ticket;
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

        public byte[] PdfTicket(string ticket, List<Images> ImagesTicket)
        {
            // Generar el PDF
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc);

                // Configurar la fuente "Courier New" con tamaño 8
                PdfFont courierFont = PdfFontFactory.CreateFont(StandardFonts.COURIER);

                // Agregar las líneas del ticket con la fuente "Courier New"
                string[] lines = ticket.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                foreach (var line in lines)
                {
                    document.Add(new Paragraph(line).SetFont(courierFont).SetFontSize(8));
                }

                // Agregar imágenes si existen
                if (ImagesTicket != null && ImagesTicket.Any())
                {
                    foreach (var imageTicket in ImagesTicket)
                    {
                        byte[] imageBytes = Convert.FromBase64String(imageTicket.ImageBase64);
                        iText.Layout.Element.Image img = new iText.Layout.Element.Image(ImageDataFactory.Create(imageBytes));
                        document.Add(img);
                    }
                }

                document.Close();

                return ms.ToArray();
            }
        }

        public async Task<TicketModel> CierreTurno(Turno turno, Dictionary<string, decimal> VentasRegistradas)
        {
            var ticket = new TicketModel();

            var movimientos = await _movimientoCajaService.GetMovimientoCajaByTurno(turno.IdTurno);

            decimal totalMovimientoEgreso = 0;
            decimal totalMovimientoIngreso = 0;
            foreach (var m in movimientos)
            {
                if (m.RazonMovimientoCaja.Tipo == TipoMovimientoCaja.Egreso)
                    totalMovimientoEgreso -= m.Importe;
                else
                    totalMovimientoIngreso += m.Importe;
            }

            ticket.TextoIzquierda("");
            ticket.TextoCentro($"CIERRE DE CAJA");
            ticket.TextoIzquierda("");
            ticket.TextoCentro($"{turno.FechaInicio.ToString("dddd", new CultureInfo("es-ES")).ToUpper()} {turno.FechaInicio.ToShortDateString()}");
            ticket.TextoIzquierda("");

            ticket.TextoIzquierda($"Hora Inicio: {turno.FechaInicio.ToShortTimeString()}");
            ticket.TextoIzquierda($"Hora Cierre: {turno.FechaInicio.ToShortTimeString()}");

            ticket.TextoIzquierda($"Usuario: {turno.ModificationUser.ToUpper()}");
            ticket.LineasGuion();

            if (turno.TotalInicioCaja > 0)
            {
                ticket.TextoIzquierda($"Inicio de Caja: {(int)turno.TotalInicioCaja}");
            }

            if (totalMovimientoEgreso != 0)
            {
                ticket.TextoIzquierda($"Egreso de Caja: {(int)totalMovimientoEgreso}");
            }

            if (totalMovimientoIngreso != 0)
            {
                ticket.TextoIzquierda($"Ingresos de Caja: {(int)totalMovimientoIngreso}");
            }

            ticket.TextoIzquierda("");
            foreach (var ventas in VentasRegistradas)
            {
                ticket.AgregaVentasCerrarTurno(ventas.Key, (int)ventas.Value);
            }
            ticket.TextoIzquierda("");

            ticket.BetweenCierreTurno("TOTAL Sistema:", (int)turno.TotalCierreCajaSistema.Value);
            ticket.BetweenCierreTurno("TOTAL Usuario:", (int)turno.TotalCierreCajaReal.Value);

            ticket.TextoIzquierda("");
            if (!string.IsNullOrEmpty(turno.ErroresCierreCaja))
            {
                ticket.LineasGuion();
                ticket.TextoIzquierda("Diferencias de Caja:");
                ticket.TextoIzquierda("");
                var diferenciasArray = turno.ErroresCierreCaja.Split(new string[] { "<br>" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var diferencia in diferenciasArray)
                {
                    var descripcionMatch = System.Text.RegularExpressions.Regex.Match(diferencia, @"'(.+?)'");
                    var montoMatch = System.Text.RegularExpressions.Regex.Match(diferencia, @"\$ (\d+)");

                    if (descripcionMatch.Success && montoMatch.Success)
                    {
                        // Extraer la descripción y el monto
                        var descripcion = descripcionMatch.Groups[1].Value;
                        var monto = Convert.ToInt32(montoMatch.Groups[1].Value);

                        // Usar ticket.FormatearTextoBetween con los valores extraídos
                        ticket.BetweenCierreTurno(descripcion, monto);
                    }
                }
            }

            if (!string.IsNullOrEmpty(turno.ObservacionesApertura) || !string.IsNullOrEmpty(turno.ObservacionesCierre))
            {
                ticket.LineasGuion();
            }

            if (!string.IsNullOrEmpty(turno.ObservacionesApertura))
            {
                ticket.TextoIzquierda("");
                ticket.TextoIzquierda($"Observaciones apertura:");
                ticket.TextoIzquierda(turno.ObservacionesApertura);
            }

            if (!string.IsNullOrEmpty(turno.ObservacionesCierre))
            {
                ticket.TextoIzquierda("");
                ticket.TextoIzquierda($"Observaciones cierre:");
                ticket.TextoIzquierda(turno.ObservacionesCierre);
            }

            return ticket;
        }
    }
}
