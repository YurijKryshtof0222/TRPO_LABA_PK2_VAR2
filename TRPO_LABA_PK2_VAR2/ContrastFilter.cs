using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRPO_LABA_PK2_VAR2;

public class ContrastFilter : IImageFilter
{
    public Bitmap Apply(Bitmap original, string filename, string extension)
    {
        var processedImage = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(processedImage))
            {
                var rect = new Rectangle(0, 0, processedImage.Width, processedImage.Height);
                g.DrawImage(original, rect);
            }

            float contrast = 1.5f; // Contrast factor

            for (int x = 0; x < processedImage.Width; x++)
            {
                for (int y = 0; y < processedImage.Height; y++)
                {
                    var pixel = processedImage.GetPixel(x, y);

                    int r = IImageFilter.ClampColor((int)((pixel.R - 128) * contrast + 128));
                    int g = IImageFilter.ClampColor((int)((pixel.G - 128) * contrast + 128));
                    int b = IImageFilter.ClampColor((int)((pixel.B - 128) * contrast + 128));

                    processedImage.SetPixel(x, y, Color.FromArgb(r, g, b));
                }
            }
            return processedImage;
    }
}
