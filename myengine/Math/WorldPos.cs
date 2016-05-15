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
        Vector3d m_pos;


        public static readonly WorldPos Zero = new WorldPos();

        public WorldPos(double x, double y, double z)
        {
            m_pos = new Vector3d(x, y, z);
        }
        public WorldPos(Vector3d pos)
        {
            this.m_pos = pos;
        }

        public double Distance(WorldPos worldPos)
        {
            return worldPos.m_pos.Distance(this.m_pos);
        }

        public Vector3d ToVector3d()
        {
            return m_pos;
        }
        public Vector3 ToVector3()
        {
            return m_pos.ToVector3();
        }

        public bool Equals(WorldPos other)
        {
            return other.m_pos == m_pos;
        }

        public override bool Equals(object obj)
        {
            if ((obj is WorldPos) == false) return false;
            return this.Equals((WorldPos)obj);
        }

        public override int GetHashCode()
        {
            return m_pos.GetHashCode();
        }


        public WorldPos Towards(WorldPos other)
        {
            return new WorldPos()
            {
                m_pos = this.m_pos.Towards(other.m_pos),
            };
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
            return new WorldPos()
            {
                m_pos = left.m_pos + right.m_pos,
            };
        }
        
        public static WorldPos operator +(WorldPos left, Vector3 right)
        {
            return new WorldPos()
            {
                m_pos = left.m_pos + right.ToVector3d(),
            };
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
            return new WorldPos()
            {
                m_pos = left.m_pos - right.m_pos,
            };
        }
        
        public static WorldPos operator -(WorldPos left, Vector3 right)
        {
            return new WorldPos()
            {
                m_pos = left.m_pos - right.ToVector3d(),
            };
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
    }
}
