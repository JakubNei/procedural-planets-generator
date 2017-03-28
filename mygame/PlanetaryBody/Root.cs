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
	public partial class Root : ComponentWithShortcuts
	{
		public double RadiusMin => config.radiusMin;

		public int ChunkNumberOfVerticesOnEdge => 50;
		public float SizeOnScreenNeededToSubdivide => 0.3f;

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
					while (oneRootChunkCircumference > 100)
					{
						oneRootChunkCircumference /= 2;
						subdivisionMaxRecurisonDepthCached++;
					}
				}
				return subdivisionMaxRecurisonDepthCached;
			}
		}


		public Material PlanetMaterial { get; set; }

		double DebugWeight => DebugKeys.keyIK * 2 - 0.8;


		const bool debugSameHeightEverywhere = false; // DEBUG

		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		public List<Chunk> rootChunks = new List<Chunk>();

		public Config config;

		public Shader computeShader;

		GenerationStats stats;

		//ProceduralMath proceduralMath;
		public Root(Entity entity) : base(entity)
		{
			//proceduralMath = new ProceduralMath();
			stats = new GenerationStats(Debug);

			perlin = new PerlinD(5646);
			worley = new WorleyD(894984, WorleyD.DistanceFunction.Euclidian);


			InitializeJobTemplate();
		}

		public void SetConfig(Config config)
		{
			this.config = config;
		}


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

			var chunk = rootChunks.FirstOrDefault(c => rayFromPlanet.CastRay(c.NoElevationRange).DidHit);

			if (chunk != null)
			{
				lock (chunk)
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
				}

				var chunkLocalPosition = (planetLocalPosition - chunk.NoElevationRange.CenterPos);

				height = chunk.GetHeight(chunkLocalPosition);
			}


			const bool getSurfaceHeightDebug = false;
			if (getSurfaceHeightDebug)
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


		void AddRootChunk(List<Vector3d> vertices, int A, int B, int C)
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
			config.SetTo(PlanetMaterial.Uniforms);
			InitializeRootSegments();
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

			if (!chunk.GenerationBegan)
			{
				list.Add(sizeOnScreen, chunk);
			}

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				if (sizeOnScreen > SizeOnScreenNeededToSubdivide)
					chunk.CreteChildren();
				else
					chunk.DeleteChildren();

				foreach (var child in chunk.childs)
				{
					Chunks_GatherWeights(list, child, recursionDepth + 1);
				}
			}
			else
			{
				Debug.Warning("recursion depth is over: " + SubdivisionMaxRecurisonDepth);
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
					DoRenderChunk(chunk);
				}
			}
			else
			{
				Debug.Warning("recursion depth is over: " + SubdivisionMaxRecurisonDepth);
				DoRenderChunk(chunk);
			}
		}

		void DoRenderChunk(Chunk chunk)
		{
			chunk.renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);

			if (computeShader.Version != chunk.meshGeneratedWithShaderVersion)
				EnqueueChunkForGeneration(chunk);
		}


		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class ChunkWeightedList
		{
			List<Tuple<double, Chunk>> l = new List<Tuple<double, Chunk>>();
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
				while (toGenerate.parentChunk != null && !toGenerate.parentChunk.GenerationBegan) // we have to generate our parent first
					toGenerate = toGenerate.parentChunk;

				// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
				if (toGenerate.parentChunk != null)
					foreach (var neighbour in toGenerate.parentChunk.childs)
						if (!neighbour.GenerationBegan)
							toGenerate = neighbour;


				EnqueueChunkForGeneration(toGenerate);
			}

			foreach (var rootChunk in this.rootChunks) Chunks_UpdateVisibility(rootChunk, 0);
		}




		public class GenerationStats
		{
			ulong countChunksGenerated;
			TimeSpan timeSpentGenerating;
			MyDebug debug;
			System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
			public GenerationStats(MyDebug debug)
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

		public class ProfilerEvent
		{
			string name;
			public ProfilerEvent(string name)
			{
				this.name = name;
			}
			public ProfilerEvent MakeChild(string name = null)
			{
				return new ProfilerEvent(name);
			}
			public void Start()
			{

			}
			public void Stop()
			{

			}
		}


		UniformsData computeShaderUniforms = new UniformsData();


		public enum WhereToRun
		{
			GPUThread,
			DoesNotMatter,
		}

		public class JobRunner
		{
			List<IJob> jobs = new List<IJob>();
			Dictionary<IJob, double> timesOutOfTime = new Dictionary<IJob, double>();
			public void AddJob(IJob job)
			{
				lock (jobs)
					jobs.Add(job);
			}
			public void RemoveNotStarted()
			{
				lock (jobs)
					jobs.RemoveAll(j => j.IsStarted == false);
			}
			public void GPUThreadTick(FrameTime ft, Func<double> secondLeftToUse)
			{
				while (jobs.Count > 0 && secondLeftToUse() > 0)
				{
					int jobsRan = 0;

					IJob[] orderedJobs;
					lock (jobs)
					{
						jobs.RemoveAll(j => j.ShouldRemove);
						orderedJobs = jobs.OrderByDescending(j => j.NextGPUThreadTickWillTakeSeconds()).ToArray();
					}

					foreach (var job in orderedJobs)
					{
						if (job.ShouldExecute)
						{
							while (job.NextGPUThreadTickWillTakeSeconds() < secondLeftToUse() && job.GPUThreadTick())
								jobsRan++;
						}
					}

					lock (jobs)
						jobs.RemoveAll(j => j.ShouldRemove);

					if (jobsRan == 0) break;
				}


				MyDebug.Instance.AddValue("jobs count", jobs.Count);
			}
		}

		public interface IJob
		{
			bool IsStarted { get; }
			bool ShouldExecute { get; }
			bool ShouldRemove { get; }
			double NextGPUThreadTickWillTakeSeconds();
			bool GPUThreadTick();
		}

		public class JobTemplate<TData>
		{
			class JobTask
			{
				public Action<TData> normalAction;
				public Action<TData, int, int> splittableAction; // splitCount, splitIndex
				public WhereToRun whereToRun;

				public TimeSpan timeTaken;

				public bool firstRunDone;
				public ulong timesExecuted;
				public double avergeSeconds
				{
					get
					{
						if (firstRunDone && timesExecuted > 0)
							return timeTaken.TotalSeconds / timesExecuted;
						return 0;
					}
				}
			}
			List<JobTask> tasksToRun = new List<JobTask>();

			public string Name { get; set; }
			public JobTemplate()
			{

			}
			public void AddSplittableTask(Action<TData, int, int> action) => AddSplittableTask(WhereToRun.DoesNotMatter, action);
			public void AddSplittableTask(WhereToRun whereToRun, Action<TData, int, int> action)
			{
				tasksToRun.Add(new JobTask() { splittableAction = action, whereToRun = whereToRun });
			}
			public void AddTask(Action<TData> action) => AddTask(WhereToRun.DoesNotMatter, action);
			public void AddTask(WhereToRun whereToRun, Action<TData> action)
			{
				tasksToRun.Add(new JobTask() { normalAction = action, whereToRun = whereToRun });
			}
			public IJob MakeInstanceWithData(TData data)
			{
				return new JobInstance(this, data);
			}
			class JobInstance : IJob
			{
				public bool ShouldExecute => !ShouldRemove;
				public bool ShouldRemove => IsFinished || IsAborted || IsFaulted;

				public bool IsStarted => currentTaskIndex > 0;
				public bool IsFinished => currentTaskIndex >= parent.tasksToRun.Count && (lastTask == null || lastTask.IsCompleted);
				public bool IsAborted { get; private set; }
				public bool IsFaulted { get; private set; }
				public Exception Exception { get; private set; }

				Task lastTask;
				int currentTaskIndex;

				readonly TData data;
				readonly JobTemplate<TData> parent;

				public JobInstance(JobTemplate<TData> parent, TData data)
				{
					this.parent = parent;
					this.data = data;
				}

				public bool GPUThreadTick()
				{
					if (!ShouldExecute) return false;
					if (lastTask != null && lastTask.IsCompleted == false) return false;
					lastTask = null;

					var jobTask = parent.tasksToRun[currentTaskIndex];

					if (jobTask.normalAction != null)
					{
						Action action = () =>
						{
							var stopWatch = Stopwatch.StartNew();
							try
							{
								jobTask.normalAction(data);
							}
							catch (Exception e)
							{
								IsFaulted = true;
								Exception = e;
							}
							if (jobTask.firstRunDone)
							{
								jobTask.timeTaken += stopWatch.Elapsed;
								jobTask.timesExecuted++;
							}
							else
							{
								jobTask.firstRunDone = true;
							}
						};
						if (jobTask.whereToRun == WhereToRun.GPUThread)
							action();
						else
							lastTask = Task.Run(action);
					}

					currentTaskIndex++;
					return true;
				}
				public void Abort()
				{
					IsAborted = true;
				}

				public double NextGPUThreadTickWillTakeSeconds()
				{
					if (IsFinished) return 0;
					var jobTask = parent.tasksToRun[currentTaskIndex];
					if (jobTask.whereToRun == WhereToRun.GPUThread) return jobTask.avergeSeconds;
					return 0;
				}
			}

		}


		JobRunner jobRunner = new JobRunner();
		ProfilerEvent profiler = new ProfilerEvent("chunk generation");

		JobTemplate<Chunk> generationJobTemplate;

		void InitializeJobTemplate()
		{
			generationJobTemplate = new JobTemplate<Chunk>();

			generationJobTemplate.AddTask(WhereToRun.GPUThread, (chunk) =>
			{
				chunk.CreateRendererAndBasicMesh();
			});
			generationJobTemplate.AddTask(WhereToRun.GPUThread, (chunk) =>
			{
				var mesh = chunk.renderer.Mesh;
				mesh.EnsureIsOnGpu();
			});
			//if (chunk.renderer == null && mesh != null) throw new Exception("concurency problem");


			computeShader = Factory.GetShader("shaders/planetGeneration.compute");

			generationJobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
			{
				var mesh = chunk.renderer.Mesh;
				config.SetTo(computeShaderUniforms);

				computeShaderUniforms.Set("param_offsetFromPlanetCenter", chunk.renderer.Offset.ToVector3());
				computeShaderUniforms.Set("param_numberOfVerticesOnEdge", ChunkNumberOfVerticesOnEdge);
				computeShaderUniforms.Set("param_cornerPositionA", chunk.NoElevationRange.a.ToVector3());
				computeShaderUniforms.Set("param_cornerPositionB", chunk.NoElevationRange.b.ToVector3());
				computeShaderUniforms.Set("param_cornerPositionC", chunk.NoElevationRange.c.ToVector3());
				computeShaderUniforms.Set("param_indiciesCount", mesh.TriangleIndicies.Count);
				computeShaderUniforms.Set("param_verticesStartIndexOffset", 0);

				computeShaderUniforms.SendAllUniformsTo(computeShader.Uniforms);
				computeShader.Bind();

				stats.Start();

				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.DispatchCompute(mesh.Vertices.Count, 1, 1); MyGL.Check();

				stats.End();
				stats.Update();
			});

			generationJobTemplate.AddTask(WhereToRun.GPUThread, (chunk) =>
			{
				var mesh = chunk.renderer.Mesh;
				mesh.Vertices.DownloadDataFromGpuToRam();
			});

			generationJobTemplate.AddTask((chunk) =>
			{
				var mesh = chunk.renderer.Mesh;
				mesh.RecalculateBounds();
			});

			generationJobTemplate.AddTask((chunk) =>
			{
				chunk.CalculateRealVisibleRange();
				chunk.meshGeneratedWithShaderVersion = computeShader.Version;
			});

			generationJobTemplate.AddTask(WhereToRun.GPUThread, (chunk) =>
			{
				var mesh = chunk.renderer.Mesh;
				CalculateNormalsOnGPU(mesh);
				chunk.isGenerationDone = true;
			});
		}


		void EnqueueChunkForGeneration(Chunk chunk)
		{
			var job = generationJobTemplate.MakeInstanceWithData(chunk);
			jobRunner.AddJob(job);
		}


		public void CalculateNormalsOnGPU(Mesh mesh)
		{
			Shader calculateNormalsShader = Factory.GetShader("internal/calculateNormalsAndTangents.compute.glsl");
			if (calculateNormalsShader.Bind())
			{
				GL.Uniform1(GL.GetUniformLocation(calculateNormalsShader.ShaderProgramHandle, "param_indiciesCount"), mesh.TriangleIndicies.Count); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.Tangents.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.DispatchCompute(mesh.Vertices.Count, 1, 1); MyGL.Check();
			}
		}

		public void GPUThreadTick(FrameTime t)
		{
			jobRunner.GPUThreadTick(t, () => 1 / t.TargetFps - t.CurrentFrameElapsedSeconds);
		}


	}
}