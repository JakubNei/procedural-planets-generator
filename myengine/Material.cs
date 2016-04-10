using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public class Material
    {

        public Shader gBufferShader;
        public Shader depthGrabShader;

        internal Material()
        {

        }

        internal virtual void BindUniforms(Shader shader=null)
        {
            if (shader == null) shader = this.gBufferShader;
            foreach(var name in uniformsChanged)
            {
                shader.SetUniform(name, uniformsData[name]);
            }
            uniformsChanged.Clear();
        }


        internal virtual void BindChangedUniforms_Internal()
        {
            //if (shader == null) shader = this.shader;
            foreach (var name in uniformsChanged)
            {
                gBufferShader.SetUniform_Internal(name, uniformsData[name]);
            }
            uniformsChanged.Clear();


            gBufferShader.ResetTexturingUnitsCounter();
            foreach (var kvp in texturesData)
            {
                gBufferShader.TrySetTextureType(kvp.Key, kvp.Value);
            }
        }



        Dictionary<string, Texture> texturesData = new Dictionary<string, Texture>();
        Dictionary<string, object> uniformsData = new Dictionary<string, object>();

        List<string> uniformsChanged = new List<string>();


        public void AllUniformsChanged()
        {
            uniformsChanged.Clear();
            foreach(var kvp in uniformsData)
            {
                uniformsChanged.Add(kvp.Key);
            }
        }



        public void SetUniform(string name, object o)
        {
            if (o is Texture)
            {
                texturesData[name] = o as Texture;
                return;
            }

            object oldObj = null;
            if(!uniformsData.TryGetValue(name, out oldObj) || !oldObj.Equals(o))
            {
                uniformsData[name] = o;
                if(!uniformsChanged.Contains(name)) uniformsChanged.Add(name);
            }                      
        }


      









    }
}
