using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;


namespace Neitri
{
	[DataContract]
	public partial struct WorldPos : IEquatable<WorldPos>
	{

		[DataMember(Order = 1)]
		double x;
		[DataMember(Order = 2)]
		double y;
		[DataMember(Order = 3)]
		double z;

		[DataMember(Order = 4)]
		long sectorX;
		[DataMember(Order = 5)]
		long sectorY;
		[DataMember(Order = 6)]
		long sectorZ;

		const double sectorCubeSideLength = 100;
		//const double offset = 0.5;


		public static readonly WorldPos Zero = new WorldPos();

		public WorldPos(double x, double y, double z) : this()
		{
			this.x = x;
			this.y = y;
			this.z = z;
			MoveSectorIfNeeded();
		}

		void MoveSectorIfNeeded()
		{
			long sector_add;

			sector_add = (long)Math.Floor(x / sectorCubeSideLength);
			x -= sectorCubeSideLength * sector_add;
			sectorX += sector_add;

			sector_add = (long)Math.Floor(y / sectorCubeSideLength);
			y -= sectorCubeSideLength * sector_add;
			sectorY += sector_add;

			sector_add = (long)Math.Floor(z / sectorCubeSideLength);
			z -= sectorCubeSideLength * sector_add;
			sectorZ += sector_add;
		}
		/*
		public double Distance(WorldPos worldPos)
		{
			return this.Towards(worldPos).ToVector3d().Length;
		}
		public double DistanceSqr(WorldPos worldPos)
		{
			return this.Towards(worldPos).ToVector3d().LengthSquared;
		}

*/
		public bool Equals(WorldPos other)
		{
			return
				other.x == x &&
				other.y == y &&
				other.z == z &&
				other.sectorX == sectorX &&
				other.sectorY == sectorY &&
				other.sectorZ == sectorZ;
		}

		public override bool Equals(object obj)
		{
			if ((obj is WorldPos) == false) return false;
			return this.Equals((WorldPos)obj);
		}

		public override int GetHashCode()
		{
			return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ sectorX.GetHashCode() ^ sectorY.GetHashCode() ^ sectorZ.GetHashCode();
		}


		public WorldPos Towards(WorldPos other)
		{
			return this.Towards(ref other);
		}
		public WorldPos Towards(ref WorldPos other)
		{
			return other.Sub(ref this);
		}

		public WorldPos Sub(ref WorldPos other)
		{
			var ret = new WorldPos();
			ret.x = x - other.x;
			ret.y = y - other.y;
			ret.z = z - other.z;
			ret.sectorX = this.sectorX - other.sectorX;
			ret.sectorY = this.sectorY - other.sectorY;
			ret.sectorZ = this.sectorZ - other.sectorZ;
			ret.MoveSectorIfNeeded();
			return ret;
		}

		public WorldPos Add(ref WorldPos other)
		{
			var ret = new WorldPos();
			ret.x = x + other.x;
			ret.y = y + other.y;
			ret.z = z + other.z;
			ret.sectorX = this.sectorX + other.sectorX;
			ret.sectorY = this.sectorY + other.sectorY;
			ret.sectorZ = this.sectorZ + other.sectorZ;
			ret.MoveSectorIfNeeded();
			return ret;
		}


		//
		// Summary:
		//     Adds two instances.
		//
		// Parameters:
		//   left:
		//     The first instance.
		//
		//   right:
		//     The second instance.
		//
		// Returns:
		//     The result of the calculation.
		public static WorldPos operator +(WorldPos left, WorldPos right)
		{
			return left.Add(ref right);
		}
	
		//
		// Summary:
		//     Subtracts two instances.
		//
		// Parameters:
		//   left:
		//     The first instance.
		//
		//   right:
		//     The second instance.
		//
		// Returns:
		//     The result of the calculation.
		public static WorldPos operator -(WorldPos left, WorldPos right)
		{
			return left.Sub(ref right);
		}
	
		//
		// Summary:
		//     Compares two instances for equality.
		//
		// Parameters:
		//   left:
		//     The first instance.
		//
		//   right:
		//     The second instance.
		//
		// Returns:
		//     True, if left equals right; false otherwise.
		public static bool operator ==(WorldPos left, WorldPos right)
		{
			return left.Equals(right);
		}
		//
		// Summary:
		//     Compares two instances for inequality.
		//
		// Parameters:
		//   left:
		//     The first instance.
		//
		//   right:
		//     The second instance.
		//
		// Returns:
		//     True, if left does not equa lright; false otherwise.
		public static bool operator !=(WorldPos left, WorldPos right)
		{
			return left.Equals(right) == false;
		}
		/*
        public static implicit operator WorldPos(Vector3d vec)
        {
            return new WorldPos()
            {
                m_pos = vec,
            };
        }

        public static implicit operator WorldPos(Vector3 vec)
        {
            return new WorldPos()
            {
                m_pos = vec.ToVector3d(),
            };
        }
        */
		public override string ToString()
		{
			var f = "0.000";
			return "(" + x.ToString(f) + ";" + y.ToString(f) + ";" + z.ToString(f) + ")[" + sectorX + ";" + sectorY + ";" + sectorZ + "]";
		}
	}
}
