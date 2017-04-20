using MyEngine;
using MyEngine.Components;
using Neitri;
using OpenTK;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public partial class Segment
	{
		public bool GenerationBegan { get; private set; } = false;
		public bool IsGenerationDone { get; private set; } = false;

		public void CreateRendererAndBasicMesh()
		{
			lock (this)
			{
				if (GenerationBegan) throw new Exception("generation already began");
				GenerationBegan = true;
			}
			var offsetCenter = NoElevationRange.CenterPos;

			var surfaceMesh = new Mesh();

			{
				surfaceMesh.Name = this.ToString() + " surface";

				var vertices = planetInfo.GetVerticesList();
				var indicies = planetInfo.GetIndiciesList();
				surfaceMesh.Vertices.SetData(vertices);
				surfaceMesh.TriangleIndicies.SetData(indicies);

				var biomes1 = new Mesh.BufferObjectVector4();
				biomes1.SetData(planetInfo.GetDefaultBiomesList());
				surfaceMesh.VertexArray.AddVertexBuffer("biomes1", biomes1);

				var biomes2 = new Mesh.BufferObjectVector4();
				biomes2.SetData(planetInfo.GetDefaultBiomesList());
				surfaceMesh.VertexArray.AddVertexBuffer("biomes2", biomes2);
			}

			var seaMesh = new Mesh();
			{
				seaMesh.Name = this.ToString() + " sea";

				var vertices = planetInfo.GetVerticesList();
				var indicies = planetInfo.GetIndiciesList();
				seaMesh.Vertices.SetData(vertices);
				seaMesh.TriangleIndicies.SetData(indicies);
			}



			{
				var o = offsetCenter.ToVector3();
				var c = 0;
				var n = NoElevationRange.Normal.ToVector3();

				var bounds = new Bounds(NoElevationRange.CenterPos.ToVector3() - o);

				bounds.Encapsulate(NoElevationRange.a.ToVector3() + n * c - o);
				bounds.Encapsulate(NoElevationRange.b.ToVector3() + n * c - o);
				bounds.Encapsulate(NoElevationRange.c.ToVector3() + n * c - o);
				bounds.Encapsulate(NoElevationRange.CenterPos.ToVector3() + n * c - o);

				bounds.Encapsulate(NoElevationRange.a.ToVector3() - n * c - o);
				bounds.Encapsulate(NoElevationRange.b.ToVector3() - n * c - o);
				bounds.Encapsulate(NoElevationRange.c.ToVector3() - n * c - o);
				bounds.Encapsulate(NoElevationRange.CenterPos.ToVector3() - n * c - o);
			}


			//if (Renderer != null) throw new Exception("something went terribly wrong, renderer should be null"); // or we marked segment for regeneration
			RendererSurface = planetInfo.Entity.AddComponent<CustomChunkMeshRenderer>();
			RendererSurface.RenderingMode = MyRenderingMode.DontRender;
			RendererSurface.segment = this;
			RendererSurface.Mesh = surfaceMesh;
			RendererSurface.Offset += offsetCenter;
			RendererSurface.Material = planetInfo.PlanetMaterial.CloneTyped();
			RendererSurface.Material.Uniforms.Set("param_offsetFromPlanetCenter", RendererSurface.Offset.ToVector3d());
			RendererSurface.Material.Uniforms.Set("param_remainderOffset", RendererSurface.Offset.Remainder());


			RendererSea = planetInfo.Entity.AddComponent<MeshRenderer>();
			RendererSea.RenderingMode = MyRenderingMode.DontRender;
			RendererSea.Mesh = seaMesh;
			RendererSea.Offset += offsetCenter;
			RendererSea.Material = planetInfo.seaMaterial;
			RendererSea.ForcePassCulling = true;


		}

		public void DestroyRenderer()
		{
			lock (this)
			{
				GenerationBegan = false;
				IsGenerationDone = false;
			}
			if (RendererSurface != null)
			{
				RendererSurface.RenderingMode = MyRenderingMode.DontRender;
				planetInfo.Entity.DestroyComponent(RendererSurface);
				RendererSurface = null;
			}
			if (RendererSea != null)
			{
				RendererSea.RenderingMode = MyRenderingMode.DontRender;
				planetInfo.Entity.DestroyComponent(RendererSea);
				RendererSea = null;
			}
		}
	}
}