using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
	public class EventThreadUpdate : DeltaTimeEvent
	{
		public EventThreadUpdate(DeltaTimeManager deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}
	public class RenderUpdate : DeltaTimeEvent
	{
		public RenderUpdate(DeltaTimeManager deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}

	public class DeltaTimeEvent : IEvent
	{
		public double DeltaTimeNow { get; }
		public double DeltaTimeOver1Second { get; }
		public double DeltaTimeOver10Seconds { get; }

		public virtual bool AllowMultiThreading => true;

		public DeltaTimeEvent(DeltaTimeManager deltaTimeManager)
		{
			this.DeltaTimeNow = deltaTimeManager.DeltaTimeNow;
			this.DeltaTimeOver1Second = deltaTimeManager.DeltaTime1Second;
			this.DeltaTimeOver10Seconds = deltaTimeManager.DeltaTime10Seconds;
		}
	}
	public class WindowResized : IEvent
	{
		public int NewPixelWidth { get; private set; }
		public int NewPixelHeight { get; private set; }
		public bool AllowMultiThreading => false;
		public WindowResized(int width, int height)
		{
			this.NewPixelWidth = width;
			this.NewPixelHeight = height;
		}
	}
}
