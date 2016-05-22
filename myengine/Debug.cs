using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;

namespace MyEngine
{
    public static class Debug
    {
        static List<string> alreadyShown = new List<string>();

        
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

            public void Update()
            {
                var now = DateTime.Now;

                frameTimes1sec.Enqueue(now);
                frameTimes10sec.Enqueue(now);

                while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();
                while ((now - frameTimes10sec.Peek()).TotalSeconds > 10) frameTimes10sec.Dequeue();

                Debug.AddValue(name, "(FPS 1s:" + FpsPer1Sec.ToString("0.") + " 10s:" + FpsPer10Sec.ToString("0.") + ")");
            }
        }

        static Dictionary<string, TickStats> nameToTickStat = new Dictionary<string, TickStats>();


        static Dictionary<string, DebugValue> nameToDebugValue = new Dictionary<string, DebugValue>();

        public class DebugValue
        {
            //public dynamic Value { get; set; }
            public bool Bool { get; set; }
        }

        public static DebugValue Value(string name)
        {
            return nameToDebugValue.GetOrAdd(name);
        }

        public static void Tick(string name)
        {
            TickStats t;
            if(nameToTickStat.TryGetValue(name, out t) == false)
            {
                t = new TickStats();
                t.name = name;
                nameToTickStat[name] = t;
            }
            t.Update();
        }
        

        static void Log(object obj, bool canRepeat)
        {
            var s = obj.ToString();
            if (canRepeat || !alreadyShown.Contains(s))
            {
                var t=new StackTrace(2);
                var f = t.GetFrame(0);
                var m = f.GetMethod();
                
                if(!canRepeat) alreadyShown.Add(s);
                Console.WriteLine("["+m.DeclaringType.Name+"."+m.Name+"] "+s);
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
