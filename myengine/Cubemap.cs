using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL4;
using OpenTK;
using Neitri;
using System.Linq;

namespace MyEngine
{
	public class Cubemap : Texture
	{
		ILog Log => Singletons.Log.Scope(typeof(Cubemap) + " " + files.Select(f => f.VirtualPath).Join(","));

		public enum Face
		{
			PositiveX = 0,
			NegativeX = 1,
			PositiveY = 2,
			NegativeY = 3,
			PositiveZ = 4,
			NegativeZ = 5,
		}

		public bool IsOnGpu { get; private set; }
		public bool WantsToBeUploadedToGpu { get; private set; }
		public bool KeepLocalCopyOfTexture { get; set; }

		public Color this[Face face, int x, int y]
		{
			get
			{
				return GetPixel(face, x, y);
			}
			set
			{
				SetPixel(face, x, y, value);
			}
		}


		int textureHandle = -1;
		Bitmap[] bmps;
		MyFile[] files;

		static readonly TextureTarget[] textureTargets = new TextureTarget[] {
			TextureTarget.TextureCubeMapNegativeX,
			TextureTarget.TextureCubeMapPositiveX,
			TextureTarget.TextureCubeMapPositiveY,
			TextureTarget.TextureCubeMapNegativeY,
			TextureTarget.TextureCubeMapPositiveZ,
			TextureTarget.TextureCubeMapNegativeZ,
		};

		public Cubemap(int textureHandle)
		{
			this.textureHandle = textureHandle;
			UpdateIsOnGpu();
		}
		public Cubemap(MyFile[] files)
		{
			this.files = files;
			bmps = new Bitmap[6];
			WantsToBeUploadedToGpu = true;
		}
		public Cubemap(int width, int height)
		{
			bmps = new Bitmap[6];
			for (int i = 0; i < 6; i++)
			{
				bmps[i] = new Bitmap(width, height);
			}
		}





		public Color GetPixel(Face side, int x, int y)
		{
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
			if (bmps == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
			var bmp = bmps[(int)side];
			lock (bmp)
			{
				if (x < 0 || x >= bmp.Width && y < 0 && y >= bmp.Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
				return bmp.GetPixel(x, y);
			}
		}

		public void SetPixel(Face side, int x, int y, Color color)
		{
			if (KeepLocalCopyOfTexture == false) throw new Exception("before you can acces texture data you have to set " + nameof(KeepLocalCopyOfTexture) + " to true");
			if (bmps == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
			var bmp = bmps[(int)side];
			lock (bmp)
			{
				if (x < 0 || x >= bmp.Width && y < 0 && y >= bmp.Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
				bmp.SetPixel(x, y, color);
			}
			WantsToBeUploadedToGpu = true;
		}


		public void Unload()
		{
			if (IsOnGpu)
			{
				GL.DeleteTexture(textureHandle); MyGL.Check();
				IsOnGpu = false;
			}
		}

		void UnloadLocalCopy()
		{
			if (bmps != null)
			{
				foreach (var bmp in bmps) bmp.Dispose();
				bmps = null;
			}
		}
		void LoadLocalCopy()
		{
			for (int i = 0; i < 6; i++)
			{
				using (var s = files[i].GetDataStream())
					bmps[i] = new Bitmap(s);
			}
		}

		void UploadToGpu()
		{
			Log.Info("uploading to GPU - start");

			// better performance: 2d array, 2d texture buffer
			if (bmps == null) return;
			if (textureHandle == -1)
			{
				textureHandle = GL.GenTexture(); MyGL.Check();
			}
			GL.BindTexture(TextureTarget.TextureCubeMap, textureHandle); MyGL.Check();

			bool useMimMaps = false; // goes black if cubeMap uses mipmaps

			// We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter()); MyGL.Check();
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter()); MyGL.Check();
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode()); MyGL.Check();
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode()); MyGL.Check();
			GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)GetTextureWrapMode()); MyGL.Check();

			// ???
			//GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, this.anisoLevel); My.Check();


			Stream s;

			for (int i = 0; i < 6; i++)
			{

				var textureTarget = textureTargets[i];

				s = null;
				var bmp = bmps[i];
				if (bmp == null)
				{
					s = files[i].GetDataStream();
					bmp = new Bitmap(s);
				}
				lock (bmp)
				{
					BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
						OpenTK.Graphics.OpenGL4.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);
					bmp.UnlockBits(bmp_data);
				}
				if (s != null)
				{
					bmp.Dispose();
					s.Dispose();
				}

			}
			if (useMimMaps)
			{
				GL.GenerateMipmap(GenerateMipmapTarget.Texture2D); MyGL.Check();
			}

			GL.BindTexture(TextureTarget.TextureCubeMap, 0); MyGL.Check();


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
			if (WantsToBeUploadedToGpu || IsOnGpu == false)
			{
				Unload();
				WantsToBeUploadedToGpu = false;
				UploadToGpu();
			}
			return textureHandle;
		}


	}
}
