using MyEngine;
using MyEngine.Components;
using MyEngine.Events;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public class Root : ComponentWithShortcuts
	{
		public double RadiusMax { get; set; } // poloměr
		public double RadiusVariation { get; set; }

		public HashSet<Chunk> toComputeShader = new HashSet<Chunk>();

		/// <summary>
		/// Is guaranteeed to be odd (1, 3, 5, 7, ...)
		/// </summary>
		public int chunkNumberOfVerticesOnEdge = 50;

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
					var oneRootChunkCircumference = rootChunks[0].noElevationRange.a.Distance(rootChunks[0].noElevationRange.b);

					subdivisionMaxRecurisonDepth = 0;
					while (oneRootChunkCircumference > 100)
					{
						oneRootChunkCircumference /= 2;
						subdivisionMaxRecurisonDepth++;
					}
				}
				return subdivisionMaxRecurisonDepth;
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

			this.RadiusMax = radius;
			this.RadiusVariation = radiusVariation;
		}


		public SpehricalCoord CalestialToSpherical(Vector3d c) => SpehricalCoord.FromCalestial(c);
		public SpehricalCoord CalestialToSpherical(Vector3 c) => SpehricalCoord.FromCalestial(c);
		public Vector3d SphericalToCalestial(SpehricalCoord s) => s.ToCalestial();

		public Vector3d GetFinalPos(Vector3d calestialPos, int detailDensity = 1)
		{
			return calestialPos.Normalized() * GetHeight(calestialPos);
		}


		public double GetHeight(Vector3d calestialPos, int detailDensity = 1)
		{
			return RadiusMax;
		}



		void AddRootChunk(List<Vector3d> vertices, int A, int B, int C)
		{
			var child = new Chunk(this, null);
			child.noElevationRange.a = vertices[A];
			child.noElevationRange.b = vertices[B];
			child.noElevationRange.c = vertices[C];
			this.rootChunks.Add(child);
		}

		public void Start()
		{
			if (chunkNumberOfVerticesOnEdge % 2 == 0) chunkNumberOfVerticesOnEdge++;

			//detailLevel = (int)ceil(planetInfo.rootChunks[0].range.ToBoundingSphere().radius / 100);

			var vertices = new List<Vector3d>();
			var indicies = new List<uint>();

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


		void Chunks_GatherWeights(ChunkWeightedList list, Chunk chunk, int recursionDepth, double parentWeight)
		{
			var cam = Entity.Scene.mainCamera;
			var distanceToCam = chunk.GetSizeOnCamera(cam);

			if (chunk.renderer == null)
			{
				list.Add(distanceToCam, chunk);
			}

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				var threshold = RadiusVariation + RadiusMax / (recursionDepth + 1);

				if (distanceToCam < threshold * 5)
					chunk.CreteChildren();
				else
					chunk.DeleteChildren();

				foreach (var child in chunk.childs)
				{
					Chunks_GatherWeights(list, child, recursionDepth + 1, distanceToCam);
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
				var areAllChildsGenerated = chunk.childs.Count > 0 && chunk.childs.All(c => c.renderer != null);

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


		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class ChunkWeightedList
		{
			List<Tuple<double, Chunk>> l = new List<Tuple<double, Chunk>>();
			public void Add(double priority, Chunk chunk)
			{
				l.Add(new Tuple<double, Chunk>(priority, chunk));
			}
			public Chunk GetMostImportantChunk()
			{
				return l.OrderBy(i => i.Item1).FirstOrDefault()?.Item2;
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
					Chunks_GatherWeights(weightedList, rootChunk, 0, 0);
				toGenerate = weightedList.GetMostImportantChunk();
			}

			if (toGenerate != null)
			{
				while (toGenerate.parentChunk != null && toGenerate.parentChunk.renderer == null) // we have to generate our parent first
					toGenerate = toGenerate.parentChunk;

				// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
				if (toGenerate.parentChunk != null)
					foreach (var neighbour in toGenerate.parentChunk.childs)
						if (neighbour.renderer == null)
							Generate(neighbour);

				Generate(toGenerate);
			}

			 foreach (var rootChunk in this.rootChunks) Chunks_UpdateVisibility(rootChunk, 0);

		}

		void Generate(Chunk toGenerate)
		{
			stats.Start();
			toGenerate.CreateRendererAndGenerateMesh();
			stats.End();
			stats.Update();
			toComputeShader.Add(toGenerate);

			//toGenerate.renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);
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



		List<Mesh> toCalculateNormals = new List<Mesh>();
		public void OnRender(RenderUpdate r)
		{
			if (computeShader.Bind())
			{
				foreach (var chunk in toComputeShader.ToArray())
				{
					var mesh = chunk.renderer?.Mesh;
					if (mesh == null || mesh.Vertices.VboHandle == -1) continue;
					toComputeShader.Remove(chunk);
					GL.Uniform1(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "planetRadiusMax"), (float)RadiusMax); My.Check();
					GL.Uniform1(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "planetRadiusVariation"), (float)RadiusVariation); My.Check();
					GL.Uniform3(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "offsetFromPlanetCenter"), chunk.renderer.Offset.ToVector3()); My.Check();
					GL.Uniform1(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "numberOfVerticesOnEdge"), chunkNumberOfVerticesOnEdge); My.Check();
					GL.Uniform3(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "cornerPositionA"), chunk.noElevationRange.a.ToVector3()); My.Check();
					GL.Uniform3(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "cornerPositionB"), chunk.noElevationRange.b.ToVector3()); My.Check();
					GL.Uniform3(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "cornerPositionC"), chunk.noElevationRange.c.ToVector3()); My.Check();
					GL.Uniform1(GL.GetUniformLocation(computeShader.ShaderProgramHandle, "indiciesCount"), mesh.TriangleIndicies.Count); My.Check();
					GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); My.Check();
					GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); My.Check();
					GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); My.Check();
					GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); My.Check();
					GL.DispatchCompute(mesh.Vertices.Count, 1, 1); My.Check();
					toCalculateNormals.Add(mesh);

					GL.BindBuffer(BufferTarget.ShaderStorageBuffer, mesh.Vertices.VboHandle); My.Check();
					var intPtr = GL.MapBuffer(BufferTarget.ShaderStorageBuffer, BufferAccess.ReadOnly); My.Check();
					mesh.Vertices.SetData(intPtr, mesh.Vertices.Count);
					GL.UnmapBuffer(BufferTarget.ShaderStorageBuffer);
					mesh.RecalculateBounds();

					break;
				}
			}
			toCalculateNormals.ForEach(CalculateNormalsOnGPU);
			toCalculateNormals.Clear();
		}

		public void CalculateNormalsOnGPU(Mesh mesh)
		{
			Shader calculateNormalsShader = Factory.GetShader("internal/recalculateNormals.compute");
			if (calculateNormalsShader.Bind())
			{
				GL.Uniform1(GL.GetUniformLocation(calculateNormalsShader.ShaderProgramHandle, "indiciesCount"), mesh.TriangleIndicies.Count); My.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); My.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); My.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); My.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); My.Check();
				GL.DispatchCompute(mesh.Vertices.Count, 1, 1); My.Check();
			}
		}


	}
}