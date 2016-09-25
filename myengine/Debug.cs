using Neitri;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MyEngine
{
	public class Debug
	{
		List<string> alreadyShown = new List<string>();

		[Dependency]
		public InputSystem Input { get; private set; }

		[Dependency]
		IDependencyManager dependency;

		public ConcurrentDictionary<string, string> stringValues = new ConcurrentDictionary<string, string>();

		public void AddValue(string key, object value)
		{
			stringValues[key] = value.ToString();
		}

		class TickStats
		{
			public string name;

			public float FpsPer1Sec
			{
				get
				{
					return frameTimes1sec.Count;
				}
			}

			public float FpsPer10Sec
			{
				get
				{
					return frameTimes10sec.Count / 10.0f;
				}
			}

			Queue<DateTime> frameTimes1sec = new Queue<DateTime>();
			Queue<DateTime> frameTimes10sec = new Queue<DateTime>();

			//DateTime lastNow;

			public void Update(Debug debug)
			{
				var now = DateTime.Now;

				//var nowFps = 1.0 / (now - lastNow).TotalSeconds;

				frameTimes1sec.Enqueue(now);
				frameTimes10sec.Enqueue(now);

				while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();
				while ((now - frameTimes10sec.Peek()).TotalSeconds > 10) frameTimes10sec.Dequeue();

				debug.AddValue(name, $"(FPS 1s:{FpsPer1Sec.ToString("0.")} 10s:{FpsPer10Sec.ToString("0.")})");
				//Debug.AddValue(name, $"(FPS now:{nowFps.ToString("0.")} 1s:{FpsPer1Sec.ToString("0.")} 10s:{FpsPer10Sec.ToString("0.")})");

				//lastNow = now;
			}
		}

		Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();

		Dictionary<string, ConVar> nameToCVar = new Dictionary<string, ConVar>();

		public class ConVar
		{
			//public dynamic Value { get; set; }
			public string name;

			bool _bool;

			Debug debug;

			public bool Bool
			{
				get
				{
					return _bool;
				}
				set
				{
					if (_bool != value)
					{
						_bool = value;
						OnChanged?.Invoke(this);
					}
				}
			}

			public OpenTK.Input.Key toogleKey = OpenTK.Input.Key.Unknown;
			public bool hasDefaultValue = false;

			public event Action<ConVar> OnChanged;

			public ConVar(Debug debug)
			{
				this.debug = debug;
			}

			public ConVar ToogledByKey(OpenTK.Input.Key key)
			{
				debug.Info($"{key} to toggled {name}");
				toogleKey = key;
				return this;
			}
		}

		public ConVar CVar(string name, bool defaultValue = false)
		{
			ConVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new ConVar(this);
				result.name = name;
				nameToCVar[name] = result;
			}
			if (result.hasDefaultValue == false)
			{
				result.Bool = defaultValue;
				result.hasDefaultValue = true;
			}
			return result;
		}

		public ConVar CVar(string name)
		{
			ConVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new ConVar(this);
				result.name = name;
				nameToCVar[name] = result;
			}
			return result;
		}

		public void Tick(string name)
		{
			TickStats t;
			if (nameToTickStat.TryGetValue(name, out t) == false)
			{
				t = new TickStats();
				t.name = name;
				nameToTickStat[name] = t;
			}
			t.Update(this);
		}

		public Debug()
		{
			AddCommonCvars();
		}

		void AddCommonCvars()
		{
			CVar("autoBuildRenderList").ToogledByKey(OpenTK.Input.Key.F4);
			CVar("reloadAllShaders").ToogledByKey(OpenTK.Input.Key.F5);
			CVar("shadowsDisabled").ToogledByKey(OpenTK.Input.Key.F6);
			CVar("drawDebugBounds").ToogledByKey(OpenTK.Input.Key.F7);
			CVar("enablePostProcessEffects").ToogledByKey(OpenTK.Input.Key.F8);
			CVar("debugDrawGBufferContents").ToogledByKey(OpenTK.Input.Key.F9);
			CVar("debugDrawNormalBufferContents").ToogledByKey(OpenTK.Input.Key.F10);
			CVar("debugRenderWithLines").ToogledByKey(OpenTK.Input.Key.F11);
			CVar("sortRenderers").ToogledByKey(OpenTK.Input.Key.F12);
		}

		public void Update()
		{
			foreach (var cvar in nameToCVar.Values)
			{
				if (cvar.toogleKey != OpenTK.Input.Key.Unknown)
				{
					if (Input.GetKeyDown(cvar.toogleKey))
					{
						cvar.Bool = !cvar.Bool;
						Info($"{cvar.name} toogled to {cvar.Bool}");
					}
				}
			}
		}

		void Log(object obj, bool canRepeat)
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