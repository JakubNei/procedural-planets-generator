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

	public class JobRunner
	{
		List<IJob> jobs = new List<IJob>();
		Dictionary<IJob, double> timesOutOfTime = new Dictionary<IJob, double>();
		public void AddJob(IJob job)
		{
			lock (jobs)
				jobs.Add(job);
		}
		public void RemoveNotStarted()
		{
			lock (jobs)
				jobs.RemoveAll(j => j.IsStarted == false);
		}
		public void GPUThreadTick(FrameTime ft, Func<double> secondLeftToUse)
		{			
			while (jobs.Count > 0 && secondLeftToUse() > 0)
			{
				int jobsRan = 0;

				IJob[] orderedJobs;
				lock (jobs)
				{
					jobs.RemoveAll(j => j.WantsToBeExecuted == false);
					orderedJobs = jobs.OrderByDescending(j => j.NextGPUThreadTickWillTakeSeconds()).ToArray();
				}

				foreach (var job in orderedJobs)
				{
					while (job.WantsToBeExecuted && job.NextGPUThreadTickWillTakeSeconds() < secondLeftToUse() && job.GPUThreadTick())
						jobsRan++;
				}

				lock (jobs)
					jobs.RemoveAll(j => j.WantsToBeExecuted == false);

				if (jobsRan == 0) break;
			}

			Singletons.Debug.AddValue("jobs count", jobs.Count);
		}
	}

}
