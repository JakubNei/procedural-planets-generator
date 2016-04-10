using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

namespace MyEngine
{
    public class MeshCollider : Collider
    {
        public Mesh mesh
        {
            set
            {
                m_mesh = value;

                collisionEntity = new BEPUphysics.Entities.Prefabs.ConvexHull(GetFrom(mesh.vertices));

                OnChanged();
            }
            get
            {
                return m_mesh;
            }
        }
        Mesh m_mesh;


        internal BEPUphysics.Entities.Prefabs.ConvexHull collisionEntity
        {
            set
            {
                m_collisionEntity = value;
                base.CollisionEntityCreated(collisionEntity);
                collisionEntity.PositionUpdated += PositionUpdated;
            }
            get
            {
                return m_collisionEntity;
            }
        }
        BEPUphysics.Entities.Prefabs.ConvexHull m_collisionEntity;

        void PositionUpdated(BEPUphysics.Entities.Entity entity)
        {
            transform.m_position = (Vector3)entity.Position;
            transform.m_rotation = entity.Orientation;
        }

        List<BEPUutilities.Vector3> GetFrom(Vector3[] d)
        {
            var l = new List<BEPUutilities.Vector3>();
            foreach (var v in d) l.Add(new BEPUutilities.Vector3(v.Z, v.Y, v.X));
            return l;
        }


        internal override void OnCreated(GameObject gameObject)
        {
            base.OnCreated(gameObject);

            var renderer = gameObject.GetComponent<MeshRenderer>();
            if (renderer)
            {
                mesh = renderer.mesh;
            }

            gameObject.OnChanged += OnChanged;

            var rigidBody = GetComponent<Rigidbody>();
            if (rigidBody) rigidBody.OnChanged += OnChanged;
        }

        void OnChanged()
        {
            if (collisionEntity == null) return;

            var rigidBody = GetComponent<Rigidbody>();

            if (rigidBody && !rigidBody.isKinematic)
            {
                collisionEntity.BecomeDynamic(rigidBody.mass);
                collisionEntity.IsAffectedByGravity = rigidBody.useGravity;
                collisionEntity.AngularVelocity = rigidBody.angularVelocity;
                collisionEntity.LinearVelocity = rigidBody.velocity;
            }
            else
            {
                collisionEntity.BecomeKinematic();
                collisionEntity.IsAffectedByGravity = false;
                collisionEntity.AngularVelocity = Vector3.Zero;
                collisionEntity.LinearVelocity = Vector3.Zero;
            }

            collisionEntity.Position = transform.position;
            collisionEntity.Orientation = transform.rotation;          
        }

    }
}
