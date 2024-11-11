using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TRPO_LABA_PK2_VAR2;

public interface IImageFilter
{
    Bitmap Apply(Bitmap original, string filename, string extension);

    public static int ClampColor(int value)
    {
        if (value > 255) return 255;
        if (value < 0) return 0;
        return value;
    }

}
