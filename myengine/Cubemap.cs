using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;
using System.Drawing.Imaging;
using OpenTK.Graphics.OpenGL;
using OpenTK;

namespace MyEngine
{
    public class Cubemap : Texture
    {
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
        Asset[] assets;

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
        public Cubemap(Asset[] assets)
        {
            this.assets = assets;
            bmps = new Bitmap[6];
            Load();
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
            if (bmps == null) throw new NullReferenceException("texture was intialized only with gpu handle, no data");
            var bmp = bmps[(int)side];
            lock(bmp)
            {
                if (x < 0 || x >= bmp.Width && y < 0 && y >= bmp.Height) throw new IndexOutOfRangeException("x or y is out of texture width or height");
                bmp.SetPixel(x, y, color);
            }
            WantsToBeUploadedToGpu = true;
        }

        /// <summary>
        /// If x and y are out of bounds for current face, it moves them around to proper face.
        /// </summary>
        /// <param name="side"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void SanitizeCoords(ref Face face, ref int x, ref int y)
        {
            var bmp = bmps[(int)face];
            

        }

        public void Unload()
        {
            if (IsOnGpu)
            {
                GL.DeleteTexture(textureHandle);
                IsOnGpu = false;
            }
        }

        void Load()
        {
            for (int i = 0; i < 6; i++)
            {
                using (var s = assets[i].GetDataStream())
                    bmps[i] = new Bitmap(s);
            }
        }

        void UploadToGpu()
        {

            // better performance: 2d array, 2d texture buffer
            if (bmps == null) return;
            if (textureHandle == -1) textureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureHandle);

            bool useMimMaps = false; // goes black if cubeMap uses mipmaps

            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)GetTextureWrapMode());

            // ???
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, this.anisoLevel);


            for (int i = 0; i < 6; i++)
            {
                
                var textureTarget = textureTargets[i];

                var bmp = bmps[i];

                lock(bmp)
                {
                    BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                    GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                        OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                    bmp.UnlockBits(bmp_data);
                }

            }
            if (useMimMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

        }
        void UpdateIsOnGpu()
        {
            var yes = new bool[1];
            GL.AreTexturesResident(1, new int[] { textureHandle }, yes);
            IsOnGpu = yes[0];
        }

        public override int GetNativeTextureID()
        {
            if (WantsToBeUploadedToGpu || IsOnGpu == false)
            {
                Unload();
                WantsToBeUploadedToGpu = false;
                UploadToGpu();
                UpdateIsOnGpu();
            }
            return textureHandle;
        }


    }
}
