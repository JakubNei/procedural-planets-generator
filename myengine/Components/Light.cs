using System;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Collections.Generic;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;

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
    public class Light : Component, IDisposable
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
                if (HasShadows && ShadowMap == null) ShadowMap = new ShadowMap(this, shadowMapResolution, shadowMapResolution);
            }
        }
        LightShadows m_shadows = LightShadows.None;

        public Vector3 color=Vector3.One;
        public float spotExponent;
        public float spotCutOff;


        public ShadowMap ShadowMap { get; private set; }
        public bool HasShadows { get { return this.Shadows != LightShadows.None && this.LighType==LightType.Directional; } }


        public Light(Entity entity) : base(entity)
        {
            entity.Scene.RenderData.Add(this);
        }

        public void UploadUBOdata(UniformBlock ubo, int lightIndex) {

            ubo.light.color = this.color;

            if (LighType == LightType.Directional) ubo.light.position = Vector3.Zero;
            else ubo.light.position = this.Entity.Transform.Position;

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
            Entity.Scene.RenderData.Remove(this);
        }
    }
}
