using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public struct Triangle
    {
        public Vector3 a;
        public Vector3 b;
        public Vector3 c;

        public Vector3 CenterPos
        {
            get
            {
                return (a + b + c) / 3.0f;
            }
        }

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            this.a = a;
            this.b = b;
            this.c = c;
        }

        public Sphere ToBoundingSphere()
        {
            var c = CenterPos;
            var radius = (float)Math.Sqrt(
                Math.Max(
                    Math.Max(
                        a.DistanceSqr(b),
                        a.DistanceSqr(c)
                    ),
                    b.DistanceSqr(c)
                )
            );
            return new Sphere(c, radius);
        }

    }
}
