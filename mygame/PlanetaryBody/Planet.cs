using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;

namespace MyGame.PlanetaryBody
{
	public partial class Planet : ComponentWithShortcuts
	{
		public double RadiusMin => config.radiusMin;

		public Camera Camera => Entity.Scene.MainCamera;

		public int ChunkNumberOfVerticesOnEdge => config.chunkNumberOfVerticesOnEdge;
		public float WeightNeededToSubdivide => config.weightNeededToSubdivide;

		int subdivisionMaxRecurisonDepthCached = -1;
		public int SubdivisionMaxRecurisonDepth
		{
			get
			{
				if (subdivisionMaxRecurisonDepthCached == -1)
				{
					var planetCircumference = 2 * Math.PI * RadiusMin;
					var oneRootChunkCircumference = planetCircumference / 6.0f;

					subdivisionMaxRecurisonDepthCached = 0;
					while (oneRootChunkCircumference > config.stopSegmentRecursionAtWorldSize)
					{
						oneRootChunkCircumference /= 2;
						subdivisionMaxRecurisonDepthCached++;
					}
				}
				return subdivisionMaxRecurisonDepthCached;
			}
		}


		public Material SurfaceMaterial { get; set; }


		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		public List<Segment> rootSegments = new List<Segment>();

		public Config config;

		public long ID;

		public Material seaMaterial;

		//ProceduralMath proceduralMath;

		public void Initialize(Config config)
		{
			this.config = config;

			//proceduralMath = new ProceduralMath();

			perlin = new PerlinD(5646);
			worley = new WorleyD(894984, WorleyD.DistanceFunction.Euclidian);

			config.SetTo(SurfaceMaterial.Uniforms);
			InitializeRootSegments();
			InitializeJobTemplate();
			InitializePrepareLoop();

			
			// water sphere
			{
				var mat = seaMaterial = Factory.NewMaterial();
				config.SetTo(mat.Uniforms);
				mat.RenderShader = Factory.GetShader("shaders/planet.sea.glsl");
				mat.RenderShader.IsTransparent = true;
			}
		}

		CVar GetSurfaceHeightDebug => Debug.GetCVar("planets / debug / get surface height");

		public GeographicCoords CalestialToSpherical(Vector3d c) => GeographicCoords.ToGeographic(c);
		public GeographicCoords CalestialToSpherical(Vector3 c) => GeographicCoords.ToSpherical(c);
		public Vector3d SphericalToCalestial(GeographicCoords s) => s.ToPosition();

		public Vector3d GetFinalPos(Vector3d calestialPos, int detailDensity = 1)
		{
			return calestialPos.Normalized() * GetSurfaceHeight(calestialPos);
		}


		public double GetSurfaceHeight(Vector3d planetLocalPosition, int detailDensity = 1)
		{
			double height = -1;

			//planetLocalPosition.Normalize();

			var rayFromPlanet = new RayD(Vector3d.Zero, planetLocalPosition);

			var chunk = rootSegments.FirstOrDefault(c => rayFromPlanet.CastRay(c.NoElevationRange).DidHit);

			if (chunk != null)
			{
				//lock (chunk)
				{
					int safe = 100;
					while (chunk.Children.Count > 0 && chunk.Children.Any(c => c.IsGenerationDone) && safe-- > 0)
					{
						foreach (var child in chunk.Children)
						{
							if (child.IsGenerationDone && rayFromPlanet.CastRay(child.NoElevationRange).DidHit)
							{
								chunk = child;
							}
						}
					}
				}

				var chunkLocalPosition = (planetLocalPosition - chunk.NoElevationRange.CenterPos);

				height = chunk.GetHeight(chunkLocalPosition);
			}

			if (GetSurfaceHeightDebug)
			{
				if (height == -1)
				{
					height = RadiusMin;
					if (chunk == null)
						Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(1, 0, 0, 1));
					else
						Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(1, 1, 0, 1));
				}
				else
				{
					Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(0, 1, 0, 1));
				}
			}


			return height;
		}

		public WorldPos GetPosition(Vector3d planetLocalPosition, double atPlanetAltitude)
		{
			var sphericalPos = this.CalestialToSpherical(planetLocalPosition);
			sphericalPos.altitude = atPlanetAltitude;
			return this.Transform.Position + this.SphericalToCalestial(sphericalPos).ToVector3();
		}

		public WorldPos GetPosition(WorldPos towards, double atPlanetAltitude)
		{
			var planetLocalPosition = this.Transform.Position.Towards(towards).ToVector3d();
			var sphericalPos = this.CalestialToSpherical(planetLocalPosition);
			sphericalPos.altitude = atPlanetAltitude;
			return this.Transform.Position + this.SphericalToCalestial(sphericalPos).ToVector3();
		}


		void AddRootChunk(ulong id, List<Vector3d> vertices, int A, int B, int C)
		{
			var range = new TriangleD()
			{
				a = vertices[A],
				b = vertices[B],
				c = vertices[C]
			};
			var child = new Segment(this, range, null, id);
			this.rootSegments.Add(child);
		}

		private void InitializeRootSegments()
		{
			//detailLevel = (int)ceil(planetInfo.rootChunks[0].range.ToBoundingSphere().radius / 100);

			var vertices = new List<Vector3d>();
			var indicies = new List<uint>();

			var r = this.RadiusMin / 2.0;

			var t = (1 + MyMath.Sqrt(5.0)) / 2.0 * r;
			var d = r;

			vertices.Add(new Vector3d(-d, t, 0));
			vertices.Add(new Vector3d(d, t, 0));
			vertices.Add(new Vector3d(-d, -t, 0));
			vertices.Add(new Vector3d(d, -t, 0));

			vertices.Add(new Vector3d(0, -d, t));
			vertices.Add(new Vector3d(0, d, t));
			vertices.Add(new Vector3d(0, -d, -t));
			vertices.Add(new Vector3d(0, d, -t));

			vertices.Add(new Vector3d(t, 0, -d));
			vertices.Add(new Vector3d(t, 0, d));
			vertices.Add(new Vector3d(-t, 0, -d));
			vertices.Add(new Vector3d(-t, 0, d));

			// 5 faces around point 0
			AddRootChunk(0, vertices, 0, 11, 5);
			AddRootChunk(1, vertices, 0, 5, 1);
			AddRootChunk(2, vertices, 0, 1, 7);
			AddRootChunk(3, vertices, 0, 7, 10);
			AddRootChunk(4, vertices, 0, 10, 11);

			// 5 adjacent faces
			AddRootChunk(5, vertices, 1, 5, 9);
			AddRootChunk(6, vertices, 5, 11, 4);
			AddRootChunk(7, vertices, 11, 10, 2);
			AddRootChunk(8, vertices, 10, 7, 6);
			AddRootChunk(9, vertices, 7, 1, 8);

			// 5 faces around point 3
			AddRootChunk(10, vertices, 3, 9, 4);
			AddRootChunk(11, vertices, 3, 4, 2);
			AddRootChunk(12, vertices, 3, 2, 6);
			AddRootChunk(13, vertices, 3, 6, 8);
			AddRootChunk(14, vertices, 3, 8, 9);

			// 5 adjacent faces
			AddRootChunk(15, vertices, 4, 9, 5);
			AddRootChunk(16, vertices, 2, 4, 11);
			AddRootChunk(17, vertices, 6, 2, 10);
			AddRootChunk(18, vertices, 8, 6, 7);
			AddRootChunk(19, vertices, 9, 8, 1);

		}


		public void MarkForRegeneration()
		{
			GenerateBiomes.ShouldReload = true;
			GenerateSea.ShouldReload = true;
			GenerateSurface.ShouldReload = true;

			foreach (var s in rootSegments)
				s.MarkForRegeneration();
		}

	}
}