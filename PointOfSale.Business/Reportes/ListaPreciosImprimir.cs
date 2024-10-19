using iText.Kernel.Pdf;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using PointOfSale.Model;
using iText.Layout.Borders;


namespace PointOfSale.Business.Reportes
{
    public class ListaPreciosImprimir
    {
        public static byte[] Imprimir(List<Product> lisProducts, bool codBarras, bool fechaModif, bool vencimiento)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                PdfWriter writer = new PdfWriter(ms);
                PdfDocument pdfDoc = new PdfDocument(writer);
                Document document = new Document(pdfDoc, PageSize.A4, false);

                int i = 0;
                int cantVueltasPasadas = 0;

                int cantTotal = lisProducts.Count;
                int resto = cantTotal <= 4 ? cantTotal : cantTotal % 4;
                int cantVueltas = cantTotal / 4;

                Table outer = ResetRow(cantTotal);

                foreach (var p in lisProducts)
                {
                    i++;

                    var tableTitulo = CreateTable(p, codBarras, fechaModif, vencimiento);

                    outer.AddCell(tableTitulo);

                    if (i == 4)
                    {
                        document.Add(outer);
                        outer = ResetRow(cantTotal);

                        i = 0;
                        cantVueltasPasadas++;

                        if (cantVueltasPasadas == cantVueltas && resto > 0)
                        {
                            outer = ResetRow(resto);
                        }
                    }
                    else if (cantVueltasPasadas == cantVueltas && resto == i)
                    {
                        document.Add(outer);
                    }
                }

                document.Close();

                return ms.ToArray();
            }
        }

        private static Table ResetRow(int cantTotal)
        {
            Table outer;

            if (cantTotal >= 4)
            {
                outer = new Table(UnitValue.CreatePercentArray(4)).UseAllAvailableWidth();
            }
            else
            {
                outer = new Table(UnitValue.CreatePercentArray(cantTotal)).SetWidth(UnitValue.CreatePercentValue(25 * cantTotal));
            }

            return outer;
        }
        private static Table CreateTable(Product p, bool codBarras, bool fechaModif, bool vencimiento)
        {
            PdfFont fNombreProducto = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont fPrecio = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
            PdfFont fKgUnidad = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            PdfFont fcodyfecha = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            Table tableTitulo = new Table(UnitValue.CreatePercentArray(new float[] { 70, 30 }))
                .SetWidth(UnitValue.CreatePercentValue(100)) // Ajustar al 100% de la celda
                .SetMargin(2) // Añadir margen para mejorar la separación
                .SetPadding(2);

            string descripcion = p.Description;
            if (descripcion.Length > 27)
            {
                descripcion = descripcion.Substring(0, 26) + ".";
            }

            // Nombre del producto
            Cell EtiquetaDescription = new Cell(1, 2)
                .Add(new Paragraph(descripcion.ToUpper()).SetFont(fNombreProducto))
                .SetFontSize(8)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(0)
                .SetBorder(Border.NO_BORDER);

            tableTitulo.AddCell(EtiquetaDescription);


            // Precio y Tipo de venta combinados en una sola celda
            var precio = (int)p.ListaPrecios[0].Precio;
            Paragraph priceAndType = new Paragraph()
                .Add(new Text("$ " + precio.ToString()).SetFont(fPrecio).SetFontSize(20))
                .Add(new Text(" /" + p.TipoVenta.ToString()).SetFont(fKgUnidad).SetFontSize(12));

            Cell EtiquetaPriceAndTipoVenta = new Cell(1, 2)
                .Add(priceAndType)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(0)
                .SetBorder(Border.NO_BORDER);

            tableTitulo.AddCell(EtiquetaPriceAndTipoVenta);


            // Crear una tabla para colocar el código de barras a la izquierda y la fecha a la derecha
            Table barCodeAndDateTable = new Table(UnitValue.CreatePercentArray(new float[] { 50, 50 }))
                .SetWidth(UnitValue.CreatePercentValue(100));

            Cell barCodeCell = new Cell().SetBorder(Border.NO_BORDER);
            if (codBarras)
            {
                var codBar = p.CodigoBarras.FirstOrDefault();
                if (codBar != null)
                {
                    barCodeCell.Add(new Paragraph(codBar.Codigo).SetFont(fcodyfecha).SetFontSize(7))
                               .SetTextAlignment(TextAlignment.LEFT);
                }
            }
            barCodeAndDateTable.AddCell(barCodeCell);

            Cell dateCell = new Cell().SetBorder(Border.NO_BORDER);
            if (fechaModif)
            {
                var modificationDate = p.ModificationDate.HasValue ? p.ModificationDate.Value.ToShortDateString() : string.Empty;
                if (!string.IsNullOrEmpty(modificationDate))
                {
                    dateCell.Add(new Paragraph(modificationDate).SetFont(fcodyfecha).SetFontSize(7))
                            .SetTextAlignment(TextAlignment.RIGHT);
                }
            }
            barCodeAndDateTable.AddCell(dateCell);

            if (codBarras || fechaModif)
            {
                Cell EtiquetaBarCodeAndDate = new Cell(1, 2)
                    .Add(barCodeAndDateTable)
                    .SetPadding(0)
                    .SetBorder(Border.NO_BORDER);

                tableTitulo.AddCell(EtiquetaBarCodeAndDate);
            }

            if (vencimiento)
            {
                Cell vtoCell = new Cell(1, 2)
                    .Add(new Paragraph("Vto: ").SetFont(fcodyfecha))
                    .SetFontSize(8)
                    .SetPaddingLeft(10)
                    .SetTextAlignment(TextAlignment.LEFT)
                    .SetBorder(Border.NO_BORDER);

                tableTitulo.AddCell(vtoCell);
            }

            return tableTitulo;
        }



    }

}