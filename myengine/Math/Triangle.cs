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
        public Vector3d a;
        public Vector3d b;
        public Vector3d c;

        public Vector3d CenterPos
        {
            get
            {
                return (a + b + c) / 3.0f;
            }
        }

        public Triangle(Vector3d a, Vector3d b, Vector3d c)
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
