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
		public Texture2D segmentNormalMap;

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

			/*
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
			*/


			var d = planetInfo.config.normalMapDimensions;

			d = ((int)(d / 16)) * 16;


			segmentNormalMap = new Texture2D(d, d);
			segmentNormalMap.UseMipMaps = false;

			//if (Renderer != null) throw new Exception("something went terribly wrong, renderer should be null"); // or we marked segment for regeneration
			RendererSurface = planetInfo.Entity.AddComponent<CustomChunkMeshRenderer>();
			RendererSurface.RenderingMode = MyRenderingMode.DontRender;
			RendererSurface.segment = this;
			RendererSurface.Mesh = surfaceMesh;
			//RendererSurface.Mesh.Bounds = bounds;
			RendererSurface.Offset += offsetCenter;
			RendererSurface.Material = planetInfo.SurfaceMaterial.CloneTyped();
			SetCommonUniforms(RendererSurface.Material.Uniforms);




			RendererSea = planetInfo.Entity.AddComponent<MeshRenderer>();
			RendererSea.RenderingMode = MyRenderingMode.DontRender;
			RendererSea.Mesh = seaMesh;
			//RendererSea.Mesh.Bounds = bounds;
			RendererSea.ForcePassRasterizationCulling = true;
			RendererSea.Offset += offsetCenter;
			RendererSea.Material = planetInfo.seaMaterial.CloneTyped();
			SetCommonUniforms(RendererSea.Material.Uniforms);
		}


		void SetCommonUniforms(UniformsData uniforms)
		{
			uniforms.Set("param_offsetFromPlanetCenter", RendererSurface.Offset.ToVector3d());
			uniforms.Set("param_remainderOffset", RendererSurface.Offset.Remainder());
			uniforms.Set("param_generation", subdivisionDepth);
			uniforms.Set("param_segmentId", (int)this.ID);
			uniforms.Set("param_segmentNormalMap", segmentNormalMap);
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