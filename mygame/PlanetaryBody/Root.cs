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

namespace MyGame.PlanetaryBody
{
	public class Root : ComponentWithShortcuts
	{
		public double RadiusMax => config.radiusMax;
		public double RadiusVariation => config.radiusVariation;
		public Texture2D baseHeightMap => config.baseHeightMap;

		public HashSet<Chunk> toBeginGeneration = new HashSet<Chunk>();

		/// <summary>
		/// Is guaranteeed to be odd (1, 3, 5, 7, ...)
		/// </summary>
		public int chunkNumberOfVerticesOnEdge = 80;
		public float sizeOnScreenNeededToSubdivide = 1f;

		//public int subdivisionMaxRecurisonDepth = 10;
		int subdivisionMaxRecurisonDepth = -1;
		public int SubdivisionMaxRecurisonDepth
		{
			get
			{
				if (subdivisionMaxRecurisonDepth < 0)
				{
					//var planetCircumference = 2 * Math.PI * radius;
					//var oneRootChunkCircumference = planetCircumference / 6.0f;
					var oneRootChunkCircumference = RadiusMax;

					subdivisionMaxRecurisonDepth = 0;
					while (oneRootChunkCircumference > 200)
					{
						oneRootChunkCircumference /= 2;
						subdivisionMaxRecurisonDepth++;
					}
				}
				return subdivisionMaxRecurisonDepth;
			}
		}


		public Material PlanetMaterial { get; set; }
		public double SubdivisionSphereRadiusModifier { get; set; } = 1f;
		public double StartingRadiusSubdivisionModifier { get; set; } = 1;

		double DebugWeight => DebugKeys.keyIK * 2 - 0.8;


		const bool debugSameHeightEverywhere = false; // DEBUG

		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		public System.Collections.Generic.List<Chunk> rootChunks = new System.Collections.Generic.List<Chunk>();

		public Config config;

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

		public void SetConfig(Config config)
		{
			this.config = config;
		}


		public SpehricalCoord CalestialToSpherical(Vector3d c) => SpehricalCoord.FromCalestial(c);
		public SpehricalCoord CalestialToSpherical(Vector3 c) => SpehricalCoord.FromCalestial(c);
		public Vector3d SphericalToCalestial(SpehricalCoord s) => s.ToCalestial();

		public Vector3d GetFinalPos(Vector3d calestialPos, int detailDensity = 1)
		{
			return calestialPos.Normalized() * GetSurfaceHeight(calestialPos);
		}



		public double GetSurfaceHeight(Vector3d planetLocalPosition, int detailDensity = 1)
		{
			double height = -1;

			//planetLocalPosition.Normalize();

			var rayFromPlanet = new RayD(Vector3d.Zero, planetLocalPosition);

			var chunk = rootChunks.FirstOrDefault(c => rayFromPlanet.CastRay(c.NoElevationRange).DidHit);

			if(chunk != null)
			{
				int safe = 100;
				while (chunk.childs.Count > 0 && chunk.childs.Any(c => c.isGenerationDone) && safe-- > 0)
				{
					foreach (var child in chunk.childs)
					{
						if (child.isGenerationDone && rayFromPlanet.CastRay(child.NoElevationRange).DidHit)
						{
							chunk = child;
						}
					}
				}

				var chunkLocalPosition = (planetLocalPosition - chunk.NoElevationRange.CenterPos);

				height = chunk.GetHeight(chunkLocalPosition);
			}
			if (height == -1)
			{
				height = RadiusMax;
				if(chunk == null)
					Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(1, 0, 0, 1));
				else
					Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(1, 1, 0, 1));
			}
			else
			{
				Scene.DebugShere(GetPosition(planetLocalPosition, height), 1000, new Vector4(0, 1, 0, 1));
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


		void AddRootChunk(System.Collections.Generic.List<Vector3d> vertices, int A, int B, int C)
		{
			var range = new TriangleD();
			range.a = vertices[A];
			range.b = vertices[B];
			range.c = vertices[C];
			var child = new Chunk(this, range, null);
			this.rootChunks.Add(child);
		}

		public void Initialize()
		{			
			PlanetMaterial.Uniforms.Set("param_planetRadiusMax", (float)RadiusMax);
			PlanetMaterial.Uniforms.Set("param_planetRadiusVariation", (float)RadiusVariation);

			InitializeRootSegments();
		}

		private void InitializeRootSegments()
		{
			if (chunkNumberOfVerticesOnEdge % 2 == 0) chunkNumberOfVerticesOnEdge++;

			//detailLevel = (int)ceil(planetInfo.rootChunks[0].range.ToBoundingSphere().radius / 100);

			var vertices = new System.Collections.Generic.List<Vector3d>();
			var indicies = new System.Collections.Generic.List<uint>();

			var r = this.RadiusMax / 2.0;

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

		void Chunks_GatherWeights(ChunkWeightedList list, Chunk chunk, int recursionDepth)
		{
			var cam = Entity.Scene.mainCamera;
			var sizeOnScreen = chunk.GetSizeOnScreen(cam);

			if (!chunk.generationBegan)
			{
				list.Add(sizeOnScreen, chunk);
			}

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				if (sizeOnScreen > sizeOnScreenNeededToSubdivide)
					chunk.CreteChildren();
				else
					chunk.DeleteChildren();

				foreach (var child in chunk.childs)
				{
					Chunks_GatherWeights(list, child, recursionDepth + 1);
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
				var areAllChildsGenerated = chunk.childs.Count > 0 && chunk.childs.All(c => c.isGenerationDone);

				// hide only if all our childs are visible, they might still be generating
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


		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class ChunkWeightedList
		{
			System.Collections.Generic.List<Tuple<double, Chunk>> l = new System.Collections.Generic.List<Tuple<double, Chunk>>();
			public void Add(double weight, Chunk chunk)
			{
				l.Add(new Tuple<double, Chunk>(weight, chunk));
			}
			public Chunk GetMostImportantChunk()
			{
				return l.OrderByDescending(i => i.Item1).FirstOrDefault()?.Item2;
			}
		}


		public void TrySubdivideOver(WorldPos pos)
		{
			var weightedList = new ChunkWeightedList();

			var toGenerate = rootChunks.FirstOrDefault(c => c.renderer == null); // first generate rootCunks;

			if (toGenerate == null) // then find the most important child chunk
			{
				//var sphere = new Sphere((pos - Transform.Position).ToVector3d(), this.RadiusMax * startingRadiusSubdivisionModifier);
				foreach (var rootChunk in this.rootChunks)
					Chunks_GatherWeights(weightedList, rootChunk, 0);
				toGenerate = weightedList.GetMostImportantChunk();
			}

			if (toGenerate != null)
			{
				while (toGenerate.parentChunk != null && !toGenerate.parentChunk.generationBegan) // we have to generate our parent first
					toGenerate = toGenerate.parentChunk;

				// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
				if (toGenerate.parentChunk != null)
					foreach (var neighbour in toGenerate.parentChunk.childs)
						if (!neighbour.generationBegan)
							toGenerate = neighbour;


				ToComputeShader(toGenerate);
			}

			foreach (var rootChunk in this.rootChunks) Chunks_UpdateVisibility(rootChunk, 0);


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


		Chunk nextChunkToGenerate;
		void ToComputeShader(Chunk chunk)
		{
			nextChunkToGenerate = chunk;
		}

		UniformsManager computeShaderUniforms = new UniformsManager();
		void ExecuteComputeShader(Chunk chunk)
		{
			stats.Start();

			chunk.CreateRendererAndBasicMesh();
			var mesh = chunk.renderer.Mesh;
			mesh.EnsureIsOnGpu();

			if (chunk.renderer == null && mesh != null) throw new Exception("concurency problem");

			computeShaderUniforms.Set("planetRadiusMax", (float)RadiusMax);
			computeShaderUniforms.Set("planetRadiusVariation", (float)RadiusVariation);
			computeShaderUniforms.Set("offsetFromPlanetCenter", chunk.renderer.Offset.ToVector3());
			computeShaderUniforms.Set("numberOfVerticesOnEdge", chunkNumberOfVerticesOnEdge);
			computeShaderUniforms.Set("cornerPositionA", chunk.NoElevationRange.a.ToVector3());
			computeShaderUniforms.Set("cornerPositionB", chunk.NoElevationRange.b.ToVector3());
			computeShaderUniforms.Set("cornerPositionC", chunk.NoElevationRange.c.ToVector3());
			computeShaderUniforms.Set("indiciesCount", mesh.TriangleIndicies.Count);
			computeShaderUniforms.Set("param_baseHeightMap", baseHeightMap);

			computeShaderUniforms.SendAllUniformsTo(computeShader.Uniforms);
			computeShader.Bind();

			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
			GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
			GL.DispatchCompute(mesh.Vertices.Count, 1, 1); MyGL.Check();
			toCalculateNormals.Add(mesh);

			mesh.Vertices.DownloadDataFromGpuToRam();
			mesh.RecalculateBounds();

			chunk.CalculateRealVisibleRange();
			chunk.isGenerationDone = true;

			stats.End();
			stats.Update();

			toCalculateNormals.Add(chunk.renderer.Mesh);


			toCalculateNormals.ForEach(CalculateNormalsOnGPU);
			toCalculateNormals.Clear();
		}

		System.Collections.Generic.List<Mesh> toCalculateNormals = new System.Collections.Generic.List<Mesh>();

		public void CalculateNormalsOnGPU(Mesh mesh)
		{
			Shader calculateNormalsShader = Factory.GetShader("internal/recalculateNormals.compute");
			if (calculateNormalsShader.Bind())
			{
				GL.Uniform1(GL.GetUniformLocation(calculateNormalsShader.ShaderProgramHandle, "indiciesCount"), mesh.TriangleIndicies.Count); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.DispatchCompute(mesh.Vertices.Count, 1, 1); MyGL.Check();
			}
		}


		public void GPUThreadUpdate()
		{
			if (nextChunkToGenerate != null)
			{
				ExecuteComputeShader(nextChunkToGenerate);
				nextChunkToGenerate = null;
			}
		}


	}
}