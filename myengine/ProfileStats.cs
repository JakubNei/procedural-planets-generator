using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine
{
	public class ProfileStats
	{
		ulong totalCount;
		TimeSpan timeTaken;
		public double AvergeSeconds
		{
			get
			{
				if (totalCount > 0)
					return timeTaken.TotalSeconds / totalCount;
				return 0;
			}
		}
		string name;

		public ProfileStats(string name = null)
		{
			this.name = name;
		}

		public struct Measurement : IDisposable
		{
			readonly ProfileStats stats;
			Stopwatch sw;
			public Measurement(ProfileStats stats)
			{
				this.stats = stats;
				sw = Stopwatch.StartNew();
			}

			public void Dispose()
			{
				End();
			}

			public void End()
			{
				stats.totalCount++;
				stats.timeTaken += sw.Elapsed;
			}
			public void EndAndLog()
			{
				End();
				this.stats.ShowStatsTime();
			}
		}

		public Measurement Start()
		{
			return new Measurement(this);
		}
		public void ShowStatsTime()
		{
			Singletons.Debug.AddValue(name, Neitri.FormatUtils.SecondsToString(AvergeSeconds));
		}

	}
}
