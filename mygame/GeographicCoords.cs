using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace MyGame
{

	public struct GeographicCoords
	{
		/// <summary>
		/// east -PI, to west PI
		/// </summary>
		public double longitude;
		public double longitude01 => longitude / (2 * Math.PI) + 0.5;
		/// <summary>
		/// top -PI/2, bottom PI/2
		/// </summary>
		public double latitude;
		public double latitude01 => latitude / Math.PI + 0.5;
		public double altitude;
		public GeographicCoords(double longitude, double latitude, double altitude)
		{
			this.longitude = longitude;
			this.latitude = latitude;
			this.altitude = altitude;
		}

		public Vector3d ToVec3d()
		{
			return new Vector3d(longitude, latitude, altitude);
		}

		public GeographicCoords(Vector3d cartesian)
		{
			this.longitude = cartesian.X;
			this.latitude = cartesian.Y;
			this.altitude = cartesian.Z;
		}

		// http://stackoverflow.com/questions/1185408/converting-from-longitude-latitude-to-cartesian-coordinates
		public static GeographicCoords ToGeographic(Vector3d cartesian)
		{
			var altidute = cartesian.Length;
			if (altidute == 0) return new GeographicCoords(0, 0, 0);
			return new GeographicCoords(
				Math.Atan2(cartesian.Z, cartesian.X),
				Math.Asin(cartesian.Y / altidute),
				altidute
			);
		}
		public static Vector3d ToCartesian(GeographicCoords geographic)
		{
			return new Vector3d(
				Math.Cos(geographic.latitude) * Math.Cos(geographic.longitude) * geographic.altitude,
				Math.Sin(geographic.latitude) * geographic.altitude,
				Math.Cos(geographic.latitude) * Math.Sin(geographic.longitude) * geographic.altitude
			);
		}

		public static GeographicCoords ToSpherical(Vector3 cartesian)
		{
			var altidute = cartesian.Length;
			if (altidute == 0) return new GeographicCoords(0, 0, 0);
			return new GeographicCoords(
				Math.Atan2(cartesian.Z, cartesian.X),
				Math.Asin(cartesian.Y / altidute),
				altidute
			);
		}
		public Vector3d ToPosition()
		{
			return ToCartesian(this);
		}




		public override string ToString()
		{
			return $"longitude:{longitude}, latitude:{latitude}, altitude:{altitude}";
		}

	}


}