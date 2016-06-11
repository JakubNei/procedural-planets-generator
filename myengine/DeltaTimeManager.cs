using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{

	public class DeltaTimeManager
	{
		Queue<DateTime> frameTimes1sec = new Queue<DateTime>();
		Queue<DateTime> frameTimes10sec = new Queue<DateTime>();
		System.Diagnostics.Stopwatch eventThreadTime = new System.Diagnostics.Stopwatch();

		public double DeltaTimeNow { get; private set; }
		public double DeltaTime1Second { get; private set; }
		public double DeltaTime10Seconds { get; private set; }

		public void Tick()
		{
			var now = DateTime.Now;			

			frameTimes1sec.Enqueue(now);
			while ((now - frameTimes1sec.Peek()).TotalMilliseconds > 1000) frameTimes1sec.Dequeue();
			DeltaTime1Second = 1.0 / (double)frameTimes1sec.Count;


			frameTimes10sec.Enqueue(now);
			while ((now - frameTimes10sec.Peek()).TotalMilliseconds > 10000) frameTimes10sec.Dequeue();
			DeltaTime10Seconds = 10.0 / (double)frameTimes10sec.Count;

			DeltaTimeNow = eventThreadTime.ElapsedMilliseconds / 1000.0;
			eventThreadTime.Restart();
		}
	}
}
