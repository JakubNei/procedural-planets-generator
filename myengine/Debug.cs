using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace MyEngine
{
    public static class Debug
    {
        static List<string> alreadyShown = new List<string>();

        public static Dictionary<string, string> stringValues = new Dictionary<string, string>();

        public static void AddValue(string key, string value)
        {
            stringValues[key] = value;
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
