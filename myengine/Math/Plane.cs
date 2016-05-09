using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public struct Plane
    {
        public Vector3 normal;
        public float distance;


        public Plane(Vector3 inNormal, Vector3 inPoint)
        {
            this.normal = Vector3.Normalize(inNormal);
            this.distance = -Vector3.Dot(inNormal, inPoint);
        }
        public Plane(Vector3 a, Vector3 b, Vector3 c)
        {
            this.normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            this.distance = -Vector3.Dot(this.normal, a);
        }
        public void SetNormalAndPosition(Vector3 inNormal, Vector3 inPoint)
        {
            this.normal = Vector3.Normalize(inNormal);
            this.distance = -Vector3.Dot(inNormal, inPoint);
        }
        public void Set3Points(Vector3 a, Vector3 b, Vector3 c)
        {
            this.normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));
            this.distance = -Vector3.Dot(this.normal, a);
        }

        // http://www.youtube.com/watch?v=4p-E_31XOPM
        public float GetDistanceToPoint(Vector3 point)
        {
            return Vector3.Dot(point, normal) + distance;
        }
    }
}
