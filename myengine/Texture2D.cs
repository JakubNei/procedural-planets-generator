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
		ILog Log => Singletons.Log.Scope(typeof(Texture2D) + " handle:" + textureHandle + ", file:" + file?.VirtualPath);

		public enum TextureType
		{
			FromFile,
			FromHandle,
			FromDimensions,
		}

		public readonly TextureType Type;

		public bool IsOnGpu { get; private set; }
		//public bool KeepLocalCopyOfTexture { get; set; }

		public int Width { get; private set; }
		public int Height { get; private set; }

		public ulong Version { get { return VersionInFile; } }
		public ulong VersionOnGpu { get; private set; } = 0;
		//public ulong VersionInRam { get; private set; } = 0;
		public ulong VersionInFile { get; private set; } = 1;

		//public Color this[int x, int y]
		//{
		//	get
		//	{
		//		return GetPixel(x, y);
		//	}
		//	set
		//	{
		//		SetPixel(x, y, value);
		//	}
		//}

		int textureHandle = -1;
		Bitmap bmp;
		FileExisting file;

		public Texture2D(int width, int height)
		{
			Type = TextureType.FromDimensions;
			UseMipMaps = true;
			Width = width;
			Height = height;
			//bmp = new Bitmap(width, height);
		}

		public Texture2D(int textureHandle)
		{
			Type = TextureType.FromHandle;
			UseMipMaps = true;
			IsOnGpu = true;
			this.textureHandle = textureHandle;
		}

		public Texture2D(FileExisting file)
		{
			Type = TextureType.FromFile;
			UseMipMaps = true;
			this.file = file;
			file.OnFileChanged(() =>
			{
				VersionInFile++;
			});
		}

		//public void SetPixel(int x, int y, Color color)
		//{
		//	if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
		//	lock (bmp)
		//	{
		//		if (bmp == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
		//		if (x < 0 || x >= Width && y < 0 && y >= Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
		//		bmp.SetPixel(x, y, color);
		//	}
		//	VersionInRam++;
		//}


		//public Color GetPixel(int x, int y)
		//{
		//	if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
		//	lock (bmp)
		//	{
		//		if (bmp == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
		//		if (x < 0 || x >= Width && y < 0 && y >= Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
		//		return bmp.GetPixel(x, y);
		//	}
		//}



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
			if (Type == TextureType.FromFile)
			{
				Log.Info("uploading to GPU - start");
			}

			if (textureHandle == -1)
			{
				textureHandle = GL.GenTexture(); MyGL.Check();
			}
			GL.BindTexture(TextureTarget.Texture2D, textureHandle); MyGL.Check();


			if (Type == TextureType.FromFile)
			{
				Stream stream = null;
				if (bmp == null && file != null)
				{
					stream = file.OpenReadWrite();
					bmp = new Bitmap(stream);
					Width = bmp.Width;
					Height = bmp.Height;
				}

				if (bmp.Width > MaxTextureSize) throw new NotSupportedException($"width is over max {MaxTextureSize}, {this}");
				if (bmp.Height > MaxTextureSize) throw new NotSupportedException($"width is over max {MaxTextureSize}, {this}");

				lock (bmp)
				{
					BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, bmpData.Width, bmpData.Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0); MyGL.Check();
					bmp.UnlockBits(bmpData);
				}
				//if (KeepLocalCopyOfTexture == false)
				{
					bmp?.Dispose();
					bmp = null;
				}
				stream?.Dispose();
			}
			else
			{
				GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, Width, Height, 0, OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, new IntPtr()); MyGL.Check();
			}


			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter()); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter()); MyGL.Check();

			// https://www.opengl.org/wiki/Sampler_Object
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode()); MyGL.Check();
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode()); MyGL.Check();

			// We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			if (UseMipMaps)
			{
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); MyGL.Check();
			}


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

			GL.BindTexture(TextureTarget.Texture2D, 0); MyGL.Check();

			IsOnGpu = true;

			if (Type == TextureType.FromFile)
			{
				VersionOnGpu = VersionInFile;
				Log.Info("uploading to GPU - end");
			}
		}


		public void GenerateMipMaps()
		{
			GL.BindTexture(TextureTarget.Texture2D, GetNativeTextureID()); MyGL.Check();

			GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); MyGL.Check();

			GL.BindTexture(TextureTarget.Texture2D, 0); MyGL.Check();
		}


		//void UpdateIsOnGpu()
		//{
		//	var yes = new bool[1];
		//	//GL.AreTexturesResident(1, new int[] { textureHandle }, yes); My.Check();
		//	//IsOnGpu = yes[0];
		//	IsOnGpu = true;
		//}

		public override int GetNativeTextureID()
		{
			if (IsOnGpu == false || (Type == TextureType.FromFile && VersionOnGpu != VersionInFile))
			{
				Dispose();
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