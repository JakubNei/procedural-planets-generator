using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public class JobTemplate<TData>
	{
		public delegate void SplittableAction(TData data, int splitIntoPartsCount, int partIndex);

		class JobTask
		{
			public Action<TData> normalAction;
			public SplittableAction splittableAction; // splitCount, splitIndex
			public WhereToRun whereToRun;

			public TimeSpan timeTaken;

			public bool firstRunDone;
			public ulong timesExecuted;
			public double avergeSeconds
			{
				get
				{
					if (firstRunDone && timesExecuted > 0)
						return timeTaken.TotalSeconds / timesExecuted;
					return 0;
				}
			}
		}
		List<JobTask> tasksToRun = new List<JobTask>();

		public string Name { get; set; }
		public void AddSplittableTask(SplittableAction splittable) => AddSplittableTask(WhereToRun.DoesNotMatter, splittable);
		public void AddSplittableTask(WhereToRun whereToRun, SplittableAction splittable)
		{
			tasksToRun.Add(new JobTask()
			{
				normalAction = (data) => splittable(data, 1, 0),
				whereToRun = whereToRun,
			});
		}
		public void AddTask(Action<TData> action) => AddTask(WhereToRun.DoesNotMatter, action);
		public void AddTask(WhereToRun whereToRun, Action<TData> action)
		{
			tasksToRun.Add(new JobTask() { normalAction = action, whereToRun = whereToRun });
		}
		public IJob MakeInstanceWithData(TData data)
		{
			return new JobInstance(this, data);
		}
		class JobInstance : IJob
		{
			public bool WantsToBeExecutedNow => currentTaskIndex < parent.tasksToRun.Count && lastTask == null;

			public bool WillNeverWantToBeExecuted => currentTaskIndex >= parent.tasksToRun.Count;

			public bool IsStarted => currentTaskIndex > 0;
			public bool IsFaulted { get; private set; }
			public Exception Exception { get; private set; }

			Task lastTask;
			int currentTaskIndex;

			readonly TData data;
			readonly JobTemplate<TData> parent;

			public JobInstance(JobTemplate<TData> parent, TData data)
			{
				this.parent = parent;
				this.data = data;
			}

			public bool GPUThreadExecute()
			{
				if (WantsToBeExecutedNow == false) return false;

				var jobTask = parent.tasksToRun[currentTaskIndex];
				currentTaskIndex++;

				if (jobTask.normalAction != null)
				{
					Action action = () =>
					{
						var stopWatch = Stopwatch.StartNew();
						try
						{
							jobTask.normalAction(data);
						}
						catch (Exception e)
						{
							IsFaulted = true;
							Exception = e;
						}
						if (jobTask.firstRunDone)
						{
							jobTask.timeTaken += stopWatch.Elapsed;
							jobTask.timesExecuted++;
						}
						else
						{
							jobTask.firstRunDone = true;
						}
						lastTask = null;
					};
					if (jobTask.whereToRun == WhereToRun.GPUThread)
						action();
					else
						lastTask = Task.Run(action);
				}
				else
				{
					throw new Exception("makes no sense");
				}

				return true;
			}
			public double NextGPUThreadTickWillTakeSeconds()
			{
				if (WantsToBeExecutedNow == false) return 0;
				var jobTask = parent.tasksToRun[currentTaskIndex];
				if (jobTask.whereToRun == WhereToRun.GPUThread) return jobTask.avergeSeconds;
				return 0;
			}
		}

	}


}
