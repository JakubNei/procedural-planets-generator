using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGame
{

    public struct SpehricalCoord
    {
        public float longitude; // east to west
        public float latitude; // top 0 bottom
        public float altitude;
        public SpehricalCoord(float longitude, float latitude, float altitude)
        {
            this.longitude = longitude;
            this.latitude = latitude;
            this.altitude = altitude;
        }
    }


}