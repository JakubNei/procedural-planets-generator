using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyEngine
{
	// TODO: improve
	public class MaterialInstance : Material
	{
		public Material parentMaterial;
	}

	public class Material : SingletonsPropertyAccesor, ICloneable
	{
		Shader renderShader;

		public virtual Shader RenderShader
		{
			get
			{
				return renderShader ?? (renderShader = Factory.DefaultGBufferShader);
			}
			set
			{
				if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => RenderShader) + " to null");
				renderShader = value;
			}
		}

		Shader depthGrabShader;

		public virtual Shader DepthGrabShader
		{
			get
			{
				return depthGrabShader ?? (depthGrabShader = Factory.DefaultDepthGrabShader);
			}
			set
			{
				if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => DepthGrabShader) + " to null");
				depthGrabShader = value;
			}
		}

		public virtual UniformsData Uniforms { get; private set; }

		public Material()
		{
			Uniforms = new UniformsData();
		}

		public virtual void BeforeBindCallback()
		{
		}

		public virtual MaterialInstance MakeInstance()
		{
			var ret = new MaterialInstance();
			ret.parentMaterial = this;
			return ret;
		}

		public virtual Material CloneTyped()
		{
			var m = new Material()
			{
				RenderShader = RenderShader,
				DepthGrabShader = DepthGrabShader,
			};
			Uniforms.SendAllUniformsTo(m.Uniforms);
			return m;
		}

		public virtual object Clone()
		{
			return CloneTyped();
		}
	}
}