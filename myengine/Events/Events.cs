using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{

	public class FrameTimeEvent : IEvent
	{
		public readonly double DeltaTime;
		public readonly double DeltaTimeOver1Second;
		public readonly double DeltaTimeOver10Seconds;


		public readonly FrameTime FrameTime;

		public FrameTimeEvent(FrameTime frameTime)
		{
			this.FrameTime = frameTime;
			this.DeltaTime = frameTime.DeltaTime;
			this.DeltaTimeOver1Second = frameTime.DeltaTime1Second;
			this.DeltaTimeOver10Seconds = frameTime.DeltaTime10Seconds;
		}
	}
	public class EventThreadUpdate : FrameTimeEvent
	{
		public EventThreadUpdate(FrameTime deltaTimeManager) : base(deltaTimeManager)
		{
		}
	}

	public class PreRenderUpdate : FrameTimeEvent
	{
		public PreRenderUpdate(FrameTime frameTime) : base(frameTime)
		{
		}
	}

	public class PostRenderUpdate : FrameTimeEvent
	{
		public PostRenderUpdate(FrameTime frameTime) : base(frameTime)
		{
		}
	}
	public class InputUpdate : FrameTimeEvent
	{
		public InputUpdate(FrameTime frameTime) : base(frameTime)
		{
		}
	}

	public class FrameStarted : IEvent
	{

	}

	public class FrameEnded : FrameTimeEvent
	{
		public FrameEnded(FrameTime frameTime) : base(frameTime)
		{
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
