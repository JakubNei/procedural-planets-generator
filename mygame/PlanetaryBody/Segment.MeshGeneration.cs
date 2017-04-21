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
				surfaceMesh.BoundsNeedRecalculation = false;

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
				seaMesh.BoundsNeedRecalculation = false;

				var vertices = planetInfo.GetVerticesList();
				var indicies = planetInfo.GetIndiciesList();
				seaMesh.Vertices.SetData(vertices);
				seaMesh.TriangleIndicies.SetData(indicies);
			}


			Bounds bounds;
			{
				var o = offsetCenter;
				var c = 0;
				var n = NoElevationRange.Normal;

				bounds = new Bounds((NoElevationRange.CenterPos - o).ToVector3());

				bounds.Encapsulate((NoElevationRange.a + n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.b + n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.c + n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.CenterPos + n * c - o).ToVector3());

				bounds.Encapsulate((NoElevationRange.a - n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.b - n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.c - n * c - o).ToVector3());
				bounds.Encapsulate((NoElevationRange.CenterPos - n * c - o).ToVector3());
			}


			//if (Renderer != null) throw new Exception("something went terribly wrong, renderer should be null"); // or we marked segment for regeneration
			RendererSurface = planetInfo.Entity.AddComponent<CustomChunkMeshRenderer>();
			RendererSurface.RenderingMode = MyRenderingMode.DontRender;
			RendererSurface.segment = this;
			RendererSurface.Mesh = surfaceMesh;
			RendererSurface.Mesh.Bounds = bounds;
			RendererSurface.Offset += offsetCenter;
			RendererSurface.Material = planetInfo.SurfaceMaterial.CloneTyped();
			RendererSurface.Material.Uniforms.Set("param_offsetFromPlanetCenter", RendererSurface.Offset.ToVector3d());
			RendererSurface.Material.Uniforms.Set("param_remainderOffset", RendererSurface.Offset.Remainder());


			RendererSea = planetInfo.Entity.AddComponent<MeshRenderer>();
			RendererSea.RenderingMode = MyRenderingMode.DontRender;
			RendererSea.Mesh = seaMesh;
			RendererSea.Mesh.Bounds = bounds;
			RendererSea.ForcePassRasterizationCulling = true;
			RendererSea.Offset += offsetCenter;
			RendererSea.Material = planetInfo.seaMaterial.CloneTyped();
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