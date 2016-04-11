using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public class MaterialPBR : Material
    {
        public class SendToShaderAttribute : Attribute
        {

        }


        public Vector4 albedo = Vector4.One;
        public Texture2D albedoTexture;

        public float metallic = 0.0f;
        public Texture2D metallicTexture;

        public float smoothness = 0.5f;
        public Texture2D smoothnessTexture;

        public Vector3 emission = Vector3.One; // global illumination

        public Texture2D normalMap;
        public Texture2D depthMap;

        static List<System.Reflection.FieldInfo> fieldsToSend = new List<System.Reflection.FieldInfo>();
        static MaterialPBR()
        {
            foreach (var f in typeof(MaterialPBR).GetFields())
            {
                //if (f.GetCustomAttributes(typeof(SendToShaderAttribute), false) != null) fieldsToSend.Add(f);
                fieldsToSend.Add(f);
            }
        }

        public MaterialPBR() : base()
        {
            albedoTexture = Texture2D.whiteTexture;
            metallicTexture = Texture2D.whiteTexture;
            smoothnessTexture = Texture2D.whiteTexture;
            normalMap = null;
        }

        /*
        public override void BindThisMaterialUniforms(Shader shader = null)
        {
            if (shader == null) shader = this.GBufferShader;
            //shader.Uniform.Set("material.albedoTexture", albedoTexture);
            foreach (var p in fieldsToSend)
            {
                object val = p.GetValue(this);
                if(val!=null) shader.Uniform.Set("material." + p.Name, val);
            }
            shader.Uniform.Set("material.useNormalMapping", normalMap!=null);
            shader.Uniform.Set("material.useParallaxMapping", depthMap != null);

            base.BindThisMaterialUniforms(shader);

            //shader.Uniform.Set("material.albedo", albedo);
            //shader.Uniform.Set("material.metallic", metallic);
            //shader.Uniform.Set("material.smoothness", smoothness);
            //shader.Uniform.Set("material.emission", emission);
        }
        */
    }
}
