using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using MyEngine;

namespace MyEngine.Components
{
    public class BoxCollider : Collider, IDisposable
    {
        Vector3 m_center = Vector3.Zero;
        Vector3 m_size = Vector3.One;

        bool representsMeshBounds = false;

        public override bool enabled
        {
            get
            {
                return base.enabled;
            }
            set
            {
                base.enabled = value;
                if (enabled) PhysicsUsage.PhysicsManager.instance.Add(collisionEntity);
                else PhysicsUsage.PhysicsManager.instance.Remove(collisionEntity);
            }
        }
        public Vector3 center
        {
            set
            {
                representsMeshBounds = false;
                m_center = value;
                OnChanged(ChangedFlags.PhysicalShape);
            }
            get
            {
                return m_center;
            }
        }
        public Vector3 size
        {
            set
            {
                representsMeshBounds = false;
                m_size = value;
                OnChanged(ChangedFlags.PhysicalShape);
            }
            get
            {
                return m_size;
            }
        }

        internal BEPUphysics.Entities.Prefabs.Box collisionEntity
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
        BEPUphysics.Entities.Prefabs.Box m_collisionEntity;

        public BoxCollider(Entity entity) : base(entity)
        {
            OnCreated();
        }

        void OnCreated()
        {

            var renderer = Entity.GetComponent<MeshRenderer>();
            if(renderer != null)
            {
                center = renderer.Mesh.bounds.center;
                size = renderer.Mesh.bounds.Extents;
                representsMeshBounds = true;
            }

            var s = size * Entity.Transform.Scale;
            collisionEntity = new BEPUphysics.Entities.Prefabs.Box(Entity.Transform.Position + center.RotateBy(Entity.Transform.Rotation), s.X, s.Y, s.Z);

            Entity.OnChanged += OnChanged;
            Entity.RaiseOnChanged(ChangedFlags.All);

        }


        void OnChanged(ChangedFlags flags)
        {
            if (collisionEntity == null) return;

            if (flags.HasFlag(ChangedFlags.PhysicsSettings))
            {
                var rigidBody = Entity.GetComponent<Rigidbody>();
                if (rigidBody != null && rigidBody.isKinematic == false)
                {
                    collisionEntity.BecomeDynamic(rigidBody.mass);
                    collisionEntity.IsAffectedByGravity = rigidBody.useGravity;
                    /*collisionEntity.AngularVelocity = rigidBody.angularVelocity;
                    collisionEntity.LinearVelocity = rigidBody.velocity;*/
                    collisionEntity.Mass = rigidBody.mass;
                }
                else
                {
                    collisionEntity.BecomeKinematic();
                    collisionEntity.IsAffectedByGravity = false;
                    collisionEntity.AngularVelocity = Vector3.Zero;
                    collisionEntity.LinearVelocity = Vector3.Zero;
                }
            }

            if (flags.HasFlag(ChangedFlags.Position) || flags.HasFlag(ChangedFlags.PhysicalShape)) collisionEntity.Position = Entity.Transform.Position + center.RotateBy(Entity.Transform.Rotation);
            if (flags.HasFlag(ChangedFlags.Rotation)) collisionEntity.Orientation = Entity.Transform.Rotation;

            if (flags.HasFlag(ChangedFlags.Scale))
            {
                var s = size * Entity.Transform.Scale * 2;
                collisionEntity.Width = s.X;
                collisionEntity.Height = s.Y;
                collisionEntity.Length = s.Z;
            }

            /*if (representsMeshBounds && flags.HasFlag(ChangedFlags.Bounds))
            {
                var renderer = entity.GetComponent<MeshRenderer>();
                if (renderer)
                {
                    center = renderer.mesh.bounds.center;
                    size = renderer.mesh.bounds.extents;
                    representsMeshBounds = true;

                    var s = size * transform.scale;

                    collisionEntity = new BEPUphysics.Entities.Prefabs.Box(transform.position + center.RotateBy(transform.rotation), s.X, s.Y, s.Z);
                }
            }*/
        }

        void PositionUpdated(BEPUphysics.Entities.Entity entity)
        {
            Entity.Transform.m_rotation = entity.Orientation;
            Entity.Transform.m_position = (Vector3)entity.Position - center.RotateBy(Entity.Transform.Rotation);
        }

        public void Dispose()
        {
            PhysicsUsage.PhysicsManager.instance.Remove(collisionEntity);
        }
    }
}
