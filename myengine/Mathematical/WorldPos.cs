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

            sector_add = (long)Math.Floor(insideSectorPosition.X / sectorCubeSideLength + 0.5);
            insideSectorPosition.X -= sectorCubeSideLength * sector_add;
            sectorX += sector_add;

            sector_add = (long)Math.Floor(insideSectorPosition.Y / sectorCubeSideLength + 0.5);
            insideSectorPosition.Y -= sectorCubeSideLength * sector_add;
            sectorY += sector_add;

            sector_add = (long)Math.Floor(insideSectorPosition.Z / sectorCubeSideLength + 0.5);
            insideSectorPosition.Z -= sectorCubeSideLength * sector_add;
            sectorZ += sector_add;
        }
        public double Distance(WorldPos worldPos)
        {
            return worldPos.insideSectorPosition.Distance(this.insideSectorPosition);
        }

        public Vector3d ToVector3d()
        {
            return new Vector3d(
                insideSectorPosition.X + sectorX * sectorCubeSideLength,
                insideSectorPosition.Y + sectorY * sectorCubeSideLength,
                insideSectorPosition.Z + sectorZ * sectorCubeSideLength
            );
        }
        public Vector3 ToVector3()
        {
            return new Vector3(
                (float)(insideSectorPosition.X + sectorX * sectorCubeSideLength),
                (float)(insideSectorPosition.Y + sectorY * sectorCubeSideLength),
                (float)(insideSectorPosition.Z + sectorZ * sectorCubeSideLength)
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
            return other - this;
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
            left.insideSectorPosition += right.insideSectorPosition;
            left.MoveSectorIfNeeded();
            left.sectorX += right.sectorX;
            left.sectorY += right.sectorY;
            left.sectorZ += right.sectorZ;
            return left;
        }

        public static WorldPos operator +(WorldPos left, Vector3 right)
        {
            left.insideSectorPosition += right.ToVector3d();
            left.MoveSectorIfNeeded();
            return left;
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
            left.insideSectorPosition -= right.insideSectorPosition;
            left.MoveSectorIfNeeded();
            left.sectorX -= right.sectorX;
            left.sectorY -= right.sectorY;
            left.sectorZ -= right.sectorZ;
            return left;
        }

        public static WorldPos operator -(WorldPos left, Vector3 right)
        {
            left.insideSectorPosition -= right.ToVector3d();
            left.MoveSectorIfNeeded();
            return left;
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
            return $"({insideSectorPosition.ToString()}[{sectorX},{sectorY},{sectorZ}])";
        }
    }
}
