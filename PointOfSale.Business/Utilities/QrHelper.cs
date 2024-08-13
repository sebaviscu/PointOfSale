using System.Drawing;
using System.Text;
using ZXing.QrCode;
using ZXing;
using ZXing.Windows.Compatibility;
using System.Drawing.Imaging;
using static QRCoder.PayloadGenerator;

namespace PointOfSale.Business.Utilities
{
    public class QrHelper
    {
        public static string GenerarQR(string link, string idSale)
        {
            using (var ms = new MemoryStream())
            {
                // Configuración del QR
                QrCodeEncodingOptions options = new QrCodeEncodingOptions
                {
                    DisableECI = true,
                    CharacterSet = "UTF-8",
                    Width = 70,  // Ajustar tamaño según necesidad
                    Height = 70
                };

                BarcodeWriter writer = new BarcodeWriter
                {
                    Format = BarcodeFormat.QR_CODE,
                    Options = options
                };

                using (Bitmap qrCodeBitmap = writer.Write(link))
                {
                    qrCodeBitmap.Save(ms, ImageFormat.Png);
                    byte[] imageBytes = ms.ToArray();
                    return Convert.ToBase64String(imageBytes);
                }
            }
        }

    }
}
