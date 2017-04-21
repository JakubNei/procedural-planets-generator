using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace MyEngine
{
	// http://docs.unity3d.com/ScriptReference/Texture.html

	public enum TextureWrapMode
	{
		Repeat = OpenTK.Graphics.OpenGL4.TextureWrapMode.Repeat,
		Clamp = OpenTK.Graphics.OpenGL4.TextureWrapMode.ClampToEdge,
	}
	public enum FilterMode
	{
		Bilinear,
		Trilinear,
		Point,
	}
	public abstract class Texture
	{
		public bool UseMipMaps { get; set; }
		public FilterMode FilterMode { get; set; } = FilterMode.Trilinear;
		public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.Repeat;


		public abstract int GetNativeTextureID();


		protected TextureMinFilter GetTextureMinFilter()
		{
			if (UseMipMaps)
			{
				if (FilterMode == FilterMode.Point) return TextureMinFilter.NearestMipmapNearest;
				else return TextureMinFilter.LinearMipmapLinear;
			}
			else
			{
				if (FilterMode == FilterMode.Point) return TextureMinFilter.Nearest;
				else return TextureMinFilter.Linear;
			}
		}
		protected TextureMagFilter GetTextureMagFilter()
		{
			if (FilterMode == FilterMode.Point) return TextureMagFilter.Nearest;
			else return TextureMagFilter.Linear;
		}
		protected TextureWrapMode GetTextureWrapMode()
		{
			return WrapMode;
		}


		protected int MaxTextureSize
		{
			get
			{
				int maxSize = 0;
				GL.GetInteger(GetPName.MaxTextureSize, out maxSize);
				return maxSize;
			}
		}
	}


}