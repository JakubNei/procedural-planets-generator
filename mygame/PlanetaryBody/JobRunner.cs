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



		public void GPUThreadTick(Func<double> secondLeftToUse, Func<IJob> jobFactory)
		{
			var maxBudget = secondLeftToUse();

			//Debug.AddValue("generation / generation jobs running", jobs.Count);
			int totalJobsRan = 0;

			while (
				secondLeftToUse() > 0 ||
				maxBudget < 0 // on shit computers, we still want to generate, albeit at big performnance cost
			)
			{
				jobs.RemoveAll(j => j.WillNeverWantToBeExecuted);

				if (jobs.Count == 0)
				{
					var j = jobFactory();
					if (j == null) return;
					jobs.Add(j);
				}

				int jobsRanThisLoop = 0;

				foreach (var job in jobs)
				{
					while (job.WantsToBeExecutedNow)
					{
						var secondsNeeded = job.NextGPUThreadExecuteWillTakeSeconds();
						if (
							(secondsNeeded < secondLeftToUse()) || // either if we have time
							(secondsNeeded > maxBudget * 0.9 && totalJobsRan == 0) // or if this is first job and it is too big
						)
						{
							if (job.GPUThreadExecute())
							{
								if (maxBudget < 0) return; // if we are on shit computer, generate just one then end, lets generate at least something
								totalJobsRan++;
								jobsRanThisLoop++;
							}
						}
						else
						{
							if (secondsNeeded > maxBudget)
							{
								if (job.NextTask.IsSplittable)
								{
									var partsToSplitTo = (maxBudget / secondsNeeded).CeilToInt();
									job.NextTask.TrySplitToParts((ushort)partsToSplitTo);

									//Log.Info(
									//	"task '" + job.NextTask.Name + "' exceeds budget limit " + Neitri.FormatUtils.SecondsToString(maxBudget) + " " +
									//	"by " + Neitri.FormatUtils.SecondsToString(secondsNeeded - maxBudget) + ", " +
									//	"splitting to " + partsToSplitTo + " parts"
									//);
								}
								else
								{
									//Log.Warn(
									//	"task '" + job.NextTask.Name + "' exceeds budget limit " + Neitri.FormatUtils.SecondsToString(maxBudget) + " " +
									//	"by " + Neitri.FormatUtils.SecondsToString(secondsNeeded - maxBudget) + ", " +
									//	"will execute in and slow down next frame"
									//);
								}
							}
							break;
						}
					}
				}

				if (jobsRanThisLoop == 0) break;
			}
		}
	}

}
