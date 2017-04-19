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
	public partial class Planet
	{

		public Shader GenerateHeights => Factory.GetShader("shaders/planet.generateHeights.compute");
		public Shader GenerateBiomes => Factory.GetShader("shaders/planet.generateBiomes.compute");

		public Shader MoveSkirts => Factory.GetShader("planet.moveSkirts.compute");


		JobRunner jobRunner = new JobRunner();

		JobTemplate<Segment> jobTemplate;

		bool useSkirts;

		void InitializeJobTemplate()
		{
			useSkirts = Debug.GetCVar("generation / use skirts", true);
			bool moveSkirtsOnGPU = Debug.GetCVar("generation / move skirts on GPU", false);
			bool calculateNormalsOnGPU = Debug.GetCVar("generation / calculate normals on GPU", true);

			jobTemplate = new JobTemplate<Segment>();

			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				segment.CreateRendererAndBasicMesh();
			}, "vytvoření trojúhelníkové sítě a vykreslovací komponenty");

			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				var mesh = segment.Renderer.Mesh;
				mesh.EnsureIsOnGpu();
			}, "přesun trojúhelníkové sítě na grafickou kartu");


			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				var mesh = segment.Renderer.Mesh;

				var verticesStartIndexOffset = 0;
				var verticesCount = mesh.Vertices.Count;

				var range = segment.NoElevationRange;
				if (useSkirts)
					range = segment.NoElevationRangeModifiedForSkirts;

				var uniforms = GenerateHeights.Uniforms;

				config.SetTo(uniforms);
				uniforms.Set("param_offsetFromPlanetCenter", segment.Renderer.Offset.ToVector3d());
				uniforms.Set("param_numberOfVerticesOnEdge", ChunkNumberOfVerticesOnEdge);
				uniforms.Set("param_cornerPositionA", range.a);
				uniforms.Set("param_cornerPositionB", range.b);
				uniforms.Set("param_cornerPositionC", range.c);
				uniforms.Set("param_indiciesCount", mesh.TriangleIndicies.Count);
				uniforms.Set("param_verticesStartIndexOffset", verticesStartIndexOffset);

				GenerateHeights.Bind();

				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, mesh.VertexArray.GetVertexBuffer("biomes1").VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, mesh.VertexArray.GetVertexBuffer("biomes2").VboHandle); MyGL.Check();
				GL.DispatchCompute(verticesCount, 1, 1); MyGL.Check();

			}, "vygenerování výšek trojúhelníkové sítě na grafické kartě");

			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				var mesh = segment.Renderer.Mesh;
				mesh.Vertices.DownloadDataFromGPU();
			}, "stáhnutí trojúhelníkové sítě z grafické karty do hlavní paměti počítače");

			jobTemplate.AddTask(segment =>
			{
				var mesh = segment.Renderer.Mesh;
				mesh.RecalculateBounds();
				segment.CalculateRealVisibleRange();
			}, "vypočet obalového kvádru trojúhelníkové sítě");

			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				var mesh = segment.Renderer.Mesh;
				if (calculateNormalsOnGPU)
					CalculateNormalsOnGPU(mesh);
				else
					mesh.RecalculateNormals();
			}, "výpočet normál trojúhelníkové sítě na grafické kartě");

			if (useSkirts)
			{
				if (false && moveSkirtsOnGPU) // TODO: move skirts on gpu
				{
					var uniforms = MoveSkirts.Uniforms;

					var ed = GetEdgeVerticesIndexes();
					for (int i = 0; i < ed.Length; i++)
					{
						uniforms.Set("param_edgeVertexIndex[" + i + "]", ed[i]);
					}

					jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
					{
						var mesh = segment.Renderer.Mesh;
						var moveAmount = -segment.NoElevationRange.CenterPos.Normalized().ToVector3() * (float)segment.NoElevationRange.ToBoundingSphere().radius / 10;


						config.SetTo(uniforms);

						uniforms.Set("param_moveAmount", moveAmount);

						MoveSkirts.Bind();

						GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
						GL.DispatchCompute(ed.Length, 1, 1); MyGL.Check();

					}, "pokud jsou sukně zapnuty: posunutí vrcholú pro vytvoření sukní na grafické kartě");
				}
				else
				{
					jobTemplate.AddTask(segment =>
					{
						var mesh = segment.Renderer.Mesh;
						var moveAmount = -segment.NoElevationRange.CenterPos.Normalized().ToVector3() * (float)segment.NoElevationRange.ToBoundingSphere().radius / 10;
						foreach (var i in GetEdgeVerticesIndexes()) mesh.Vertices[i] += moveAmount;
					}, "pokud jsou sukně zapnuty: posunutí vrcholú pro vytvoření sukní na centrální procesorové jednotce");

					jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
					{
						var mesh = segment.Renderer.Mesh;
						mesh.Vertices.UploadDataToGPU();
					}, "pokud jsou sukně zapnuty: přesun upravené trojúhelníkové sítě zpět na grafickou kartu");
				}
			}
			/*
			jobTemplate.AddTask(WhereToRun.GPUThread, segment =>
			{
				var mesh = segment.Renderer.Mesh;

				var verticesStartIndexOffset = 0;
				var verticesCount = mesh.Vertices.Count;

				var range = segment.NoElevationRange;
				if (useSkirts)
					range = segment.NoElevationRangeModifiedForSkirts;

				var uniforms = GenerateBiomes.Uniforms;


				config.SetTo(uniforms);
				uniforms.Set("param_offsetFromPlanetCenter", segment.Renderer.Offset.ToVector3d());
				uniforms.Set("param_numberOfVerticesOnEdge", ChunkNumberOfVerticesOnEdge);
				uniforms.Set("param_cornerPositionA", range.a);
				uniforms.Set("param_cornerPositionB", range.b);
				uniforms.Set("param_cornerPositionC", range.c);
				uniforms.Set("param_indiciesCount", mesh.TriangleIndicies.Count);
				uniforms.Set("param_verticesStartIndexOffset", verticesStartIndexOffset);

				GenerateBiomes.Bind();

				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 0, mesh.Vertices.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 1, mesh.Normals.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 2, mesh.UVs.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 3, mesh.TriangleIndicies.VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 4, mesh.VertexArray.GetVertexBuffer("biomes1").VboHandle); MyGL.Check();
				GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, 5, mesh.VertexArray.GetVertexBuffer("biomes2").VboHandle); MyGL.Check();
				GL.DispatchCompute(verticesCount, 1, 1); MyGL.Check();

			}, "vygenerování biomů na grafické kartě");
			*/
			ulong chunksGenerated = 0;
			jobTemplate.AddTask(segment =>
			{
				segment.NotifyGenerationDone();

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
				Log.Info("generating " + s);

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
