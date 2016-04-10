using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    // http://docs.unity3d.com/ScriptReference/Texture.html

    public enum TextureWrapMode
    {
        Repeat,
        Clamp,
    }
    public enum FilterMode
    {
        Bilinear,
        Trilinear,
        Point,
    }
    public class Texture
    {
        public int anisoLevel;
        public FilterMode filterMode = FilterMode.Bilinear;
        public float mipMapBias;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

        public virtual int GetNativeTextureID() { return 0;  }


        internal TextureMinFilter GetTextureMinFilter(bool withMipMaps = false)
        {
            if (withMipMaps)
            {
                if (filterMode == FilterMode.Point) return TextureMinFilter.NearestMipmapNearest;
                else return TextureMinFilter.LinearMipmapLinear;
            }
            else
            {
                if (filterMode == FilterMode.Point) return TextureMinFilter.Nearest;
                else return TextureMinFilter.Linear;
            }
        }
        internal TextureMagFilter GetTextureMagFilter(bool withMipMaps = false)
        {
            if (filterMode == FilterMode.Point) return TextureMagFilter.Nearest;
            else return TextureMagFilter.Linear;
        }
        internal TextureWrapMode GetTextureWrapMode()
        {
            if (wrapMode == TextureWrapMode.Repeat) return TextureWrapMode.Repeat;
            else return TextureWrapMode.Clamp;
        }
    }

    
}
