using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public static class ColorExtensions
	{
		public static Vector4 ToVector4(this Color c)
		{
			return new Vector4(c.R / 255.0f, c.G / 255.0f, c.B / 255.0f, c.A / 255.0f);
		}
	}
}
