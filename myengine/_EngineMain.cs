using MyEngine.Components;
using Neitri;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
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

		public EngineMain() : base(
			1400,
			900,
			new GraphicsMode(),
			"MyEngine",
			GameWindowFlags.Default,
			DisplayDevice.Default,
			4,
			0,
			GraphicsContextFlags.ForwardCompatible
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
				Debug.Info(r.ToString() + ": " + GL.GetString(r));
			}
			//Debug.Info(StringName.Extensions.ToString() + ": " + GL.GetString(StringName.Extensions));

			// Other state
			GL.Enable(EnableCap.Texture2D);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			GL.Enable(EnableCap.Multisample);

			//GL.ClearColor(System.Drawing.Color.MidnightBlue);
			GL.ClearColor(System.Drawing.Color.Black);
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

		DeltaTimeManager eventThreadTime = new DeltaTimeManager();

		void EventThreadMain()
		{
			Debug.Tick("event");
			eventThreadTime.Tick();

			EventSystem.Raise(new MyEngine.Events.EventThreadUpdate(eventThreadTime));

			Thread.Sleep(5);
		}

		DeltaTimeManager renderThreadTime = new DeltaTimeManager();

		ulong frameCounter;

		void RenderMain()
		{
			if (Debug.CommonCVars.PauseRenderPrepare() == false)
			{
				renderManagerBackReady.Wait();
				renderManagerBackReady.Reset();
				var tmp = renderManagerFront;
				renderManagerFront = renderManagerBack;
				renderManagerBack = tmp;
				renderManagerPrepareNext.Set();
			}

			frameCounter++;
			Debug.AddValue("rendering / frames rendered", frameCounter);

			UpdateGPUMemoryInfo();

			Debug.Tick("rendering / main render");
			renderThreadTime.Tick();

			if (Debug.CommonCVars.ReloadAllShaders().EatBoolIfTrue())
			{
				Factory.ReloadAllShaders();
			}

			if (this.Focused) Input.Update();
			Debug.Update();

			EventSystem.Raise(new MyEngine.Events.InputUpdate(renderThreadTime));
			EventSystem.Raise(new MyEngine.Events.EventThreadUpdate(renderThreadTime));
			EventSystem.Raise(new MyEngine.Events.RenderUpdate(renderThreadTime));

			ubo.engine.totalElapsedSecondsSinceEngineStart = (float)stopwatchSinceStart.Elapsed.TotalSeconds;
			ubo.engine.gammaCorrectionTextureRead = 2.2f;
			ubo.engine.gammaCorrectionFinalColor = 1 / 2.2f;

			//GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

			{
				var scene = scenes[0];
				var camera = scene.mainCamera;
				var dataToRender = scene.DataToRender;

				renderManagerFront.SkyboxCubeMap = scene.skyBox;
				renderManagerFront.RenderAll(ubo, camera, dataToRender.Lights, camera.postProcessEffects);
			}

			SwapBuffers();

			GC.Collect();
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
			GL.GetInteger((GetPName)GPU_MEMORY_INFO_TOTAL_AVAILABLE_MEMORY_NVX, out totalAvailableKb);
			GL.GetInteger((GetPName)GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX, out currentAvailableKb);

			int total = totalAvailableKb / 1024;
			int used = (totalAvailableKb - currentAvailableKb) / 1024;
			Debug.AddValue("rendering / GPU memory used", $"{used}/{total} mb");
		}
	}
}