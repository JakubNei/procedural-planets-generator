using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{

    public struct SpehricalCoord
    {
        public double longitude; // east to west
        public double latitude; // top 0 bottom
        public double altitude;
        public SpehricalCoord(double longitude, double latitude, double altitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
        }
    }


}