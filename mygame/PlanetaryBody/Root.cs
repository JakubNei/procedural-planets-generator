using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public class Root : ComponentWithShortcuts
	{
		public double radius { get; set; } // poloměr
		public double radiusVariation = 20;

        public HashSet<Chunk> toComputeShader = new HashSet<Chunk>();

		/// <summary>
		/// Is guaranteeed to be odd (1, 3, 5, 7, ...)
		/// </summary>
		public int chunkNumberOfVerticesOnEdge = 30;

		//public int subdivisionMaxRecurisonDepth = 10;
		int? subdivisionMaxRecurisonDepth = 0;
		public int SubdivisionMaxRecurisonDepth
		{
			get
			{
				if (!subdivisionMaxRecurisonDepth.HasValue)
				{
					//var planetCircumference = 2 * Math.PI * radius;
					//var oneRootChunkCircumference = planetCircumference / 6.0f;
					var oneRootChunkCircumference = rootChunks[0].noElevationRange.a.Distance(rootChunks[0].noElevationRange.b);

					subdivisionMaxRecurisonDepth = 0;
					while (oneRootChunkCircumference > 50)
					{
						oneRootChunkCircumference /= 2;
						subdivisionMaxRecurisonDepth++;
					}
				}
				return subdivisionMaxRecurisonDepth.Value;
			}
		}


		public volatile Material planetMaterial;
		public double subdivisionSphereRadiusModifier { get; set; } = 1f;
		public double startingRadiusSubdivisionModifier = 1;

		double DebugWeight => DebugKeys.keyIK * 2 - 0.8;


		const bool debugSameHeightEverywhere = false; // DEBUG

		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		public List<Chunk> rootChunks = new List<Chunk>();

        public Shader computeShader;

		//public Chunk.MeshGenerationService MeshGenerationService { get; private set; }

		GenerationStats stats;

		public static Root instance;
		//ProceduralMath proceduralMath;
		public Root(Entity entity) : base(entity)
		{
			//proceduralMath = new ProceduralMath();
			stats = new GenerationStats(Debug);

			instance = this;
			perlin = new PerlinD(5646);
			worley = new WorleyD(894984, WorleyD.DistanceFunction.Euclidian);

			//MeshGenerationService = new Chunk.MeshGenerationService(entity.Debug);

			Debug.CommonCVars.SmoothChunksEdgeNormals().ToogledByKey(OpenTK.Input.Key.N);


            computeShader = Factory.GetShader("shaders/planetGeneration.compute");

        }

        public void Configure(double radius, double radiusVariation)
		{
			// proceduralMath.Configure(radius, radiusVariation);

			this.radius = radius;
			this.radiusVariation = radiusVariation;
		}


		public SpehricalCoord CalestialToSpherical(Vector3d c) => SpehricalCoord.FromCalestial(c);
		public SpehricalCoord CalestialToSpherical(Vector3 c) => SpehricalCoord.FromCalestial(c);
		public Vector3d SphericalToCalestial(SpehricalCoord s) => s.ToCalestial();

		public Vector3d GetFinalPos(Vector3d calestialPos, int detailDensity = 1)
		{
            return calestialPos.Normalized() * radius;

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

		long getHeight_counter = 0;
		public double GetHeight(Vector3d calestialPos, int detailDensity = 1)
		{
            return radius;

			double ret;
#if PERF_TEXT
			getHeight_sw.Start();
#endif
			if (false)
			{
				// return proceduralMath.GetHeight(calestialPos, detailDensity);
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



		void AddRootChunk(List<Vector3d> vertices, int A, int B, int C)
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

			var vertices = new List<Vector3d>();
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
			AddRootChunk(vertices, 0, 11, 5);
			AddRootChunk(vertices, 0, 5, 1);
			AddRootChunk(vertices, 0, 1, 7);
			AddRootChunk(vertices, 0, 7, 10);
			AddRootChunk(vertices, 0, 10, 11);

			// 5 adjacent faces
			AddRootChunk(vertices, 1, 5, 9);
			AddRootChunk(vertices, 5, 11, 4);
			AddRootChunk(vertices, 11, 10, 2);
			AddRootChunk(vertices, 10, 7, 6);
			AddRootChunk(vertices, 7, 1, 8);

			// 5 faces around point 3
			AddRootChunk(vertices, 3, 9, 4);
			AddRootChunk(vertices, 3, 4, 2);
			AddRootChunk(vertices, 3, 2, 6);
			AddRootChunk(vertices, 3, 6, 8);
			AddRootChunk(vertices, 3, 8, 9);

			// 5 adjacent faces
			AddRootChunk(vertices, 4, 9, 5);
			AddRootChunk(vertices, 2, 4, 11);
			AddRootChunk(vertices, 6, 2, 10);
			AddRootChunk(vertices, 8, 6, 7);
			AddRootChunk(vertices, 9, 8, 1);
		}


		void DestroyChildren(Chunk chunk)
		{
			chunk.DestroyRenderer();
			foreach (var child in chunk.childs)
				DestroyChildren(child);
		}
		void HideChilds(Chunk chunk)
		{
			foreach (var child in chunk.childs)
			{
				child.renderer?.SetRenderingMode(MyRenderingMode.DontRender);
				child.renderer = null;
				HideChilds(child);
			}
		}

		void TrySubdivideToLevel_GatherWeights(ChunkWeightedList weightedList, Chunk chunk, int recursionDepth, double parentWeight)
		{
			var cam = Entity.Scene.mainCamera;
			var weight = chunk.GetWeight(cam);

			if (recursionDepth == 0) weight *= 100; // root chunks have epic weight

			if(weight < 0.3f)
			{
				HideChilds(chunk);
				return;
			}

			if (chunk.renderer == null)
				weightedList.Add(weight, chunk);

			// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
			if (chunk.parentChunk != null)
				foreach (var neighbour in chunk.parentChunk.childs)
					if (neighbour.renderer == null)
						weightedList.Add(weight, neighbour);


			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				foreach (var child in chunk.childs)
				{
					TrySubdivideToLevel_GatherWeights(weightedList, child, recursionDepth + 1, weight);
				}
			}
		}


		// return true if all childs are visible
		// we can hide parent only once all 4 childs are generated
		// we have to show all 4 childs at once
		void Chunks_UpdateVisibility(Chunk chunk, int recursionDepth)
		{
			var cam = Entity.Scene.mainCamera;

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				var areAllChildsGenerated = chunk.childs.All(c => c.renderer != null);

				// hide only if all our childs are visible, they mighht still be generating
				if (areAllChildsGenerated)
				{
					chunk.renderer?.SetRenderingMode(MyRenderingMode.DontRender);

					foreach (var child in chunk.childs)
					{
						Chunks_UpdateVisibility(child, recursionDepth + 1);
					}
				}
				else
				{
					chunk.renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);
				}
			}
			else
			{
				chunk.renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);
			}
		}

		public void GetVisibleChunksWithin(List<Chunk> chunksResult, Sphere sphere)
		{
			foreach (var rootChunk in this.rootChunks)
			{
				rootChunk.GetVisibleChunksWithin(chunksResult, sphere);
			}
		}

		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class ChunkWeightedList
		{
			List<Tuple<double, Chunk>> l = new List<Tuple<double, Chunk>>();
			public void Add(double weight, Chunk chunk)
			{
				l.Add(new Tuple<double, Chunk>(weight, chunk));
			}
			public Chunk GetHighestWeightChunk()
			{
				return l.OrderByDescending(i => i.Item1).FirstOrDefault()?.Item2;
			}
		}

		public void TrySubdivideOver(WorldPos pos)
		{
			if (Debug.CommonCVars.SmoothChunksEdgeNormals())
			{
				var all = new List<Chunk>();
				//Console.WriteLine($"SMOOTH NORMALS gather chunks");
				GetVisibleChunksWithin(all, new Sphere(this.Center.ToVector3d(), double.MaxValue));
				//Console.WriteLine($"SMOOTH NORMALS starting on {all.Count} chunks");

				foreach (var a in all)
				{
					foreach (var b in all)
					{
						if (a == b) continue;
						a.SmoothEdgeNormalsWith(b);
					}
				}
				//Console.WriteLine($"SMOOTH NORMALS end");
			}
			var weightedList = new ChunkWeightedList();
			var sphere = new Sphere((pos - Transform.Position).ToVector3d(), this.radius * startingRadiusSubdivisionModifier);
			foreach (var rootChunk in this.rootChunks)
				TrySubdivideToLevel_GatherWeights(weightedList, rootChunk, 0, 0);

			var toGenerate = weightedList.GetHighestWeightChunk();
			if (toGenerate != null)
			{
				while (toGenerate.parentChunk != null && toGenerate.parentChunk.renderer == null) // we have to generate our parent first
					toGenerate = toGenerate.parentChunk;

				stats.Start();
				toGenerate.CreateRendererAndGenerateMesh();
				stats.End();
				stats.Update();
				toGenerate.SubDivide();
                toComputeShader.Add(toGenerate);

                toGenerate.renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);
			}

            //foreach (var rootChunk in this.rootChunks) Chunks_UpdateVisibility(rootChunk, 0);

        }

        public class GenerationStats
		{
			ulong countChunksGenerated;
			TimeSpan timeSpentGenerating;
			Debug debug;
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			public GenerationStats(Debug debug)
			{
				this.debug = debug;
			}
			public void Update()
			{
				debug.AddValue("generation / total chunks generated", countChunksGenerated);
				debug.AddValue("generation / total time spent generating", timeSpentGenerating.TotalSeconds + " s");
				debug.AddValue("generation / average time spent generating", (timeSpentGenerating.TotalSeconds / (float)countChunksGenerated) + " s");
			}
			public void Start()
			{
				stopwatch.Reset();
				stopwatch.Start();
			}
			public void End()
			{
				stopwatch.Stop();
				timeSpentGenerating += stopwatch.Elapsed;
				countChunksGenerated++;
			}
		}


        public void OnRender(RenderUpdate r)
        {
            computeShader.Bind();
            foreach (var c in toComputeShader.ToArray())
            {
                var m = c.renderer.Mesh;
                if (m.Vertices.VboHandle == -1) continue;
                toComputeShader.Remove(c);
                GL.Uniform3(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "chunkOffset"), c.renderer.Offset.ToVector3());
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, m.Vertices.VboHandle); My.Check();
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, m.Normals.VboHandle); My.Check();
                GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, m.UVs.VboHandle); My.Check();
                GL.DispatchCompute(m.Vertices.Count, 1, 1); My.Check();
            }


        }



    }
}