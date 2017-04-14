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
		public float SizeOnScreenNeededToSubdivide => config.sizeOnScreenNeededToSubdivide;

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


		public Material PlanetMaterial { get; set; }


		public WorldPos Center => Transform.Position;

		PerlinD perlin;
		WorleyD worley;

		public List<Segment> rootSegments = new List<Segment>();

		public Config config;


		//ProceduralMath proceduralMath;
		public override void OnAddedToEntity(Entity entity)
		{
			base.OnAddedToEntity(entity);


		}

		public void Initialize(Config config)
		{
			this.config = config;

			//proceduralMath = new ProceduralMath();

			perlin = new PerlinD(5646);
			worley = new WorleyD(894984, WorleyD.DistanceFunction.Euclidian);


			InitializeJobTemplate();
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


		void AddRootChunk(List<Vector3d> vertices, int A, int B, int C)
		{
			var range = new TriangleD()
			{
				a = vertices[A],
				b = vertices[B],
				c = vertices[C]
			};
			var child = new Segment(this, range, null);
			this.rootSegments.Add(child);
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

		void GatherWeights(WeightedSegmentsList toGenerate, Segment segment, int recursionDepth)
		{
			var weight = segment.GetGenerationWeight(Camera);

			if (segment.GenerationBegan == false)
			{
				toGenerate.Add(segment, weight);
			}

			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				if (weight > SizeOnScreenNeededToSubdivide)
					segment.CreateChildren();
				else
					segment.DeleteChildren();

				foreach (var child in segment.Children)
				{
					GatherWeights(toGenerate, child, recursionDepth + 1);
				}
			}
			else
			{
				//Log.Warn("recursion depth is over: " + SubdivisionMaxRecurisonDepth);
			}
		}


		// return true if all childs are visible
		// we can hide parent only once all 4 childs are generated
		// we have to show all 4 childs at once
		void UpdateVisibility(Segment segment, WeightedSegmentsList toGenerate, int recursionDepth)
		{
			if (recursionDepth < SubdivisionMaxRecurisonDepth)
			{
				var areAllChildsGenerated = segment.Children.Count > 0 && segment.Children.All(c => c.IsGenerationDone);

				// hide only if all our childs are visible, they might still be generating
				if (areAllChildsGenerated)
				{
					segment.Renderer?.SetRenderingMode(MyRenderingMode.DontRender);

					foreach (var child in segment.Children)
					{
						UpdateVisibility(child, toGenerate, recursionDepth + 1);
					}

                    return;
				}
			}

            segment.Renderer?.SetRenderingMode(MyRenderingMode.RenderGeometryAndCastShadows);

            if (Generateheights.Version != segment.meshGeneratedWithShaderVersion)
                toGenerate.Add(segment, float.MaxValue);
        }



		// new SortedList<double, Chunk>(ReverseComparer<double>.Default)
		class WeightedSegmentsList : Dictionary<Segment, double>
		{
			public Camera cam;

			public new void Add(Segment chunk, double weight)
			{
				PrivateAdd1(chunk, weight);

				// we have to generate all our parents first
				while (chunk.parent != null && chunk.parent.GenerationBegan == false)
				{
					chunk = chunk.parent;
					var w = chunk.GetGenerationWeight(cam);
					PrivateAdd1(chunk, Math.Max(w, weight));
				}
			}
			private void PrivateAdd1(Segment segment, double weight)
			{
				PrivateAdd2(segment, weight);

				// if we want to show this chunk, our neighbours have the same weight, because we cant be shown without our neighbours
				if (segment.parent != null)
				{
					foreach (var neighbour in segment.parent.Children)
					{
						if (neighbour.GenerationBegan == false)
						{
							var w = neighbour.GetGenerationWeight(cam);
							PrivateAdd2(neighbour, Math.Max(w, weight));
						}
					}
				}
			}
			private void PrivateAdd2(Segment segment, double weight)
			{
                double w;
                if (this.TryGetValue(segment, out w))
				{
					if (w > weight) return; // the weight already present is bigger, dont change it
				}

				this[segment] = weight;
			}
			public IEnumerable<Segment> GetWeighted()
			{
				return this.OrderByDescending(i => i.Value).Take(100).Select(i => i.Key);
			}
			public Segment GetMostImportantChunk()
			{
				return this.OrderByDescending(i => i.Value).FirstOrDefault().Key;
			}
		}

		Queue<Segment> toGenerateOrdered;
		WeightedSegmentsList toGenerate;
		public void TrySubdivideOver(WorldPos pos)
		{
			if (toGenerate == null)
				toGenerate = new WeightedSegmentsList() { cam = Camera };
			toGenerate.Clear();

			foreach (var rootSegment in rootSegments)
			{
				if (rootSegment.GenerationBegan == false)
				{
					// first generate rootCunks
					toGenerate.Add(rootSegment, float.MaxValue);
				}
				else
				{
					// then their children
					GatherWeights(toGenerate, rootSegment, 0);
				}
			}

			foreach (var rootSegment in this.rootSegments)
			{
				UpdateVisibility(rootSegment, toGenerate, 0);
			}


			Debug.AddValue("generation / segments to generate", toGenerate.Count);
			toGenerateOrdered = new Queue<Segment>(toGenerate.GetWeighted());


		}





		public Shader Generateheights => Factory.GetShader("shaders/planet.generateHeights.compute");
		UniformsData generateHeightsUniforms = new UniformsData();

		public Shader MoveSkirts => Factory.GetShader("planet.moveSkirts.compute");
		UniformsData moveSkirtsUniforms = new UniformsData();


		JobRunner jobRunner = new JobRunner();

		JobTemplate<Segment> jobTemplate;

		void InitializeJobTemplate()
		{
			bool useSkirts = Debug.GetCVar("generation / use skirts", true);
			bool moveSkirtsOnGPU = Debug.GetCVar("generation / move skirts on GPU", true);
			bool calculateNormalsOnGPU = Debug.GetCVar("generation / calculate normals on GPU", true);

			jobTemplate = new JobTemplate<Segment>();

			jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
			{
				chunk.CreateRendererAndBasicMesh();
			}, "vytvoření trojúhelníkové sítě a vykreslovací komponenty");

			jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
			{
				var mesh = chunk.Renderer.Mesh;
				mesh.EnsureIsOnGpu();
			}, "přesun trojúhelníkové sítě na grafickou kartu");


			//if (chunk.renderer == null && mesh != null) throw new Exception("concurrency problem");


			// splitting this is actually not needed as it this task takes least amount of time

			/*jobTemplate.AddSplittableTask(WhereToRun.GPUThread, (chunk, maxCount, index) =>
			{
				var mesh = chunk.Renderer.Mesh;

				var verticesStartIndexOffset = 0;
				var verticesCountMax = mesh.Vertices.Count;
				var verticesCount = verticesCountMax;

				if (maxCount > 1)
				{
					verticesStartIndexOffset = ((index / (float)maxCount) * verticesCountMax).FloorToInt();
					verticesCount = (verticesCountMax / (float)maxCount).FloorToInt();

					if (index == maxCount - 1) // last part
						verticesCount = verticesCountMax - verticesStartIndexOffset;
				}
				*/
			jobTemplate.AddTask(WhereToRun.GPUThread, (chunk) =>
			{
				var mesh = chunk.Renderer.Mesh;

				var verticesStartIndexOffset = 0;
				var verticesCount = mesh.Vertices.Count;

				var range = chunk.NoElevationRange;
				if (useSkirts)
					range = chunk.NoElevationRangeModifiedForSkirts;

				config.SetTo(generateHeightsUniforms);
				generateHeightsUniforms.Set("param_offsetFromPlanetCenter", chunk.Renderer.Offset.ToVector3d());
				generateHeightsUniforms.Set("param_numberOfVerticesOnEdge", ChunkNumberOfVerticesOnEdge);
				generateHeightsUniforms.Set("param_cornerPositionA", range.a);
				generateHeightsUniforms.Set("param_cornerPositionB", range.b);
				generateHeightsUniforms.Set("param_cornerPositionC", range.c);
				generateHeightsUniforms.Set("param_indiciesCount", mesh.TriangleIndicies.Count);
				generateHeightsUniforms.Set("param_verticesStartIndexOffset", verticesStartIndexOffset);

				generateHeightsUniforms.SendAllUniformsTo(Generateheights.Uniforms);
				Generateheights.Bind();

				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.DispatchCompute(verticesCount, 1, 1); MyGL.Check();

			}, "vygenerování výšek trojúhelníkové sítě na grafické kartě");

			jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
			{
				var mesh = chunk.Renderer.Mesh;
				mesh.Vertices.DownloadDataFromGPU();
			}, "stáhnutí trojúhelníkové sítě z grafické karty do hlavní paměti počítače");

			jobTemplate.AddTask(chunk =>
			{
				var mesh = chunk.Renderer.Mesh;
				mesh.RecalculateBounds();
				chunk.CalculateRealVisibleRange();
			}, "vypočet obalového kvádru trojúhelníkové sítě");

			jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
			{
				var mesh = chunk.Renderer.Mesh;
				if (calculateNormalsOnGPU)
					CalculateNormalsOnGPU(mesh);
				else
					mesh.RecalculateNormals();
			}, "výpočet normál trojúhelníkové sítě na grafické kartě");

			if (useSkirts)
			{
				if (moveSkirtsOnGPU)
				{
					var ed = GetEdgeVerticesIndexes();
					for (int i = 0; i < ed.Length; i++)
					{
						moveSkirtsUniforms.Set("param_edgeVertexIndex[" + i + "]", ed[i]);
					}

					jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
					{
						var mesh = chunk.Renderer.Mesh;
						var moveAmount = -chunk.NoElevationRange.CenterPos.Normalized().ToVector3() * (float)chunk.NoElevationRange.ToBoundingSphere().radius / 10;


						config.SetTo(moveSkirtsUniforms);

						moveSkirtsUniforms.Set("param_moveAmount", moveAmount);

						moveSkirtsUniforms.SendAllUniformsTo(MoveSkirts.Uniforms);
						MoveSkirts.Bind();

						GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
						GL.DispatchCompute(ed.Length, 1, 1); MyGL.Check();

					}, "pokud jsou sukně zapnuty: posunutí vrcholú pro vytvoření sukní na grafické kartě");
				}
				else
				{
					jobTemplate.AddTask(chunk =>
					{
						var mesh = chunk.Renderer.Mesh;
						var moveAmount = -chunk.NoElevationRange.CenterPos.Normalized().ToVector3() * (float)chunk.NoElevationRange.ToBoundingSphere().radius / 10;
						foreach (var i in GetEdgeVerticesIndexes()) mesh.Vertices[i] += moveAmount;
					}, "pokud jsou sukně zapnuty: posunutí vrcholú pro vytvoření sukní na centrální procesorové jednotce");

					jobTemplate.AddTask(WhereToRun.GPUThread, chunk =>
					{
						var mesh = chunk.Renderer.Mesh;
						mesh.Vertices.UploadDataToGPU();
					}, "pokud jsou sukně zapnuty: přesun upravené trojúhelníkové sítě zpět na grafickou kartu");
				}
			}

			ulong chunksGenerated = 0;
			jobTemplate.AddTask(chunk =>
			{
				chunk.meshGeneratedWithShaderVersion = Generateheights.Version;
				chunk.NotifyGenerationDone();

				chunksGenerated++;
				Debug.AddValue("generation / total segments generated", chunksGenerated);
				Debug.AddValue("generation / total time spent generating", Neitri.FormatUtils.SecondsToString(jobTemplate.SecondsTaken));
				Debug.AddValue("generation / average time spent generating", Neitri.FormatUtils.SecondsToString(jobTemplate.AverageSeconds));

			}, "ukončení generování");


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
			if (toGenerateOrdered == null) return;

			Func<double> secondLeftToUse;
			if (Debug.GetCVar("generation / limit generation by fps", true))
				secondLeftToUse = () => 1 / t.TargetFps - t.CurrentFrameElapsedSeconds;
			else
				secondLeftToUse = () => float.MaxValue;


			Func<IJob> jobFactory = () =>
			{
				Segment s = null;
				while (toGenerateOrdered.Count > 0 && s == null)
				{
					s = toGenerateOrdered.Dequeue();
					if (s.GenerationBegan) s = null;
				}

				if (s == null) return null;
				return jobTemplate.MakeInstanceWithData(s);
			};


			if (Debug.GetCVar("generation / print statistics report").EatBoolIfTrue())
			{
				Log.Trace(Environment.NewLine + jobTemplate.StatisticsReport());
			}

			jobRunner.GPUThreadTick(secondLeftToUse, jobFactory);
		}
	}
}