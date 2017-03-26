using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MyEngine
{
    public partial class ImageViewer : Form
    {
        public ImageViewer()
        {
            InitializeComponent();
        }



		public static Color ColorFromVector4(Vector4 color)
		{
			return Color.FromArgb(
				(byte)Math.Round(255 * color.W),
				(byte)Math.Round(255 * color.X),
				(byte)Math.Round(255 * color.Y),
				(byte)Math.Round(255 * color.Z)
			);
		}


		Bitmap NewBitMap(int width, int height)
		{
			return new Bitmap(width, height, PixelFormat.Format32bppArgb);
			/*
			// this caused exceptions, because we were modyfing existing bitmap which was at the same time being rendered
			// so the .net rendering was unable to lock the bitmap data for reading while we were writing to it
			var bitmap = pictureBox1.Image as Bitmap;
			if (bitmap == null || bitmap.Width != width || bitmap.Height != height || bitmap.PixelFormat != PixelFormat.Format32bppArgb)
			{
				bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
				pictureBox1.Image = bitmap;
			}
			return bitmap;
			*/
		}

		public ImageViewer SetData(int width, int height, Func<int, int, Vector4> getColor)
		{
			SetData(width, height, (x, y) => ColorFromVector4(getColor(x, y)));
			return this;
		}
		public ImageViewer SetData(int width, int height, Func<int, int, Color> getColor)
		{
			lock (this)
			{
				var bitmap = NewBitMap(width, height);
				var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
				for (int x = 0; x < width; x++)
					for (int y = 0; y < height; y++)
						SetPixel(data, x, y, getColor(x, y));
				bitmap.UnlockBits(data);
				pictureBox1.Image = bitmap;
				return this;
			}
		}
		public ImageViewer SetData(Color[,] colors)
		{
			lock (this)
			{
				var width = colors.GetLength(0);
				var height = colors.GetLength(1);
				var bitmap = NewBitMap(width, height);
				var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
				for (int x = 0; x < width; x++)
					for (int y = 0; y < height; y++)
						SetPixel(data, x, y, colors[x, y]);
				bitmap.UnlockBits(data);
				pictureBox1.Image = bitmap;
				return this;
			}
		}
		public ImageViewer SetImage(Image image)
		{
			pictureBox1.Image = image;
			return this;
		}

		public static ImageViewer ShowNew()
		{
			var viewer = new ImageViewer();
			viewer.Show();
			return viewer;
		}
		public static ImageViewer ShowNew(int width, int height, Func<int, int, Vector4> getColor)
		{
			return ShowNew(width, height, (x, y) => ColorFromVector4(getColor(x, y)));
		}

		public static ImageViewer ShowNew(int width, int height, Func<int, int, Color> getColor)
		{
			var viewer = new ImageViewer();
			viewer.SetData(width, height, getColor);
			viewer.Show();
			return viewer;
		}

		public static ImageViewer ShowNew(Color[,] colors)
		{
			var viewer = new ImageViewer();
			viewer.SetData(colors);
			viewer.Show();
			return viewer;
		}

		public static ImageViewer ShowNew(Image image)
		{
			var viewer = new ImageViewer();
			viewer.SetImage(image);
			viewer.Show();
			return viewer;
		}


		/// <summary>
		/// Help from http://stackoverflow.com/questions/7433508/c-sharp-bitmap-using-unsafe-code
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="c"></param>
		private static unsafe void SetPixel(BitmapData data, int x, int y, Color c)
		{
			byte* d =
				(byte*)data.Scan0.ToPointer() +
				data.Stride * y +
				4 * x;

			d[0] = c.B;
			d[1] = c.G;
			d[2] = c.R;
			d[3] = c.A;
		}

		private static unsafe Color GetPixel(BitmapData data, int x, int y)
		{
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

	}
}
