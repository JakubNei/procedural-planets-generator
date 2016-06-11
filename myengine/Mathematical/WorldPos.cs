using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;

namespace MyEngine
{
    public struct WorldPos : IEquatable<WorldPos>
    {
        Vector3d insideSectorPosition;
        long sectorX;
        long sectorY;
        long sectorZ;

        const double sectorCubeSideLength = 100;
        //const double offset = 0.5;


        public static readonly WorldPos Zero = new WorldPos();

        public WorldPos(double x, double y, double z) : this()
        {
            insideSectorPosition = new Vector3d(x, y, z);
            MoveSectorIfNeeded();
        }
        public WorldPos(Vector3d pos) : this()
        {
            insideSectorPosition = pos;
            MoveSectorIfNeeded();
        }
        public WorldPos(Vector3 pos) : this()
        {
            insideSectorPosition = pos.ToVector3d();
            MoveSectorIfNeeded();
        }

        void MoveSectorIfNeeded()
        {
            
            long sector_add;

            sector_add = (long)Math.Floor(insideSectorPosition.X / sectorCubeSideLength);
            insideSectorPosition.X -= sectorCubeSideLength * sector_add;
            sectorX += sector_add;

            sector_add = (long)Math.Floor(insideSectorPosition.Y / sectorCubeSideLength);
            insideSectorPosition.Y -= sectorCubeSideLength * sector_add;
            sectorY += sector_add;

            sector_add = (long)Math.Floor(insideSectorPosition.Z / sectorCubeSideLength);
            insideSectorPosition.Z -= sectorCubeSideLength * sector_add;
            sectorZ += sector_add;
        }
        public double Distance(WorldPos worldPos)
        {
            return this.Towards(worldPos).ToVector3d().Length;
        }
        public double DistanceSqr(WorldPos worldPos)
        {
            return this.Towards(worldPos).ToVector3d().LengthSquared;
        }

        public Vector3d ToVector3d()
        {
            return new Vector3d(
                insideSectorPosition.X + (sectorX * sectorCubeSideLength),
                insideSectorPosition.Y + (sectorY * sectorCubeSideLength),
                insideSectorPosition.Z + (sectorZ * sectorCubeSideLength)
            );
        }
        public Vector3 ToVector3()
        {
            return new Vector3(
                (float)(insideSectorPosition.X + (sectorX * sectorCubeSideLength)),
                (float)(insideSectorPosition.Y + (sectorY * sectorCubeSideLength)),
                (float)(insideSectorPosition.Z + (sectorZ * sectorCubeSideLength))
            );
        }

        public bool Equals(WorldPos other)
        {
            return
                other.insideSectorPosition == insideSectorPosition &&
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
            return insideSectorPosition.GetHashCode() ^ sectorX.GetHashCode() ^ sectorY.GetHashCode() ^ sectorZ.GetHashCode();
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
            ret.insideSectorPosition = this.insideSectorPosition - other.insideSectorPosition;
            ret.sectorX = this.sectorX - other.sectorX;
            ret.sectorY = this.sectorY - other.sectorY;
            ret.sectorZ = this.sectorZ - other.sectorZ;
            ret.MoveSectorIfNeeded();
            return ret;
        }

        public WorldPos Add(ref WorldPos other)
        {
            var ret = new WorldPos();
            ret.insideSectorPosition = this.insideSectorPosition + other.insideSectorPosition;
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
        public static WorldPos operator +(WorldPos left, Vector3d right)
        {
            left.insideSectorPosition += right;
            left.MoveSectorIfNeeded();
            return left;
        }
        public static WorldPos operator +(WorldPos left, Vector3 right)
        {
            return left + right.ToVector3d();
        }


        public static WorldPos operator +(Vector3d left, WorldPos right)
        {
            return right + left;
        }
        public static WorldPos operator +(Vector3 left, WorldPos right)
        {
            return right + left;
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
        public static WorldPos operator -(WorldPos left, Vector3d right)
        {
            left.insideSectorPosition -= right;
            left.MoveSectorIfNeeded();
            return left;
        }
        public static WorldPos operator -(WorldPos left, Vector3 right)
        {
            return left - right.ToVector3d();
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
            return $"({insideSectorPosition.X.ToString(f)};{insideSectorPosition.Y.ToString(f)};{insideSectorPosition.Z.ToString(f)})[{sectorX},{sectorY},{sectorZ}])";
        }
    }
}
