using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public class UniformsManager
    {
        Dictionary<string, Texture> uniformsTexturesData = new Dictionary<string, Texture>();
        Dictionary<string, object> uniformsObjectData = new Dictionary<string, object>();

        HashSet<string> uniformsChanged = new HashSet<string>();


        public void SendAllUniformsTo(UniformsManager uniformManager)
        {
            foreach (var kvp in uniformsObjectData) uniformManager.Set(kvp.Key, kvp.Value);
            foreach (var kvp in uniformsTexturesData) uniformManager.Set(kvp.Key, kvp.Value);
        }

        /// <summary>
        /// Uploads all uniforms markes as changed into GPU at uniform location from shader.
        /// </summary>
        /// <param name="shader"></param>
        public void UploadChangedUniforms(Shader shader)
        {
            //if (shader == null) shader = this.shader;
            foreach (var name in uniformsChanged)
            {
                TryUploadStructType(shader, name, uniformsObjectData[name]);
            }
            uniformsChanged.Clear();

            int texturingUnit = 0;
            foreach (var kvp in uniformsTexturesData)
            {
                if (TryUploadStructType(shader, kvp.Key, texturingUnit))
                {
                    TryUploadTextureType(shader, kvp.Key, kvp.Value, texturingUnit);
                    texturingUnit++;
                }
            }
        }





        public void MarkAllUniformsAsChanged()
        {
            foreach (var kvp in uniformsObjectData)
            {
                uniformsChanged.Add(kvp.Key);
            }
        }



        public void Set(string name, object o)
        {
            if (o is Texture)
            {
                Texture oldTex;
                if (uniformsTexturesData.TryGetValue(name, out oldTex) == false || oldTex.Equals(o) == false)
                {
                    uniformsTexturesData[name] = o as Texture;
                }
            }
            else {
                object oldObj = null;
                if (uniformsObjectData.TryGetValue(name, out oldObj) == false || oldObj.Equals(o) == false)
                {
                    uniformsObjectData[name] = o;
                    uniformsChanged.Add(name);
                }
            }
        }

        public T Get<T>(string name, T defaultValue = default(T))
        {

            object obj = null;
            if (uniformsObjectData.TryGetValue(name, out obj))
            {
                try
                {
                    return (T)obj;
                }
                catch
                {

                }
            }

            Texture tex;
            if (uniformsTexturesData.TryGetValue(name, out tex))
            {
                try
                {
                    return (T)((object)tex);
                }
                catch
                {

                }
            }

            return defaultValue;
        }


        bool TryUploadStructType(Shader shader, string name, object o)
        {

            var location = shader.GetUniformLocation(name);
            if (location == -1) return false;
            if (o is Matrix4)
            {
                var u = (Matrix4)o;
                GL.UniformMatrix4(location, false, ref u);
                return true;
            }
            if (o is bool)
            {
                var u = (bool)o;
                GL.Uniform1(location, u ? 1 : 0);
                return true;
            }
            if (o is int)
            {
                var u = (int)o;
                GL.Uniform1(location, u);
                return true;
            }
            if (o is float)
            {
                var u = (float)o;
                GL.Uniform1(location, u);
                return true;
            }
            if (o is double)
            {
                var u = (double)o;
                GL.Uniform1(location, u);
                return true;
            }
            if (o is Vector2)
            {
                var u = (Vector2)o;
                GL.Uniform2(location, ref u);
                return true;
            }
            if (o is Vector3)
            {
                var u = (Vector3)o;
                GL.Uniform3(location, ref u);
                return true;
            }
            if (o is Vector4)
            {
                var u = (Vector4)o;
                GL.Uniform4(location, ref u);
                return true;
            }
            return false;
        }



        bool TryUploadTextureType(Shader shader, string name, object o, int texturingUnit)
        {
            if (o is Texture2D)
            {
                var u = (Texture2D)o;
                SendTexture(name, u, texturingUnit);
                return true;
            }
            if (o is Cubemap)
            {
                var u = (Cubemap)o;
                SendTexture(name, u, texturingUnit);
                return true;
            }

            return false;
        }
        void SendTexture(string name, Texture2D texture2D, int texturingUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit);
            GL.BindTexture(TextureTarget.Texture2D, texture2D.GetNativeTextureID());
        }
        void SendTexture(string name, Cubemap cubeMap, int texturingUnit)
        {
            GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMap.GetNativeTextureID());
        }


    }

}