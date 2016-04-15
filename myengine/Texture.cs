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
    public abstract class Texture
    {
        public int anisoLevel;
        public bool UsingMipMaps { get; protected set; }
        public FilterMode filterMode = FilterMode.Trilinear;
        public float mipMapBias;
        public TextureWrapMode wrapMode = TextureWrapMode.Repeat;

        public virtual int GetNativeTextureID()
        {
            return 0;
        }


        protected TextureMinFilter GetTextureMinFilter()
        {
            if (UsingMipMaps)
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
        protected TextureMagFilter GetTextureMagFilter()
        {
            if (filterMode == FilterMode.Point) return TextureMagFilter.Nearest;
            else return TextureMagFilter.Linear;
        }
        protected TextureWrapMode GetTextureWrapMode()
        {
            return wrapMode;
        }
    }

    
}