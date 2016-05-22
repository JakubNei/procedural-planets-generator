using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
    public class InputUpdate : IEvent
    {
        public double DeltaTimeNow { get; }
        public double DeltaTimeOver1Second { get; }
        public double DeltaTimeOver10Seconds { get; }
        public InputUpdate(double deltaTimeNow, double deltaTimeOver1Second, double deltaTimeOver10Seconds)
        {
            this.DeltaTimeNow = deltaTimeNow;
            this.DeltaTimeOver1Second = deltaTimeOver1Second;
            this.DeltaTimeOver10Seconds = deltaTimeOver10Seconds;
        }
    }
    public class WindowResized : IEvent
    {
        public int NewPixelWidth { get; private set; }
        public int NewPixelHeight { get; private set; }
        public WindowResized(int width, int height)
        {
            this.NewPixelWidth = width;
            this.NewPixelHeight = height;
        }
    }
}
