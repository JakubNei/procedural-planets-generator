using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyEngine
{
	public class ParallerRunner
	{
		Thread[] threads;
		ManualResetEventSlim[] workerThreadsRun;
		ManualResetEventSlim[] workerThreadFinished;
		public ParallerRunner(int threadCount = 0, string threadPrefixName = "", ThreadPriority threadPriority = ThreadPriority.Normal)
		{
			if (threadCount < 1) threadCount = Environment.ProcessorCount;
			if (threadCount < 1) threadCount = 1;

			threads = new Thread[threadCount];
			workerThreadFinished = new ManualResetEventSlim[threadCount];
			workerThreadsRun = new ManualResetEventSlim[threadCount];

			for (int i = 0; i < threads.Length; i++)
			{
				workerThreadsRun[i] = new ManualResetEventSlim(false);
				workerThreadFinished[i] = new ManualResetEventSlim(false);

				int workerThreadIndex = i;
				var t = new Thread(() =>
				{
					while (true)
					{
						workerThreadsRun[workerThreadIndex].Wait();
						ExecuteNext(workerThreadIndex);
					}
				});
				threads[i] = t;

				t.Name = threadPrefixName + " worker #" + i;
				t.IsBackground = true;
				t.Priority = threadPriority;
				t.Start();

			}
		}




		int fromInclusive;
		int toExclusive;
		Action<int> body;
		int currentIndex;

		SemaphoreSlim s;
		private void ExecuteNext(int workerThreadIndex)
		{
			var myCurrentIndex = Interlocked.Increment(ref currentIndex) - 1;
			if (myCurrentIndex >= toExclusive)
			{
				workerThreadFinished[workerThreadIndex].Set();
				return;
			}
			body(myCurrentIndex);
		}
		private bool ExecuteNextAndReturnStatus(int workerThreadIndex)
		{
			var myCurrentIndex = Interlocked.Increment(ref currentIndex) - 1;
			if (myCurrentIndex >= toExclusive)
			{
				workerThreadFinished[workerThreadIndex].Set();
				return false;
			}
			body(myCurrentIndex);
			return true;
		}


		public void For(int fromInclusive, int toExclusive, Action<int> body)
		{
			this.fromInclusive = fromInclusive;
			this.toExclusive = toExclusive;
			this.currentIndex = fromInclusive;
			this.body = body;

			var useThreads = threads.Length;
			var maxUseThreads = toExclusive - fromInclusive;
			if (useThreads > maxUseThreads) useThreads = maxUseThreads;

			for (int i = 0; i < useThreads; i++)
			{
				workerThreadFinished[i].Reset();
				workerThreadsRun[i].Set();
			}

			for (int i = 0; i < useThreads; i++)
			{
				workerThreadFinished[i].Wait();
				workerThreadsRun[i].Reset();
			}
		}


		public void ForUseThisThreadToo(int fromInclusive, int toExclusive, Action<int> body)
		{
			this.fromInclusive = fromInclusive;
			this.toExclusive = toExclusive;
			this.currentIndex = fromInclusive;
			this.body = body;

			var useThreads = threads.Length;
			var maxUseThreads = toExclusive - fromInclusive;
			if (useThreads > maxUseThreads) useThreads = maxUseThreads;

			for (int i = 1; i < useThreads; i++)
			{
				workerThreadFinished[i].Reset();
				workerThreadsRun[i].Set();
			}

			while (ExecuteNextAndReturnStatus(0)) ;

			for (int i = 1; i < useThreads; i++)
			{
				workerThreadFinished[i].Wait();
				workerThreadsRun[i].Reset();
			}
		}

	}

}
