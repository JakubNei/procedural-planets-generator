using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;

using MyEngine;
using MyEngine.Events;
using MyEngine.Components;

namespace MyEngine.Components
{
    public abstract class Collider : Component
    {
        public virtual bool enabled { set; get; }
        public Rigidbody attachedRigidbody;
        public bool isTrigger;
        public float contactOffset;
        //public PhysicMaterial material;
        //public PhysicMaterial sharedMaterial;
        //public Vector3 ClosestPointOnBounds(Vector3 position);

        //internal BEPUphysics.Entities.Entity collisionEntityBase;
        /*
        public bool Raycast(Ray ray, out RaycastHit hitInfo, float maxDistance)
        {
            hitInfo = new RaycastHit();
            return false;
        }
        */
        internal BEPUphysics.Entities.Entity collisionEntity_generic;

        public Collider(Entity entity) : base(entity)
        {
        }

        internal void CollisionEntityCreated(BEPUphysics.Entities.Entity collisionEntity)
        {
            if (collisionEntity == null) return;

            collisionEntity.Tag = this;
            PhysicsUsage.PhysicsManager.instance.Add(collisionEntity);

            collisionEntity.CollisionInformation.Events.ContactCreated += Events_ContactCreated;
            collisionEntity.CollisionInformation.Events.ContactRemoved += Events_ContactRemoved;

            collisionEntity_generic = collisionEntity;
        }


        private T GenerateCollision<T>(BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable sender, BEPUphysics.BroadPhaseEntries.Collidable other, BEPUphysics.NarrowPhaseSystems.Pairs.CollidablePairHandler pair, BEPUphysics.CollisionTests.ContactData contact) where T : Events.CollisionBase, new()
        {
            var otherEntity = other as BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable;
            var otherCollider = otherEntity.Entity.Tag as Collider;

            var collision = new T()
            {
                entity = otherCollider.Entity,
                contacts = new CollisionBase.ContactPoint[1],
            };
            collision.contacts[0] = new CollisionBase.ContactPoint()
            {
                normal = contact.Normal,
                point = contact.Position,
                otherCollider = otherCollider,
                thisCollider = this.Entity.GetComponent<Collider>(),
            };
            return collision;
        }

        internal void Events_ContactCreated(BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable sender, BEPUphysics.BroadPhaseEntries.Collidable other, BEPUphysics.NarrowPhaseSystems.Pairs.CollidablePairHandler pair, BEPUphysics.CollisionTests.ContactData contact)
        {
            var collision = GenerateCollision<Events.CollisionEnter>(sender, other, pair, contact);            
            this.Entity.EventSystem.Raise(collision);
        }

        internal void Events_ContactRemoved(BEPUphysics.BroadPhaseEntries.MobileCollidables.EntityCollidable sender, BEPUphysics.BroadPhaseEntries.Collidable other, BEPUphysics.NarrowPhaseSystems.Pairs.CollidablePairHandler pair, BEPUphysics.CollisionTests.ContactData contact)
        {
            var collision = GenerateCollision<Events.CollisionExit>(sender, other, pair, contact);
            this.Entity.EventSystem.Raise(collision);
        }    
    }

}


namespace MyEngine.Events
{
    public abstract class CollisionBase : EventBase
    {
        public class ContactPoint
        {
            public Vector3 normal;
            public Collider otherCollider;
            public Vector3 point;
            public Collider thisCollider;

        }
        public ContactPoint[] contacts;
        public Entity entity;

        public Vector3 relativeVelocity;


        public Collider collider { get { return entity.GetComponent<Collider>(); } }
        public Rigidbody rigidbody { get { return entity.GetComponent<Rigidbody>(); } }
        public Transform transform { get { return entity.Transform; } }
    }
    public class CollisionEnter : CollisionBase
    {

    }
    public class CollisionExit : CollisionBase
    {

    }
}
