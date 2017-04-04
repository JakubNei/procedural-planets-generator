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

			while (secondLeftToUse() > 0)
			{
				jobs.RemoveAll(j => j.WillNeverWantToBeExecuted);

				if (jobs.Count == 0)
				{
					var j = jobFactory();
					if (j == null) return;
					jobs.Add(j);
				}

				int jobsRan = 0;

				foreach (var job in jobs)
				{
					while (job.WantsToBeExecutedNow)
					{
						var secondsNeeded = job.NextGPUThreadExecuteWillTakeSeconds();
						if (secondsNeeded < secondLeftToUse())
						{
							if (job.GPUThreadExecute())
								jobsRan++;
						}
						else
						{
							if (secondsNeeded > maxBudget)
							{
								if (job.NextTask.IsSplittable)
								{
									var partsToSplitTo = (maxBudget / secondsNeeded).CeilToInt();
									job.NextTask.TrySplitToParts((ushort)partsToSplitTo);

									Log.Warn(
										"generation task exceeds budget limit " + Neitri.FormatUtils.SecondsToString(maxBudget) + " " +
										"by " + Neitri.FormatUtils.SecondsToString(secondsNeeded - maxBudget) + ", " +
										"splitting to " + partsToSplitTo + " parts: '" + job.NextTask.Name + "'"
									);
								}
								else
								{
									Log.Error(
										"generation task exceeds budget limit " + Neitri.FormatUtils.SecondsToString(maxBudget) + " " +
										"by " + Neitri.FormatUtils.SecondsToString(secondsNeeded - maxBudget) + ", " +
										"unable to split: '" + job.NextTask.Name + "'"
									);
								}
							}
							break;
						}
					}
				}

				if (jobsRan == 0) break;
			}
		}
	}

}
