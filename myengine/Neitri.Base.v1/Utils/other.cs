using Neitri.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Neitri
{

	public class Profiler : IDisposable
	{
		string name;
		Stopwatch time;
		ILogging log;

		public Profiler(string name, ILogging log)
		{
			this.name = name;
			this.time = new Stopwatch();
			this.time.Start();
			this.log = log;
		}
		public void Dispose()
		{
			this.time.Stop();
			log.Trace("'" + name + "' took " + time.ElapsedMilliseconds + " ms");
		}
	}

}
