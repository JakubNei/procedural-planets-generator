using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using MyEngine.Components;

using OpenTK;

namespace MyEngine
{
    public struct RaycastHit
    {
        internal Vector3 m_Point;
        internal Vector3 m_Normal;
        internal int m_FaceID;
        internal float m_Distance;
        internal Vector2 m_UV;
        internal Collider m_Collider;
        public Vector3 Point
        {
            get
            {
                return this.m_Point;
            }
        }
        public Vector3 Normal
        {
            get
            {
                return this.m_Normal;
            }
        }
       /* public Vector3 barycentricCoordinate
        {
            get
            {
                return new Vector3(1f - (this.m_UV.Y + this.m_UV.X), this.m_UV.X, this.m_UV.Y);
            }
            set
            {
                this.m_UV = value.Xy;
            }
        }*/
        public float Distance
        {
            get
            {
                return this.m_Distance;
            }
            set
            {
                this.m_Distance = value;
            }
        }
       /* public int triangleIndex
        {
            get
            {
                return this.m_FaceID;
            }
        }*/
        public Collider Collider
        {
            get
            {
                return this.m_Collider;
            }
        }
        public Rigidbody Tigidbody
        {
            get
            {
                return (!(this.Collider != null)) ? null : this.Collider.attachedRigidbody;
            }
        }
        public Transform Transform
        {
            get
            {
                Rigidbody rigidbody = this.Tigidbody;
                if (rigidbody != null)
                {
                    return rigidbody.Entity.Transform;
                }
                if (this.Collider != null)
                {
                    return this.Collider.Entity.Transform;
                }
                return null;
            }
        }

     
    }
}
