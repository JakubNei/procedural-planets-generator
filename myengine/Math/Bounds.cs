using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace MyEngine
{
    public struct Bounds
    {
        private Vector3 m_Center;
        private Vector3 m_Extents;
        public Vector3 Center
        {
            get
            {
                return this.m_Center;
            }
            set
            {
                this.m_Center = value;
            }
        }
        public Vector3 Size
        {
            get
            {
                return this.m_Extents * 2f;
            }
            set
            {
                this.m_Extents = value * 0.5f;
            }
        }
        public Vector3 Extents
        {
            get
            {
                return this.m_Extents;
            }
            set
            {
                this.m_Extents = value;
            }
        }
        public Vector3 Min
        {
            get
            {
                return this.Center - this.Extents;
            }
            set
            {
                this.SetMinMax(value, this.Max);
            }
        }
        public Vector3 Max
        {
            get
            {
                return this.Center + this.Extents;
            }
            set
            {
                this.SetMinMax(this.Min, value);
            }
        }
        public Bounds(Vector3 center, Vector3 size)
        {
            this.m_Center = center;
            this.m_Extents = size * 0.5f;
        }
        public override int GetHashCode()
        {
            return this.Center.GetHashCode() ^ this.Extents.GetHashCode() << 2;
        }
        public override bool Equals(object other)
        {
            if (!(other is Bounds))
            {
                return false;
            }
            Bounds bounds = (Bounds)other;
            return this.Center.Equals(bounds.Center) && this.Extents.Equals(bounds.Extents);
        }
        public void SetMinMax(Vector3 min, Vector3 max)
        {
            this.Extents = (max - min) * 0.5f;
            this.Center = min + this.Extents;
        }
        public void Encapsulate(Vector3 point)
        {
            var min = this.Min;
            var max = this.Max;
            if (point.X < min.X) min.X = point.X;
            if (point.Y < min.Y) min.Y = point.Y;
            if (point.Z < min.Z) min.Z = point.Z;
            if (point.X > max.X) max.X = point.X;
            if (point.Y > max.Y) max.Y = point.Y;
            if (point.Z > max.Z) max.Z = point.Z;
            this.SetMinMax(min, max);
        }
        public void Encapsulate(Bounds bounds)
        {
            this.Encapsulate(bounds.Center - bounds.Extents);
            this.Encapsulate(bounds.Center + bounds.Extents);
        }
        public void Expand(float amount)
        {
            amount *= 0.5f;
            this.Extents += new Vector3(amount, amount, amount);
        }
        public void Expand(Vector3 amount)
        {
            this.Extents += amount * 0.5f;
        }
        public bool Intersects(Bounds bounds)
        {
            return this.Min.X <= bounds.Max.X && this.Max.X >= bounds.Min.X && this.Min.Y <= bounds.Max.Y && this.Max.Y >= bounds.Min.Y && this.Min.Z <= bounds.Max.Z && this.Max.Z >= bounds.Min.Z;
        }
    /*
        public bool Contains(Vector3 point)
        {
            return Bounds.Internal_Contains(this, point);
        }
   
        public float SqrDistance(Vector3 point)
        {
            return Bounds.Internal_SqrDistance(this, point);
        }       
        
        public bool IntersectRay(Ray ray)
        {
            float num;
            return Bounds.Internal_IntersectRay(ref ray, ref this, out num);
        }
        public bool IntersectRay(Ray ray, out float distance)
        {
            return Bounds.Internal_IntersectRay(ref ray, ref this, out distance);
        }       
        public Vector3 ClosestPoint(Vector3 point)
        {
            return Bounds.Internal_GetClosestPoint(ref this, ref point);
        }*/
        public override string ToString()
        {
            return String.Format("Center: {0}, Extents: {1}", new object[]
            {
                this.m_Center,
                this.m_Extents
            });
        }
        /*public string ToString(string format)
        {
            return String.Format("Center: {0}, Extents: {1}", new object[]
            {
                this.m_Center.ToString(format),
                this.m_Extents.ToString(format)
            });
        }*/
        public static bool operator ==(Bounds lhs, Bounds rhs)
        {
            return lhs.Center == rhs.Center && lhs.Extents == rhs.Extents;
        }
        public static bool operator !=(Bounds lhs, Bounds rhs)
        {
            return !(lhs == rhs);
        }

        
    }
}
