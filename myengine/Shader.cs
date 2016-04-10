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
    public class Shader : IUnloadable
    {

        string prependSource;
        public bool shouldReload;
        internal int shaderProgramHandle { get; private set; }
        List<int> shaderParts = new List<int>();
        Material lastStateMaterial;
        

        public void Unload()
        {
            foreach (var p in shaderParts) GL.DeleteShader(p);
            GL.DeleteProgram(shaderProgramHandle);
        }

        ResourcePath resource;
        public Shader(ResourcePath resource)
        {
            Load(resource);
            lastStateMaterial = new Material() { gBufferShader = this };
        }
        FileSystemWatcher watcher;
        List<Thread> watcherThreads = new List<Thread>();
        void StopAllWatchers()
        {
            foreach (var w in watcherThreads)
            {
                w.Abort();
            }
            watcherThreads.Clear();
        }
        void StartWatcher(ResourcePath resource)
        {
            try
            {
                var watcherThread = new Thread(() =>
                {
                    var path = (string)resource;
                    var lastSlash = path.LastIndexOf(Path.DirectorySeparatorChar);
                    var dir = System.Environment.CurrentDirectory + Path.DirectorySeparatorChar + path.Substring(0, lastSlash);
                    var fileName = path.Substring(lastSlash + 1);


                    watcher = new FileSystemWatcher();
                    watcher.EnableRaisingEvents = false;
                    watcher.Path = dir;
                    /* Watch for changes in LastAccess and LastWrite times, and 
                       the renaming of files or directories. */
                    watcher.NotifyFilter = NotifyFilters.LastWrite
                       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    // Only watch text files.
                    watcher.Filter = fileName;

                    // Add event handlers.
                    // watcher.Changed += new FileSystemEventHandler(OnChanged);
                    /*watcher.Changed += new FileSystemEventHandler(
                        (object o, FileSystemEventArgs e) =>
                        {
                            Shader s = (o as Shader);
                            Debug.Info("Reloading " + resource);
                            Load(resource);
                        }
                    );*/

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;
                    var a = watcher.WaitForChanged(WatcherChangeTypes.Changed);
                    shouldReload = true;

                    //Debug.Info(a.ToString());
                });
                watcherThread.IsBackground = true;
                watcherThread.Start();

                watcherThreads.Add(watcherThread);
            }
            catch (Exception e)
            {
                //StartWatcher(resource);
            }
        }
        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
        }
        public void Load(ResourcePath resource)
        {
            StopAllWatchers();

            this.resource = resource;

            shaderParts.Clear();
            shaderProgramHandle = GL.CreateProgram();

            int numOfRetries;

            string source="";

            numOfRetries = 5;
            while (numOfRetries > 0)
            {
                try
                {
                    using (var fs = new FileStream(resource, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var sr = new StreamReader(fs, Encoding.Default))
                        source = sr.ReadToEnd();

                    numOfRetries = 0;
                }
                catch (IOException e)
                {
                    numOfRetries--;
                }
            }
            StartWatcher(resource);


            int line = 0;


            ResourcePath prependAllRes = "internal/prependAll.shader";
            string prependAll = "";
            numOfRetries = 5;
            while (numOfRetries > 0)
            {
                try
                {
                    using (var fs = new FileStream(prependAllRes, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var sr = new StreamReader(fs, Encoding.Default))
                        prependAll = sr.ReadToEnd();
                    numOfRetries = 0;
                }
                catch (IOException e)
                {
                    numOfRetries--;
                }
            }
            StartWatcher(prependAllRes);


            string prepend = "";
            {
                ShaderType shaderType = ShaderType.VertexShader;
                int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);
                if (startOfTag == -1)
                {
                    Debug.Error("Shader part start not found " + resource.originalPath);
                    return;
                }
                prepend += source.Substring(0, startOfTag - 1);
                source = source.Substring(startOfTag);
            }

            line += prepend.Split('\n').Length;

            prepend = prependAll + prepend;
            

            foreach (ShaderType type in System.Enum.GetValues(typeof(ShaderType)))
            {

                ShaderType shaderType = ShaderType.VertexShader;
                int startOfTag = GetClosestShaderTypeTagPosition(source, 0, ref shaderType);

                if (startOfTag != -1)
                {
                    var tagLength = shaderType.ToString().Length + 2;
                    ShaderType _st = ShaderType.VertexShader;
                    int endOfShaderPart = GetClosestShaderTypeTagPosition(source, startOfTag + tagLength, ref _st);
                    if (endOfShaderPart == -1) endOfShaderPart = source.Length;

                    var startOfShaderPart = startOfTag + tagLength;
                    string shaderPart = source.Substring(
                        startOfShaderPart,
                        endOfShaderPart - startOfShaderPart
                    );


                    AttachShader(prepend + "\n#line "+line+"\n"+shaderPart, shaderType, resource);

                    line += shaderPart.Split('\n').Length;

                    source = source.Substring(endOfShaderPart);
                }
            }

            FinalizeInit();
        }


        int GetClosestShaderTypeTagPosition(string source, int offset, ref ShaderType shaderType)
        {
            int startOfTag = -1;
            foreach (ShaderType type in System.Enum.GetValues(typeof(ShaderType)) )
            {
                string tag = "[" + type.ToString() + "]";

                int thisStartOfTag = source.IndexOf(tag, offset);
                if (thisStartOfTag != -1)
                {
                    if(startOfTag == -1 || thisStartOfTag<startOfTag)
                    shaderType = type;
                    startOfTag = thisStartOfTag;
                }
            }
            return startOfTag;
        }

        static Shader lastBindedShader;

        internal void Bind()
        {
            
            if (shouldReload)
            {
                Debug.Info("Reloading " + resource.originalPath);
                Unload();
                Load(resource);
                lastStateMaterial.AllUniformsChanged();
                shouldReload = false;
            }
            if (lastBindedShader != this)
            {
                GL.UseProgram(shaderProgramHandle);
                lastBindedShader = this;
            }

            lastStateMaterial.BindChangedUniforms_Internal();
        }




        void Prepend(ResourcePath name)
        {
            using (var fs = new System.IO.StreamReader(name))
                prependSource += fs.ReadToEnd();
        }

        bool AttachShader(string source, ShaderType type, ResourcePath resource)
        {

            //source = source.Replace(maxNumberOfLights_name, maxNumberOfLights.ToString());
            

            int handle = GL.CreateShader(type);
            shaderParts.Add(handle);

            GL.ShaderSource(handle, prependSource + source);

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
                Debug.Error(type.ToString() + " :: " + source + "\n" + GL.GetShaderInfoLog(handle) + "\n in file: " + resource.originalPath);
                return false;
            }
                       


            GL.AttachShader(shaderProgramHandle, handle);

            // delete intermediate shader objects
            foreach (var p in shaderParts) GL.DeleteShader(p);
            shaderParts.Clear();

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


        internal bool SetUniformBuffer(string name, int uniformBufferIndex)
        {
            var location = GL.GetUniformBlockIndex(shaderProgramHandle, name);
            if (location == -1)
            {
                Debug.Warning(resource.originalPath + ", uniform block index " + name + " not found ", false);
                return false;
            }
            GL.UniformBlockBinding(shaderProgramHandle, location, uniformBufferIndex);
            return true;
        }



        internal int GetLocation(string name)
        {
            var location = GL.GetUniformLocation(shaderProgramHandle, name);
            if (location == -1)
            {
                Debug.Warning(this.resource.originalPath + ", uniform " + name + " not found ", false);
            }
            return location;
        }

        public void SetUniform(string name, object o)
        {
            lastStateMaterial.SetUniform(name, o);
        }



        internal bool SetUniform_Internal(string name, object o)
        {
            //if (TrySetTextureType(name, o)) return true;
            return TrySetStructType(name, o);
        }





        internal bool TrySetStructType(string name, object o)
        {

            var location = GetLocation(name);
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
                GL.Uniform1(location, u?1:0);
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



        internal bool TrySetTextureType(string name, object o)
        {
            if (o is Texture2D)
            {
                var u = (Texture2D)o;
                SetTexture(name, u);
                return true;
            }
            if (o is Cubemap)
            {
                var u = (Cubemap)o;
                SetTexture(name, u);
                return true;
            }

            return false;
        }



        int nextTexturingUnit = 0;
        int savedNextTexturingUnit = 0;
        internal void ResetTexturingUnitsCounter()
        {
            nextTexturingUnit = 0;
        }
        internal void SaveTexturingUnitCounter()
        {
            savedNextTexturingUnit = nextTexturingUnit;
        }
        internal void LoadTexturingUnitCounter()
        {
            nextTexturingUnit = savedNextTexturingUnit;
        }
        internal void SetTexture(string name, Texture2D texture2D)
        {
            int texturingUnit = nextTexturingUnit;
            nextTexturingUnit++;
            GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit);
            GL.BindTexture(TextureTarget.Texture2D, texture2D.GetNativeTextureID());
            SetUniform(name, texturingUnit);
        }        
        internal void SetTexture(string name, Cubemap cubeMap)
        {
            int texturingUnit = nextTexturingUnit;
            nextTexturingUnit++;
            GL.ActiveTexture(TextureUnit.Texture0 + texturingUnit);
            GL.BindTexture(TextureTarget.TextureCubeMap, cubeMap.GetNativeTextureID());
            SetUniform(name, texturingUnit);
        }




        public const int positionLocation = 0;
        public const int normalLocation = 1;
        public const int tangentLocation = 2;
        public const int uvLocation = 3;
        public const int modelMatricesLocation = 4;
        

    }
}
