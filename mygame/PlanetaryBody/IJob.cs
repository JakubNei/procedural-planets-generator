using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame.PlanetaryBody
{
	public interface IJob
	{
		bool WillNeverWantToBeExecuted { get; }
		bool WantsToBeExecutedNow { get; }
		double NextGPUThreadExecuteWillTakeSeconds();
		bool GPUThreadExecute();

		ITask NextTask { get; }

	}

	public interface ITask
	{
		string Name { get; }
		bool IsSplittable { get; }
		bool TrySplitToParts(ushort parts);
	}


}
