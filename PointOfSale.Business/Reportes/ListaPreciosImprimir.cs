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
        static PdfFont fNombreProducto = PdfFontFactory.CreateFont(StandardFonts.COURIER);
        static PdfFont fPrecio = PdfFontFactory.CreateFont(StandardFonts.COURIER_BOLD);
        static PdfFont fKgUnidad = PdfFontFactory.CreateFont(StandardFonts.COURIER);
        static PdfFont fcodyfecha = PdfFontFactory.CreateFont(StandardFonts.COURIER);

        public static byte[] Imprimir(List<Product> lisProducts, bool codBarras, bool fechaModif)
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

                    var tableTitulo = CreateTable(p, codBarras, fechaModif);

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

        private static Table CreateTable(Product p, bool codBarras, bool fechaModif)
        {
            Table tableTitulo = new Table(UnitValue.CreatePercentArray(8))
                .SetWidth(UnitValue.CreatePercentValue(25));

            string descripcion = p.Description;
            if (descripcion.Length > 17)
            {
                descripcion = descripcion.Substring(0, 17) + ".";
            }

            Cell EtiquetaDescription = new Cell(1, 8)
                .Add(new Paragraph(descripcion).SetFont(fNombreProducto))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPaddingTop(6)
                .SetPaddingBottom(4)
                .SetBorderBottom(Border.NO_BORDER);

            Cell EtiquetaPrice = new Cell(1, 6)
                .Add(new Paragraph("$ " + p.Price.ToString()).SetFont(fPrecio))
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetPadding(5)
                .SetPaddingBottom(10)
                .SetBorderTop(Border.NO_BORDER)
                .SetBorderRight(Border.NO_BORDER);

            Cell EtiquetaTipoVenta = new Cell(1, 2)
                .Add(new Paragraph("/" + p.TipoVenta.ToString()).SetFont(fKgUnidad))
                .SetTextAlignment(TextAlignment.CENTER)
                .SetPadding(5)
                .SetPaddingBottom(10)
                .SetBorderTop(Border.NO_BORDER)
                .SetBorderLeft(Border.NO_BORDER);

            if (fechaModif && codBarras)
            {
                EtiquetaTipoVenta.SetBorderBottom(Border.NO_BORDER);
                EtiquetaPrice.SetBorderBottom(Border.NO_BORDER);
            }

            Cell EtiquetaBarCode = new Cell(1, 4).Add(new Paragraph(""));
            if (codBarras)
            {
                var codBar = p.CodigoBarras.FirstOrDefault();
                if (codBar != null)
                {
                    EtiquetaBarCode = new Cell(1, 4)
                        .Add(new Paragraph(codBar.Codigo).SetFont(fcodyfecha))
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetPaddingBottom(4)
                        .SetBorderTop(Border.NO_BORDER)
                        .SetBorderRight(Border.NO_BORDER);
                }
            }

            Cell EtiquetaModificationDate = new Cell(1, 4).Add(new Paragraph(""));
            if (fechaModif)
            {
                var modificationDate = p.ModificationDate.HasValue ? p.ModificationDate.Value.ToShortDateString() : string.Empty;
                EtiquetaModificationDate = new Cell(1, 4)
                    .Add(new Paragraph(modificationDate).SetFont(fcodyfecha))
                    .SetTextAlignment(TextAlignment.RIGHT)
                    .SetPaddingBottom(4)
                    .SetBorderTop(Border.NO_BORDER)
                    .SetBorderLeft(Border.NO_BORDER);
            }

            tableTitulo.AddCell(EtiquetaDescription);
            tableTitulo.AddCell(EtiquetaPrice);
            tableTitulo.AddCell(EtiquetaTipoVenta);

            if (fechaModif || codBarras)
            {
                tableTitulo.AddCell(EtiquetaBarCode);
                tableTitulo.AddCell(EtiquetaModificationDate);
            }

            return tableTitulo;
        }
    }

}