using UnityEngine;
using System.Collections.Generic;

namespace Neitri
{
    public static class BoundsExtensions
    {
        // http://answers.unity3d.com/questions/29797/how-to-get-8-vertices-from-bounds-properties.html
        public static Vector3[] GetCorners(this Bounds bounds)
        {
            var boundPoint1 = bounds.min;
            var boundPoint2 = bounds.max;
            return new[] {
                boundPoint1,
                boundPoint2,
                new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z),
                new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z),
                new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z),
                new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z),
                new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z),
                new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z),
            };
        }
    }


}