using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
	public class EventThreadUpdate : DeltaTimeEvent
	{
		public EventThreadUpdate(FrameTime deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}

	public class PreRenderUpdate : DeltaTimeEvent
	{
		public PreRenderUpdate(FrameTime deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}

	public class PostRenderUpdate : DeltaTimeEvent
	{
		public PostRenderUpdate(FrameTime deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}
	public class InputUpdate : DeltaTimeEvent
	{
		public InputUpdate(FrameTime deltaTimeManager) : base(deltaTimeManager)
		{

		}
	}

	public class DeltaTimeEvent : IEvent
	{
		public readonly double DeltaTime;
		public readonly double DeltaTimeOver1Second;
		public readonly double DeltaTimeOver10Seconds;


		public readonly FrameTime FrameTime;

		public DeltaTimeEvent(FrameTime deltaTimeManager)
		{
			this.FrameTime = deltaTimeManager;
			this.DeltaTime = deltaTimeManager.DeltaTime;
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
