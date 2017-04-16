using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PreparePlanetData
{

	public class Texture
	{
		Bitmap bitmap;
		BitmapData data;

		public int Height;
		public int Width;

		int HeightMinusOne;
		int WidthMinusOne;

		string name = "unnamed";

		private Texture(Bitmap bitmap)
		{
			this.bitmap = bitmap;
			LoadDimensions();
			LockBits();
		}

		void LoadDimensions()
		{
			this.Height = bitmap.Height;
			this.Width = bitmap.Width;
			this.HeightMinusOne = bitmap.Height - 1;
			this.WidthMinusOne = bitmap.Width - 1;
		}

		public Texture(int width, int height) : this(new Bitmap((int)width, (int)height, PixelFormat.Format32bppArgb))
		{
		}


		public static Texture LoadTexture(string filePath)
		{
			Console.WriteLine("loading " + filePath);
			var bitmap = (Bitmap)Image.FromFile(filePath, true);
			bitmap = bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), PixelFormat.Format32bppArgb);
			Console.WriteLine("loaded " + filePath);
			var t = new Texture(bitmap);
			t.name = filePath;
			return t;
		}

		private void LockBits()
		{
			if (data != null) return;
			data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadWrite, bitmap.PixelFormat);
		}
		public void UnlockBits()
		{
			if (data == null) return;
			bitmap.UnlockBits(data);
			data = null;
		}

		/// <summary>
		/// Help from http://stackoverflow.com/questions/7433508/c-sharp-bitmap-using-unsafe-code
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="c"></param>
		private unsafe void SetPixel(int x, int y, Color c)
		{
			x %= Width;
			y %= Height;

			byte* d =
				(byte*)data.Scan0.ToPointer() +
				data.Stride * y +
				4 * x;

			d[0] = c.B;
			d[1] = c.G;
			d[2] = c.R;
			d[3] = c.A;
		}

		public Color GetPixel(float u, float v)
		{
			return GetPixel(
				(int)Math.Round(WidthMinusOne * u),
				(int)Math.Round(HeightMinusOne * v)
			);
		}

		public unsafe Color GetPixel(int x, int y)
		{
			x %= Width;
			y %= Height;
			if (x < 0) x = 0;
			//else if (x > WidthMinusOne) x = WidthMinusOne;
			if (y < 0) y = 0;
			//else if (y > WidthMinusOne) y = WidthMinusOne;

			byte* d =
				(byte*)data.Scan0.ToPointer() +
				data.Stride * y +
				4 * x;

			return Color.FromArgb(
				d[3],
				d[2],
				d[1],
				d[0]
			);
		}


		public Image CloneImage()
		{
			UnlockBits();
			var bmp = (Bitmap)this.bitmap.Clone();
			LockBits();
			return bmp;
		}
		public Vector4 GetTexel(int x, int y)
		{
			Color c;
			c = GetPixel(x, y);
			return new Vector4(
				c.R / 255.0f,
				c.G / 255.0f,
				c.B / 255.0f,
				c.A / 255.0f
			);
		}

		public Vector4 GetTexel(float u, float v)
		{
			Color c;
			c = GetPixel(
				(int)Math.Round(WidthMinusOne * u),
				(int)Math.Round(HeightMinusOne * v)
			);
			return new Vector4(
				c.R / 255.0f,
				c.G / 255.0f,
				c.B / 255.0f,
				c.A / 255.0f
			);
		}

		public void SetAt(float u, float v, Vector4 color)
		{
			SetAt(
				(uint)Math.Round(WidthMinusOne * u),
				(uint)Math.Round(HeightMinusOne * v),
				color
			);
		}
		public void SetAt(float u, float v, Vector3 color)
		{
			SetAt(
				(int)Math.Round(WidthMinusOne * u),
				(int)Math.Round(HeightMinusOne * v),
				color
			);
		}
		public void SetAt(float u, float v, Color color)
		{
			SetAt(
				(int)Math.Round(WidthMinusOne * u),
				(int)Math.Round(HeightMinusOne * v),
				color
			);
		}


		public void SetAt(int x, int y, Vector4 color)
		{
			if (color.x > 1) color.x = 1;
			if (color.y > 1) color.y = 1;
			if (color.z > 1) color.z = 1;
			if (color.w > 1) color.w = 1;

			if (color.x < 0) color.x = 0;
			if (color.y < 0) color.y = 0;
			if (color.z < 0) color.z = 0;
			if (color.w < 0) color.w = 0;

			var c = Color.FromArgb(
				(int)Math.Round(255 * color.w),
				(int)Math.Round(255 * color.x),
				(int)Math.Round(255 * color.y),
				(int)Math.Round(255 * color.z)
			);
			SetPixel(
				x,
				y,
				c
			);
		}

		public void SetAt(int x, int y, Vector3 color)
		{
			if (color.x > 1) color.x = 1;
			if (color.y > 1) color.y = 1;
			if (color.z > 1) color.z = 1;

			if (color.x < 0) color.x = 0;
			if (color.y < 0) color.y = 0;
			if (color.z < 0) color.z = 0;

			var c = Color.FromArgb(
				255,
				(int)Math.Round(255 * color.x),
				(int)Math.Round(255 * color.y),
				(int)Math.Round(255 * color.z)
			);
			SetPixel(
				x,
				y,
				c
			);
		}

		public void SetAt(int x, int y, Color color)
		{
			SetPixel(
				x,
				y,
				color
			);
		}

		public void SetAt(int x, int y, ushort channel, float value)
		{
			var c = GetTexel(x, y);

			if (channel == 0) c.x = value;
			else if (channel == 1) c.y = value;
			else if (channel == 2) c.z = value;
			else if (channel == 3) c.w = value;

			SetAt(
				x,
				y,
				c
			);
		}

		public void Save(string filePath)
		{
			Console.WriteLine("saving " + filePath);
			UnlockBits();
			bitmap.Save(filePath);
			LockBits();
			Console.WriteLine("saved " + filePath);
		}


		private ImageCodecInfo GetEncoder(ImageFormat format)
		{
			ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
			foreach (ImageCodecInfo codec in codecs)
			{
				if (codec.FormatID == format.Guid)
				{
					return codec;
				}
			}
			return null;
		}
		public void SaveAsJPG(string filePath, int qualityPercent)
		{
			Console.WriteLine("saving as jpg" + filePath);
			ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);

			// Create an Encoder object based on the GUID
			// for the Quality parameter category.
			System.Drawing.Imaging.Encoder myEncoder =
				System.Drawing.Imaging.Encoder.Quality;

			// Create an EncoderParameters object.
			// An EncoderParameters object has an array of EncoderParameter
			// objects. In this case, there is only one
			// EncoderParameter object in the array.
			EncoderParameters myEncoderParameters = new EncoderParameters(1);

			EncoderParameter myEncoderParameter = new EncoderParameter(myEncoder, qualityPercent);
			myEncoderParameters.Param[0] = myEncoderParameter;

			bitmap.Save(filePath, jgpEncoder, myEncoderParameters);
			Console.WriteLine("saved as jpg " + filePath);
		}

		public void Resize(int newWidth, int newHeight)
		{
			Console.WriteLine("resizing " + name + " to " + newWidth + "x" + newHeight + " from " + Width + "x" + Height);

			UnlockBits();

			float ratio = 1;
			float minSize = Math.Min(newHeight, newHeight);

			if (bitmap.Width > bitmap.Height)
			{
				ratio = minSize / (float)bitmap.Width;
			}
			else
			{
				ratio = minSize / (float)bitmap.Height;
			}

			Bitmap newBitmap = new Bitmap((int)(bitmap.Width * ratio), (int)(bitmap.Height * ratio));

			using (Graphics graphics = Graphics.FromImage(newBitmap))
			{
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.CompositingMode = CompositingMode.SourceCopy;
				graphics.DrawImage(bitmap, 0, 0, newBitmap.Width, newBitmap.Height);
			}

			bitmap = newBitmap;

			LoadDimensions();
			LockBits();

			Console.WriteLine("resized " + name);
		}
	}

}