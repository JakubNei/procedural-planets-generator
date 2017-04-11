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
		public delegate void SplittableAction(TData data, ushort splitIntoPartsCount, ushort partIndex);

		class TaskTemplate
		{
			public Action<TData> normalAction;
			public SplittableAction splittableAction; // splitCount, splitIndex
			public WhereToRun whereToRun;

			public TimeSpan timeTaken;

			public StackTrace creationStackTrace;

			public ulong timesExecutedBeforeMeasurement;

			public bool CanMeasure => timesExecutedBeforeMeasurement > 3;
			public ulong timesExecuted;
			public double AvergeSeconds
			{
				get
				{
					if (CanMeasure && timesExecuted > 0)
						return timeTaken.TotalSeconds / timesExecuted;
					return 0;
				}
			}

			public string name;
		}
		List<TaskTemplate> tasksToRun = new List<TaskTemplate>();

		public double SecondsTaken => tasksToRun.Sum(t => t.timeTaken.TotalSeconds);
		public double AverageSeconds => tasksToRun.Sum(t => t.AvergeSeconds);


		public string Name { get; set; }
		public void AddSplittableTask(SplittableAction splittable, string name = null) => AddSplittableTask(WhereToRun.DoesNotMatter, splittable, name);
		public void AddSplittableTask(WhereToRun whereToRun, SplittableAction splittable, string name = null)
		{
			tasksToRun.Add(new TaskTemplate()
			{
				name = name,
				splittableAction = splittable,
				whereToRun = whereToRun,
				creationStackTrace = new StackTrace(1, true),
			});
		}
		public void AddTask(Action<TData> action, string name = null) => AddTask(WhereToRun.DoesNotMatter, action, name);
		public void AddTask(WhereToRun whereToRun, Action<TData> action, string name = null)
		{
			tasksToRun.Add(new TaskTemplate()
			{
				name = name,
				normalAction = action,
				whereToRun = whereToRun,
				creationStackTrace = new StackTrace(1, true),
			});
		}
		public IJob MakeInstanceWithData(TData data)
		{
			return new JobInstance(this, data);
		}

		public string StatisticsReport()
		{
			var t = tasksToRun.Sum(j => j.AvergeSeconds);
			return tasksToRun.Select(j => j.name + " = " + j.AvergeSeconds / t * 100).Join(Environment.NewLine);
			//return tasksToRun.Select(j => j.name + " = " + Neitri.FormatUtils.SecondsToString(j.AvergeSeconds)).Join(Environment.NewLine);
		}

		class JobInstance : IJob
		{
			public bool WantsToBeExecutedNow => tasksToRun.Count > 0 && (lastSystemTask == null || lastSystemTask.IsCompleted);

			public bool WillNeverWantToBeExecuted => tasksToRun.Count == 0;


			public bool IsFaulted { get; private set; }
			public Exception Exception { get; private set; }


			List<TaskInstance> tasksToRun = new List<TaskInstance>();

			public ITask NextTask => nextTask;

			volatile Task lastSystemTask;

			TaskInstance nextTask => tasksToRun.First();

			readonly TData data;
			readonly JobTemplate<TData> parent;

			class TaskInstance : ITask
			{
				public TaskTemplate taskTemplate;
				public JobInstance parent;

				public Action<TData> action;

				public bool IsSplittable => taskTemplate.splittableAction != null;

				public string Name => taskTemplate.name;

				public TaskInstance(JobInstance parent, TaskTemplate taskTemplate, ushort splitCount, ushort splitIndex)
				{
					this.parent = parent;
					this.taskTemplate = taskTemplate;

					if (this.taskTemplate.splittableAction == null)
						throw new Exception("this should not happen, trying to make splitted task instance on non splittable task template");
					else
						action = (data) => this.taskTemplate.splittableAction(data, splitCount, splitIndex);
				}

				public TaskInstance(JobInstance parent, TaskTemplate taskTemplate)
				{
					this.parent = parent;
					this.taskTemplate = taskTemplate;

					if (this.taskTemplate.splittableAction == null)
						action = this.taskTemplate.normalAction;
					else
						action = (data) => this.taskTemplate.splittableAction(data, 1, 0);
				}

				public bool TrySplitToParts(ushort partsCount)
				{
					var myIndex = parent.tasksToRun.IndexOf(this);
					parent.tasksToRun.RemoveAll((t) => t.taskTemplate == this.taskTemplate);

					for (ushort i = 0; i < partsCount; i++)
					{
						parent.tasksToRun.Insert(myIndex + i, new TaskInstance(parent, taskTemplate, i, partsCount));
					}

					return true;
				}
			}

			public JobInstance(JobTemplate<TData> parent, TData data)
			{
				this.parent = parent;
				this.data = data;
				foreach (var taskTemplate in parent.tasksToRun)
					this.tasksToRun.Add(new TaskInstance(this, taskTemplate));

			}



			public bool GPUThreadExecute()
			{
				if (WantsToBeExecutedNow == false) return false;


				var task = nextTask;
				tasksToRun.RemoveAt(0);
				Action<TData> currentAction = task.action;

				Action action = () =>
				{
					var stopWatch = Stopwatch.StartNew();
					try
					{
						currentAction(data);
					}
					catch (Exception e)
					{
						IsFaulted = true;
						Exception = e;
					}
					if (task.taskTemplate.CanMeasure)
					{
						task.taskTemplate.timeTaken += stopWatch.Elapsed;
						task.taskTemplate.timesExecuted++;
					}
					else
					{
						task.taskTemplate.timesExecutedBeforeMeasurement++;
					}
				};
				if (task.taskTemplate.whereToRun == WhereToRun.GPUThread)
					action();
				else
					lastSystemTask = Task.Run(action);


				return true;
			}
			public double NextGPUThreadExecuteWillTakeSeconds()
			{
				if (WantsToBeExecutedNow == false) return 0;
				if (nextTask.taskTemplate.whereToRun == WhereToRun.GPUThread) return nextTask.taskTemplate.AvergeSeconds;
				return 0;
			}
		}

	}


}
