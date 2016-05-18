using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
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
