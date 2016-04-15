using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.CompilerServices;

namespace MyEngine
{
    public static class SystemDrawingExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Color Blend(this Color a, Color other)
        {
            return Color.FromArgb(
                255,
                (byte)MyMath.Clamp(a.R + other.R * other.A/255.0f, 0, 255),
                (byte)MyMath.Clamp(a.G + other.G * other.A/255.0f, 0, 255),
                (byte)MyMath.Clamp(a.B + other.B * other.A/255.0f, 0, 255)
            );
        }
    }
}
