using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{

	public class TickStats
	{
		public string name;

		public float FpsPer1Sec { get; private set; }
		public float FpsPer10Sec { get; private set; }

		Queue<DateTime> frameTimes1sec = new Queue<DateTime>();
		Queue<DateTime> frameTimes10sec = new Queue<DateTime>();

		public void Update(MyDebug debug)
		{
			var now = DateTime.Now;
			frameTimes1sec.Enqueue(now);
			frameTimes10sec.Enqueue(now);

			while ((now - frameTimes1sec.Peek()).TotalSeconds > 1) frameTimes1sec.Dequeue();
			while ((now - frameTimes10sec.Peek()).TotalSeconds > 10) frameTimes10sec.Dequeue();

			if ((now - frameTimes1sec.Peek()).TotalSeconds < 0.9) FpsPer1Sec = 60;
			else FpsPer1Sec = frameTimes1sec.Count;

			if ((now - frameTimes10sec.Peek()).TotalSeconds < 9) FpsPer10Sec = 60;
			else FpsPer10Sec = frameTimes10sec.Count / 10.0f;

			debug.AddValue(name, $"average FPS over 1 second {FpsPer1Sec.ToString("0.")}, average FPS over 10 seconds {FpsPer10Sec.ToString("0.")}");
		}
	}

}
