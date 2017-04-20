using Neitri;
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
	public class Texture2D : Texture, IDisposable, IHasVersion
	{
		ILog Log => Singletons.Log.Scope(typeof(Texture2D) + " " + file.VirtualPath);

		public bool IsOnGpu { get; private set; }
		public bool WantsToBeUploadedToGpu { get; private set; }
		public bool KeepLocalCopyOfTexture { get; set; }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public ulong Version { get { return VersionInFile; } }
		public ulong VersionOnGpu { get; private set; } = 0;
		public ulong VersionInFile { get; private set; } = 1;

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
		FileExisting file;

		public Texture2D(int width, int height)
		{
			UseMipMaps = true;
			bmp = new Bitmap(width, height);
		}

		public Texture2D(int textureHandle)
		{
			UseMipMaps = true;
			this.textureHandle = textureHandle;
			UpdateIsOnGpu();
		}

		public Texture2D(FileExisting file)
		{
			UseMipMaps = true;
			this.file = file;
			file.OnFileChanged(() =>
			{
				WantsToBeUploadedToGpu = true;
				VersionInFile++;
			});
			WantsToBeUploadedToGpu = true;
		}

		public void SetPixel(int x, int y, Color color)
		{
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
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
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
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
				GL.DeleteTexture(textureHandle); MyGL.Check();
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
			using (var s = file.OpenReadWrite())
				bmp = new Bitmap(s);
		}

		void UploadToGpu()
		{
			Log.Info("uploading to GPU - start");

			if (textureHandle == -1)
			{
				textureHandle = GL.GenTexture(); MyGL.Check();
			}
			GL.BindTexture(TextureTarget.Texture2D, textureHandle); MyGL.Check();

			Stream stream = null;

			if (bmp == null)
			{
				stream = file.OpenReadWrite();
				bmp = new Bitmap(stream);
				this.Width = bmp.Width;
				this.Height = bmp.Height;
			}

			if (bmp.Width > MaxTextureSize) throw new NotSupportedException($"width is over max {MaxTextureSize}, {this}");
			if (bmp.Height > MaxTextureSize) throw new NotSupportedException($"width is over max {MaxTextureSize}, {this}");

			lock (bmp)
			{
				BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0); MyGL.Check();
				bmp.UnlockBits(bmpData);
			}
			if (KeepLocalCopyOfTexture == false)
			{
				bmp?.Dispose();
				bmp = null;
			}
			stream?.Dispose();


			// We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			if (UseMipMaps)
			{
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); MyGL.Check();
			}

			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter()); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter()); MyGL.Check();

			// https://www.opengl.org/wiki/Sampler_Object
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode()); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode()); MyGL.Check();

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

			VersionOnGpu = VersionInFile;
			UpdateIsOnGpu();

			Log.Info("uploading to GPU - end");
		}


		void UpdateIsOnGpu()
		{
			//var yes = new bool[1];
			//GL.AreTexturesResident(1, new int[] { textureHandle }, yes); My.Check();
			//IsOnGpu = yes[0];
			IsOnGpu = true;
		}

		public override int GetNativeTextureID()
		{
			if (WantsToBeUploadedToGpu)
			{
				Dispose();
				WantsToBeUploadedToGpu = false;
				UploadToGpu();
			}
			return textureHandle;
		}

		public override string ToString()
		{
			return $"{nameof(Texture2D)}: '{file.VirtualPath}' {Width} x {Height}";
		}

	}
}