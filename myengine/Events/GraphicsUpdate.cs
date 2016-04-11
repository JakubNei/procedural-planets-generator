using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyEngine.Events
{
    public class GraphicsUpdate : IEvent
    {
        public double DeltaTime
        {
            get; private set;
        }
        public GraphicsUpdate(double deltaTime)
        {
            this.DeltaTime = deltaTime;
        }
    }
}
