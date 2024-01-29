using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using PointOfSale.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PointOfSale.Business.Reportes
{
    public class ListaPreciosImprimir
    {
        static Font fNombreProducto = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 25, Font.BOLD, BaseColor.BLACK);
        static Font fPrecio = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 20, Font.BOLD, BaseColor.BLACK);
        static Font fKgUnidad = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, Font.NORMAL, BaseColor.GRAY);
        static Font fcodyfecha = FontFactory.GetFont(FontFactory.HELVETICA, 6, Font.NORMAL, BaseColor.GRAY);

        public static void Imprimir(List<Product> lisProducts)
        {
            try
            {
                var pdfDoc = new iTextSharp.text.Document(PageSize.A4, 10f, 10f, 10f, 10f);
                string path = $"C:\\Users\\sebastian.viscusso\\Desktop\\etiqueta" + DateTime.Now.Ticks.ToString() + ".pdf";

                PdfWriter.GetInstance(pdfDoc, new FileStream(path, FileMode.OpenOrCreate));
                pdfDoc.Open();

                int i = 0;
                var cantVueltasPasadas = 0;

                var cantTotal = lisProducts.Count();
                var resto = cantTotal <= 4 ? cantTotal : cantTotal % 4;
                var cantVueltas = cantTotal / 4;

                PdfPTable outer = ResetRow(cantTotal);

                foreach (var p in lisProducts)
                {
                    i++;

                    var tableTitulo = CreateTable(p);

                    outer.AddCell(tableTitulo);

                    if (i == 4)
                    {
                        pdfDoc.Add(outer);
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
                        pdfDoc.Add(outer);
                    }

                }
                pdfDoc.Close();
            }

            catch (Exception ex)
            {

            }
        }

        private static PdfPTable ResetRow(int cantTotal)
        {
            PdfPTable outer;

            if (cantTotal >= 4)
            {
                outer = new PdfPTable(4)
                {
                    HorizontalAlignment = 0,
                    WidthPercentage = 100,
                };
            }
            else
            {
                outer = new PdfPTable(cantTotal)
                {
                    HorizontalAlignment = 0,
                    WidthPercentage = 25 * cantTotal,
                };
            }

            return outer;
        }

        private static PdfPTable CreateTable(Product p)
        {

            var tableTitulo = new PdfPTable(new[] { 1f, 1f, 1f, 1f, 1f, 1f })

            {
                HorizontalAlignment = 0,
                WidthPercentage = 25,
                DefaultCell = { MinimumHeight = 8f }
            };

            var EtiquetaDescription = new PdfPCell(new Phrase(p.Description, fNombreProducto))
            {
                Colspan = 6,
                HorizontalAlignment = 1,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingTop = 6,
                PaddingBottom = 4,
                BorderColorBottom = BaseColor.WHITE,
                BorderWidthBottom = 0.1f
            };

            var EtiquetaPrice = new PdfPCell(new Phrase("$ " + p.Price.ToString(), fPrecio))
            {
                Colspan = 4,
                HorizontalAlignment = 2,    //0=Izquierda, 1=Centro, 2=Derecha
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                BorderColorBottom = BaseColor.WHITE,
                BorderWidthBottom = 0.1f,
                BorderColorTop = BaseColor.WHITE,
                BorderWidthTop = 0.1f,
                BorderColorRight = BaseColor.WHITE,
                BorderWidthRight = 0.1f
            };

            var EtiquetaTipoVenta = new PdfPCell(new Phrase(" / " + p.TipoVenta.ToString(), fKgUnidad))
            {
                Colspan = 2,
                HorizontalAlignment = 1,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                Padding = 5,
                BorderColorBottom = BaseColor.WHITE,
                BorderWidthBottom = 0.1f,
                BorderColorTop = BaseColor.WHITE,
                BorderWidthTop = 0.1f,
                BorderColorLeft = BaseColor.WHITE,
                BorderWidthLeft = 0.1f
            };

            var EtiquetaBarCode = new PdfPCell(new Phrase(p.BarCode, fcodyfecha))
            {
                Colspan = 3,
                HorizontalAlignment = 0,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingBottom = 4,
                BorderColorTop = BaseColor.WHITE,
                BorderWidthTop = 0.1f,
                BorderColorRight = BaseColor.WHITE,
                BorderWidthRight = 0.1f
            };

            var modificationDate = p.ModificationDate.HasValue ? p.ModificationDate.Value.ToShortDateString() : string.Empty;

            var EtiquetaModificationDate = new PdfPCell(new Phrase(modificationDate, fcodyfecha))
            {
                Colspan = 3,
                HorizontalAlignment = 2,
                VerticalAlignment = Element.ALIGN_MIDDLE,
                PaddingBottom = 4,
                BorderColorTop = BaseColor.WHITE,
                BorderWidthTop = 0.1f,
                BorderColorLeft = BaseColor.WHITE,
                BorderWidthLeft = 0.1f
            };

            tableTitulo.AddCell(EtiquetaDescription);
            tableTitulo.AddCell(EtiquetaPrice);

            tableTitulo.AddCell(EtiquetaTipoVenta);
            tableTitulo.AddCell(EtiquetaBarCode);
            tableTitulo.AddCell(EtiquetaModificationDate);

            return tableTitulo;
        }
    }
}
