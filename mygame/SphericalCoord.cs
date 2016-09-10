using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MyGame
{

    public struct SpehricalCoord
    {
		/// <summary>
		/// east to west
		/// </summary>
		public double longitude;
		/// <summary>
		/// top 0 bottom
		/// </summary>
		public double latitude;
        public double altitude;
        public SpehricalCoord(double longitude, double latitude, double altitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
        }

        public Vector3d ToVec3d()
        {
            return new Vector3d(longitude, latitude, altitude);
        }

        public SpehricalCoord (Vector3d fromVec3d)
        {
            this.longitude = fromVec3d.X;
            this.latitude = fromVec3d.Y;
            this.altitude = fromVec3d.Z;
        }
    }


}