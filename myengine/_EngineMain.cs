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
	public class EngineMain : IDisposable
	{
		public InputSystem Input { get; private set; }

		public MyDebug Debug { get; private set; }

		public FileSystem FileSystem { get; private set; } = new FileSystem("../Resources/");

		List<SceneSystem> scenes = new List<SceneSystem>();

		public IDependencyManager Dependency { get; private set; } = new Neitri.DependencyInjection.DependencyManager();

		public Events.EventSystem EventSystem { get; private set; }

		[Dependency(Register = true)]
		public Factory Factory { get; private set; }

		const string defaultWindowTitle = "Procedural Planet Generator";
		public string WindowTitle { get { return gameWindow.Title; } set { gameWindow.Title = value; } }
		public WindowState WindowState { get { return gameWindow.WindowState; } set { gameWindow.WindowState = value; } }
		public bool CursorVisible { get { return gameWindow.CursorVisible; } set { gameWindow.CursorVisible = value; } }
		public bool ExitRequested { get; private set; }
		public bool Focused => gameWindow.Focused;

		public bool ShouldContinueRunning => gameWindow.IsStoppingOrStopped == false && ExitRequested == false;


		// to simulate OpenTk.GameWindow functionalty, see it's source https://github.com/mono/opentk/blob/master/Source/OpenTK/GameWindow.cs
		private MyGameWindow gameWindow;
		class MyGameWindow : GameWindow
		{
			public bool IsStoppingOrStopped => IsDisposed || IsExiting;
			public MyGameWindow(int width, int height, GraphicsMode mode, string title, GameWindowFlags options, DisplayDevice device, int major, int minor, GraphicsContextFlags flags)
			   : base(width, height, mode, title, options, device, major, minor, flags)
			{
			}
		}


		public EngineMain()
		{
			gameWindow = new MyGameWindow(
				1400,
				900,
				new GraphicsMode(),
				"initializing",
				GameWindowFlags.Default,
				DisplayDevice.Default,
				4,
				3,
				GraphicsContextFlags.ForwardCompatible/*| GraphicsContextFlags.Debug*/
			);
			gameWindow.VSync = VSyncMode.Off;

			Input = new InputSystem(this);
			Debug = new MyDebug(Input);
			EventSystem = new Events.EventSystem();

			Debug.Info("START"); // to have debug initialized before anything else

			Dependency.Register(FileSystem, Debug, Input, EventSystem, this);
			Dependency.BuildUp(this);

			System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;
			Thread.CurrentThread.Priority = ThreadPriority.Highest;
		}

		public void Run()
		{
			OnStart();
		}
		public void Exit()
		{
			ExitRequested = true;
		}

		Stopwatch stopwatchSinceStart = new System.Diagnostics.Stopwatch();

		public static UniformBlock ubo { get; set; }

		public SceneSystem NewScene()
		{
			var scene = new SceneSystem(this);
			EventSystem.PassEventsTo(scene.EventSystem);
			scenes.Add(scene);
			return scene;
		}

		void OnStart()
		{

			ubo = new UniformBlock();
			//new PhysicsUsage.PhysicsManager();

			stopwatchSinceStart.Restart();

			renderManagerFront = Dependency.Create<RenderManager>();
			renderManagerBack = Dependency.Create<RenderManager>();


			/*Debug.CommonCVars.VSync().ToogledByKey(OpenTK.Input.Key.V).OnChanged += (cvar) =>
			{
				if (cvar.Bool) VSync = VSyncMode.On;
				else VSync = VSyncMode.Off;
			};
			Debug.CommonCVars.VSync().InitializeWith(false);*/

			Debug.CommonCVars.Fullscreen().ToogledByKey(OpenTK.Input.Key.F).OnChanged += (cvar) =>
			{
				if (cvar.Bool && WindowState != WindowState.Fullscreen)
					WindowState = WindowState.Fullscreen;
				else
					WindowState = WindowState.Normal;
			};

			Debug.CommonCVars.SortRenderers().Bool = true;

			WindowTitle = defaultWindowTitle;



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

			gameWindow.Resize += (sender, e) => OnResize();
			OnResize();

			gameWindow.Visible = true;

			StartOtherThreads();

			while (ShouldContinueRunning)
			{
				MainLoop();
			}

			gameWindow.Exit();
		}

		ulong resizeEventVersion = 0;
		void OnResize()
		{
			resizeEventVersion++;
			var myVersion = resizeEventVersion;
			// we want to call this only during render thread, because someone might be playing with GL context, for example RenderManager
			EventSystem.Once<Events.FrameStarted>((evt) =>
			{
				if (myVersion == resizeEventVersion)
				{
					var resizeEvent = new Events.WindowResized(gameWindow.Width, gameWindow.Height);
					Debug.Info("Window resized to: width:" + resizeEvent.NewPixelWidth + " height:" + resizeEvent.NewPixelHeight);
					EventSystem.Raise(resizeEvent);
				}
			});
		}






		void StartOtherThreads()
		{
			//StartSecondaryThread(EventThreadMain);
			StartThreadLoop(BuildRenderListMain);
		}

		void StartThreadLoop(Action action)
		{
			var t = new Thread(() =>
			{
				while (ShouldContinueRunning)
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

		void MainLoop()
		{
			renderThreadTime.FrameBegan();
			EventSystem.Raise(new MyEngine.Events.FrameStarted());

			try
			{
				gameWindow.ProcessEvents();
			}
			catch (Exception e)
			{
				Debug.Error(e);
			}

			this.WindowTitle = defaultWindowTitle + " " + renderThreadTime;
			Debug.Tick("rendering / main render");

			// if window is not focused we dont want to have our character and camera responding to keyboard and mouse inputs
			if (this.Focused)
			{
				Input.Update();
				Debug.InputUpdate();
			}

			Debug.LogicUpdate();

			EventSystem.Raise(new MyEngine.Events.InputUpdate(renderThreadTime));
			EventSystem.Raise(new MyEngine.Events.EventThreadUpdate(renderThreadTime));

			if (ShouldContinueRunning == false) return;

			frameCounter++;
			Debug.AddValue("rendering / frames rendered", frameCounter);

			UpdateGPUMemoryInfo();

			if (Debug.CommonCVars.ReloadAllShaders().EatBoolIfTrue())
				Factory.ReloadAllShaders();

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
				renderManagerPrepareNext.Set();
			}

			{
				var scene = scenes[0];
				var camera = scene.mainCamera;
				var dataToRender = scene.DataToRender;

				//if (renderThreadTime.FpsPer1Sec > 30)
				renderManagerFront.SkyboxCubeMap = scene.skyBox;
				//else renderManagerFront.SkyboxCubeMap = null;
				renderManagerFront.RenderAll(ubo, camera, dataToRender.Lights, camera.postProcessEffects);
			}

			if (ShouldContinueRunning == false) return;

			gameWindow.SwapBuffers();

			EventSystem.Raise(new MyEngine.Events.PostRenderUpdate(renderThreadTime));


			//GC.Collect();
			Mesh.ProcessFinalizerQueue();

			EventSystem.Raise(new MyEngine.Events.FrameEnded(renderThreadTime));

			// TEST
			//while (renderThreadTime.CurrentFrameElapsedTimeFps > renderThreadTime.TargetFps) Thread.Sleep(5);
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

		public void Dispose()
		{
			gameWindow.Dispose();
		}
	}
}