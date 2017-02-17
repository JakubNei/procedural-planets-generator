using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading;

namespace MyEngine
{
    public partial class Shader : IDisposable
    {
        public const int positionLocation = 0;
        public const int normalLocation = 1;
        public const int tangentLocation = 2;
        public const int uvLocation = 3;
        public const int modelMatricesLocation = 4;

        public enum State
        {
            NotLoaded,
            LoadedSuccess,
            LoadedError,
        }
        public State LoadState { get; private set; }
        public UniformsManager Uniforms { get; private set; }

        public bool shouldReload;

        public bool HasTesselation { get; private set; }

        public int ShaderProgramHandle { get; private set; }

        [Dependency]
        Debug debug;

        Dictionary<string, int> cache_uniformLocations = new Dictionary<string, int>();

        MyFile file;

        FileChangedWatcher fileWatcher = new FileChangedWatcher();

        static Shader lastBindedShader;

        Shader(MyFile file)
        {
            this.file = file;
            this.Uniforms = new UniformsManager();
        }

        void Load()
        {
            ShaderProgramHandle = GL.CreateProgram(); My.Check();

            var builder = new ShaderBuilder(file.FileSystem, debug);
            var success = true;
            builder.LoadAndParse(file);

            if (builder.buildResults.Count == 0)
            {
                debug.Error("no shader parts were found, possible part markers are: " + Enum.GetNames(typeof(ShaderType)).Select(s => "[" + s + "]").Join(" "));
            }

            foreach (var r in builder.buildResults)
            {
                success &= AttachShader(r.shaderContents, r.shaderType, r.filePath);
            }

            FinalizeInit();

            if (success)
            {
                debug.Info(typeof(Shader) + " " + file + " loaded successfully");
                LoadState = State.LoadedSuccess;
            } else
            {
                LoadState = State.LoadedError;
            }


            fileWatcher.WatchFile(file.RealPath, (string newFilePath) =>
            {
                fileWatcher.StopAllWatchers();
                shouldReload = true;
            });

            Uniforms.MarkAllUniformsAsChanged();
            cache_uniformLocations.Clear();

        }

        /// <summary>
        /// Reloads the shader if marked to reload, binds the shader, uploads all changed uniforms;
        /// </summary>
        public void Bind()
        {
            if (LoadState == State.NotLoaded)
            {
                Load();
            }
            else if (shouldReload)
            {
                debug.Info("Reloading " + file.VirtualPath);
                Dispose();
                Load();
                Uniforms.MarkAllUniformsAsChanged();
                shouldReload = false;
            }

            if (LoadState == State.LoadedError) return;

            if (lastBindedShader != this)
            {
                GL.UseProgram(ShaderProgramHandle); My.Check();
                lastBindedShader = this;
            }
            Uniforms.UploadChangedUniforms(this);
        }

        bool AttachShader(string source, ShaderType type, string resource)
        {
            if (type == ShaderType.TessControlShader) HasTesselation = true;
            //source = source.Replace(maxNumberOfLights_name, maxNumberOfLights.ToString());

            int handle = GL.CreateShader(type); My.Check();


            GL.ShaderSource(handle, source); My.Check();

            GL.CompileShader(handle); My.Check();

            string logInfo;
            GL.GetShaderInfoLog(handle, out logInfo); My.Check();
            if (logInfo.Length > 0)
            {
                debug.Error($"Error occured during compilation of {type} from '{resource}'\n{logInfo}");
            }

            int statusCode = 0;
            GL.GetShader(handle, ShaderParameter.CompileStatus, out statusCode); My.Check();
            if (statusCode != 1)
            {
                var error = GL.GetShaderInfoLog(handle); My.Check();
                //debug.Error(type.ToString() + " :: " + source + "\n" + error + "\n in file: " + resource);
                return false;
            }

            GL.AttachShader(ShaderProgramHandle, handle); My.Check();


            return true;
        }

        void CheckError(GetProgramParameterName n)
        {
            int statusCode = 0;
            GL.GetProgram(ShaderProgramHandle, n, out statusCode); My.Check();
            if (statusCode != 1)
            {
                var infoLog = GL.GetProgramInfoLog(ShaderProgramHandle); My.Check();
                debug.Error(n + "\n" + infoLog);
            }
        }

        void FinalizeInit()
        {
            /*
            GL.BindAttribLocation(shaderProgramHandle, Shader.positionLocation, "in_position"); My.Check();
            GL.BindAttribLocation(shaderProgramHandle, Shader.normalLocation, "in_normal"); My.Check();
            GL.BindAttribLocation(shaderProgramHandle, Shader.tangentLocation, "in_tangent"); My.Check();
            GL.BindAttribLocation(shaderProgramHandle, Shader.uvLocation, "in_uv"); My.Check();
            */
            GL.LinkProgram(ShaderProgramHandle); My.Check();
            CheckError(GetProgramParameterName.LinkStatus);

            GL.ValidateProgram(ShaderProgramHandle); My.Check();
            CheckError(GetProgramParameterName.ValidateStatus);

            EngineMain.ubo.SetUniformBuffers(this);

            //Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
        }

        public bool SetUniformBlockBufferIndex(string name, int uniformBufferIndex)
        {
            var location = GL.GetUniformBlockIndex(ShaderProgramHandle, name); My.Check();
            if (location == -1)
            {
                debug.Warning(file + ", uniform block index " + name + " not found ", false);
                return false;
            }
            GL.UniformBlockBinding(ShaderProgramHandle, location, uniformBufferIndex); My.Check();
            return true;
        }

        public int GetUniformLocation(string name)
        {
            int location = -1;
            if (cache_uniformLocations.TryGetValue(name, out location) == false)
            {
                location = GL.GetUniformLocation(ShaderProgramHandle, name); My.Check();
                if (location == -1)
                {
                    debug.Warning(file + ", uniform " + name + " not found ", false);
                }
                cache_uniformLocations[name] = location;
            }
            return location;
        }

        public void Dispose()
        {
            GL.DeleteProgram(ShaderProgramHandle); My.Check();
            LoadState = State.NotLoaded;
        }
    }
}