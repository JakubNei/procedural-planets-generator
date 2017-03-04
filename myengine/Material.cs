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
	public class MaterialInstance : Material
	{
		public Material parentMaterial;

		public MaterialInstance(Factory factory) : base(factory)
		{
		}
	}

	public class Material : ICloneable
	{
		Factory factory;

		Shader gBufferShader;

		public virtual Shader GBufferShader
		{
			get
			{
				return gBufferShader ?? (gBufferShader = factory.DefaultGBufferShader);
			}
			set
			{
				if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => GBufferShader) + " to null");
				gBufferShader = value;
			}
		}

		Shader depthGrabShader;

		public virtual Shader DepthGrabShader
		{
			get
			{
				return depthGrabShader ?? (depthGrabShader = factory.DefaultDepthGrabShader);
			}
			set
			{
				if (value == null) throw new NullReferenceException("can not set " + MemberName.For(() => DepthGrabShader) + " to null");
				depthGrabShader = value;
			}
		}

		public virtual UniformsManager Uniforms { get; private set; }

		public Material(Factory factory)
		{
			this.factory = factory;
			Uniforms = new UniformsManager();
		}

		public virtual void BeforeBindCallback()
		{
		}

		public virtual MaterialInstance MakeInstance()
		{
			var ret = new MaterialInstance(factory);
			ret.parentMaterial = this;
			return ret;
		}

		public virtual Material CloneTyped()
		{
			var m = new Material(factory)
			{
				GBufferShader = GBufferShader,
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