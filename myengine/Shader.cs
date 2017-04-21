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
using Neitri.Base;

namespace MyEngine
{
	public partial class Shader : SingletonsPropertyAccesor, IDisposable, IHasVersion
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
		public UniformsData Uniforms { get; private set; }

		public bool IsTransparent { get; set; }

		public ulong Version { get { return VersionInFile; } }
		public ulong VersionOnGpu { get; private set; } = 0;
		public ulong VersionInFile { get; private set; } = 1;

		public bool ShouldReload { get; set; }

		public bool HasTesselation { get; private set; }

		public int ShaderProgramHandle { get; private set; }

		Dictionary<string, int> cachedUniformLocations = new Dictionary<string, int>();



		FileExisting file;

		FileChangedWatcher fileWatcher = new FileChangedWatcher();

		static int lastBindedShaderHandle;

		public Shader(FileExisting file)
		{
			this.file = file;
			this.Uniforms = new UniformsData();
		}

		void Load()
		{
			ShaderProgramHandle = GL.CreateProgram(); MyGL.Check();

			var builder = new ShaderBuilder(file.FileSystem);
			var success = true;
			builder.LoadAndParse(file);

			if (builder.buildResults.Count == 0)
			{
				Log.Error("no shader parts were found, possible part markers are: " + Enum.GetNames(typeof(ShaderType)).Select(s => "[" + s + "]").Join(" "));
			}

			foreach (var r in builder.buildResults)
			{
				success &= AttachShader(r.shaderContents, r.shaderType, r.filePath);
			}

			FinalizeInit();

			if (success)
			{
				Log.Info(typeof(Shader) + " " + file + " loaded successfully");
				LoadState = State.LoadedSuccess;
				VersionOnGpu = VersionInFile;
			}
			else
			{
				LoadState = State.LoadedError;
				Log.Error("fix the error then press any key to reload ...");
				Console.ReadKey();
				Log.Error("reloading ...");
				Load();
				return;
			}

			fileWatcher.WatchFile(file.RealPath, (string newFilePath) =>
			{
				VersionInFile++;
				fileWatcher.StopAllWatchers();
				ShouldReload = true;
			});

			Uniforms.MarkAllUniformsAsChanged();
			cachedUniformLocations.Clear();

		}

		public void EnsureIsOnGpu()
		{
			if (LoadState == State.NotLoaded)
			{
				Load();
			}
		}

		/// <summary>
		/// Reloads the shader if marked to reload, binds the shader, uploads all changed uniforms;
		/// </summary>
		public bool Bind()
		{
			if (LoadState == State.NotLoaded)
			{
				Load();
			}
			else if (ShouldReload)
			{
				Log.Info("Reloading " + file.VirtualPath);
				Dispose();
				Load();
				ShouldReload = false;
			}

			if (LoadState == State.LoadedError) return false;

			if (lastBindedShaderHandle != ShaderProgramHandle)
			{
				GL.UseProgram(ShaderProgramHandle); MyGL.Check();
				lastBindedShaderHandle = ShaderProgramHandle;
			}
			Uniforms.UploadChangedUniforms(this);
			return true;
		}

		bool AttachShader(string source, ShaderType type, string resource)
		{
			if (type == ShaderType.TessControlShader) HasTesselation = true;
			//source = source.Replace(maxNumberOfLights_name, maxNumberOfLights.ToString());

			int handle = GL.CreateShader(type); MyGL.Check();


			GL.ShaderSource(handle, source); MyGL.Check();

			GL.CompileShader(handle); MyGL.Check();

			string logInfo;
			GL.GetShaderInfoLog(handle, out logInfo); MyGL.Check();
			if (logInfo.Length > 0)
			{
				Log.Error($"Error occured during compilation of {type} from '{resource}'\n{logInfo}");
				return false;
			}

			int statusCode = 0;
			GL.GetShader(handle, ShaderParameter.CompileStatus, out statusCode); MyGL.Check();
			if (statusCode != 1)
			{
				var error = GL.GetShaderInfoLog(handle); MyGL.Check();
				//Log.Error(type.ToString() + " :: " + source + "\n" + error + "\n in file: " + resource);
				return false;
			}

			GL.AttachShader(ShaderProgramHandle, handle); MyGL.Check();


			return true;
		}

		void CheckError(GetProgramParameterName n)
		{
			int statusCode = 0;
			GL.GetProgram(ShaderProgramHandle, n, out statusCode); MyGL.Check();
			if (statusCode != 1)
			{
				var infoLog = GL.GetProgramInfoLog(ShaderProgramHandle); MyGL.Check();
				Log.Error(n + "\n" + infoLog);
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
			GL.LinkProgram(ShaderProgramHandle); MyGL.Check();
			CheckError(GetProgramParameterName.LinkStatus);

			GL.ValidateProgram(ShaderProgramHandle); MyGL.Check();
			CheckError(GetProgramParameterName.ValidateStatus);

			EngineMain.ubo.SetUniformBuffers(this);

			//Debug.WriteLine(GL.GetProgramInfoLog(shaderProgramHandle));
		}

		public bool SetUniformBlockBufferIndex(string name, int uniformBufferIndex)
		{
			var location = GL.GetUniformBlockIndex(ShaderProgramHandle, name); MyGL.Check();
			if (location == -1)
			{
				Log.Warn(file + ", uniform block index " + name + " not found ");
				return false;
			}
			GL.UniformBlockBinding(ShaderProgramHandle, location, uniformBufferIndex); MyGL.Check();
			return true;
		}

		public int GetUniformLocation(string name)
		{
			int location = -1;
			if (cachedUniformLocations.TryGetValue(name, out location) == false)
			{
				location = GL.GetUniformLocation(ShaderProgramHandle, name); MyGL.Check();
				if (location == -1)
				{
					Log.Warn(file + ", uniform " + name + " not found ");
				}
				cachedUniformLocations[name] = location;
			}
			return location;
		}

		public void Dispose()
		{
			GL.DeleteProgram(ShaderProgramHandle); MyGL.Check();
			LoadState = State.NotLoaded;
		}
	}
}