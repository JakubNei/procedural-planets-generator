using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace MyEngine
{
	public class Texture2D : Texture, IDisposable
	{
		public bool IsOnGpu { get; private set; }
		public bool WantsToBeUploadedToGpu { get; private set; }
		public bool KeepLocalCopyOfTexture { get; set; }

		public int Width { get { return bmp.Width; } }
		public int Height { get { return bmp.Height; } }

		public Color this[int x, int y]
		{
			get
			{
				return GetPixel(x, y);
			}
			set
			{
				SetPixel(x, y, value);
			}
		}

		int textureHandle = -1;
		Bitmap bmp;
		MyFile file;

		public Texture2D(int width, int height)
		{
			bmp = new Bitmap(width, height);
		}

		public Texture2D(int textureHandle)
		{
			this.textureHandle = textureHandle;
			UpdateIsOnGpu();
		}

		public Texture2D(MyFile file)
		{
			this.file = file;
			file.OnFileChanged(() => WantsToBeUploadedToGpu = true);
			WantsToBeUploadedToGpu = true;
		}

		public void SetPixel(int x, int y, Color color)
		{
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + MemberName.For(() => KeepLocalCopyOfTexture) + " to true");
			lock (bmp)
			{
				if (bmp == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
				if (x < 0 || x >= Width && y < 0 && y >= Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
				bmp.SetPixel(x, y, color);
			}
			WantsToBeUploadedToGpu = true;
		}

		public Color GetPixel(int x, int y)
		{
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + MemberName.For(() => KeepLocalCopyOfTexture) + " to true");
			lock (bmp)
			{
				if (bmp == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
				if (x < 0 || x >= Width && y < 0 && y >= Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
				return bmp.GetPixel(x, y);
			}
		}

		public Color GetPixelBilinear(float x, float y)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			if (IsOnGpu)
			{
				GL.DeleteTexture(textureHandle); My.Check();
				IsOnGpu = false;
			}
		}

		void UnloadLocalCopy()
		{
			if (bmp != null)
			{
				bmp.Dispose();
				bmp = null;
			}
		}

		void LoadLocalCopy()
		{
			using (var s = file.GetDataStream())
				bmp = new Bitmap(s);
		}

		void UploadToGpu()
		{
            if (textureHandle == -1)
            {
                textureHandle = GL.GenTexture(); My.Check();
            }
			GL.BindTexture(TextureTarget.Texture2D, textureHandle); My.Check();

			Stream stream = null;

			if (bmp == null)
			{
				stream = file.GetDataStream();
				bmp = new Bitmap(stream);
			}
			lock (bmp)
			{
				BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0); My.Check();
				bmp.UnlockBits(bmpData);
			}
			bmp?.Dispose();
			bmp = null;
			stream?.Dispose();


            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            if (UsingMipMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); My.Check();
            }

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter()); My.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter()); My.Check();

			// https://www.opengl.org/wiki/Sampler_Object
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode()); My.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode()); My.Check();

			// ???
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, this.anisoLevel);

			//var extensions = GL.GetString(StringName.Extensions); My.Check();
			//if (extensions.Contains("GL_EXT_texture_filter_anisotropic"))
			//{
   //             if (maxAniso.HasValue == false)
   //             {
   //                 maxAniso = GL.GetInteger((GetPName)ExtTextureFilterAnisotropic.MaxTextureMaxAnisotropyExt); My.Check();
   //             }
			//	GL.TexParameter(
			//	   TextureTarget.Texture2D,
			//	   (TextureParameterName)ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt,
			//	   maxAniso.Value
			//	);
			//}
		}

		static int? maxAniso;

		void UpdateIsOnGpu()
		{
			//var yes = new bool[1];
			//GL.AreTexturesResident(1, new int[] { textureHandle }, yes); My.Check();
			//IsOnGpu = yes[0];
		}

		public override int GetNativeTextureID()
		{
			if (WantsToBeUploadedToGpu)
			{
				Dispose();
				WantsToBeUploadedToGpu = false;
				UploadToGpu();
				UpdateIsOnGpu();
			}
			return textureHandle;
		}
	}
}