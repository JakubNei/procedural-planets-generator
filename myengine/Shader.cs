using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
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

		public bool IsLoaded { get; private set; }
		public UniformsManager Uniforms { get; private set; }

		public bool shouldReload;

		internal int shaderProgramHandle { get; private set; }

		[Dependency]
		Debug debug;

		Dictionary<string, int> cache_uniformLocations = new Dictionary<string, int>();

		Asset asset;

		List<int> shaderPartHandles = new List<int>();

		FileChangedWatcher fileWatcher = new FileChangedWatcher();

		static Shader lastBindedShader;

		[Dependency]
		IDependencyManager dependency;

		Shader(Asset asset)
		{
			this.asset = asset;
			this.Uniforms = new UniformsManager();
		}

		void Load()
		{
			shaderPartHandles.Clear();
			shaderProgramHandle = GL.CreateProgram();

			var builder = dependency.Create<ShaderBuilder>();
			var success = true;
			builder.Load(asset);
			foreach (var r in builder.buildResults)
			{
				success &= AttachShader(r.shaderContents, r.shaderType, r.filePath);
			}

			FinalizeInit();

			if (success) debug.Info(typeof(Shader) + " " + asset + " loaded successfully");

			fileWatcher.WatchFile(asset.RealPath, (string newFilePath) =>
			{
				fileWatcher.StopAllWatchers();
				shouldReload = true;
			});

			Uniforms.MarkAllUniformsAsChanged();
			cache_uniformLocations.Clear();

			IsLoaded = true;
		}

		/// <summary>
		/// Reloads the shader if marked to reload, binds the shader, uploads all changed uniforms;
		/// </summary>
		public void Bind()
		{
			if (!IsLoaded)
			{
				Load();
			}
			else if (shouldReload)
			{
				debug.Info("Reloading " + asset.VirtualPath);
				Dispose();
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
				debug.Error("Vertex Shader failed!\nLog:\n" + logInfo);
			}

			int statusCode = 0;
			GL.GetShader(handle, ShaderParameter.CompileStatus, out statusCode);
			if (statusCode != 1)
			{
				debug.Error(type.ToString() + " :: " + source + "\n" + GL.GetShaderInfoLog(handle) + "\n in file: " + resource);
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
				debug.Error(n + "\n" + GL.GetProgramInfoLog(shaderProgramHandle));
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
				debug.Warning(asset + ", uniform block index " + name + " not found ", false);
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
					debug.Warning(asset + ", uniform " + name + " not found ", false);
				}
				cache_uniformLocations[name] = location;
			}
			return location;
		}

		public void Dispose()
		{
			foreach (var p in shaderPartHandles) GL.DeleteShader(p);
			GL.DeleteProgram(shaderProgramHandle);
			IsLoaded = false;
		}
	}
}