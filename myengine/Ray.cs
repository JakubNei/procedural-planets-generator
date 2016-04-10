using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;


namespace MyEngine
{

    public struct Ray
    {
        private Vector3 m_Origin;
        private Vector3 m_Direction;
        public Vector3 origin
        {
            get
            {
                return this.m_Origin;
            }
            set
            {
                this.m_Origin = value;
            }
        }
        public Vector3 direction
        {
            get
            {
                return this.m_Direction;
            }
            set
            {
                this.m_Direction = value.Normalized();
            }
        }
        public Ray(Vector3 origin, Vector3 direction)
        {
            this.m_Origin = origin;
            this.m_Direction = direction.Normalized();
        }
        public Vector3 GetPoint(float distance)
        {
            return this.m_Origin + this.m_Direction * distance;
        }
        public override string ToString()
        {
            return String.Format("Origin: {0}, Dir: {1}", new object[]
            {
                this.m_Origin,
                this.m_Direction
            });
        }
    }
}
