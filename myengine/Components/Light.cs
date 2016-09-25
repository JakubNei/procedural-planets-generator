using MyEngine;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace MyEngine.Components
{
	// http://docs.unity3d.com/ScriptReference/Light.html
	public enum LightType
	{
		Spot,
		Directional,
		Point,
		Area
	}

	public enum LightShadows
	{
		None,
		Hard,
		Soft,
	}

	public interface ILight
	{
		bool HasShadows { get; }
		ShadowMap ShadowMap { get; }

		void UploadUBOdata(Camera camera, UniformBlock ubo, int lightIndex);
	}

	public class Light : ComponentWithShortcuts, IDisposable, ILight
	{
		public LightType LighType = LightType.Point;

		public LightShadows Shadows
		{
			get
			{
				return m_shadows;
			}
			set
			{
				m_shadows = value;
				const int shadowMapResolution = 1000;
				if (HasShadows && ShadowMap == null) ShadowMap = Dependency.Create<ShadowMap>(this, shadowMapResolution, shadowMapResolution);
			}
		}

		LightShadows m_shadows = LightShadows.None;

		public Vector3 color = Vector3.One;
		public float spotExponent;
		public float spotCutOff;

		public ShadowMap ShadowMap { get; private set; }
		public bool HasShadows { get { return this.Shadows != LightShadows.None && this.LighType == LightType.Directional; } }

		public Light(Entity entity) : base(entity)
		{
			entity.Scene.DataToRender.Add(this);
		}

		public void UploadUBOdata(Camera camera, UniformBlock ubo, int lightIndex)
		{
			ubo.light.color = this.color;

			if (LighType == LightType.Directional) ubo.light.position = Vector3.Zero;
			else ubo.light.position = camera.ViewPointPosition.Towards(this.Entity.Transform.Position).ToVector3();

			if (LighType == LightType.Point) ubo.light.direction = Vector3.Zero;
			else ubo.light.direction = this.Entity.Transform.Forward;

			ubo.light.spotExponent = this.spotExponent;
			ubo.light.spotCutOff = this.spotCutOff;

			ubo.light.hasShadows = HasShadows ? 1 : 0;
			ubo.light.lightIndex = lightIndex;

			ubo.lightUBO.UploadData();
		}

		public void Dispose()
		{
			Entity.Scene.DataToRender.Remove(this);
		}
	}
}