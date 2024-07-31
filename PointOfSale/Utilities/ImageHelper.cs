using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;

namespace PointOfSale.Utilities
{
    public static class ImageHelper
    {
        public static byte[] ResizeImage(byte[] imageBytes, int width, int height)
        {
            using (var ms = new MemoryStream(imageBytes))
            {
                using (var originalImage = Image.FromStream(ms))
                {
                    var resizedImage = new Bitmap(width, height);
                    using (var graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.CompositingQuality = CompositingQuality.HighQuality;
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.HighQuality;

                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }

                    using (var resultStream = new MemoryStream())
                    {
                        resizedImage.Save(resultStream, ImageFormat.Png);
                        return resultStream.ToArray();
                    }
                }
            }
        }
    }
}
