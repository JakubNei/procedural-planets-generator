using MyEngine;
using MyEngine.Components;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public class Root : ComponentWithShortcuts
	{
		public double radius { get; set; }
		public double radiusVariation = 20;

		/// <summary>
		/// Is guaranteeed to be odd (1, 3, 5, 7, ...)
		/// </summary>
		public int chunkNumberOfVerticesOnEdge = 20;

		public int subdivisionMaxRecurisonDepth = 10;
		public volatile Material planetMaterial;
		public double subdivisionSphereRadiusModifier { get; set; } = 1f;
		public double startingRadiusSubdivisionModifier = 1;

		double debugWeight
		{
			get
			{
				return DebugKeys.keyIK * 2 - 0.8;
			}
		}

		const bool debugSameHeightEverywhere = false; // DEBUG

		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		List<Chunk> rootChunks = new List<Chunk>();

		public Chunk.MeshGenerationService MeshGenerationService { get; private set; }

		public static Root instance;
		ProceduralMath proceduralMath;
		public Root(Entity entity) : base(entity)
		{
			proceduralMath = new ProceduralMath();

			instance = this;
			perlin = new PerlinD(5646);
			worley = new WorleyD(894984, WorleyD.DistanceFunction.Euclidian);

			MeshGenerationService = new Chunk.MeshGenerationService(entity.Debug);
		}

		public void Configure(double radius, double radiusVariation)
		{

			proceduralMath.Configure(radius, radiusVariation);

			this.radius = radius;
			this.radiusVariation = radiusVariation;
		}

		
		public SpehricalCoord CalestialToSpherical(Vector3d c) => SpehricalCoord.FromCalestial(c);
		public SpehricalCoord CalestialToSpherical(Vector3 c) => SpehricalCoord.FromCalestial(c);
		public Vector3d SphericalToCalestial(SpehricalCoord s) => s.ToCalestial();

		public Vector3d GetFinalPos(Vector3d calestialPos, int detailDensity = 1)
		{
			var s = CalestialToSpherical(calestialPos);
			s.altitude = GetHeight(calestialPos, detailDensity);
			return SphericalToCalestial(s);

			/*
			// this makes camera movement jiggery
			calestialPos.Normalize();
			calestialPos *= GetHeight(calestialPos, detailDensity);
			return calestialPos;
			*/
		}

		Stopwatch getHeight_sw = new Stopwatch();
		long getHeight_counter = 0;
		public double GetHeight(Vector3d calestialPos, int detailDensity = 1)
		{

			double ret;
#if PERF_TEXT
			getHeight_sw.Start();
#endif
			if (false)
			{
				return proceduralMath.GetHeight(calestialPos, detailDensity);
			}
			else
			{
				var initialPos = calestialPos.Normalized();
				var pos = initialPos;

				int octaves = 2;
				double freq = 10;
				double ampModifier = .05f;
				double freqModifier = 15;
				double result = 0.0f;
				double amp = radiusVariation;
				pos *= freq;
				for (int i = 0; i < octaves; i++)
				{
					result += perlin.Get3D(pos) * amp;
					pos *= freqModifier;
					amp *= ampModifier;
				}

				{
					// hill tops
					var p = perlin.Get3D(initialPos * 10.0f);
					if (p > 0) result -= p * radiusVariation * 2;
				}

				{
					// craters
					var p = worley.GetAt(initialPos * 2.0f, 1);
					result += MyMath.SmoothStep(0.0f, 0.1f, p[0]) * radiusVariation * 2;
				}

				result += radius;
				return result;
			}
#if PERF_TEXT
			getHeight_sw.Stop();
			getHeight_counter++;
			if (getHeight_counter <= 10)
			{
				getHeight_sw.Reset();
			}
			else
			{
				if (getHeight_counter % 50 == 0) Console.WriteLine(getHeight_sw.ElapsedTicks / (getHeight_counter - 10));
			}
#endif

			return ret;

			/*
			int octaves = 4;
			double sum = 0.5;
			double freq = 1.0, amp = 1.0;
			vec2 dsum = vec2(0);
			for (int i=0; i < octaves; i++)
			{
				Vector3 n = perlin.Get3D(calestialPos*freq);
				dsum += vec2(n.y, n.z);
				sum += amp * n.x / (1 + dot(dsum, dsum));
				freq *= lacunarity;
				amp *= gain;
			}
			return sum;
			*/
		}

		List<Vector3d> vertices;

		void FACE(int A, int B, int C)
		{
			var child = new Chunk(this, null);
			child.noElevationRange.a = vertices[A];
			child.noElevationRange.b = vertices[B];
			child.noElevationRange.c = vertices[C];
			child.realVisibleRange.a = GetFinalPos(child.noElevationRange.a);
			child.realVisibleRange.b = GetFinalPos(child.noElevationRange.b);
			child.realVisibleRange.c = GetFinalPos(child.noElevationRange.c);
			this.rootChunks.Add(child);
		}

		public void Start()
		{
			if (chunkNumberOfVerticesOnEdge % 2 == 0) chunkNumberOfVerticesOnEdge++;

			//detailLevel = (int)ceil(planetInfo.rootChunks[0].range.ToBoundingSphere().radius / 100);

			vertices = new List<Vector3d>();
			var indicies = new List<uint>();

			var r = this.radius / 2.0;

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
			FACE(0, 11, 5);
			FACE(0, 5, 1);
			FACE(0, 1, 7);
			FACE(0, 7, 10);
			FACE(0, 10, 11);

			// 5 adjacent faces
			FACE(1, 5, 9);
			FACE(5, 11, 4);
			FACE(11, 10, 2);
			FACE(10, 7, 6);
			FACE(7, 1, 8);

			// 5 faces around point 3
			FACE(3, 9, 4);
			FACE(3, 4, 2);
			FACE(3, 2, 6);
			FACE(3, 6, 8);
			FACE(3, 8, 9);

			// 5 adjacent faces
			FACE(4, 9, 5);
			FACE(2, 4, 11);
			FACE(6, 2, 10);
			FACE(8, 6, 7);
			FACE(9, 8, 1);
		}

		void StopMeshGenerationInChilds(Chunk chunk)
		{
			lock (chunk.childs)
			{
				foreach (var child in chunk.childs)
				{
					child.StopMeshGeneration();
					StopMeshGenerationInChilds(child);
				}
			}
		}

		void TrySubdivideToLevel_Generation(Chunk chunk, double tresholdWeight, int recursionDepth)
		{
			var cam = Entity.Scene.mainCamera;
			var weight = chunk.GetWeight(cam) + debugWeight + 0.1;
			if (recursionDepth > 0 && weight > tresholdWeight)
			//if (recursionDepth > 0 && GeometryUtility.Intersects(chunk.realVisibleRange, sphere))
			{
				chunk.SubDivide();
				chunk.StopMeshGeneration();
				lock (chunk.childs)
				{
					foreach (var child in chunk.childs)
					{
						TrySubdivideToLevel_Generation(child, tresholdWeight, recursionDepth - 1);
					}
				}
			}
			else
			{
				if (chunk.renderer == null)
				{
					chunk.RequestMeshGeneration();
				}
				StopMeshGenerationInChilds(chunk);
			}
		}

		void HideChilds(Chunk chunk)
		{
			lock (chunk.childs)
			{
				foreach (var child in chunk.childs)
				{
					if (child.renderer != null && child.renderer.RenderingMode != RenderingMode.DontRender) child.renderer.RenderingMode = RenderingMode.DontRender;
					HideChilds(child);
				}
			}
		}

		// return true if all childs are visible
		bool TrySubdivideToLevel_Visibility(Chunk chunk, double tresholdWeight, int recursionDepth)
		{
			var cam = Entity.Scene.mainCamera;
			var weight = chunk.GetWeight(cam) + debugWeight;

			//if (recursionDepth > 0 && GeometryUtility.Intersects(chunk.realVisibleRange, sphere))
			if (recursionDepth > 0 && weight > tresholdWeight)
			{
				var areChildrenFullyVisible = true;
				chunk.SubDivide();
				lock (chunk.childs)
				{
					foreach (var child in chunk.childs)
					{
						areChildrenFullyVisible &= TrySubdivideToLevel_Visibility(child, tresholdWeight, recursionDepth - 1);
					}
				}

				// hide only if all our childs are visible, they mighht still be generating
				if (areChildrenFullyVisible) if (chunk.renderer != null) chunk.renderer.RenderingMode = RenderingMode.DontRender;

				return areChildrenFullyVisible;
			}
			else
			{
				if (chunk.renderer == null) return false;

				// end node, we must show this one and hide all childs or parents, parents should already be hidden
				if (chunk.renderer.RenderingMode != RenderingMode.RenderGeometryAndCastShadows)
				{
					// is not visible
					// show it
					chunk.renderer.RenderingMode = RenderingMode.RenderGeometryAndCastShadows;

					// averge edge normals
					var toSmoothWith = new List<Chunk>();
					var s = chunk.realVisibleRange.ToBoundingSphere();
					s.radius *= 1.5f;
					GetVisibleChunksWithin(toSmoothWith, s);
					toSmoothWith.ForEach(c =>
					{
						chunk.SmoothEdgeNormalsWith(c);
					//c.SmoothEdgeNormalsBasedOn(chunk);
				});
				}

				// if visible, update final positions weight according to distance
				if (chunk.renderer.RenderingMode == RenderingMode.RenderGeometryAndCastShadows)
				{
					/*
					var camPos = (Scene.mainCamera.Transform.Position - this.Transform.Position).ToVector3();
					var d = chunk.renderer.Mesh.Vertices.FindClosest(p => p.Distance(camPos)).Distance(camPos);
					var e0 = sphere.radius / subdivisionSphereRadiusModifier_debugModified;
					var e1 = e0 * subdivisionSphereRadiusModifier_debugModified;
					var w = MyMath.SmoothStep(e0, e1, d);
					*/
					var w = MyMath.Clamp01(weight / tresholdWeight);
					chunk.renderer.Material.Uniforms.Set("param_finalPosWeight", (float)w);
				}

				HideChilds(chunk);

				return true;
			}
		}

		public void GetVisibleChunksWithin(List<Chunk> chunksResult, Sphere sphere)
		{
			foreach (var rootChunk in this.rootChunks)
			{
				rootChunk.GetVisibleChunksWithin(chunksResult, sphere);
			}
		}

		public void TrySubdivideOver(WorldPos pos)
		{
			if (Input.GetKey(OpenTK.Input.Key.K))
			{
				var all = new List<Chunk>();
				Console.WriteLine($"SMOOTH NORMALS gather chunks");
				GetVisibleChunksWithin(all, new Sphere(this.Center.ToVector3d(), double.MaxValue));
				Console.WriteLine($"SMOOTH NORMALS starting on {all.Count} chunks");

				foreach (var a in all)
				{
					foreach (var b in all)
					{
						if (a == b) continue;
						a.SmoothEdgeNormalsWith(b);
					}
				}
				Console.WriteLine($"SMOOTH NORMALS end");
			}

			var sphere = new Sphere((pos - Transform.Position).ToVector3d(), this.radius * startingRadiusSubdivisionModifier);
			foreach (var rootChunk in this.rootChunks)
			{
				TrySubdivideToLevel_Generation(rootChunk, startingRadiusSubdivisionModifier, this.subdivisionMaxRecurisonDepth);
				TrySubdivideToLevel_Visibility(rootChunk, startingRadiusSubdivisionModifier, this.subdivisionMaxRecurisonDepth);
			}
		}
	}
}