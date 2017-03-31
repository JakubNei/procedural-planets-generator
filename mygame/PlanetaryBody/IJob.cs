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
		double NextGPUThreadTickWillTakeSeconds();
		bool GPUThreadExecute();
	}

}
