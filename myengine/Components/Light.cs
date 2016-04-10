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

        public LightType type = LightType.Point;
        public LightShadows shadows
        {
            get
            {
                return _shadows;
            }
            set
            {
                _shadows = value;
                const int shadowMapResolution = 1000;
                if (hasShadows && shadowMap == null) shadowMap = new ShadowMap(this, shadowMapResolution, shadowMapResolution);
            }
        }
        LightShadows _shadows = LightShadows.None;

        public Vector3 color=Vector3.One;
        public float spotExponent;
        public float spotCutOff;


        internal ShadowMap shadowMap;
        internal bool hasShadows { get { return this.shadows != LightShadows.None && this.type==LightType.Directional; } }


        public Light(Entity entity) : base(entity)
        {
            entity.Scene.Add(this);
        }

        internal void UploadUBOdata(UniformBlock ubo, int lightIndex) {

            ubo.light.color = this.color;

            if (type == LightType.Directional) ubo.light.position = Vector3.Zero;
            else ubo.light.position = this.Entity.Transform.Position;

            if (type == LightType.Point) ubo.light.direction = Vector3.Zero;
            else ubo.light.direction = this.Entity.Transform.Forward;

            ubo.light.spotExponent = this.spotExponent;
            ubo.light.spotCutOff = this.spotCutOff;

            ubo.light.hasShadows = hasShadows ? 1 : 0;
            ubo.light.lightIndex = lightIndex;

            ubo.lightUBO.UploadData();
        }

        public void Dispose()
        {
            Entity.Scene.Remove(this);
        }
    }
}
