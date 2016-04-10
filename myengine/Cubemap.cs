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

        
        int textureHandle;


        internal Cubemap(int textureHandle)
        {
            this.textureHandle = textureHandle;
        }
        internal Cubemap(ResourcePath[] resources)
        {
            this.Load(resources);
        }



        public void Unload()
        {
            GL.DeleteTexture(textureHandle);
        }

        void Load(ResourcePath[] resources)
        {
            
            // better performance: 2d array, 2d texture buffer

            textureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.TextureCubeMap, textureHandle);

            bool useMimMaps = false; // goes black if cubeMap uses mipmaps

            // We will not upload mipmaps, so disable mipmapping (otherwise the texture will not appear).
            // We can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
            // mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)GetTextureMinFilter(useMimMaps));
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)GetTextureMagFilter(useMimMaps));
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)GetTextureWrapMode());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)GetTextureWrapMode());
            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)GetTextureWrapMode());

            // ???
            //GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, this.anisoLevel);

            TextureTarget[] textureTargets = new TextureTarget[] { 
                TextureTarget.TextureCubeMapPositiveX, 
                TextureTarget.TextureCubeMapNegativeX,
                TextureTarget.TextureCubeMapPositiveY, 
                TextureTarget.TextureCubeMapNegativeY,
                TextureTarget.TextureCubeMapPositiveZ, 
                TextureTarget.TextureCubeMapNegativeZ,
            };


            for (int i = 0; i < 6; i++)
            {
                
                var textureTarget = textureTargets[i];

                Bitmap bmp = new Bitmap(resources[i]);
                BitmapData bmp_data = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

                GL.TexImage2D(textureTarget, 0, PixelInternalFormat.Rgba, bmp_data.Width, bmp_data.Height, 0,
                    OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmp_data.Scan0);

                bmp.UnlockBits(bmp_data);

            }
            if (useMimMaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }
            

        }
        public bool IsOnGpu()
        {
            var yes = new bool[1];
            GL.AreTexturesResident(1, new int[] { textureHandle }, yes);
            return yes[0];
        }
        public int GetNativeTextureID()
        {
            return textureHandle;
        }
     

    }
}
