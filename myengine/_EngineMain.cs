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
	public static class Singletons
	{
		public static InputSystem Input { get; private set; }
		public static MyDebug Debug { get; private set; }
		public static Factory Factory { get; private set; }
		public static ILog Log { get; private set; }
		public static FileSystem FileSystem { get; private set; } = new FileSystem("../Resources/");
		public static Events.EventSystem EventSystem { get; private set; }

		public static void Start(EngineMain engine)
		{
			var log = new Neitri.Logging.LogConsole();
			log.messageFormatter = (logEntry) =>
				string.Format("[{0}][{1}] {2}",
					DateTime.Now.ToString("HH:mm:ss.fff"),
					logEntry.Caller,
					logEntry.Message
				);
			Log = log;

			Input = new InputSystem(engine);
			Debug = new MyDebug();
			EventSystem = new Events.EventSystem();
			Factory = new Factory();

		}
	}

	public class SingletonsPropertyAccesor
	{
		public InputSystem Input => Singletons.Input;
		public MyDebug Debug => Singletons.Debug;
		public Factory Factory => Singletons.Factory;
		public ILog Log => Singletons.Log;
		public FileSystem FileSystem => Singletons.FileSystem;
		public Events.EventSystem EventSystem => Singletons.EventSystem;

	}

	public class EngineMain : SingletonsPropertyAccesor, IDisposable
	{

		List<SceneSystem> scenes = new List<SceneSystem>();


		const string defaultWindowTitle = "Procedural Planet Generator";
		public string WindowTitle { get { return gameWindow.Title; } set { gameWindow.Title = value; } }
		public WindowState WindowState { get { return gameWindow.WindowState; } set { gameWindow.WindowState = value; } }
		public bool CursorVisible { get { return gameWindow.CursorVisible; } set { gameWindow.CursorVisible = value; } }
		public bool ExitRequested { get; private set; }
		public bool Focused => gameWindow.Focused;

		public bool ShouldContinueRunning => gameWindow.IsStoppingOrStopped == false && ExitRequested == false;

		CVar PauseRenderPrepare => Debug.GetCVar("rendering / debug / pause render prepare");
		CVar TargetFps => Debug.GetCVar("rendering / fps / target fps", 60);

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

			Singletons.Start(this);
			Singletons.Log.Info("START");

			gameWindow.VSync = VSyncMode.Off;


			//System.Diagnostics.Process.GetCurrentProcess().PriorityClass = System.Diagnostics.ProcessPriorityClass.High;
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
			scenes.Add(scene);
			return scene;
		}

		void OnStart()
		{

			ubo = new UniformBlock();
			//new PhysicsUsage.PhysicsManager();

			stopwatchSinceStart.Restart();

			renderManagerFront = new RenderManager();
			renderManagerBack = new RenderManager();


			/*Debug.CommonCVars.VSync().ToogledByKey(OpenTK.Input.Key.V).OnChanged += (cvar) =>
			{
				if (cvar.Bool) VSync = VSyncMode.On;
				else VSync = VSyncMode.Off;
			};
			Debug.CommonCVars.VSync().InitializeWith(false);*/

			Debug.GetCVar("rendering / fullscreen").OnChangedAndNow((cvar) =>
			{
				if (cvar.Bool && WindowState != WindowState.Fullscreen)
					WindowState = WindowState.Fullscreen;
				else
					WindowState = WindowState.Normal;
			});


			WindowTitle = defaultWindowTitle;



			foreach (StringName r in System.Enum.GetValues(typeof(StringName)))
			{
				if (r == StringName.Extensions) break;
				var str = GL.GetString(r); MyGL.Check();
				Log.Info(r.ToString() + ": " + str);
			}

			// Other state
			//GL.Enable(EnableCap.Texture2D); My.Check();
			//GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest); My.Check();
			//GL.Enable(EnableCap.Multisample); My.Check();		

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
					Log.Info("Window resized to: width:" + resizeEvent.NewPixelWidth + " height:" + resizeEvent.NewPixelHeight);
					EventSystem.Raise(resizeEvent);
				}
			});
		}




		void StartOtherThreads()
		{
			var t = new Thread(() =>
			{
				while (ShouldContinueRunning)
				{
					BuildRenderListMain();
				}
			});
			t.Name = "prepare render";
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
			if (PauseRenderPrepare)
			{
				Debug.AddValue("rendering / render prepare", "paused");
			}
			else
			{
				Debug.Tick("rendering / render prepare");
				foreach (var scene in scenes)
				{
					var camera = scene.MainCamera;
					var dataToRender = scene.DataToRender;
					if (camera != null && dataToRender != null)
					{
						try
						{
							renderManagerBack.PrepareRender(dataToRender, camera);
						}
						catch (Exception e)
						{
							Log.Exception(e);
						}
					}
				}
			}
			renderManagerBackReady.Set();
		}

		FrameTime eventThreadTime = new FrameTime();
		FrameTime renderThreadTime = new FrameTime();


		void MainLoop()
		{
			renderThreadTime.FrameBegan();
			renderThreadTime.TargetFps = TargetFps;
			Debug.AddValue("rendering / frames rendered", renderThreadTime.FrameCounter);

			EventSystem.Raise(new MyEngine.Events.FrameStarted());

			try
			{
				gameWindow.ProcessEvents();
			}
			catch (Exception e)
			{
				Log.Error(e);
			}

			this.WindowTitle = defaultWindowTitle + " - " + renderThreadTime;
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


			UpdateGPUMemoryInfo();

			if (Debug.GetCVar("rendering / debug / reload all shaders").EatBoolIfTrue())
				Factory.ReloadAllShaders();

			ubo.engine.totalElapsedSecondsSinceEngineStart = (float)stopwatchSinceStart.Elapsed.TotalSeconds;
			ubo.engine.gammaCorrectionTextureRead = 2.2f;
			ubo.engine.gammaCorrectionFinalColor = 1 / 2.2f;

			//GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit); My.Check();
			EventSystem.Raise(new MyEngine.Events.PreRenderUpdate(renderThreadTime));

			if (PauseRenderPrepare == false)
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
				var camera = scene.MainCamera;
				var dataToRender = scene.DataToRender;

				//if (renderThreadTime.FpsPer1Sec > 30)
				renderManagerFront.SkyboxCubeMap = scene.skyBox;
				//else renderManagerFront.SkyboxCubeMap = null;
				renderManagerFront.RenderAll(ubo, dataToRender.Lights, camera.postProcessEffects);
			}

			if (ShouldContinueRunning == false) return;

			gameWindow.SwapBuffers();

			EventSystem.Raise(new MyEngine.Events.PostRenderUpdate(renderThreadTime));


			//GC.Collect();
			Mesh.ProcessFinalizerQueue();

			EventSystem.Raise(new MyEngine.Events.FrameEnded(renderThreadTime));

			if (Debug.GetCVar("rendering / fps / automatically adjust target fps", false))
			{
				if (renderThreadTime.FrameCounter > TargetFps.Number * 10 && renderThreadTime.FpsPer10Sec < TargetFps)
				{
					TargetFps.Number = (float)renderThreadTime.FpsPer1Sec;
					Debug.AddValue("rendering / fps / adjusted target fps", TargetFps.Number);
				}
			}

			if (Debug.GetCVar("rendering / fps / fps throttling enabled", false))
			{
				var targetFps = TargetFps * 1.2;
				var secondsWeCanSleep = 1 / targetFps - renderThreadTime.CurrentFrameElapsedSeconds;
				if (secondsWeCanSleep > 0)
				{
					Debug.AddValue("rendering /fps / theoretical unthrottled fps", renderThreadTime.CurrentFrameElapsedTimeFps + " fps");
					Thread.Sleep((1000 * secondsWeCanSleep).FloorToInt());
				}
			}
		}


		void UpdateGPUMemoryInfo()
		{
			// http://developer.download.nvidia.com/opengl/specs/GL_NVX_gpu_memory_info.txt

			//var GPU_MEMORY_INFO_DEDICATED_VIDMEM_NVX = 0x9047;
			var GPU_MEMORY_INFO_TOTAL_AVAILABLE_MEMORY_NVX = 0x9048;
			var GPU_MEMORY_INFO_CURRENT_AVAILABLE_VIDMEM_NVX = 0x9049;
			//var GPU_MEMORY_INFO_EVICTION_COUNT_NVX = 0x904A;
			//var GPU_MEMORY_INFO_EVICTED_MEMORY_NVX = 0x904;

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