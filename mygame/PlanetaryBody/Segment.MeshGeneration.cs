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
			var mesh = new Mesh();
			mesh.Name = this.ToString();


			var normals = mesh.Normals;


			var vertices = planetInfo.GetVerticesList();
			var indicies = planetInfo.GetIndiciesList();

			mesh.Vertices.SetData(vertices);
			mesh.TriangleIndicies.SetData(indicies);

			// tell every vertex where on the planet it is, so it can query from biomes splat map
			/*
			mesh.UVs.Clear();
			mesh.UVs.Capacity = mesh.Vertices.Count;
			for (int i = 0; i < mesh.Vertices.Count; i++)
			{
				var v = mesh.Vertices[i];
				var s = planetaryBody.CalestialToSpherical(v);
				mesh.UVs.Add(new Vector2((float)s.longitude01, (float)s.latitude01));
			}
			*/

			bool useSkirts = false;
			//useSkirts = true;
			if (useSkirts)
			{
				var skirtVertices = mesh.Duplicate(planetInfo.GetEdgeVerticesIndexes(), mesh.Vertices, mesh.Normals);
				var moveAmount = this.NoElevationRange.ToBoundingSphere().radius / 10;
				mesh.MoveVertices(skirtVertices, -this.NoElevationRange.Normal.ToVector3() * (float)moveAmount, mesh.Vertices);
			}

			{
				var o = offsetCenter.ToVector3();
				var c = 0;
				var n = NoElevationRange.Normal.ToVector3();

				mesh.Bounds = new Bounds(NoElevationRange.CenterPos.ToVector3() - o);

				mesh.Bounds.Encapsulate(NoElevationRange.a.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.b.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.c.ToVector3() + n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.CenterPos.ToVector3() + n * c - o);

				mesh.Bounds.Encapsulate(NoElevationRange.a.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.b.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.c.ToVector3() - n * c - o);
				mesh.Bounds.Encapsulate(NoElevationRange.CenterPos.ToVector3() - n * c - o);
			}

			if (Renderer != null) throw new Exception("something went terribly wrong, renderer should be null");
			Renderer = planetInfo.Entity.AddComponent<CustomChunkMeshRenderer>();
			Renderer.segment = this;
			Renderer.Mesh = mesh;
			Renderer.Offset += offsetCenter;
			Renderer.Material = planetInfo.PlanetMaterial.CloneTyped();
			Renderer.Material.Uniforms.Set("param_offsetFromPlanetCenter", Renderer.Offset.ToVector3());
		}

	}
}