using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRPO_LABA_PK2_VAR2;

public class BlurFilter
{
    public Bitmap Apply(Bitmap original, string filename, string extension)
    {
        var processedImage = new Bitmap(original.Width, original.Height);
            using (var g = Graphics.FromImage(processedImage))
            {
                var rect = new Rectangle(0, 0, processedImage.Width, processedImage.Height);
                g.DrawImage(original, rect);
            }

            // Simple box blur implementation
            for (int x = 1; x < processedImage.Width - 1; x++)
            {
                for (int y = 1; y < processedImage.Height - 1; y++)
                {
                    var avgR = 0;
                    var avgG = 0;
                    var avgB = 0;

                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            var pixel = processedImage.GetPixel(x + i, y + j);
                            avgR += pixel.R;
                            avgG += pixel.G;
                            avgB += pixel.B;
                        }
                    }

                    avgR /= 9;
                    avgG /= 9;
                    avgB /= 9;

                 processedImage.SetPixel(x, y, Color.FromArgb(avgR, avgG, avgB));
                }
            }

        return processedImage;
    }

}
