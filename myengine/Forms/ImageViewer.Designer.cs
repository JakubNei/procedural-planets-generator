using OpenTK;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace MyEngine
{
	partial class ImageViewer
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
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


		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.pictureBox1 = new System.Windows.Forms.PictureBox();
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
			this.SuspendLayout();
			// 
			// pictureBox1
			// 
			this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
			| System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.pictureBox1.Location = new System.Drawing.Point(0, 0);
			this.pictureBox1.Name = "pictureBox1";
			this.pictureBox1.Size = new System.Drawing.Size(727, 661);
			this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.pictureBox1.TabIndex = 0;
			this.pictureBox1.TabStop = false;
			// 
			// ImageViewer
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(726, 661);
			this.Controls.Add(this.pictureBox1);
			this.Name = "ImageViewer";
			this.Text = "Image Viewer";
			((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		public System.Windows.Forms.PictureBox pictureBox1;
	}
}

