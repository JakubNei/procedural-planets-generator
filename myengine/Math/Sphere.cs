using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public struct Sphere
    {
        public Vector3 center;
        public float radius;
        public Sphere(Vector3 center, float radius)
        {
            this.center = center;
            this.radius = radius;
        }
    }
}
