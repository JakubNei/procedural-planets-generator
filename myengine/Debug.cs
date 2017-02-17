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
	public class Debug
	{
        public static Debug Instance { get; private set; }

		List<string> alreadyShown = new List<string>();

		[Dependency]
		public InputSystem Input { get; private set; }

		[Dependency]
		IDependencyManager dependency;


		private ConcurrentDictionary<string, string> stringValues = new ConcurrentDictionary<string, string>();

		public CommonCVars CommonCVars { get; private set; }

		class TraceListener : System.Diagnostics.TraceListener
		{
			Debug debug;
			public TraceListener(Debug debug)
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

		public Debug()
		{
            Instance = this;
            CommonCVars = new CommonCVars(this);
			AddCommonCvars();

			System.Diagnostics.Debug.Listeners.Add(new TraceListener(this));
		}

		public class TickStats
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

				debug.AddValue(name, $"FPS:{FpsPer1Sec.ToString("0.")}, average FPS over 10s:{FpsPer10Sec.ToString("0.")}");
				//Debug.AddValue(name, $"(FPS now:{nowFps.ToString("0.")} 1s:{FpsPer1Sec.ToString("0.")} 10s:{FpsPer10Sec.ToString("0.")})");

				//lastNow = now;
			}
		}


		public void AddValue(string key, object value)
		{
			stringValues[key] = value.ToString();
		}


		Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();

		Dictionary<string, CVar> nameToCVar = new Dictionary<string, CVar>();

		public CVar CVar(string name, bool defaultValue = false)
		{
			CVar result;
			if (!nameToCVar.TryGetValue(name, out result))
			{
				result = new CVar(this);
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
		DictionaryWatcher<string, string> stringValuesWatcher;
		DictionaryWatcher<string, CVar, string> ckeyValuesWatcher;

		void AddCommonCvars()
		{
			CommonCVars.ShowDebugForm().ToogledByKey(OpenTK.Input.Key.F1).OnChanged += (cvar) =>
			{
				if (debugForm == null)
				{
					debugForm = new DebugForm();
					{
						var items = debugForm.listView1.Items;
						stringValuesWatcher = new DictionaryWatcher<string, string>();
						stringValuesWatcher.OnAdded += (key, value) => items.Add(new ListViewItem(new string[] { key, value }) { Tag = key });
						stringValuesWatcher.OnUpdated += (key, value) => items.OfType<ListViewItem>().First(i => (string)i.Tag == key).SubItems[1].Text = value;
						stringValuesWatcher.OnRemoved += (key) => items.Remove(items.OfType<ListViewItem>().First(i => (string)i.Tag == key));
					}
					{
						var items = debugForm.listView2.Items;
						ckeyValuesWatcher = new DictionaryWatcher<string, CVar, string>();
						ckeyValuesWatcher.comparisonValueSelector = (item) => item.Bool.ToString();
						ckeyValuesWatcher.OnAdded += (key, item) => items.Add(new ListViewItem(new string[] { item.toogleKey.ToString(), item.name, item.Bool.ToString() }) { Tag = key });
						ckeyValuesWatcher.OnUpdated += (key, item) =>
						{
							var subItems = items.OfType<ListViewItem>().First(i => (string)i.Tag == key).SubItems;
							subItems[0].Text = item.toogleKey.ToString();
							subItems[2].Text = item.Bool.ToString();
						};
						ckeyValuesWatcher.OnRemoved += (key) => items.Remove(items.OfType<ListViewItem>().First(i => (string)i.Tag == key));
					}
				}

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
			CommonCVars.SortRenderers().ToogledByKey(OpenTK.Input.Key.F12);

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
						Info($"{cvar.name} toogled to  {cvar.Bool}");
					}
				}
			}

			if (debugForm?.Visible == true)
			{
				stringValuesWatcher.UpdateBy(stringValues);
				ckeyValuesWatcher.UpdateBy(nameToCVar);
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