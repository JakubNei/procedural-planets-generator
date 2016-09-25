using Neitri;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MyEngine
{
	public class Factory
	{
		public Shader DefaultGBufferShader => GetShader("internal/deferred.gBuffer.PBR.shader");
		public Shader DefaultDepthGrabShader => GetShader("internal/depthGrab.default.shader");

		public Mesh SkyBoxMesh => GetMesh("internal/skybox.obj");
		public Mesh QuadMesh => GetMesh("internal/quad.obj");

		public Texture2D whiteTexture => GetTexture2D("internal/white.png");
		public Texture2D greyTexture => GetTexture2D("internal/grey.png");
		public Texture2D blackTexture => GetTexture2D("internal/black.png");

		[Dependency]
		public IDependencyManager Dependency { get; private set; }

		Dictionary<string, Shader> allShaders = new Dictionary<string, Shader>();

		public Shader GetShader(string asset)
		{
			Shader s;
			if (!allShaders.TryGetValue(asset, out s))
			{
				s = Dependency.Create<Shader>(assetSystem.FindAsset(asset));
				allShaders[asset] = s;
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
			return Dependency.Create<Material>();
		}

		[Dependency(Register = true)]
		ObjLoader objLoader;

		[Dependency]
		AssetSystem assetSystem;

		Dictionary<string, Mesh> allMeshes = new Dictionary<string, Mesh>();

		public Mesh GetMesh(string asset, bool allowDuplicates = false)
		{
			//if (!resource.originalPath.EndsWith(".obj")) throw new System.Exception("Resource path does not end with .obj");

			Mesh s;
			if (allowDuplicates || !allMeshes.TryGetValue(asset, out s))
			{
				s = objLoader.Load(this.assetSystem.FindAsset(asset));
				allMeshes[asset] = s;
			}
			return s;
		}

		internal Dictionary<string, Texture2D> allTexture2Ds = new Dictionary<string, Texture2D>();

		public Texture2D GetTexture2D(string asset)
		{
			Texture2D s;
			if (!allTexture2Ds.TryGetValue(asset, out s))
			{
				s = new Texture2D(this.assetSystem.FindAsset(asset));
				allTexture2Ds[asset] = s;
			}
			return s;
		}

		internal Dictionary<string, Cubemap> allCubeMaps = new Dictionary<string, Cubemap>();

		public Cubemap GetCubeMap(string[] assets)
		{
			Cubemap s;
			var key = string.Join("###", assets.Select((x) => x.ToString()));
			if (!allCubeMaps.TryGetValue(key, out s))
			{
				s = new Cubemap(assetSystem.FindAssets(assets).ToArray());
				allCubeMaps[key] = s;
			}
			return s;
		}
	}
}