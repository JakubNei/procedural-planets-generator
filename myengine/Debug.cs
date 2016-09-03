using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace MyEngine
{
	public class Debug
	{
		static List<string> alreadyShown = new List<string>();

		public static Debug instance;


		public InputSystem Input { get; private set; }

		public Debug(InputSystem input)
		{
			this.Input = input;
		}

		public static ConcurrentDictionary<string, string> stringValues = new ConcurrentDictionary<string, string>();

		public static void AddValue(string key, string value)
		{
			stringValues[key] = value;
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

			public void Update()
			{
				var now = DateTime.Now;

				//var nowFps = 1.0 / (now - lastNow).TotalSeconds;

				frameTimes1sec.Enqueue(now);
				frameTimes10sec.Enqueue(now);

				while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();
				while ((now - frameTimes10sec.Peek()).TotalSeconds > 10) frameTimes10sec.Dequeue();

				Debug.AddValue(name, $"(FPS 1s:{FpsPer1Sec.ToString("0.")} 10s:{FpsPer10Sec.ToString("0.")})");
				//Debug.AddValue(name, $"(FPS now:{nowFps.ToString("0.")} 1s:{FpsPer1Sec.ToString("0.")} 10s:{FpsPer10Sec.ToString("0.")})");

				//lastNow = now;
			}
		}

		static Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();

		static Dictionary<string, ConVar> nameToCVar = new Dictionary<string, ConVar>();

		public class ConVar
		{
			//public dynamic Value { get; set; }
			public string name;
			public bool Bool { get; set; }
			public OpenTK.Input.Key toogleKey = OpenTK.Input.Key.Unknown;
			public bool hasDefaultValue = false;
			public ConVar ToogledByKey(OpenTK.Input.Key key)
			{
				Debug.Info($"{key} to toggled {name}");
				toogleKey = key;
				return this;
			}
		}

		public static ConVar CVar(string name, bool defaultValue = false)
		{
			ConVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new ConVar();
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
		public static ConVar CVar(string name)
		{
			ConVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new ConVar();
				result.name = name;
				nameToCVar[name] = result;
			}
			return result;
		}

		public static void Tick(string name)
		{
			TickStats t;
			if (nameToTickStat.TryGetValue(name, out t) == false)
			{
				t = new TickStats();
				t.name = name;
				nameToTickStat[name] = t;
			}
			t.Update();
		}

		public void Start()
		{
			Debug.CVar("autoBuildRenderList").ToogledByKey(OpenTK.Input.Key.F4);
			Debug.CVar("reloadAllShaders").ToogledByKey(OpenTK.Input.Key.F5);
			Debug.CVar("shadowsDisabled").ToogledByKey(OpenTK.Input.Key.F6);
			Debug.CVar("drawDebugBounds").ToogledByKey(OpenTK.Input.Key.F7);
			Debug.CVar("enablePostProcessEffects").ToogledByKey(OpenTK.Input.Key.F8);
			Debug.CVar("debugDrawGBufferContents").ToogledByKey(OpenTK.Input.Key.F9);
			Debug.CVar("debugDrawNormalBufferContents").ToogledByKey(OpenTK.Input.Key.F10);
			Debug.CVar("debugRenderWithLines").ToogledByKey(OpenTK.Input.Key.F11);
			Debug.CVar("sortRenderers").ToogledByKey(OpenTK.Input.Key.F12);
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
						Debug.Info($"{cvar.name} toogled to {cvar.Bool}");
					}
				}
			}
		}

		static void Log(object obj, bool canRepeat)
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

		public static void Info(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
		}

		public static void Warning(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
		}
		public static void Error(object obj, bool canRepeat = true, bool pause = false)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Console.BackgroundColor = ConsoleColor.Black;
			Log(obj, canRepeat);
			if (pause) Pause();
		}
		public static void Pause()
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Console.BackgroundColor = ConsoleColor.Black;
			Console.WriteLine("Press any key to continue ...");
			Console.ReadKey();
		}

	}
}
