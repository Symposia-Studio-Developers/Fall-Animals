using ImageMagick;

namespace Helpers
{
    public class Webp2Png
    {
        public static void ConvertWebp2Png(string WebpPath, string PngPath)
        {
            using var image = new MagickImage(WebpPath);
            image.Write(PngPath, MagickFormat.Png);
        }
    }
}