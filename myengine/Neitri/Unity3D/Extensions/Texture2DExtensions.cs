using UnityEngine;
using System.Collections;



namespace Neitri
{
	public static class Texture2DExtensions
	{

		public static Texture2D ResizeKeepData(this Texture2D t, int width, int height)
		{
			int oldW = t.width;
			int oldH = t.height;
			Color[] pixels = t.GetPixels();
			t.Resize(width, height);
			for (int x = 0; x < t.width; x++)
			{
				for (int y = 0; y < t.height; y++)
				{
					var color = pixels[
						(x / t.width * oldW) +
						(y / t.height * oldH) * oldW
					];
					t.SetPixel(x, y, color);
				}
			}
			t.Apply();
			return t;
		}

		private static Rect GetRectWidthHeight(this Texture2D text)
		{
			return new Rect(0, 0, text.width, text.height);
		}

		public static void CopyTo(this Texture2D source, Texture2D target, Rect targetRect)
		{
			source.CopyTo(target, targetRect, source.GetRectWidthHeight());
		}
		public static void CopyTo(this Texture2D source, Texture2D target, Rect targetRect, Rect sourceRect)
		{
			var xMin = Mathf.RoundToInt(targetRect.xMin);
			var xMax = Mathf.RoundToInt(targetRect.xMax);
			var yMin = Mathf.RoundToInt(targetRect.yMin);
			var yMax = Mathf.RoundToInt(targetRect.yMax);
			for (int x = xMin; x < xMax; x++)
			{
				for (int y = yMin; y < xMax; y++)
				{
					var u = sourceRect.xMin + x / xMax * sourceRect.width;
					var v = sourceRect.yMax + y / yMax * sourceRect.height;
					var c = source.GetPixelBilinear(u, v);
					target.SetPixel(x, y, c);
				}
			}
		}

	}

}