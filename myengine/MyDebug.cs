using Neitri;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
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

		public MyDebug(InputSystem input)
		{
			this.Input = input;
            Instance = this;
            CommonCVars = new CommonCVars(this);
			AddCommonCvars();

			System.Diagnostics.Debug.Listeners.Add(new TraceListener(this));
		}


		public void AddValue(string key, object value)
		{
			stringValues[key] = value.ToString();
		}


		Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();

		Dictionary<string, CVar> nameToCVar = new Dictionary<string, CVar>();

		public CVar GetCVar(string name, bool defaultValue = false)
		{
			CVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new CVar(this);
				result.Bool = defaultValue;
				result.name = name;
				nameToCVar[name] = result;
			}
			return result;
		}

		public CVar GetCVar(string name)
		{
			CVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new CVar(this);
				result.name = name;
				nameToCVar[name] = result;
			}
			return result;
		}

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
			foreach (var cvar in nameToCVar.Values)
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
				debugForm.UpdateBy(stringValues, nameToCVar);
		}


		private void Log(object obj, bool canRepeat)
		{
			var s = obj.ToString();
			if (canRepeat || !alreadyShown.Contains(s))
			{
				var t = new StackTrace(2);
				var f = t.GetFrame(0);
				var m = f.GetMethod();

				if (!canRepeat) alreadyShown.Add(s);
				Console.WriteLine("[" + m.DeclaringType.Name + "." + m.Name + "] " + s);
			}
		}

		public void Info(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
		}

		public void Warning(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
		}

		public void Error(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
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