using Neitri;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MyEngine
{
	public class MyDebug
	{
		public static MyDebug Instance { get; private set; }

		List<string> alreadyShown = new List<string>();

		public readonly InputSystem Input;



		private ConcurrentDictionary<string, string> stringValues = new ConcurrentDictionary<string, string>();

		public CommonCVars CommonCVars { get; private set; }

		class TraceListener : System.Diagnostics.TraceListener
		{
			MyDebug debug;
			public TraceListener(MyDebug debug)
			{
				this.debug = debug;
			}
			public override void Write(string message)
			{
				debug.Info(message);
			}

			public override void WriteLine(string message)
			{
				debug.Info(message);
			}
		}

		public readonly ILog Log;

		public MyDebug(InputSystem input, FileSystem fs)
		{
			Log = new Neitri.Logging.LogConsole();
			this.Input = input;
			Instance = this;

			cvars = new CVarFactory(() => File.Open(fs.GetPhysicalPath("cvars.config"), FileMode.OpenOrCreate), Log);
			CommonCVars = new CommonCVars(this);
			AddCommonCvars();

			System.Diagnostics.Debug.Listeners.Add(new TraceListener(this));
		}


		public void AddValue(string key, object value)
		{
			stringValues[key] = value.ToString();
		}


		Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();

		CVarFactory cvars;
		public CVar GetCVar(string name, bool defaultValue = false) => cvars.GetCVar(name, defaultValue);

		public CVar GetCVar(string name) => cvars.GetCVar(name);


		public TickStats Tick(string name)
		{
			TickStats t;
			if (nameToTickStat.TryGetValue(name, out t) == false)
			{
				t = new TickStats();
				t.name = name;
				nameToTickStat[name] = t;
			}
			t.Update(this);
			return t;
		}


		DebugForm debugForm;

		void AddCommonCvars()
		{
			CommonCVars.ShowDebugForm().ToogledByKey(OpenTK.Input.Key.F1).OnChanged += (cvar) =>
			{
				if (debugForm == null) debugForm = new DebugForm();
				if (cvar.Bool) debugForm.Show();
				else debugForm.Hide();
			};
			CommonCVars.PauseRenderPrepare().ToogledByKey(OpenTK.Input.Key.F4);
			CommonCVars.ReloadAllShaders().ToogledByKey(OpenTK.Input.Key.F5);
			CommonCVars.ShadowsDisabled().ToogledByKey(OpenTK.Input.Key.F6);
			CommonCVars.DrawDebugBounds().ToogledByKey(OpenTK.Input.Key.F7);
			CommonCVars.EnablePostProcessEffects().ToogledByKey(OpenTK.Input.Key.F8);
			CommonCVars.DebugDrawGBufferContents().ToogledByKey(OpenTK.Input.Key.F9);
			CommonCVars.DebugDrawNormalBufferContents().ToogledByKey(OpenTK.Input.Key.F10);
			CommonCVars.DebugRenderWithLines().ToogledByKey(OpenTK.Input.Key.F11);

		}


		public void InputUpdate()
		{
			foreach (var cvar in cvars.NameToCvar.Values)
			{
				if (cvar.toogleKey != OpenTK.Input.Key.Unknown)
				{
					if (Input.GetKeyDown(cvar.toogleKey))
					{
						cvar.Bool = !cvar.Bool;
					}
				}
			}
		}

		public void LogicUpdate()
		{
			if (debugForm?.Visible == true)
				debugForm.UpdateBy(stringValues, cvars.NameToCvar);
		}


		public void Info(object obj)
		{
			Log.Info(obj);
		}

		public void Warning(object obj)
		{
			Log.Warn(obj);
		}

		public void Error(object obj)
		{
			Log.Error(obj);
		}

		public void Pause()
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine("Press any key to continue ...");
			Console.ReadKey();
		}
	}
}