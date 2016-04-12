using System;
using System.Threading;
using System.Collections.Generic;
using System.Text;
using System.IO;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace MyEngine
{
    public partial class Shader : IUnloadable
    {
        public const int positionLocation = 0;
        public const int normalLocation = 1;
        public const int tangentLocation = 2;
        public const int uvLocation = 3;
        public const int modelMatricesLocation = 4;

        public static Shader DefaultGBufferShader { get; private set; }
        public static Shader DefaultDepthGrabShader { get; private set; }

        public UniformsManager Uniforms { get; private set; }

        public bool shouldReload;

        internal int shaderProgramHandle { get; private set; }

                
        Dictionary<string, int> cache_uniformLocations = new Dictionary<string, int>();

        Asset asset;

        List<int> shaderPartHandles = new List<int>();

        FileChangedWatcher fileWatcher = new FileChangedWatcher();

        static Shader lastBindedShader;

        static Shader()
        {
            DefaultGBufferShader = Factory.GetShader("internal/deferred.gBuffer.standart.shader");
            DefaultDepthGrabShader = Factory.GetShader("internal/depthGrab.standart.shader");
        }

        public Shader(Asset asset)
        {
            Require.NotNull(() => asset);
            this.asset = asset;
            this.Uniforms = new UniformsManager();
            Load();
        }


        public void Unload()
        {
            foreach (var p in shaderPartHandles) GL.DeleteShader(p);
            GL.DeleteProgram(shaderProgramHandle);
        }


        static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }

        void Load()
        {
            
            shaderPartHandles.Clear();
            shaderProgramHandle = GL.CreateProgram();


            var builder = new ShaderBuilder(asset.AssetSystem);
            builder.Load(asset);
            foreach(var r in builder.buildResults)
            {
                AttachShader(r.shaderContents, r.shaderType, r.filePath);
            }

            FinalizeInit();
            
            Debug.Info(typeof(Shader) + " " + asset + " loaded successfully");

            fileWatcher.WatchFile(asset.RealPath, (string newFileName) => {
                shouldReload = true;
                fileWatcher.StopAllWatchers();
            });


            Uniforms.MarkAllUniformsAsChanged();
            cache_uniformLocations.Clear();
        }




        /// <summary>
        /// Reloads the shader if marked to reload, binds the shader, uploads all changed uniforms;
        /// </summary>
        public void Bind()
        {            
            if (shouldReload)
            {
                Debug.Info("Reloading " + asset.VirtualPath);
                Unload();
                Load();
                Uniforms.MarkAllUniformsAsChanged();
                shouldReload = false;
            }
            if (lastBindedShader != this)
            {
                GL.UseProgram(shaderProgramHandle);
                lastBindedShader = this;
            }
            Uniforms.UploadChangedUniforms(this);
        }




      

        bool AttachShader(string source, ShaderType type, string resource)
        {

            //source = source.Replace(maxNumberOfLights_name, maxNumberOfLights.ToString());
            

            int handle = GL.CreateShader(type);
            shaderPartHandles.Add(handle);

            GL.ShaderSource(handle, source);

            GL.CompileShader(handle);



            string logInfo;
            GL.GetShaderInfoLog(handle, out logInfo);
            if (logInfo.Length > 0 && !logInfo.Contains("hardware"))
            {
                Debug.Error("Vertex Shader failed!\nLog:\n" + logInfo);
            }



            int statusCode = 0;
            GL.GetShader(handle, ShaderParameter.CompileStatus, out statusCode);
            if (statusCode != 1)
            {
                Debug.Error(type.ToString() + " :: " + source + "\n" + GL.GetShaderInfoLog(handle) + "\n in file: " + resource);
                return false;
            }
                       


            GL.AttachShader(shaderProgramHandle, handle);

            // delete intermediate shader objects
            foreach (var p in shaderPartHandles) GL.DeleteShader(p);
            shaderPartHandles.Clear();

            return true;
        }


        void CheckError(GetProgramParameterName n)
        {
            int statusCode = 0;
            GL.GetProgram(shaderProgramHandle, n, out statusCode);
            if (statusCode != 1)
            {
                Debug.Error(n+"\n"+GL.GetProgramInfoLog(shaderProgramHandle));
            }
        }

        void FinalizeInit()
        {
            /*
            GL.BindAttribLocation(shaderProgramHandle, Shader.positionLocation, "in_position");
            GL.BindAttribLocation(shaderProgramHandle, Shader.normalLocation, "in_normal");
            GL.BindAttribLocation(shaderProgramHandle, Shader.tangentLocation, "in_tangent");
            GL.BindAttribLocation(shaderProgramHandle, Shader.uvLocation, "in_uv");
            */
            GL.LinkProgram(shaderProgramHandle);
            CheckError(GetProgramParameterName.LinkStatus);


            GL.ValidateProgram(shaderProgramHandle);
            CheckError(GetProgramParameterName.ValidateStatus);


            EngineMain.ubo.SetUniformBuffers(this);

            //Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
        }


        public bool SetUniformBlockBufferIndex(string name, int uniformBufferIndex)
        {
            var location = GL.GetUniformBlockIndex(shaderProgramHandle, name);
            if (location == -1)
            {
                Debug.Warning(asset + ", uniform block index " + name + " not found ", false);
                return false;
            }
            GL.UniformBlockBinding(shaderProgramHandle, location, uniformBufferIndex);
            return true;
        }


        public int GetUniformLocation(string name)
        {
            int location = -1;
            if (cache_uniformLocations.TryGetValue(name, out location) == false)
            {
                location = GL.GetUniformLocation(shaderProgramHandle, name);
                if (location == -1)
                {
                    Debug.Warning(asset + ", uniform " + name + " not found ", false);
                }
                cache_uniformLocations[name] = location;
            }
            return location;
        }
        

    }
}
