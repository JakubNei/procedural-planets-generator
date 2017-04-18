using Neitri;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
{
	public class Factory : SingletonsPropertyAccesor
	{
		public Shader DefaultGBufferShader => GetShader("internal/deferred.gBuffer.PBR.shader");
		public Shader DefaultDepthGrabShader => GetShader("internal/depthGrab.default.shader");

		public Mesh SkyBoxMesh => GetMesh("internal/skybox.obj");
		public Mesh QuadMesh => GetMesh("internal/quad.obj");

		public Texture2D WhiteTexture => GetTexture2D("internal/white.*");
		public Texture2D GreyTexture => GetTexture2D("internal/grey.*");
		public Texture2D BlackTexture => GetTexture2D("internal/black.*");
		public Texture2D DefaultNormalMap => GetTexture2D("internal/normal.*");
		public Texture2D TestTexture => GetTexture2D("internal/test.*");
		

		ConcurrentDictionary<string, Shader> allShaders = new ConcurrentDictionary<string, Shader>();


		public Shader GetShader(string file)
		{
			Shader s;
			if (!allShaders.TryGetValue(file, out s))
			{
				s = new Shader(FileSystem.FileExistingFile(file));
				allShaders[file] = s;
			}
			return s;
		}

		public void ReloadAllShaders()
		{
			foreach (var s in allShaders)
			{
				s.Value.shouldReload = true;
			}
		}

		public Material NewMaterial()
		{
			return new Material();
		}


		ObjLoader objLoader = new ObjLoader();

		ConcurrentDictionary<string, Mesh> allMeshes = new ConcurrentDictionary<string, Mesh>();

		public Mesh GetMesh(string file, bool allowDuplicates = false)
		{
			//if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

			Mesh s;
			if (allowDuplicates || !allMeshes.TryGetValue(file, out s))
			{
				s = objLoader.Load(this.FileSystem.FileExistingFile(file));
				allMeshes[file] = s;
			}
			return s;
		}

		ConcurrentDictionary<string, Texture2D> allTexture2Ds = new ConcurrentDictionary<string, Texture2D>();

		public Texture2D GetTexture2D(string file)
		{
			var search = new GlobSearch(file);
			var texture = allTexture2Ds.FirstOrDefault(kvp => search.Matches(kvp.Key)).Value;

			if (texture == null)
			{
				var f = this.FileSystem.FileExistingFile(file);
				texture = new Texture2D(f);
				allTexture2Ds[f.VirtualPath] = texture;
			}
			return texture;
		}



		ConcurrentDictionary<string, Cubemap> allCubeMaps = new ConcurrentDictionary<string, Cubemap>();

		public Cubemap GetCubeMap(string[] files)
		{
			Cubemap s;
			var key = string.Join("###", files.Select((x) => x.ToString()));
			if (!allCubeMaps.TryGetValue(key, out s))
			{
				s = new Cubemap(FileSystem.Findfiles(files).ToArray());
				allCubeMaps[key] = s;
			}
			return s;
		}
	}
}