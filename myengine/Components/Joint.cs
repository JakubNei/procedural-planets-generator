using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyEngine;

namespace MyEngine.Components
{
    public abstract class Joint : Component
    {
        public Joint(Entity entity) : base(entity)
        {
        }
    }
}
