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
		/// east -PI, to west PI
		/// </summary>
		public double longitude;
		/// <summary>
		/// top -PI/2, bottom PI/2
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

		public SpehricalCoord(Vector3d fromVec3d)
		{
			this.longitude = fromVec3d.X;
			this.latitude = fromVec3d.Y;
			this.altitude = fromVec3d.Z;
		}

		// http://stackoverflow.com/questions/1185408/converting-from-longitude-latitude-to-cartesian-coordinates
		public static SpehricalCoord FromCalestial(Vector3d c)
		{
			var r = c.Length;
			if (r == 0) return new SpehricalCoord(0, 0, 0);
			return new SpehricalCoord(
				Math.Atan2(c.Z, c.X),
				Math.Asin(c.Y / r),
				r
			);
		}
		public static SpehricalCoord FromCalestial(Vector3 c)
		{
			var r = c.Length;
			if (r == 0) return new SpehricalCoord(0, 0, 0);
			return new SpehricalCoord(
				Math.Atan2(c.Z, c.X),
				Math.Asin(c.Y / r),
				r
			);
		}
		public Vector3d ToCalestial()
		{
			var r = altitude;
			//s.latitude = s.latitude / 180.0f * M_PI;
			//s.longitude = s.longitude / 180.0f * M_PI;
			//if (r == 0) r = radius;
			return new Vector3d(
				Math.Cos(latitude) * Math.Cos(longitude) * r,
				Math.Sin(latitude) * r,
				Math.Cos(latitude) * Math.Sin(longitude) * r
			);
		}

		public override string ToString()
		{
			return $"longitude:{longitude}, latitude:{latitude}, altitude:{altitude}";
		}

	}


}