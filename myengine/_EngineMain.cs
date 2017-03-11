using MyEngine.Components;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyEngine
{
	public class EngineMain : GameWindow
	{
		[Dependency(Register = true)]
		public InputSystem Input { get; private set; }

		[Dependency(Register = true)]
		public Debug Debug { get; private set; }

		[Dependency(Register = true)]
		public FileSystem FileSystem { get; private set; }

		List<SceneSystem> scenes = new List<SceneSystem>();

		public IDependencyManager Dependency { get; private set; } = new Neitri.DependencyInjection.DependencyManager();

		[Dependency(Register = true)]
		public Events.EventSystem EventSystem { get; private set; }

		[Dependency(Register = true)]
		public Factory Factory { get; private set; }

		public string windowTitle = "Procedural Planets Generator";

		public EngineMain() : base(
			1400,
			900,
			new GraphicsMode(),
			"initializing",
			GameWindowFlags.Default,
			DisplayDevice.Default,
			4,
			3,
			GraphicsContextFlags.ForwardCompatible// | GraphicsContextFlags.Debug
		)
		{
			Dependency.Register(this);
			Dependency.BuildUp(this);

			Debug.Info("START"); // to have debug initialized before anything else

			System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;

			VSync = VSyncMode.Off;
			TargetRenderFrequency = 0;


			//Texture2D.InitTexture2D();
			ubo = new UniformBlock();
			//new PhysicsUsage.PhysicsManager();

			stopwatchSinceStart.Restart();

			//{
			//    var winForm = new Panels.DebugValuesTable();
			//    winForm.Show();
			//}
			renderManagerFront = Dependency.Create<RenderManager>();
			renderManagerBack = Dependency.Create<RenderManager>();

			RenderFrame += (sender, evt) =>
			{
				TryStartSecondary();
				RenderMain();

				/*
                var task1 = Task.Factory.StartNew(BuildRenderListMain);
                var task2 = Task.Factory.StartNew(EventThreadMain);
                var continuationTask = Task.Factory.ContinueWhenAll(new[] { task1, task2 }, task => Task.Factory.StartNew(RenderMain));
                continuationTask.Wait();
                */
			};

			Debug.CommonCVars.Fullscreen().ToogledByKey(OpenTK.Input.Key.F).OnChanged += (cvar) =>
			{
				if (cvar.Bool && WindowState != WindowState.Fullscreen)
					WindowState = WindowState.Fullscreen;
				else
					WindowState = WindowState.Normal;
			};

		}

		System.Diagnostics.Stopwatch stopwatchSinceStart = new System.Diagnostics.Stopwatch();

		public static UniformBlock ubo;

		public SceneSystem AddScene()
		{
			var s = new SceneSystem(this);
			AddScene(s);
			return s;
		}

		protected override void OnLoad(System.EventArgs e)
		{
			foreach (StringName r in System.Enum.GetValues(typeof(StringName)))
			{
				if (r == StringName.Extensions) break;
				var str = GL.GetString(r); MyGL.Check();
				Debug.Info(r.ToString() + ": " + str);
			}

			// Other state
			//GL.Enable(EnableCap.Texture2D); My.Check();
			//GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); My.Check();
			//GL.Enable(EnableCap.Multisample); My.Check();

			GL.ClearColor(System.Drawing.Color.Black); MyGL.Check();

		}

		protected override void OnUnload(EventArgs e)
		{
		}

		protected override void OnResize(EventArgs e)
		{
			var resizeEvent = new Events.WindowResized(Width, Height);
			EventSystem.Raise(resizeEvent);
			Debug.Info("Window resized to: width:" + resizeEvent.NewPixelWidth + " height:" + resizeEvent.NewPixelHeight);
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{

		}

		public void AddScene(SceneSystem scene)
		{
			EventSystem.PassEventsTo(scene.EventSystem);
			scenes.Add(scene);
		}

		public override void Exit()
		{
			if (IsDisposed) return;
			if (IsExiting) return;
			base.Exit();
		}

		bool secondaryAlreadyStarted = false;

		void TryStartSecondary()
		{
			if (secondaryAlreadyStarted) return;
			secondaryAlreadyStarted = true;

			//StartSecondaryThread(EventThreadMain);
			StartSecondaryThread(BuildRenderListMain);
		}

		void StartSecondaryThread(Action action)
		{
			var t = new Thread(() =>
			{
				while (this.IsDisposed == false && this.IsExiting == false)
				{
					action();
				}
			});

			t.Priority = ThreadPriority.Highest;
			t.IsBackground = true;
			t.Start();
		}

		ManualResetEventSlim renderManagerBackReady = new ManualResetEventSlim();
		ManualResetEventSlim renderManagerPrepareNext = new ManualResetEventSlim(true);
		RenderManager renderManagerFront;
		RenderManager renderManagerBack;

		void BuildRenderListMain()
		{
			renderManagerPrepareNext.Wait();
			renderManagerPrepareNext.Reset();
			if (Debug.CommonCVars.PauseRenderPrepare())
			{
				Debug.AddValue("rendering / render prepare", "paused");
			}
			else
			{
				Debug.Tick("rendering / render prepare");
				foreach (var scene in scenes)
				{
					var camera = scene.mainCamera;
					var dataToRender = scene.DataToRender;
					if (camera != null && dataToRender != null)
					{
						renderManagerBack.BuildRenderList(dataToRender.Renderers, camera);
					}
				}
			}
			renderManagerBackReady.Set();
		}

		FrameTime eventThreadTime = new FrameTime();

		//void EventThreadMain()
		//{
		//	Debug.Tick("event");
		//	eventThreadTime.Tick();

		//	EventSystem.Raise(new MyEngine.Events.EventThreadUpdate(eventThreadTime));

		//	Thread.Sleep(5);
		//}

		FrameTime renderThreadTime = new FrameTime();

		ulong frameCounter;

		void RenderMain()
		{
			renderThreadTime.FrameBegan();
			EventSystem.Raise(new MyEngine.Events.FrameStarted());

			this.Title = windowTitle + " " + renderThreadTime;
			Debug.Tick("rendering / main render");

			if (this.Focused) Input.Update();
			Debug.Update();

			EventSystem.Raise(new MyEngine.Events.InputUpdate(renderThreadTime));
			EventSystem.Raise(new MyEngine.Events.EventThreadUpdate(renderThreadTime));


			frameCounter++;
			Debug.AddValue("rendering / frames rendered", frameCounter);

			UpdateGPUMemoryInfo();

			if (Debug.CommonCVars.ReloadAllShaders().EatBoolIfTrue())
			{
				Factory.ReloadAllShaders();
			}

			ubo.engine.totalElapsedSecondsSinceEngineStart = (float)stopwatchSinceStart.Elapsed.TotalSeconds;
			ubo.engine.gammaCorrectionTextureRead = 2.2f;
			ubo.engine.gammaCorrectionFinalColor = 1 / 2.2f;

			//GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); My.Check();
			EventSystem.Raise(new MyEngine.Events.PreRenderUpdate(renderThreadTime));

			if (Debug.CommonCVars.PauseRenderPrepare() == false)
			{
				renderManagerBackReady.Wait();
				renderManagerBackReady.Reset();
				var tmp = renderManagerFront;
				renderManagerFront = renderManagerBack;
				renderManagerBack = tmp;
			}

			{
				var scene = scenes[0];
				var camera = scene.mainCamera;
				var dataToRender = scene.DataToRender;

				if (renderThreadTime.FpsPer1Sec > 30) renderManagerFront.SkyboxCubeMap = scene.skyBox;
				else renderManagerFront.SkyboxCubeMap = null;
				renderManagerFront.RenderAll(ubo, camera, dataToRender.Lights, camera.postProcessEffects);
			}	

			if (Debug.CommonCVars.PauseRenderPrepare() == false)
			{
				renderManagerPrepareNext.Set();
			}

			EventSystem.Raise(new MyEngine.Events.PostRenderUpdate(renderThreadTime));

			SwapBuffers();

			GC.Collect();
			Mesh.ProcessFinalizerQueue();

			EventSystem.Raise(new MyEngine.Events.FrameEnded());
		}


		void UpdateGPUMemoryInfo()
		{

			// http://developer.download.nvidia.com/opengl/specs/GL_NVX_gpu_memory_info.txt

			var GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX = 0x9047;
			var GPU_MEMORY_INFO_TOTAL_AVAILABLE_MEMORY_NVX = 0x9048;
			var GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX = 0x9049;
			var GPU_MEMORY_INFO_EVICTION_COUNT_NVX = 0x904A;
			var GPU_MEMORY_INFO_EVICTED_MEMORY_NVX = 0x904;

			int totalAvailableKb, currentAvailableKb;
			GL.GetInteger((GetPName)GPU_MEMORY_INFO_TOTAL_AVAILABLE_MEMORY_NVX, out totalAvailableKb); MyGL.Check();
			GL.GetInteger((GetPName)GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX, out currentAvailableKb); MyGL.Check();

			int total = totalAvailableKb / 1024;
			int used = (totalAvailableKb - currentAvailableKb) / 1024;
			Debug.AddValue("rendering / GPU memory used", $"{used}/{total} mb");
		}
	}
}