using MyEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{

	public enum WhereToRun
	{
		GPUThread,
		DoesNotMatter,
	}

	public class JobRunner : SingletonsPropertyAccesor
	{
		List<IJob> jobs = new List<IJob>();
		Dictionary<IJob, double> timesOutOfTime = new Dictionary<IJob, double>();

		public int JobsCount => jobs.Count;

		public void AddJob(IJob job)
		{
			jobs.Add(job);
		}

		public void GPUThreadTick(Func<double> secondLeftToUse)
		{
			var maxBudget = secondLeftToUse();

			Debug.AddValue("generation / generation jobs running", jobs.Count);

			while (jobs.Count > 0 && secondLeftToUse() > 0)
			{
				int jobsRan = 0;

				jobs.RemoveAll(j => j.WillNeverWantToBeExecuted);

				foreach (var job in jobs)
				{
					while (job.WantsToBeExecutedNow)
					{
						if (job.NextGPUThreadTickWillTakeSeconds() < secondLeftToUse())
						{
							if (job.GPUThreadExecute())
								jobsRan++;
						}
						else
						{
							var s = job.NextGPUThreadTickWillTakeSeconds();
							if (s > maxBudget)
							{
								// split next job
								Log.Warn("generation job exceeded budget limit by " + (s - maxBudget) + " seconds");
							}
							break;
						}
					}
				}

				jobs.RemoveAll(j => j.WillNeverWantToBeExecuted);

				if (jobsRan == 0) break;
			}
		}
	}

}
