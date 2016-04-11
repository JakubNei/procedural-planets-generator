using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using MyEngine;

namespace MyEngine.Components
{
    public enum ForceMode
    {
        Force,
        Acceleration = 5,
        Impulse = 1,
        VelocityChange
    }
    public enum CollisionDetectionMode
    {
        Discrete,
        Continuous,
        ContinuousDynamic
    }

    public enum RigidbodyConstraints
    {
        None,
        FreezePositionX = 2,
        FreezePositionY = 4,
        FreezePositionZ = 8,
        FreezeRotationX = 16,
        FreezeRotationY = 32,
        FreezeRotationZ = 64,
        FreezePosition = 14,
        FreezeRotation = 112,
        FreezeAll = 126
    }

    public enum RigidbodyInterpolation
    {
        None,
        Interpolate,
        Extrapolate
    }

    public partial class Rigidbody : Component
    {
        public Vector3 velocity;
        /*public Vector3 angularVelocity;
        public float drag;
        public float angularDrag;*/
        public float mass = 10;
        public bool useGravity = true;
        //public float maxDepenetrationVelocity;
        bool m_isKinematic = false;

        public Rigidbody(Entity entity) : base(entity)
        {
            var r = Entity.GetComponent<MeshRenderer>();
            if (r != null)
            {
                var s = r.Mesh.bounds.Size;
                this.mass = s.X * s.Y * s.Z;
            }
        }

        public bool isKinematic
        {
            set
            {
                m_isKinematic = value;
                // TODO
                // Entity.RaiseOnChanged(ChangedFlags.PhysicsSettings);
            }
            get
            {
                return m_isKinematic;
            }
        }
       /* public bool freezeRotation = false;
        public RigidbodyConstraints constraints = RigidbodyConstraints.None;
        public CollisionDetectionMode collisionDetectionMode = CollisionDetectionMode.Continuous;
        public Vector3 centerOfMass;
        public Vector3 worldCenterOfMass;
        public Quaternion inertiaTensorRotation;
        public Vector3 inertiaTensor;
        public bool detectCollisions;
        public bool useConeFriction;
        public Vector3 position;
        public Quaternion rotation;
        public RigidbodyInterpolation interpolation = RigidbodyInterpolation.Interpolate;
        public int solverIterationCount;

        public float sleepThreshold;
        public float maxAngularVelocity;*/

    
        public void AddForce(Vector3 force, ForceMode mode = ForceMode.Force)
        {
            var c = Entity.GetComponent<Collider>();
            if (c != null) AddForceAtPosition(force, c.collisionEntity_generic.Position, mode);
        }
        public void AddForceAtPosition(Vector3 force, Vector3 position, ForceMode mode = ForceMode.Force)
        {
            var c = Entity.GetComponent<Collider>();
            if (c != null) c.collisionEntity_generic.ApplyImpulse(position, force);
        }
    }
}
